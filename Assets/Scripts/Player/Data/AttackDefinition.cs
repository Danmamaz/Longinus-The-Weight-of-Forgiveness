using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// Defines settings of an exact attack.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttack", menuName = "Longinus/Combat/Attack Definition")]
    public class AttackDefinition : ScriptableObject
    {
        [Tooltip("The exact name of animation state or a trigger in Animator")]
        public string animationStateName;

        [Tooltip("Stamina multiplier for this attack (base cost * by this multiplier)")]
        public float staminaMultiplier = 1f;

        [Tooltip("Damage multiplier (base weapon damage * by this multiplier)")]
        public float damageMultiplier = 1f;
        [Tooltip("Strenght of a physical impulse forward, when the attack is performed")]
        public float forwardStepForce = 0f;

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