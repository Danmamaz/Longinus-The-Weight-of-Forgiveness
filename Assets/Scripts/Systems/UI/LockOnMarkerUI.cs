using UnityEngine;

namespace Longinus.UI
{
    /// <summary>
    /// Screen-space UI marker that tracks a locked-on world target.
    /// Requires a Screen Space - Overlay canvas assigned to _canvas.
    /// </summary>
    public class LockOnMarkerUI : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("RectTransform of the marker image to reposition each frame.")]
        private RectTransform _markerRect;

        [SerializeField, Tooltip("Screen Space - Overlay canvas that owns this marker.")]
        private Canvas _canvas;

        #endregion

        #region Private Variables

        private Camera _cam;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Provides the camera used for world-to-screen projection. Must be called immediately after instantiation.
        /// </summary>
        public void Initialize(Camera cam)
        {
            _cam = cam;
        }

        /// <summary>
        /// Repositions the marker over the target's world position. Hides the marker when the target is behind the camera.
        /// </summary>
        public void UpdatePosition(Transform worldTarget)
        {
            Vector3 screenPos = _cam.WorldToScreenPoint(worldTarget.position + Vector3.up * 1.8f);

            if (screenPos.z < 0f)
            {
                Hide();
                return;
            }

            _markerRect.position = screenPos;
            Show();
        }

        /// <summary>
        /// Makes the marker visible.
        /// </summary>
        public void Show() => gameObject.SetActive(true);

        /// <summary>
        /// Hides the marker without destroying it.
        /// </summary>
        public void Hide() => gameObject.SetActive(false);

        #endregion
    }
}
