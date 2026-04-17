using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// Handles physics-based movement, rotation, and ground detection for the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerLocomotion : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Movement Settings")]
        [SerializeField, Tooltip("Rate at which the player reaches maximum speed.")] 
        private float _acceleration = 60f; 
        
        [SerializeField, Tooltip("Rate at which the player stops when there is no input.")] 
        private float _deceleration = 40f;
        
        [Header("Ground Detection")]
        [SerializeField, Tooltip("Layers considered as solid ground.")] 
        private LayerMask _groundLayer;
        
        [SerializeField, Tooltip("Vertical offset for the ground detection sphere.")] 
        private float _groundCheckOffset = 0.1f;
        
        [SerializeField, Tooltip("Radius of the ground detection sphere.")] 
        private float _groundCheckRadius = 0.3f;

        #endregion

        #region Private Variables
        
        private Rigidbody _rb;
        
        #endregion

        #region Public Properties
        
        public Vector3 CurrentVelocity => _rb.linearVelocity;
        public bool IsGrounded { get; private set; }
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.useGravity = true; 
            
            if (_groundLayer.value == 0)
            {
                Debug.LogWarning("[PlayerLocomotion] Ground Layer is not set! Automatically assigning 'Default'.");
                _groundLayer = LayerMask.GetMask("Default");
                if (_groundLayer.value == 0) _groundLayer = 1; 
            }
        }

        private void FixedUpdate()
        {
            CheckGround();
            ApplyGravityMultiplier();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 spherePosition = transform.position + Vector3.up * (_groundCheckRadius - _groundCheckOffset);
            Gizmos.color = IsGrounded ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(spherePosition, _groundCheckRadius);
        }
#endif

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Performs a sphere cast to determine if the player is touching the ground.
        /// </summary>
        private void CheckGround()
        {
            Vector3 spherePosition = transform.position + Vector3.up * (_groundCheckRadius - _groundCheckOffset);
            IsGrounded = Physics.CheckSphere(spherePosition, _groundCheckRadius, _groundLayer, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Applies extra downward force when falling to make jumps/falls feel heavier and more responsive.
        /// </summary>
        private void ApplyGravityMultiplier()
        {
            if (!IsGrounded && _rb.linearVelocity.y < 0)
            {
                _rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Calculates and applies horizontal velocity based on input direction, speed, and acceleration.
        /// </summary>
        /// <param name="moveDirection">Normalized input direction.</param>
        /// <param name="moveSpeed">Target maximum speed.</param>
        public void HandleMovement(Vector3 moveDirection, float moveSpeed)
        {
            Vector3 targetVelocity = moveDirection * moveSpeed;
            Vector3 currentHorizontalVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

            float accelRate = moveDirection.sqrMagnitude > 0.01f ? _acceleration : _deceleration;
            
            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity, 
                targetVelocity, 
                accelRate * Time.fixedDeltaTime
            );

            Vector3 finalVelocity = new Vector3(newHorizontalVelocity.x, _rb.linearVelocity.y, newHorizontalVelocity.z);
            _rb.linearVelocity = finalVelocity;
        }

        /// <summary>
        /// Smoothly rotates the player to face the desired direction.
        /// </summary>
        /// <param name="direction">Target direction to face.</param>
        /// <param name="rotationSpeed">Speed of the rotation interpolation.</param>
        public void HandleRotation(Vector3 direction, float rotationSpeed)
        {
            if (direction.sqrMagnitude == 0f) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion nextRotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(nextRotation);
        }

        /// <summary>
        /// Forces a specific velocity onto the rigidbody while maintaining the current vertical velocity.
        /// </summary>
        public void SetVelocity(Vector3 velocity)
        {
            _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);
        }

        /// <summary>
        /// Instantly halts horizontal movement.
        /// </summary>
        public void StopMovement()
        {
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        }

        /// <summary>
        /// Applies constant physical impulse forward.
        /// </summary>
        public void ApplyForwardImpulse(float force)
        {
            if (force > 0f)
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                _rb.AddForce(transform.forward * force, ForceMode.Impulse);
            }
        }

        #endregion
    }
}