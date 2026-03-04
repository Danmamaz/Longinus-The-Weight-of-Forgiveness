using UnityEngine;

namespace InGameItems
{
public class TrainingDummy : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log($"[Dummy] Took damage: {amount} in {hitPoint}");
    }
}
}