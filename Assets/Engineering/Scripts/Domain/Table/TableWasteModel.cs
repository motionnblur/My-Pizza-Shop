namespace Engineering.Scripts.Domain.Table
{
    public sealed class TableWasteModel
    {
        private int _leftoverCount;

        public int LeftoverCount => _leftoverCount;

        public AddLeftoversResult TryAddLeftovers(int count)
        {
            if (count <= 0)
                return AddLeftoversResult.InvalidAmount;

            _leftoverCount += count;
            return new AddLeftoversResult(true, _leftoverCount);
        }

        public void Clear()
        {
            _leftoverCount = 0;
        }
    }
}
