using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.ServeStation
{
    public class PlateTrigger : MonoBehaviour
    {
        [SerializeField] private ServeStation serveStation;
        private void OnTriggerEnter(Collider other)
        {
            var playerInventory = other.GetComponentInParent<PlayerPizzaInventory>();
            serveStation.TryDepositPizzas(playerInventory);
        }
    }
}