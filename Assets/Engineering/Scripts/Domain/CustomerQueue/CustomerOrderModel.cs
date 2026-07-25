using System;

namespace Engineering.Scripts.Domain.CustomerQueue
{
    public sealed class CustomerOrderModel
    {
        private readonly int _initialPizzaCount;
        private int _remainingPizzaCount;

        public int InitialPizzaCount => _initialPizzaCount;
        public int RemainingPizzaCount => _remainingPizzaCount;
        public bool IsCompleted => _remainingPizzaCount <= 0;

        public CustomerOrderModel(int pizzaCount)
        {
            if (pizzaCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(pizzaCount), "Order must have a positive pizza count.");

            _initialPizzaCount = pizzaCount;
            _remainingPizzaCount = pizzaCount;
        }

        public int ReceivePizzas(int amount)
        {
            if (amount <= 0)
                return 0;

            var accepted = Math.Min(amount, _remainingPizzaCount);
            _remainingPizzaCount -= accepted;
            return accepted;
        }
    }
}
