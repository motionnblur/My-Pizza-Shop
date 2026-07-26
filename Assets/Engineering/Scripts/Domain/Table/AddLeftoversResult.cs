namespace Engineering.Scripts.Domain.Table
{
    public enum AddLeftoverFailureReason
    {
        None,
        InvalidAmount,
        CapacityExceeded
    }

    public readonly struct AddLeftoversResult
    {
        public bool Added { get; }
        public int CurrentCount { get; }
        public AddLeftoverFailureReason Reason { get; }

        public AddLeftoversResult(bool added, int currentCount)
        {
            Added = added;
            CurrentCount = currentCount;
            Reason = AddLeftoverFailureReason.None;
        }

        private AddLeftoversResult(bool added, int currentCount, AddLeftoverFailureReason reason)
        {
            Added = added;
            CurrentCount = currentCount;
            Reason = reason;
        }

        public static AddLeftoversResult InvalidAmount => new AddLeftoversResult(false, -1, AddLeftoverFailureReason.InvalidAmount);
        public static AddLeftoversResult CapacityExceeded => new AddLeftoversResult(false, -1, AddLeftoverFailureReason.CapacityExceeded);
    }
}
