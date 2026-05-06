using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Longinus.Player;
using Longinus.Save;

namespace Longinus.Systems
{
    /// <summary>
    /// Manages scene loading with fade transitions and wires player death to save-based reload.
    /// Acts as the authoritative source for the current scene's build index.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Transition References")]
        [SerializeField, Tooltip("White flash overlay shown at the start of a transition.")]
        private GameObject _white;

        [SerializeField, Tooltip("Black overlay held during the scene load.")]
        private GameObject _black;

        [Header("Settings")]
        [SerializeField, Tooltip("Seconds of fade held before the scene actually loads.")]
        private float _transitionDuration = 7f;

        #endregion

        #region Public Properties

        public static SceneController Instance { get; private set; }
        public int currentSceneIndex { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _white.SetActive(false);
            currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.Stats.OnDeath += LoadSavedGame;
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Loads the scene stored in the save file, or starts a new game if no save exists.
        /// </summary>
        public void LoadSavedGame()
        {
            if (!SaveSystem.HasSaveFile())
            {
                Debug.LogWarning("[SceneController] No save file found. Redirecting to New Game.");
                StartNewGame();
                return;
            }

            StartCoroutine(StartSceneLoading(SaveSystem.GetSavedSceneIndex()));
        }

        /// <summary>
        /// Loads scene index 1, the default first gameplay scene.
        /// </summary>
        public void LoadDefaultScene()
        {
            StartCoroutine(StartSceneLoading(1));
        }

        /// <summary>
        /// Wipes save data and starts a fresh playthrough from scene 1.
        /// </summary>
        public void StartNewGame()
        {
            SaveSystem.DeleteSaveData();
            StartCoroutine(StartSceneLoading(1));
        }

        private IEnumerator StartSceneLoading(int sceneIndex)
        {
            _white.SetActive(true);
            _black.SetActive(true);

            yield return new WaitForSeconds(_transitionDuration);
            SceneManager.LoadScene(sceneIndex);
        }

        #endregion
    }
}
