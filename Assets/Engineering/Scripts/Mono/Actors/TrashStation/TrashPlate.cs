using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.TrashStation
{
    public class TrashPlate : MonoBehaviour
    {
        [SerializeField] private TrashStation trashStation;

        private void OnTriggerEnter(Collider other)
        {
            TryTrashPizzas(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryTrashPizzas(other);
        }

        private void TryTrashPizzas(Collider other)
        {
            if (!other.CompareTag("Player") || trashStation == null)
                return;

            trashStation.TrashAllPizzas(other.GetComponentInParent<PlayerPizzaInventory>());
        }
    }
}
