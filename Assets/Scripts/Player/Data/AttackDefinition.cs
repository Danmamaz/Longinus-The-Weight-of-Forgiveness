using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// ScriptableObject that defines the properties of a single attack in a combo chain.
    /// Hashes the animation state name at load time to avoid per-frame string lookups.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttack", menuName = "Longinus/Combat/Attack Definition")]
    public class AttackDefinition : ScriptableObject
    {
        [Tooltip("Exact name of the Animator state or trigger to cross-fade into.")]
        public string animationStateName;

        [Tooltip("Stamina cost multiplier applied on top of PlayerCombatManager._baseStaminaCost.")]
        public float staminaMultiplier = 1f;

        [Tooltip("Damage multiplier applied on top of the weapon's base damage. Not yet wired to DamageCollider.")]
        public float damageMultiplier = 1f;

        [Tooltip("Forward impulse force (Impulse mode) applied to the player when the attack fires.")]
        public float forwardStepForce = 0f;

        /// <summary>
        /// Pre-hashed animator state ID. Set automatically from animationStateName on asset load.
        /// </summary>
        public int AnimationHash { get; private set; }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(animationStateName))
            {
                AnimationHash = Animator.StringToHash(animationStateName);
            }
        }
    }
}
