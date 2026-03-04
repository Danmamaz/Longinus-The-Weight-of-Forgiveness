using UnityEngine;
using UnityEngine.Events;

public class PlayerStatsManager : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenDelay = 1.2f;

    public float CurrentHealth { get; private set; }
    public float CurrentStamina { get; private set; }

    public UnityEvent<float> OnDamage;
    public UnityEvent OnStaminaChange;
    public UnityEvent OnDeath;

    private float _staminaRegenerationTimer = 0f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentStamina = maxStamina;
    }

    private void Update()
    {
        HandleStaminaRegen();
    }

    private void HandleStaminaRegen()
    {
        if (_staminaRegenerationTimer < staminaRegenDelay)
        {
            _staminaRegenerationTimer += Time.deltaTime;
            return;
        }

        if (CurrentStamina < maxStamina)
        {
            CurrentStamina += staminaRegenRate * Time.deltaTime;
            if (CurrentStamina > maxStamina) CurrentStamina = maxStamina;
            OnStaminaChange?.Invoke();
        }
    }

    public bool TryConsumeStamina(float amount)
    {
        
        if (CurrentStamina <= 0) return false;

        CurrentStamina -= amount;
        if (CurrentStamina < 0) CurrentStamina = 0;

        _staminaRegenerationTimer = 0f; 
        
        OnStaminaChange?.Invoke();
        return true;
    }

    public void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (CurrentHealth <= 0) return;
        
        CurrentHealth -= amount;
        
        OnDamage?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            // Die() method needs to be added
            
            OnDeath?.Invoke();
        }
    }
}