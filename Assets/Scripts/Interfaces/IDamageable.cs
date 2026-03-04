using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal);
}