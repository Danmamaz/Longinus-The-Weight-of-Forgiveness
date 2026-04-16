using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// Визначає параметри конкретного удару в комбо-серії.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttack", menuName = "Longinus/Combat/Attack Definition")]
    public class AttackDefinition : ScriptableObject
    {
        [Tooltip("The exact name of animation state or a trigger in Animator")]
        public string animationStateName;

        [Tooltip("Множник витривалості для цієї атаки (базова вартість * цей множник)")]
        public float staminaMultiplier = 1f;

        [Tooltip("Множник шкоди (базова шкода зброї * цей множник)")]
        public float damageMultiplier = 1f;

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