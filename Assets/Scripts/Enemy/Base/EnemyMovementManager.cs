using UnityEngine;
using UnityEngine.AI;

namespace Enemy.BaseEnemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovementManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Range(0f, 10f)] private float stoppingDistance = 2f;

        private NavMeshAgent _agent;
        private Transform _currentTarget;
        private bool _rotationLocked;

#region Unity Lifecycle

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = stoppingDistance;
        }

#endregion

#region Movement
        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }

        public void Chase()
        {
            if (_currentTarget == null || !_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(_currentTarget.position);
        }

        public void Stop()
        {
            if (!_agent.isOnNavMesh) return;
            
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        public void MoveTo(Transform target)
        {
            if (target == null || !_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(target.position);
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            if (!_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(targetPosition);
        }

        public bool ReachedDestination()
        {
            if (!_agent.isOnNavMesh || _agent.pathPending) return false;

            if (float.IsPositiveInfinity(_agent.remainingDistance)) return false;

            return _agent.remainingDistance <= _agent.stoppingDistance;
        }
    
#endregion

#region Rotation

    public void LockRotation()
    {
        _rotationLocked = true;
        _agent.updateRotation = false;
    }

    public void UnlockRotation()
    {
        _rotationLocked = false;
        _agent.updateRotation = true;
    }

    public void FaceTarget()
    {
        if (_currentTarget == null || _rotationLocked) return;

        Vector3 dir = (_currentTarget.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

#endregion
    }
}