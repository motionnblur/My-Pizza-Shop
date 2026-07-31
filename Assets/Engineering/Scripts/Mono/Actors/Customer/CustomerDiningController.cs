using Engineering.Scripts.Mono.Actors.Table;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    public class CustomerDiningController : MonoBehaviour
    {
        private TableManager _tableManager;
        private int _tableIndex = -1;
        private int _seatIndex = -1;
        private Transform _assignedSeatTransform;
        private Transform _exitPoint;
        private float _eatingDuration;
        private float _eatingTimer;
        private int _expectedLeftoverCount;
        private bool _hasReservedSeat;
        private bool _hasFinishedEating;
        private bool _leftoversAdded;

        public Transform ReservedSeatTransform => _assignedSeatTransform;
        public Transform ExitPoint => _exitPoint;

        public void Configure(TableManager tableManager, Transform exitPoint, float eatingDuration)
        {
            _tableManager = tableManager;
            _exitPoint = exitPoint;
            _eatingDuration = eatingDuration;
        }

        public bool TryReserveSeat(int expectedLeftoverCount)
        {
            if (_tableManager == null)
                return false;

            if (!_tableManager.TryReserveSeat(expectedLeftoverCount, out var tableIndex, out var seatIndex))
                return false;

            _tableIndex = tableIndex;
            _seatIndex = seatIndex;
            _assignedSeatTransform = _tableManager.GetSeatTransform(tableIndex, seatIndex);
            _expectedLeftoverCount = expectedLeftoverCount;
            _hasReservedSeat = true;
            _hasFinishedEating = false;
            _leftoversAdded = false;
            return true;
        }

        public void StartEating()
        {
            _eatingTimer = _eatingDuration;
        }

        public bool TickEating(float deltaTime)
        {
            if (_hasFinishedEating)
                return true;

            _eatingTimer -= deltaTime;
            if (_eatingTimer > 0f)
                return false;

            AddLeftovers();
            ReleaseSeat();
            _hasFinishedEating = true;
            return true;
        }

        public bool ReleaseSeat()
        {
            if (!_hasReservedSeat || _tableManager == null)
                return false;

            _hasReservedSeat = false;
            _tableManager.ReleaseSeat(_tableIndex, _seatIndex);
            _tableIndex = -1;
            _seatIndex = -1;
            _assignedSeatTransform = null;
            return true;
        }

        private void AddLeftovers()
        {
            if (_leftoversAdded || _tableManager == null)
                return;

            _leftoversAdded = true;
            _tableManager.AddLeftoversToTable(_tableIndex, _expectedLeftoverCount);
        }
    }
}
