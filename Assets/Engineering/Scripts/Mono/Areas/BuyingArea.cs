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
            //
        }

        private void OnTriggerStay(Collider other)
        {
            if (isPurchased) return;
            
            if (other.gameObject.tag.Equals("Player"))
            {
                EconomyManager.Instance.ProcessPayment(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                //
            }
        }
        
        public void AddPayment(int amount)
        {
            _totalPricePlayerGive += amount;
            if (_totalPricePlayerGive >= _unlockPrice)
            {
                isPurchased = true;
                EconomyManager.Instance.PlayerBuyBuyingArea(this);
            }
        }
    }
}