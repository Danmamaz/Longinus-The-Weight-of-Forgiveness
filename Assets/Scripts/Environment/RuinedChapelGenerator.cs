using System.Collections.Generic;
using UnityEngine;

namespace Longinus.Environment
{
    public class RuinedChapelGenerator : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField] private Transform _arenaCenter;
        [SerializeField] private float _arenaRadius = 15f;
        [SerializeField] private int _randomSeed = 42;

        [Header("Prop Prefabs — minimum drag-and-drop")]
        [SerializeField] private GameObject[] _rockPrefabs;
        [SerializeField] private GameObject[] _pillarPrefabs;
        [SerializeField] private GameObject[] _rubblePrefabs;
        [SerializeField] private GameObject[] _foliagePrefabs;
        [SerializeField] private GameObject _candleHolderPrefab;
        [SerializeField] private GameObject _brokenAltarPrefab;

        // Inner ring kept sparse — player needs unobstructed fighting space.
        private static readonly ScatterRing[] SCATTER_RINGS = new[]
        {
            new ScatterRing { radiusMin = 2f,  radiusMax = 5f,  density = 3,  types = ScatterType.Rubble },
            new ScatterRing { radiusMin = 6f,  radiusMax = 10f, density = 8,  types = ScatterType.Pillar | ScatterType.Foliage },
            new ScatterRing { radiusMin = 11f, radiusMax = 15f, density = 15, types = ScatterType.Rock   | ScatterType.Rubble },
            new ScatterRing { radiusMin = 14f, radiusMax = 17f, density = 25, types = ScatterType.Foliage | ScatterType.Rock }
        };

        private static readonly LandmarkSpawn[] LANDMARKS = new[]
        {
            new LandmarkSpawn { angle = 0f,   distance = 12f, type = ScatterType.BrokenAltar },
            new LandmarkSpawn { angle = 90f,  distance = 10f, type = ScatterType.Pillar },
            new LandmarkSpawn { angle = 180f, distance = 10f, type = ScatterType.Pillar },
            new LandmarkSpawn { angle = 270f, distance = 10f, type = ScatterType.Pillar },
            new LandmarkSpawn { angle = 45f,  distance = 8f,  type = ScatterType.Candle },
            new LandmarkSpawn { angle = 135f, distance = 8f,  type = ScatterType.Candle },
            new LandmarkSpawn { angle = 225f, distance = 8f,  type = ScatterType.Candle },
            new LandmarkSpawn { angle = 315f, distance = 8f,  type = ScatterType.Candle }
        };

        [System.Flags]
        private enum ScatterType
        {
            None         = 0,
            Rock         = 1,
            Pillar       = 2,
            Rubble       = 4,
            Foliage      = 8,
            Candle       = 16,
            BrokenAltar  = 32
        }

        [System.Serializable]
        private struct ScatterRing
        {
            public float radiusMin;
            public float radiusMax;
            public int density;
            public ScatterType types;
        }

        [System.Serializable]
        private struct LandmarkSpawn
        {
            public float angle;
            public float distance;
            public ScatterType type;
        }

        #endregion

        #region State / Core Logic

        [ContextMenu("Generate Arena Props")]
        public void Generate()
        {
            if (_arenaCenter == null)
                _arenaCenter = transform;

            ClearChildren();
            Random.InitState(_randomSeed);

            foreach (LandmarkSpawn lm in LANDMARKS)
                PlaceLandmark(lm);

            foreach (ScatterRing ring in SCATTER_RINGS)
                ScatterInRing(ring);

            Debug.Log($"[ChapelGen] Generated {transform.childCount} props");
        }

        [ContextMenu("Clear Generated Props")]
        public void ClearChildren()
        {
            var children = new List<GameObject>();
            foreach (Transform t in transform)
                children.Add(t.gameObject);

            foreach (GameObject child in children)
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        private void PlaceLandmark(LandmarkSpawn lm)
        {
            GameObject prefab = PickPrefab(lm.type);
            if (prefab == null) return;

            Vector3 worldPos = AngleToWorld(lm.angle, lm.distance);
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, worldPos, rot, transform);
        }

        private void ScatterInRing(ScatterRing ring)
        {
            for (int i = 0; i < ring.density; i++)
            {
                float angle = Random.Range(0f, 360f);
                float dist  = Random.Range(ring.radiusMin, ring.radiusMax);
                Vector3 pos = AngleToWorld(angle, dist);

                if (!TryGetGroundPosition(pos, out Vector3 grounded))
                    continue;

                ScatterType chosenType = PickFromFlags(ring.types);
                GameObject prefab = PickPrefab(chosenType);
                if (prefab == null) continue;

                Quaternion rot = Quaternion.Euler(
                    Random.Range(-5f, 5f),
                    Random.Range(0f, 360f),
                    Random.Range(-5f, 5f));

                float scale = Random.Range(0.85f, 1.15f);
                GameObject inst = Instantiate(prefab, grounded, rot, transform);
                inst.transform.localScale *= scale;
            }
        }

        private Vector3 AngleToWorld(float angleDeg, float distance)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad) * distance, 0f, Mathf.Sin(rad) * distance);
            return _arenaCenter.position + offset;
        }

        private bool TryGetGroundPosition(Vector3 worldXZ, out Vector3 result)
        {
            Vector3 rayOrigin = new Vector3(worldXZ.x, worldXZ.y + 20f, worldXZ.z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 50f))
            {
                result = hit.point;
                return true;
            }
            result = worldXZ;
            return false;
        }

        private ScatterType PickFromFlags(ScatterType flags)
        {
            var available = new List<ScatterType>();
            foreach (ScatterType t in System.Enum.GetValues(typeof(ScatterType)))
            {
                if (t == ScatterType.None) continue;
                if ((flags & t) != 0) available.Add(t);
            }
            return available[Random.Range(0, available.Count)];
        }

        private GameObject PickPrefab(ScatterType type)
        {
            switch (type)
            {
                case ScatterType.Rock:        return PickRandom(_rockPrefabs);
                case ScatterType.Pillar:      return PickRandom(_pillarPrefabs);
                case ScatterType.Rubble:      return PickRandom(_rubblePrefabs);
                case ScatterType.Foliage:     return PickRandom(_foliagePrefabs);
                case ScatterType.Candle:      return _candleHolderPrefab;
                case ScatterType.BrokenAltar: return _brokenAltarPrefab;
                default:                      return null;
            }
        }

        private GameObject PickRandom(GameObject[] arr)
        {
            if (arr == null || arr.Length == 0) return null;
            return arr[Random.Range(0, arr.Length)];
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_arenaCenter == null) return;

            Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f);
            foreach (ScatterRing ring in SCATTER_RINGS)
            {
                DrawCircleGizmo(_arenaCenter.position, ring.radiusMin);
                DrawCircleGizmo(_arenaCenter.position, ring.radiusMax);
            }

            Gizmos.color = Color.red;
            foreach (LandmarkSpawn lm in LANDMARKS)
                Gizmos.DrawWireSphere(AngleToWorld(lm.angle, lm.distance), 0.5f);
        }

        private void DrawCircleGizmo(Vector3 center, float radius)
        {
            const int SEGMENTS = 32;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= SEGMENTS; i++)
            {
                float a = (i / (float)SEGMENTS) * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}
