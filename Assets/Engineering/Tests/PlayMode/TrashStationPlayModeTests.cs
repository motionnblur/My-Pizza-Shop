using System.Collections;
using System.Reflection;
using Engineering.Scripts.Mono.Actors.TrashStation;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
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
            var isAnimatingField = typeof(TrashStation).GetField("_isAnimating",
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
            var isAnimatingField = typeof(TrashStation).GetField("_isAnimating",
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
            var isAnimatingField = typeof(TrashStation).GetField("_isAnimating",
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
        public IEnumerator TrashAllPizzas_AnimatesAndDestroysTempVisuals()
        {
            CreateFixture(playerPizzaCount: 2);
            yield return null;

            var trashStation = _trashStationObject.GetComponent<TrashStation>();
            var initialChildCount = _trashStationObject.transform.childCount;

            trashStation.TrashAllPizzas(_playerObject.GetComponent<PlayerPizzaInventory>());

            yield return null;

            var tempVisualsField = typeof(TrashStation).GetField("_tempVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var tempVisuals = (System.Collections.Generic.List<GameObject>)tempVisualsField.GetValue(trashStation);

            if (tempVisuals != null && tempVisuals.Count > 0)
            {
                yield return new WaitForSeconds(0.2f);
            }

            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
            Assert.That(_trashStationObject.transform.childCount, Is.EqualTo(initialChildCount));
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
    }
}
