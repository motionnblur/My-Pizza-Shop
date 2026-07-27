using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Engineering.Scripts.Mono.Actors.TrashStation;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class TrashStationPlayModeTests
    {
        private GameObject _trashStationObject;
        private GameObject _playerObject;
        private GameObject _pizzaVisualPrefab;
        private STrashStation _trashSettings;
        private SVoidEventChannel _pizzaTrashedEvent;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_trashStationObject != null)
                Object.Destroy(_trashStationObject);

            if (_playerObject != null)
                Object.Destroy(_playerObject);

            if (_pizzaVisualPrefab != null)
                Object.Destroy(_pizzaVisualPrefab);

            if (_trashSettings != null)
                Object.Destroy(_trashSettings);

            if (_pizzaTrashedEvent != null)
                Object.Destroy(_pizzaTrashedEvent);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_RemovesAllPizzasFromPlayer()
        {
            CreateFixture(playerPizzaCount: 3);
            yield return null;

            _trashStationObject.GetComponent<TrashStation>().TrashAllPizzas(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_ReturnsEarlyWhenPlayerHasNoPizzas()
        {
            CreateFixture(playerPizzaCount: 0);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var isAnimatingField = typeof(TrashStation).GetField("_isPizzaAnimating",
                BindingFlags.Instance | BindingFlags.NonPublic);

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
            Assert.That((bool)isAnimatingField.GetValue(trashStation), Is.False);
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_NullPlayerIsNoOp()
        {
            CreateFixture(playerPizzaCount: 3);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var isAnimatingField = typeof(TrashStation).GetField("_isPizzaAnimating",
                BindingFlags.Instance | BindingFlags.NonPublic);

            trashStation.TrashAllPizzas(null);

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(3));
            Assert.That((bool)isAnimatingField.GetValue(trashStation), Is.False);
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_RaisesPizzaTrashedEventOnce()
        {
            CreateFixture(playerPizzaCount: 3);
            var invocationCount = 0;
            _pizzaTrashedEvent.RegisterListener(() => invocationCount++);
            yield return null;

            _trashStationObject.GetComponent<TrashStation>().TrashAllPizzas(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            yield return new WaitForSeconds(0.2f);

            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_DoesNotReTriggerWhileAlreadyAnimating()
        {
            CreateFixture(playerPizzaCount: 5);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var isAnimatingField = typeof(TrashStation).GetField("_isPizzaAnimating",
                BindingFlags.Instance | BindingFlags.NonPublic);
            isAnimatingField.SetValue(trashStation, true);

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_HandlesMissingPizzaVisualPrefabGracefully()
        {
            CreateFixture(playerPizzaCount: 3, includePizzaVisualPrefab: false);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var invocationCount = 0;
            _pizzaTrashedEvent.RegisterListener(() => invocationCount++);

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_HandlesMissingConfigGracefully()
        {
            CreateFixture(playerPizzaCount: 3);
            yield return null;

            SetPrivateField(_trashStationObject.GetComponent<TrashStation>(),
                "sTrashStation", null);

            var invocationCount = 0;
            _pizzaTrashedEvent.RegisterListener(() => invocationCount++);

            _trashStationObject.GetComponent<TrashStation>().TrashAllPizzas(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TrashAllPizzas_AnimatesAndReleasesVisuals()
        {
            CreateFixture(playerPizzaCount: 2);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return new WaitForSeconds(0.2f);

            var activeVisuals = GetActiveVisuals(trashStation);

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));

            Assert.That(activeVisuals.Count, Is.EqualTo(0),
                "All visuals should be released after animation completes");

            var pool = GetPool(trashStation);
            Assert.That(pool.CountInactive, Is.EqualTo(2),
                "Released visuals should be inactive in the pool after completion");
        }

        [UnityTest]
        public IEnumerator FirstTrashCreatesRequiredVisuals()
        {
            CreateFixture(playerPizzaCount: 3);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            var activeVisuals = GetActiveVisuals(trashStation);
            Assert.That(activeVisuals.Count, Is.EqualTo(3),
                "Should have one active visual per pizza after first trash");
        }

        [UnityTest]
        public IEnumerator SecondTrashReusesPooledVisuals()
        {
            CreateFixture(playerPizzaCount: 3);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var inventory = _playerObject.GetComponent<PlayerPizzaInventory>();

            trashStation.TrashAllPizzas(inventory);

            yield return new WaitForSeconds(0.2f);

            inventory.TryAdd(3);

            var pool = GetPool(trashStation);
            var countAllBefore = pool.CountAll;

            trashStation.TrashAllPizzas(inventory);

            yield return null;

            Assert.That(pool.CountAll, Is.EqualTo(countAllBefore),
                "No additional pizza visuals should be instantiated after pool warm-up");
            Assert.That(pool.CountActive, Is.EqualTo(3),
                "All pooled visuals should be active after second trash");
        }

        [UnityTest]
        public IEnumerator ReleasedVisualsAreInactiveAndResetBeforeReuse()
        {
            CreateFixture(playerPizzaCount: 2);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var inventory = _playerObject.GetComponent<PlayerPizzaInventory>();

            trashStation.TrashAllPizzas(inventory);

            yield return new WaitForSeconds(0.2f);

            var pool = GetPool(trashStation);
            Assert.That(pool.CountInactive, Is.EqualTo(2), "All visuals should be inactive in pool after release");

            inventory.TryAdd(2);
            trashStation.TrashAllPizzas(inventory);

            yield return null;

            var activeVisuals = GetActiveVisuals(trashStation);
            Assert.That(activeVisuals.Count, Is.EqualTo(2), "Reused visuals should be active");
            Assert.That(pool.CountAll, Is.EqualTo(2), "No new visuals should have been created");
        }

        [UnityTest]
        public IEnumerator AllVisualsReleasedAfterCompletion()
        {
            CreateFixture(playerPizzaCount: 4);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var isAnimatingField = typeof(TrashStation).GetField("_isPizzaAnimating",
                BindingFlags.Instance | BindingFlags.NonPublic);

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return new WaitForSeconds(0.2f);

            var activeVisuals = GetActiveVisuals(trashStation);
            Assert.That(activeVisuals.Count, Is.EqualTo(0), "All visuals released after animation completes");
            Assert.That((bool)isAnimatingField.GetValue(trashStation), Is.False,
                "_isPizzaAnimating should be false after all animations finish");
        }

        [UnityTest]
        public IEnumerator DestroyingStationDuringAnimationDoesNotError()
        {
            CreateFixture(playerPizzaCount: 3);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            Object.Destroy(_trashStationObject);
            _trashStationObject = null;

            yield return new WaitForSeconds(0.5f);
        }

        [UnityTest]
        public IEnumerator TrashPlateTriggerOnEnter_TrashesAllPizzas()
        {
            CreateFixture(playerPizzaCount: 4);
            yield return null;

            var playerCollider = _playerObject.AddComponent<CapsuleCollider>();

            InvokePrivateMethod(_trashStationObject.GetComponentInChildren<TrashPlate>(),
                "OnTriggerEnter", playerCollider);

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
        }

        private void CreateFixture(
            int playerPizzaCount,
            bool includePizzaVisualPrefab = true)
        {
            _trashSettings = ScriptableObject.CreateInstance<STrashStation>();
            _trashSettings.animationDuration = 0.01f;
            _trashSettings.staggerDelay = 0.01f;

            _pizzaTrashedEvent = ScriptableObject.CreateInstance<SVoidEventChannel>();

            _pizzaVisualPrefab = includePizzaVisualPrefab
                ? new GameObject("DummyPizzaVisual")
                : null;

            _trashStationObject = new GameObject("TrashStationTest");
            _trashStationObject.SetActive(false);
            var trashStation = _trashStationObject.AddComponent<TrashStation>();

            var trashTarget = new GameObject("TrashTargetTest");
            trashTarget.transform.SetParent(_trashStationObject.transform);

            SetPrivateField(trashStation, "sTrashStation", _trashSettings);
            SetPrivateField(trashStation, "pizzaVisualPrefab", _pizzaVisualPrefab);
            SetPrivateField(trashStation, "trashTarget", trashTarget.transform);
            SetPrivateField(trashStation, "pizzaTrashedEvent", _pizzaTrashedEvent);

            var plateObject = new GameObject("TrashPlateTest");
            plateObject.transform.SetParent(_trashStationObject.transform);
            var plateCollider = plateObject.AddComponent<BoxCollider>();
            plateCollider.isTrigger = true;
            var trashPlate = plateObject.AddComponent<TrashPlate>();
            SetPrivateField(trashPlate, "trashStation", trashStation);

            _trashStationObject.SetActive(true);

            _playerObject = new GameObject("TrashPlayerTest");
            _playerObject.SetActive(false);
            _playerObject.tag = "Player";

            var anchorObject = new GameObject("PizzaStackAnchor");
            anchorObject.transform.SetParent(_playerObject.transform);

            var inventory = _playerObject.AddComponent<PlayerPizzaInventory>();
            SetPrivateField(inventory, "capacity", 10);
            SetPrivateField(inventory, "pizzaStackAnchor", anchorObject.transform);

            _playerObject.SetActive(true);

            inventory.TryAdd(playerPizzaCount);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected {target.GetType().Name} to define '{methodName}'.");
            method.Invoke(target, arguments);
        }

        private static List<GameObject> GetActiveVisuals(TrashStation station)
        {
            var field = typeof(TrashStation).GetField("_activePizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Expected TrashStation to define '_activePizzaVisuals'.");
            return (List<GameObject>)field.GetValue(station);
        }

        private static ObjectPool<GameObject> GetPool(TrashStation station)
        {
            var field = typeof(TrashStation).GetField("_pizzaPool",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Expected TrashStation to define '_pizzaPool'.");
            return (ObjectPool<GameObject>)field.GetValue(station);
        }
    }
}
