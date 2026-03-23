using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Longinus.Interfaces;

namespace Longinus.Player
{
    /// <summary>
    /// Manages the detection and execution of interactable objects within the player's vicinity.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractionSystem : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Events")]
        [Tooltip("Triggered when the list of available interactables changes. Useful for updating UI prompts.")]
        public UnityEvent<List<IInteractable>> OnInteractablesChanged;
        
        #endregion

        #region Private Variables
        
        private readonly List<IInteractable> _interactablesInRange = new List<IInteractable>();
        
        #endregion

        #region Unity Lifecycle

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractable interactableObj))
            {
                if (!_interactablesInRange.Contains(interactableObj))
                {
                    _interactablesInRange.Add(interactableObj);
                    OnInteractablesChanged?.Invoke(_interactablesInRange);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IInteractable interactableObj))
            {
                if (_interactablesInRange.Remove(interactableObj))
                {
                    OnInteractablesChanged?.Invoke(_interactablesInRange);
                }
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Finds the closest valid interactable object and executes its interaction logic.
        /// </summary>
        public void InteractWithClosestObject()
        {
            RemoveNullReferences();

            if (_interactablesInRange.Count == 0) return;

            IInteractable closestInteractable = GetClosestInteractable();
            closestInteractable?.Interact();
            
            // Re-evaluate list in case the interacted object destroyed itself (e.g., item pickup)
            RemoveNullReferences();
        }

        /// <summary>
        /// Calculates the distance to all interactables in range and returns the closest one.
        /// </summary>
        private IInteractable GetClosestInteractable()
        {
            IInteractable closest = null;
            float minDistanceSqr = float.MaxValue;
            Vector3 currentPosition = transform.position;

            foreach (var interactable in _interactablesInRange)
            {
                // Cast to MonoBehaviour to get transform position safely
                if (interactable is MonoBehaviour mb)
                {
                    float distSqr = (mb.transform.position - currentPosition).sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        closest = interactable;
                    }
                }
            }

            return closest;
        }

        /// <summary>
        /// Cleans up the tracking list by removing destroyed or disabled interactables.
        /// </summary>
        private void RemoveNullReferences()
        {
            bool hasChanged = false;
            
            for (int i = _interactablesInRange.Count - 1; i >= 0; i--)
            {
                // If the object was destroyed (like a picked-up item), it will evaluate to null
                if (_interactablesInRange[i] == null || (_interactablesInRange[i] is MonoBehaviour mb && !mb.gameObject.activeInHierarchy))
                {
                    _interactablesInRange.RemoveAt(i);
                    hasChanged = true;
                }
            }

            if (hasChanged)
            {
                OnInteractablesChanged?.Invoke(_interactablesInRange);
            }
        }

        #endregion
    }
}