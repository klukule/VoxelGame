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

        private void CreateSaveFile()
        {
            _storageConnection.Query(CREATE_WORLD_FILE_V1);
            _storageConnection.Query($"INSERT INTO Metadata(\"Name\", \"Value\") VALUES(\"Seed\", @seed)", new { seed = Seed });
        }

        private void OpenStorage()
        {
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

        public bool ChunkExists(int x, int z) => _storageConnection.QueryFirst<int>("SELECT COUNT(*) FROM Chunks WHERE X=@X AND Y=@Y", new { X = x, Y = z }) > 0;

        public void LoadChunkFromFile(Chunk chunk)
        {
            //var exists = _storageConnection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Chunks WHERE X=@X AND Y=@Y", new { X = x, Y = z }) > 0;
            //if (!exists) return null;
            //return new Chunk(new Vector2(x, z));

            var pos = chunk.Position;
            //Task.Factory.StartNew(() =>
            {
                var rawData = _storageConnection.QueryFirst<byte[]>("SELECT Voxels FROM Chunks WHERE X=@X AND Y=@Y", new { X = (int)pos.X, Y = (int)pos.Y });
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

        public void SaveChunkToFile(Chunk chunk, bool sync)
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
	""Position""  TEXT NOT NULL,
	""Inventory"" TEXT NOT NULL,
	PRIMARY KEY(""ID"")
);
";
    }
}
