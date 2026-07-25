using System;

namespace Engineering.Scripts.Domain.Payment
{
    public sealed class PaymentSessionModel
    {
        private readonly int _paymentPerTick;

        public int PaymentPerTick => _paymentPerTick;
        public bool IsActive { get; private set; }

        public PaymentSessionModel(int paymentPerTick)
        {
            if (paymentPerTick <= 0)
                throw new ArgumentOutOfRangeException(nameof(paymentPerTick), "paymentPerTick must be positive.");

            _paymentPerTick = paymentPerTick;
            IsActive = false;
        }

        public void Begin()
        {
            IsActive = true;
        }

        public void Cancel()
        {
            IsActive = false;
        }

        public int GetNextPayment()
        {
            return IsActive ? _paymentPerTick : 0;
        }
    }
}
