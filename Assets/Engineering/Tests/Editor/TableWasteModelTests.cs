using Engineering.Scripts.Domain.Table;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class TableWasteModelTests
    {
        [Test]
        public void Constructor_LeftoverCountIsZero()
        {
            var waste = new TableWasteModel();
            Assert.That(waste.LeftoverCount, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_DefaultMaxLeftovers()
        {
            var waste = new TableWasteModel();
            Assert.That(waste.MaxLeftovers, Is.EqualTo(10));
        }

        [Test]
        public void Constructor_CustomMaxLeftovers()
        {
            var waste = new TableWasteModel(5);
            Assert.That(waste.MaxLeftovers, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_ZeroMaxLeftovers_UsesDefault()
        {
            var waste = new TableWasteModel(0);
            Assert.That(waste.MaxLeftovers, Is.EqualTo(10));
        }

        [Test]
        public void Constructor_NegativeMaxLeftovers_UsesDefault()
        {
            var waste = new TableWasteModel(-5);
            Assert.That(waste.MaxLeftovers, Is.EqualTo(10));
        }

        [Test]
        public void IsFull_ReturnsTrueWhenAtCapacity()
        {
            var waste = new TableWasteModel(3);
            waste.TryAddLeftovers(3);
            Assert.That(waste.IsFull, Is.True);
        }

        [Test]
        public void IsFull_ReturnsFalseWhenBelowCapacity()
        {
            var waste = new TableWasteModel(3);
            waste.TryAddLeftovers(2);
            Assert.That(waste.IsFull, Is.False);
        }

        [Test]
        public void IsFull_ReturnsFalseWhenEmpty()
        {
            var waste = new TableWasteModel(3);
            Assert.That(waste.IsFull, Is.False);
        }

        [Test]
        public void RemainingCapacity_ReturnsFullCapacityInitially()
        {
            var waste = new TableWasteModel(10);
            Assert.That(waste.RemainingCapacity, Is.EqualTo(10));
        }

        [Test]
        public void RemainingCapacity_DecreasesAfterAddition()
        {
            var waste = new TableWasteModel(10);
            waste.TryAddLeftovers(3);
            Assert.That(waste.RemainingCapacity, Is.EqualTo(7));
        }

        [Test]
        public void RemainingCapacity_ZeroWhenFull()
        {
            var waste = new TableWasteModel(5);
            waste.TryAddLeftovers(5);
            Assert.That(waste.RemainingCapacity, Is.EqualTo(0));
        }

        [Test]
        public void CanAdd_ReturnsTrueWhenWithinCapacity()
        {
            var waste = new TableWasteModel(10);
            Assert.That(waste.CanAdd(5), Is.True);
        }

        [Test]
        public void CanAdd_ReturnsTrueWhenExactlyFits()
        {
            var waste = new TableWasteModel(10);
            waste.TryAddLeftovers(5);
            Assert.That(waste.CanAdd(5), Is.True);
        }

        [Test]
        public void CanAdd_ReturnsFalseWhenExceedsCapacity()
        {
            var waste = new TableWasteModel(10);
            waste.TryAddLeftovers(8);
            Assert.That(waste.CanAdd(3), Is.False);
        }

        [Test]
        public void CanAdd_ReturnsFalseForZero()
        {
            var waste = new TableWasteModel(10);
            Assert.That(waste.CanAdd(0), Is.False);
        }

        [Test]
        public void CanAdd_ReturnsFalseForNegative()
        {
            var waste = new TableWasteModel(10);
            Assert.That(waste.CanAdd(-1), Is.False);
        }

        [Test]
        public void TryAddLeftovers_AddsPositiveCount()
        {
            var waste = new TableWasteModel();
            var result = waste.TryAddLeftovers(3);
            Assert.That(result.Added, Is.True);
            Assert.That(result.CurrentCount, Is.EqualTo(3));
            Assert.That(waste.LeftoverCount, Is.EqualTo(3));
        }

        [Test]
        public void TryAddLeftovers_RejectsZero()
        {
            var waste = new TableWasteModel();
            var result = waste.TryAddLeftovers(0);
            Assert.That(result.Added, Is.False);
            Assert.That(waste.LeftoverCount, Is.EqualTo(0));
            Assert.That(result.Reason, Is.EqualTo(AddLeftoverFailureReason.InvalidAmount));
        }

        [Test]
        public void TryAddLeftovers_RejectsNegative()
        {
            var waste = new TableWasteModel();
            var result = waste.TryAddLeftovers(-1);
            Assert.That(result.Added, Is.False);
            Assert.That(waste.LeftoverCount, Is.EqualTo(0));
            Assert.That(result.Reason, Is.EqualTo(AddLeftoverFailureReason.InvalidAmount));
        }

        [Test]
        public void TryAddLeftovers_RejectsWhenCapacityExceeded()
        {
            var waste = new TableWasteModel(5);
            waste.TryAddLeftovers(4);
            var result = waste.TryAddLeftovers(2);
            Assert.That(result.Added, Is.False);
            Assert.That(result.Reason, Is.EqualTo(AddLeftoverFailureReason.CapacityExceeded));
            Assert.That(waste.LeftoverCount, Is.EqualTo(4));
        }

        [Test]
        public void TryAddLeftovers_RejectsWhenExactlyOneOver()
        {
            var waste = new TableWasteModel(5);
            waste.TryAddLeftovers(5);
            var result = waste.TryAddLeftovers(1);
            Assert.That(result.Added, Is.False);
            Assert.That(result.Reason, Is.EqualTo(AddLeftoverFailureReason.CapacityExceeded));
            Assert.That(waste.LeftoverCount, Is.EqualTo(5));
        }

        [Test]
        public void TryAddLeftovers_AccumulatesFromMultipleSources()
        {
            var waste = new TableWasteModel();
            waste.TryAddLeftovers(2);
            waste.TryAddLeftovers(3);
            Assert.That(waste.LeftoverCount, Is.EqualTo(5));
        }

        [Test]
        public void Clear_ResetsToZero()
        {
            var waste = new TableWasteModel();
            waste.TryAddLeftovers(4);
            waste.Clear();
            Assert.That(waste.LeftoverCount, Is.EqualTo(0));
        }

        [Test]
        public void Clear_AfterClear_CanAddAgain()
        {
            var waste = new TableWasteModel(5);
            waste.TryAddLeftovers(5);
            waste.Clear();
            Assert.That(waste.CanAdd(5), Is.True);
        }

        [Test]
        public void AddLeftoversResult_InvalidAmount_HasCorrectValues()
        {
            var result = AddLeftoversResult.InvalidAmount;
            Assert.That(result.Added, Is.False);
            Assert.That(result.CurrentCount, Is.EqualTo(-1));
            Assert.That(result.Reason, Is.EqualTo(AddLeftoverFailureReason.InvalidAmount));
        }

        [Test]
        public void AddLeftoversResult_CapacityExceeded_HasCorrectValues()
        {
            var result = AddLeftoversResult.CapacityExceeded;
            Assert.That(result.Added, Is.False);
            Assert.That(result.Reason, Is.EqualTo(AddLeftoverFailureReason.CapacityExceeded));
        }

        [Test]
        public void SharedAccumulation_TwoCustomerAdds()
        {
            var waste = new TableWasteModel();
            waste.TryAddLeftovers(1);
            waste.TryAddLeftovers(4);
            Assert.That(waste.LeftoverCount, Is.EqualTo(5));
        }
    }
}
