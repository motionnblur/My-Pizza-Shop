using System;
using Engineering.Scripts.Domain.Economy;
using NUnit.Framework;

namespace Engineering.Tests
{
    public class WalletModelTests
    {
        [Test]
        public void Constructor_RejectsNegativeStartingMoney()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WalletModel(-1));
        }

        [Test]
        public void Constructor_AcceptsZeroStartingMoney()
        {
            var model = new WalletModel(0);

            Assert.That(model.Money, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_DecreasesMoneyAndRaisesBalanceChangedOnce()
        {
            var model = new WalletModel(100);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.TrySpend(30);

            Assert.That(result, Is.True);
            Assert.That(model.Money, Is.EqualTo(70));
            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [Test]
        public void TrySpend_FailsWithoutEnoughMoneyAndRaisesNoEvent()
        {
            var model = new WalletModel(50);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.TrySpend(100);

            Assert.That(result, Is.False);
            Assert.That(model.Money, Is.EqualTo(50));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_RejectsZeroAmount()
        {
            var model = new WalletModel(100);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.TrySpend(0);

            Assert.That(result, Is.False);
            Assert.That(model.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_RejectsNegativeAmount()
        {
            var model = new WalletModel(100);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.TrySpend(-10);

            Assert.That(result, Is.False);
            Assert.That(model.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void Credit_IncreasesMoneyAndRaisesBalanceChangedOnce()
        {
            var model = new WalletModel(100);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.Credit(25);

            Assert.That(result, Is.True);
            Assert.That(model.Money, Is.EqualTo(125));
            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [Test]
        public void Credit_RejectsZeroAmount()
        {
            var model = new WalletModel(100);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.Credit(0);

            Assert.That(result, Is.False);
            Assert.That(model.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void Credit_RejectsNegativeAmount()
        {
            var model = new WalletModel(100);
            var invocationCount = 0;
            model.BalanceChanged += balance => invocationCount++;

            var result = model.Credit(-25);

            Assert.That(result, Is.False);
            Assert.That(model.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void BalanceChanged_CarriesTheNewBalance()
        {
            var model = new WalletModel(100);
            var reportedBalance = -1;
            model.BalanceChanged += balance => reportedBalance = balance;

            model.TrySpend(40);

            Assert.That(reportedBalance, Is.EqualTo(60));
        }
    }
}
