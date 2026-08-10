using System;
using System.Collections;
using UnityEngine;

namespace RainOfCubes
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(Renderer))]
    public sealed class FallingCube : MonoBehaviour
    {
        private const float OppositeHueOffset = 0.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Dependencies")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Renderer _renderer;

        [Header("Lifetime")]
        [SerializeField, Min(0f)] private float _minimumLifetime = 2f;
        [SerializeField, Min(0f)] private float _maximumLifetime = 5f;

        [Header("Colors")]
        [SerializeField] private Color _initialColor = Color.white;
        [SerializeField, Range(0.1f, 1f)] private float _collisionColorSaturation = 0.75f;
        [SerializeField, Range(0.1f, 1f)] private float _collisionColorBrightness = 0.95f;

        private Action<FallingCube> _returnToPool;
        private MaterialPropertyBlock _materialPropertyBlock;
        private Coroutine _lifetimeCoroutine;
        private bool _hasTouchedPlatform;
        private bool _isSpawned;
        private float _selectedLifetime;
        private Color _currentColor;

        public bool HasTouchedPlatform => _hasTouchedPlatform;
        public bool IsSpawned => _isSpawned;
        public float SelectedLifetime => _selectedLifetime;
        public Color CurrentColor => _currentColor;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            SetColor(_initialColor);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isSpawned || _hasTouchedPlatform)
            {
                return;
            }

            if (!collision.gameObject.TryGetComponent<PlatformSurface>(out _))
            {
                return;
            }

            _hasTouchedPlatform = true;
            SetColor(CreateCollisionColor());

            _selectedLifetime = UnityEngine.Random.Range(_minimumLifetime, _maximumLifetime);
            _lifetimeCoroutine = StartCoroutine(ReturnAfterLifetime(_selectedLifetime));
        }

        private void OnValidate()
        {
            _maximumLifetime = Mathf.Max(_minimumLifetime, _maximumLifetime);
        }

        internal void Initialize(Action<FallingCube> returnToPool)
        {
            _returnToPool = returnToPool ?? throw new ArgumentNullException(nameof(returnToPool));
        }

        internal void Spawn(Vector3 position, Quaternion rotation)
        {
            StopLifetimeCoroutine();

            _hasTouchedPlatform = false;
            _isSpawned = true;
            _selectedLifetime = 0f;

            transform.SetPositionAndRotation(position, rotation);
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            SetColor(_initialColor);

            gameObject.SetActive(true);
            _rigidbody.WakeUp();
        }

        internal void PrepareForPool()
        {
            StopLifetimeCoroutine();

            _hasTouchedPlatform = false;
            _isSpawned = false;
            _selectedLifetime = 0f;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.Sleep();
            SetColor(_initialColor);

            gameObject.SetActive(false);
        }

        private IEnumerator ReturnAfterLifetime(float lifetime)
        {
            yield return new WaitForSeconds(lifetime);

            _lifetimeCoroutine = null;
            RequestReturnToPool();
        }

        private void RequestReturnToPool()
        {
            if (!_isSpawned)
            {
                return;
            }

            _isSpawned = false;
            _returnToPool?.Invoke(this);
        }

        private void StopLifetimeCoroutine()
        {
            if (_lifetimeCoroutine == null)
            {
                return;
            }

            StopCoroutine(_lifetimeCoroutine);
            _lifetimeCoroutine = null;
        }

        private Color CreateCollisionColor()
        {
            float hue = UnityEngine.Random.value;
            Color collisionColor = Color.HSVToRGB(
                hue,
                _collisionColorSaturation,
                _collisionColorBrightness);
            collisionColor.a = _initialColor.a;

            if (AreColorsApproximatelyEqual(collisionColor, _initialColor))
            {
                hue = Mathf.Repeat(hue + OppositeHueOffset, 1f);
                collisionColor = Color.HSVToRGB(
                    hue,
                    _collisionColorSaturation,
                    _collisionColorBrightness);
                collisionColor.a = _initialColor.a;
            }

            return collisionColor;
        }

        private void SetColor(Color color)
        {
            _currentColor = color;
            _materialPropertyBlock.Clear();
            _materialPropertyBlock.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private static bool AreColorsApproximatelyEqual(Color first, Color second)
        {
            return Mathf.Approximately(first.r, second.r)
                && Mathf.Approximately(first.g, second.g)
                && Mathf.Approximately(first.b, second.b)
                && Mathf.Approximately(first.a, second.a);
        }
    }
}
