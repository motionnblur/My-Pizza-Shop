using UnityEngine;

namespace Engineering.Engineering.Scripts.Mono.Actors.ServeStation
{
    public class ServeTrigger : MonoBehaviour
    {
        [SerializeField] private ServeStation serveStation;
        private void OnTriggerEnter(Collider other)
        {
            serveStation.TryServePizzas(other);
        }

        private void OnTriggerStay(Collider other)
        {
            serveStation.TryServePizzas(other);
        }
    }
}