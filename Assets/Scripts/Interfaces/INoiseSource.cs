namespace Longinus.Interfaces
{
    /// <summary>
    /// Defines an entity or object capable of emitting sound that AI systems can detect.
    /// </summary>
    public interface INoiseSource
    {
        #region Core Methods
        
        /// <summary>
        /// Evaluates whether the source is currently emitting a detectable level of noise.
        /// </summary>
        /// <returns>True if noise is being produced, otherwise false.</returns>
        bool IsMakingNoise();
        
        #endregion
    }
}