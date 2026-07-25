using System;
using System.Collections;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.Payment;
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
        private WaitForSeconds _paymentDelay;
        private CurrencyService _currencyService;
        private Coroutine _activePaymentCoroutine;
        private PaymentSessionModel _paymentSession;

        private void Awake()
        {
            if (_sAnimation != null)
                _paymentDelay = new WaitForSeconds(1f / _sAnimation.moneySpendSpeed);
        }

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
            if (sEconomy.playerMoneySpendRate <= 0) return;
            if (_sAnimation == null || _sAnimation.moneySpendSpeed <= 0) return;
            if (_activePaymentCoroutine != null) return;

            _paymentSession = new PaymentSessionModel(sEconomy.playerMoneySpendRate);
            _paymentSession.Begin();
            _activePaymentCoroutine = StartCoroutine(DelayedPayment(ba));
        }

        public void CancelPayment()
        {
            _paymentSession?.Cancel();

            if (_activePaymentCoroutine != null)
            {
                StopCoroutine(_activePaymentCoroutine);
                _activePaymentCoroutine = null;
            }
        }

        private IEnumerator DelayedPayment(BuyingArea ba)
        {
            var wallet = _currencyService.Wallet;

            yield return new WaitForSeconds(_sAnimation.moneySpendDelay);

            while (ba != null && wallet != null && _paymentSession.IsActive)
            {
                var animationTargetPosition = ba.transform.position;
                var pay = _paymentSession.GetNextPayment();
                if (pay <= 0)
                    break;

                if (_currencyService.TrySpend(pay))
                {
                    ba.AddPayment(pay);

                    yield return _paymentDelay ?? new WaitForSeconds(1f / _sAnimation.moneySpendSpeed);

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

            _paymentSession.Cancel();
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
