using UnityEngine;

namespace Longinus.Interfaces
{
    /// <summary>
    /// Defines an entity that can receive health and poise damage from external sources.
    /// </summary>
    public interface IDamageable
    {
        #region Core Methods
        
        /// <summary>
        /// Applies damage and stagger impact to the entity.
        /// </summary>
        /// <param name="amount">The raw health damage value.</param>
        /// <param name="poiseDamage">The stagger/poise damage value.</param>
        /// <param name="hitPoint">The exact world coordinate where the impact occurred.</param>
        /// <param name="hitNormal">The directional normal of the impact surface.</param>
        void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal);
        
        #endregion
    }
}