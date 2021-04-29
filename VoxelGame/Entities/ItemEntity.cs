using OpenTK;
using VoxelGame.Assets;
using VoxelGame.Blocks;
using VoxelGame.Worlds;
using VoxelGame.Items;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Entities
{
    /// <summary>
    /// Dropped item entity
    /// </summary>
    public class ItemEntity : Entity
    {
        private Item _item;             // Item that this entity is for
        private Rigidbody _rigidbody;   // Entity collision shape

        /// <summary>
        /// Create new entity for given item
        /// </summary>
        /// <param name="i">The item</param>
        public ItemEntity(Item i) { _item = i; }

        public override void Begin()
        {
            // Create rigid body and add initial impulse
            _rigidbody = new Rigidbody(this, 5, new BoundingBox(-0.125f, 0.125f, -0.125f, 0.125f, -0.125f, 0.125f));
            _rigidbody.AddImpluse(new Vector3(0, 30, 0));
            base.Begin();
        }

        public override void Update()
        {
            var chunkPos = ChunkPosition;

            // Get chunk the entity is in
            if (World.Instance.TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Y, out Chunk chunk))
            {
                // Check if the entity is in the water, if so then apply underwater drag
                var block = Position.ToChunkSpaceFloored();
                bool isInWater = chunk.GetBlockID((int)block.X, (int)block.Y, (int)block.Z) == GameBlocks.WATER.ID;
                _rigidbody.Drag = isInWater ? UnderWaterDrag : 0;
            }

            // Get position near player's feet
            var goToPos = World.Instance.WorldCamera.Position - Vector3.UnitY * 1;

            // Get distance between item and player
            var dist = Vector3.Distance(Position, goToPos);
            if (dist < 2) // If is less than two units away
            {
                // Disable physics & move the item to player
                _rigidbody.IsActive = false;
                var dir = (goToPos - Position).Normalized();
                var move = (dir / dist) * 5;
                Position += move * Time.DeltaTime;
                if (dist < 0.5f) // If less than half a block away, "suck in" and add to players inventory
                {
                    //Add to inventory
                    World.Instance.Player.GetInventory().AddItem(_item);
                    World.Instance.DestroyEntity(this);
                }
            }
            else
            {
                _rigidbody.IsActive = true;
            }
        }

        public override void Destroyed()
        {
            _rigidbody.ClearOwner();
            _rigidbody = null;
        }

        public override void Render()
        {
            // Draw spining item on the ground
            var material = AssetDatabase.GetAsset<Material>("Materials/Fallback.mat");
            material.SetTexture(0, _item.Icon);
            var mat = Matrix4.CreateScale(new Vector3(.25f, -.25f, .25f)) * Matrix4.CreateRotationY(Time.GameTime) * Matrix4.CreateTranslation(Position);
            Renderer.DrawRequest(_item.Mesh, material, mat);

            base.Render();
        }
    }
}
