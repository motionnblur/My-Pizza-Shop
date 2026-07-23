using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SEconomy", menuName = "Scriptables/SEconomy", order = 0)]
    public class SEconomy : ScriptableObject
    {
        public int totalPlayerMoney = 250;
        public int playerMoneySpendRate = 5;
        public GameObject moneyPrefab;
    }
}