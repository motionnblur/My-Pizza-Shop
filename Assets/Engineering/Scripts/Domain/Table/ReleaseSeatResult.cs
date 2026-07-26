namespace Engineering.Scripts.Domain.Table
{
    public readonly struct ReleaseSeatResult
    {
        public bool Released { get; }
        public int SeatIndex { get; }

        public ReleaseSeatResult(bool released, int seatIndex)
        {
            Released = released;
            SeatIndex = seatIndex;
        }

        public static ReleaseSeatResult InvalidIndex => new ReleaseSeatResult(false, -1);
        public static ReleaseSeatResult NotOccupied => new ReleaseSeatResult(false, -1);
    }
}
