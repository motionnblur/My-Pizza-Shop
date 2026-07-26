using Engineering.Scripts.Domain.Table;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.Table
{
    public class Table : MonoBehaviour
    {
        [SerializeField] private Transform[] seatTransforms;

        private TableModel _model;

        public int SeatCount => seatTransforms != null ? seatTransforms.Length : 0;
        public bool HasAvailableSeat
        {
            get
            {
                EnsureModel();
                return _model != null && !_model.IsFull;
            }
        }

        private void Awake()
        {
            if (seatTransforms == null || seatTransforms.Length == 0)
            {
                Debug.LogWarning($"Table '{name}' has no seat transforms assigned. " +
                    "Assign seat/pivot child transforms to the seatTransforms array in the Inspector.", this);
                return;
            }

            for (var i = 0; i < seatTransforms.Length; i++)
            {
                if (seatTransforms[i] == null)
                {
                    Debug.LogWarning($"Table '{name}' has a null entry at seatTransforms[{i}]. " +
                        "This seat will be inaccessible.", this);
                }
            }

            _model = new TableModel(seatTransforms.Length);
        }

        public void Initialize(int seatCapacity)
        {
            _model = new TableModel(seatCapacity);
        }

        private void EnsureModel()
        {
            if (_model == null && SeatCount > 0)
                _model = new TableModel(SeatCount);
        }

        public ReserveSeatResult TryReserveSeat()
        {
            EnsureModel();
            if (_model == null)
                return ReserveSeatResult.Full;
            return _model.TryReserveSeat();
        }

        public ReleaseSeatResult TryReleaseSeat(int seatIndex)
        {
            EnsureModel();
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
