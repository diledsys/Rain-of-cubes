using System.Collections;
using UnityEngine;

namespace RainOfCubes
{
    [DisallowMultipleComponent]
    public sealed class CubeSpawner : MonoBehaviour
    {
        private const float HalfSizeFactor = 0.5f;
        private const float SpawnAreaGizmoHeight = 0.1f;

        [Header("Dependencies")]
        [SerializeField] private CubePool _cubePool;
        [SerializeField] private Transform _spawnCenter;

        [Header("Spawn Settings")]
        [SerializeField] private Vector2 _spawnAreaSize = new Vector2(14f, 14f);
        [SerializeField, Min(0.05f)] private float _spawnInterval = 0.25f;

        private WaitForSeconds _spawnDelay;
        private Coroutine _spawnCoroutine;

        private void OnEnable()
        {
            _spawnDelay = new WaitForSeconds(_spawnInterval);
            _spawnCoroutine = StartCoroutine(SpawnContinuously());
        }

        private void OnDisable()
        {
            if (_spawnCoroutine == null)
            {
                return;
            }

            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        private void OnValidate()
        {
            _spawnAreaSize.x = Mathf.Max(0f, _spawnAreaSize.x);
            _spawnAreaSize.y = Mathf.Max(0f, _spawnAreaSize.y);
        }

        private IEnumerator SpawnContinuously()
        {
            while (isActiveAndEnabled)
            {
                SpawnCube();
                yield return _spawnDelay;
            }

            _spawnCoroutine = null;
        }

        private void SpawnCube()
        {
            Vector3 center = _spawnCenter.position;
            float halfWidth = _spawnAreaSize.x * HalfSizeFactor;
            float halfDepth = _spawnAreaSize.y * HalfSizeFactor;
            Vector3 position = new Vector3(
                center.x + UnityEngine.Random.Range(-halfWidth, halfWidth),
                center.y,
                center.z + UnityEngine.Random.Range(-halfDepth, halfDepth));

            _cubePool.TrySpawn(position, UnityEngine.Random.rotation, out _);
        }

        private void OnDrawGizmosSelected()
        {
            if (_spawnCenter == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(
                _spawnCenter.position,
                new Vector3(_spawnAreaSize.x, SpawnAreaGizmoHeight, _spawnAreaSize.y));
        }
    }
}
