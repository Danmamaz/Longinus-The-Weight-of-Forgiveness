using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        Debug.Log($"[Dummy] Took damage: {amount} in {hitPoint}");
    }
}