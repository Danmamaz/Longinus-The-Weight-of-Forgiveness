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

        [Tooltip("All BoneColliderGroup components on this boss's skeleton, indexed by group name.")]
        [SerializeField] private BoneColliderGroup[] _boneGroups;

        [Tooltip("AoE slam hitbox component for ground-slam attacks. Boss only.")]
        [SerializeField] private AoESlamHitbox _aoeSlamHitbox;

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

        /// <summary>
        /// Triggered via Animation Event on the shoot animation's fire frame.
        /// </summary>
        public void OnFireProjectile()
        {
            (_controller.ShootState as EnemyShootState)?.OnFireProjectile();
        }

        /// <summary>
        /// Triggered via Animation Event when the boss phase-transition animation finishes.
        /// Routes through <see cref="BossController.OnTransitionFinished"/> so both the state
        /// machine and the phase commit happen in one call.
        /// </summary>
        public void OnPhaseTransitionFinished()
        {
            GetComponent<BossController>()?.OnTransitionFinished();
        }

        /// <summary>
        /// Alias for <see cref="OnPhaseTransitionFinished"/>. Use whichever name is clearer
        /// on the animation event in the Animator window.
        /// </summary>
        public void OnBossTransitionFinished()
        {
            GetComponent<BossController>()?.OnTransitionFinished();
        }

        /// <summary>
        /// Triggered via Animation Event at the apex of the PhaseLeap animation.
        /// Initiates the arc-movement coroutine on the boss.
        /// </summary>
        public void OnPhaseLeapStart()
        {
            GetComponent<BossController>()?.ExecuteLeapMovement();
        }

        /// <summary>
        /// Triggered via Animation Event to activate a named bone collider group.
        /// </summary>
        public void EnableBoneGroup(string groupName)
        {
            foreach (var group in _boneGroups)
            {
                if (group != null && group.GroupName == groupName)
                {
                    group.Enable();
                    return;
                }
            }
        }

        /// <summary>
        /// Triggered via Animation Event to deactivate a named bone collider group.
        /// </summary>
        public void DisableBoneGroup(string groupName)
        {
            foreach (var group in _boneGroups)
            {
                if (group != null && group.GroupName == groupName)
                {
                    group.Disable();
                    return;
                }
            }
        }

        /// <summary>
        /// Triggered via Animation Event to deactivate all bone collider groups at once
        /// (e.g., on attack interrupt or animation exit).
        /// </summary>
        public void DisableAllBoneGroups()
        {
            foreach (var group in _boneGroups)
            {
                if (group != null) group.Disable();
            }
        }

        /// <summary>
        /// Triggered via Animation Event on the AoE Slam's impact frame.
        /// </summary>
        public void TriggerAoESlam()
        {
            _aoeSlamHitbox?.TriggerSlam();
        }

        /// <summary>
        /// Triggered via Animation Event when a boss attack animation fully completes.
        /// Also disables all bone groups to prevent stuck-open hitboxes.
        /// </summary>
        public void OnBossAttackFinished()
        {
            DisableAllBoneGroups();
            (_controller.AttackState as BossAttackState)?.OnAttackFinished();
        }

        #endregion
    }
}