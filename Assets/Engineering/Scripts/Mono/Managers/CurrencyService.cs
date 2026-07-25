using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class CurrencyService : MonoBehaviour
    {
        [SerializeField] private PlayerWallet _playerWallet;

        public PlayerWallet Wallet => _playerWallet;

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
