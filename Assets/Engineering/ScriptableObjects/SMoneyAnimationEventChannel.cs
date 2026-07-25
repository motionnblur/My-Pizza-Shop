using System;
using UnityEngine;

namespace Engineering.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MoneyAnimationEventChannel", menuName = "Scriptables/Events/Money Animation Event Channel", order = 12)]
    public class SMoneyAnimationEventChannel : ScriptableObject
    {
        private event Action<MoneyAnimationRequest> Raised;

        public void Raise(MoneyAnimationRequest request)
        {
            Raised?.Invoke(request);
        }

        public void RegisterListener(Action<MoneyAnimationRequest> listener)
        {
            Raised += listener;
        }

        public void UnregisterListener(Action<MoneyAnimationRequest> listener)
        {
            Raised -= listener;
        }
    }
}
