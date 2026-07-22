using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SEconomy", menuName = "Scriptables/SEconomy", order = 0)]
    public class SEconomy : ScriptableObject
    {
        public int playerMoneySpendRate = 5;
        public float playerMoneySpendSpeed = 1f;
        public GameObject moneyPrefab;
    }
}