using System.Collections.Generic;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.Inventory;
using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerWasteInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 10;
        [SerializeField] private Transform wasteStackAnchor;
        [SerializeField] private GameObject leftoverVisualPrefab;
        [SerializeField, Min(0.01f)] private float wasteStackSpacing = 0.14f;
        [SerializeField] private SIntEventChannel wasteInventoryChangedEvent;

        private readonly List<GameObject> _wasteVisuals = new();
        private WasteInventoryModel _model;

        public int Count
        {
            get
            {
                EnsureModel();
                return _model.Count;
            }
        }

        public int Capacity => capacity;
        public int RemainingCapacity
        {
            get
            {
                EnsureModel();
                return _model.RemainingCapacity;
            }
        }

        public Transform WasteStackAnchor => wasteStackAnchor;
        public float WasteStackSpacing => wasteStackSpacing;

        private void Awake()
        {
            CreateVisualPool();
            RefreshVisuals();
        }

        private void Start()
        {
            wasteInventoryChangedEvent?.Raise(Count);
        }

        public int TryAdd(int requestedAmount)
        {
            EnsureModel();
            return _model.TryAdd(requestedAmount);
        }

        public int TryRemove(int requestedAmount)
        {
            EnsureModel();
            return _model.TryRemove(requestedAmount);
        }

        public void Clear()
        {
            EnsureModel();
            _model.Clear();
        }

        private void EnsureModel()
        {
            if (_model != null)
                return;

            _model = new WasteInventoryModel(capacity);
            _model.Changed += OnModelChanged;
        }

        private void OnModelChanged(int count)
        {
            RefreshVisuals();
            wasteInventoryChangedEvent?.Raise(count);
        }

        private void CreateVisualPool()
        {
            if (leftoverVisualPrefab == null || wasteStackAnchor == null)
                return;

            for (var index = 0; index < capacity; index++)
            {
                var visual = Instantiate(leftoverVisualPrefab, wasteStackAnchor);
                visual.transform.localPosition = Vector3.up * (index * wasteStackSpacing);
                visual.transform.localRotation = Quaternion.identity;
                visual.SetActive(false);
                _wasteVisuals.Add(visual);
            }
        }

        private void RefreshVisuals()
        {
            var count = _model != null ? _model.Count : 0;

            for (var index = 0; index < _wasteVisuals.Count; index++)
                _wasteVisuals[index].SetActive(index < count);
        }
    }
}
