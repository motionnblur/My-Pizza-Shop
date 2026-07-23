using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.PizzaMaker
{
    public class GrillPlate : MonoBehaviour
    {
        [SerializeField] private GrillStation grillStation;
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
            TryCollectPizzas(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryCollectPizzas(other);
        }

        private void TryCollectPizzas(Collider other)
        {
            if (!other.CompareTag("Player") || grillStation == null)
                return;

            grillStation.TryCollectAll(other.GetComponentInParent<PlayerPizzaInventory>());
        }
    }
}
