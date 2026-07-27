using System;
using Engineering.Scripts.Domain.Inventory;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class WasteInventoryModelTests
    {
        [Test]
        public void Constructor_RejectsNegativeCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WasteInventoryModel(-1));
        }

        [Test]
        public void Constructor_AcceptsZeroCapacity()
        {
            var model = new WasteInventoryModel(0);

            Assert.That(model.Capacity, Is.EqualTo(0));
            Assert.That(model.TryAdd(1), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void NewModel_HasZeroCount()
        {
            var model = new WasteInventoryModel(10);

            Assert.That(model.Count, Is.EqualTo(0));
            Assert.That(model.RemainingCapacity, Is.EqualTo(10));
            Assert.That(model.IsFull, Is.False);
        }

        [Test]
        public void TryAdd_AcceptsFullRequestedAmount()
        {
            var model = new WasteInventoryModel(10);

            var accepted = model.TryAdd(4);

            Assert.That(accepted, Is.EqualTo(4));
            Assert.That(model.Count, Is.EqualTo(4));
            Assert.That(model.RemainingCapacity, Is.EqualTo(6));
            Assert.That(model.IsFull, Is.False);
        }

        [Test]
        public void TryAdd_CapsAtRemainingCapacity()
        {
            var model = new WasteInventoryModel(10);

            Assert.That(model.TryAdd(7), Is.EqualTo(7));
            Assert.That(model.TryAdd(5), Is.EqualTo(3));
            Assert.That(model.Count, Is.EqualTo(10));
            Assert.That(model.IsFull, Is.True);
        }

        [Test]
        public void TryAdd_ReturnsZeroWhenFull()
        {
            var model = new WasteInventoryModel(2);
            model.TryAdd(2);

            Assert.That(model.TryAdd(1), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public void TryAdd_RejectsZeroAmount()
        {
            var model = new WasteInventoryModel(10);

            Assert.That(model.TryAdd(0), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryAdd_RejectsNegativeAmount()
        {
            var model = new WasteInventoryModel(10);

            Assert.That(model.TryAdd(-3), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryRemove_RemovesOnlyWhatIsStored()
        {
            var model = new WasteInventoryModel(10);
            model.TryAdd(4);

            var removed = model.TryRemove(6);

            Assert.That(removed, Is.EqualTo(4));
            Assert.That(model.Count, Is.EqualTo(0));
            Assert.That(model.RemainingCapacity, Is.EqualTo(10));
        }

        [Test]
        public void TryRemove_ReturnsZeroWhenEmpty()
        {
            var model = new WasteInventoryModel(10);

            Assert.That(model.TryRemove(3), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryRemove_RejectsZeroAndNegativeAmounts()
        {
            var model = new WasteInventoryModel(10);
            model.TryAdd(5);

            Assert.That(model.TryRemove(0), Is.EqualTo(0));
            Assert.That(model.TryRemove(-2), Is.EqualTo(0));
            Assert.That(model.Count, Is.EqualTo(5));
        }

        [Test]
        public void TryRemove_PartialTransfer_LeavesRemainder()
        {
            var model = new WasteInventoryModel(10);
            model.TryAdd(7);

            var removed = model.TryRemove(3);

            Assert.That(removed, Is.EqualTo(3));
            Assert.That(model.Count, Is.EqualTo(4));
            Assert.That(model.RemainingCapacity, Is.EqualTo(6));
        }

        [Test]
        public void Clear_ResetsCountToZero()
        {
            var model = new WasteInventoryModel(10);
            model.TryAdd(5);

            model.Clear();

            Assert.That(model.Count, Is.EqualTo(0));
            Assert.That(model.IsFull, Is.False);
        }

        [Test]
        public void Clear_OnEmptyModel_DoesNotError()
        {
            var model = new WasteInventoryModel(10);

            Assert.DoesNotThrow(() => model.Clear());
            Assert.That(model.Count, Is.EqualTo(0));
        }

        [Test]
        public void Changed_RaisesWithNewCountOnAdd()
        {
            var model = new WasteInventoryModel(10);
            var lastReportedCount = -1;
            model.Changed += count => lastReportedCount = count;

            model.TryAdd(3);

            Assert.That(lastReportedCount, Is.EqualTo(3));
        }

        [Test]
        public void Changed_RaisesWithNewCountOnRemove()
        {
            var model = new WasteInventoryModel(10);
            model.TryAdd(5);
            var lastReportedCount = -1;
            model.Changed += count => lastReportedCount = count;

            model.TryRemove(2);

            Assert.That(lastReportedCount, Is.EqualTo(3));
        }

        [Test]
        public void Changed_RaisesWithZeroOnClear()
        {
            var model = new WasteInventoryModel(10);
            model.TryAdd(5);
            var lastReportedCount = -1;
            model.Changed += count => lastReportedCount = count;

            model.Clear();

            Assert.That(lastReportedCount, Is.EqualTo(0));
        }

        [Test]
        public void Changed_DoesNotRaiseForRejectedOperations()
        {
            var model = new WasteInventoryModel(1);
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

        [Test]
        public void Changed_DoesNotRaiseForClearOnEmpty()
        {
            var model = new WasteInventoryModel(10);
            var invocationCount = 0;
            model.Changed += count => invocationCount++;

            model.Clear();

            Assert.That(invocationCount, Is.EqualTo(0));
        }
    }
}
