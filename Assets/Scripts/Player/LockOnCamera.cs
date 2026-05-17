using UnityEngine;

namespace Longinus.Player
{
    /// <summary>
    /// Smoothly follows the player and orbits to frame both the player and the lock-on target.
    /// Falls back to a simple offset follow when no target is locked.
    /// Attach to the Camera GameObject. Runs in LateUpdate so it always follows physics.
    /// </summary>
    public class LockOnCamera : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("References")]
        [SerializeField, Tooltip("Player root transform to orbit around.")]
        private Transform _playerTransform;

        [SerializeField, Tooltip("Lock-on system to query for the current target.")]
        private LockOnSystem _lockOnSystem;

        [Header("Lock-On Camera")]
        [SerializeField, Tooltip("Distance kept behind the player while locked on.")]
        private float _orbitDistance = 5f;

        [SerializeField, Tooltip("Height above the player pivot while locked on.")]
        private float _orbitHeight = 2f;

        [SerializeField, Tooltip("Position lerp speed during lock-on.")]
        private float _positionFollowSpeed = 8f;

        [SerializeField, Tooltip("Rotation slerp speed.")]
        private float _rotationFollowSpeed = 10f;

        [Header("Normal Camera Fallback")]
        [SerializeField, Tooltip("Position lerp speed when not locked on.")]
        private float _normalFollowSpeed = 6f;

        [SerializeField, Tooltip("Camera offset in player-local space when not locked on.")]
        private Vector3 _normalOffset = new Vector3(0f, 3f, -5f);

        [SerializeField, Tooltip("Enable when another component already controls this transform's position. During lock-on, only rotation will be overridden.")]
        private bool _overridePositionOnly;

        #endregion

        #region Private Variables

        // Reserved for a future SmoothDamp migration; Lerp is used now.
        private Vector3 _currentVelocity;

        #endregion

        #region Unity Lifecycle

        private void LateUpdate()
        {
            if (_lockOnSystem == null || !_lockOnSystem.IsLockedOn)
                RunNormalFollow();
            else
                RunLockOnFollow();
        }

        #endregion

        #region State/Core Logic

        private void RunLockOnFollow()
        {
            Transform target = _lockOnSystem.CurrentTarget;
            if (target == null)
            {
                RunNormalFollow();
                return;
            }

            if (!_overridePositionOnly)
            {
                Vector3 dirAwayFromTarget = _playerTransform.position - target.position;
                dirAwayFromTarget.y = 0f;
                if (dirAwayFromTarget == Vector3.zero)
                    dirAwayFromTarget = -_playerTransform.forward;
                dirAwayFromTarget.Normalize();

                Vector3 idealPos = _playerTransform.position
                                 + dirAwayFromTarget * _orbitDistance
                                 + Vector3.up * _orbitHeight;
                transform.position = Vector3.Lerp(
                    transform.position, idealPos, _positionFollowSpeed * Time.deltaTime);
            }

            Vector3 midpoint = (_playerTransform.position + target.position) * 0.5f;
            Vector3 lookDir = midpoint - transform.position;
            if (lookDir == Vector3.zero) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, _rotationFollowSpeed * Time.deltaTime);
        }

        // Only active when no dedicated camera system is controlling this transform.
        private void RunNormalFollow()
        {
            if (_overridePositionOnly) return;

            Vector3 idealPos = _playerTransform.position
                             + _playerTransform.TransformDirection(_normalOffset);
            transform.position = Vector3.Lerp(
                transform.position, idealPos, _normalFollowSpeed * Time.deltaTime);

            Vector3 lookDir = _playerTransform.position - transform.position;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    _rotationFollowSpeed * Time.deltaTime);
            }
        }

        #endregion
    }
}
