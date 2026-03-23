using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.InGameItems
{
    /// <summary>
    /// A simple target dummy used strictly for testing combat interactions and damage registration.
    /// </summary>
    public class TrainingDummy : MonoBehaviour, IDamageable
    {
        #region Event Listeners/Callbacks
        
        /// <summary>
        /// Logs incoming damage and poise damage to the console for testing purposes.
        /// </summary>
        public void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal)
        {
            // This is the only acceptable use of Debug.Log in production logic, as it's specifically a testing dummy.
            Debug.Log($"[TrainingDummy] Took {amount} damage (Poise: {poiseDamage}) at {hitPoint}");
        }
        
        #endregion
    }
}