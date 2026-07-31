using UnityEngine;
using UnityEngine.AI;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerNavigation : MonoBehaviour
    {
        private NavMeshAgent _agent;

        public bool IsReady => _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;
        public bool HasPendingPath => _agent != null && _agent.pathPending;

        public bool HasArrived => _agent != null && _agent.remainingDistance <= _agent.stoppingDistance;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void MoveTo(Vector3 destination)
        {
            if (_agent == null) return;
            if (!_agent.isActiveAndEnabled) return;
            if (!_agent.isOnNavMesh) return;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (_agent == null) return;
            _agent.isStopped = true;
        }

        public void Resume()
        {
            if (_agent == null) return;
            _agent.isStopped = false;
        }

        public void SetAvoidancePriority(int priority)
        {
            if (!IsReady) return;
            _agent.avoidancePriority = priority;
        }

        public void Face(Quaternion rotation)
        {
            transform.rotation = rotation;
        }
    }
}
