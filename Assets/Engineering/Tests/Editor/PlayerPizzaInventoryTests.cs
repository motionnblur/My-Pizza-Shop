using System.Reflection;
using Engineering.Scripts.Mono.Actors.GrillStation;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;

namespace Engineering.Tests
{
    public class PlayerPizzaInventoryTests
    {
        private GameObject _playerObject;
        private PlayerPizzaInventory _inventory;
        private SIntEventChannel _pizzaInventoryChangedEvent;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("PizzaInventoryTestPlayer");
            _inventory = _playerObject.AddComponent<PlayerPizzaInventory>();
            _pizzaInventoryChangedEvent = ScriptableObject.CreateInstance<SIntEventChannel>();
            SetPrivateField(_inventory, "pizzaInventoryChangedEvent", _pizzaInventoryChangedEvent);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_pizzaInventoryChangedEvent);
            Object.DestroyImmediate(_playerObject);
        }

        [Test]
        public void TryAdd_OnlyAcceptsTheRemainingCapacityAndPublishesTheNewCount()
        {
            var lastPublishedCount = -1;
            _pizzaInventoryChangedEvent.RegisterListener(value => lastPublishedCount = value);

            var firstAcceptedAmount = _inventory.TryAdd(7);
            var secondAcceptedAmount = _inventory.TryAdd(5);

            Assert.That(firstAcceptedAmount, Is.EqualTo(7));
            Assert.That(secondAcceptedAmount, Is.EqualTo(3));
            Assert.That(_inventory.Count, Is.EqualTo(10));
            Assert.That(lastPublishedCount, Is.EqualTo(10));
        }

        [Test]
        public void TryRemove_OnlyRemovesWhatPlayerHasAndPublishesTheNewCount()
        {
            var lastPublishedCount = -1;
            _pizzaInventoryChangedEvent.RegisterListener(value => lastPublishedCount = value);

            _inventory.TryAdd(4);
            var removedAmount = _inventory.TryRemove(6);

            Assert.That(removedAmount, Is.EqualTo(4));
            Assert.That(_inventory.Count, Is.EqualTo(0));
            Assert.That(lastPublishedCount, Is.EqualTo(0));
        }

        [Test]
        public void TryAdd_ReturnsZero_WhenWasteInventoryHasItems()
        {
            var wasteInv = _playerObject.AddComponent<PlayerWasteInventory>();
            SetPrivateField(wasteInv, "capacity", 10);
            wasteInv.TryAdd(5);

            var accepted = _inventory.TryAdd(3);

            Assert.That(accepted, Is.EqualTo(0));
            Assert.That(_inventory.Count, Is.EqualTo(0));
            Assert.That(wasteInv.Count, Is.EqualTo(5));
        }

        [Test]
        public void TryAdd_AcceptsPizzas_WhenWasteInventoryEmpty()
        {
            _playerObject.AddComponent<PlayerWasteInventory>();

            var accepted = _inventory.TryAdd(3);

            Assert.That(accepted, Is.EqualTo(3));
            Assert.That(_inventory.Count, Is.EqualTo(3));
        }

        [Test]
        public void WasteInventory_TryAdd_ReturnsZero_WhenPizzaInventoryHasItems()
        {
            var wasteInv = _playerObject.AddComponent<PlayerWasteInventory>();
            SetPrivateField(wasteInv, "capacity", 10);

            _inventory.TryAdd(5);

            var accepted = wasteInv.TryAdd(3);

            Assert.That(accepted, Is.EqualTo(0));
            Assert.That(_inventory.Count, Is.EqualTo(5));
        }

        [Test]
        public void WasteInventory_TryAdd_AcceptsWaste_WhenPizzaInventoryEmpty()
        {
            var wasteInv = _playerObject.AddComponent<PlayerWasteInventory>();
            SetPrivateField(wasteInv, "capacity", 10);

            var accepted = wasteInv.TryAdd(4);

            Assert.That(accepted, Is.EqualTo(4));
            Assert.That(wasteInv.Count, Is.EqualTo(4));
        }

        [Test]
        public void PizzaStackBasePosition_UsesThePlateColliderTopSurface()
        {
            var plateObject = new GameObject("PizzaPlateTest");
            plateObject.transform.position = new Vector3(3f, 2f, 4f);
            var plateColliderComponent = plateObject.AddComponent<BoxCollider>();
            plateColliderComponent.center = new Vector3(0f, 0.5f, 0f);
            plateColliderComponent.size = new Vector3(2f, 1f, 2f);
            var grillPlate = plateObject.AddComponent<GrillPlate>();
            SetPrivateField(grillPlate, "plateCollider", plateColliderComponent);

            Assert.That(grillPlate.PizzaStackBasePosition, Is.EqualTo(new Vector3(3f, 3f, 4f)));

            Object.DestroyImmediate(plateObject);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
