using UnityEngine;

namespace Longinus.Visuals
{
    public class ArenaFogVolume : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField] private Vector3 _volumeSize = new Vector3(30f, 3f, 30f);

        private const float EMISSION_RATE          = 80f;
        private const float PARTICLE_LIFETIME_MIN  = 4f;
        private const float PARTICLE_LIFETIME_MAX  = 8f;
        private const float PARTICLE_SIZE_MIN      = 3f;
        private const float PARTICLE_SIZE_MAX      = 6f;
        private const float DRIFT_SPEED            = 0.3f;

        private static readonly Color FOG_COLOR = new Color(0.6f, 0.65f, 0.7f, 0.15f);

        #endregion

        #region Private Variables

        private ParticleSystem _ps;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _ps = gameObject.AddComponent<ParticleSystem>();
            ConfigureFogSystem();
        }

        #endregion

        #region State / Core Logic

        private void ConfigureFogSystem()
        {
            _ps.Stop();

            var main = _ps.main;
            main.startLifetime   = new ParticleSystem.MinMaxCurve(PARTICLE_LIFETIME_MIN, PARTICLE_LIFETIME_MAX);
            main.startSpeed      = new ParticleSystem.MinMaxCurve(0f, DRIFT_SPEED);
            main.startSize       = new ParticleSystem.MinMaxCurve(PARTICLE_SIZE_MIN, PARTICLE_SIZE_MAX);
            main.startColor      = FOG_COLOR;
            main.maxParticles    = 200;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake     = false;

            var emission = _ps.emission;
            emission.enabled      = true;
            emission.rateOverTime = EMISSION_RATE;

            var shape = _ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale     = _volumeSize;

            var velocity = _ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space   = ParticleSystemSimulationSpace.World;
            velocity.x       = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            velocity.y       = new ParticleSystem.MinMaxCurve(0f, 0.1f);
            velocity.z       = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);

            var sizeOverLifetime = _ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size    = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f,   0f),
                    new Keyframe(0.2f, 1f),
                    new Keyframe(0.8f, 1f),
                    new Keyframe(1f,   0f)));

            var colorOverLifetime = _ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(FOG_COLOR, 0.5f) },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f,    0f),
                    new GradientAlphaKey(0.15f, 0.5f),
                    new GradientAlphaKey(0f,    1f)
                });
            colorOverLifetime.color = grad;

            var rend = _ps.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rend.material.SetColor("_BaseColor", FOG_COLOR);
            rend.material.SetFloat("_Surface", 1f);

            _ps.Play();
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color  = new Color(0.6f, 0.65f, 0.7f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, _volumeSize);
        }
#endif
    }
}
