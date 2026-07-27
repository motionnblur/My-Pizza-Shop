using System;

namespace Engineering.Scripts.Domain.Inventory
{
    public sealed class WasteInventoryModel
    {
        private readonly int _capacity;
        private int _count;

        public int Capacity => _capacity;
        public int Count => _count;
        public int RemainingCapacity => _capacity - _count;
        public bool IsFull => _count >= _capacity;

        public event Action<int> Changed;

        public WasteInventoryModel(int capacity)
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

        public void Clear()
        {
            if (_count <= 0)
                return;

            _count = 0;
            Changed?.Invoke(_count);
        }
    }
}
