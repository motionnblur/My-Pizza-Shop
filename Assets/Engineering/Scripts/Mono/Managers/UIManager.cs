using System;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Engineering.Scripts.Mono.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Text moneyText;
        [SerializeField] private Text pizzaText;
        [SerializeField] private SIntEventChannel pizzaInventoryChangedEvent;
        private PlayerWallet _playerWallet;
        private bool _isWalletSubscribed;

        public void Initialize(PlayerWallet playerWallet)
        {
            if (playerWallet == null)
                throw new ArgumentNullException(nameof(playerWallet));

            if (_playerWallet != null)
            {
                if (_playerWallet != playerWallet)
                    throw new InvalidOperationException(
                        $"{nameof(UIManager)}: already initialized with a different {nameof(PlayerWallet)}.");
                return;
            }

            _playerWallet = playerWallet;

            if (isActiveAndEnabled)
            {
                SubscribeToWallet();
                UpdateMoneyText(_playerWallet.Money);
            }
        }

        private void OnEnable()
        {
            pizzaInventoryChangedEvent?.RegisterListener(UpdatePizzaText);
            SubscribeToWallet();
            if (_playerWallet != null)
                UpdateMoneyText(_playerWallet.Money);
        }

        private void OnDisable()
        {
            pizzaInventoryChangedEvent?.UnregisterListener(UpdatePizzaText);
            UnsubscribeFromWallet();
        }

        private void SubscribeToWallet()
        {
            if (_playerWallet == null || _isWalletSubscribed)
                return;

            _playerWallet.BalanceChanged += OnBalanceChanged;
            _isWalletSubscribed = true;
        }

        private void UnsubscribeFromWallet()
        {
            if (!_isWalletSubscribed || _playerWallet == null)
                return;

            _playerWallet.BalanceChanged -= OnBalanceChanged;
            _isWalletSubscribed = false;
        }

        private void OnBalanceChanged(int balance)
        {
            UpdateMoneyText(balance);
        }

        public void UpdateMoneyText(int money)
        {
            if (moneyText != null)
                moneyText.text = money.ToString();
        }

        private void UpdatePizzaText(int pizzaCount)
        {
            if (pizzaText != null)
                pizzaText.text = pizzaCount.ToString();
        }
    }
}
