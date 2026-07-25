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
        [SerializeField] private PlayerWallet _playerWallet;

        private void OnEnable()
        {
            pizzaInventoryChangedEvent?.RegisterListener(UpdatePizzaText);
            if (_playerWallet != null)
            {
                _playerWallet.BalanceChanged += OnBalanceChanged;
                UpdateMoneyText(_playerWallet.Money);
            }
        }

        private void OnDisable()
        {
            pizzaInventoryChangedEvent?.UnregisterListener(UpdatePizzaText);
            if (_playerWallet != null)
            {
                _playerWallet.BalanceChanged -= OnBalanceChanged;
            }
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
