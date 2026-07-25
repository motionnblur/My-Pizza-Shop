using System;

namespace Engineering.Scripts.Domain.GrillStation
{
    public sealed class GrillStationModel
    {
        private int _maxReadyPizzas;
        private int _readyPizzaCount;

        public int ReadyPizzaCount => _readyPizzaCount;
        public int MaxReadyPizzas => _maxReadyPizzas;
        public bool CanProduce => _readyPizzaCount < _maxReadyPizzas;

        public GrillStationModel(int maxReadyPizzas)
        {
            UpdateConfiguration(maxReadyPizzas);
            _readyPizzaCount = 0;
        }

        public void UpdateConfiguration(int maxReadyPizzas)
        {
            if (maxReadyPizzas < 0)
                throw new ArgumentOutOfRangeException(nameof(maxReadyPizzas), "maxReadyPizzas must not be negative.");

            _maxReadyPizzas = maxReadyPizzas;
        }

        public bool TryProduceOne()
        {
            if (!CanProduce)
                return false;

            _readyPizzaCount++;
            return true;
        }

        public int RemoveReady(int amount)
        {
            if (amount <= 0)
                return 0;

            var removedAmount = Math.Min(amount, _readyPizzaCount);
            _readyPizzaCount -= removedAmount;
            return removedAmount;
        }
    }
}
