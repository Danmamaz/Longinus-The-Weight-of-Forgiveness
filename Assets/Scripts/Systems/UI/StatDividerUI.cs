using System.Collections.Generic;
using UnityEngine;

namespace Longinus.UI
{
    /// <summary>
    /// Spawns and positions visual divider marks along a stat bar at regular stat-value intervals.
    /// Uses an object pool to avoid runtime allocation when max values change.
    /// </summary>
    public class StatDividerUI : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Settings")]
        [SerializeField, Tooltip("Stat value interval at which a divider mark is placed (e.g. 50 for HP, 25 for stamina).")]
        private float statInterval = 25f;

        [Header("References")]
        [SerializeField, Tooltip("RectTransform of the stat bar, used to measure its physical pixel width.")]
        private RectTransform barRect;

        [SerializeField, Tooltip("Divider prefab (an Image with the divider sprite).")]
        private GameObject dividerPrefab;

        [SerializeField, Tooltip("Container for divider instances. Pivot should be X:0, Y:0.5.")]
        private RectTransform dividerContainer;

        #endregion

        #region Private Variables

        private readonly List<GameObject> _activeDividers = new List<GameObject>();
        private readonly Queue<GameObject> _dividerPool = new Queue<GameObject>();

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Rebuilds all divider marks to match the new maximum stat value.
        /// Call only when the maximum stat value changes, not every frame.
        /// </summary>
        public void UpdateDividers(float maxStat)
        {
            if (maxStat <= 0 || statInterval <= 0) return;

            int requiredDividers = Mathf.FloorToInt((maxStat - 0.1f) / statInterval);
            float barWidth = barRect.rect.width;

            foreach (var divider in _activeDividers)
            {
                divider.SetActive(false);
                _dividerPool.Enqueue(divider);
            }
            _activeDividers.Clear();

            for (int i = 1; i <= requiredDividers; i++)
            {
                GameObject divObj = GetDivider();
                RectTransform divRect = divObj.GetComponent<RectTransform>();
                float normalizedPos = (i * statInterval) / maxStat;
                divRect.anchoredPosition = new Vector2(normalizedPos * barWidth, 0f);
            }
        }

        private GameObject GetDivider()
        {
            GameObject divObj = _dividerPool.Count > 0
                ? _dividerPool.Dequeue()
                : Instantiate(dividerPrefab, dividerContainer);

            divObj.SetActive(true);
            _activeDividers.Add(divObj);
            return divObj;
        }

        #endregion
    }
}
