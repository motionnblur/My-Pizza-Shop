using System.Collections;
using Engineering.ScriptableObjects;
using ServeStationType = Engineering.Scripts.Mono.Actors.ServeStation.ServeStation;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    public class CustomerSpawner : MonoBehaviour
    {
        [SerializeField] private ServeStationType station;
        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform[] approachWaypoints;
        [SerializeField] private Transform queueEntryPoint;
        [SerializeField] private SServeStation sServeStation;

        private Coroutine _spawnCoroutine;

        private void OnEnable()
        {
            _spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        private void OnDisable()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (enabled)
            {
                if (station != null && sServeStation != null && customerPrefab != null && spawnPoint != null)
                {
                    if (station.CustomerCount < sServeStation.maxQueueCustomers)
                    {
                        var customerObject = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
                        var customerBot = customerObject.GetComponent<CustomerBot>();

                        if (customerBot != null)
                        {
                            var waypoints = BuildApproachWaypoints();
                            var orderAmount = Random.Range(sServeStation.minPizzasPerOrder, sServeStation.maxPizzasPerOrder + 1);
                            customerBot.Initialize(station, orderAmount, waypoints);

                            if (!station.RegisterCustomer(customerBot))
                                Destroy(customerObject);
                        }
                        else
                        {
                            Destroy(customerObject);
                        }
                    }
                }

                yield return new WaitForSeconds(sServeStation != null ? sServeStation.customerSpawnInterval : 2f);
            }
        }

        private Transform[] BuildApproachWaypoints()
        {
            if (queueEntryPoint == null)
                return approachWaypoints;

            if (approachWaypoints == null || approachWaypoints.Length == 0)
                return new[] { queueEntryPoint };

            var combined = new Transform[approachWaypoints.Length + 1];
            System.Array.Copy(approachWaypoints, combined, approachWaypoints.Length);
            combined[approachWaypoints.Length] = queueEntryPoint;
            return combined;
        }
    }
}
