namespace Longinus.Interfaces
{
    /// <summary>
    /// Defines an object within the world that the player can manually interact with.
    /// </summary>
    public interface IInteractable
    {
        #region Core Methods
        
        /// <summary>
        /// Retrieves the contextual UI prompt for this specific interaction.
        /// </summary>
        /// <returns>A localized or static string to display to the player.</returns>
        string GetInteractionText();

        /// <summary>
        /// Executes the logic bound to the interaction.
        /// </summary>
        void Interact();
        
        #endregion
    }
}