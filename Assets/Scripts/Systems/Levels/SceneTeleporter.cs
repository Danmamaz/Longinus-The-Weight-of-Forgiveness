using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Longinus.Levels
{
    /// <summary>
    /// Triggers a scene transition when any collider enters the attached trigger volume.
    /// Activates a fade overlay and waits before loading the target scene.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SceneTeleporter : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("References")]
        [SerializeField, Tooltip("Fade overlay activated before the scene loads.")]
        private GameObject _fade;

        [Header("Settings")]
        [SerializeField, Tooltip("Build index of the scene to load.")]
        private int _targetSceneIndex = 2;

        [SerializeField, Tooltip("Seconds between fade activation and scene load.")]
        private float _fadeDelay = 3f;

        #endregion

        #region Unity Lifecycle

        private void OnTriggerEnter(Collider collider)
        {
            StartCoroutine(LoadScene());
        }

        #endregion

        #region State/Core Logic

        private IEnumerator LoadScene()
        {
            if (_fade != null) _fade.SetActive(true);
            yield return new WaitForSeconds(_fadeDelay);
            SceneManager.LoadScene(_targetSceneIndex);
        }

        #endregion
    }
}
