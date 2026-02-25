using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Boss-specific attack state. Plays the animation from
    /// BossCombatController.CurrentAttack and delegates branching
    /// to Animation Events.
    /// </summary>
    public sealed class BossAttackState : EnemyBaseState
    {
        private readonly BossCombatController _combat;

        public BossAttackState(EnemyController ctx, EnemyStateMachine sm, BossCombatController combat)
            : base(ctx, sm)
        {
            _combat = combat;
        }

        public override void EnterState()
        {
            if (_combat.CurrentAttack != null)
                _ctx.Animator.Play(_combat.CurrentAttack.AnimHash);
            else
                _ctx.Animator.Play("Attack");
        }

        public override void UpdateState()      { }
        public override void FixedUpdateState() { }
        public override void ExitState()        { }

        public override void CheckSwitchState()
        {
            // Transitions are driven by Animation Events
            // (OnRecoveryBranchEvent) and BossCombatController,
            // not by polling here.
        }
    }
}