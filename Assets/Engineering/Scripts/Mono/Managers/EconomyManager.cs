using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

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

        public void ProcessPayment(PlayerWallet pWallet, int money)
        {
            Debug.Log("PlayerSpendMoney: " + money);
        }

        public void PlayerBuyBuyingArea()
        {
            
        }
    }
}