using System;
using UnityEngine;

namespace Engineering.Scripts.Mono.Areas
{
    public class BuyingArea : MonoBehaviour
    {
        private int _unlockPrice = 100;
        private int _totalPricePlayerGive = 0;

        public event Action<int> PlayerSpendMoneyEvent;
        public event Action PlayerBuyBuyingAreaEvent;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                //
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                _totalPricePlayerGive += 1;
                PlayerSpendMoneyEvent?.Invoke(1);
                
                if (_totalPricePlayerGive == _unlockPrice)
                    PlayerBuyBuyingAreaEvent?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                //
            }
        }
    }
}