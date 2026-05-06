using UnityEngine;
using Longinus.EnemySystem;

namespace Longinus.InGameItems
{
    /// <summary>
    /// Handles the player-facing kill-or-spare choice when a boss enters its spareable death phase.
    /// Opens the left door on kill, right door on spare. The choice is resolved either by the player
    /// landing another hit within the window, or by a 5-second mercy timer expiring.
    /// </summary>
    [RequireComponent(typeof(EnemyStatsManager))]
    public class DecisionInteractable : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Door Animators")]
        [SerializeField] private Animator _leftDoorAnimator;
        [SerializeField] private Animator _rightDoorAnimator;

        [Header("VFX")]
        [SerializeField] private GameObject _blueFlash;
        [SerializeField] private GameObject _redFlash;

        #endregion

        #region Private Variables

        private EnemyStatsManager _enemyStats;
        private bool _isWaitingForChoice;
        private Collider _collider;
        private int _originalLayer;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _enemyStats = GetComponent<EnemyStatsManager>();
            _collider = GetComponent<Collider>();
            _originalLayer = gameObject.layer;

            _enemyStats.OnSpareableDeath += StartChoicePhase;
            _enemyStats.OnDamageTaken += HandleFollowUpHit;
        }

        private void OnDestroy()
        {
            if (_enemyStats != null)
            {
                _enemyStats.OnSpareableDeath -= StartChoicePhase;
                _enemyStats.OnDamageTaken -= HandleFollowUpHit;
            }
        }

        #endregion

        #region State/Core Logic

        private void StartChoicePhase()
        {
            _isWaitingForChoice = true;

            Invoke(nameof(ExecuteSpareChoice), 5f);

            // The state machine disables the hitbox collider on death entry; re-enable it after one
            // frame so the player can still land a killing blow during the choice window.
            Invoke(nameof(ForceEnableHitbox), 0.1f);
        }

        private void ForceEnableHitbox()
        {
            if (_collider != null) _collider.enabled = true;
            gameObject.layer = _originalLayer;
            Debug.Log("[DecisionInteractable] Hitbox re-enabled. Choice window is open.");
        }

        private void HandleFollowUpHit(float damage, float currentHealth)
        {
            if (_isWaitingForChoice)
            {
                CancelInvoke(nameof(ExecuteSpareChoice));
                ExecuteKillChoice();
            }
        }

        private void ExecuteKillChoice()
        {
            _isWaitingForChoice = false;
            Debug.Log("[DecisionInteractable] Choice: KILL. Opening left door.");
            _redFlash.SetActive(true);
            _enemyStats.ExecuteFinalDeath();
            if (_leftDoorAnimator != null) _leftDoorAnimator.SetTrigger("Open");
        }

        private void ExecuteSpareChoice()
        {
            if (!_isWaitingForChoice) return;

            _isWaitingForChoice = false;
            Debug.Log("[DecisionInteractable] Timer expired. Choice: SPARE. Opening right door.");
            _blueFlash.SetActive(true);
            if (_rightDoorAnimator != null) _rightDoorAnimator.SetTrigger("Open");
        }

        #endregion
    }
}
