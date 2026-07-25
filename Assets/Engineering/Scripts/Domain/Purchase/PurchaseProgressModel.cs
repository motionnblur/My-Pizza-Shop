using System;

namespace Engineering.Scripts.Domain.Purchase
{
    public sealed class PurchaseProgressModel
    {
        private readonly int _unlockPrice;
        private int _paidAmount;
        private bool _isPurchased;

        public int UnlockPrice => _unlockPrice;
        public int PaidAmount => _paidAmount;
        public int RemainingAmount => Math.Max(0, _unlockPrice - _paidAmount);
        public bool IsPurchased => _isPurchased;

        public PurchaseProgressModel(int unlockPrice)
        {
            if (unlockPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(unlockPrice), "unlockPrice must be positive.");

            _unlockPrice = unlockPrice;
            _paidAmount = 0;
            _isPurchased = false;
        }

        public PurchaseProgressResult ApplyPayment(int amount)
        {
            if (amount <= 0)
                return PurchaseProgressResult.None;

            if (_isPurchased)
                return PurchaseProgressResult.None;

            _paidAmount += amount;

            var completedNow = !_isPurchased && _paidAmount >= _unlockPrice;
            if (completedNow)
                _isPurchased = true;

            return new PurchaseProgressResult(
                appliedAmount: amount,
                totalPaidAmount: _paidAmount,
                remainingAmount: RemainingAmount,
                completedNow: completedNow,
                isPurchased: _isPurchased);
        }
    }
}
