using UnityEngine;
using UnityEngine.Events;
public class EnemyStatsManager : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }



    public UnityEvent OnDeath;

    public UnityEvent<float> OnDamage;




    private void Awake()
    {
        CurrentHealth = maxHealth;

    }

    private void Update()
    {

    }


    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
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