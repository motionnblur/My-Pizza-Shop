using Engineering.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Engineering.Scripts.Mono.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        [SerializeField] private SEconomy sEconomy;
        [SerializeField] private Text moneyText;
        [SerializeField] private Text pizzaText;
        [SerializeField] private SIntEventChannel pizzaInventoryChangedEvent;

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
        }

        private void OnDisable()
        {
            pizzaInventoryChangedEvent?.UnregisterListener(UpdatePizzaText);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void UpdateMoneyText(int money)
        {
            moneyText.text = money.ToString();
        }

        private void UpdatePizzaText(int pizzaCount)
        {
            if (pizzaText != null)
                pizzaText.text = pizzaCount.ToString();
        }
    }
}
