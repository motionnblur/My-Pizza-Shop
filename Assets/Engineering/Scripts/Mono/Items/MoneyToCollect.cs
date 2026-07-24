using Engineering.Scripts.Mono.Managers;
using UnityEngine;

namespace Engineering.Scripts.Mono.Items
{
    public class MoneyToCollect : MonoBehaviour
    {
        [SerializeField] private int moneyToCollect = 5;
        private bool _isCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected || !other.CompareTag("Player"))
                return;

            _isCollected = true;
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.CollectMoneyFromGround(this, moneyToCollect);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
