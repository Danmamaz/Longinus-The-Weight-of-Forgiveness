using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Longinus.Audio
{
    public class AudioDirector : MonoBehaviour
    {
        #region Constants & Inspector Variables

        private const float MUSIC_DEFAULT_VOLUME   = 0.6f;
        private const float SFX_DEFAULT_VOLUME     = 0.8f;
        private const float AMBIENT_DEFAULT_VOLUME = 0.4f;
        private const float MUSIC_FADE_DURATION    = 2f;

        public enum MusicTrack
        {
            None,
            MainMenu,
            Exploration,
            BossPhase1,
            BossPhase2,
            Victory,
            Death
        }

        [Header("Music Clips — drop once, never touch again")]
        [SerializeField] private AudioClip _mainMenuMusic;
        [SerializeField] private AudioClip _explorationMusic;
        [SerializeField] private AudioClip _bossPhase1Music;
        [SerializeField] private AudioClip _bossPhase2Music;
        [SerializeField] private AudioClip _victoryMusic;
        [SerializeField] private AudioClip _deathSting;

        [Header("Ambient")]
        [SerializeField] private AudioClip _outdoorAmbient;
        [SerializeField] private AudioClip _arenaAmbient;

        [Header("SFX")]
        [SerializeField] private AudioClip _swordSwing;
        [SerializeField] private AudioClip _swordHit;
        [SerializeField] private AudioClip _playerHurt;
        [SerializeField] private AudioClip _enemyHurt;
        [SerializeField] private AudioClip _checkpointActivate;
        [SerializeField] private AudioClip _footstep;
        [SerializeField] private AudioClip _menuClick;

        #endregion

        #region Private Variables

        private AudioSource _musicSource;
        private AudioSource _ambientSource;
        private AudioSource _sfxSource;
        private MusicTrack  _currentTrack;
        private Coroutine   _fadeMusicCoroutine;
        private Dictionary<MusicTrack, AudioClip> _trackMap;

        #endregion

        #region Public Properties

        public static AudioDirector Instance { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSource              = gameObject.AddComponent<AudioSource>();
            _musicSource.loop         = true;
            _musicSource.volume       = MUSIC_DEFAULT_VOLUME;
            _musicSource.playOnAwake  = false;
            _musicSource.spatialBlend = 0f;

            _ambientSource              = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop         = true;
            _ambientSource.volume       = AMBIENT_DEFAULT_VOLUME;
            _ambientSource.playOnAwake  = false;
            _ambientSource.spatialBlend = 0f;

            _sfxSource             = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop        = false;
            _sfxSource.volume      = SFX_DEFAULT_VOLUME;
            _sfxSource.playOnAwake = false;

            _trackMap = new Dictionary<MusicTrack, AudioClip>
            {
                [MusicTrack.MainMenu]    = _mainMenuMusic,
                [MusicTrack.Exploration] = _explorationMusic,
                [MusicTrack.BossPhase1]  = _bossPhase1Music,
                [MusicTrack.BossPhase2]  = _bossPhase2Music,
                [MusicTrack.Victory]     = _victoryMusic,
                [MusicTrack.Death]       = _deathSting
            };
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region State / Core Logic

        public void PlayMusic(MusicTrack track)
        {
            if (track == _currentTrack) return;
            if (!_trackMap.TryGetValue(track, out AudioClip clip) || clip == null) return;

            if (_fadeMusicCoroutine != null) StopCoroutine(_fadeMusicCoroutine);
            _currentTrack      = track;
            _fadeMusicCoroutine = StartCoroutine(FadeMusic(clip));
        }

        private IEnumerator FadeMusic(AudioClip newClip)
        {
            float halfDuration = MUSIC_FADE_DURATION / 2f;
            float startVolume  = _musicSource.volume;
            float t            = 0f;

            while (t < halfDuration)
            {
                t += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
                yield return null;
            }

            _musicSource.clip = newClip;
            _musicSource.Play();

            t = 0f;
            while (t < halfDuration)
            {
                t += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, MUSIC_DEFAULT_VOLUME, t / halfDuration);
                yield return null;
            }

            _musicSource.volume = MUSIC_DEFAULT_VOLUME;
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;
            if (_ambientSource.clip == clip) return;
            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, SFX_DEFAULT_VOLUME * volumeScale);
        }

        public void PlaySwordSwing()  => PlaySFX(_swordSwing);
        public void PlaySwordHit()    => PlaySFX(_swordHit);
        public void PlayPlayerHurt()  => PlaySFX(_playerHurt);
        public void PlayEnemyHurt()   => PlaySFX(_enemyHurt);
        public void PlayCheckpoint()  => PlaySFX(_checkpointActivate);
        public void PlayMenuClick()   => PlaySFX(_menuClick);
        public void PlayFootstep(float volumeScale = 0.6f) => PlaySFX(_footstep, volumeScale);

        #endregion

        #region Event Listeners / Callbacks

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case "Main Menu":
                    PlayMusic(MusicTrack.MainMenu);
                    break;

                case "Introduction Chapter":
                    PlayMusic(MusicTrack.Exploration);
                    PlayAmbient(_outdoorAmbient);
                    break;

                case "Beach":
                    PlayAmbient(_arenaAmbient);
                    break;
            }
        }

        #endregion
    }
}
