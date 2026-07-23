using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SServeStation", menuName = "Scriptables/SServeStation", order = 0)]
    public class SServeStation : ScriptableObject
    {
        [Min(1)] public int maxPizzas = 5;
        [Min(0)] public int pricePerPizza = 10;
    }
}
