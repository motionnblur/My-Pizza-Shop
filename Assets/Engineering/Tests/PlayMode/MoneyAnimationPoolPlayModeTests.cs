using System;
using System.Collections;
using System.Reflection;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class MoneyAnimationPoolPlayModeTests
    {
        private static readonly Vector3 PrefabScale = new(0.74f, 0.22f, 0.38f);
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = CreateFixture();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_fixture?.ManagerObject != null)
                UnityEngine.Object.Destroy(_fixture.ManagerObject);

            if (_fixture?.Prefab != null)
                UnityEngine.Object.Destroy(_fixture.Prefab);

            if (_fixture?.Economy != null)
                UnityEngine.Object.Destroy(_fixture.Economy);

            if (_fixture?.SAnimation != null)
                UnityEngine.Object.Destroy(_fixture.SAnimation);

            yield return null;
            _fixture = null;
        }

        [UnityTest]
        public IEnumerator InitialPoolSize_PrewarmsInactiveMoneyChildren()
        {
            ConfigurePool(initialPoolSize: 3, maxPoolSize: 3);

            _fixture.Manager.DoMoneyAnimation(Vector3.zero, Vector3.one);
            yield return WaitForAllAnimationsToFinish();

            Assert.That(_fixture.Manager.transform.childCount, Is.EqualTo(3));
            Assert.That(CountActiveChildren(), Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator CompletedAnimation_ReusesTheSamePooledChild()
        {
            ConfigurePool(initialPoolSize: 1, maxPoolSize: 1);

            _fixture.Manager.DoMoneyAnimation(Vector3.zero, Vector3.one);
            var firstChild = GetOnlyActiveChild();
            yield return WaitForAllAnimationsToFinish();

            _fixture.Manager.DoMoneyAnimation(Vector3.zero, Vector3.one);
            var secondChild = GetOnlyActiveChild();
            Assert.That(secondChild, Is.SameAs(firstChild));

            yield return WaitForAllAnimationsToFinish();
        }

        [UnityTest]
        public IEnumerator CompletedAnimation_ResetsPooledChildTransformState()
        {
            ConfigurePool(initialPoolSize: 1, maxPoolSize: 1);

            _fixture.Manager.DoMoneyAnimation(Vector3.zero, Vector3.one);
            var child = GetOnlyActiveChild();
            yield return WaitForAllAnimationsToFinish();

            Assert.That(child.activeSelf, Is.False);
            Assert.That(child.transform.localScale, Is.EqualTo(PrefabScale));
            Assert.That(child.transform.localRotation, Is.EqualTo(Quaternion.identity));
        }

        [UnityTest]
        public IEnumerator OverlappingAnimations_AllPlayAndPoolRetainsAtMostMaxSize()
        {
            ConfigurePool(initialPoolSize: 1, maxPoolSize: 1);

            _fixture.Manager.DoMoneyAnimation(Vector3.zero, Vector3.one);
            _fixture.Manager.DoMoneyAnimation(Vector3.one, Vector3.zero);
            _fixture.Manager.DoMoneyAnimation(Vector3.left, Vector3.right);
            yield return null;

            Assert.That(_fixture.Manager.transform.childCount, Is.EqualTo(3));
            Assert.That(CountActiveChildren(), Is.EqualTo(3));

            yield return WaitForAllAnimationsToFinish();
            yield return null;

            Assert.That(CountActiveChildren(), Is.EqualTo(0));
            Assert.That(_fixture.Manager.transform.childCount, Is.LessThanOrEqualTo(1));
        }

        [UnityTest]
        public IEnumerator KillingMoneyTween_ReturnsChildToPoolAndRemovesItFromActiveState()
        {
            ConfigurePool(initialPoolSize: 1, maxPoolSize: 1);

            _fixture.Manager.DoMoneyAnimation(Vector3.zero, Vector3.one);
            var child = GetOnlyActiveChild();
            KillDOTweenTargets(child.transform);
            yield return null;

            Assert.That(child.activeSelf, Is.False,
                "Production code needs an OnKill-safe return-to-pool path for interrupted money animations.");
            Assert.That(CountActiveChildren(), Is.EqualTo(0),
                "Production code needs an OnKill-safe return-to-pool path for interrupted money animations.");
        }

        private void ConfigurePool(int initialPoolSize, int maxPoolSize)
        {
            SetPrivateField(_fixture.Manager, "_initialPoolSize", initialPoolSize);
            SetPrivateField(_fixture.Manager, "_maxPoolSize", maxPoolSize);
        }

        private IEnumerator WaitForAllAnimationsToFinish()
        {
            var deadline = Time.realtimeSinceStartup + 2f;
            while (CountActiveChildren() > 0 && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.That(CountActiveChildren(), Is.EqualTo(0),
                "Money animation did not return its active child to the pool before the timeout.");
        }

        private int CountActiveChildren()
        {
            var activeCount = 0;
            for (var i = 0; i < _fixture.Manager.transform.childCount; i++)
            {
                if (_fixture.Manager.transform.GetChild(i).gameObject.activeSelf)
                    activeCount++;
            }

            return activeCount;
        }

        private GameObject GetOnlyActiveChild()
        {
            GameObject activeChild = null;
            for (var i = 0; i < _fixture.Manager.transform.childCount; i++)
            {
                var child = _fixture.Manager.transform.GetChild(i).gameObject;
                if (!child.activeSelf)
                    continue;

                Assert.That(activeChild, Is.Null, "Expected exactly one active money child.");
                activeChild = child;
            }

            Assert.That(activeChild, Is.Not.Null, "Expected an active money child after starting the animation.");
            return activeChild;
        }

        private static Fixture CreateFixture()
        {
            var prefab = new GameObject("MoneyAnimationTestPrefab");
            prefab.transform.localScale = PrefabScale;

            var economy = ScriptableObject.CreateInstance<SEconomy>();
            economy.moneyPrefab = prefab;

            var managerObject = new GameObject("MoneyAnimationTestManager");
            var manager = managerObject.AddComponent<AnimationManager>();
            var sAnimation = ScriptableObject.CreateInstance<SAnimation>();
            sAnimation.duration = 0.05f;

            SetPrivateField(manager, "sEconomy", economy);
            SetPrivateField(manager, "_sAnimation", sAnimation);

            return new Fixture(prefab, economy, sAnimation, managerObject, manager);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static void KillDOTweenTargets(Transform target)
        {
            var dotweenType = Type.GetType("DG.Tweening.DOTween, DOTween");
            Assert.That(dotweenType, Is.Not.Null, "Expected the DOTween runtime assembly to be loaded.");

            var killMethod = dotweenType.GetMethod(
                "Kill",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(object), typeof(bool) },
                modifiers: null);
            Assert.That(killMethod, Is.Not.Null, "Expected DOTween to expose its public target-kill API.");
            killMethod.Invoke(null, new object[] { target, false });
        }

        private sealed class Fixture
        {
            public Fixture(GameObject prefab, SEconomy economy, SAnimation sAnimation, GameObject managerObject, AnimationManager manager)
            {
                Prefab = prefab;
                Economy = economy;
                SAnimation = sAnimation;
                ManagerObject = managerObject;
                Manager = manager;
            }

            public GameObject Prefab { get; }
            public SEconomy Economy { get; }
            public SAnimation SAnimation { get; }
            public GameObject ManagerObject { get; }
            public AnimationManager Manager { get; }
        }
    }
}
