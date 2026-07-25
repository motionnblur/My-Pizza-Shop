using System;

namespace Engineering.Scripts.Domain.Inventory
{
    public sealed class PizzaInventoryModel
    {
        private readonly int _capacity;
        private int _count;

        public int Capacity => _capacity;
        public int Count => _count;

        public event Action<int> Changed;

        public PizzaInventoryModel(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must not be negative.");

            _capacity = capacity;
            _count = 0;
        }

        public int TryAdd(int requestedAmount)
        {
            if (requestedAmount <= 0)
                return 0;

            var acceptedAmount = Math.Min(requestedAmount, _capacity - _count);
            if (acceptedAmount <= 0)
                return 0;

            _count += acceptedAmount;
            Changed?.Invoke(_count);
            return acceptedAmount;
        }

        public int TryRemove(int requestedAmount)
        {
            if (requestedAmount <= 0)
                return 0;

            var removedAmount = Math.Min(requestedAmount, _count);
            if (removedAmount <= 0)
                return 0;

            _count -= removedAmount;
            Changed?.Invoke(_count);
            return removedAmount;
        }
    }
}
