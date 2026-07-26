namespace Engineering.Scripts.Domain.Table
{
    public readonly struct AddLeftoversResult
    {
        public bool Added { get; }
        public int CurrentCount { get; }

        public AddLeftoversResult(bool added, int currentCount)
        {
            Added = added;
            CurrentCount = currentCount;
        }

        public static AddLeftoversResult InvalidAmount => new AddLeftoversResult(false, -1);
    }
}
