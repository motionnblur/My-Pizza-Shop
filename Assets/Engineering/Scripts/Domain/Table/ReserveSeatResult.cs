namespace Engineering.Scripts.Domain.Table
{
    public readonly struct ReserveSeatResult
    {
        public bool Reserved { get; }
        public int SeatIndex { get; }

        public ReserveSeatResult(bool reserved, int seatIndex)
        {
            Reserved = reserved;
            SeatIndex = seatIndex;
        }

        public static ReserveSeatResult Full => new ReserveSeatResult(false, -1);
    }
}
