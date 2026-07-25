namespace Engineering.Scripts.Domain.CustomerQueue
{
    public readonly struct RemoveFrontResult
    {
        public CustomerOrderModel RemovedOrder { get; }
        public bool HasRemoved => RemovedOrder != null;

        public RemoveFrontResult(CustomerOrderModel removedOrder)
        {
            RemovedOrder = removedOrder;
        }

        public static RemoveFrontResult Empty => new RemoveFrontResult(null);
    }
}
