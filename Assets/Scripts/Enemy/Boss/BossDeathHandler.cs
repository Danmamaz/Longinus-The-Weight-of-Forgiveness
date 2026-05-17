using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Longinus.PlotSystem;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Observes boss phase transitions and fires the BR-01 plot branch when the boss dies in Phase 2.
    /// Handles corpse persistence and scene-reload dead state via <see cref="SpawnAsCorpse"/>.
    /// </summary>
    [RequireComponent(typeof(BossController))]
    public class BossDeathHandler : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Plot Wiring")]
        [SerializeField, Tooltip("ID of the PlotBranch to fire on boss death (must exist in the registry).")]
        private string _branchIdToFire = "BR-01";

        [Header("Phase Validation")]
        [SerializeField, Tooltip("When true, BR-01 only fires if the boss reached Phase 2 before dying.")]
        private bool _onlyFireIfPhase2Kill = true;

        [Header("Corpse Persistence")]
        [SerializeField, Tooltip("Optional prefab spawned in place of the boss on death. Leave empty to keep the current model.")]
        private GameObject _bossCorpsePrefab;

        [SerializeField, Tooltip("Disables all AI scripts but keeps the current mesh frozen on the last death animation frame.")]
        private bool _persistCurrentModel = true;

        [Header("Death Sequence Timing")]
        [SerializeField, Tooltip("Seconds to wait after the Dead phase is entered before firing the plot branch.")]
        private float _deathAnimDuration = 4f;

        #endregion

        #region Private Variables

        private BossController _boss;
        private bool _deathFired;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _boss = GetComponent<BossController>();
        }

        private void OnEnable()
        {
            _boss.OnPhaseChanged += HandlePhaseChange;
        }

        private void OnDisable()
        {
            _boss.OnPhaseChanged -= HandlePhaseChange;
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Called when scene loads and <c>Flag_BossDefeated</c> is already set.
        /// Jumps the death animation to its last frame and freezes the model.
        /// </summary>
        public void SpawnAsCorpse()
        {
            _deathFired = true;

            if (TryGetComponent(out Animator anim))
                anim.Play("Death", 0, 1f);

            PersistCorpse();
        }

        private IEnumerator DeathSequence()
        {
            yield return new WaitForSeconds(_deathAnimDuration);

            if (PlotManager.Instance != null)
            {
                bool fired = PlotManager.Instance.TryFireBranch(_branchIdToFire);
                if (!fired)
                    Debug.LogError($"[BossDeathHandler] Failed to fire branch {_branchIdToFire}. " +
                                   "Verify it exists in the PlotBranchRegistry and its conditions are met.");
            }

            if (_persistCurrentModel)
            {
                PersistCorpse();
            }
            else if (_bossCorpsePrefab != null)
            {
                Instantiate(_bossCorpsePrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }

        private void PersistCorpse()
        {
            if (TryGetComponent(out EnemyController ec)) ec.enabled = false;
            if (TryGetComponent(out EnemyMovementManager mm)) mm.enabled = false;
            if (TryGetComponent(out EnemyStatsManager esm)) esm.enabled = false;
            if (TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;

            // Freeze the Animator on its last frame — disabling preserves the current pose.
            if (TryGetComponent(out Animator animator)) animator.enabled = false;

            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                // Keep trigger colliders live for lore interaction points on the corpse.
                if (!c.isTrigger) c.enabled = false;
            }

            enabled = false;
        }

        #endregion

        #region Event Listeners/Callbacks

        private void HandlePhaseChange(BossController.BossPhase newPhase)
        {
            if (newPhase != BossController.BossPhase.Dead) return;
            if (_deathFired) return;

            if (_onlyFireIfPhase2Kill && !_boss.WasEverInPhase2)
            {
                Debug.LogWarning("[BossDeathHandler] Boss died before reaching Phase 2 — BR-01 not fired.");
                return;
            }

            _deathFired = true;
            StartCoroutine(DeathSequence());
        }

        #endregion
    }
}
