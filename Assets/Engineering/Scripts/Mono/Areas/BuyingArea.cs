using System;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Areas
{
    public class BuyingArea : MonoBehaviour
    {
        [SerializeField] private EconomyManager economyManager;
        private int _unlockPrice = 100;
        private int _totalPricePlayerGive = 0;
        private bool isPurchased = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isPurchased) return;
            
            if (other.CompareTag("Player") && economyManager != null)
            {
                economyManager.ProcessPayment(this);
            }
        }

        private void OnTriggerStay(Collider other)
        {
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && economyManager != null)
            {
                economyManager.CancelPayment();
            }
        }
        
        public void AddPayment(int amount)
        {
            _totalPricePlayerGive += amount;
            if (_totalPricePlayerGive >= _unlockPrice)
            {
                isPurchased = true;
                if (economyManager != null)
                    economyManager.PlayerBuyBuyingArea(this);
            }
        }
    }
}
