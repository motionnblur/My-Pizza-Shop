using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SSound", menuName = "Scriptables/SSound", order = 0)]
    public class SSound : ScriptableObject
    {
        public AudioClip moneyCollectEffect;
        public AudioClip buyEffect;
        public AudioClip pizzaServeEffect;
        public AudioClip pizzaTrashEffect;
    }
}