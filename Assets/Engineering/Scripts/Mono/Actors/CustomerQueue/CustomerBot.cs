using ServeStationType = Engineering.Engineering.Scripts.Mono.Actors.ServeStation.ServeStation;
using UnityEngine;
using UnityEngine.AI;

namespace Engineering.Engineering.Scripts.Mono.Actors.CustomerQueue
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerBot : MonoBehaviour
    {
        private ServeStationType _station;
        private int _remainingPizzaCount;
        private Transform[] _approachWaypoints;
        private int _currentWaypointIndex;
        private Transform _assignedSlot;
        private bool _hasReachedAssignedSlot;
        private NavMeshAgent _agent;

        private enum BotState { Approaching, MovingToSlot, AtSlot }
        private BotState _state;

        public int RemainingPizzaCount => _remainingPizzaCount;
        public bool HasReachedAssignedSlot => _hasReachedAssignedSlot;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_station == null || _agent.pathPending)
                return;

            switch (_state)
            {
                case BotState.Approaching:
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _currentWaypointIndex++;
                        if (_currentWaypointIndex < _approachWaypoints.Length)
                        {
                            _agent.SetDestination(_approachWaypoints[_currentWaypointIndex].position);
                        }
                        else
                        {
                            _state = BotState.MovingToSlot;
                            if (_assignedSlot != null)
                                _agent.SetDestination(_assignedSlot.position);
                        }
                    }
                    break;

                case BotState.MovingToSlot:
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _hasReachedAssignedSlot = true;
                        _state = BotState.AtSlot;
                        if (_assignedSlot != null)
                            transform.rotation = _assignedSlot.rotation;
                    }
                    break;
            }
        }

        public void Initialize(ServeStationType station, int orderAmount, Transform[] approachWaypoints)
        {
            _station = station;
            _remainingPizzaCount = orderAmount;
            _approachWaypoints = approachWaypoints;
            _currentWaypointIndex = 0;
            _hasReachedAssignedSlot = false;

            if (_approachWaypoints != null && _approachWaypoints.Length > 0 && _approachWaypoints[0] != null)
            {
                _state = BotState.Approaching;
                _agent.SetDestination(_approachWaypoints[0].position);
            }
            else
            {
                _state = BotState.MovingToSlot;
                if (_assignedSlot != null)
                    _agent.SetDestination(_assignedSlot.position);
            }
        }

        public void AssignQueueSlot(Transform queueSlot)
        {
            _assignedSlot = queueSlot;
            _hasReachedAssignedSlot = false;

            if (_state == BotState.AtSlot || _state == BotState.MovingToSlot)
            {
                _state = BotState.MovingToSlot;
                if (_assignedSlot != null)
                    _agent.SetDestination(_assignedSlot.position);
            }
        }

        public void ReceivePizzas(int amount)
        {
            if (amount <= 0) return;
            _remainingPizzaCount = Mathf.Max(0, _remainingPizzaCount - amount);
        }
    }
}
