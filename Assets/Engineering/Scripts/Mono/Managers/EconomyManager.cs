using System;
using System.Collections;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Areas;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class EconomyManager : MonoBehaviour
    {
        [SerializeField] private SEconomy sEconomy;
        [SerializeField] private SAnimation _sAnimation;
        [SerializeField] private SVoidEventChannel buyingAreaPurchasedEvent;
        [SerializeField] private SMoneyAnimationEventChannel moneyAnimationRequested;
        private CurrencyService _currencyService;
        private Coroutine _activePaymentCoroutine;

        public void Initialize(CurrencyService currencyService)
        {
            if (currencyService == null)
                throw new ArgumentNullException(nameof(currencyService));

            if (_currencyService != null)
            {
                if (_currencyService != currencyService)
                    throw new InvalidOperationException(
                        $"{nameof(EconomyManager)}: already initialized with a different {nameof(CurrencyService)}.");
                return;
            }

            _currencyService = currencyService;
        }

        public void ProcessPayment(BuyingArea ba)
        {
            if (ba == null) return;
            if (_currencyService == null || _currencyService.Wallet == null) return;
            if (sEconomy == null) return;
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
            var wallet = _currencyService.Wallet;
            var pay = sEconomy.playerMoneySpendRate;
            var delay = 1f / _sAnimation.moneySpendSpeed;

            yield return new WaitForSeconds(_sAnimation.moneySpendDelay);

            while (ba != null && wallet != null)
            {
                var animationTargetPosition = ba.transform.position;

                if (_currencyService.TrySpend(pay))
                {
                    ba.AddPayment(pay);

                    yield return new WaitForSeconds(delay);

                    if (_currencyService.Wallet == null)
                        break;

                    if (moneyAnimationRequested != null)
                    {
                        moneyAnimationRequested.Raise(new MoneyAnimationRequest
                        {
                            SourcePosition = wallet.MoneyAnimationOriginPosition,
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
