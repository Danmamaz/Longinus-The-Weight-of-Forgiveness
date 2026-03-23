using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.InGameItems
{
    /// <summary>
    /// Handles collision detection for attacks and applies damage and poise damage to valid targets.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DamageCollider : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Damage Settings")]
        [SerializeField, Tooltip("Base damage applied on hit.")] 
        private float _damageAmount;
        
        [SerializeField, Tooltip("Poise damage applied on hit.")] 
        private float _poiseAmount;
        
        [SerializeField, Tooltip("Reference to the entity that owns this collider to prevent self-damage.")] 
        private GameObject _owner;
        
        #endregion

        #region Private Variables
        
        private Collider _collider;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == _owner) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = (transform.position - hitPoint).normalized;

                damageable.TakeDamage(_damageAmount, _poiseAmount, hitPoint, hitNormal);
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Enables the damage collider. Usually called via Animation Events.
        /// </summary>
        public void Enable() => _collider.enabled = true;

        /// <summary>
        /// Disables the damage collider. Usually called via Animation Events.
        /// </summary>
        public void Disable() => _collider.enabled = false;

        #endregion
    }
}