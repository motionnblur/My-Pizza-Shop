using System;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using UnityEngine;

namespace Engineering.Scripts.Mono.Items
{
    public class MoneyToCollect : MonoBehaviour
    {
        [SerializeField] private int moneyToCollect = 5;
        [SerializeField] private SVoidEventChannel groundMoneyCollectedEvent;
        [SerializeField] private SMoneyAnimationEventChannel moneyAnimationRequested;
        private CurrencyService _currencyService;
        private bool _isCollected;

        public void Initialize(CurrencyService currencyService)
        {
            if (currencyService == null)
                throw new ArgumentNullException(nameof(currencyService));

            if (_currencyService != null)
            {
                if (_currencyService != currencyService)
                    throw new InvalidOperationException(
                        $"{nameof(MoneyToCollect)}: already initialized with a different {nameof(CurrencyService)}.");
                return;
            }

            _currencyService = currencyService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected || !other.CompareTag("Player"))
                return;

            if (_currencyService == null || _currencyService.Wallet == null)
                return;

            _isCollected = true;

            _currencyService.Credit(moneyToCollect);

            groundMoneyCollectedEvent?.Raise();

            if (moneyAnimationRequested != null)
            {
                moneyAnimationRequested.Raise(new MoneyAnimationRequest
                {
                    SourcePosition = transform.position,
                    DestinationTransform = _currencyService.Wallet.MoneyAnimationOrigin
                });
            }

            Destroy(gameObject);
        }
    }
}
