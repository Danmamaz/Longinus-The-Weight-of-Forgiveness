using System.Text;
using UnityEngine;
using Longinus.Player;
using System.Collections;

namespace Longinus.UI
{
    /// <summary>
    /// Manages the core in-game user interface, including player stats, interactable prompts, and the pause menu.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("System References")]
        [SerializeField, Tooltip("Reference to the player's stats manager.")]
        private PlayerStatsManager _playerStats;
        
        [SerializeField, Tooltip("Reference to the player's interaction system.")]
        private InteractionSystem _interactionSystem;
        
        [Header("Pause Menu")]
        [SerializeField, Tooltip("The root game object of the pause menu UI.")]
        private GameObject _pauseMenu;

        [Header("Death UI")]
        [SerializeField, Tooltip("Root object of a death screen.")]
        private GameObject _deathScreen;

        #endregion

        #region Private Variables
        
        private bool _isPaused;
        private readonly StringBuilder _interactableStringBuilder = new StringBuilder();
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _isPaused = false;
            if (_pauseMenu != null) _pauseMenu.SetActive(false);
        }

        private void OnEnable()
        {
            if (_playerStats != null)
            {
                // Subscribing to the refactored standard C# Actions
                _playerStats.OnDeath += ShowDeathScreen;
                
            }
        }

        private void OnDisable()
        {
            if (_playerStats != null)
            {
                _playerStats.OnDeath -= ShowDeathScreen;
            }

            // Safety net: ensure time is unpaused if the UI object is destroyed during a scene transition
            Time.timeScale = 1f;
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Toggles the game pause state, managing the UI panel and time scale.
        /// </summary>
        /// <returns>True if the game is now paused, false otherwise.</returns>
        public bool TogglePauseMenu()
        {
            if (_pauseMenu == null) return false;
            
            _isPaused = !_isPaused;
            if (_isPaused)_pauseMenu.SetActive(true);
            else
            {
                Invoke("ExitPauseMenu", 3f);
                _pauseMenu.GetComponent<Animator>().SetTrigger("Exit");
            }
            Time.timeScale = _isPaused ? 0f : 1f;

            return _isPaused;
        }

        void ExitPauseMenu()
        {
            _pauseMenu.SetActive(false);
        }

        private void ShowDeathScreen()
        {
            if (_deathScreen != null)
            {
                StartCoroutine(Wait());
                _deathScreen.SetActive(true);
            }


            IEnumerator Wait()
            {
                yield return new WaitForSeconds(2f);
            }
        }

        #endregion
        
        #region Buttons

        /// <summary>
        /// Functionality of a resume button
        /// </summary>
        public void ResumeButton() {TogglePauseMenu();}

        #endregion
    }
}