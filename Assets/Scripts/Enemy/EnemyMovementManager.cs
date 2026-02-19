using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementManager : MonoBehaviour
{
    private NavMeshAgent _agent;

    [SerializeField] private Transform playerTarget;

    [SerializeField] private float stoppingDistance = 2f;


    private void Awake()
    {
        AgentSetup();
    }

    public void AgentSetup()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = stoppingDistance;

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTarget = playerObj.transform;
        }
    }

    public void Chase()
    {
        if (playerTarget != null)
        {
            _agent.isStopped = false;
            _agent.SetDestination(playerTarget.position);
        }
    }

    public void Stop()
    {
        _agent.isStopped = true;
    }

    public void MoveTo(Transform target)
    {
        if (target != null)
        {
            _agent.isStopped = false;
            _agent.SetDestination(target.position);
        }
    }

    public bool ReachedDestination() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;

}
