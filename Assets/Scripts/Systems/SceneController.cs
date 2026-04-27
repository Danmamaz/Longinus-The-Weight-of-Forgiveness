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
        if (PlayerController.Instance != null)
        PlayerController.Instance.Stats.OnDeath += LoadSavedGame;
    }

    public void LoadSavedGame()
    {
        if (!SaveSystem.HasSaveFile())
        {
            Debug.LogWarning("[SceneController] No save file found. Redirecting to New Game.");
            StartNewGame();
            return;
        }
        
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

    public void StartNewGame()
    {
        SaveSystem.DeleteSaveData(); 

        StartCoroutine(StartSceneLoading(1));
    }    
}
