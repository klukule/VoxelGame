using Ionic.Zip;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Assets;
using Dapper;
using OpenTK;
using VoxelGame.Containers;

namespace VoxelGame.Worlds
{
    public partial class World : ILoadable, ISaveable
    {
        public static readonly string WORLD_SAVE_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxelGame\\Worlds\\";

        private SqliteConnection _storageConnection;

        /// <summary>
        /// World name
        /// </summary>
        public string Name { get; }


        [JsonIgnore]
        public string Path => WORLD_SAVE_DIRECTORY + $"{Name}\\Save.world";

        /// <summary>
        /// Load the world
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="pack">Package</param>
        /// <returns>Loaded world</returns>
        public ILoadable Load(string path, ZipFile pack)
        {
            if (_instance != null)
                return null;

            if (!File.Exists(path))
                return null;

            var name = System.IO.Path.GetDirectoryName(path).Split('\\', StringSplitOptions.RemoveEmptyEntries).Last();
            var seed = "";

            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                seed = connection.QueryFirst<string>("SELECT \"Value\" FROM Metadata WHERE \"Name\"=\"Seed\"");
            }

            return new World(name, seed);
        }

        /// <summary>
        /// Save the world
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));

            if (!File.Exists(Path))         // Create new save file
                CreateSaveFile();

            // Save existing
        }

        private string LoadPropertyFromStorage(string propertyName) => _storageConnection.QueryFirstOrDefault<string>("SELECT Value FROM Metadata WHERE Name = @propertyName LIMIT 1", new { propertyName });
        private void SavePropertyToStorage(string propertyName, string value) => _storageConnection.Query("INSERT OR REPLACE INTO Metadata(Name, Value) VALUES(@propertyName, @value)", new { propertyName, value });
        private void CreateSaveFile()
        {
            _storageConnection.Query(CREATE_WORLD_FILE_V1);
            //_storageConnection.Query($"INSERT INTO Metadata(\"Name\", \"Value\") VALUES(\"Seed\", @seed)", new { seed = Seed });
            SavePropertyToStorage("Seed", Seed);
        }

        private void OpenStorage()
        {

            if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            var newSave = !File.Exists(Path);
            _storageConnection = new SqliteConnection($"Data Source={Path}");
            _storageConnection.Open();
            if (newSave)
                CreateSaveFile();
        }

        private void CloseStorage()
        {
            if (_storageConnection != null)
                _storageConnection.Dispose();
            _storageConnection = null;
        }

        private bool ChunkExists(int x, int z) => _storageConnection.QueryFirst<int>("SELECT COUNT(*) FROM Chunks WHERE X=@X AND Y=@Y LIMIT 1", new { X = x, Y = z }) > 0;

        private void LoadChunkFromFile(Chunk chunk)
        {
            //var exists = _storageConnection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Chunks WHERE X=@X AND Y=@Y", new { X = x, Y = z }) > 0;
            //if (!exists) return null;
            //return new Chunk(new Vector2(x, z));

            var pos = chunk.Position;
            //Task.Factory.StartNew(() =>
            {
                var rawData = _storageConnection.QueryFirst<byte[]>("SELECT Voxels FROM Chunks WHERE X=@X AND Y=@Y LIMIT 1", new { X = (int)pos.X, Y = (int)pos.Y });
                try
                {
                    for (int x = 0; x < Chunk.WIDTH; x++)
                        for (int z = 0; z < Chunk.WIDTH; z++)
                            for (int y = 0; y < Chunk.HEIGHT; y++)
                                if (rawData[x * Chunk.WIDTH * Chunk.HEIGHT + z * Chunk.HEIGHT + y] > 0)
                                    chunk.PlaceBlock(x, y, z, rawData[x * Chunk.WIDTH * Chunk.HEIGHT + z * Chunk.HEIGHT + y], false);
                }
                catch
                {

                }
                lock (_loadedChunks)
                    _loadedChunks.Add(chunk);
                RequestChunkUpdate(chunk, true, 7, 7);
            }//);
        }

        private void SaveChunkToFile(Chunk chunk, bool sync)
        {
            // TODO: Do this in async to avoid lags
            using var voxelStream = new MemoryStream();
            var blocks = chunk.Blocks;
            for (int x = 0; x < Chunk.WIDTH; x++)
                for (int z = 0; z < Chunk.WIDTH; z++)
                    for (int y = 0; y < Chunk.HEIGHT; y++)
                        voxelStream.WriteByte((byte)(blocks[x, y, z]?.id ?? 0)); // 0 = null/air; > 0 = block

            var voxels = voxelStream.GetBuffer();
            //var task = Task.Factory.StartNew(() =>
            {
                if (ChunkExists((int)chunk.Position.X, (int)chunk.Position.Y))
                    _storageConnection.Query("UPDATE Chunks SET Voxels = @voxels WHERE X=@X AND Y=@Y", new { X = (int)chunk.Position.X, Y = (int)chunk.Position.Y, voxels });
                else
                    _storageConnection.Query("INSERT INTO Chunks(X, Y, Voxels, Entities) VALUES(@X, @Y, @voxels, @entities)", new { X = (int)chunk.Position.X, Y = (int)chunk.Position.Y, voxels, entities = "{}" });
            }//);

            /* if (sync)
                 task.Wait();*/
        }

        private PlayerDTO GetPlayerFromStorage(int playerId)
        {
            return _storageConnection.QueryFirstOrDefault<PlayerDTO>("SELECT * FROM Players WHERE ID = @playerId LIMIT 1", new { playerId });
        }

        private void StorePlayerInStorage(PlayerDTO dto)
        {
            _storageConnection.Query("REPLACE INTO Players(ID, Name, PositionSerialized, InventorySerialized, MetadataSerialized) VALUES(@ID, @Name, @PositionSerialized, @InventorySerialized,@MetadataSerialized)", new
            {
                PositionSerialized = dto.PositionSerialized,
                InventorySerialized = dto.InventorySerialized,
                MetadataSerialized = dto.MetadataSerialized,
                Name = dto.Name,
                ID = dto.ID
            });
        }

        // TODO: Move to player class
        /// <summary>
        /// Player data transfer object 
        /// </summary>
        private class PlayerDTO
        {
            public int ID { get; set; } = 0;
            public string Name { get; set; } = "Default";

            internal string PositionSerialized
            {
                get => JsonConvert.SerializeObject(Position);
                set => Position = JsonConvert.DeserializeObject<Transform>(value);
            }

            public Transform Position { get; set; } = new Transform(Vector3.Zero, Vector3.Zero);


            internal string InventorySerialized
            {
                get => JsonConvert.SerializeObject(Inventory);
                set => Inventory = JsonConvert.DeserializeObject<List<ItemStack>>(value);
            }

            public List<ItemStack> Inventory { get; set; } = new List<ItemStack>();

            internal string MetadataSerialized
            {
                get => JsonConvert.SerializeObject(Metadata);
                set => Metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
            }

            public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

            public class Transform
            {
                [JsonConverter(typeof(Vector3Converter))]
                public Vector3 Position { get; set; }
                [JsonConverter(typeof(Vector3Converter))]
                public Vector3 Rotation { get; set; }

                public Transform(Vector3 position, Vector3 rotation)
                {
                    Position = position;
                    Rotation = rotation;
                }
            }
        }

        private const string CREATE_WORLD_FILE_V1 = @"
-- World metadata table
CREATE TABLE ""Metadata""(

    ""Name""  TEXT NOT NULL,
	""Value"" TEXT NOT NULL,
	PRIMARY KEY(""Name"")
);

-- Store file format version in there
INSERT INTO Metadata(""Name"", ""Value"") VALUES(""Version"", ""1"");

-- Chunk table
CREATE TABLE ""Chunks""(

    ""X"" INTEGER NOT NULL,
	""Y"" INTEGER NOT NULL,
	""Voxels""    BLOB NOT NULL,
	""Entities""  TEXT NOT NULL,
	PRIMARY KEY(""X"",""Y"")
);

-- Players table
CREATE TABLE ""Players""(

    ""ID""    INTEGER NOT NULL,
	""Name""  TEXT NOT NULL,
	""PositionSerialized""  TEXT NOT NULL,
	""InventorySerialized"" TEXT NOT NULL,
	""MetadataSerialized""  TEXT NOT NULL,
	PRIMARY KEY(""ID"")
);
";
    }
}
