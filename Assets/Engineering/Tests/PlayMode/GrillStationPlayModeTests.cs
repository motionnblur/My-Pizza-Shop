using System.Collections;
using System.Reflection;
using Engineering.Scripts.Mono.Actors.GrillStation;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class GrillStationPlayModeTests
    {
        private GameObject _stationObject;
        private GameObject _playerObject;
        private SGrillStation _grillSettings;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_stationObject != null)
                Object.Destroy(_stationObject);

            if (_playerObject != null)
                Object.Destroy(_playerObject);

            if (_grillSettings != null)
                Object.Destroy(_grillSettings);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GrillStation_StopsAtCapacityAndCollectsOnlyThePlayerRemainingCapacity()
        {
            _grillSettings = ScriptableObject.CreateInstance<SGrillStation>();
            _grillSettings.productionInterval = 0.05f;
            _grillSettings.maxReadyPizzas = 2;

            _stationObject = new GameObject("GrillStationTest");
            _stationObject.SetActive(false);
            var grillStation = _stationObject.AddComponent<GrillStation>();
            SetPrivateField(grillStation, "sGrillStation", _grillSettings);
            _stationObject.SetActive(true);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillStation.ReadyPizzaCount, Is.EqualTo(2));

            _playerObject = new GameObject("PizzaInventoryTestPlayer");
            _playerObject.SetActive(false);
            var inventory = _playerObject.AddComponent<PlayerPizzaInventory>();
            SetPrivateField(inventory, "capacity", 1);
            _playerObject.SetActive(true);

            var collectedAmount = grillStation.TryCollectAll(inventory);

            Assert.That(collectedAmount, Is.EqualTo(1));
            Assert.That(inventory.Count, Is.EqualTo(1));
            Assert.That(grillStation.ReadyPizzaCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator GrillPlateTrigger_FindsInventoryOnPlayerScriptsSibling()
        {
            _grillSettings = ScriptableObject.CreateInstance<SGrillStation>();
            _grillSettings.productionInterval = 0.05f;
            _grillSettings.maxReadyPizzas = 1;

            _stationObject = new GameObject("GrillStationTest");
            _stationObject.SetActive(false);
            var grillStation = _stationObject.AddComponent<GrillStation>();
            SetPrivateField(grillStation, "sGrillStation", _grillSettings);
            _stationObject.SetActive(true);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillStation.ReadyPizzaCount, Is.EqualTo(1));

            _playerObject = new GameObject("Player");
            _playerObject.tag = "Player";
            var scriptsObject = new GameObject("Scripts");
            scriptsObject.transform.SetParent(_playerObject.transform);
            var inventory = scriptsObject.AddComponent<PlayerPizzaInventory>();

            var meshObject = new GameObject("Mesh");
            meshObject.transform.SetParent(_playerObject.transform);
            meshObject.tag = "Player";
            var playerCollider = meshObject.AddComponent<BoxCollider>();

            var plateObject = new GameObject("GrillPlate");
            var grillPlate = plateObject.AddComponent<GrillPlate>();
            SetPrivateField(grillPlate, "grillStation", grillStation);

            InvokePrivateMethod(grillPlate, "OnTriggerEnter", playerCollider);

            Assert.That(inventory.Count, Is.EqualTo(1));
            Assert.That(grillStation.ReadyPizzaCount, Is.EqualTo(0));

            Object.Destroy(plateObject);
        }

        [UnityTest]
        public IEnumerator GrillPlate_DoesNotCollectPizzas_WhenPlayerHasWaste()
        {
            _grillSettings = ScriptableObject.CreateInstance<SGrillStation>();
            _grillSettings.productionInterval = 0.05f;
            _grillSettings.maxReadyPizzas = 3;

            _stationObject = new GameObject("GrillStationTest");
            _stationObject.SetActive(false);
            var grillStation = _stationObject.AddComponent<GrillStation>();
            SetPrivateField(grillStation, "sGrillStation", _grillSettings);
            _stationObject.SetActive(true);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillStation.ReadyPizzaCount, Is.EqualTo(3));

            _playerObject = new GameObject("Player");
            _playerObject.tag = "Player";
            _playerObject.SetActive(false);
            var scriptsObject = new GameObject("Scripts");
            scriptsObject.transform.SetParent(_playerObject.transform);

            var pizzaInv = scriptsObject.AddComponent<PlayerPizzaInventory>();
            SetPrivateField(pizzaInv, "capacity", 10);

            var wasteInv = scriptsObject.AddComponent<PlayerWasteInventory>();
            SetPrivateField(wasteInv, "capacity", 10);
            wasteInv.TryAdd(4);

            var meshObject = new GameObject("Mesh");
            meshObject.transform.SetParent(_playerObject.transform);
            meshObject.tag = "Player";
            var playerCollider = meshObject.AddComponent<BoxCollider>();
            _playerObject.SetActive(true);

            var plateObject = new GameObject("GrillPlate");
            var grillPlate = plateObject.AddComponent<GrillPlate>();
            SetPrivateField(grillPlate, "grillStation", grillStation);

            InvokePrivateMethod(grillPlate, "OnTriggerEnter", playerCollider);

            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "Player with waste should not collect pizzas.");
            Assert.That(wasteInv.Count, Is.EqualTo(4),
                "Waste count should be unchanged.");
            Assert.That(grillStation.ReadyPizzaCount, Is.EqualTo(3),
                "GrillStation pizzas must be preserved when player has waste.");

            Object.Destroy(plateObject);
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
