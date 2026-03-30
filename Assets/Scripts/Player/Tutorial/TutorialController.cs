using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    private enum TutorialStep
    {
        Movement,
        Dodge,
        Attack,
        Completed
    }

    private TutorialStep currentStep = TutorialStep.Movement;

    private bool pressedW = false;
    private bool pressedA = false;
    private bool pressedS = false;
    private bool pressedD = false;

    [Header("UI Компоненти - WASD")]
    [SerializeField] private Image imageW;
    [SerializeField] private Image imageA;
    [SerializeField] private Image imageS;
    [SerializeField] private Image imageD;
    [SerializeField] private GameObject WASDPanel;

    [Header("Спрайти - WASD")]
    [SerializeField] private Sprite spriteW_Pressed;
    [SerializeField] private Sprite spriteA_Pressed;
    [SerializeField] private Sprite spriteS_Pressed;
    [SerializeField] private Sprite spriteD_Pressed;

    [Header("UI Компоненти - Shift")]
    [SerializeField] private Image imageShift;
    [SerializeField] private GameObject ShiftPanel;
    [SerializeField] private Sprite spriteShift_Pressed;

    [Header("UI Компоненти - ЛКМ")]
    [SerializeField] private Image imageLMB;
    [SerializeField] private GameObject LMBPanel;
    [SerializeField] private Sprite spriteLMB_Pressed;

    [Header("Налаштування анімації")]
    [SerializeField] private float animationDuration = 0.4f;

    private RectTransform wasdRect;
    private RectTransform shiftRect;
    private RectTransform lmbRect;

    private Vector2 startPosSlot1;
    private Vector2 startPosSlot2;
    private Vector2 startPosSlot3;

    // Константи для масштабу
    private readonly Vector3 activeScale = Vector3.one; // Звичайний розмір (1, 1, 1)
    private readonly Vector3 inactiveScale = new Vector3(0.33f, 0.33f, 1f); // В 3 рази менше

    private void Awake()
    {
        if (WASDPanel != null) wasdRect = WASDPanel.GetComponent<RectTransform>();
        if (ShiftPanel != null) shiftRect = ShiftPanel.GetComponent<RectTransform>();
        if (LMBPanel != null) lmbRect = LMBPanel.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (Keyboard.current == null) Debug.LogError("АХТУНГ: Рушій не бачить клавіатуру!");

        if (wasdRect != null) startPosSlot1 = wasdRect.anchoredPosition;
        if (shiftRect != null) startPosSlot2 = shiftRect.anchoredPosition;
        if (lmbRect != null) startPosSlot3 = lmbRect.anchoredPosition;

        // Встановлюємо початкові масштаби (WASD - великий, інші - маленькі)
        if (wasdRect != null) wasdRect.localScale = activeScale;
        if (shiftRect != null) shiftRect.localScale = inactiveScale;
        if (lmbRect != null) lmbRect.localScale = inactiveScale;

        if (WASDPanel != null) WASDPanel.SetActive(true);
        if (ShiftPanel != null) ShiftPanel.SetActive(true);
        if (LMBPanel != null) LMBPanel.SetActive(true);

        Debug.Log("ТУТОРІАЛ СТАРТ: Натисни W, A, S, D.");
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;
        if (currentStep == TutorialStep.Completed) return;

        CheckTutorialProgression();
    }

    private void CheckTutorialProgression()
    {
        switch (currentStep)
        {
            case TutorialStep.Movement:
                if (!pressedW && Keyboard.current.wKey.wasPressedThisFrame) { pressedW = true; if (imageW) imageW.sprite = spriteW_Pressed; }
                if (!pressedA && Keyboard.current.aKey.wasPressedThisFrame) { pressedA = true; if (imageA) imageA.sprite = spriteA_Pressed; }
                if (!pressedS && Keyboard.current.sKey.wasPressedThisFrame) { pressedS = true; if (imageS) imageS.sprite = spriteS_Pressed; }
                if (!pressedD && Keyboard.current.dKey.wasPressedThisFrame) { pressedD = true; if (imageD) imageD.sprite = spriteD_Pressed; }

                if (pressedW && pressedA && pressedS && pressedD)
                {
                    if (wasdRect != null) StartCoroutine(SlideOutRight(wasdRect));
                    
                    // Shift стає активним (їде в Слот 1 і збільшується)
                    if (shiftRect != null) StartCoroutine(SlideAndScaleToPosition(shiftRect, startPosSlot1, activeScale));
                    // ЛКМ просто їде вгору в Слот 2, залишаючись маленьким
                    if (lmbRect != null) StartCoroutine(SlideAndScaleToPosition(lmbRect, startPosSlot2, inactiveScale));

                    AdvanceStep(TutorialStep.Dodge, "ТУТОРІАЛ: Добре. Тепер натисни Лівий Shift.");
                }
                break;

            case TutorialStep.Dodge:
                if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
                {
                    if (imageShift != null) imageShift.sprite = spriteShift_Pressed;

                    if (shiftRect != null) StartCoroutine(SlideOutRight(shiftRect));
                    
                    // ЛКМ стає активним (їде в Слот 1 і збільшується)
                    if (lmbRect != null) StartCoroutine(SlideAndScaleToPosition(lmbRect, startPosSlot1, activeScale));

                    AdvanceStep(TutorialStep.Attack, "ТУТОРІАЛ: Чудово. Тепер натисни ЛКМ.");
                }
                break;

            case TutorialStep.Attack:
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (imageLMB != null) imageLMB.sprite = spriteLMB_Pressed;

                    if (lmbRect != null) StartCoroutine(SlideOutRight(lmbRect));

                    AdvanceStep(TutorialStep.Completed, "ТУТОРІАЛ ЗАВЕРШЕНО.");
                }
                break;
        }
    }

    private void AdvanceStep(TutorialStep nextStep, string message)
    {
        currentStep = nextStep;
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

        if (currentStep == TutorialStep.Completed && panel == lmbRect)
        {
            gameObject.SetActive(false);
        }
    }

    // --- ОНОВЛЕНА КОРУТИНА: ТЕПЕР МАНІПУЛЮЄ І ПОЗИЦІЄЮ, І МАСШТАБОМ ---
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
}