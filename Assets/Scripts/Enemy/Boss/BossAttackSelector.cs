using System.Collections.Generic;
using UnityEngine;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Maintains per-attack cooldowns and performs weighted-random attack selection
    /// based on phase eligibility and distance to the player.
    /// </summary>
    public class BossAttackSelector : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("All attack definitions available to this boss.")]
        private BossAttackDefinition[] _allAttacks;

        [Header("Phase 2 Modifiers")]
        [SerializeField, Tooltip("Cooldown multiplier applied to all attacks during Phase 2. 0.7 = 30% faster.")]
        private float _phase2CooldownMultiplier = 0.7f;

        [SerializeField, Tooltip("Damage multiplier available to attacks during Phase 2. Consumed by individual attacks.")]
        private float _phase2DamageMultiplier = 1.2f;

        #endregion

        #region Private Variables

        private Dictionary<string, float> _cooldowns = new();
        private BossController _boss;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _boss = GetComponent<BossController>();
            foreach (var a in _allAttacks)
            {
                if (a != null) _cooldowns[a.AttackId] = 0f;
            }
        }

        private void Update()
        {
            var keys = new List<string>(_cooldowns.Keys);
            foreach (var k in keys)
            {
                if (_cooldowns[k] > 0f)
                    _cooldowns[k] -= Time.deltaTime;
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Returns the best attack for the current phase and distance, or null if none qualify.
        /// </summary>
        public BossAttackDefinition SelectAttack(float distanceToPlayer)
        {
            var valid = new List<(BossAttackDefinition def, float weight)>();

            foreach (var a in _allAttacks)
            {
                if (a == null) continue;
                if (!a.IsAllowedInPhase(_boss.CurrentPhase)) continue;
                if (_cooldowns.TryGetValue(a.AttackId, out float cd) && cd > 0f) continue;
                if (distanceToPlayer < a.MinRange) continue;
                if (distanceToPlayer > a.MaxRange) continue;
                valid.Add((a, a.Weight));
            }

            if (valid.Count == 0) return null;
            return WeightedRandom(valid);
        }

        /// <summary>
        /// Records that an attack was used and starts its cooldown, scaled by the current phase multiplier.
        /// </summary>
        public void MarkAttackUsed(BossAttackDefinition def)
        {
            if (def != null)
                _cooldowns[def.AttackId] = def.Cooldown * GetCurrentCooldownMultiplier();
        }

        public float GetCurrentCooldownMultiplier()
        {
            return _boss.CurrentPhase == BossController.BossPhase.Phase2
                ? _phase2CooldownMultiplier : 1f;
        }

        public float GetCurrentDamageMultiplier()
        {
            return _boss.CurrentPhase == BossController.BossPhase.Phase2
                ? _phase2DamageMultiplier : 1f;
        }

        private BossAttackDefinition WeightedRandom(List<(BossAttackDefinition def, float weight)> items)
        {
            float total = 0f;
            foreach (var (_, w) in items) total += w;

            float roll = Random.Range(0f, total);
            float acc = 0f;
            foreach (var (def, w) in items)
            {
                acc += w;
                if (roll <= acc) return def;
            }
            return items[items.Count - 1].def;
        }

        #endregion
    }
}
