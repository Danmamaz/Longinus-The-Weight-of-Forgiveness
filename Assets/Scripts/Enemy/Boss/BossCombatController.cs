using System.Collections;
using System.Collections.Generic;
using PlotBranching;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(BossPoiseManager))]
    public sealed class BossCombatController : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────
        [Header("Lunge (Heal-Punish)")]
        [Tooltip("Reaction delay before lunge (seconds).")]
        [SerializeField, Min(0f)] private float _reactionDelay = 0.25f;

        [Header("Distance Thresholds")]
        [SerializeField] private float _meleeRange = 2f;
        [SerializeField] private float _closeRange = 5f;
        [SerializeField] private float _midRange   = 10f;

        [Header("Combo")]
        [Tooltip("Dot-product threshold (forward · toPlayer). 0 = 90°, 1 = exact forward.")]
        [SerializeField, Range(-1f, 1f)] private float _comboDotThreshold = 0.4f;

        [Header("Stun")]
        [SerializeField, Min(0.1f)] private float _stunDuration = 3f;

        [Header("Target")]
        [SerializeField] private Transform _player;

        [Header("Boss Data")]
        [SerializeField] private BossData bossData;

        // ── Data from BossData (set via Initialize) ────────────
        private BossAttackData[] _attacks;
        private BossAttackData   _lungeAttack;
        private bool             _initialized;

        // ── Cached Components ──────────────────────────────────
        private EnemyController       _enemyController;
        private EnemyStateMachine     _stateMachine;
        private BossPoiseManager      _poiseManager;
        private DistanceBandEvaluator _bandEvaluator;

        // ── Boss-specific States ───────────────────────────────
        private BossAttackState  _bossAttackState;
        private BossStunnedState _bossStunnedState;

        // ── Runtime ────────────────────────────────────────────
        private readonly Dictionary<string, float>  _cooldownTimestamps = new();
        private readonly List<BossAttackData>       _candidateBuffer    = new();
        private Coroutine _healReactionCo;

        public BossAttackData    CurrentAttack    { get; private set; }
        public DistanceBand      CurrentBand      { get; private set; }
        public BossAttackState   BossAttackState  => _bossAttackState;
        public BossStunnedState  BossStunnedState => _bossStunnedState;

        // ════════════════════════════════════════════════════════
        //  INITIALIZATION
        // ════════════════════════════════════════════════════════

        /// <summary>
        /// Injects combat data from a BossData ScriptableObject.
        /// Must be called before the first Update tick (e.g. by a spawner).
        /// </summary>
        public void Initialize(PlotBranching.BossData bossData)
        {
            if (bossData == null)
            {
                Debug.LogError($"[BossCombatController] Initialize called with null BossData on '{name}'.");
                return;
            }

            _attacks     = bossData.attacks;
            _lungeAttack = bossData.lungeAttack;
            _initialized = true;

            if (_attacks == null || _attacks.Length == 0)
                Debug.LogWarning($"[BossCombatController] BossData '{bossData.bossName}' has no attacks assigned.");
        }

        // ── Unity Lifecycle ────────────────────────────────────
        private void Start()
        {
            _enemyController = GetComponent<EnemyController>();
            _poiseManager    = GetComponent<BossPoiseManager>();
            _bandEvaluator   = new DistanceBandEvaluator(_meleeRange, _closeRange, _midRange);

            _bossAttackState  = new BossAttackState(_enemyController, _stateMachine, this);
            _bossStunnedState = new BossStunnedState(_enemyController, _stateMachine,
                                                     _poiseManager, _stunDuration);
            
            Initialize(bossData);
        }

        private void OnEnable()  => PlayerEvents.OnHealStarted += OnPlayerHealStarted;
        private void OnDisable() => PlayerEvents.OnHealStarted -= OnPlayerHealStarted;

        private void Update()
        {
            if (_player == null) return;
            CurrentBand = _bandEvaluator.Evaluate(transform.position, _player.position);
        }

        // ════════════════════════════════════════════════════════
        //  1. ATTACK SELECTION
        // ════════════════════════════════════════════════════════
        public BossAttackData SelectNextAttack()
        {
            if (!_initialized || _attacks == null || _attacks.Length == 0)
                return null;

            BuildCandidateList();

            if (_candidateBuffer.Count == 0) return null;

            BossAttackData chosen = WeightedRandom(_candidateBuffer);
            RegisterCooldown(chosen);
            CurrentAttack = chosen;
            return chosen;
        }


        // ════════════════════════════════════════════════════════
        //  2. INPUT READING — Heal Punish
        // ════════════════════════════════════════════════════════
        private void OnPlayerHealStarted()
        {
            if (_stateMachine == null) return;
            if (!(_stateMachine.CurrentState is EnemyIdleState)) return;
            if (_lungeAttack == null) return;
            if (_healReactionCo != null) return;

            _healReactionCo = StartCoroutine(HealPunishRoutine());
        }

        private IEnumerator HealPunishRoutine()
        {
            yield return new WaitForSeconds(_reactionDelay);

            if (_stateMachine.CurrentState is EnemyIdleState)
            {
                CurrentAttack = _lungeAttack;
                RegisterCooldown(_lungeAttack);
                _stateMachine.ChangeState(_bossAttackState);
            }

            _healReactionCo = null;
        }

        // ════════════════════════════════════════════════════════
        //  3. MID-ACTION BRANCHING — Dynamic Combos
        // ════════════════════════════════════════════════════════
        public void OnRecoveryBranchEvent()
        {
            ComboBranchResult result = EvaluateComboBranching();

            switch (result)
            {
                case ComboBranchResult.Continue:
                    BossAttackData next = SelectNextAttack();
                    if (next != null)
                        _stateMachine.ChangeState(_bossAttackState);
                    break;

                case ComboBranchResult.Cancel:
                    CurrentAttack = null;
                    _stateMachine.ChangeState(_enemyController.IdleState);
                    break;
            }
        }

        public ComboBranchResult EvaluateComboBranching()
        {
            if (_player == null) return ComboBranchResult.Cancel;

            if (CurrentAttack == null || !CurrentAttack.CanCombo)
                return ComboBranchResult.Cancel;

            Vector3 toPlayer = _player.position - transform.position;
            float   sqrDist  = toPlayer.sqrMagnitude;

            if (sqrDist > _meleeRange * _meleeRange)
                return ComboBranchResult.Cancel;

            float dot = Vector3.Dot(transform.forward, toPlayer.normalized);
            if (dot < _comboDotThreshold)
                return ComboBranchResult.Cancel;

            return ComboBranchResult.Continue;
        }

        // ════════════════════════════════════════════════════════
        //  INTERNALS
        // ════════════════════════════════════════════════════════
        private void BuildCandidateList()
        {
            _candidateBuffer.Clear();
            float time = Time.time;

            for (int i = 0, len = _attacks.Length; i < len; i++)
            {
                BossAttackData atk = _attacks[i];

                if (atk.RequiredDistanceBand != CurrentBand) continue;

                if (_cooldownTimestamps.TryGetValue(atk.AttackID, out float readyAt) && time < readyAt)
                    continue;

                _candidateBuffer.Add(atk);
            }
        }

        private static BossAttackData WeightedRandom(List<BossAttackData> pool)
        {
            float totalWeight = 0f;
            for (int i = 0, len = pool.Count; i < len; i++)
                totalWeight += pool[i].Weight;

            float roll = Random.value * totalWeight;
            float cumulative = 0f;

            for (int i = 0, len = pool.Count; i < len; i++)
            {
                cumulative += pool[i].Weight;
                if (roll <= cumulative) return pool[i];
            }

            return pool[pool.Count - 1];
        }

        private void RegisterCooldown(BossAttackData atk)
        {
            if (atk.Cooldown > 0f)
                _cooldownTimestamps[atk.AttackID] = Time.time + atk.Cooldown;
        }
    }
}