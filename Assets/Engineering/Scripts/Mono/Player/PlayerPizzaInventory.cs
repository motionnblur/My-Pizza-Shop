using System.Collections.Generic;
using Engineering.ScriptableObjects;
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
        private int _count;

        public int Count => _count;
        public int Capacity => capacity;

        private void Awake()
        {
            CreateVisualPool();
            RefreshVisuals();
        }

        private void Start()
        {
            pizzaInventoryChangedEvent?.Raise(_count);
        }

        public int TryAdd(int requestedAmount)
        {
            if (requestedAmount <= 0)
                return 0;

            var acceptedAmount = Mathf.Min(requestedAmount, capacity - _count);
            if (acceptedAmount <= 0)
                return 0;

            _count += acceptedAmount;
            RefreshVisuals();
            pizzaInventoryChangedEvent?.Raise(_count);
            return acceptedAmount;
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
            for (var index = 0; index < _pizzaVisuals.Count; index++)
                _pizzaVisuals[index].SetActive(index < _count);
        }
    }
}
