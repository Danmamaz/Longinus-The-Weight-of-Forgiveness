using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Modifies the player's karma value.
    /// </summary>
    [System.Serializable]
    public class KarmaChange : Consequence
    {
        #region Inspector Variables
        
        [SerializeField, Tooltip("Amount of karma to add or subtract.")]
        private int _amount;
        
        #endregion

        #region State/Core Logic
        
        public override void Apply(PlotManager context)
        {
            context.ChangeKarma(_amount);
        }
        
        #endregion
    }

    /// <summary>
    /// Alters the global world state.
    /// </summary>
    [System.Serializable]
    public class ChangeWorldState : Consequence
    {
        #region Inspector Variables
        
        [SerializeField, Tooltip("The new world state to transition into.")]
        private WorldStateType _worldState;
        
        #endregion

        #region State/Core Logic
        
        public override void Apply(PlotManager context)
        {
            context.ChangeWorldState(_worldState);
        }
        
        #endregion
    }

    /// <summary>
    /// Unlocks a previously blocked path or door in the world.
    /// </summary>
    [System.Serializable]
    public class OpenPath : Consequence
    {
        #region Inspector Variables
        
        [SerializeField, Tooltip("Unique ID of the path or door to open.")]
        private string _pathId;
        
        #endregion

        #region State/Core Logic
        
        public override void Apply(PlotManager context)
        {
            if (!string.IsNullOrEmpty(_pathId))
            {
                // Delegating to PlotManager to safely handle state mutation and event invocation
                context.OpenPath(_pathId); 
            }
        }
        
        #endregion
    }
}