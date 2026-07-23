using System;
using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Scriptables/Events/Void Event Channel", order = 10)]
    public class SVoidEventChannel : ScriptableObject
    {
        private event Action Raised;

        public void Raise()
        {
            Raised?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            Raised += listener;
        }

        public void UnregisterListener(Action listener)
        {
            Raised -= listener;
        }
    }
}
