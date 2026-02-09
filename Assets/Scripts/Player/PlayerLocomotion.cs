using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float acceleration = 60f; 
    [SerializeField] private float decceleration = 40f;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckOffset = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.3f;

    private Rigidbody _rb;
    private CapsuleCollider _collider;

    public Vector3 CurrentVelocity => _rb.linearVelocity;
    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.useGravity = true; 
        
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("PlayerLocomotion: Ground Layer не вибрано! Автоматично встановлюю 'Default'.");
            groundLayer = LayerMask.GetMask("Default");
            if (groundLayer.value == 0) groundLayer = 1; 
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyGravityMultiplier();
        
    }

    private void CheckGround()
    {
        Vector3 spherePosition = transform.position + Vector3.up * (groundCheckRadius - groundCheckOffset);
        IsGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void ApplyGravityMultiplier()
    {
        if (!IsGrounded && _rb.linearVelocity.y < 0)
        {
            _rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }

    public void HandleMovement(Vector3 moveDirection, float moveSpeed)
    {
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        Vector3 currentHorizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        float accelRate = moveDirection.sqrMagnitude > 0.01f ? acceleration : decceleration;
        
        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity, 
            targetVelocity, 
            accelRate * Time.fixedDeltaTime
        );

        Vector3 finalVelocity = new Vector3(newHorizontalVelocity.x, _rb.linearVelocity.y, newHorizontalVelocity.z);

        _rb.linearVelocity = finalVelocity;
    }

    public void HandleRotation(Vector3 direction, float rotationSpeed)
    {
        if (direction.sqrMagnitude == 0) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion nextRotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(nextRotation);
    }

    public void SetVelocity(Vector3 velocity)
    {
        _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);
    }

    public void StopMovement()
    {
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
    }

}