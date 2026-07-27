using Engineering.Scripts.Domain.CustomerQueue;
using Engineering.Scripts.Mono.Actors.Table;
using ServeStationType = Engineering.Scripts.Mono.Actors.ServeStation.ServeStation;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerBot : MonoBehaviour
    {
        [Header("Order UI")]
        [SerializeField] private TMP_Text orderText;
        [SerializeField] private string orderTextFormat = "Pizza: {0}";

        private ServeStationType _station;
        private CustomerOrderModel _orderModel;
        private Transform[] _approachWaypoints;
        private int _currentWaypointIndex;
        private Transform _assignedSlot;
        private int _queueIndex;
        private bool _hasReachedAssignedSlot;
        private NavMeshAgent _agent;

        private bool _approachStarted;
        private enum BotState { Approaching, MovingToSlot, AtSlot, WaitingForTable, MovingToTable, Eating, Leaving }
        private BotState _state;

        private TableManager _tableManager;
        private int _tableIndex;
        private int _seatIndex;
        private Transform _assignedSeatTransform;
        private Transform _exitPoint;
        private float _eatingDuration;
        private float _eatingTimer;
        private float _pathTimeout;
        private const float MovingToTableTimeout = 10f;

        public CustomerOrderModel OrderModel => _orderModel;
        public int RemainingPizzaCount => _orderModel?.RemainingPizzaCount ?? 0;
        public bool HasReachedAssignedSlot => _hasReachedAssignedSlot;
        public bool IsWaitingForTable => _state == BotState.WaitingForTable;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnDestroy()
        {
            if (_tableManager != null && _state is BotState.MovingToTable or BotState.Eating)
            {
                _tableManager.ReleaseSeat(_tableIndex, _seatIndex);
            }
        }

        private bool IsAgentReady => _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;

        private void Update()
        {
            if (_agent == null) return;

            if (_state == BotState.MovingToTable)
                ProcessPathTimeout();

            if (_state < BotState.WaitingForTable && !_approachStarted) return;
            var needNavMesh = _state is BotState.Approaching or BotState.MovingToSlot or BotState.MovingToTable;
            if (needNavMesh && (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)) return;

            switch (_state)
            {
                case BotState.Approaching:
                    if (!IsAgentReady || _agent.pathPending) break;
                    if (_agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _currentWaypointIndex++;
                        if (_currentWaypointIndex < _approachWaypoints.Length)
                            TrySetDestination(_approachWaypoints[_currentWaypointIndex].position);
                        else
                        {
                            _state = BotState.MovingToSlot;
                            if (_assignedSlot != null)
                                TrySetDestination(_assignedSlot.position);
                        }
                    }
                    break;

                case BotState.MovingToSlot:
                    if (!IsAgentReady || _agent.pathPending) break;
                    if (_agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _hasReachedAssignedSlot = true;
                        _agent.isStopped = true;
                        _state = BotState.AtSlot;
                        if (_assignedSlot != null)
                            transform.rotation = _assignedSlot.rotation;
                    }
                    break;

                case BotState.WaitingForTable:
                    break;

                case BotState.MovingToTable:
                    if (_assignedSeatTransform == null)
                    {
                        Debug.LogWarning($"CustomerBot '{name}': no seat transform assigned. Releasing seat.", this);
                        _tableManager?.ReleaseSeat(_tableIndex, _seatIndex);
                        _state = BotState.Leaving;
                        break;
                    }
                    if (!IsAgentReady || _agent.pathPending) break;
                    if (_agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        _state = BotState.Eating;
                        _eatingTimer = _eatingDuration;
                        _pathTimeout = 0f;
                        if (IsAgentReady)
                            _agent.isStopped = true;
                        if (_assignedSeatTransform != null)
                            transform.rotation = _assignedSeatTransform.rotation;
                    }
                    break;

                case BotState.Eating:
                    _eatingTimer -= Time.deltaTime;
                    if (_eatingTimer <= 0f)
                    {
                        if (_tableManager != null)
                        {
                            _tableManager.AddLeftoversToTable(_tableIndex, _orderModel.InitialPizzaCount);
                            _tableManager.ReleaseSeat(_tableIndex, _seatIndex);
                        }
                        _state = BotState.Leaving;
                        if (IsAgentReady)
                        {
                            _agent.isStopped = false;
                            TrySetDestination(_exitPoint.position);
                        }
                    }
                    break;

                case BotState.Leaving:
                    if (_exitPoint == null || !IsAgentReady)
                    {
                        Destroy(gameObject);
                        break;
                    }
                    if (_agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        Destroy(gameObject);
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
            RefreshOrderText();
        }

        public void RefreshOrderText()
        {
            if (orderText == null)
                return;

            var remainingPizzaCount = RemainingPizzaCount;
            orderText.text = string.Format(orderTextFormat, remainingPizzaCount);
            orderText.gameObject.SetActive(remainingPizzaCount > 0);
        }

        public void SetupDining(TableManager tableManager, Transform exitPoint, float eatingDuration)
        {
            _tableManager = tableManager;
            _exitPoint = exitPoint;
            _eatingDuration = eatingDuration;
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
            if (IsAgentReady)
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

        public bool TransitionToDining()
        {
            var expectedLeftovers = _orderModel?.InitialPizzaCount ?? 0;
            if (_tableManager != null && _tableManager.TryReserveSeat(expectedLeftovers, out var tableIdx, out var seatIdx))
            {
                var seatTransform = _tableManager.GetSeatTransform(tableIdx, seatIdx);
                _tableIndex = tableIdx;
                _seatIndex = seatIdx;
                _assignedSeatTransform = seatTransform;
                _state = BotState.MovingToTable;
                _pathTimeout = 0f;
                _hasReachedAssignedSlot = false;
                if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
                    _agent.isStopped = false;
                if (seatTransform != null)
                    TrySetDestination(seatTransform.position);
                return true;
            }

            _state = BotState.WaitingForTable;
            return false;
        }

        public bool RetryReserveTable()
        {
            if (_state != BotState.WaitingForTable)
                return false;

            var expectedLeftovers = _orderModel?.InitialPizzaCount ?? 0;
            if (_tableManager != null && _tableManager.TryReserveSeat(expectedLeftovers, out var tableIdx, out var seatIdx))
            {
                var seatTransform = _tableManager.GetSeatTransform(tableIdx, seatIdx);
                _tableIndex = tableIdx;
                _seatIndex = seatIdx;
                _assignedSeatTransform = seatTransform;
                _state = BotState.MovingToTable;
                _pathTimeout = 0f;
                _hasReachedAssignedSlot = false;
                if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
                    _agent.isStopped = false;
                if (seatTransform != null)
                    TrySetDestination(seatTransform.position);
                return true;
            }

            return false;
        }

        private void ProcessPathTimeout()
        {
            _pathTimeout += Time.deltaTime;
            if (_pathTimeout >= MovingToTableTimeout)
            {
                Debug.LogWarning($"CustomerBot '{name}': could not reach seat within {MovingToTableTimeout}s. Releasing seat.", this);
                if (_tableManager != null)
                    _tableManager.ReleaseSeat(_tableIndex, _seatIndex);
                _state = BotState.Leaving;
                _pathTimeout = 0f;
                if (_agent != null)
                {
                    _agent.isStopped = false;
                    if (_agent.isOnNavMesh && _exitPoint != null)
                        TrySetDestination(_exitPoint.position);
                }
            }
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
