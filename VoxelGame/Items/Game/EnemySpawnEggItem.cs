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
            var enemy = new TestingEnemyEntity();
            enemy.Position = new Vector3(chunk.Position.X * Chunk.WIDTH, 0, chunk.Position.Y * Chunk.WIDTH) + position;
            World.Instance.AddEntity(enemy);
        }
    }
}
