using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Longinus.EnemySystem;
using Longinus.Player;

namespace Longinus.Visuals
{
    [RequireComponent(typeof(Renderer))]
    public class DissolveOnDeath : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField] private float _dissolveDuration = 2.5f;
        [SerializeField] private float _dissolveDelay    = 1f;
        [SerializeField] private bool  _destroyAfterDissolve = true;

        private const string DISSOLVE_PROPERTY = "_DissolveAmount";
        private static readonly int DissolveID = Shader.PropertyToID(DISSOLVE_PROPERTY);

        #endregion

        #region Private Variables

        private Renderer[]          _renderers;
        private MaterialPropertyBlock _propertyBlock;
        private bool                _isDissolving;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _renderers     = GetComponentsInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();

            if (TryGetComponent(out EnemyStatsManager esm))
                esm.OnDeath += BeginDissolve;
            else if (TryGetComponent(out PlayerStatsManager psm))
                psm.OnDeath += BeginDissolve;
        }

        private void OnDestroy()
        {
            if (TryGetComponent(out EnemyStatsManager esm))
                esm.OnDeath -= BeginDissolve;

            if (TryGetComponent(out PlayerStatsManager psm))
                psm.OnDeath -= BeginDissolve;
        }

        #endregion

        #region State / Core Logic

        private void BeginDissolve()
        {
            if (_isDissolving) return;
            _isDissolving = true;
            StartCoroutine(DissolveRoutine());
        }

        private IEnumerator DissolveRoutine()
        {
            yield return new WaitForSeconds(_dissolveDelay);

            float t = 0f;
            while (t < _dissolveDuration)
            {
                t += Time.deltaTime;
                float amount = t / _dissolveDuration;

                foreach (Renderer r in _renderers)
                {
                    r.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetFloat(DissolveID, amount);
                    r.SetPropertyBlock(_propertyBlock);
                }

                yield return null;
            }

            if (_destroyAfterDissolve)
                Destroy(gameObject);
        }

        #endregion
    }
}
