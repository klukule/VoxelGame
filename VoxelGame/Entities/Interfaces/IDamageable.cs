namespace VoxelGame.Entities
{
    /// <summary>
    /// Entity that has health and can receive damage
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Entity health
        /// </summary>
        int Health { get; set; }

        /// <summary>
        /// Kill entity
        /// </summary>
        void Die();

        /// <summary>
        /// Deal damage to the entity
        /// </summary>
        /// <param name="damage">amount of damage</param>
        void TakeDamage(int damage);
    }
}
