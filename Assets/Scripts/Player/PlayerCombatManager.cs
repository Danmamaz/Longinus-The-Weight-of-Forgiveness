using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    [Header("Combat Config")]
    [SerializeField] private Collider weaponCollider;

    public bool IsAttacking { get; private set; }
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            weaponCollider.isTrigger = true;
        }
    }

    public bool AttemptAttack(bool isHeavy)
    {
        PlayerStatsManager stats = GetComponent<PlayerStatsManager>(); 
        float cost = isHeavy ? 20f : 10f;

        if (!stats.TryConsumeStamina(cost)) return false;

        IsAttacking = true;
        string trigger = isHeavy ? "HeavyAttack" : "LightAttack";
        _animator.SetTrigger(trigger);
        return true;
    }

    
    public void OpenHitbox()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    public void CloseHitbox()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void EndAttack()
    {
        IsAttacking = false;
        CloseHitbox();
    }
}