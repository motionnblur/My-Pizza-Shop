using System;
using System.Collections;
using System.Reflection;
using Engineering.Engineering.Scripts.Mono.Items;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Engineering.Tests
{
    public class EconomyPaymentPlayModeTests
    {
        private Fixture _fixture;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_fixture != null)
                yield return DestroyFixture(_fixture);

            _fixture = null;
        }

        [UnityTest]
        public IEnumerator EnteringBuyingArea_TransfersTheConfiguredPaymentRate()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            yield return null;

            InvokePrivateMethod(_fixture.BuyingArea, "OnTriggerEnter", _fixture.PlayerCollider);
            yield return null;

            Assert.That(_fixture.Wallet.Money, Is.EqualTo(95));
            Assert.That(_fixture.BuyingArea, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator LeavingBuyingArea_CancelsFurtherScheduledPayments()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 20f);
            yield return null;

            InvokePrivateMethod(_fixture.BuyingArea, "OnTriggerEnter", _fixture.PlayerCollider);
            yield return null;

            InvokePrivateMethod(_fixture.BuyingArea, "OnTriggerExit", _fixture.PlayerCollider);
            yield return new WaitForSeconds(0.1f);

            Assert.That(_fixture.Wallet.Money, Is.EqualTo(95));
        }

        [UnityTest]
        public IEnumerator CompletingPurchase_DestroysTheBuyingArea()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            yield return null;

            _fixture.BuyingArea.AddPayment(100);
            yield return null;

            Assert.That(_fixture.BuyingArea == null, Is.True);
        }

        [UnityTest]
        public IEnumerator CompletingPurchase_RaisesBuyingAreaPurchasedEventOnce()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            var invocationCount = 0;
            _fixture.BuyingAreaPurchasedEvent.RegisterListener(() => invocationCount++);
            yield return null;

            _fixture.BuyingArea.AddPayment(100);
            yield return null;

            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CompletingPurchaseDuringPayment_UsesCachedAnimationDestination()
        {
            _fixture = CreateFixture(spendRate: 100, spendSpeed: 20f, includeAnimationManager: true);
            yield return null;

            InvokePrivateMethod(_fixture.BuyingArea, "OnTriggerEnter", _fixture.PlayerCollider);
            yield return new WaitForSeconds(0.1f);

            Assert.That(_fixture.BuyingArea == null, Is.True);
            Assert.That(_fixture.Wallet.Money, Is.EqualTo(0));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator CollectingGroundMoney_WithoutAnimationManager_AddsTheConfiguredAmountUpdatesTheUiAndDestroysThePickup()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            var pickupObject = new GameObject("MoneyToCollectTestPickup");
            var pickup = pickupObject.AddComponent<MoneyToCollect>();
            pickupObject.AddComponent<BoxCollider>().isTrigger = true;
            SetPrivateField(pickup, "moneyToCollect", 25);
            yield return null;

            InvokePrivateMethod(pickup, "OnTriggerEnter", _fixture.PlayerCollider);
            yield return null;

            Assert.That(_fixture.Wallet.Money, Is.EqualTo(125));
            Assert.That(_fixture.MoneyText.text, Is.EqualTo("125"));
            Assert.That(pickup == null, Is.True);
        }

        [UnityTest]
        public IEnumerator CollectingGroundMoney_RaisesGroundMoneyCollectedEventOnce()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            var invocationCount = 0;
            _fixture.GroundMoneyCollectedEvent.RegisterListener(() => invocationCount++);
            var pickupObject = new GameObject("MoneyToCollectEventPickup");
            var pickup = pickupObject.AddComponent<MoneyToCollect>();
            pickupObject.AddComponent<BoxCollider>().isTrigger = true;
            yield return null;

            InvokePrivateMethod(pickup, "OnTriggerEnter", _fixture.PlayerCollider);
            yield return null;

            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CollectingGroundMoney_WithAnimationManager_UpdatesTheBalanceAndStartsMoneyAnimation()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f, includeAnimationManager: true, animationDuration: 1f);
            var pickupObject = new GameObject("MoneyToCollectAnimatedPickup");
            pickupObject.transform.position = new Vector3(3f, 0f, 2f);
            var pickup = pickupObject.AddComponent<MoneyToCollect>();
            pickupObject.AddComponent<BoxCollider>().isTrigger = true;
            SetPrivateField(pickup, "moneyToCollect", 25);
            yield return null;

            InvokePrivateMethod(pickup, "OnTriggerEnter", _fixture.PlayerCollider);

            Assert.That(_fixture.Wallet.Money, Is.EqualTo(125));
            Assert.That(_fixture.MoneyText.text, Is.EqualTo("125"));
            Assert.That(CountActiveChildren(_fixture.AnimationManagerObject), Is.EqualTo(1));

            yield return null;
            Assert.That(pickup == null, Is.True);
        }

        [UnityTest]
        public IEnumerator CollectingGroundMoney_FollowsTheMovingPlayerAnimationTarget()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f, includeAnimationManager: true, animationDuration: 0.1f);
            var pickupObject = new GameObject("MoneyToCollectMovingPlayerPickup");
            pickupObject.transform.position = new Vector3(5f, 0f, 0f);
            var pickup = pickupObject.AddComponent<MoneyToCollect>();
            pickupObject.AddComponent<BoxCollider>().isTrigger = true;
            yield return null;

            InvokePrivateMethod(pickup, "OnTriggerEnter", _fixture.PlayerCollider);
            _fixture.PlayerObject.transform.position = new Vector3(10f, 0f, 0f);
            yield return new WaitForSeconds(0.2f);

            Assert.That(
                HasChildAtPosition(_fixture.AnimationManagerObject, _fixture.Wallet.MoneyAnimationOriginPosition),
                Is.True,
                "Collected money should finish at the player's latest animation origin.");
        }

        [UnityTest]
        public IEnumerator CollectingGroundMoney_IgnoresNonPlayerColliders()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            var pickupObject = new GameObject("MoneyToCollectNonPlayerPickup");
            var pickup = pickupObject.AddComponent<MoneyToCollect>();
            pickupObject.AddComponent<BoxCollider>().isTrigger = true;
            SetPrivateField(pickup, "moneyToCollect", 25);
            var nonPlayerObject = new GameObject("NonPlayerCollider");
            var nonPlayerCollider = nonPlayerObject.AddComponent<BoxCollider>();
            yield return null;

            InvokePrivateMethod(pickup, "OnTriggerEnter", nonPlayerCollider);

            Assert.That(_fixture.Wallet.Money, Is.EqualTo(100));
            Assert.That(_fixture.MoneyText.text, Is.EqualTo("100"));
            Assert.That(pickup, Is.Not.Null);

            UnityEngine.Object.Destroy(pickupObject);
            UnityEngine.Object.Destroy(nonPlayerObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CollectingGroundMoney_OnlyAwardsMoneyOnceForRepeatedTriggers()
        {
            _fixture = CreateFixture(spendRate: 5, spendSpeed: 1f);
            var pickupObject = new GameObject("MoneyToCollectRepeatedTriggerPickup");
            var pickup = pickupObject.AddComponent<MoneyToCollect>();
            pickupObject.AddComponent<BoxCollider>().isTrigger = true;
            SetPrivateField(pickup, "moneyToCollect", 25);
            yield return null;

            InvokePrivateMethod(pickup, "OnTriggerEnter", _fixture.PlayerCollider);
            InvokePrivateMethod(pickup, "OnTriggerEnter", _fixture.PlayerCollider);

            Assert.That(_fixture.Wallet.Money, Is.EqualTo(125));
            Assert.That(_fixture.MoneyText.text, Is.EqualTo("125"));

            yield return null;
            Assert.That(pickup == null, Is.True);
        }

        private static Fixture CreateFixture(
            int spendRate,
            float spendSpeed,
            bool includeAnimationManager = false,
            float animationDuration = 0.05f)
        {
            var playerObject = new GameObject("PaymentTestPlayer");
            playerObject.tag = "Player";
            var wallet = playerObject.AddComponent<PlayerWallet>();
            var playerCollider = playerObject.AddComponent<CapsuleCollider>();

            var moneyTextObject = new GameObject("PaymentTestMoneyText");
            var moneyText = moneyTextObject.AddComponent<Text>();
            var uiManagerObject = new GameObject("PaymentTestUiManager");
            var uiManager = uiManagerObject.AddComponent<UIManager>();
            SetPrivateField(uiManager, "moneyText", moneyText);

            var economy = ScriptableObject.CreateInstance<SEconomy>();
            economy.playerMoneySpendRate = spendRate;

            var sAnimation = ScriptableObject.CreateInstance<SAnimation>();
            sAnimation.moneySpendDelay = 0f;
            sAnimation.moneySpendSpeed = spendSpeed;

            var groundMoneyCollectedEvent = ScriptableObject.CreateInstance<SVoidEventChannel>();
            var buyingAreaPurchasedEvent = ScriptableObject.CreateInstance<SVoidEventChannel>();

            var managerObject = new GameObject("PaymentTestEconomyManager");
            var economyManager = managerObject.AddComponent<EconomyManager>();
            SetPrivateField(economyManager, "sEconomy", economy);
            SetPrivateField(economyManager, "_sAnimation", sAnimation);
            SetPrivateField(economyManager, "groundMoneyCollectedEvent", groundMoneyCollectedEvent);
            SetPrivateField(economyManager, "buyingAreaPurchasedEvent", buyingAreaPurchasedEvent);

            GameObject moneyPrefab = null;
            GameObject animationManagerObject = null;
            if (includeAnimationManager)
            {
                moneyPrefab = new GameObject("PaymentTestMoneyPrefab");
                economy.moneyPrefab = moneyPrefab;

                animationManagerObject = new GameObject("PaymentTestAnimationManager");
                var animationManager = animationManagerObject.AddComponent<AnimationManager>();
                sAnimation.duration = animationDuration;
                SetPrivateField(animationManager, "sEconomy", economy);
                SetPrivateField(animationManager, "_sAnimation", sAnimation);
            }

            var buyingAreaObject = new GameObject("PaymentTestBuyingArea");
            buyingAreaObject.AddComponent<BoxCollider>().isTrigger = true;
            var buyingArea = buyingAreaObject.AddComponent<BuyingArea>();

            return new Fixture(
                playerObject,
                wallet,
                playerCollider,
                moneyTextObject,
                moneyText,
                uiManagerObject,
                managerObject,
                economy,
                sAnimation,
                buyingAreaObject,
                buyingArea,
                moneyPrefab,
                animationManagerObject,
                groundMoneyCollectedEvent,
                buyingAreaPurchasedEvent);
        }

        private static IEnumerator DestroyFixture(Fixture fixture)
        {
            if (fixture.UiManagerObject != null)
            {
                UnityEngine.Object.Destroy(fixture.UiManagerObject);
            }

            if (fixture.MoneyTextObject != null)
            {
                UnityEngine.Object.Destroy(fixture.MoneyTextObject);
            }

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

            if (fixture.SAnimation != null)
            {
                UnityEngine.Object.Destroy(fixture.SAnimation);
            }

            if (fixture.MoneyPrefab != null)
            {
                UnityEngine.Object.Destroy(fixture.MoneyPrefab);
            }

            if (fixture.GroundMoneyCollectedEvent != null)
            {
                UnityEngine.Object.Destroy(fixture.GroundMoneyCollectedEvent);
            }

            if (fixture.BuyingAreaPurchasedEvent != null)
            {
                UnityEngine.Object.Destroy(fixture.BuyingAreaPurchasedEvent);
            }

            yield return null;
        }

        private static int CountActiveChildren(GameObject gameObject)
        {
            var activeChildCount = 0;
            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.activeSelf)
                    activeChildCount++;
            }

            return activeChildCount;
        }

        private static bool HasChildAtPosition(GameObject gameObject, Vector3 position)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (Vector3.Distance(child.position, position) < 0.01f)
                    return true;
            }

            return false;
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
                GameObject moneyTextObject,
                Text moneyText,
                GameObject uiManagerObject,
                GameObject managerObject,
                SEconomy economy,
                SAnimation sAnimation,
                GameObject buyingAreaObject,
                BuyingArea buyingArea,
                GameObject moneyPrefab,
                GameObject animationManagerObject,
                SVoidEventChannel groundMoneyCollectedEvent,
                SVoidEventChannel buyingAreaPurchasedEvent)
            {
                PlayerObject = playerObject;
                Wallet = wallet;
                PlayerCollider = playerCollider;
                MoneyTextObject = moneyTextObject;
                MoneyText = moneyText;
                UiManagerObject = uiManagerObject;
                ManagerObject = managerObject;
                Economy = economy;
                SAnimation = sAnimation;
                BuyingAreaObject = buyingAreaObject;
                BuyingArea = buyingArea;
                MoneyPrefab = moneyPrefab;
                AnimationManagerObject = animationManagerObject;
                GroundMoneyCollectedEvent = groundMoneyCollectedEvent;
                BuyingAreaPurchasedEvent = buyingAreaPurchasedEvent;
            }

            public GameObject PlayerObject { get; }
            public PlayerWallet Wallet { get; }
            public Collider PlayerCollider { get; }
            public GameObject MoneyTextObject { get; }
            public Text MoneyText { get; }
            public GameObject UiManagerObject { get; }
            public GameObject ManagerObject { get; }
            public SEconomy Economy { get; }
            public SAnimation SAnimation { get; }
            public GameObject BuyingAreaObject { get; }
            public BuyingArea BuyingArea { get; }
            public GameObject MoneyPrefab { get; }
            public GameObject AnimationManagerObject { get; }
            public SVoidEventChannel GroundMoneyCollectedEvent { get; }
            public SVoidEventChannel BuyingAreaPurchasedEvent { get; }
        }
    }
}
