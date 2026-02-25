using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "NewBossAttack", menuName = "Combat/Boss Attack Data")]
    public sealed class BossAttackData : ScriptableObject
    {
        [field: SerializeField] public string       AttackID             { get; private set; }
        [field: SerializeField] public int           AnimHash             { get; private set; }
        [field: SerializeField] public float         Damage               { get; private set; }

        [Tooltip("Relative probability weight for weighted-random selection.")]
        [field: SerializeField, Min(0.01f)] public float Weight           { get; private set; } = 1f;

        [Tooltip("Cooldown in seconds before this attack can be selected again.")]
        [field: SerializeField, Min(0f)]    public float Cooldown         { get; private set; }

        [field: SerializeField] public DistanceBand  RequiredDistanceBand { get; private set; }

        [Tooltip("If true, this attack can chain into a combo on Recovery.")]
        [field: SerializeField] public bool          CanCombo             { get; private set; }

#if UNITY_EDITOR
        [ContextMenu("Generate AnimHash from AttackID")]
        private void BakeAnimHash() => AnimHash = Animator.StringToHash(AttackID);
#endif
    }
}