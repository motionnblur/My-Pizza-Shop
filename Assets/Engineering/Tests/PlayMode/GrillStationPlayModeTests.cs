using System.Collections;
using System.Reflection;
using Engineering.Engineering.Scripts.Mono.Actors.PizzaMaker;
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

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
