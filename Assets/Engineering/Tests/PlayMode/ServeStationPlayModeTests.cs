using System.Collections;
using System.Reflection;
using Engineering.Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class ServeStationPlayModeTests
    {
        private GameObject _serveStationObject;
        private GameObject _playerObject;
        private GameObject _economyManagerObject;
        private SServeStation _serveSettings;
        private SVoidEventChannel _pizzaServedEvent;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_serveStationObject != null)
                Object.Destroy(_serveStationObject);

            if (_playerObject != null)
                Object.Destroy(_playerObject);

            if (_economyManagerObject != null)
                Object.Destroy(_economyManagerObject);

            if (_serveSettings != null)
                Object.Destroy(_serveSettings);

            if (_pizzaServedEvent != null)
                Object.Destroy(_pizzaServedEvent);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ServeAll_CollectsPizzasFromPlayerAndStacksThemOnTheStation()
        {
            CreateFixture(maxPizzas: 5, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var result = _serveStationObject.GetComponent<ServeStation>().TryServeAll(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            Assert.That(result, Is.EqualTo(3));
            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
            Assert.That(_serveStationObject.GetComponent<ServeStation>().ServedPizzaCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ServeAll_DoesNotExceedStationCapacity()
        {
            CreateFixture(maxPizzas: 2, pricePerPizza: 10, playerPizzaCount: 5);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var result = serveStation.TryServeAll(_playerObject.GetComponent<PlayerPizzaInventory>());

            Assert.That(result, Is.EqualTo(2));
            Assert.That(serveStation.ServedPizzaCount, Is.EqualTo(2));
            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ServeAll_ReturnsZeroWhenPlayerHasNoPizzas()
        {
            CreateFixture(maxPizzas: 5, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var result = _serveStationObject.GetComponent<ServeStation>().TryServeAll(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            Assert.That(result, Is.EqualTo(0));
            Assert.That(_serveStationObject.GetComponent<ServeStation>().ServedPizzaCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator ServeAll_ReturnsZeroWhenStationIsFull()
        {
            CreateFixture(maxPizzas: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var playerInventory = _playerObject.GetComponent<PlayerPizzaInventory>();

            serveStation.TryServeAll(playerInventory);
            var result = serveStation.TryServeAll(playerInventory);

            Assert.That(result, Is.EqualTo(0));
            Assert.That(serveStation.ServedPizzaCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator ServeAll_AwardsMoneyForEachServedPizza()
        {
            CreateFixture(maxPizzas: 5, pricePerPizza: 10, playerPizzaCount: 3, startMoney: 50);
            yield return null;

            _serveStationObject.GetComponent<ServeStation>().TryServeAll(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(80));
        }

        [UnityTest]
        public IEnumerator ServeAll_RaisesPizzaServedEventOnce()
        {
            CreateFixture(maxPizzas: 5, pricePerPizza: 10, playerPizzaCount: 3);
            var invocationCount = 0;
            _pizzaServedEvent.RegisterListener(() => invocationCount++);
            yield return null;

            _serveStationObject.GetComponent<ServeStation>().TryServeAll(
                _playerObject.GetComponent<PlayerPizzaInventory>());

            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ServePlateTriggers_ServePizzasToTheStation()
        {
            CreateFixture(maxPizzas: 5, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var playerCollider = _playerObject.AddComponent<CapsuleCollider>();

            InvokePrivateMethod(_serveStationObject.GetComponentInChildren<ServePlate>(),
                "OnTriggerEnter", playerCollider);

            yield return null;

            Assert.That(_serveStationObject.GetComponent<ServeStation>().ServedPizzaCount, Is.EqualTo(3));
            Assert.That(_playerObject.GetComponent<PlayerPizzaInventory>().Count, Is.EqualTo(0));
        }

        private void CreateFixture(
            int maxPizzas,
            int pricePerPizza,
            int playerPizzaCount,
            int startMoney = 100)
        {
            _serveSettings = ScriptableObject.CreateInstance<SServeStation>();
            _serveSettings.maxPizzas = maxPizzas;
            _serveSettings.pricePerPizza = pricePerPizza;

            _pizzaServedEvent = ScriptableObject.CreateInstance<SVoidEventChannel>();

            _serveStationObject = new GameObject("ServeStationTest");
            _serveStationObject.SetActive(false);
            var serveStation = _serveStationObject.AddComponent<ServeStation>();
            SetPrivateField(serveStation, "sServeStation", _serveSettings);
            SetPrivateField(serveStation, "pizzaServedEvent", _pizzaServedEvent);

            var plateObject = new GameObject("ServePlateTest");
            plateObject.transform.SetParent(_serveStationObject.transform);
            plateObject.transform.localPosition = Vector3.zero;
            var plateCollider = plateObject.AddComponent<BoxCollider>();
            plateCollider.center = new Vector3(0f, 0.25f, 0f);
            plateCollider.size = new Vector3(2f, 0.5f, 2f);
            var servePlate = plateObject.AddComponent<ServePlate>();
            SetPrivateField(servePlate, "serveStation", serveStation);
            SetPrivateField(servePlate, "plateCollider", plateCollider);
            SetPrivateField(serveStation, "servePlate", servePlate);

            var anchorObject = new GameObject("ServeStationAnchor");
            anchorObject.transform.SetParent(_serveStationObject.transform);
            SetPrivateField(serveStation, "pizzaStackAnchor", anchorObject.transform);

            _serveStationObject.SetActive(true);

            _playerObject = new GameObject("ServePlayerTest");
            _playerObject.SetActive(false);
            _playerObject.tag = "Player";
            var wallet = _playerObject.AddComponent<PlayerWallet>();
            SetPrivateField(wallet, "money", startMoney);
            var inventory = _playerObject.AddComponent<PlayerPizzaInventory>();
            SetPrivateField(inventory, "capacity", 10);
            _playerObject.SetActive(true);

            inventory.TryAdd(playerPizzaCount);

            _economyManagerObject = new GameObject("ServeEconomyManagerTest");
            var economyManager = _economyManagerObject.AddComponent<EconomyManager>();
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
