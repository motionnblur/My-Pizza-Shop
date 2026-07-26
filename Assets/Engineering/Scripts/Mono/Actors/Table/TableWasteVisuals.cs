using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Engineering.Scripts.Mono.Actors.Table
{
    public class TableWasteVisuals : MonoBehaviour
    {
        [SerializeField] private GameObject leftoverPrefab;
        [SerializeField] private Transform leftoverStackAnchor;
        [SerializeField, Min(0.01f)] private float leftoverStackSpacing = 0.14f;

        private ObjectPool<GameObject> _leftoverPool;
        private readonly List<GameObject> _activeVisuals = new();
        private Vector3 _originalScale = Vector3.one;
        private bool _poolInitialized;

        private const int PoolDefaultCapacity = 10;
        private const int PoolMaxSize = 20;

        private void Awake()
        {
            if (leftoverPrefab != null)
                _originalScale = leftoverPrefab.transform.localScale;
        }

        private void EnsurePoolInitialized()
        {
            if (_poolInitialized)
                return;

            if (leftoverPrefab == null)
            {
                Debug.LogWarning($"TableWasteVisuals on '{name}': leftoverPrefab is not assigned. " +
                    "Leftover visuals will not be shown.", this);
                return;
            }

            _leftoverPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var obj = Instantiate(leftoverPrefab);
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: null,
                actionOnRelease: OnReleaseVisual,
                actionOnDestroy: obj => Destroy(obj),
                defaultCapacity: PoolDefaultCapacity,
                maxSize: PoolMaxSize);

            _poolInitialized = true;
        }

        private static void OnReleaseVisual(GameObject visual)
        {
            if (visual != null)
                visual.SetActive(false);
        }

        public void Refresh(int leftoverCount)
        {
            EnsurePoolInitialized();

            if (leftoverPrefab == null)
                return;

            var anchor = leftoverStackAnchor != null ? leftoverStackAnchor : transform;

            while (_activeVisuals.Count < leftoverCount)
            {
                var visual = _leftoverPool.Get();
                visual.SetActive(true);
                _activeVisuals.Add(visual);
            }

            while (_activeVisuals.Count > leftoverCount)
            {
                var lastIdx = _activeVisuals.Count - 1;
                var visual = _activeVisuals[lastIdx];
                _activeVisuals.RemoveAt(lastIdx);
                _leftoverPool.Release(visual);
            }

            for (var i = 0; i < _activeVisuals.Count; i++)
            {
                var visual = _activeVisuals[i];
                visual.transform.position = anchor.position + Vector3.up * (i * leftoverStackSpacing);
                visual.transform.rotation = Quaternion.identity;
                visual.transform.localScale = _originalScale;
            }
        }

        public void Clear()
        {
            foreach (var visual in _activeVisuals)
            {
                if (visual != null)
                    _leftoverPool?.Release(visual);
            }
            _activeVisuals.Clear();
        }

        private void OnDestroy()
        {
            Clear();
            if (_leftoverPool != null)
            {
                _leftoverPool.Clear();
                _leftoverPool.Dispose();
                _leftoverPool = null;
            }
        }
    }
}
