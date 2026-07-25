using System;
using Engineering.Scripts.Domain.Purchase;
using Engineering.Scripts.Mono.Managers;
using UnityEngine;

namespace Engineering.Scripts.Mono.Areas
{
    public class BuyingArea : MonoBehaviour
    {
        [Min(1)]
        [SerializeField]
        private int _unlockPrice = 100;

        private EconomyManager _economyManager;
        private PurchaseProgressModel _purchaseProgressModel;

        public int UnlockPrice => GetOrCreateModel().UnlockPrice;
        public int PaidAmount => GetOrCreateModel().PaidAmount;
        public int RemainingAmount => GetOrCreateModel().RemainingAmount;
        public bool IsPurchased => GetOrCreateModel().IsPurchased;

        public void Initialize(EconomyManager economyManager)
        {
            if (economyManager == null)
                throw new ArgumentNullException(nameof(economyManager));

            if (_economyManager != null)
            {
                if (_economyManager != economyManager)
                    throw new InvalidOperationException(
                        $"{nameof(BuyingArea)}: already initialized with a different {nameof(EconomyManager)}.");
                return;
            }

            _economyManager = economyManager;
        }

        private void Awake()
        {
            GetOrCreateModel();
        }

        private PurchaseProgressModel GetOrCreateModel()
        {
            if (_purchaseProgressModel == null)
                _purchaseProgressModel = new PurchaseProgressModel(_unlockPrice);
            return _purchaseProgressModel;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPurchased) return;

            if (other.CompareTag("Player") && _economyManager != null)
            {
                _economyManager.ProcessPayment(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && _economyManager != null)
            {
                _economyManager.CancelPayment();
            }
        }

        public void AddPayment(int amount)
        {
            var result = GetOrCreateModel().ApplyPayment(amount);
            if (result.CompletedNow && _economyManager != null)
                _economyManager.PlayerBuyBuyingArea(this);
        }
    }
}
