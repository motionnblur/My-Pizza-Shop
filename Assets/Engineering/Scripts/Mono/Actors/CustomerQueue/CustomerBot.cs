using Engineering.Scripts.Domain.CustomerQueue;
using ServeStationType = Engineering.Scripts.Mono.Actors.ServeStation.ServeStation;
using UnityEngine;
using UnityEngine.AI;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerBot : MonoBehaviour
    {
        private ServeStationType _station;
        private CustomerOrderModel _orderModel;
        private Transform[] _approachWaypoints;
        private int _currentWaypointIndex;
        private Transform _assignedSlot;
        private int _queueIndex;
        private bool _hasReachedAssignedSlot;
        private NavMeshAgent _agent;

        private bool _approachStarted;
        private enum BotState { Approaching, MovingToSlot, AtSlot }
        private BotState _state;

        public CustomerOrderModel OrderModel => _orderModel;
        public int RemainingPizzaCount => _orderModel?.RemainingPizzaCount ?? 0;
        public bool HasReachedAssignedSlot => _hasReachedAssignedSlot;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_station == null || _agent == null)
                return;

            if (!_approachStarted)
                return;

            if (!_agent.isOnNavMesh)
                return;

            if (_agent.pathPending)
                return;

            switch (_state)
            {
                case BotState.Approaching:
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _currentWaypointIndex++;
                        if (_currentWaypointIndex < _approachWaypoints.Length)
                        {
                            TrySetDestination(_approachWaypoints[_currentWaypointIndex].position);
                        }
                        else
                        {
                            _state = BotState.MovingToSlot;
                            if (_assignedSlot != null)
                                TrySetDestination(_assignedSlot.position);
                        }
                    }
                    break;

                case BotState.MovingToSlot:
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _hasReachedAssignedSlot = true;
                        _agent.isStopped = true;
                        _state = BotState.AtSlot;
                        if (_assignedSlot != null)
                            transform.rotation = _assignedSlot.rotation;
                    }
                    break;
            }
        }

        public void Initialize(ServeStationType station, CustomerOrderModel orderModel, Transform[] approachWaypoints)
        {
            _station = station;
            _orderModel = orderModel ?? throw new System.ArgumentNullException(nameof(orderModel));
            _approachWaypoints = approachWaypoints;
            _currentWaypointIndex = 0;
            _hasReachedAssignedSlot = false;
            _approachStarted = false;
        }

        public void BeginApproach()
        {
            _approachStarted = true;

            if (_approachWaypoints != null && _approachWaypoints.Length > 0 && _approachWaypoints[0] != null)
            {
                _state = BotState.Approaching;
                TrySetDestination(_approachWaypoints[0].position);
            }
            else
            {
                _state = BotState.MovingToSlot;
                if (_assignedSlot != null)
                    TrySetDestination(_assignedSlot.position);
            }
        }

        public void AssignQueueSlot(Transform queueSlot, int queueIndex)
        {
            _assignedSlot = queueSlot;
            _queueIndex = queueIndex;
            _hasReachedAssignedSlot = false;

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.avoidancePriority = queueIndex;
            }

            if (_state == BotState.AtSlot || _state == BotState.MovingToSlot)
            {
                _state = BotState.MovingToSlot;
                if (_assignedSlot != null)
                    TrySetDestination(_assignedSlot.position);
            }
        }

        public void ReceivePizzas(int amount)
        {
            _orderModel?.ReceivePizzas(amount);
        }

        private void TrySetDestination(Vector3 destination)
        {
            if (_agent == null) return;
            if (!_agent.isActiveAndEnabled) return;
            if (!_agent.isOnNavMesh) return;

            _agent.SetDestination(destination);
        }
    }
}
