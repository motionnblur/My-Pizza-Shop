using System;
using System.Collections.Generic;

namespace Engineering.Scripts.Domain.CustomerQueue
{
    public sealed class CustomerQueueModel
    {
        private readonly List<CustomerOrderModel> _orders = new List<CustomerOrderModel>();
        private int _capacity;

        public int Count => _orders.Count;
        public int Capacity => _capacity;
        public bool IsFull => _orders.Count >= _capacity;
        public bool HasFront => _orders.Count > 0;
        public CustomerOrderModel Front => HasFront ? _orders[0] : null;

        public CustomerQueueModel(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be positive.");

            _capacity = capacity;
        }

        public void UpdateCapacity(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be positive.");

            _capacity = capacity;
        }

        public EnqueueResult TryEnqueue(CustomerOrderModel order)
        {
            if (order == null)
                return EnqueueResult.Rejected;

            if (_orders.Count >= _capacity)
                return EnqueueResult.Rejected;

            _orders.Add(order);
            return new EnqueueResult(true, _orders.Count - 1);
        }

        public RemoveFrontResult TryRemoveFront()
        {
            if (_orders.Count == 0)
                return RemoveFrontResult.Empty;

            var front = _orders[0];
            _orders.RemoveAt(0);
            return new RemoveFrontResult(front);
        }

        public RemoveFrontResult RemoveAt(int index)
        {
            if (index < 0 || index >= _orders.Count)
                return RemoveFrontResult.Empty;

            var removed = _orders[index];
            _orders.RemoveAt(index);
            return new RemoveFrontResult(removed);
        }
    }
}
