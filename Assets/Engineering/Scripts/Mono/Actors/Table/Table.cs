using Engineering.Scripts.Domain.Table;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.Table
{
    public class Table : MonoBehaviour
    {
        [SerializeField] private Transform[] seatTransforms;
        [SerializeField] private TableWasteVisuals wasteVisuals;
        [SerializeField] private int maxLeftovers = 10;

        private TableModel _model;
        private TableWasteModel _wasteModel;

        public int SeatCount => seatTransforms != null ? seatTransforms.Length : 0;
        public bool HasAvailableSeat
        {
            get
            {
                EnsureModel();
                return _model != null && !_model.IsFull;
            }
        }
        public int MaxLeftovers => _wasteModel?.MaxLeftovers ?? maxLeftovers;
        public int LeftoverCount
        {
            get
            {
                EnsureModel();
                return _wasteModel != null ? _wasteModel.LeftoverCount : 0;
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
            _wasteModel = new TableWasteModel(maxLeftovers);
        }

        public void Initialize(int seatCapacity)
        {
            _model = new TableModel(seatCapacity);
            _wasteModel = new TableWasteModel(maxLeftovers);
        }

        private void EnsureModel()
        {
            if (_model == null && SeatCount > 0)
                _model = new TableModel(SeatCount);
            if (_wasteModel == null)
                _wasteModel = new TableWasteModel(maxLeftovers);
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

        public bool CanAcceptLeftovers(int amount)
        {
            EnsureModel();
            return _wasteModel != null && _wasteModel.CanAdd(amount);
        }

        public AddLeftoversResult AddLeftovers(int pizzaCount)
        {
            EnsureModel();
            if (_wasteModel == null)
                return AddLeftoversResult.InvalidAmount;

            var result = _wasteModel.TryAddLeftovers(pizzaCount);
            if (result.Added && wasteVisuals != null)
                wasteVisuals.Refresh(_wasteModel.LeftoverCount);

            return result;
        }

        public void ClearLeftovers()
        {
            EnsureModel();
            _wasteModel?.Clear();
            if (wasteVisuals != null)
                wasteVisuals.Clear();
        }

        public int TryRemoveLeftovers(int requestedAmount)
        {
            EnsureModel();
            if (_wasteModel == null)
                return 0;

            var removed = _wasteModel.TryRemoveLeftovers(requestedAmount);
            if (removed > 0 && wasteVisuals != null)
                wasteVisuals.Refresh(_wasteModel.LeftoverCount);

            return removed;
        }
    }
}
