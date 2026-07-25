using System;

namespace Engineering.Scripts.Domain.Economy
{
    public sealed class WalletModel
    {
        private int _money;

        public int Money => _money;

        public event Action<int> BalanceChanged;

        public WalletModel(int startingMoney)
        {
            if (startingMoney < 0)
                throw new ArgumentOutOfRangeException(nameof(startingMoney), "startingMoney must not be negative.");

            _money = startingMoney;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return false;
            if (_money < amount) return false;

            _money -= amount;
            BalanceChanged?.Invoke(_money);
            return true;
        }

        public bool Credit(int amount)
        {
            if (amount <= 0) return false;

            _money += amount;
            BalanceChanged?.Invoke(_money);
            return true;
        }
    }
}
