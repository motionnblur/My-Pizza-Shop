using System;
using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "IntEventChannel", menuName = "Scriptables/Events/Int Event Channel", order = 11)]
    public class SIntEventChannel : ScriptableObject
    {
        private event Action<int> Raised;

        public void Raise(int value)
        {
            Raised?.Invoke(value);
        }

        public void RegisterListener(Action<int> listener)
        {
            Raised += listener;
        }

        public void UnregisterListener(Action<int> listener)
        {
            Raised -= listener;
        }
    }
}
