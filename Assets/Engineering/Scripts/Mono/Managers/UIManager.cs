using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Engineering.Scripts.Mono.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        [SerializeField] private Text moneyText;
        [SerializeField] private Text pizzaText;
        [SerializeField] private SIntEventChannel pizzaInventoryChangedEvent;
        [SerializeField] private PlayerWallet _playerWallet;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

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

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
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
