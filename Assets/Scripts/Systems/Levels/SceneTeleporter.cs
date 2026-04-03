using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{

    [SerializeField] private GameObject _fade;
    void OnTriggerEnter(Collider collider)
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        _fade.SetActive(true);
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(2);

    }
}
