using System;
using System.Collections;
using System.Reflection;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class EconomyPaymentPlayModeTests
    {
        [UnityTest]
        public IEnumerator EnteringBuyingArea_TransfersTheConfiguredPaymentRate()
        {
            var fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            yield return null;

            InvokePrivateMethod(fixture.BuyingArea, "OnTriggerEnter", fixture.PlayerCollider);
            yield return null;

            Assert.That(fixture.Wallet.Money, Is.EqualTo(95));
            Assert.That(fixture.BuyingArea, Is.Not.Null);

            yield return DestroyFixture(fixture);
        }

        [UnityTest]
        public IEnumerator LeavingBuyingArea_CancelsFurtherScheduledPayments()
        {
            var fixture = CreateFixture(spendRate: 5, spendSpeed: 20f);
            yield return null;

            InvokePrivateMethod(fixture.BuyingArea, "OnTriggerEnter", fixture.PlayerCollider);
            InvokePrivateMethod(fixture.BuyingArea, "OnTriggerExit", fixture.PlayerCollider);
            yield return new WaitForSeconds(0.1f);

            Assert.That(fixture.Wallet.Money, Is.EqualTo(95));

            yield return DestroyFixture(fixture);
        }

        [UnityTest]
        public IEnumerator CompletingPurchase_DestroysTheBuyingArea()
        {
            var fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            yield return null;

            fixture.BuyingArea.AddPayment(100);
            yield return null;

            Assert.That(fixture.BuyingArea == null, Is.True);

            yield return DestroyFixture(fixture);
        }

        [UnityTest]
        public IEnumerator CompletingPurchaseDuringPayment_UsesCachedAnimationDestination()
        {
            var fixture = CreateFixture(spendRate: 100, spendSpeed: 20f, includeAnimationManager: true);
            yield return null;

            InvokePrivateMethod(fixture.BuyingArea, "OnTriggerEnter", fixture.PlayerCollider);
            yield return new WaitForSeconds(0.1f);

            Assert.That(fixture.BuyingArea == null, Is.True);
            Assert.That(fixture.Wallet.Money, Is.EqualTo(0));
            LogAssert.NoUnexpectedReceived();

            yield return DestroyFixture(fixture);
        }

        private static Fixture CreateFixture(int spendRate, float spendSpeed, bool includeAnimationManager = false)
        {
            var playerObject = new GameObject("PaymentTestPlayer");
            playerObject.tag = "Player";
            var wallet = playerObject.AddComponent<PlayerWallet>();
            var playerCollider = playerObject.AddComponent<CapsuleCollider>();

            var economy = ScriptableObject.CreateInstance<SEconomy>();
            economy.playerMoneySpendRate = spendRate;
            economy.playerMoneySpendSpeed = spendSpeed;

            var managerObject = new GameObject("PaymentTestEconomyManager");
            var economyManager = managerObject.AddComponent<EconomyManager>();
            SetPrivateField(economyManager, "sEconomy", economy);

            GameObject moneyPrefab = null;
            GameObject animationManagerObject = null;
            if (includeAnimationManager)
            {
                moneyPrefab = new GameObject("PaymentTestMoneyPrefab");
                economy.moneyPrefab = moneyPrefab;

                animationManagerObject = new GameObject("PaymentTestAnimationManager");
                var animationManager = animationManagerObject.AddComponent<AnimationManager>();
                SetPrivateField(animationManager, "sEconomy", economy);
                SetPrivateField(animationManager, "_duration", 0.05f);
            }

            var buyingAreaObject = new GameObject("PaymentTestBuyingArea");
            buyingAreaObject.AddComponent<BoxCollider>().isTrigger = true;
            var buyingArea = buyingAreaObject.AddComponent<BuyingArea>();

            return new Fixture(
                playerObject,
                wallet,
                playerCollider,
                managerObject,
                economy,
                buyingAreaObject,
                buyingArea,
                moneyPrefab,
                animationManagerObject);
        }

        private static IEnumerator DestroyFixture(Fixture fixture)
        {
            if (fixture.ManagerObject != null)
            {
                UnityEngine.Object.Destroy(fixture.ManagerObject);
            }

            if (fixture.AnimationManagerObject != null)
            {
                UnityEngine.Object.Destroy(fixture.AnimationManagerObject);
            }

            if (fixture.PlayerObject != null)
            {
                UnityEngine.Object.Destroy(fixture.PlayerObject);
            }

            if (fixture.BuyingAreaObject != null)
            {
                UnityEngine.Object.Destroy(fixture.BuyingAreaObject);
            }

            if (fixture.Economy != null)
            {
                UnityEngine.Object.Destroy(fixture.Economy);
            }

            if (fixture.MoneyPrefab != null)
            {
                UnityEngine.Object.Destroy(fixture.MoneyPrefab);
            }

            yield return null;
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected {target.GetType().Name} to define '{methodName}'.");
            method.Invoke(target, arguments);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }

        private sealed class Fixture
        {
            public Fixture(
                GameObject playerObject,
                PlayerWallet wallet,
                Collider playerCollider,
                GameObject managerObject,
                SEconomy economy,
                GameObject buyingAreaObject,
                BuyingArea buyingArea,
                GameObject moneyPrefab,
                GameObject animationManagerObject)
            {
                PlayerObject = playerObject;
                Wallet = wallet;
                PlayerCollider = playerCollider;
                ManagerObject = managerObject;
                Economy = economy;
                BuyingAreaObject = buyingAreaObject;
                BuyingArea = buyingArea;
                MoneyPrefab = moneyPrefab;
                AnimationManagerObject = animationManagerObject;
            }

            public GameObject PlayerObject { get; }
            public PlayerWallet Wallet { get; }
            public Collider PlayerCollider { get; }
            public GameObject ManagerObject { get; }
            public SEconomy Economy { get; }
            public GameObject BuyingAreaObject { get; }
            public BuyingArea BuyingArea { get; }
            public GameObject MoneyPrefab { get; }
            public GameObject AnimationManagerObject { get; }
        }
    }
}
