using System.Collections.Generic;
using System.Reflection;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Actors.GrillStation;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.Scripts.Mono.Actors.Table;
using Engineering.Scripts.Mono.Actors.TrashStation;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;

namespace Engineering.Tests
{
    public static class PlayerPrefabTestFixture
    {
        private const string PlayerResourcePath = "Player";

        private static GameObject _cachedPlayerPrefab;

        public static GameObject LoadPlayerPrefab()
        {
            if (_cachedPlayerPrefab == null)
            {
                _cachedPlayerPrefab = Resources.Load<GameObject>(PlayerResourcePath);
                Assert.That(_cachedPlayerPrefab, Is.Not.Null,
                    "Player.prefab must be in Resources for PlayMode tests. " +
                    "Ensure PlayerPrefabPrebuildSetup ran before PlayMode tests.");
            }

            return _cachedPlayerPrefab;
        }

        public static GameObject InstantiatePlayer()
        {
            var prefab = LoadPlayerPrefab();
            var instance = Object.Instantiate(prefab);
            Assert.That(instance, Is.Not.Null, "Failed to instantiate Player.prefab.");

            var scriptsChild = instance.transform.Find("Scripts");
            if (scriptsChild != null)
            {
                var movement = scriptsChild.GetComponent<PlayerMovement>();
                if (movement != null)
                    movement.enabled = false;
            }

            return instance;
        }

        public static PlayerPizzaInventory GetPizzaInventory(GameObject playerInstance)
        {
            var scriptsChild = playerInstance.transform.Find("Scripts");
            Assert.That(scriptsChild, Is.Not.Null,
                "Player instance must have a 'Scripts' child.");

            var pizzaInv = scriptsChild.GetComponent<PlayerPizzaInventory>();
            Assert.That(pizzaInv, Is.Not.Null,
                "'Scripts' child must have a PlayerPizzaInventory component.");

            return pizzaInv;
        }

        public static PlayerWasteInventory GetWasteInventory(GameObject playerInstance)
        {
            var scriptsChild = playerInstance.transform.Find("Scripts");
            Assert.That(scriptsChild, Is.Not.Null,
                "Player instance must have a 'Scripts' child.");

            var wasteInv = scriptsChild.GetComponent<PlayerWasteInventory>();
            Assert.That(wasteInv, Is.Not.Null,
                "'Scripts' child must have a PlayerWasteInventory component.");

            return wasteInv;
        }

        public static Collider GetPlayerCollider(GameObject playerInstance)
        {
            var meshChild = playerInstance.transform.Find("Mesh");
            Assert.That(meshChild, Is.Not.Null,
                "Player instance must have a 'Mesh' child.");

            var collider = meshChild.GetComponent<Collider>();
            Assert.That(collider, Is.Not.Null,
                "'Mesh' child must have a Collider.");

            return collider;
        }

        public static void SetPlayerKinematic(GameObject playerInstance)
        {
            var meshChild = playerInstance.transform.Find("Mesh");
            Assert.That(meshChild, Is.Not.Null);

            var rb = meshChild.GetComponent<Rigidbody>();
            Assert.That(rb, Is.Not.Null, "Mesh child must have a Rigidbody for physics tests.");

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        public static void AddPizzas(GameObject playerInstance, int count)
        {
            var pizzaInv = GetPizzaInventory(playerInstance);
            var accepted = pizzaInv.TryAdd(count);
            Assert.That(accepted, Is.EqualTo(count),
                "Expected to add {0} pizzas, but only {1} were accepted.", count, accepted);
        }

        public static void AddWaste(GameObject playerInstance, int count)
        {
            var wasteInv = GetWasteInventory(playerInstance);
            var accepted = wasteInv.TryAdd(count);
            Assert.That(accepted, Is.EqualTo(count),
                "Expected to add {0} waste items, but only {1} were accepted.", count, accepted);
        }

        public static GrillStationFixture CreateGrillStationFixture(int maxReadyPizzas = 5, float productionInterval = 0.02f)
        {
            return new GrillStationFixture(maxReadyPizzas, productionInterval);
        }

        public static ServePlateFixture CreateServePlateFixture(int maxStoredPizzas = 10, int pricePerPizza = 5)
        {
            return new ServePlateFixture(maxStoredPizzas, pricePerPizza);
        }

        public static TrashPlateFixture CreateTrashPlateFixture()
        {
            return new TrashPlateFixture();
        }

        public static TableWasteFixture CreateTableWasteFixture(int maxLeftovers = 10, int initialLeftovers = 5)
        {
            return new TableWasteFixture(maxLeftovers, initialLeftovers);
        }

        public class GrillStationFixture
        {
            public GameObject StationObject { get; }
            public GrillStation Station { get; }
            public GrillPlate Plate { get; }
            public SGrillStation Settings { get; }

            internal GrillStationFixture(int maxReadyPizzas, float productionInterval)
            {
                Settings = ScriptableObject.CreateInstance<SGrillStation>();
                Settings.maxReadyPizzas = maxReadyPizzas;
                Settings.productionInterval = productionInterval;

                StationObject = new GameObject("GrillStationFixture");
                StationObject.SetActive(false);
                Station = StationObject.AddComponent<GrillStation>();
                SetPrivateField(Station, "sGrillStation", Settings);

                var plateObject = new GameObject("GrillPlate");
                plateObject.transform.SetParent(StationObject.transform);
                var plateCollider = plateObject.AddComponent<BoxCollider>();
                plateCollider.isTrigger = true;
                Plate = plateObject.AddComponent<GrillPlate>();
                SetPrivateField(Plate, "grillStation", Station);

                StationObject.SetActive(true);
            }

            public void Destroy()
            {
                if (StationObject != null) Object.Destroy(StationObject);
                if (Settings != null) Object.Destroy(Settings);
            }
        }

        public class ServePlateFixture
        {
            public GameObject StationObject { get; }
            public ServeStation Station { get; }
            public PlateTrigger PlateTrigger { get; }
            public SServeStation Settings { get; }

            internal ServePlateFixture(int maxStoredPizzas, int pricePerPizza)
            {
                Settings = ScriptableObject.CreateInstance<SServeStation>();
                Settings.maxStoredPizzas = maxStoredPizzas;
                Settings.pricePerPizza = pricePerPizza;
                Settings.maxQueueCustomers = 10;
                Settings.maxPizzasPerOrder = 5;
                Settings.minPizzasPerOrder = 1;

                StationObject = new GameObject("ServeStationFixture");
                StationObject.SetActive(false);
                Station = StationObject.AddComponent<ServeStation>();
                SetPrivateField(Station, "sServeStation", Settings);

                var plateObject = new GameObject("PlateTrigger");
                plateObject.transform.SetParent(StationObject.transform);
                var triggerCollider = plateObject.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                PlateTrigger = plateObject.AddComponent<PlateTrigger>();
                SetPrivateField(PlateTrigger, "serveStation", Station);

                StationObject.SetActive(true);
            }

            public void Destroy()
            {
                if (StationObject != null) Object.Destroy(StationObject);
                if (Settings != null) Object.Destroy(Settings);
            }
        }

        public class TrashPlateFixture
        {
            public GameObject StationObject { get; }
            public TrashStation Station { get; }
            public TrashPlate Plate { get; }
            public STrashStation Settings { get; }

            internal TrashPlateFixture()
            {
                Settings = ScriptableObject.CreateInstance<STrashStation>();
                Settings.animationDuration = 0.01f;
                Settings.staggerDelay = 0.01f;

                StationObject = new GameObject("TrashStationFixture");
                StationObject.SetActive(false);
                Station = StationObject.AddComponent<TrashStation>();

                var trashTarget = new GameObject("TrashTarget");
                trashTarget.transform.SetParent(StationObject.transform);

                SetPrivateField(Station, "sTrashStation", Settings);
                SetPrivateField(Station, "trashTarget", trashTarget.transform);

                var plateObject = new GameObject("TrashPlate");
                plateObject.transform.SetParent(StationObject.transform);
                var triggerCollider = plateObject.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                Plate = plateObject.AddComponent<TrashPlate>();
                SetPrivateField(Plate, "trashStation", Station);

                StationObject.SetActive(true);
            }

            public void Destroy()
            {
                if (StationObject != null) Object.Destroy(StationObject);
                if (Settings != null) Object.Destroy(Settings);
            }
        }

        public class TableWasteFixture
        {
            public GameObject TableObject { get; }
            public Table Table { get; }
            public TableWasteTrigger Trigger { get; }
            public GameObject TriggerObject { get; }

            internal TableWasteFixture(int maxLeftovers, int initialLeftovers)
            {
                TableObject = new GameObject("TableFixture");
                TableObject.SetActive(false);
                Table = TableObject.AddComponent<Table>();
                SetPrivateField(Table, "maxLeftovers", maxLeftovers);

                var seats = new Transform[1];
                var seat = new GameObject("Seat0");
                seat.transform.SetParent(TableObject.transform);
                seats[0] = seat.transform;
                SetPrivateField(Table, "seatTransforms", seats);

                TableObject.SetActive(true);

                Table.AddLeftovers(initialLeftovers);

                TriggerObject = new GameObject("TableWasteTrigger");
                var triggerCollider = TriggerObject.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                Trigger = TriggerObject.AddComponent<TableWasteTrigger>();
                SetPrivateField(Trigger, "table", Table);
            }

            public void Destroy()
            {
                if (TableObject != null) Object.Destroy(TableObject);
                if (TriggerObject != null) Object.Destroy(TriggerObject);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                "Expected {0} to define field '{1}'.", target.GetType().Name, fieldName);
            field.SetValue(target, value);
        }
    }
}
