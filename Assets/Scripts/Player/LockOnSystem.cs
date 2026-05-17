using System.Collections.Generic;
using UnityEngine;
using Longinus.UI;

namespace Longinus.Player
{
    /// <summary>
    /// Handles lock-on target acquisition, validation, and UI marker lifecycle.
    /// Attach to the same GameObject as PlayerController.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class LockOnSystem : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Detection Settings")]
        [SerializeField, Tooltip("Sphere radius within which enemies are eligible for lock-on.")]
        private float _lockOnRadius = 12f;

        [SerializeField, Tooltip("Layer mask that identifies enemy colliders.")]
        private LayerMask _enemyLayer;

        [SerializeField, Tooltip("Layer mask used for line-of-sight obstruction checks.")]
        private LayerMask _obstacleLayer;

        [SerializeField, Tooltip("Half-angle of the camera FOV cone considered for target selection.")]
        private float _maxAngleDegrees = 60f;

        [Header("UI")]
        [SerializeField, Tooltip("Prefab containing LockOnMarkerUI. Instantiated when a target is acquired.")]
        private LockOnMarkerUI _markerPrefab;

        [SerializeField, Tooltip("Camera used for screen-space projection and FOV checks.")]
        private Camera _camera;

        #endregion

        #region Private Variables

        private Transform _currentTarget;
        private LockOnMarkerUI _activeMarker;
        private bool _isLockedOn;

        // Pre-allocated buffer — prevents GC allocation on every overlap query
        private readonly Collider[] _hitBuffer = new Collider[20];

        #endregion

        #region Public Properties

        public bool IsLockedOn => _isLockedOn;
        public Transform CurrentTarget => _currentTarget;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            if (!_isLockedOn) return;
            if (!ValidateCurrentTarget()) return;

            _activeMarker.UpdatePosition(_currentTarget);
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Toggles lock-on: clears an existing target or acquires the best available one.
        /// </summary>
        public void ToggleLockOn()
        {
            if (_isLockedOn)
                ClearLockOn();
            else
                TryAcquireTarget();
        }

        /// <summary>
        /// Releases the current target and destroys the UI marker.
        /// </summary>
        public void ClearLockOn()
        {
            _currentTarget = null;
            _isLockedOn = false;

            if (_activeMarker != null)
            {
                Destroy(_activeMarker.gameObject);
                _activeMarker = null;
            }
        }

        /// <summary>
        /// Cycles the lock-on target left (-1) or right (+1) in screen space.
        /// Wraps around to the opposite screen edge when no target exists in the requested direction.
        /// </summary>
        public void SwitchTarget(int direction)
        {
            if (!_isLockedOn) return;

            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _lockOnRadius, _hitBuffer, _enemyLayer);

            var allCandidates = new List<(Transform t, float screenX)>(count);
            for (int i = 0; i < count; i++)
            {
                if (_hitBuffer[i] == null) continue;
                Transform t = _hitBuffer[i].transform;
                if (t == _currentTarget) continue;
                if (!HasLineOfSight(t)) continue;
                Vector3 screenPos = _camera.WorldToScreenPoint(t.position);
                if (screenPos.z < 0f) continue;
                allCandidates.Add((t, screenPos.x));
            }

            float currentScreenX = _camera.WorldToScreenPoint(_currentTarget.position).x;

            var candidates = new List<(Transform t, float screenX)>(allCandidates.Count);
            foreach (var c in allCandidates)
            {
                if (direction > 0 && c.screenX > currentScreenX) candidates.Add(c);
                else if (direction < 0 && c.screenX < currentScreenX) candidates.Add(c);
            }

            if (candidates.Count == 0)
            {
                if (allCandidates.Count == 0) return;

                // Wrap: pick the candidate furthest in the requested direction
                var wrap = allCandidates[0];
                for (int i = 1; i < allCandidates.Count; i++)
                {
                    if (direction > 0 && allCandidates[i].screenX > wrap.screenX) wrap = allCandidates[i];
                    else if (direction < 0 && allCandidates[i].screenX < wrap.screenX) wrap = allCandidates[i];
                }
                candidates.Add(wrap);
            }

            candidates.Sort((a, b) =>
                Mathf.Abs(a.screenX - currentScreenX).CompareTo(Mathf.Abs(b.screenX - currentScreenX)));

            SetTarget(candidates[0].t);
        }

        private void TryAcquireTarget()
        {
            if (_camera == null)
            {
                Debug.LogWarning("[LockOnSystem] No camera assigned. Cannot acquire target.");
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _lockOnRadius, _hitBuffer, _enemyLayer);

            Transform best = FindBestTarget(_hitBuffer, count);
            if (best == null) return;

            SetTarget(best);
        }

        /// <summary>
        /// Finds the enemy most centered in the camera's view within the detection cone.
        /// </summary>
        private Transform FindBestTarget(Collider[] candidates, int count)
        {
            // Cos of the max angle is the minimum acceptable dot product (acts as FOV gate)
            float bestDot = Mathf.Cos(_maxAngleDegrees * Mathf.Deg2Rad);
            Transform best = null;

            for (int i = 0; i < count; i++)
            {
                if (candidates[i] == null) continue;

                Vector3 toEnemy = (candidates[i].transform.position - _camera.transform.position).normalized;
                float dot = Vector3.Dot(_camera.transform.forward, toEnemy);

                // Skip targets outside the FOV cone or less centered than the current best
                if (dot < bestDot) continue;

                if (!HasLineOfSight(candidates[i].transform)) continue;

                bestDot = dot;
                best = candidates[i].transform;
            }

            return best;
        }

        /// <summary>
        /// Checks whether an unobstructed ray can reach the target from eye height.
        /// </summary>
        private bool HasLineOfSight(Transform target)
        {
            Vector3 origin = transform.position + Vector3.up * 1.4f;
            Vector3 dir = (target.position + Vector3.up * 1f) - origin;
            return !Physics.Raycast(origin, dir.normalized, dir.magnitude, _obstacleLayer);
        }

        private void SetTarget(Transform target)
        {
            if (_activeMarker != null)
            {
                Destroy(_activeMarker.gameObject);
                _activeMarker = null;
            }
            _currentTarget = target;
            _isLockedOn = true;
            _activeMarker = Instantiate(_markerPrefab);
            _activeMarker.Initialize(_camera);
        }

        /// <summary>
        /// Confirms the current target is still alive, active, and within range.
        /// Calls ClearLockOn and returns false if any check fails.
        /// </summary>
        private bool ValidateCurrentTarget()
        {
            if (_currentTarget == null)
            {
                ClearLockOn();
                return false;
            }

            if (!_currentTarget.gameObject.activeInHierarchy)
            {
                ClearLockOn();
                return false;
            }

            float sqrDist = (transform.position - _currentTarget.position).sqrMagnitude;
            if (sqrDist > _lockOnRadius * _lockOnRadius * 1.5f)
            {
                ClearLockOn();
                return false;
            }

            return true;
        }

        #endregion
    }
}
