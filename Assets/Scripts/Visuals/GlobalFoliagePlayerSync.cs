using UnityEngine;
using Longinus.Player;

namespace Longinus.Visuals
{
    public class GlobalFoliagePlayerSync : MonoBehaviour
    {
        #region Constants & Inspector Variables

        // Must match the global property declaration in FoliageBend.shader (outside CBUFFER)
        private static readonly int PlayerWorldPosID = Shader.PropertyToID("_PlayerWorldPos");

        #endregion

        #region Unity Lifecycle

        private void LateUpdate()
        {
            if (PlayerController.Instance == null) return;

            Vector3 pos = PlayerController.Instance.transform.position;
            Shader.SetGlobalVector(PlayerWorldPosID, pos);
        }

        #endregion
    }
}
