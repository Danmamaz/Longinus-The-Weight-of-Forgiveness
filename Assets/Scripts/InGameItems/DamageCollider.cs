using UnityEngine;

namespace InGameItems
{
public class DamageCollider : MonoBehaviour
{
    [SerializeField] private float damageAmount;
    [SerializeField] private float poiseAmount;
    [SerializeField] private GameObject owner;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        // Prevent attacking the owner 
        if (other.gameObject == owner) return;

        // Searching for the Interface, TryGetComponent faster than GetComponent 
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            // Determine the approximate point of contact
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = (transform.position - hitPoint).normalized;

            damageable.TakeDamage(damageAmount, poiseAmount, hitPoint, hitNormal);
        }
    }

    public void Enable() => _collider.enabled = true;
    public void Disable() => _collider.enabled = false;
}
}