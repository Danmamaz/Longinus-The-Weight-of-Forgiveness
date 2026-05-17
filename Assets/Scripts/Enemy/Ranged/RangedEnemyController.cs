using UnityEngine;
using Longinus.InGameItems;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Extends EnemyController with ranged-combat awareness: shoot/kite range checks,
    /// a ProjectileLauncher reference, and registration of the two ranged states.
    /// Must live on the same GameObject as EnemyController.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public class RangedEnemyController : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Range Thresholds")]
        [SerializeField, Tooltip("Optimal engagement distance the enemy tries to maintain.")]
        private float _preferredRange = 8f;

        [SerializeField, Tooltip("Distance below which the enemy is considered too close and will kite away.")]
        private float _tooCloseRange = 4f;

        [SerializeField, Tooltip("Maximum distance at which the enemy can fire a projectile.")]
        private float _shootRange = 10f;

        [Header("Combat Timing")]
        [SerializeField, Tooltip("Seconds the enemy waits after firing before it can fire again.")]
        private float _shootCooldown = 2f;

        [Header("References")]
        [SerializeField, Tooltip("Launcher component responsible for spawning and firing projectiles.")]
        private ProjectileLauncher _launcher;

        #endregion

        #region Private Variables

        private float _sqrPreferredRange;
        private float _sqrTooCloseRange;
        private float _sqrShootRange;
        private EnemyController _controller;

        #endregion

        #region Public Properties

        public ProjectileLauncher Launcher => _launcher;
        public float ShootCooldown => _shootCooldown;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();

            _sqrPreferredRange = _preferredRange * _preferredRange;
            _sqrTooCloseRange = _tooCloseRange * _tooCloseRange;
            _sqrShootRange = _shootRange * _shootRange;
        }

        // State registration happens in Start so EnemyController.Awake() has already
        // built the state machine before we pass it into the new state constructors.
        private void Start()
        {
            var kiteState = new EnemyKiteState(_controller, _controller.StateMachine);
            var shootState = new EnemyShootState(_controller, _controller.StateMachine);
            _controller.RegisterRangedStates(kiteState, shootState);
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Returns true when the player has closed inside the minimum safe distance.
        /// </summary>
        public bool IsPlayerTooClose()
        {
            Transform player = _controller.PlayerTransform;
            if (player == null) return false;

            return (_controller.transform.position - player.position).sqrMagnitude <= _sqrTooCloseRange;
        }

        /// <summary>
        /// Returns true when the player is within projectile firing range.
        /// </summary>
        public bool IsPlayerInShootRange()
        {
            Transform player = _controller.PlayerTransform;
            if (player == null) return false;

            return (_controller.transform.position - player.position).sqrMagnitude <= _sqrShootRange;
        }

        #endregion
    }
}
