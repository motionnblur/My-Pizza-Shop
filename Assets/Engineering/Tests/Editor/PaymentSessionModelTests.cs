using System;
using Engineering.Scripts.Domain.Payment;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class PaymentSessionModelTests
    {
        [Test]
        public void Constructor_RejectsZeroPaymentPerTick()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PaymentSessionModel(0));
        }

        [Test]
        public void Constructor_RejectsNegativePaymentPerTick()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PaymentSessionModel(-5));
        }

        [Test]
        public void IsActive_IsFalseAfterConstruction()
        {
            var model = new PaymentSessionModel(5);

            Assert.That(model.IsActive, Is.False);
        }

        [Test]
        public void Begin_ActivatesTheSession()
        {
            var model = new PaymentSessionModel(5);

            model.Begin();

            Assert.That(model.IsActive, Is.True);
        }

        [Test]
        public void Cancel_DeactivatesTheSession()
        {
            var model = new PaymentSessionModel(5);
            model.Begin();

            model.Cancel();

            Assert.That(model.IsActive, Is.False);
        }

        [Test]
        public void GetNextPayment_ReturnsZeroWhenInactive()
        {
            var model = new PaymentSessionModel(5);

            Assert.That(model.GetNextPayment(), Is.EqualTo(0));
        }

        [Test]
        public void GetNextPayment_ReturnsPaymentPerTickWhenActive()
        {
            var model = new PaymentSessionModel(7);
            model.Begin();

            Assert.That(model.GetNextPayment(), Is.EqualTo(7));
            Assert.That(model.GetNextPayment(), Is.EqualTo(7));
        }

        [Test]
        public void GetNextPayment_ReturnsZeroAfterCancel()
        {
            var model = new PaymentSessionModel(5);
            model.Begin();
            Assert.That(model.GetNextPayment(), Is.EqualTo(5));

            model.Cancel();

            Assert.That(model.GetNextPayment(), Is.EqualTo(0));
        }
    }
}
