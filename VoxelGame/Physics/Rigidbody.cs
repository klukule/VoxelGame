using OpenTK;
using VoxelGame.Entities;

namespace VoxelGame.Physics
{
    /// <summary>
    /// Simple rigid body containing 1 or more collision shapes
    /// </summary>
    public class Rigidbody
    {
        private BoundingBox[] _collisionShapes;

        /// <summary>
        /// Owner actor
        /// </summary>
        public Entity Owner { get; private set; }

        /// <summary>
        /// Whether the body is active or not
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Body mass
        /// </summary>
        public float Mass { get; set; } = 1;

        /// <summary>
        /// Body drag
        /// </summary>
        public float Drag { get; set; } = 0;

        /// <summary>
        /// Body velocity
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Collision shapes defining this body
        /// </summary>
        public BoundingBox[] CollisionShapes => _collisionShapes;

        /// <summary>
        /// Create new body
        /// </summary>
        public Rigidbody(Entity owner, float mass, params BoundingBox[] colShapes)
        {
            Owner = owner;
            Mass = mass;
            _collisionShapes = colShapes;

            Sim.AddRigidbody(this);
        }

        /// <summary>
        /// Removes assigned owner - removes rigidbody from simulation
        /// </summary>
        public void ClearOwner()
        {
            Owner = null;
            Sim.RemoveRigidbody(this);
        }

        /// <summary>
        /// Adds instant force (impulse)
        /// </summary>
        public void AddImpluse(Vector3 impulseForce) => Velocity += impulseForce / Mass;

        /// <summary>
        /// Adds constant force
        /// </summary>
        public void AddForce(Vector3 force) => Velocity += force * Time.DeltaTime / Mass;
    }
}
