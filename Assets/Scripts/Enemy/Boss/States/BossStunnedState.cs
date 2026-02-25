using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Entered when Poise is broken. Plays stun animation.
    /// Exit logic (timer / animation end) drives ResetPoise + transition.
    /// </summary>
    public sealed class BossStunnedState : EnemyBaseState
    {
        private readonly BossPoiseManager _poise;
        private readonly float _stunDuration;
        private float _timer;

        public BossStunnedState(EnemyController ctx, EnemyStateMachine sm,
                                BossPoiseManager poise, float stunDuration = 3f)
            : base(ctx, sm)
        {
            _poise        = poise;
            _stunDuration = stunDuration;
        }

        public override void EnterState()
        {
            _timer = 0f;
            _ctx.Animator.Play("Stunned");
        }

        public override void UpdateState()
        {
            _timer += Time.deltaTime;
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _poise.ResetPoise();
        }

        public override void CheckSwitchState()
        {
            if (_timer >= _stunDuration)
            {
                _stateMachine.ChangeState(_ctx.IdleState);
            }
        }
    }
}