using System;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class CurrencyService : MonoBehaviour
    {
        private PlayerWallet _playerWallet;

        public PlayerWallet Wallet => _playerWallet;

        public void Initialize(PlayerWallet playerWallet)
        {
            if (playerWallet == null)
                throw new ArgumentNullException(nameof(playerWallet));

            if (_playerWallet != null)
            {
                if (_playerWallet != playerWallet)
                    throw new InvalidOperationException(
                        $"{nameof(CurrencyService)}: already initialized with a different {nameof(PlayerWallet)}.");
                return;
            }

            _playerWallet = playerWallet;
        }

        public bool TrySpend(int amount)
        {
            return _playerWallet != null && _playerWallet.TrySpend(amount);
        }

        public bool Credit(int amount)
        {
            return _playerWallet != null && _playerWallet.Credit(amount);
        }
    }
}
