using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Attach to weapon bone / hitbox child object.
    /// Enable / Disable called from Animation Events.
    /// Uses OverlapSphere per FixedUpdate tick while active.
    /// </summary>
    public sealed class BossDamageCollider : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────
        [Header("Overlap Settings")]
        [SerializeField] private float     _radius     = 0.4f;
        [SerializeField] private Vector3   _offset     = Vector3.zero;
        [SerializeField] private LayerMask _targetMask;

        [Header("Damage Source")]
        [Tooltip("Resolved at runtime from BossCombatController.CurrentAttack.")]
        [SerializeField] private BossCombatController _combatController;

        [Header("Poise Damage")]
        [Tooltip("Poise damage dealt per hit (independent of HP damage).")]
        [SerializeField, Min(0f)] private float _poiseDamage = 20f;

        // ── Runtime ────────────────────────────────────────────
        private readonly HashSet<Collider> _alreadyHit = new();
        private readonly Collider[]        _hitBuffer  = new Collider[16];
        private bool _active;

        // ── Animation Event Entry Points ───────────────────────

        /// <summary>Call from Animation Event at attack active-frame start.</summary>
        public void EnableCollider()
        {
            _alreadyHit.Clear();
            _active = true;
        }

        /// <summary>Call from Animation Event at attack active-frame end.</summary>
        public void DisableCollider()
        {
            _active = false;
        }

        // ── Physics ────────────────────────────────────────────
        private void FixedUpdate()
        {
            if (!_active) return;

            Vector3 center = transform.TransformPoint(_offset);
            int count = Physics.OverlapSphereNonAlloc(center, _radius, _hitBuffer, _targetMask);

            for (int i = 0; i < count; i++)
            {
                Collider col = _hitBuffer[i];

                // Skip duplicates within the same swing.
                if (!_alreadyHit.Add(col)) continue;

                // Compute hit point & normal for IDamageable signature.
                Vector3 hitPoint  = col.ClosestPoint(center);
                Vector3 hitNormal = (hitPoint - center).normalized;

                // HP damage.
                if (col.TryGetComponent(out IDamageable damageable))
                {
                    float damage = _combatController != null && _combatController.CurrentAttack != null
                        ? _combatController.CurrentAttack.Damage
                        : 0f;

                    damageable.TakeDamage(damage, hitPoint, hitNormal);
                }

                // Poise damage.
                if (_poiseDamage > 0f && col.TryGetComponent(out BossPoiseManager poise))
                {
                    poise.TakePoiseDamage(_poiseDamage);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _active ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.TransformPoint(_offset), _radius);
        }
#endif
    }
}