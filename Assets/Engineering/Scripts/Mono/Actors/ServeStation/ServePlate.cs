using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServePlate : MonoBehaviour
    {
        [SerializeField] private ServeStation serveStation;
        [SerializeField] private BoxCollider plateCollider;

        public Vector3 PizzaStackBasePosition
        {
            get
            {
                if (plateCollider == null)
                    return transform.position;

                var bounds = plateCollider.bounds;
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }

        private void Awake()
        {
            if (plateCollider == null)
                plateCollider = GetComponent<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            TryServePizzas(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryServePizzas(other);
        }

        private void TryServePizzas(Collider other)
        {
            if (!other.CompareTag("Player") || serveStation == null)
                return;

            var playerInventory = other.GetComponentInParent<PlayerPizzaInventory>();
            if (playerInventory == null)
                return;

            serveStation.TryDepositPizzas(playerInventory);
            serveStation.TryServeFrontCustomer();
        }
    }
}
