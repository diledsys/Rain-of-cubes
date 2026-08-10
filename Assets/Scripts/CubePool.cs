using System.Collections.Generic;
using UnityEngine;

namespace RainOfCubes
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class CubePool : MonoBehaviour
    {
        [SerializeField] private FallingCube _cubePrefab;
        [SerializeField] private Transform _inactiveCubesRoot;
        [SerializeField, Min(1)] private int _capacity = 50;

        private Queue<FallingCube> _availableCubes;

        public int Capacity => _capacity;
        public int AvailableCount => _availableCubes?.Count ?? 0;

        private void Awake()
        {
            _availableCubes = new Queue<FallingCube>(_capacity);

            for (int index = 0; index < _capacity; index++)
            {
                FallingCube cube = Instantiate(_cubePrefab, _inactiveCubesRoot);
                cube.name = $"Falling Cube {index + 1}";
                cube.Initialize(ReturnCube);
                cube.PrepareForPool();
                _availableCubes.Enqueue(cube);
            }
        }

        public bool TrySpawn(Vector3 position, Quaternion rotation, out FallingCube cube)
        {
            if (_availableCubes.Count == 0)
            {
                cube = null;
                return false;
            }

            cube = _availableCubes.Dequeue();
            cube.Spawn(position, rotation);
            return true;
        }

        private void ReturnCube(FallingCube cube)
        {
            cube.PrepareForPool();
            _availableCubes.Enqueue(cube);
        }
    }
}
