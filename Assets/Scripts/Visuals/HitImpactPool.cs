using System.Collections.Generic;
using UnityEngine;

namespace Longinus.Visuals
{
    public class HitImpactPool : MonoBehaviour
    {
        #region Constants & Inspector Variables

        private const int   POOL_SIZE         = 20;
        private const float SPARK_LIFETIME    = 0.3f;
        private const float SMOKE_LIFETIME    = 0.6f;
        private const int   SPARK_BURST_COUNT = 18;
        private const int   SMOKE_BURST_COUNT = 6;

        private static readonly Color SPARK_COLOR = new Color(2f, 1.6f, 0.5f, 1f);
        private static readonly Color SMOKE_COLOR = new Color(0.4f, 0.4f, 0.4f, 0.8f);

        #endregion

        #region Private Variables

        private Queue<ParticleSystem> _sparkPool;
        private Queue<ParticleSystem> _smokePool;

        #endregion

        #region Public Properties

        public static HitImpactPool Instance { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Instance   = this;
            _sparkPool = new Queue<ParticleSystem>(POOL_SIZE);
            _smokePool = new Queue<ParticleSystem>(POOL_SIZE);

            for (int i = 0; i < POOL_SIZE; i++)
            {
                _sparkPool.Enqueue(CreateSparkSystem());
                _smokePool.Enqueue(CreateSmokeSystem());
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region State / Core Logic

        public void PlayHitImpact(Vector3 position, Vector3 normal)
        {
            ParticleSystem spark = DequeueOrReuse(_sparkPool);
            spark.transform.position = position;
            spark.transform.rotation = Quaternion.LookRotation(normal);
            spark.Emit(SPARK_BURST_COUNT);
            _sparkPool.Enqueue(spark);

            ParticleSystem smoke = DequeueOrReuse(_smokePool);
            smoke.transform.position = position;
            smoke.Emit(SMOKE_BURST_COUNT);
            _smokePool.Enqueue(smoke);
        }

        private ParticleSystem DequeueOrReuse(Queue<ParticleSystem> pool)
        {
            return pool.Dequeue();
        }

        private ParticleSystem CreateSparkSystem()
        {
            var go = new GameObject("PooledSpark");
            go.transform.SetParent(transform);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.startLifetime    = SPARK_LIFETIME;
            main.startSpeed       = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize        = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor       = SPARK_COLOR;
            main.gravityModifier  = 1.2f;
            main.maxParticles     = 50;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;
            main.playOnAwake      = false;

            var emission = ps.emission;
            emission.enabled      = true;
            emission.rateOverTime = 0;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle     = 35f;
            shape.radius    = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(SPARK_COLOR,             0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0f), 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = grad;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rend.material.SetColor("_BaseColor", SPARK_COLOR);
            rend.material.EnableKeyword("_EMISSION");

            return ps;
        }

        private ParticleSystem CreateSmokeSystem()
        {
            var go = new GameObject("PooledSmoke");
            go.transform.SetParent(transform);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.startLifetime    = SMOKE_LIFETIME;
            main.startSpeed       = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize        = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startColor       = SMOKE_COLOR;
            main.gravityModifier  = -0.1f;
            main.maxParticles     = 30;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;
            main.playOnAwake      = false;

            var emission = ps.emission;
            emission.enabled      = true;
            emission.rateOverTime = 0;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = 0.2f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size    = new ParticleSystem.MinMaxCurve(
                1f, AnimationCurve.Linear(0f, 0.5f, 1f, 2f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(SMOKE_COLOR, 0f) },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0f,   1f)
                });
            colorOverLifetime.color = grad;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rend.material.SetColor("_BaseColor", SMOKE_COLOR);

            return ps;
        }

        #endregion
    }
}
