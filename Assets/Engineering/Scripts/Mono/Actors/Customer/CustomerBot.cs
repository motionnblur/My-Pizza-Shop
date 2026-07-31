using Engineering.Scripts.Domain.CustomerQueue;
using Engineering.Scripts.Mono.Actors.Table;
using ServeStationType = Engineering.Scripts.Mono.Actors.ServeStation.ServeStation;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    [RequireComponent(typeof(CustomerNavigation))]
    [RequireComponent(typeof(CustomerDiningController))]
    public class CustomerBot : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CustomerOrderView orderView;
        [SerializeField] private CustomerNavigation navigation;
        [SerializeField] private CustomerDiningController dining;

        [Header("Movement")]
        [SerializeField] private float movingToTableTimeout = 10f;

        private ServeStationType _station;
        private CustomerOrderModel _orderModel;
        private Transform[] _approachWaypoints;
        private int _currentWaypointIndex;
        private Transform _assignedSlot;
        private int _queueIndex;
        private bool _hasReachedAssignedSlot;

        private bool _approachStarted;
        private enum BotState { Approaching, MovingToSlot, AtSlot, WaitingForTable, MovingToTable, Eating, Leaving }
        private BotState _state;

        private float _pathTimeout;

        public CustomerOrderModel OrderModel => _orderModel;
        public int RemainingPizzaCount => _orderModel?.RemainingPizzaCount ?? 0;
        public bool HasReachedAssignedSlot => _hasReachedAssignedSlot;
        public bool IsWaitingForTable => _state == BotState.WaitingForTable;

        private void Awake()
        {
            if (navigation == null)
                navigation = GetComponent<CustomerNavigation>();
            if (dining == null)
                dining = GetComponent<CustomerDiningController>();
            if (orderView == null)
                orderView = GetComponent<CustomerOrderView>();
        }

        private void OnDestroy()
        {
            if (dining != null)
                dining.ReleaseSeat();
        }

        private void Update()
        {
            if (_state == BotState.MovingToTable)
                ProcessPathTimeout();

            if (_state < BotState.WaitingForTable && !_approachStarted) return;
            var needNavMesh = _state is BotState.Approaching or BotState.MovingToSlot or BotState.MovingToTable;
            if (needNavMesh && (navigation == null || !navigation.IsReady)) return;

            switch (_state)
            {
                case BotState.Approaching:
                    if (navigation.HasPendingPath) break;
                    if (navigation.HasArrived)
                    {
                        _currentWaypointIndex++;
                        if (_currentWaypointIndex < _approachWaypoints.Length)
                            navigation.MoveTo(_approachWaypoints[_currentWaypointIndex].position);
                        else
                        {
                            _state = BotState.MovingToSlot;
                            if (_assignedSlot != null)
                                navigation.MoveTo(_assignedSlot.position);
                        }
                    }
                    break;

                case BotState.MovingToSlot:
                    if (navigation.HasPendingPath) break;
                    if (navigation.HasArrived)
                    {
                        _hasReachedAssignedSlot = true;
                        navigation.Stop();
                        _state = BotState.AtSlot;
                        if (_assignedSlot != null)
                            navigation.Face(_assignedSlot.rotation);
                    }
                    break;

                case BotState.WaitingForTable:
                    break;

                case BotState.MovingToTable:
                    if (dining == null || dining.ReservedSeatTransform == null)
                    {
                        Debug.LogWarning($"CustomerBot '{name}': no seat transform assigned. Releasing seat.", this);
                        dining?.ReleaseSeat();
                        _state = BotState.Leaving;
                        break;
                    }
                    if (navigation.HasPendingPath) break;
                    if (navigation.HasArrived)
                    {
                        BeginEating();
                    }
                    break;

                case BotState.Eating:
                    if (dining != null && dining.TickEating(Time.deltaTime))
                    {
                        BeginLeaving();
                    }
                    break;

                case BotState.Leaving:
                    if (dining == null || dining.ExitPoint == null || navigation == null || !navigation.IsReady)
                    {
                        Destroy(gameObject);
                        break;
                    }
                    if (navigation.HasArrived)
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
            var view = ResolveOrderView();
            if (view == null)
                return;

            view.ShowRemainingPizzas(RemainingPizzaCount);
        }

        private void ShowNoSeatMessage()
        {
            var view = ResolveOrderView();
            if (view == null)
                return;

            view.ShowNoSeatMessage();
        }

        private void HideNoSeatMessage()
        {
            var view = ResolveOrderView();
            if (view == null)
                return;

            view.Hide();
        }

        public void SetupDining(TableManager tableManager, Transform exitPoint, float eatingDuration)
        {
            if (dining != null)
                dining.Configure(tableManager, exitPoint, eatingDuration);
        }

        public void BeginApproach()
        {
            _approachStarted = true;
            if (_approachWaypoints != null && _approachWaypoints.Length > 0 && _approachWaypoints[0] != null)
            {
                _state = BotState.Approaching;
                if (navigation != null)
                    navigation.MoveTo(_approachWaypoints[0].position);
            }
            else
            {
                _state = BotState.MovingToSlot;
                if (_assignedSlot != null && navigation != null)
                    navigation.MoveTo(_assignedSlot.position);
            }
        }

        public void AssignQueueSlot(Transform queueSlot, int queueIndex)
        {
            _assignedSlot = queueSlot;
            _queueIndex = queueIndex;
            _hasReachedAssignedSlot = false;
            if (navigation != null && navigation.IsReady)
            {
                navigation.Resume();
                navigation.SetAvoidancePriority(queueIndex);
            }
            if (_state == BotState.AtSlot || _state == BotState.MovingToSlot)
            {
                _state = BotState.MovingToSlot;
                if (_assignedSlot != null)
                    navigation.MoveTo(_assignedSlot.position);
            }
        }

        public bool TransitionToDining()
        {
            var expectedLeftovers = _orderModel?.InitialPizzaCount ?? 0;
            if (dining != null && dining.TryReserveSeat(expectedLeftovers))
            {
                _state = BotState.MovingToTable;
                _pathTimeout = 0f;
                _hasReachedAssignedSlot = false;
                if (navigation != null && navigation.IsReady)
                    navigation.Resume();
                var seatTransform = dining.ReservedSeatTransform;
                if (seatTransform != null)
                    navigation.MoveTo(seatTransform.position);
                HideNoSeatMessage();
                return true;
            }

            _state = BotState.WaitingForTable;
            ShowNoSeatMessage();
            return false;
        }

        public bool RetryReserveTable()
        {
            if (_state != BotState.WaitingForTable)
                return false;

            var expectedLeftovers = _orderModel?.InitialPizzaCount ?? 0;
            if (dining != null && dining.TryReserveSeat(expectedLeftovers))
            {
                _state = BotState.MovingToTable;
                _pathTimeout = 0f;
                _hasReachedAssignedSlot = false;
                if (navigation != null && navigation.IsReady)
                    navigation.Resume();
                var seatTransform = dining.ReservedSeatTransform;
                if (seatTransform != null)
                    navigation.MoveTo(seatTransform.position);
                HideNoSeatMessage();
                return true;
            }

            return false;
        }

        public void BeginEating()
        {
            _state = BotState.Eating;
            _pathTimeout = 0f;
            if (dining != null)
                dining.StartEating();
            if (navigation != null && navigation.IsReady)
                navigation.Stop();
            if (dining != null && dining.ReservedSeatTransform != null && navigation != null)
                navigation.Face(dining.ReservedSeatTransform.rotation);
        }

        public void BeginLeaving()
        {
            _state = BotState.Leaving;
            if (navigation != null && navigation.IsReady)
            {
                navigation.Resume();
                var exitPoint = dining != null ? dining.ExitPoint : null;
                if (exitPoint != null)
                    navigation.MoveTo(exitPoint.position);
            }
        }

        private void ProcessPathTimeout()
        {
            _pathTimeout += Time.deltaTime;
            if (_pathTimeout < movingToTableTimeout)
                return;

            Debug.LogWarning($"CustomerBot '{name}': could not reach seat within {movingToTableTimeout}s. Releasing seat.", this);
            dining?.ReleaseSeat();
            _state = BotState.Leaving;
            _pathTimeout = 0f;
            if (navigation != null && navigation.IsReady)
            {
                navigation.Resume();
                var exitPoint = dining != null ? dining.ExitPoint : null;
                if (exitPoint != null)
                    navigation.MoveTo(exitPoint.position);
            }
        }

        private CustomerOrderView ResolveOrderView()
        {
            if (orderView == null)
                orderView = GetComponent<CustomerOrderView>();
            return orderView;
        }
    }
}
