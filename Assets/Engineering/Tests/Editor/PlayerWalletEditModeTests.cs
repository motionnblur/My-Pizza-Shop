using System.Reflection;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;

namespace Engineering.Tests
{
    public class PlayerWalletEditModeTests
    {
        [Test]
        public void Credit_IncreasesMoneyAndRaisesBalanceChangedOnce()
        {
            var wallet = CreateWallet(startMoney: 100);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.Credit(25);

            Assert.That(result, Is.True);
            Assert.That(wallet.Money, Is.EqualTo(125));
            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [Test]
        public void TrySpend_DecreasesMoneyAndRaisesBalanceChangedOnce()
        {
            var wallet = CreateWallet(startMoney: 100);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.TrySpend(30);

            Assert.That(result, Is.True);
            Assert.That(wallet.Money, Is.EqualTo(70));
            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [Test]
        public void TrySpend_FailsWithoutEnoughMoneyAndRaisesNoEvent()
        {
            var wallet = CreateWallet(startMoney: 50);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.TrySpend(100);

            Assert.That(result, Is.False);
            Assert.That(wallet.Money, Is.EqualTo(50));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_RejectsZeroAmount()
        {
            var wallet = CreateWallet(startMoney: 100);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.TrySpend(0);

            Assert.That(result, Is.False);
            Assert.That(wallet.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_RejectsNegativeAmount()
        {
            var wallet = CreateWallet(startMoney: 100);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.TrySpend(-10);

            Assert.That(result, Is.False);
            Assert.That(wallet.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void Credit_RejectsZeroAmount()
        {
            var wallet = CreateWallet(startMoney: 100);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.Credit(0);

            Assert.That(result, Is.False);
            Assert.That(wallet.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [Test]
        public void Credit_RejectsNegativeAmount()
        {
            var wallet = CreateWallet(startMoney: 100);
            var invocationCount = 0;
            wallet.BalanceChanged += balance => invocationCount++;

            var result = wallet.Credit(-25);

            Assert.That(result, Is.False);
            Assert.That(wallet.Money, Is.EqualTo(100));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        private static PlayerWallet CreateWallet(int startMoney)
        {
            var go = new GameObject("WalletTest");
            var wallet = go.AddComponent<PlayerWallet>();
            SetPrivateField(wallet, "money", startMoney);
            return wallet;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
