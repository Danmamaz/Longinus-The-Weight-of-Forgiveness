using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class InteractionSystem : MonoBehaviour
{
    private List<IInteractable> _interactables = new List<IInteractable>();
    private int _currentItemIndex;
    public UnityEvent<List<IInteractable>> onInteractibleEnter;
    public UnityEvent<List<IInteractable>> onInteractibleLeave;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactableObj))
        {
            _interactables.Add(interactableObj);
            
            onInteractibleEnter?.Invoke(_interactables);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactableObj))
        {
            _interactables.Remove(interactableObj);
            
            onInteractibleLeave?.Invoke(_interactables);
        }
    }

    public void InteractWithSelectedObject()
    {
        if (_interactables.Count > 0)
        {
            _interactables[_currentItemIndex].Interact();
            
        }
    }
}
