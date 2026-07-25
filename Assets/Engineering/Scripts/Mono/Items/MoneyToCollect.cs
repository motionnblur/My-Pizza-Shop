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
        private bool _isCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected || !other.CompareTag("Player"))
                return;

            _isCollected = true;

            if (CurrencyService.Instance != null)
                CurrencyService.Instance.Credit(moneyToCollect);

            groundMoneyCollectedEvent?.Raise();

            var wallet = CurrencyService.Instance != null ? CurrencyService.Instance.Wallet : null;
            if (wallet != null && moneyAnimationRequested != null)
            {
                moneyAnimationRequested.Raise(new MoneyAnimationRequest
                {
                    SourcePosition = transform.position,
                    DestinationTransform = wallet.MoneyAnimationOrigin
                });
            }

            Destroy(gameObject);
        }
    }
}
