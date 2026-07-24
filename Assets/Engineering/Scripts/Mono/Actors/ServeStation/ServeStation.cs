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
        [SerializeField] private ServePlate servePlate;
        [SerializeField] private SVoidEventChannel pizzaServedEvent;
        [SerializeField] private Transform[] queueSlots;

        private readonly List<CustomerBot> _customers = new List<CustomerBot>();

        public int CustomerCount => _customers.Count;
        public int QueueCapacity => sServeStation != null ? sServeStation.maxQueueCustomers : 0;

        public bool RegisterCustomer(CustomerBot customer)
        {
            if (customer == null || sServeStation == null)
                return false;

            if (_customers.Count >= sServeStation.maxQueueCustomers)
                return false;

            var slotIndex = _customers.Count;
            if (slotIndex >= queueSlots.Length || queueSlots[slotIndex] == null)
                return false;

            _customers.Add(customer);
            customer.AssignQueueSlot(queueSlots[slotIndex]);
            return true;
        }

        public int TryServeFrontCustomer(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null || sServeStation == null)
                return 0;

            if (_customers.Count == 0)
                return 0;

            var frontCustomer = _customers[0];
            if (!frontCustomer.HasReachedAssignedSlot)
                return 0;

            var playerPizzaCount = playerPizzaInventory.Count;
            if (playerPizzaCount <= 0)
                return 0;

            var transferAmount = Mathf.Min(playerPizzaCount, frontCustomer.RemainingPizzaCount);
            if (transferAmount <= 0)
                return 0;

            var removedAmount = playerPizzaInventory.TryRemove(transferAmount);
            if (removedAmount <= 0)
                return 0;

            frontCustomer.ReceivePizzas(removedAmount);

            var moneyEarned = removedAmount * sServeStation.pricePerPizza;
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.AwardMoney(moneyEarned);

            pizzaServedEvent?.Raise();

            if (frontCustomer.RemainingPizzaCount <= 0)
            {
                _customers.RemoveAt(0);
                Destroy(frontCustomer.gameObject);

                for (var i = 0; i < _customers.Count; i++)
                {
                    if (i < queueSlots.Length && queueSlots[i] != null)
                        _customers[i].AssignQueueSlot(queueSlots[i]);
                }
            }

            return removedAmount;
        }
    }
}
