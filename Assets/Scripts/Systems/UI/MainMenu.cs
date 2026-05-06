using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Longinus.Save;
using Longinus.Systems;

namespace Longinus.UI
{
    /// <summary>
    /// Controls main menu intro animations and button callbacks for new game, continue, and load screen toggle.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("UI Elements")]
        [SerializeField] private RectTransform _logoRect;
        [SerializeField] private List<RectTransform> _buttonRects = new List<RectTransform>();

        [Header("Screens & Overlays")]
        [SerializeField] private GameObject _textToAppear;
        [SerializeField] private GameObject _white;
        [SerializeField] private GameObject _loadScreen;

        [Header("Animation Settings")]
        [SerializeField] private float _timeToAppear = 2f;
        [SerializeField] private float _moveDistance = 70f;
        [SerializeField] private float _moveDuration = 1f;

        #endregion

        #region Private Variables

        private bool _isLoadActive;

        #endregion

        #region Unity Lifecycle

        public void Start()
        {
            _textToAppear.SetActive(false);

            foreach (var btn in _buttonRects)
            {
                btn.gameObject.SetActive(false);
            }

            StartCoroutine(AppearText());
        }

        #endregion

        #region State/Core Logic

        private IEnumerator AppearText()
        {
            yield return new WaitForSeconds(_timeToAppear);
            _textToAppear.SetActive(true);
        }

        /// <summary>
        /// Reveals the main menu buttons with a slide-up animation. Called from an Animation Event.
        /// </summary>
        public void ShowButtons()
        {
            _white.SetActive(true);
            _textToAppear.SetActive(false);

            foreach (var buttonRect in _buttonRects)
            {
                buttonRect.gameObject.SetActive(true);
                StartCoroutine(MoveUIElementUp(buttonRect, _moveDistance, _moveDuration, null));
            }

            if (_logoRect != null)
            {
                Animator logoAnim = _logoRect.GetComponent<Animator>();
                StartCoroutine(MoveUIElementUp(_logoRect, _moveDistance, _moveDuration, logoAnim));
            }
        }

        private IEnumerator MoveUIElementUp(RectTransform rect, float distance, float duration, Animator animToDisable)
        {
            if (animToDisable != null) animToDisable.enabled = false;

            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, distance);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                t = t * t * (3f - 2f * t); // Smooth-step easing

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.anchoredPosition = endPos;

            if (animToDisable != null)
            {
                animToDisable.enabled = true;
                animToDisable.SetTrigger("Up");
            }
        }

        #endregion

        #region Buttons

        /// <summary>
        /// Toggles the load/continue screen panel.
        /// </summary>
        public void ToggleLoadMenu()
        {
            _isLoadActive = !_isLoadActive;
            _loadScreen.SetActive(_isLoadActive);
        }

        /// <summary>
        /// Wipes existing save data and starts a fresh playthrough.
        /// </summary>
        public void OnNewGameClicked()
        {
            SceneController.Instance.StartNewGame();
        }

        /// <summary>
        /// Loads the saved game if a save file exists.
        /// </summary>
        public void OnContinueClicked()
        {
            if (SaveSystem.HasSaveFile())
            {
                SceneController.Instance.LoadSavedGame();
            }
            else
            {
                Debug.Log("[MainMenu] No save file found.");
            }
        }

        #endregion
    }
}
