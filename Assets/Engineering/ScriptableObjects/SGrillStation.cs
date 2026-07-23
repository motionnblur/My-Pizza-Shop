using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SGrillStation", menuName = "Scriptables/SGrillStation", order = 0)]
    public class SGrillStation : ScriptableObject
    {
        [Min(0.01f)] public float productionInterval = 3f;
        [Min(1)] public int maxReadyPizzas = 5;
    }
}
