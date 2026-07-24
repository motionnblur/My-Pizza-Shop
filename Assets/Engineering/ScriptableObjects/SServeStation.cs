using UnityEngine;
using UnityEngine.Serialization;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SServeStation", menuName = "Scriptables/SServeStation", order = 0)]
    public class SServeStation : ScriptableObject
    {
        [FormerlySerializedAs("maxPizzas")]
        [Min(1)] public int maxPizzasPerOrder = 5;
        [Min(1)] public int minPizzasPerOrder = 1;
        [Min(1)] public int maxQueueCustomers = 10;
        [Min(0.1f)] public float customerSpawnInterval = 2f;
        [Min(0)] public int pricePerPizza = 10;

        private void OnValidate()
        {
            maxQueueCustomers = Mathf.Clamp(maxQueueCustomers, 1, 10);
            minPizzasPerOrder = Mathf.Clamp(minPizzasPerOrder, 1, maxPizzasPerOrder);
            maxPizzasPerOrder = Mathf.Max(minPizzasPerOrder, maxPizzasPerOrder);
            customerSpawnInterval = Mathf.Max(0.1f, customerSpawnInterval);
        }
    }
}
