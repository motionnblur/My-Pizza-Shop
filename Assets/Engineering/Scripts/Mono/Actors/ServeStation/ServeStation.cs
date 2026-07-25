using System;
using System.Collections.Generic;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.CustomerQueue;
using Engineering.Scripts.Domain.ServeStation;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using Engineering.Scripts.Mono.Actors.CustomerQueue;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServeStation : MonoBehaviour
    {
        [SerializeField] private SServeStation sServeStation;
        [SerializeField] private SVoidEventChannel pizzaServedEvent;
        [SerializeField] private Transform[] queueSlots;
        [SerializeField] private GameObject pizzaVisualPrefab;
        [SerializeField] private BoxCollider plateCollider;
        [SerializeField, Min(0.01f)] private float pizzaStackSpacing = 0.14f;
        private CurrencyService _currencyService;

        private readonly List<CustomerBot> _customers = new List<CustomerBot>();
        private readonly List<GameObject> _pizzaVisuals = new List<GameObject>();
        private ServeStationModel _model;
        private CustomerQueueModel _queueModel;

        public int CustomerCount => _queueModel?.Count ?? 0;
        public int QueueCapacity => sServeStation != null ? sServeStation.maxQueueCustomers : 0;
        public int StoredPizzaCount => _model?.StoredPizzaCount ?? 0;

        public void Initialize(CurrencyService currencyService)
        {
            if (currencyService == null)
                throw new ArgumentNullException(nameof(currencyService));

            if (_currencyService != null)
            {
                if (_currencyService != currencyService)
                    throw new InvalidOperationException(
                        $"{nameof(ServeStation)}: already initialized with a different {nameof(CurrencyService)}.");
                return;
            }

            _currencyService = currencyService;
        }

        private void OnEnable()
        {
            TryPrepareModel();
            CreateVisualPool();
            RefreshVisuals();
        }

        private bool TryPrepareModel()
        {
            if (sServeStation == null)
                return false;

            if (_model == null)
            {
                _model = new ServeStationModel(sServeStation.maxStoredPizzas, sServeStation.pricePerPizza);
            }
            else
            {
                _model.UpdateConfiguration(sServeStation.maxStoredPizzas, sServeStation.pricePerPizza);
            }

            if (_queueModel == null)
            {
                _queueModel = new CustomerQueueModel(sServeStation.maxQueueCustomers);
            }
            else
            {
                _queueModel.UpdateCapacity(sServeStation.maxQueueCustomers);
            }

            return true;
        }

        public bool RegisterCustomer(CustomerBot customer)
        {
            if (customer == null || sServeStation == null)
                return false;

            if (!TryPrepareModel())
                return false;

            var orderModel = customer.OrderModel;
            if (orderModel == null)
                return false;

            var slotIndex = _customers.Count;
            if (queueSlots == null || slotIndex >= queueSlots.Length || queueSlots[slotIndex] == null)
                return false;

            var enqueueResult = _queueModel.TryEnqueue(orderModel);
            if (!enqueueResult.Accepted)
                return false;

            _customers.Add(customer);
            customer.AssignQueueSlot(queueSlots[slotIndex], slotIndex);
            return true;
        }

        public int TryDepositPizzas(PlayerPizzaInventory playerPizzaInventory)
        {
            if (playerPizzaInventory == null)
                return 0;

            if (!TryPrepareModel())
                return 0;

            var requestedAmount = _model.CalculateDepositAmount(playerPizzaInventory.Count);
            if (requestedAmount <= 0)
                return 0;

            var removedAmount = playerPizzaInventory.TryRemove(requestedAmount);
            if (removedAmount <= 0)
                return 0;

            var depositedAmount = _model.Deposit(removedAmount);
            RefreshVisuals();
            return depositedAmount;
        }

        public int TryServeFrontCustomer()
        {
            if (!TryPrepareModel())
                return 0;

            if (_currencyService == null || _currencyService.Wallet == null)
                return 0;

            if (_queueModel == null || !_queueModel.HasFront)
                return 0;

            var frontCustomer = _customers[0];
            if (!frontCustomer.HasReachedAssignedSlot)
                return 0;

            var frontOrder = _queueModel.Front;
            if (frontOrder == null)
                return 0;

            var result = _model.TryServe(frontOrder.RemainingPizzaCount);
            if (!result.HasDelivery)
                return 0;

            frontOrder.ReceivePizzas(result.DeliveredPizzaCount);
            RefreshVisuals();

            _currencyService.Credit(result.MoneyEarned);

            pizzaServedEvent?.Raise();

            if (result.OrderCompleted)
            {
                _queueModel.TryRemoveFront();
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

            return result.DeliveredPizzaCount;
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
                _pizzaVisuals[index].SetActive(index < StoredPizzaCount);
            }
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
