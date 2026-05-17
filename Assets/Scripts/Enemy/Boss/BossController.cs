using System;
using System.Collections;
using UnityEngine;
using Longinus.PlotSystem;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Orchestrates boss phase progression, arena sealing, and UI activation.
    /// Composes with <see cref="EnemyController"/> and <see cref="EnemyStatsManager"/> — does NOT inherit from them.
    /// </summary>
    /// <remarks>
    /// <b>Animator Parameter Contract</b> — the attached Animator Controller MUST expose these parameters:
    /// <para>
    /// <b>Triggers:</b><br/>
    ///   Engage           — fires on arena entry via <see cref="BossArenaTrigger"/><br/>
    ///   PhaseTransition  — fires when HP drops to or below 50 %<br/>
    ///   Die              — inherited from base enemy<br/>
    ///   Stagger          — inherited from base enemy<br/>
    ///   AttackSweep      — Phase 1 melee sweep<br/>
    ///   AttackThrust     — Phase 1 forward thrust<br/>
    ///   AttackAoESlam    — Phase 1 area-of-effect ground slam<br/>
    ///   AttackPhase2_A   — Phase 2 gap-closer leap<br/>
    ///   AttackPhase2_B   — Phase 2 spinning fury
    /// </para>
    /// <para>
    /// <b>Bools:</b><br/>
    ///   IsPhase2  — drives blend trees and attack selection after the phase transition finishes<br/>
    ///   IsMoving  — inherited from base enemy locomotion
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(EnemyController), typeof(EnemyStatsManager))]
    public class BossController : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Phase Config")]
        [SerializeField, Tooltip("HP fraction at or below which Phase 2 is triggered (0–1).")]
        private float _phaseTransitionHealthPercent = 0.5f;

        [Header("Arena")]
        [SerializeField, Tooltip("Centre of the boss arena. Used for leash checks and camera framing.")]
        private Transform _arenaCenter;

        [SerializeField, Tooltip("Radius of the boss arena in world units.")]
        private float _arenaRadius = 15f;

        [SerializeField, Tooltip("GameObject containing the arena wall colliders. Activated on engage, deactivated on death.")]
        private GameObject _arenaWalls;

        [Header("Boss UI")]
        [SerializeField, Tooltip("Root of the boss health bar HUD. Toggled on when the boss is engaged.")]
        private GameObject _bossHealthBarRoot;

        [Header("Phase 2 VFX")]
        [SerializeField, Tooltip("Particle system aura/flame VFX activated at the phase transition.")]
        private ParticleSystem _phase2VFX;

        [SerializeField, Tooltip("AudioSource used to play the phase-change roar.")]
        private AudioSource _phase2AudioSource;

        [SerializeField, Tooltip("Roar clip played once at the start of the phase transition.")]
        private AudioClip _phase2RoarClip;

        [SerializeField, Tooltip("Optional environmental light that shifts colour during the transition.")]
        private Light _phase2AreaLight;

        [SerializeField, Tooltip("Light colour during Phase 1 (used as lerp start).")]
        private Color _phase1LightColor = Color.white;

        [SerializeField, Tooltip("Light colour target for Phase 2.")]
        private Color _phase2LightColor = new Color(1f, 0.3f, 0.2f, 1f);

        #endregion

        #region Private Variables

        private EnemyController _controller;
        private EnemyStatsManager _stats;
        private BossAttackSelector _attackSelector;
        private BossAttackState _bossAttackState;
        private BossPhaseTransitionState _transitionState;
        private bool _phaseTransitionFired;
        private bool _wasEverInPhase2;

        #endregion

        #region Public Properties

        public enum BossPhase { Inactive, Phase1, Transitioning, Phase2, Dead }

        public BossPhase CurrentPhase { get; private set; } = BossPhase.Inactive;
        public bool WasEverInPhase2 => _wasEverInPhase2;
        public float ArenaRadius => _arenaRadius;
        public Vector3 ArenaCenter => _arenaCenter != null ? _arenaCenter.position : transform.position;

        #endregion

        #region Events

        public event Action<BossPhase> OnPhaseChanged;
        public event Action OnPhase2Triggered;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
            _stats = GetComponent<EnemyStatsManager>();
            _attackSelector = GetComponent<BossAttackSelector>();

            _bossAttackState = new BossAttackState(
                _controller, _controller.StateMachine, this, _attackSelector);
            _controller.OverrideAttackState(_bossAttackState);

            _transitionState = new BossPhaseTransitionState(
                _controller, _controller.StateMachine, this);
            _controller.RegisterPhaseTransitionState(_transitionState);

            if (_arenaWalls != null) _arenaWalls.SetActive(false);
            if (_bossHealthBarRoot != null) _bossHealthBarRoot.SetActive(false);
        }

        private void Start()
        {
            // On scene reload after the boss is already dead, skip engagement and show the corpse.
            if (PlotManager.Instance != null && PlotManager.Instance.CheckFlag("Flag_BossDefeated"))
            {
                GetComponent<BossDeathHandler>()?.SpawnAsCorpse();
            }
        }

        private void OnEnable()
        {
            _stats.OnDamageTaken += HandleDamage;
            _stats.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (_stats != null)
            {
                _stats.OnDamageTaken -= HandleDamage;
                _stats.OnDeath -= HandleDeath;
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Transitions the boss from Inactive to Phase 1. Called by <see cref="BossArenaTrigger"/> on player entry.
        /// </summary>
        public void EngageBoss()
        {
            if (CurrentPhase != BossPhase.Inactive) return;

            CurrentPhase = BossPhase.Phase1;

            if (_arenaWalls != null) _arenaWalls.SetActive(true);
            if (_bossHealthBarRoot != null) _bossHealthBarRoot.SetActive(true);

            _controller.Animator.SetTrigger("Engage");
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        /// <summary>
        /// Commits the phase 2 state after the transition animation finishes.
        /// Called by <see cref="OnTransitionFinished"/> once the animation event arrives.
        /// </summary>
        public void OnPhaseTransitionFinished()
        {
            CurrentPhase = BossPhase.Phase2;
            _wasEverInPhase2 = true;
            _controller.Animator.SetBool("IsPhase2", true);
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        /// <summary>
        /// Unified entry point called by <see cref="EnemyAnimationEventRelay"/> when the
        /// transition animation event fires. Signals the state and commits Phase 2.
        /// </summary>
        public void OnTransitionFinished()
        {
            _transitionState?.OnTransitionAnimationFinished();
            OnPhaseTransitionFinished();
        }

        /// <summary>
        /// Physically launches the boss toward the player in an arc. Called via animation event
        /// at the leap's apex frame so the boss appears to fly through the air.
        /// </summary>
        public void ExecuteLeapMovement(float duration = 0.5f)
        {
            StartCoroutine(LeapRoutine(duration));
        }

        private void TriggerPhase2()
        {
            _phaseTransitionFired = true;
            CurrentPhase = BossPhase.Transitioning;
            PlayPhase2Effects();
            _controller.StateMachine.ChangeState(_transitionState);
            OnPhase2Triggered?.Invoke();
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        private void PlayPhase2Effects()
        {
            if (_phase2VFX != null) _phase2VFX.Play();

            if (_phase2AudioSource != null && _phase2RoarClip != null)
                _phase2AudioSource.PlayOneShot(_phase2RoarClip);

            if (_phase2AreaLight != null)
                StartCoroutine(LerpLightColor(_phase1LightColor, _phase2LightColor, 1.5f));
        }

        private IEnumerator LerpLightColor(Color from, Color to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _phase2AreaLight.color = Color.Lerp(from, to, t / duration);
                yield return null;
            }
            _phase2AreaLight.color = to;
        }

        private IEnumerator LeapRoutine(float duration)
        {
            if (_controller.PlayerTransform == null) yield break;

            Vector3 start = transform.position;
            Vector3 target = _controller.PlayerTransform.position;
            target -= (target - start).normalized * 1.5f;

            _controller.MovementManager.SetAgentActive(false);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = t / duration;
                Vector3 pos = Vector3.Lerp(start, target, normalized);
                pos.y += Mathf.Sin(normalized * Mathf.PI) * 2.5f;
                transform.position = pos;
                yield return null;
            }

            _controller.MovementManager.SetAgentActive(true);
        }

        #endregion

        #region Event Listeners/Callbacks

        private void HandleDamage(float damage, float currentHealth)
        {
            if (_phaseTransitionFired) return;
            if (CurrentPhase != BossPhase.Phase1) return;

            float threshold = _stats.MaxHealth * _phaseTransitionHealthPercent;
            if (currentHealth <= threshold)
                TriggerPhase2();
        }

        private void HandleDeath()
        {
            CurrentPhase = BossPhase.Dead;
            if (_arenaWalls != null) _arenaWalls.SetActive(false);
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_arenaCenter == null) return;
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(_arenaCenter.position, _arenaRadius);
        }
#endif
    }
}
