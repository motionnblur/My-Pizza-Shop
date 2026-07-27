namespace Engineering.Scripts.Domain.Table
{
    public sealed class TableWasteModel
    {
        private int _leftoverCount;
        private readonly int _maxLeftovers;

        public int LeftoverCount => _leftoverCount;
        public int MaxLeftovers => _maxLeftovers;
        public bool IsFull => _leftoverCount >= _maxLeftovers;
        public int RemainingCapacity => _maxLeftovers - _leftoverCount;

        public TableWasteModel(int maxLeftovers = 10)
        {
            _maxLeftovers = maxLeftovers > 0 ? maxLeftovers : 10;
        }

        public bool CanAdd(int amount)
        {
            return amount > 0 && _leftoverCount + amount <= _maxLeftovers;
        }

        public AddLeftoversResult TryAddLeftovers(int count)
        {
            if (count <= 0)
                return AddLeftoversResult.InvalidAmount;

            if (_leftoverCount + count > _maxLeftovers)
                return AddLeftoversResult.CapacityExceeded;

            _leftoverCount += count;
            return new AddLeftoversResult(true, _leftoverCount);
        }

        public int TryRemoveLeftovers(int requestedAmount)
        {
            if (requestedAmount <= 0)
                return 0;

            var removedAmount = _leftoverCount < requestedAmount ? _leftoverCount : requestedAmount;
            if (removedAmount <= 0)
                return 0;

            _leftoverCount -= removedAmount;
            return removedAmount;
        }

        public void Clear()
        {
            _leftoverCount = 0;
        }
    }
}
