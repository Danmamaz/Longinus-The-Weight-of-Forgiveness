using UnityEngine;
using PlotBranching;

namespace InGameItems
{
public class DecisionInteractable : MonoBehaviour, IInteractable
{
    [Header("Configuration")]
    public DecisionNode decisionToTrigger;
    
    [SerializeField] DecisionHandler decisionHandler; 

    public void Interact()
    {
        Debug.Log(decisionToTrigger);
        Debug.Log(decisionHandler);
        if (decisionHandler != null && decisionToTrigger != null)
        {
            decisionHandler.PresentDecision(decisionToTrigger);
        }
        else
        {
            Debug.LogError("Missing DecisionHandler or DecisionNode assignment.");
        }
    }

    public string GetInteractionText()
    {
        return "Press E to Decide";
    }
}
}