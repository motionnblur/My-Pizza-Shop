using Engineering.Scripts.Domain.Table;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.Table
{
    public class Table : MonoBehaviour
    {
        [SerializeField] private Transform[] seatTransforms;

        private TableModel _model;

        public int SeatCount => seatTransforms != null ? seatTransforms.Length : 0;
        public bool HasAvailableSeat => _model != null && !_model.IsFull;

        private void Awake()
        {
            if (seatTransforms != null && seatTransforms.Length > 0)
                _model = new TableModel(seatTransforms.Length);
        }

        public void Initialize(int seatCapacity)
        {
            _model = new TableModel(seatCapacity);
        }

        public ReserveSeatResult TryReserveSeat()
        {
            if (_model == null)
                return ReserveSeatResult.Full;
            return _model.TryReserveSeat();
        }

        public ReleaseSeatResult TryReleaseSeat(int seatIndex)
        {
            if (_model == null)
                return ReleaseSeatResult.InvalidIndex;
            return _model.TryReleaseSeat(seatIndex);
        }

        public Transform GetSeatTransform(int seatIndex)
        {
            if (seatTransforms == null || seatIndex < 0 || seatIndex >= seatTransforms.Length)
                return null;
            return seatTransforms[seatIndex];
        }
    }
}
