using System.Collections.Generic;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using Engineering.Engineering.Scripts.Mono.Actors.CustomerQueue;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServeStation : MonoBehaviour
    {
        [SerializeField] private SServeStation sServeStation;
        [SerializeField] private SVoidEventChannel pizzaServedEvent;
        [SerializeField] private Transform[] queueSlots;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField] private BoxCollider plateCollider;
        [SerializeField, Min(0.01f)] private float pizzaStackSpacing = 0.14f;

        private readonly List<CustomerBot> _customers = new List<CustomerBot>();
        private readonly List<GameObject> _pizzaVisuals = new List<GameObject>();
        private int _storedPizzaCount;

        public int CustomerCount => _customers.Count;
        public int QueueCapacity => sServeStation != null ? sServeStation.maxQueueCustomers : 0;
        public int StoredPizzaCount => _storedPizzaCount;

        private void OnEnable()
        {
            CreateVisualPool();
            RefreshVisuals();
        }

        public bool RegisterCustomer(CustomerBot customer)
        {
            if (customer == null || sServeStation == null)
                return false;

            if (_customers.Count >= sServeStation.maxQueueCustomers)
                return false;

            var slotIndex = _customers.Count;
            if (queueSlots == null || slotIndex >= queueSlots.Length || queueSlots[slotIndex] == null)
                return false;

            _customers.Add(customer);
            customer.AssignQueueSlot(queueSlots[slotIndex], slotIndex);
            return true;
        }

        public int TryDepositPizzas(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null || sServeStation == null)
                return 0;

            var playerCount = playerPizzaInventory.Count;
            if (playerCount <= 0)
                return 0;

            var spaceAvailable = sServeStation.maxStoredPizzas - _storedPizzaCount;
            if (spaceAvailable <= 0)
                return 0;

            var transferAmount = Mathf.Min(playerCount, spaceAvailable);
            var removedAmount = playerPizzaInventory.TryRemove(transferAmount);
            if (removedAmount <= 0)
                return 0;

            _storedPizzaCount += removedAmount;
            RefreshVisuals();
            return removedAmount;
        }

        public int TryServeFrontCustomer()
        {
            if (sServeStation == null)
                return 0;

            if (_customers.Count == 0)
                return 0;

            var frontCustomer = _customers[0];
            if (!frontCustomer.HasReachedAssignedSlot)
                return 0;

            if (_storedPizzaCount <= 0)
                return 0;

            var transferAmount = Mathf.Min(_storedPizzaCount, frontCustomer.RemainingPizzaCount);
            if (transferAmount <= 0)
                return 0;

            _storedPizzaCount -= transferAmount;
            frontCustomer.ReceivePizzas(transferAmount);
            RefreshVisuals();

            var moneyEarned = transferAmount * sServeStation.pricePerPizza;
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.AwardMoney(moneyEarned);

            pizzaServedEvent?.Raise();

            if (frontCustomer.RemainingPizzaCount <= 0)
            {
                _customers.RemoveAt(0);
                Destroy(frontCustomer.gameObject);

                if (queueSlots != null)
                {
                    for (var i = 0; i < _customers.Count; i++)
                    {
                        if (i < queueSlots.Length && queueSlots[i] != null)
                            _customers[i].AssignQueueSlot(queueSlots[i], i);
                    }
                }
            }

            return transferAmount;
        }

        private void CreateVisualPool()
        {
            if (_pizzaVisuals.Count > 0 || sServeStation == null || pizzaVisualPrefab == null)
                return;

            for (var index = 0; index < sServeStation.maxStoredPizzas; index++)
            {
                var pizzaVisual = Instantiate(pizzaVisualPrefab, transform);
                pizzaVisual.SetActive(false);
                _pizzaVisuals.Add(pizzaVisual);
            }
        }

        private void RefreshVisuals()
        {
            var basePos = PizzaStackBasePosition;

            for (var index = 0; index < _pizzaVisuals.Count; index++)
            {
                if (_pizzaVisuals[index] == null)
                    continue;

                _pizzaVisuals[index].transform.position = basePos + Vector3.up * (index * pizzaStackSpacing);
                _pizzaVisuals[index].transform.rotation = Quaternion.identity;
                _pizzaVisuals[index].SetActive(index < _storedPizzaCount);
            }
        }
        
        public void TryServePizzas(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            TryServeFrontCustomer();
        }
        
        private Vector3 PizzaStackBasePosition
        {
            get
            {
                if (plateCollider == null)
                    return transform.position;

                var bounds = plateCollider.bounds;
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }
    }
}
