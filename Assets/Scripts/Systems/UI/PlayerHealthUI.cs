using UnityEngine;
using UnityEngine.UI;
using Longinus.Player;

namespace Longinus.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private PlayerStatsManager _playerStats;
        
        [SerializeField] 
        private Slider _healthSlider;

        private void Start()
        {
            if (_playerStats == null || _healthSlider == null)
            {
                Debug.LogError("[PlayerHealthUI] Не призначені посилання в інспекторі!");
                return;
            }

            _healthSlider.maxValue = _playerStats.MaxHealth;
            _healthSlider.value = _playerStats.CurrentHealth;

            _playerStats.OnDamage += UpdateHealthBar;
        }

        private void OnDestroy()
        {
            if (_playerStats != null)
            {
                _playerStats.OnDamage -= UpdateHealthBar;
            }
        }

        private void UpdateHealthBar(float currentHealth)
        {
            _healthSlider.value = currentHealth;
        }
    }
}