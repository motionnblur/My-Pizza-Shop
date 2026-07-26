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
        }

        [Test]
        public void TryAddLeftovers_RejectsNegative()
        {
            var waste = new TableWasteModel();
            var result = waste.TryAddLeftovers(-1);
            Assert.That(result.Added, Is.False);
            Assert.That(waste.LeftoverCount, Is.EqualTo(0));
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
        public void AddLeftoversResult_InvalidAmount_HasCorrectValues()
        {
            var result = AddLeftoversResult.InvalidAmount;
            Assert.That(result.Added, Is.False);
            Assert.That(result.CurrentCount, Is.EqualTo(-1));
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
