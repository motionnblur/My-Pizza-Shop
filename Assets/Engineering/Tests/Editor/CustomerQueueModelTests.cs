using System;
using Engineering.Scripts.Domain.CustomerQueue;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class CustomerQueueModelTests
    {
        #region CustomerOrderModel

        [Test]
        public void CustomerOrderModel_Constructor_RejectsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CustomerOrderModel(0));
        }

        [Test]
        public void CustomerOrderModel_Constructor_RejectsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CustomerOrderModel(-1));
        }

        [Test]
        public void CustomerOrderModel_Constructor_SetsInitialCount()
        {
            var order = new CustomerOrderModel(5);
            Assert.That(order.InitialPizzaCount, Is.EqualTo(5));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(5));
            Assert.That(order.IsCompleted, Is.False);
        }

        [Test]
        public void CustomerOrderModel_ZeroReceive_DoesNothing()
        {
            var order = new CustomerOrderModel(5);
            var accepted = order.ReceivePizzas(0);
            Assert.That(accepted, Is.EqualTo(0));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(5));
        }

        [Test]
        public void CustomerOrderModel_NegativeReceive_DoesNothing()
        {
            var order = new CustomerOrderModel(5);
            var accepted = order.ReceivePizzas(-3);
            Assert.That(accepted, Is.EqualTo(0));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(5));
        }

        [Test]
        public void CustomerOrderModel_PartialDelivery_ReturnsAcceptedAmount()
        {
            var order = new CustomerOrderModel(5);
            var accepted = order.ReceivePizzas(3);
            Assert.That(accepted, Is.EqualTo(3));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(2));
            Assert.That(order.IsCompleted, Is.False);
        }

        [Test]
        public void CustomerOrderModel_CompleteDelivery_ReturnsAcceptedAmount()
        {
            var order = new CustomerOrderModel(5);
            var accepted = order.ReceivePizzas(5);
            Assert.That(accepted, Is.EqualTo(5));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(0));
            Assert.That(order.IsCompleted, Is.True);
        }

        [Test]
        public void CustomerOrderModel_OverDelivery_ReturnsOnlyUpToRemaining()
        {
            var order = new CustomerOrderModel(5);
            var accepted = order.ReceivePizzas(10);
            Assert.That(accepted, Is.EqualTo(5));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(0));
            Assert.That(order.IsCompleted, Is.True);
        }

        [Test]
        public void CustomerOrderModel_SequentialReceives_Accumulate()
        {
            var order = new CustomerOrderModel(8);
            Assert.That(order.ReceivePizzas(3), Is.EqualTo(3));
            Assert.That(order.ReceivePizzas(2), Is.EqualTo(2));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(3));
            Assert.That(order.ReceivePizzas(5), Is.EqualTo(3));
            Assert.That(order.RemainingPizzaCount, Is.EqualTo(0));
            Assert.That(order.IsCompleted, Is.True);
        }

        [Test]
        public void CustomerOrderModel_CompletedOrder_SubsequentReceiveReturnsZero()
        {
            var order = new CustomerOrderModel(3);
            order.ReceivePizzas(3);
            Assert.That(order.IsCompleted, Is.True);
            var accepted = order.ReceivePizzas(5);
            Assert.That(accepted, Is.EqualTo(0));
        }

        #endregion

        #region CustomerQueueModel

        [Test]
        public void CustomerQueueModel_Constructor_RejectsZeroCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CustomerQueueModel(0));
        }

        [Test]
        public void CustomerQueueModel_Constructor_RejectsNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CustomerQueueModel(-1));
        }

        [Test]
        public void CustomerQueueModel_Constructor_SetsCapacity()
        {
            var queue = new CustomerQueueModel(5);
            Assert.That(queue.Capacity, Is.EqualTo(5));
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.IsFull, Is.False);
            Assert.That(queue.HasFront, Is.False);
            Assert.That(queue.Front, Is.Null);
        }

        [Test]
        public void CustomerQueueModel_Enqueue_RejectsNull()
        {
            var queue = new CustomerQueueModel(5);
            var result = queue.TryEnqueue(null);
            Assert.That(result.Accepted, Is.False);
        }

        [Test]
        public void CustomerQueueModel_Enqueue_AcceptsOrder()
        {
            var queue = new CustomerQueueModel(5);
            var order = new CustomerOrderModel(3);
            var result = queue.TryEnqueue(order);
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.QueueIndex, Is.EqualTo(0));
            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.HasFront, Is.True);
            Assert.That(queue.Front, Is.SameAs(order));
        }

        [Test]
        public void CustomerQueueModel_Enqueue_RejectsWhenFull()
        {
            var queue = new CustomerQueueModel(2);
            Assert.That(queue.TryEnqueue(new CustomerOrderModel(3)).Accepted, Is.True);
            Assert.That(queue.TryEnqueue(new CustomerOrderModel(3)).Accepted, Is.True);
            var result = queue.TryEnqueue(new CustomerOrderModel(3));
            Assert.That(result.Accepted, Is.False);
            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.IsFull, Is.True);
        }

        [Test]
        public void CustomerQueueModel_FIFO_FrontIsFirstEnqueued()
        {
            var queue = new CustomerQueueModel(3);
            var first = new CustomerOrderModel(1);
            var second = new CustomerOrderModel(2);
            queue.TryEnqueue(first);
            queue.TryEnqueue(second);
            Assert.That(queue.Front, Is.SameAs(first));
            Assert.That(queue.Front.InitialPizzaCount, Is.EqualTo(1));
        }

        [Test]
        public void CustomerQueueModel_RemoveFront_ReturnsEmptyWhenNoOrders()
        {
            var queue = new CustomerQueueModel(5);
            var result = queue.TryRemoveFront();
            Assert.That(result.HasRemoved, Is.False);
            Assert.That(result.RemovedOrder, Is.Null);
        }

        [Test]
        public void CustomerQueueModel_RemoveFront_RemovesAndReturnsFront()
        {
            var queue = new CustomerQueueModel(5);
            var first = new CustomerOrderModel(3);
            var second = new CustomerOrderModel(5);
            queue.TryEnqueue(first);
            queue.TryEnqueue(second);

            var result = queue.TryRemoveFront();
            Assert.That(result.HasRemoved, Is.True);
            Assert.That(result.RemovedOrder, Is.SameAs(first));
            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.Front, Is.SameAs(second));
        }

        [Test]
        public void CustomerQueueModel_RemoveFront_UpdatesIndices()
        {
            var queue = new CustomerQueueModel(5);
            queue.TryEnqueue(new CustomerOrderModel(3));
            queue.TryEnqueue(new CustomerOrderModel(5));
            queue.TryEnqueue(new CustomerOrderModel(2));

            queue.TryRemoveFront();
            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.Front.InitialPizzaCount, Is.EqualTo(5));

            queue.TryRemoveFront();
            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.Front.InitialPizzaCount, Is.EqualTo(2));

            queue.TryRemoveFront();
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.HasFront, Is.False);
        }

        [Test]
        public void CustomerQueueModel_EnqueueAfterRemove_UsesNextSlot()
        {
            var queue = new CustomerQueueModel(2);
            queue.TryEnqueue(new CustomerOrderModel(3));
            queue.TryEnqueue(new CustomerOrderModel(5));
            queue.TryRemoveFront();

            var newOrder = new CustomerOrderModel(2);
            var result = queue.TryEnqueue(newOrder);
            Assert.That(result.Accepted, Is.True);
            Assert.That(result.QueueIndex, Is.EqualTo(1));
            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.Front.InitialPizzaCount, Is.EqualTo(5));
        }

        [Test]
        public void CustomerQueueModel_DoesNotExposeMutableCollection()
        {
            var queueType = typeof(CustomerQueueModel);
            var properties = queueType.GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                {
                    Assert.Fail($"CustomerQueueModel exposes a List<> property: {prop.Name}");
                }
            }

            var fields = queueType.GetFields(System.Reflection.BindingFlags.Instance |
                                              System.Reflection.BindingFlags.Public |
                                              System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.IsPublic && field.FieldType.IsGenericType &&
                    field.FieldType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                {
                    Assert.Fail($"CustomerQueueModel exposes a public List<> field: {field.Name}");
                }
            }
        }

        #endregion
    }
}
