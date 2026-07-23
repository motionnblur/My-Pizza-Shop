using Engineering.Scripts.Mono.Managers;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Items
{
    public class MoneyToCollect : MonoBehaviour
    {
        [SerializeField] private int moneyToCollect = 5;
        private void OnTriggerEnter(Collider other)
        {
            EconomyManager.Instance.CollectMoneyFromGround(this, moneyToCollect);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}