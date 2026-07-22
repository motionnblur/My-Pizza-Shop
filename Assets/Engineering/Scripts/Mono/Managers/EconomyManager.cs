using System.Collections;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }
        private PlayerWallet _pWallet;

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

        private void Start()
        {
            _pWallet = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerWallet>();
        }

        public void ProcessPayment(BuyingArea ba)
        {
            StartCoroutine(DelayedPayment(ba, 1, 0.1f));
        }

        private IEnumerator DelayedPayment(BuyingArea ba, int pay, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (ba == null) yield break;

            var moneyInPlayerPocket = _pWallet.Money;
            var afterMoneyInPlayerPocket = moneyInPlayerPocket - pay;

            if (afterMoneyInPlayerPocket > 0)
            {
                _pWallet.Money = afterMoneyInPlayerPocket;
                ba.AddPayment(pay);
            }
        }

        public void PlayerBuyBuyingArea(BuyingArea ba)
        {
            if (ba == null || ba.gameObject == null) return;
            Destroy(ba.gameObject);
        }
    }
}