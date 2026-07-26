using System;

namespace Engineering.Scripts.Domain.Table
{
    public sealed class TableModel
    {
        private readonly int _seatCapacity;
        private readonly bool[] _occupiedSeats;

        public int SeatCapacity => _seatCapacity;
        public bool IsFull => AvailableSeatCount == 0;

        public int AvailableSeatCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _occupiedSeats.Length; i++)
                {
                    if (!_occupiedSeats[i])
                        count++;
                }
                return count;
            }
        }

        public TableModel(int seatCapacity)
        {
            if (seatCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(seatCapacity), "Table must have a positive seat capacity.");
            _seatCapacity = seatCapacity;
            _occupiedSeats = new bool[seatCapacity];
        }

        public ReserveSeatResult TryReserveSeat()
        {
            for (var i = 0; i < _occupiedSeats.Length; i++)
            {
                if (!_occupiedSeats[i])
                {
                    _occupiedSeats[i] = true;
                    return new ReserveSeatResult(true, i);
                }
            }
            return ReserveSeatResult.Full;
        }

        public ReleaseSeatResult TryReleaseSeat(int seatIndex)
        {
            if (seatIndex < 0 || seatIndex >= _seatCapacity)
                return ReleaseSeatResult.InvalidIndex;
            if (!_occupiedSeats[seatIndex])
                return ReleaseSeatResult.NotOccupied;
            _occupiedSeats[seatIndex] = false;
            return new ReleaseSeatResult(true, seatIndex);
        }
    }
}
