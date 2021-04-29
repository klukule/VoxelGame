using OpenTK;
using System;
using System.Collections.Generic;

namespace VoxelGame.Physics
{
    /// <summary>
    /// Simple rigidbody physics simulator
    /// </summary>
    public static class Sim
    {
        private static List<Rigidbody> _rigidbodies = new List<Rigidbody>();

        public static float FixedTimeStep = 0.03333f;
        public static Vector3 Gravity = new Vector3(0, -25, 0);

        /// <summary>
        /// Registers new rigidbody
        /// </summary>
        public static void AddRigidbody(Rigidbody body) => _rigidbodies.Add(body);

        /// <summary>
        /// Removes registered rigidbody
        /// </summary>
        public static void RemoveRigidbody(Rigidbody body) => _rigidbodies.Remove(body);

        public static void Update(float deltaTime)
        {
            // Clamp delta time to some maximum value to avoid issues when there is large freeze
            if (deltaTime > FixedTimeStep)
                deltaTime = FixedTimeStep;

            // Simulate each rigid body
            for (int i = 0; i < _rigidbodies.Count; i++)
            {
                var body = _rigidbodies[i];
                if (!body.IsActive) continue;
                var normVelocity = body.Velocity.Normalized();

                // 
                for (int c = 0; c < body.CollisionShapes.Length; c++)
                {
                    body.Velocity += Gravity * deltaTime;

                    if (body.Velocity.X != 0)
                    {
                        if (body.CollisionShapes[c]
                            .IntersectsWorldDirectional(body, new Vector3(normVelocity.X / 10, .25f, 0)))
                        {
                            body.Owner.OnPreVoxelCollisionEnter();
                            body.Velocity = new Vector3(0, body.Velocity.Y, body.Velocity.Z);
                            body.Owner.OnPostVoxelCollisionEnter();
                        }
                    }

                    if (body.Velocity.Z != 0)
                    {
                        if (body.CollisionShapes[c]
                            .IntersectsWorldDirectional(body, new Vector3(0, .25f, normVelocity.Z / 10)))
                        {
                            body.Owner.OnPreVoxelCollisionEnter();
                            body.Velocity = new Vector3(body.Velocity.X, body.Velocity.Y, 0);
                            body.Owner.OnPostVoxelCollisionEnter();
                        }
                    }

                    if (body.Velocity.Y > 0)
                    {
                        if (body.CollisionShapes[c].IntersectsWorldDirectional(body, new Vector3(0, .1f, 0)))
                        {
                            body.Owner.OnPreVoxelCollisionEnter();
                            body.Velocity = new Vector3(body.Velocity.X, 0, body.Velocity.Z);
                            body.Owner.OnPostVoxelCollisionEnter();
                        }
                    }
                    else if (body.Velocity.Y < 0)
                    {
                        if (body.CollisionShapes[c].IntersectsWorldDirectional(body, new Vector3(0, -.1f, 0)))
                        {
                            body.Owner.OnPreVoxelCollisionEnter();
                            body.Velocity = new Vector3(body.Velocity.X, 0, body.Velocity.Z);
                            body.Owner.Position = new Vector3(body.Owner.Position.X,
                                (float)Math.Round(body.Owner.Position.Y), body.Owner.Position.Z);
                            body.Owner.OnPostVoxelCollisionEnter();
                        }
                    }
                }

                body.Velocity *= 1 / (1 + body.Drag * deltaTime);
                body.Owner.Position += body.Velocity * deltaTime;
            }
        }
    }
}
