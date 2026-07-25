using System;
using Engineering.Scripts.Domain.Purchase;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class PurchaseProgressModelTests
    {
        [Test]
        public void Constructor_RejectsZeroUnlockPrice()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PurchaseProgressModel(0));
        }

        [Test]
        public void Constructor_RejectsNegativeUnlockPrice()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PurchaseProgressModel(-1));
        }

        [Test]
        public void NewModel_StartsWithZeroPaidAmount()
        {
            var model = new PurchaseProgressModel(100);
            Assert.That(model.PaidAmount, Is.EqualTo(0));
        }

        [Test]
        public void NewModel_StartsWithFullRemainingAmount()
        {
            var model = new PurchaseProgressModel(100);
            Assert.That(model.RemainingAmount, Is.EqualTo(100));
        }

        [Test]
        public void NewModel_StartsIncomplete()
        {
            var model = new PurchaseProgressModel(100);
            Assert.That(model.IsPurchased, Is.False);
        }

        [Test]
        public void NegativePayment_ChangesNothing()
        {
            var model = new PurchaseProgressModel(100);
            var result = model.ApplyPayment(-10);
            Assert.That(result.HasAppliedPayment, Is.False);
            Assert.That(model.PaidAmount, Is.EqualTo(0));
            Assert.That(model.IsPurchased, Is.False);
        }

        [Test]
        public void ZeroPayment_ChangesNothing()
        {
            var model = new PurchaseProgressModel(100);
            var result = model.ApplyPayment(0);
            Assert.That(result.HasAppliedPayment, Is.False);
            Assert.That(model.PaidAmount, Is.EqualTo(0));
            Assert.That(model.IsPurchased, Is.False);
        }

        [Test]
        public void PartialPayment_IncreasesPaidAmount()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(30);
            Assert.That(model.PaidAmount, Is.EqualTo(30));
        }

        [Test]
        public void PartialPayment_DecreasesRemainingAmount()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(30);
            Assert.That(model.RemainingAmount, Is.EqualTo(70));
        }

        [Test]
        public void PartialPayment_DoesNotCompletePurchase()
        {
            var model = new PurchaseProgressModel(100);
            var result = model.ApplyPayment(30);
            Assert.That(model.IsPurchased, Is.False);
            Assert.That(result.CompletedNow, Is.False);
        }

        [Test]
        public void SequentialPartialPayments_Accumulate()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(30);
            model.ApplyPayment(40);
            Assert.That(model.PaidAmount, Is.EqualTo(70));
            Assert.That(model.RemainingAmount, Is.EqualTo(30));
        }

        [Test]
        public void ExactPayment_CompletesPurchase()
        {
            var model = new PurchaseProgressModel(100);
            var result = model.ApplyPayment(100);
            Assert.That(model.IsPurchased, Is.True);
            Assert.That(result.CompletedNow, Is.True);
        }

        [Test]
        public void ExactPayment_ReportsCompletedNowOnce()
        {
            var model = new PurchaseProgressModel(100);
            var firstResult = model.ApplyPayment(100);
            Assert.That(firstResult.CompletedNow, Is.True);

            var secondResult = model.ApplyPayment(0);
            Assert.That(secondResult.CompletedNow, Is.False);
        }

        [Test]
        public void PaymentLargerThanRemaining_IsAppliedInFull()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(95);
            var result = model.ApplyPayment(10);
            Assert.That(model.PaidAmount, Is.EqualTo(105));
            Assert.That(result.AppliedAmount, Is.EqualTo(10));
        }

        [Test]
        public void RemainingAmount_IsZeroAfterOverpayment()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(95);
            model.ApplyPayment(10);
            Assert.That(model.RemainingAmount, Is.EqualTo(0));
        }

        [Test]
        public void Overpayment_CompletesPurchase()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(95);
            model.ApplyPayment(10);
            Assert.That(model.IsPurchased, Is.True);
        }

        [Test]
        public void PaymentAfterCompletion_ChangesNoState()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(100);
            Assert.That(model.IsPurchased, Is.True);

            var result = model.ApplyPayment(50);
            Assert.That(model.PaidAmount, Is.EqualTo(100));
            Assert.That(model.IsPurchased, Is.True);
            Assert.That(result.HasAppliedPayment, Is.False);
        }

        [Test]
        public void PaymentAfterCompletion_DoesNotReportCompletionAgain()
        {
            var model = new PurchaseProgressModel(100);
            model.ApplyPayment(100);
            var result = model.ApplyPayment(50);
            Assert.That(result.CompletedNow, Is.False);
        }

        [Test]
        public void HasAppliedPayment_IsFalseForIgnoredPayment()
        {
            var model = new PurchaseProgressModel(100);
            var result = model.ApplyPayment(0);
            Assert.That(result.HasAppliedPayment, Is.False);
        }

        [Test]
        public void HasAppliedPayment_IsTrueForAcceptedPositivePayment()
        {
            var model = new PurchaseProgressModel(100);
            var result = model.ApplyPayment(10);
            Assert.That(result.HasAppliedPayment, Is.True);
        }
    }
}
