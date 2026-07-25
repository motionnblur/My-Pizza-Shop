using System;
using Engineering.Scripts.Domain.Inventory;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class PizzaInventoryModelTests
    {
        [Test]
        public void Constructor_RejectsNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PizzaInventoryModel(-1));
        }

        [Test]
        public void Constructor_AcceptsZeroCapacity()
        {
            var model = new PizzaInventoryModel(0);

            Assert.That(model.Capacity, Is.EqualTo(0));
            Assert.That(model.TryAdd(1), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryAdd_AcceptsFullRequestedAmount()
        {
            var model = new PizzaInventoryModel(10);

            var accepted = model.TryAdd(4);

            Assert.That(accepted, Is.EqualTo(4));
            Assert.That(model.Count, Is.EqualTo(4));
        }

        [Test]
        public void TryAdd_CapsAtRemainingCapacity()
        {
            var model = new PizzaInventoryModel(10);

            Assert.That(model.TryAdd(7), Is.EqualTo(7));
            Assert.That(model.TryAdd(5), Is.EqualTo(3));
            Assert.That(model.Count, Is.EqualTo(10));
        }

        [Test]
        public void TryAdd_ReturnsZeroWhenFull()
        {
            var model = new PizzaInventoryModel(2);
            model.TryAdd(2);

            Assert.That(model.TryAdd(1), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public void TryAdd_RejectsZeroAmount()
        {
            var model = new PizzaInventoryModel(10);

            Assert.That(model.TryAdd(0), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryAdd_RejectsNegativeAmount()
        {
            var model = new PizzaInventoryModel(10);

            Assert.That(model.TryAdd(-3), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryRemove_RemovesOnlyWhatIsStored()
        {
            var model = new PizzaInventoryModel(10);
            model.TryAdd(4);

            var removed = model.TryRemove(6);

            Assert.That(removed, Is.EqualTo(4));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryRemove_ReturnsZeroWhenEmpty()
        {
            var model = new PizzaInventoryModel(10);

            Assert.That(model.TryRemove(3), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryRemove_RejectsZeroAndNegativeAmounts()
        {
            var model = new PizzaInventoryModel(10);
            model.TryAdd(5);

            Assert.That(model.TryRemove(0), Is.EqualTo(0));
            Assert.That(model.TryRemove(-2), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(5));
        }

        [Test]
        public void Changed_RaisesWithNewCountOnAddAndRemove()
        {
            var model = new PizzaInventoryModel(10);
            var lastReportedCount = -1;
            model.Changed += count => lastReportedCount = count;

            model.TryAdd(3);
            Assert.That(lastReportedCount, Is.EqualTo(3));

            model.TryRemove(1);
            Assert.That(lastReportedCount, Is.EqualTo(2));
        }

        [Test]
        public void Changed_DoesNotRaiseForRejectedOperations()
        {
            var model = new PizzaInventoryModel(1);
            var invocationCount = 0;
            model.Changed += count => invocationCount++;

            model.TryAdd(0);
            model.TryRemove(0);
            Assert.That(invocationCount, Is.EqualTo(0));

            model.TryAdd(1);
            Assert.That(invocationCount, Is.EqualTo(1));

            model.TryAdd(1);
            Assert.That(invocationCount, Is.EqualTo(1));
        }
    }
}
