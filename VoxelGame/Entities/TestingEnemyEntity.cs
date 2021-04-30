using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoxelGame.Assets;
using VoxelGame.Physics;
using VoxelGame.Rendering;
using VoxelGame.Worlds;

namespace VoxelGame.Entities
{
    public class TestingEnemyEntity : Entity, IDamageable
    {
        public const int MAX_HEALTH = 20;

        private float _walkSpeed = 3f;
        private Rigidbody _rigidbody;       // Player colision model
        private Vector3 _vel;               // Player velocity
        private World _currentWorld;
        private float _currentHealth = MAX_HEALTH;
        private Mesh _mesh;

        private int _pathWaypointIndex = 0;
        private List<Vector3> _path = null;
        private float _pathUpdateTickRate = 0.25f;
        private float _pathUpdateTick;
        private bool _pathSearching = false;

        private float _damageTickRate = 1f;
        private float _damageTick;

        public int Health
        {
            get => (int)Math.Ceiling(_currentHealth);
            set => _currentHealth = value;
        }

        public TestingEnemyEntity()
        {
            _mesh = AssetDatabase.GetAsset<Mesh>("Models/Cube.obj");
        }

        public override void Begin()
        {
            _currentWorld = World.Instance;
            Name = "Testing enemy";
            _rigidbody = new Rigidbody(this, 70, new BoundingBox(-0.25f, 0.25f, 0, 2, -0.25f, 0.25f));
        }

        public override void Update()
        {
            _vel = Vector3.Zero;
            if (_pathUpdateTick + _pathUpdateTickRate <= Time.GameTime && !_pathSearching)
            {
                // TODO: Better movement
                // TODO: Fix collection changed exception
                _pathSearching = true;
                // Try find path only if near player
                Task.Factory.StartNew(() =>
                {
                    var dist = Vector3.Distance(_currentWorld.Player.Position, Position);
                    if (dist > 2 && dist < 16)
                    {
                        try
                        {
                            _path = AStarPathFinder.FindPath(Position, _currentWorld.Player.Position, 32);
                            _pathWaypointIndex = 0;
                        }
                        catch
                        {
                            //_path = null;
                        }
                    }

                    _pathUpdateTick = Time.GameTime;
                    _pathSearching = false;
                });
            }

            if (_path != null && _pathWaypointIndex >= _path.Count) _path = null;
            if (_path != null)
            {
                var vec = Vector3.NormalizeFast(_path[_pathWaypointIndex] - Position);
                _vel = vec * _walkSpeed;
                _vel.Y *= 0.1f;

                if (Vector3.Distance(Position, _path[_pathWaypointIndex]) < 0.5f)
                    _pathWaypointIndex++;
            }
            _rigidbody.Velocity = new Vector3(_vel.X, _rigidbody.Velocity.Y + _vel.Y, _vel.Z);

            // Damage player
            if (Vector3.Distance(_currentWorld.Player.Position, Position) <= 2)
            {
                if(_damageTick + _damageTickRate <= Time.GameTime)
                {
                    _currentWorld.Player.TakeDamage(2);
                    _damageTick = Time.GameTime;
                }
            }
        }

        public override void Render()
        {
            var material = AssetDatabase.GetAsset<Material>("Materials/Fallback.mat");
            material.SetTexture(0, AssetDatabase.GetAsset<Texture>("Textures/FallBack.png"));
            var mat = Matrix4.CreateScale(new Vector3(0.5f, 2, 0.5f)) * Matrix4.CreateTranslation(Position);
            Renderer.DrawRequest(_mesh, material, mat);
        }

        public void Die()
        {
        }

        public void TakeDamage(int damage)
        {
        }

        // TODO: Move to separate file
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            float toVector_x = target.X - current.X;
            float toVector_y = target.Y - current.Y;
            float toVector_z = target.Z - current.Z;

            float sqdist = toVector_x * toVector_x + toVector_y * toVector_y + toVector_z * toVector_z;

            if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
                return target;
            var dist = (float)Math.Sqrt(sqdist);

            return new Vector3(current.X + toVector_x / dist * maxDistanceDelta,
                current.Y + toVector_y / dist * maxDistanceDelta,
                current.Z + toVector_z / dist * maxDistanceDelta);
        }
    }
}
