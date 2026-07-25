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
        [SerializeField] private CurrencyService currencyService;
        private bool _isCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected || !other.CompareTag("Player"))
                return;

            if (currencyService == null || currencyService.Wallet == null)
                return;

            _isCollected = true;

            currencyService.Credit(moneyToCollect);

            groundMoneyCollectedEvent?.Raise();

            if (moneyAnimationRequested != null)
            {
                moneyAnimationRequested.Raise(new MoneyAnimationRequest
                {
                    SourcePosition = transform.position,
                    DestinationTransform = currencyService.Wallet.MoneyAnimationOrigin
                });
            }

            Destroy(gameObject);
        }
    }
}
