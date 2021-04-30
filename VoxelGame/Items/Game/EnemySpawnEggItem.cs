using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Blocks;
using VoxelGame.Entities;
using VoxelGame.Worlds;

namespace VoxelGame.Items
{
    public class EnemySpawnEggItem : Item
    {
        public override string Key => "Item_Enemy_Spawn_Egg";
        public override string IconLocation => "Textures/Items/Enemy_Spawn_Egg.png";
        public EnemySpawnEggItem()
        {
            GenerateGraphics();
            ItemDatabase.RegisterItem(this);
        }

        public override void OnInteract(Vector3 position, Chunk chunk)
        {
            // Can spawn
            if (chunk.GetBlockID((int)position.X, (int)position.Y, (int)position.Z) <= 0 &&
                chunk.GetBlockID((int)position.X, (int)position.Y + 1, (int)position.Z) <= 0)
                World.Instance.AddEntity(new TestingEnemyEntity() { Position = new Vector3(chunk.Position.X * Chunk.WIDTH + 0.5f, 0, chunk.Position.Y * Chunk.WIDTH + 0.5f) + position });
        }
    }
}
