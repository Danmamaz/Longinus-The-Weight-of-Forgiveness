using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Executes a single-frame radial damage burst when an AoE Slam animation event fires.
    /// Does not use a tracked bone collider — damage is applied instantly via OverlapSphere.
    /// </summary>
    public class AoESlamHitbox : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("Radius of the ground-slam blast in world units.")]
        private float _radius = 4f;

        [SerializeField, Tooltip("Flat damage dealt to each target inside the radius.")]
        private float _damage = 25f;

        [SerializeField, Tooltip("Poise damage dealt to each target inside the radius.")]
        private float _poiseDamage = 30f;

        [SerializeField, Tooltip("Layer mask for targets that can be hit (player layer).")]
        private LayerMask _hitLayer;

        [SerializeField, Tooltip("Origin of the blast — typically the boss's feet bone.")]
        private Transform _impactCenter;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Called via Animation Event. Damages all IDamageable targets in radius on the same frame.
        /// </summary>
        public void TriggerSlam()
        {
            if (_impactCenter == null) return;

            Collider[] hits = Physics.OverlapSphere(_impactCenter.position, _radius, _hitLayer);
            foreach (var c in hits)
            {
                if (c.TryGetComponent(out IDamageable d))
                {
                    Vector3 hitPoint = c.ClosestPoint(_impactCenter.position);
                    Vector3 normal = (c.transform.position - _impactCenter.position).normalized;
                    d.TakeDamage(_damage, _poiseDamage, hitPoint, normal);
                }
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_impactCenter == null) return;
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
            Gizmos.DrawWireSphere(_impactCenter.position, _radius);
        }
#endif
    }
}
