using UnityEngine;

namespace Longinus.EnemySystem
{
    [CreateAssetMenu(fileName = "BossAttack_New", menuName = "Longinus/Combat/Boss Attack Definition")]
    public class BossAttackDefinition : ScriptableObject
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("Unique identifier used by the selector and cooldown tracker (e.g. 'Sweep', 'Thrust', 'AoESlam').")]
        private string _attackId;

        [SerializeField, Tooltip("Animator trigger parameter name to fire this attack (e.g. 'AttackSweep').")]
        private string _animatorTrigger;

        [SerializeField, Tooltip("BoneColliderGroup name to activate during active frames. Leave empty for AoE attacks that use TriggerAoESlam.")]
        private string _boneGroupName;

        [SerializeField, Tooltip("Minimum distance to player for this attack to be eligible.")]
        private float _minRange;

        [SerializeField, Tooltip("Maximum distance to player for this attack to be eligible.")]
        private float _maxRange;

        [SerializeField, Tooltip("Seconds before this attack can be selected again.")]
        private float _cooldown = 3f;

        [SerializeField, Tooltip("Relative probability weight for weighted-random selection. Higher = more frequent.")]
        private float _weight = 1f;

        [SerializeField, Tooltip("Whether this attack is available during Phase 1.")]
        private bool _allowedInPhase1 = true;

        [SerializeField, Tooltip("Whether this attack is available during Phase 2.")]
        private bool _allowedInPhase2 = true;

        [SerializeField, Tooltip("Optional designer curve for hitbox timing relative to wind-up (informational, read by animation system).")]
        private AnimationCurve _windUpHitboxCurve;

        #endregion

        #region Public Properties

        public string AttackId => _attackId;
        public string AnimatorTrigger => _animatorTrigger;
        public string BoneGroupName => _boneGroupName;
        public float MinRange => _minRange;
        public float MaxRange => _maxRange;
        public float Cooldown => _cooldown;
        public float Weight => _weight;

        public bool IsAllowedInPhase(BossController.BossPhase phase)
        {
            if (phase == BossController.BossPhase.Phase1) return _allowedInPhase1;
            if (phase == BossController.BossPhase.Phase2) return _allowedInPhase2;
            return false;
        }

        #endregion
    }
}
