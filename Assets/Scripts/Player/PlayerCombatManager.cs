using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// Manages player combat actions, including attack execution, stamina consumption, and hitbox toggling.
    /// </summary>
    [RequireComponent(typeof(PlayerStatsManager))]
    public class PlayerCombatManager : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Combat Config")]
        [SerializeField, Tooltip("Reference to the player's weapon damage collider.")]
        private Collider _weaponCollider;

        [SerializeField, Tooltip("Stamina consumed when performing a light attack.")]
        private float _lightAttackStaminaCost = 10f;

        [SerializeField, Tooltip("Stamina consumed when performing a heavy attack.")]
        private float _heavyAttackStaminaCost = 20f;

        #endregion

        #region Private Variables

        private Animator _animator;
        private PlayerStatsManager _statsManager;

        // Cached animator hashes for performance optimization
        private readonly int _animLightAttackHash = Animator.StringToHash("LightAttack");
        private readonly int _animHeavyAttackHash = Animator.StringToHash("HeavyAttack");

        #endregion

        #region Public Properties

        public bool IsAttacking { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _statsManager = GetComponent<PlayerStatsManager>();
            
            if (_weaponCollider != null)
            {
                _weaponCollider.enabled = false;
                _weaponCollider.isTrigger = true;
            }
            else
            {
                Debug.LogWarning("[PlayerCombatManager] Weapon Collider is not assigned.");
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Attempts to execute an attack, checking and consuming stamina if successful.
        /// </summary>
        /// <param name="isHeavy">True for a heavy attack, false for a light attack.</param>
        /// <returns>True if the attack was successfully initiated, false if stamina was insufficient.</returns>
        public bool AttemptAttack(bool isHeavy)
        {
            float cost = isHeavy ? _heavyAttackStaminaCost : _lightAttackStaminaCost;

            if (!_statsManager.TryConsumeStamina(cost))
            {
                return false;
            }

            IsAttacking = true;
            _animator.SetTrigger(isHeavy ? _animHeavyAttackHash : _animLightAttackHash);
            
            return true;
        }

        #endregion

        #region Event Listeners/Callbacks

        /// <summary>
        /// Activates the weapon's damage hitbox. Usually triggered via Animation Events.
        /// </summary>
        public void OpenHitbox()
        {
            if (_weaponCollider != null) 
            {
                _weaponCollider.enabled = true;
            }
        }

        /// <summary>
        /// Deactivates the weapon's damage hitbox. Usually triggered via Animation Events.
        /// </summary>
        public void CloseHitbox()
        {
            if (_weaponCollider != null) 
            {
                _weaponCollider.enabled = false;
            }
        }

        /// <summary>
        /// Marks the end of the attack sequence. Usually triggered via Animation Events.
        /// </summary>
        public void EndAttack()
        {
            IsAttacking = false;
            CloseHitbox();
        }

        #endregion
    }
}