using System;
using Engineering.Scripts.Domain.Economy;
using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerWallet : MonoBehaviour
    {
        [SerializeField] private int money = 100;
        [SerializeField] private Transform moneyAnimationOrigin;

        private WalletModel _model;

        public int Money
        {
            get
            {
                EnsureModel();
                return _model.Money;
            }
        }

        public event Action<int> BalanceChanged;

        public Transform MoneyAnimationOrigin => moneyAnimationOrigin != null
            ? moneyAnimationOrigin
            : transform;

        public Vector3 MoneyAnimationOriginPosition => MoneyAnimationOrigin.position;

        public bool TrySpend(int amount)
        {
            EnsureModel();
            return _model.TrySpend(amount);
        }

        public bool Credit(int amount)
        {
            EnsureModel();
            return _model.Credit(amount);
        }

        private void EnsureModel()
        {
            if (_model != null)
                return;

            _model = new WalletModel(money);
            _model.BalanceChanged += OnModelBalanceChanged;
        }

        private void OnModelBalanceChanged(int balance)
        {
            money = balance;
            BalanceChanged?.Invoke(balance);
        }
    }
}
