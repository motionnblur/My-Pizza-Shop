namespace Engineering.Scripts.Domain.Purchase
{
    public readonly struct PurchaseProgressResult
    {
        public int AppliedAmount { get; }
        public int TotalPaidAmount { get; }
        public int RemainingAmount { get; }
        public bool CompletedNow { get; }
        public bool IsPurchased { get; }
        public bool HasAppliedPayment => AppliedAmount > 0;

        public PurchaseProgressResult(int appliedAmount, int totalPaidAmount, int remainingAmount, bool completedNow, bool isPurchased)
        {
            AppliedAmount = appliedAmount;
            TotalPaidAmount = totalPaidAmount;
            RemainingAmount = remainingAmount;
            CompletedNow = completedNow;
            IsPurchased = isPurchased;
        }

        public static PurchaseProgressResult None => new PurchaseProgressResult(0, 0, 0, false, false);
    }
}
