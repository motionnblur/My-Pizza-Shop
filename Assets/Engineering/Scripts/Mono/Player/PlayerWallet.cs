using System;
using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int money = 100;
        [SerializeField] private Transform moneyAnimationOrigin;

        public int Money => money;

        public event Action<int> BalanceChanged;

        public Transform MoneyAnimationOrigin => moneyAnimationOrigin != null
            ? moneyAnimationOrigin
            : transform;

        public Vector3 MoneyAnimationOriginPosition => MoneyAnimationOrigin.position;

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return false;
            if (money < amount) return false;

            money -= amount;
            BalanceChanged?.Invoke(money);
            return true;
        }

        public bool Credit(int amount)
        {
            if (amount <= 0) return false;

            money += amount;
            BalanceChanged?.Invoke(money);
            return true;
        }
    }
}
