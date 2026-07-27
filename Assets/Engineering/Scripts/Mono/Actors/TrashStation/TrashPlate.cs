using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.TrashStation
{
    public class TrashPlate : MonoBehaviour
    {
        [SerializeField] private TrashStation trashStation;

        private void OnTriggerEnter(Collider other)
        {
            TryProcessPlayer(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryProcessPlayer(other);
        }

        private void TryProcessPlayer(Collider other)
        {
            if (trashStation == null)
                return;

            if (!IsPlayerCollider(other))
                return;

            var playerRoot = other.transform.root;

            var playerPizzaInventory = playerRoot.GetComponentInChildren<PlayerPizzaInventory>();
            if (playerPizzaInventory != null && playerPizzaInventory.Count > 0)
                trashStation.TrashAllPizzas(playerPizzaInventory);

            var playerWasteInventory = playerRoot.GetComponentInChildren<PlayerWasteInventory>();
            if (playerWasteInventory != null && playerWasteInventory.Count > 0)
                trashStation.TryDisposeWaste(playerWasteInventory);
        }

        private static bool IsPlayerCollider(Collider other)
        {
            if (other.CompareTag("Player"))
                return true;

            var root = other.transform.root;
            return root != null && root.CompareTag("Player");
        }
    }
}
