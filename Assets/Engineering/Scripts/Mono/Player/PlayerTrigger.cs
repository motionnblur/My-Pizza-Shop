using System;
using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerTrigger : MonoBehaviour
    {
        public event Action<Collider> TriggerEnterEvent;
        public event Action<Collider> TriggerExitEvent;
        
        public void HandleTriggerEnter(Collider other)
        {
            TriggerEnterEvent?.Invoke(other);
        }

        public void HandleTriggerExit(Collider other)
        {
            TriggerExitEvent?.Invoke(other);
        }
    }
}
