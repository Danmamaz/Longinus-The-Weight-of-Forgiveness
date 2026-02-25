using System;

namespace Combat
{
    /// <summary>
    /// Static event bus for player actions that enemies can react to.
    /// </summary>
    public static class PlayerEvents
    {
        public static event Action OnHealStarted;

        public static void RaiseHealStarted() => OnHealStarted?.Invoke();
    }
}