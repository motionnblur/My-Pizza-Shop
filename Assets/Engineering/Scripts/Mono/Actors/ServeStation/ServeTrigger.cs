using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServeTrigger : MonoBehaviour
    {
        [SerializeField] private ServeStation serveStation;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || serveStation == null)
                return;

            serveStation.TryServeFrontCustomer();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player") || serveStation == null)
                return;

            serveStation.TryServeFrontCustomer();
        }
    }
}