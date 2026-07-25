using System;

namespace Engineering.Scripts.Domain.ServeStation
{
    public readonly struct ServeResult
    {
        public int DeliveredPizzaCount { get; }
        public int MoneyEarned { get; }
        public bool OrderCompleted { get; }
        public bool HasDelivery => DeliveredPizzaCount > 0;

        public ServeResult(int deliveredPizzaCount, int moneyEarned, bool orderCompleted)
        {
            DeliveredPizzaCount = deliveredPizzaCount;
            MoneyEarned = moneyEarned;
            OrderCompleted = orderCompleted;
        }

        public static ServeResult None => new ServeResult(0, 0, false);
    }
}
