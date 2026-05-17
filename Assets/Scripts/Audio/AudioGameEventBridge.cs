using System.Collections;
using UnityEngine;
using Longinus.Player;
using Longinus.EnemySystem;

namespace Longinus.Audio
{
    public class AudioGameEventBridge : MonoBehaviour
    {
        #region Private Variables

        private BossController       _boss;
        private System.Action<float> _onPlayerDamage;
        private System.Action        _onPlayerDeath;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (PlayerController.Instance == null) return;

            PlayerStatsManager stats = PlayerController.Instance.Stats;

            _onPlayerDamage = _ => AudioDirector.Instance?.PlayPlayerHurt();
            _onPlayerDeath  = () => AudioDirector.Instance?.PlayMusic(AudioDirector.MusicTrack.Death);

            stats.OnDamage += _onPlayerDamage;
            stats.OnDeath  += _onPlayerDeath;
        }

        private void OnEnable()
        {
            StartCoroutine(LateSubscribeToBoss());
        }

        private void OnDisable()
        {
            if (_boss != null)
                _boss.OnPhaseChanged -= OnBossPhase;

            if (PlayerController.Instance != null)
            {
                PlayerStatsManager stats = PlayerController.Instance.Stats;
                if (_onPlayerDamage != null) stats.OnDamage -= _onPlayerDamage;
                if (_onPlayerDeath  != null) stats.OnDeath  -= _onPlayerDeath;
            }
        }

        #endregion

        #region State / Core Logic

        private IEnumerator LateSubscribeToBoss()
        {
            yield return null;

            _boss = FindObjectOfType<BossController>();
            if (_boss != null)
                _boss.OnPhaseChanged += OnBossPhase;
        }

        #endregion

        #region Event Listeners / Callbacks

        private void OnBossPhase(BossController.BossPhase phase)
        {
            switch (phase)
            {
                case BossController.BossPhase.Phase1:
                    AudioDirector.Instance?.PlayMusic(AudioDirector.MusicTrack.BossPhase1);
                    break;

                case BossController.BossPhase.Phase2:
                    AudioDirector.Instance?.PlayMusic(AudioDirector.MusicTrack.BossPhase2);
                    break;

                case BossController.BossPhase.Dead:
                    AudioDirector.Instance?.PlayMusic(AudioDirector.MusicTrack.Victory);
                    break;
            }
        }

        #endregion
    }
}
