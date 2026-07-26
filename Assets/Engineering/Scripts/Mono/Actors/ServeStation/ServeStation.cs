using System;
using Engineering.ScriptableObjects;
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
        [SerializeField] private CustomerQueueController queueController;
        [SerializeField] private ServeStationVisuals stationVisuals;
        private CurrencyService _currencyService;

        private ServeStationModel _model;

        public int CustomerCount => queueController != null ? queueController.CustomerCount : 0;
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
            if (sServeStation == null)
                return;

            TryPrepareModel();

            if (queueController != null)
            {
                queueController.Initialize(sServeStation.maxQueueCustomers);
            }

            if (stationVisuals != null)
            {
                stationVisuals.Initialize(sServeStation.maxStoredPizzas);
                stationVisuals.Refresh(StoredPizzaCount);
            }
        }

        private void TryPrepareModel()
        {
            if (_model == null)
            {
                _model = new ServeStationModel(sServeStation.maxStoredPizzas, sServeStation.pricePerPizza);
            }
            else
            {
                _model.UpdateConfiguration(sServeStation.maxStoredPizzas, sServeStation.pricePerPizza);
            }
        }

        public bool TryRegisterCustomer(CustomerBot customer)
        {
            if (customer == null || sServeStation == null)
                return false;

            if (_model == null)
                return false;

            if (queueController == null)
                return false;

            return queueController.TryRegister(customer);
        }

        public int DepositFrom(PlayerPizzaInventory inventory)
        {
            if (inventory == null)
                return 0;

            if (_model == null)
                return 0;

            var requestedAmount = _model.CalculateDepositAmount(inventory.Count);
            if (requestedAmount <= 0)
                return 0;

            var removedAmount = inventory.TryRemove(requestedAmount);
            if (removedAmount <= 0)
                return 0;

            var depositedAmount = _model.Deposit(removedAmount);

            if (stationVisuals != null)
                stationVisuals.Refresh(StoredPizzaCount);

            return depositedAmount;
        }

        public int ServeFrontCustomer()
        {
            if (_model == null)
                return 0;

            if (_currencyService == null || _currencyService.Wallet == null)
                return 0;

            if (queueController == null || queueController.FrontCustomer == null)
                return 0;

            var frontCustomer = queueController.FrontCustomer;
            if (!frontCustomer.HasReachedAssignedSlot)
                return 0;

            var frontOrder = frontCustomer.OrderModel;
            if (frontOrder == null)
                return 0;

            var result = _model.TryServe(frontOrder.RemainingPizzaCount);
            if (!result.HasDelivery)
                return 0;

            frontOrder.ReceivePizzas(result.DeliveredPizzaCount);

            if (stationVisuals != null)
                stationVisuals.Refresh(StoredPizzaCount);

            _currencyService.Credit(result.MoneyEarned);

            pizzaServedEvent?.Raise();

            if (result.OrderCompleted)
            {
                queueController.RemoveFrontCustomer();
            }

            return result.DeliveredPizzaCount;
        }
    }
}
