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

        [SerializeField, Tooltip("Stamina consumed when performing an attack.")]
        private float _baseStaminaCost = 10f;

        [Header("Combo Data")]
        [SerializeField, Tooltip("Array of attacks, that formulates current combo")]
        private AttackDefinition[] _currentCombo;

        #endregion

        #region Private Variables

        private Animator _animator;
        private PlayerStatsManager _statsManager;

        private int _comboIndex = 0;
        private bool _canQueueNextAttack = false;
        private bool _nextInputReceived = false;

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
        /// Called whenever attack needs to be performed.
        /// </summary>
        public bool AttemptAttack()
        {
            if (_currentCombo == null || _currentCombo.Length == 0) return false;

            if (IsAttacking)
            {
                if (_canQueueNextAttack && _comboIndex < _currentCombo.Length - 1)
                {
                    _nextInputReceived = true;
                }
                return false; 
            }

            _comboIndex = 0;
            return ExecuteAttack(_comboIndex);
        }

        /// <summary>
        /// Physical start of an attack from the array.
        /// </summary>
        private bool ExecuteAttack(int index)
        {
            var attack = _currentCombo[index];
            float currentCost = _baseStaminaCost * attack.staminaMultiplier;

            if (!_statsManager.TryConsumeStamina(currentCost))
            {
                return false;
            }

            IsAttacking = true;
            _canQueueNextAttack = false;
            _nextInputReceived = false;
            
            _animator.CrossFade(attack.AnimationHash, 0.1f);
            
            return true;
        }

        #endregion

        #region Animation Events

        /// <summary>
        /// Called via Animation Event durring the attack.
        /// </summary>
        public void OpenComboWindow()
        {
            _canQueueNextAttack = true;
        }

        /// <summary>
        /// Called via Animation Event durring the moment, when the weapon is returning.
        /// </summary>
        public void CloseComboWindow()
        {
            _canQueueNextAttack = false;
        }

        /// <summary>
        /// If the player clicked - attack. If not - animation stops.
        /// </summary>
        public void TransitionToNextAttack()
        {
            if (_nextInputReceived && _comboIndex < _currentCombo.Length - 1)
            {
                _comboIndex++;
                ExecuteAttack(_comboIndex);
            }
        }

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