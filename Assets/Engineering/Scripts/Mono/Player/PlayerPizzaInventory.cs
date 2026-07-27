using System.Collections.Generic;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.Inventory;
using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerPizzaInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 10;
        [SerializeField] private Transform pizzaStackAnchor;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField, Min(0.01f)] private float pizzaStackSpacing = 0.14f;
        [SerializeField] private SIntEventChannel pizzaInventoryChangedEvent;

        private readonly List<GameObject> _pizzaVisuals = new List<GameObject>();
        private PizzaInventoryModel _model;
        private PlayerWasteInventory _cachedWasteInventory;
        private bool _wasteInventoryResolved;

        public int Count
        {
            get
            {
                EnsureModel();
                return _model.Count;
            }
        }

        public int Capacity => capacity;
        public Transform PizzaStackAnchor => pizzaStackAnchor;
        public float PizzaStackSpacing => pizzaStackSpacing;

        private void Awake()
        {
            ResolveWasteInventory();
            CreateVisualPool();
            RefreshVisuals();
        }

        private void ResolveWasteInventory()
        {
            if (_wasteInventoryResolved)
                return;

            _cachedWasteInventory = GetComponent<PlayerWasteInventory>();
            _wasteInventoryResolved = true;
        }

        private void Start()
        {
            pizzaInventoryChangedEvent?.Raise(Count);
        }

        public int TryAdd(int requestedAmount)
        {
            ResolveWasteInventory();

            if (_cachedWasteInventory != null && _cachedWasteInventory.Count > 0)
                return 0;

            EnsureModel();
            return _model.TryAdd(requestedAmount);
        }

        public int TryRemove(int requestedAmount)
        {
            EnsureModel();
            return _model.TryRemove(requestedAmount);
        }

        private void EnsureModel()
        {
            if (_model != null)
                return;

            _model = new PizzaInventoryModel(capacity);
            _model.Changed += OnModelChanged;
        }

        private void OnModelChanged(int count)
        {
            RefreshVisuals();
            pizzaInventoryChangedEvent?.Raise(count);
        }

        private void CreateVisualPool()
        {
            if (pizzaVisualPrefab == null || pizzaStackAnchor == null)
                return;

            for (var index = 0; index < capacity; index++)
            {
                var pizzaVisual = Instantiate(pizzaVisualPrefab, pizzaStackAnchor);
                pizzaVisual.transform.localPosition = Vector3.up * (index * pizzaStackSpacing);
                pizzaVisual.transform.localRotation = Quaternion.identity;
                pizzaVisual.SetActive(false);
                _pizzaVisuals.Add(pizzaVisual);
            }
        }

        private void RefreshVisuals()
        {
            var count = _model != null ? _model.Count : 0;

            for (var index = 0; index < _pizzaVisuals.Count; index++)
                _pizzaVisuals[index].SetActive(index < count);
        }
    }
}
