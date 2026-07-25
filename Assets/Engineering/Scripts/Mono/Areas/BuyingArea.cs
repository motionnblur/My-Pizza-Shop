using System;
using Engineering.Scripts.Mono.Managers;
using UnityEngine;

namespace Engineering.Scripts.Mono.Areas
{
    public class BuyingArea : MonoBehaviour
    {
        private EconomyManager _economyManager;
        private int _unlockPrice = 100;
        private int _totalPricePlayerGive = 0;
        private bool isPurchased = false;

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

        private void OnTriggerEnter(Collider other)
        {
            if (isPurchased) return;
            
            if (other.CompareTag("Player") && _economyManager != null)
            {
                _economyManager.ProcessPayment(this);
            }
        }

        private void OnTriggerStay(Collider other)
        {
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
            _totalPricePlayerGive += amount;
            if (_totalPricePlayerGive >= _unlockPrice)
            {
                isPurchased = true;
                if (_economyManager != null)
                    _economyManager.PlayerBuyBuyingArea(this);
            }
        }
    }
}
