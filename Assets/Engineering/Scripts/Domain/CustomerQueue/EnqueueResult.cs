namespace Engineering.Scripts.Domain.CustomerQueue
{
    public readonly struct EnqueueResult
    {
        public bool Accepted { get; }
        public int QueueIndex { get; }

        public EnqueueResult(bool accepted, int queueIndex)
        {
            Accepted = accepted;
            QueueIndex = queueIndex;
        }

        public static EnqueueResult Rejected => new EnqueueResult(false, -1);
    }
}
