using UnityEngine;
using Enemy.BaseEnemy;
using InGameItems;

public class EnemyAnimationEventRelay : MonoBehaviour
{
    private EnemyController _controller;

    private void Awake() => _controller = GetComponent<EnemyController>();

    public void OnWindUpEnd() =>
        (_controller.AttackState as EnemyAttackState)?.OnWindUpEnd();

    public void OnActiveEnd() =>
        (_controller.AttackState as EnemyAttackState)?.OnActiveEnd();

    public void OnAttackFinished() =>
        (_controller.AttackState as EnemyAttackState)?.OnAttackFinished();

    public void EnableDamageCollider() =>
        _controller.GetComponentInChildren<DamageCollider>()?.Enable();

    public void DisableDamageCollider() =>
        _controller.GetComponentInChildren<DamageCollider>()?.Disable();

    public void OnStaggerFinished() =>
    (_controller.StaggeredState as EnemyStaggeredState)?.OnStaggerFinished();
}