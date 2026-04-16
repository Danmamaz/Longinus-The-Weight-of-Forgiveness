using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Longinus.Player;

namespace Longinus.UI
{
    /// <summary>
    /// Handles the visual representation of the player's attack cooldown.
    /// Manages UI fill amounts, color darkening during cooldowns, and a flash effect upon recovery.
    /// </summary>
    public class AttackCooldownUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private PlayerCombatManager _combatManager;
        
        [SerializeField, Tooltip("Image with Type: Filled")] 
        private Image _fillImage;
        [SerializeField] Animator _flash;
        
        [SerializeField, Tooltip("All images that need to be darken")] 
        private Image[] _imagesToDarken;

        [Header("Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _darkColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private bool _wasOnCooldown;

        private void Update()
        {
            if (_combatManager == null) return;

            bool isOnCooldown = _combatManager.CurrentAttackCooldown > 0f;

            if (isOnCooldown && _combatManager.MaxAttackCooldown > 0f)
            {
                _fillImage.fillAmount = 1f - (_combatManager.CurrentAttackCooldown / _combatManager.MaxAttackCooldown);
            }
            else
            {
                _fillImage.fillAmount = 1f; 
            }

            if (isOnCooldown && !_wasOnCooldown)
            {
                SetImagesColor(_darkColor);
            }
            else if (!isOnCooldown && _wasOnCooldown)
            {
                _flash.SetTrigger("Flash");
                SetImagesColor(_normalColor);
            }

            _wasOnCooldown = isOnCooldown;
        }

        /// <summary>
        /// Applies a specific color to all tracked UI images in the array.
        /// </summary>
        private void SetImagesColor(Color targetColor)
        {
            foreach (var img in _imagesToDarken)
            {
                if (img != null) img.color = targetColor;
            }
        }
    }
}