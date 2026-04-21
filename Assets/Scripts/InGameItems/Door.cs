using UnityEngine;
using Longinus.PlotSystem;

public class Door : MonoBehaviour
{
    public string requiredFlagID; 

    private void Start()
    {
        if (PlotManager.Instance != null && PlotManager.Instance.CheckFlag(requiredFlagID))
        {
            OpenDoorInstantly();
            return;
        }

        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnFlagUpdated.AddListener(OnGlobalFlagUpdated);
        }
    }

    private void OnDestroy()
    {
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnFlagUpdated.RemoveListener(OnGlobalFlagUpdated);
        }
    }

    private void OnGlobalFlagUpdated(string updatedFlagID)
    {
        if (updatedFlagID == requiredFlagID)
        {
            OpenDoorWithAnimation();
        }
    }

    private void OpenDoorInstantly()
    {
        Debug.Log($"[Door] {name} is already open.");
    }

    private void OpenDoorWithAnimation()
    {
        Debug.Log($"[Door] {name} is opening now!");
    }
}