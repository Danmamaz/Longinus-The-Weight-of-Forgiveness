using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Longinus.Tutorial
{
    /// <summary>
    /// Drives the movement, dodge, and attack tutorial sequence.
    /// Advances through steps as the player performs required inputs, with animated UI panel transitions.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        private enum TutorialStep { Movement, Dodge, Attack, Completed }

        #region Constants & Inspector Variables

        [Header("WASD UI")]
        [SerializeField] private Image imageW;
        [SerializeField] private Image imageA;
        [SerializeField] private Image imageS;
        [SerializeField] private Image imageD;
        [SerializeField] private GameObject WASDPanel;

        [Header("WASD Pressed Sprites")]
        [SerializeField] private Sprite spriteW_Pressed;
        [SerializeField] private Sprite spriteA_Pressed;
        [SerializeField] private Sprite spriteS_Pressed;
        [SerializeField] private Sprite spriteD_Pressed;

        [Header("Dodge UI")]
        [SerializeField] private Image imageShift;
        [SerializeField] private GameObject ShiftPanel;
        [SerializeField] private Sprite spriteShift_Pressed;

        [Header("Attack UI")]
        [SerializeField] private Image imageLMB;
        [SerializeField] private GameObject LMBPanel;
        [SerializeField] private Sprite spriteLMB_Pressed;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.4f;

        #endregion

        #region Private Variables

        private TutorialStep _currentStep = TutorialStep.Movement;

        private bool _pressedW, _pressedA, _pressedS, _pressedD;

        private RectTransform _wasdRect;
        private RectTransform _shiftRect;
        private RectTransform _lmbRect;

        private Vector2 _startPosSlot1;
        private Vector2 _startPosSlot2;

        private readonly Vector3 _activeScale = Vector3.one;
        private readonly Vector3 _inactiveScale = new Vector3(0.33f, 0.33f, 1f);

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (WASDPanel != null) _wasdRect = WASDPanel.GetComponent<RectTransform>();
            if (ShiftPanel != null) _shiftRect = ShiftPanel.GetComponent<RectTransform>();
            if (LMBPanel != null) _lmbRect = LMBPanel.GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (Keyboard.current == null) Debug.LogError("[TutorialController] No keyboard detected!");

            if (_wasdRect != null) _startPosSlot1 = _wasdRect.anchoredPosition;
            if (_shiftRect != null) _startPosSlot2 = _shiftRect.anchoredPosition;

            if (_wasdRect != null) _wasdRect.localScale = _activeScale;
            if (_shiftRect != null) _shiftRect.localScale = _inactiveScale;
            if (_lmbRect != null) _lmbRect.localScale = _inactiveScale;

            if (WASDPanel != null) WASDPanel.SetActive(true);
            if (ShiftPanel != null) ShiftPanel.SetActive(true);
            if (LMBPanel != null) LMBPanel.SetActive(true);

            Debug.Log("[TutorialController] Start: Press W, A, S, D.");
        }

        private void Update()
        {
            if (Keyboard.current == null || Mouse.current == null) return;
            if (_currentStep == TutorialStep.Completed) return;

            CheckTutorialProgression();
        }

        #endregion

        #region State/Core Logic

        private void CheckTutorialProgression()
        {
            switch (_currentStep)
            {
                case TutorialStep.Movement:
                    if (!_pressedW && Keyboard.current.wKey.wasPressedThisFrame) { _pressedW = true; if (imageW) imageW.sprite = spriteW_Pressed; }
                    if (!_pressedA && Keyboard.current.aKey.wasPressedThisFrame) { _pressedA = true; if (imageA) imageA.sprite = spriteA_Pressed; }
                    if (!_pressedS && Keyboard.current.sKey.wasPressedThisFrame) { _pressedS = true; if (imageS) imageS.sprite = spriteS_Pressed; }
                    if (!_pressedD && Keyboard.current.dKey.wasPressedThisFrame) { _pressedD = true; if (imageD) imageD.sprite = spriteD_Pressed; }

                    if (_pressedW && _pressedA && _pressedS && _pressedD)
                    {
                        if (_wasdRect != null) StartCoroutine(SlideOutRight(_wasdRect));
                        if (_shiftRect != null) StartCoroutine(SlideAndScaleToPosition(_shiftRect, _startPosSlot1, _activeScale));
                        if (_lmbRect != null) StartCoroutine(SlideAndScaleToPosition(_lmbRect, _startPosSlot2, _inactiveScale));
                        AdvanceStep(TutorialStep.Dodge, "[TutorialController] Movement complete. Press Left Shift to dodge.");
                    }
                    break;

                case TutorialStep.Dodge:
                    if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
                    {
                        if (imageShift != null) imageShift.sprite = spriteShift_Pressed;
                        if (_shiftRect != null) StartCoroutine(SlideOutRight(_shiftRect));
                        if (_lmbRect != null) StartCoroutine(SlideAndScaleToPosition(_lmbRect, _startPosSlot1, _activeScale));
                        AdvanceStep(TutorialStep.Attack, "[TutorialController] Dodge complete. Press LMB to attack.");
                    }
                    break;

                case TutorialStep.Attack:
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        if (imageLMB != null) imageLMB.sprite = spriteLMB_Pressed;
                        if (_lmbRect != null) StartCoroutine(SlideOutRight(_lmbRect));
                        AdvanceStep(TutorialStep.Completed, "[TutorialController] Tutorial complete.");
                    }
                    break;
            }
        }

        private void AdvanceStep(TutorialStep nextStep, string message)
        {
            _currentStep = nextStep;
            Debug.Log(message);
        }

        private IEnumerator SlideOutRight(RectTransform panel)
        {
            Vector2 startPos = panel.anchoredPosition;
            Vector2 targetPos = new Vector2(startPos.x + 2000f, startPos.y);
            float timeElapsed = 0f;

            while (timeElapsed < animationDuration)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / animationDuration;
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                panel.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, easeOut);
                yield return null;
            }

            panel.anchoredPosition = targetPos;
            panel.gameObject.SetActive(false);

            if (_currentStep == TutorialStep.Completed && panel == _lmbRect)
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator SlideAndScaleToPosition(RectTransform panel, Vector2 targetPos, Vector3 targetScale)
        {
            Vector2 startPos = panel.anchoredPosition;
            Vector3 startScale = panel.localScale;
            float timeElapsed = 0f;

            while (timeElapsed < animationDuration)
            {
                timeElapsed += Time.deltaTime;
                float t = timeElapsed / animationDuration;
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                panel.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, easeOut);
                panel.localScale = Vector3.LerpUnclamped(startScale, targetScale, easeOut);
                yield return null;
            }

            panel.anchoredPosition = targetPos;
            panel.localScale = targetScale;
        }

        #endregion
    }
}
