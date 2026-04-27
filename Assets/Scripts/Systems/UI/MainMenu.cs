using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Longinus.Save;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements (Assign RectTransforms!)")]
    [SerializeField] private RectTransform _logoRect;
    [SerializeField] private List<RectTransform> _buttonRects = new List<RectTransform>();
    
    [Header("Screens & Text")]
    [SerializeField] private GameObject _textToAppear;
    [SerializeField] private GameObject _white;
    [SerializeField] private GameObject _loadScreen;
    
    [Header("Settings")]
    [SerializeField] private float _timeToAppear = 2f;
    [SerializeField] private float _moveDistance = 70f;
    [SerializeField] private float _moveDuration = 1f;

    private bool _isLoadActive = false;

    public void Start()
    {
        _textToAppear.SetActive(false);
        
        foreach (var btn in _buttonRects)
        {
            btn.gameObject.SetActive(false);
        }
        
        StartCoroutine(AppearText());
    }

    private IEnumerator AppearText()
    {
        yield return new WaitForSeconds(_timeToAppear);
        _textToAppear.SetActive(true);
    }

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
        if (animToDisable != null)
        {
            animToDisable.enabled = false;
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, distance);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); 

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

    public void ToggleLoadMenu()
    {
        _isLoadActive = !_isLoadActive;
        _loadScreen.SetActive(_isLoadActive);
    }

    public void OnNewGameClicked()
    {
        SceneController.Instance.StartNewGame();
    }

    public void OnContinueClicked()
    {
        if (SaveSystem.HasSaveFile())
        {
            SceneController.Instance.LoadSavedGame();
        }
        else
        {
            Debug.Log("Немає збережень!");
        }
    }

}