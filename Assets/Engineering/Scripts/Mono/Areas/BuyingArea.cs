using System;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Areas
{
    public class BuyingArea : MonoBehaviour
    {
        private int _unlockPrice = 100;
        private int _totalPricePlayerGive = 0;
        private bool isPurchased = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                //
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isPurchased) return;
            
            if (other.gameObject.tag.Equals("Player"))
            {
                PlayerWallet pWallet = other.GetComponent<PlayerWallet>();
                EconomyManager.Instance.ProcessPayment(pWallet, _unlockPrice);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                //
            }
        }
        
        public void AddPayment(int money)
        {
            _totalPricePlayerGive += money;
            if (_totalPricePlayerGive >= _unlockPrice)
            {
                CompletePurchase();
            }
        }

        private void CompletePurchase()
        {
            isPurchased = true;
        }
    }
}