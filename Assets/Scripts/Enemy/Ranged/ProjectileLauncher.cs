using Longinus.EnemySystem;
using UnityEngine;

namespace Longinus.InGameItems
{
    /// <summary>
    /// Spawns and launches a projectile toward a target transform.
    /// Attach to the ranged enemy GameObject alongside RangedEnemyController.
    /// </summary>
    public class ProjectileLauncher : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Projectile Setup")]
        [SerializeField, Tooltip("Projectile prefab. Must have a Projectile component.")]
        private GameObject _projectilePrefab;

        [SerializeField, Tooltip("World-space spawn point for the projectile (e.g. tip of a staff or barrel).")]
        private Transform _spawnPoint;

        [SerializeField, Tooltip("Initial speed given to the projectile on launch.")]
        private float _projectileSpeed = 15f;

        [Header("Damage Values")]
        [SerializeField, Tooltip("Health damage applied when the projectile hits an IDamageable.")]
        private float _damage = 10f;

        [SerializeField, Tooltip("Poise damage applied when the projectile hits an IDamageable.")]
        private float _poiseDamage = 5f;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Instantiates a projectile at the spawn point and fires it toward the target.
        /// </summary>
        public void Fire()
        {
            Vector3 target = GetComponent<EnemyController>().LastKnownPlayerPosition;
            if (_projectilePrefab == null || _spawnPoint == null || target == null)
            {
                Debug.LogWarning("[ProjectileLauncher] Missing prefab, spawn point, or target — Fire() aborted.");
                return;
            }

            GameObject obj = Instantiate(_projectilePrefab, _spawnPoint.position, _spawnPoint.rotation);

            if (obj.TryGetComponent(out Projectile projectile))
            {
                Vector3 direction = (target - _spawnPoint.position).normalized;
                projectile.Launch(direction, _projectileSpeed, _damage, _poiseDamage, gameObject);
            }
            else
            {
                Debug.LogError("[ProjectileLauncher] Projectile prefab is missing a Projectile component.");
            }
        }

        #endregion
    }
}
