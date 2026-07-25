using System;
using System.Collections.Generic;

namespace Engineering.Scripts.Domain.CustomerQueue
{
    public sealed class CustomerQueueModel
    {
        private readonly Queue<CustomerOrderModel> _orders = new Queue<CustomerOrderModel>();
        private int _capacity;

        public int Count => _orders.Count;
        public int Capacity => _capacity;
        public bool IsFull => _orders.Count >= _capacity;
        public bool HasFront => _orders.Count > 0;
        public CustomerOrderModel Front => HasFront ? _orders.Peek() : null;

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

            _orders.Enqueue(order);
            return new EnqueueResult(true, _orders.Count);
        }

        public RemoveFrontResult TryRemoveFront()
        {
            if (_orders.Count == 0)
                return RemoveFrontResult.Empty;

            var front = _orders.Dequeue();
            return new RemoveFrontResult(front);
        }
    }
}
