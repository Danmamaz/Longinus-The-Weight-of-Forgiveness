using UnityEngine;
using Longinus.InGameItems;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Relays animation events from the Animator to the EnemyController and its respective active states.
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public class EnemyAnimationEventRelay : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Tooltip("Explicit reference to the weapon's damage collider to avoid expensive runtime GetComponent calls.")]
        [SerializeField] private DamageCollider _weaponDamageCollider;
        
        #endregion

        #region Private Variables
        
        private EnemyController _controller;
        
        #endregion

        #region Unity Lifecycle
        
        /// <summary>
        /// Initializes required component references and caches the damage collider.
        /// </summary>
        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
            
            // Fallback caching to eliminate brute-force searches during animation execution
            if (_weaponDamageCollider == null)
            {
                _weaponDamageCollider = GetComponentInChildren<DamageCollider>();
                if (_weaponDamageCollider == null)
                {
                    Debug.LogWarning($"[EnemyAnimationEventRelay] No DamageCollider assigned or found on {gameObject.name}. Attack hitbox events will fail silently.");
                }
            }
        }
        
        #endregion

        #region Event Listeners/Callbacks
        
        /// <summary>
        /// Triggered via Animation Event when the attack wind-up phase ends.
        /// </summary>
        public void OnWindUpEnd()
        {
            (_controller.AttackState as EnemyAttackState)?.OnWindUpEnd();
        }

        /// <summary>
        /// Triggered via Animation Event when the active damage phase of an attack ends.
        /// </summary>
        public void OnActiveEnd()
        {
            (_controller.AttackState as EnemyAttackState)?.OnActiveEnd();
        }

        /// <summary>
        /// Triggered via Animation Event when the entire attack animation sequence is completely finished.
        /// </summary>
        public void OnAttackFinished()
        {
            (_controller.AttackState as EnemyAttackState)?.OnAttackFinished();
        }

        /// <summary>
        /// Triggered via Animation Event to activate the weapon's damage hitbox.
        /// </summary>
        public void EnableDamageCollider()
        {
            _weaponDamageCollider?.Enable();
        }

        /// <summary>
        /// Triggered via Animation Event to deactivate the weapon's damage hitbox.
        /// </summary>
        public void DisableDamageCollider()
        {
            _weaponDamageCollider?.Disable();
        }

        /// <summary>
        /// Triggered via Animation Event when the stagger recovery animation finishes.
        /// </summary>
        public void OnStaggerFinished()
        {
            (_controller.StaggeredState as EnemyStaggeredState)?.OnStaggerFinished();
        }
        
        #endregion
    }
}