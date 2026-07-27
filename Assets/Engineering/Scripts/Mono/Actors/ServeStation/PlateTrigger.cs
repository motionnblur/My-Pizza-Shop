using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.ServeStation
{
    public class PlateTrigger : MonoBehaviour
    {
        [SerializeField] private ServeStation serveStation;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || serveStation == null)
                return;

            var playerRoot = other.transform.root;
            var playerInventory = playerRoot.GetComponentInChildren<PlayerPizzaInventory>();
            serveStation.DepositFrom(playerInventory);
        }
    }
}
