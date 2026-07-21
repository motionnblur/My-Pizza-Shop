using UnityEngine;

namespace Engineering.Scripts.Mono.Player
{
    [RequireComponent(typeof(Collider))]
    public class PlayerTriggerRelay : MonoBehaviour
    {
        [SerializeField] private PlayerTrigger playerTrigger;

        private void OnTriggerEnter(Collider other)
        {
            playerTrigger.HandleTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            playerTrigger.HandleTriggerExit(other);
        }
    }
}
