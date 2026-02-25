using System;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(BossCombatController))]
    public sealed class BossPoiseManager : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────
        [Header("Poise")]
        [SerializeField, Min(1f)] private float _maxPoise        = 100f;
        [SerializeField, Min(0f)] private float _regenPerSecond  = 15f;
        [SerializeField, Min(0f)] private float _regenDelay      = 5f;

        // ── Events ─────────────────────────────────────────────
        public event Action OnStanceBreak;

        // ── Runtime ────────────────────────────────────────────
        private EnemyController      _enemyController;
        private BossCombatController _combatController;
        private float _currentPoise;
        private float _lastDamageTime = Mathf.NegativeInfinity;
        private bool  _broken;

        public float CurrentPoise => _currentPoise;
        public float MaxPoise     => _maxPoise;
        public bool  IsBroken     => _broken;

        // ── Unity Lifecycle ────────────────────────────────────
        private void Awake()
        {
            _enemyController  = GetComponent<EnemyController>();
            _combatController = GetComponent<BossCombatController>();
            _currentPoise     = _maxPoise;
        }

        private void Update()
        {
            if (_broken) return;

            if (Time.time - _lastDamageTime >= _regenDelay && _currentPoise < _maxPoise)
            {
                _currentPoise = Mathf.Min(
                    _currentPoise + _regenPerSecond * Time.deltaTime, _maxPoise);
            }
        }

        // ── Public API ─────────────────────────────────────────

        public void TakePoiseDamage(float amount)
        {
            if (_broken) return;

            _lastDamageTime = Time.time;
            _currentPoise   = Mathf.Max(_currentPoise - amount, 0f);

            if (_currentPoise <= 0f)
            {
                _broken = true;
                OnStanceBreak?.Invoke();
                _enemyController.StateMachine.ChangeState(_combatController.BossStunnedState);
            }
        }

        public void ResetPoise()
        {
            _currentPoise = _maxPoise;
            _broken       = false;
        }
    }
}