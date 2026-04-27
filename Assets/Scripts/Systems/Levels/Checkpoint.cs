using UnityEngine;
using Longinus.Player;
using Longinus.PlotSystem;
using Longinus.Save;
using Longinus.Interfaces;
using UnityEngine.SceneManagement;

namespace Longinus.InGameItems
{
    public class Checkpoint : MonoBehaviour, IInteractable
    {
        [Header("Checkpoint Settings")]
        [SerializeField, Tooltip("Position, where the player will rest")] 
        private Transform _spawnPoint;
        
        [SerializeField] private PlotState _plotStateRef;

        public void Interact()
        {
            PlayerController player = PlayerController.Instance;
            player.Stats.RestoreAll();

            player.Animator.SetTrigger("Rest");
            _spawnPoint.GetComponent<Animator>().SetTrigger("Activated");

            player.transform.position = _spawnPoint.position;
            player.transform.rotation = _spawnPoint.rotation;

            SaveSystem.SaveState(_plotStateRef, player.Stats, _spawnPoint.position, SceneManager.GetActiveScene().buildIndex);            
        }

        public string GetInteractionText()
        {
            return "Rest";
        }
    }
}