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

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void UpdateMoneyText(int money)
        {
            moneyText.text = money.ToString();
        }
    }
}
