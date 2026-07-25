using System.Collections;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }
        [SerializeField] private SEconomy sEconomy;
        [SerializeField] private SAnimation _sAnimation;
        [SerializeField] private SVoidEventChannel buyingAreaPurchasedEvent;
        [SerializeField] private SMoneyAnimationEventChannel moneyAnimationRequested;
        private PlayerWallet _pWallet;
        private Coroutine _activePaymentCoroutine;

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
            _pWallet = FindFirstObjectByType<PlayerWallet>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ProcessPayment(BuyingArea ba)
        {
            if (_sAnimation == null || _sAnimation.moneySpendSpeed <= 0) return;
            if (_activePaymentCoroutine != null) return;
            _activePaymentCoroutine = StartCoroutine(DelayedPayment(ba));
        }

        public void CancelPayment()
        {
            if (_activePaymentCoroutine != null)
            {
                StopCoroutine(_activePaymentCoroutine);
                _activePaymentCoroutine = null;
            }
        }

        private IEnumerator DelayedPayment(BuyingArea ba)
        {
            if (ba == null || _pWallet == null || sEconomy == null) yield break;

            var pay = sEconomy.playerMoneySpendRate;
            var delay = 1f / _sAnimation.moneySpendSpeed;

            yield return new WaitForSeconds(_sAnimation.moneySpendDelay);

            while (ba != null && _pWallet != null)
            {
                var animationTargetPosition = ba.transform.position;

                if (CurrencyService.Instance != null && CurrencyService.Instance.TrySpend(pay))
                {
                    ba.AddPayment(pay);

                    yield return new WaitForSeconds(delay);

                    if (_pWallet == null)
                        break;

                    if (moneyAnimationRequested != null)
                    {
                        moneyAnimationRequested.Raise(new MoneyAnimationRequest
                        {
                            SourcePosition = _pWallet.MoneyAnimationOriginPosition,
                            DestinationPosition = animationTargetPosition
                        });
                    }
                }
                else
                {
                    break;
                }
            }

            _activePaymentCoroutine = null;
        }

        public void PlayerBuyBuyingArea(BuyingArea ba)
        {
            if (ba == null || ba.gameObject == null) return;
            Destroy(ba.gameObject);
            buyingAreaPurchasedEvent?.Raise();
        }
    }
}
