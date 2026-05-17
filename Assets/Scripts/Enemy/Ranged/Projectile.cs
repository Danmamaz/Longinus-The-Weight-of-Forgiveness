using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.InGameItems
{
    /// <summary>
    /// Self-propelled projectile that applies damage on contact and self-destructs after a set lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("Seconds before the projectile destroys itself if it hits nothing.")]
        private float _lifetime = 5f;

        #endregion

        #region Private Variables

        private float _damage;
        private float _poiseDamage;
        private GameObject _owner;
        private Rigidbody _rb;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void FixedUpdate()
        {
            _lifetime -= Time.fixedDeltaTime;
            if (_lifetime <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == _owner) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = (transform.position - hitPoint).normalized;
                damageable.TakeDamage(_damage, _poiseDamage, hitPoint, hitNormal);
            }

            Destroy(gameObject);
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Initialises velocity and damage values. Must be called immediately after instantiation.
        /// </summary>
        public void Launch(Vector3 direction, float speed, float damage, float poiseDamage, GameObject owner)
        {
            _damage = damage;
            _poiseDamage = poiseDamage;
            _owner = owner;
            _rb.linearVelocity = direction * speed;
        }

        #endregion
    }
}
