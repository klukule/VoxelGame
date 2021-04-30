using OpenTK;
using VoxelGame.Physics;

namespace VoxelGame.Entities
{
    /// <summary>
    /// Base class for any entity
    /// </summary>
    public abstract class Entity
    {
        // Drag when underwater
        protected const float UnderWaterDrag = 25;

        /// <summary>
        /// Entity name
        /// </summary>
        public string Name;

        /// <summary>
        /// Entity position
        /// </summary>
        public Vector3 Position;

        // TODO: Use quaternions
        /// <summary>
        /// Entity Euler rotation (Pitch, Yaw, Roll)
        /// </summary>
        public Vector3 Rotation;

        /// <summary>
        /// Entity collider
        /// </summary>
        public virtual Rigidbody Collision => null;

        /// <summary>
        /// Forward vector the entity is facing
        /// </summary>
        public Vector3 ForwardVector => Mathf.GetForwardFromRotation(Rotation);

        /// <summary>
        /// Right vector from entity's point of view
        /// </summary>
        public Vector3 RightVector => Mathf.GetRightFromRotation(Rotation);

        /// <summary>
        /// Up vector from entity's point of view
        /// </summary>
        public Vector3 UpVector => Mathf.GetUpFromRotation(Rotation);

        /// <summary>
        /// Returns entity position aligned to full block
        /// </summary>
        public Vector3 BlockPosition => new Vector3((int)Position.X, (int)Position.Y, (int)Position.Z);

        /// <summary>
        /// Chunk coordinates the entity is in
        /// </summary>
        public Vector2 ChunkPosition
        {
            get
            {
                var pos = Position.ToChunkPosition();
                return new Vector2(pos.X, pos.Z);
            }
        }

        /// <summary>
        /// Chunk relative position
        /// </summary>
        public Vector3 PositionInChunk => Position.ToChunkSpace();

        /// <summary>
        /// Called on world load
        /// </summary>
        public virtual void Begin() { }

        /// <summary>
        /// Called on world exit
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Called during each update
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Called during scene rendering
        /// </summary>
        public virtual void Render() { }

        /// <summary>
        /// Called during cleanup
        /// </summary>
        public virtual void Destroyed() { }

        /// <summary>
        /// Called during GUI render pass
        /// </summary>
        public virtual void RenderGUI() { }

        /// <summary>
        /// Called before entity vs. voxel collition happend
        /// </summary>
        public virtual void OnPreVoxelCollisionEnter() { }

        /// <summary>
        /// Called after entity vs. voxel collition happend
        /// </summary>
        public virtual void OnPostVoxelCollisionEnter() { }
    }
}
