using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Manages NavMeshAgent-based movement, pathfinding, and target facing for enemies.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovementManager : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Movement Settings")]
        [SerializeField, Range(0f, 10f), Tooltip("Distance at which the agent stops moving towards the target.")] 
        private float _stoppingDistance = 2f;

        [SerializeField, Range(1f, 50f), Tooltip("Speed at which the enemy rotates to face the target manually.")]
        private float _rotationSpeed = 10f;

        #endregion

        #region Private Variables
        
        private NavMeshAgent _agent;
        private Transform _currentTarget;
        private bool _isRotationLocked;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes the NavMeshAgent and applies initial settings.
        /// </summary>
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = _stoppingDistance;
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Sets the primary target for the movement manager to track or chase.
        /// </summary>
        /// <param name="target">The transform of the target (e.g., Player).</param>
        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }

        /// <summary>
        /// Resumes movement and sets the destination to the current target's position.
        /// </summary>
        public void Chase()
        {
            if (_currentTarget == null || !_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(_currentTarget.position);
        }

        /// <summary>
        /// Halts current movement and clears the path.
        /// </summary>
        public void Stop()
        {
            if (!_agent.isOnNavMesh) return;
            
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        /// <summary>
        /// Moves the agent to a specific Transform's position.
        /// </summary>
        /// <param name="target">The destination Transform.</param>
        public void MoveTo(Transform target)
        {
            if (target == null || !_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(target.position);
        }

        /// <summary>
        /// Moves the agent to a specific coordinate in the world.
        /// </summary>
        /// <param name="targetPosition">The destination Vector3.</param>
        public void MoveToPosition(Vector3 targetPosition)
        {
            if (!_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(targetPosition);
        }

        /// <summary>
        /// Evaluates if the agent has reached its current path destination.
        /// </summary>
        /// <returns>True if the destination is reached, otherwise false.</returns>
        public bool ReachedDestination()
        {
            if (!_agent.isOnNavMesh || _agent.pathPending) return false;

            if (float.IsPositiveInfinity(_agent.remainingDistance)) return false;

            return _agent.remainingDistance <= _agent.stoppingDistance;
        }
        
        /// <summary>
        /// Prevents the NavMeshAgent from automatically updating its rotation.
        /// </summary>
        public void LockRotation()
        {
            _isRotationLocked = true;
            _agent.updateRotation = false;
        }

        /// <summary>
        /// Completely disables or enables the NavMeshAgent to prevent sliding during physics/root motion actions.
        /// </summary>
        public void SetAgentActive(bool isActive)
        {
            if (_agent != null && _agent.enabled != isActive)
            {
                _agent.enabled = isActive;
            }
        }

        /// <summary>
        /// Allows the NavMeshAgent to automatically update its rotation along the path.
        /// </summary>
        public void UnlockRotation()
        {
            _isRotationLocked = false;
            _agent.updateRotation = true;
        }

        /// <summary>
        /// Smoothly rotates the enemy to face the currently assigned target, ignoring the Y-axis.
        /// </summary>
        public void FaceTarget()
        {
            if (_currentTarget == null || _isRotationLocked) return;

            Vector3 direction = _currentTarget.position - transform.position;
            direction.y = 0f;

            // Prevent look rotation failure when vectors are perfectly aligned or zero length
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            }
        }

        #endregion
    }
}