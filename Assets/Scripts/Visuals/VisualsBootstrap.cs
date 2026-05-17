using UnityEngine;

namespace Longinus.Visuals
{
    public class VisualsBootstrap : MonoBehaviour
    {
        #region Unity Lifecycle

        private void Awake()
        {
            if (HitImpactPool.Instance == null)
            {
                var hitPool = new GameObject("HitImpactPool");
                hitPool.AddComponent<HitImpactPool>();
                DontDestroyOnLoad(hitPool);
            }
        }

        #endregion
    }
}
