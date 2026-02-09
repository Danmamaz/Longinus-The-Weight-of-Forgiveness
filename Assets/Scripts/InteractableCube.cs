using UnityEngine;

public class InteractableCube : MonoBehaviour, IInteractable
{
    public string GetInteractionText()
    {
        return "A box";
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {GetInteractionText()}");
    }
}
