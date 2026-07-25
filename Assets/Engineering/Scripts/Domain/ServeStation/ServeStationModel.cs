using System;

namespace Engineering.Scripts.Domain.ServeStation
{
    public sealed class ServeStationModel
    {
        private int _storedPizzaCount;
        private int _maxStoredPizzas;
        private int _pricePerPizza;

        public int StoredPizzaCount => _storedPizzaCount;
        public int MaxStoredPizzas => _maxStoredPizzas;
        public int PricePerPizza => _pricePerPizza;
        public int RemainingCapacity => Math.Max(0, _maxStoredPizzas - _storedPizzaCount);

        public ServeStationModel(int maxStoredPizzas, int pricePerPizza)
        {
            UpdateConfiguration(maxStoredPizzas, pricePerPizza);
            _storedPizzaCount = 0;
        }

        public void UpdateConfiguration(int maxStoredPizzas, int pricePerPizza)
        {
            if (maxStoredPizzas < 0)
                throw new ArgumentOutOfRangeException(nameof(maxStoredPizzas), "maxStoredPizzas must not be negative.");
            if (pricePerPizza < 0)
                throw new ArgumentOutOfRangeException(nameof(pricePerPizza), "pricePerPizza must not be negative.");

            _maxStoredPizzas = maxStoredPizzas;
            _pricePerPizza = pricePerPizza;
        }

        public int CalculateDepositAmount(int availablePizzaCount)
        {
            if (availablePizzaCount <= 0)
                return 0;

            var remaining = RemainingCapacity;
            if (remaining <= 0)
                return 0;

            return Math.Min(availablePizzaCount, remaining);
        }

        public int Deposit(int pizzaCount)
        {
            var accepted = CalculateDepositAmount(pizzaCount);
            _storedPizzaCount += accepted;
            return accepted;
        }

        public ServeResult TryServe(int remainingOrder)
        {
            if (remainingOrder <= 0)
                return ServeResult.None;

            if (_storedPizzaCount <= 0)
                return ServeResult.None;

            var delivered = Math.Min(_storedPizzaCount, remainingOrder);
            _storedPizzaCount -= delivered;

            var moneyEarned = delivered * _pricePerPizza;
            var orderCompleted = delivered > 0 && delivered >= remainingOrder;

            return new ServeResult(delivered, moneyEarned, orderCompleted);
        }
    }
}
