using UnityEngine;

namespace Longinus.Visuals
{
    public class AmbientDust : MonoBehaviour
    {
        #region Constants & Inspector Variables

        private const float DUST_VOLUME_RADIUS = 8f;
        private const float DUST_EMISSION      = 20f;
        private const float DUST_LIFETIME      = 6f;

        private static readonly Color DUST_COLOR = new Color(1f, 0.9f, 0.7f, 0.3f);

        #endregion

        #region Private Variables

        private ParticleSystem _ps;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _ps = gameObject.AddComponent<ParticleSystem>();
            ConfigureDust();
        }

        #endregion

        #region State / Core Logic

        private void ConfigureDust()
        {
            _ps.Stop();

            var main = _ps.main;
            main.startLifetime   = DUST_LIFETIME;
            main.startSpeed      = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            main.startSize       = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startColor      = DUST_COLOR;
            main.maxParticles    = 150;
            main.gravityModifier = 0.02f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake     = false;

            var emission = _ps.emission;
            emission.enabled      = true;
            emission.rateOverTime = DUST_EMISSION;

            var shape = _ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = DUST_VOLUME_RADIUS;

            var velocity = _ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space   = ParticleSystemSimulationSpace.World;
            velocity.x       = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            velocity.y       = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            velocity.z       = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

            var noise = _ps.noise;
            noise.enabled   = true;
            noise.strength  = 0.5f;
            noise.frequency = 0.3f;

            var colorOverLifetime = _ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(DUST_COLOR, 0.5f) },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f,   0f),
                    new GradientAlphaKey(0.3f, 0.3f),
                    new GradientAlphaKey(0f,   1f)
                });
            colorOverLifetime.color = grad;

            var rend = _ps.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rend.material.SetColor("_BaseColor", DUST_COLOR);

            _ps.Play();
        }

        #endregion
    }
}
