using UnityEngine;
using Longinus.EnemySystem;

namespace Longinus.Visuals
{
    [RequireComponent(typeof(BossController))]
    public class BossPhasePostFX : MonoBehaviour
    {
        #region Unity Lifecycle

        private void OnEnable()
        {
            GetComponent<BossController>().OnPhaseChanged += OnPhase;
        }

        private void OnDisable()
        {
            GetComponent<BossController>().OnPhaseChanged -= OnPhase;
        }

        #endregion

        #region Event Listeners / Callbacks

        private void OnPhase(BossController.BossPhase phase)
        {
            if (PostProcessingDirector.Instance == null) return;

            switch (phase)
            {
                case BossController.BossPhase.Phase1:
                    PostProcessingDirector.Instance.TransitionTo(
                        PostProcessingDirector.PostProcessingMode.BossArenaPhase1);
                    break;

                case BossController.BossPhase.Phase2:
                    PostProcessingDirector.Instance.TransitionTo(
                        PostProcessingDirector.PostProcessingMode.BossArenaPhase2);
                    break;

                case BossController.BossPhase.Dead:
                    PostProcessingDirector.Instance.TransitionTo(
                        PostProcessingDirector.PostProcessingMode.PostBossKill);
                    break;
            }
        }

        #endregion
    }
}
