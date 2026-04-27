using UnityEngine;
using UnityEngine.SceneManagement;
using Longinus.Save;
using System.Collections;
using Longinus.Player;

public class SceneController : MonoBehaviour
{
    [SerializeField] private GameObject _white;
    [SerializeField] private GameObject _black;
    public static SceneController Instance;
    public int currentSceneIndex { get; private set;}

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _white.SetActive(false);
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayerController.Instance.Stats.OnDeath += LoadSavedGame;
    }

    public void LoadSavedGame()
    {
        int sceneToLoad = SaveSystem.GetSavedSceneIndex();
        
        StartCoroutine(StartSceneLoading(sceneToLoad));
    }

    public void LoadDefaultScene()
    {
        StartCoroutine(StartSceneLoading(1));
    }


    private IEnumerator StartSceneLoading(int sceneIndex)
    {
        _white.SetActive(false);
        _white.SetActive(true); 
        _black.SetActive(true);
        
        yield return new WaitForSeconds(7);
        SceneManager.LoadScene(sceneIndex);
    }
    
}
