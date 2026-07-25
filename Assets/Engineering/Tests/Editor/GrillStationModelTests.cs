using System;
using Engineering.Scripts.Domain.GrillStation;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class GrillStationModelTests
    {
        [Test]
        public void Constructor_RejectsNegativeMaxReadyPizzas()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GrillStationModel(-1));
        }

        [Test]
        public void ZeroMaximum_NeverProduces()
        {
            var model = new GrillStationModel(0);

            Assert.That(model.CanProduce, Is.False);
            Assert.That(model.TryProduceOne(), Is.False);
            Assert.That(model.ReadyPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void TryProduceOne_IncrementsUntilMaximum()
        {
            var model = new GrillStationModel(2);

            Assert.That(model.TryProduceOne(), Is.True);
            Assert.That(model.TryProduceOne(), Is.True);
            Assert.That(model.TryProduceOne(), Is.False);
            Assert.That(model.ReadyPizzaCount, Is.EqualTo(2));
        }

        [Test]
        public void CanProduce_ReflectsCapacity()
        {
            var model = new GrillStationModel(1);

            Assert.That(model.CanProduce, Is.True);

            model.TryProduceOne();

            Assert.That(model.CanProduce, Is.False);
        }

        [Test]
        public void RemoveReady_RemovesRequestedAmount()
        {
            var model = new GrillStationModel(5);
            model.TryProduceOne();
            model.TryProduceOne();
            model.TryProduceOne();

            var removed = model.RemoveReady(2);

            Assert.That(removed, Is.EqualTo(2));
            Assert.That(model.ReadyPizzaCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveReady_ClampsToReadyCount()
        {
            var model = new GrillStationModel(5);
            model.TryProduceOne();
            model.TryProduceOne();

            var removed = model.RemoveReady(10);

            Assert.That(removed, Is.EqualTo(2));
            Assert.That(model.ReadyPizzaCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveReady_RejectsZeroAndNegativeAmounts()
        {
            var model = new GrillStationModel(5);
            model.TryProduceOne();

            Assert.That(model.RemoveReady(0), Is.EqualTo(0));
            Assert.That(model.RemoveReady(-2), Is.EqualTo(0));
            Assert.That(model.ReadyPizzaCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveReady_AllowsProductionAgain()
        {
            var model = new GrillStationModel(1);
            model.TryProduceOne();

            model.RemoveReady(1);

            Assert.That(model.CanProduce, Is.True);
            Assert.That(model.TryProduceOne(), Is.True);
        }

        [Test]
        public void UpdateConfiguration_RaisesMaximumAndAllowsMoreProduction()
        {
            var model = new GrillStationModel(1);
            model.TryProduceOne();
            Assert.That(model.TryProduceOne(), Is.False);

            model.UpdateConfiguration(3);

            Assert.That(model.MaxReadyPizzas, Is.EqualTo(3));
            Assert.That(model.TryProduceOne(), Is.True);
            Assert.That(model.ReadyPizzaCount, Is.EqualTo(2));
        }

        [Test]
        public void UpdateConfiguration_RejectsNegativeValue()
        {
            var model = new GrillStationModel(2);

            Assert.Throws<ArgumentOutOfRangeException>(() => model.UpdateConfiguration(-1));
            Assert.That(model.MaxReadyPizzas, Is.EqualTo(2));
        }
    }
}
