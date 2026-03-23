using UnityEngine;
using UnityEngine.UI;

namespace Longinus.UI
{
    /// <summary>
    /// Provides unscaled time to a UI Graphic's material, allowing shader animations to continue even when time is paused.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class UIUnscaledTimeProvider : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        // Cached statically so all instances share the same property ID without recalculating
        private static readonly int UnscaledTimeID = Shader.PropertyToID("_UnscaledTime");
        
        #endregion

        #region Private Variables
        
        private Material _materialInstance;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Graphic graphicComponent = GetComponent<Graphic>();
            
            if (graphicComponent != null && graphicComponent.material != null)
            {
                // Instantiate the material to prevent modifying the shared project asset
                _materialInstance = new Material(graphicComponent.material);
                graphicComponent.material = _materialInstance;
            }
        }

        private void Update()
        {
            if (_materialInstance != null)
            {
                _materialInstance.SetFloat(UnscaledTimeID, Time.unscaledTime);
            }
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                // Clean up the instantiated material to prevent memory leaks
                Destroy(_materialInstance);
            }
        }

        #endregion
    }
}