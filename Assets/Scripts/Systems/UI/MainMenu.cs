using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject _logo;
    [SerializeField] GameObject _white;
    [SerializeField] GameObject _black;
    [SerializeField] float _timeToAppear;
    [SerializeField] GameObject _textToAppear;

    [SerializeField] List<GameObject> buttons = new List<GameObject>();
    [SerializeField] Animator animator;

    public void Start()
    {
        _textToAppear.SetActive(false);
        _white.SetActive(false);
        StartCoroutine(AppearText());
    }

    private IEnumerator AppearText()
    {
        yield return new WaitForSeconds(_timeToAppear);
        _textToAppear.SetActive(true);
    }

    public void ShowButtons()
    {
        foreach (var button in buttons)
        {
            button.SetActive(true);
        }
        _white.SetActive(true);
        _textToAppear.SetActive(false);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(StartSceneLoading(sceneIndex));
    }

    private IEnumerator StartSceneLoading(int sceneIndex)
    {
        //Start animation
        _white.SetActive(false);
        _white.SetActive(true);
        _black.SetActive(true);
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(sceneIndex);

    }



}
