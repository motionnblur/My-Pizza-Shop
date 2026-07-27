using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.Inventory;
using Engineering.Scripts.Mono.Actors.Table;
using Engineering.Scripts.Mono.Actors.TrashStation;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class TableWastePlayModeTests
    {
        private readonly List<GameObject> _toCleanup = new();
        private STrashStation _trashSettings;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in _toCleanup)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _toCleanup.Clear();

            if (_trashSettings != null)
                Object.Destroy(_trashSettings);

            yield return null;
        }

        private (Table table, TableManager manager, TableWasteVisuals visuals) CreateTableWithWaste(
            int seatCount = 2, int maxLeftovers = 10)
        {
            var tableGO = new GameObject("TestTable");
            tableGO.SetActive(false);
            var table = tableGO.AddComponent<Table>();
            var seats = new Transform[seatCount];
            for (var i = 0; i < seatCount; i++)
            {
                var seat = new GameObject($"Seat{i}");
                seat.transform.SetParent(tableGO.transform);
                seats[i] = seat.transform;
            }
            SetPrivateField(table, "seatTransforms", seats);
            SetPrivateField(table, "maxLeftovers", maxLeftovers);

            var anchor = new GameObject("LeftoverStackAnchor");
            anchor.transform.SetParent(tableGO.transform);
            var visuals = tableGO.AddComponent<TableWasteVisuals>();
            SetPrivateField(visuals, "leftoverPrefab", CreateLeftoverPrefab());
            SetPrivateField(visuals, "leftoverStackAnchor", anchor.transform);
            SetPrivateField(visuals, "leftoverStackSpacing", 0.14f);
            SetPrivateField(table, "wasteVisuals", visuals);

            tableGO.SetActive(true);
            _toCleanup.Add(tableGO);

            var managerGO = new GameObject("TestTableManager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table });
            _toCleanup.Add(managerGO);

            managerGO.SetActive(true);

            return (table, manager, visuals);
        }

        private GameObject CreatePlayer(bool withWasteInventory = true, int wasteCapacity = 10)
        {
            var player = new GameObject("TestPlayer");
            player.tag = "Player";
            player.SetActive(false);

            var wasteAnchor = new GameObject("WasteStackAnchor");
            wasteAnchor.transform.SetParent(player.transform);

            if (withWasteInventory)
            {
                var wasteInv = player.AddComponent<PlayerWasteInventory>();
                SetPrivateField(wasteInv, "capacity", wasteCapacity);
                SetPrivateField(wasteInv, "wasteStackAnchor", wasteAnchor.transform);
                SetPrivateField(wasteInv, "leftoverVisualPrefab", CreateLeftoverPrefab());
            }

            player.SetActive(true);
            _toCleanup.Add(player);
            return player;
        }

        private GameObject CreateNonPlayerObject()
        {
            var obj = new GameObject("NonPlayer");
            obj.SetActive(false);
            obj.AddComponent<BoxCollider>().isTrigger = true;
            obj.SetActive(true);
            _toCleanup.Add(obj);
            return obj;
        }

        private GameObject CreateLeftoverPrefab()
        {
            var prefab = new GameObject("DummyLeftover");
            prefab.SetActive(false);
            _toCleanup.Add(prefab);
            return prefab;
        }

        // --- Test 1: Player collects waste from table ---

        [UnityTest]
        public IEnumerator TableWasteTrigger_PlayerCollectsWaste()
        {
            var (table, _, _) = CreateTableWithWaste(1, 10);
            table.AddLeftovers(5);
            yield return null;

            Assert.That(table.LeftoverCount, Is.EqualTo(5), "Table should have 5 leftovers initially.");

            var player = CreatePlayer(true, 10);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();

            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(player.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            yield return null;

            Assert.That(wasteInv.Count, Is.EqualTo(5), "Player should have collected all 5 leftovers.");
            Assert.That(table.LeftoverCount, Is.EqualTo(0), "Table should have 0 leftovers after collection.");
        }

        // --- Test 2: Non-player objects are ignored ---

        [UnityTest]
        public IEnumerator TableWasteTrigger_NonPlayerIsIgnored()
        {
            var (table, _, _) = CreateTableWithWaste(1, 10);
            table.AddLeftovers(3);
            yield return null;

            var nonPlayer = CreateNonPlayerObject();

            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(nonPlayer.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            yield return null;

            Assert.That(table.LeftoverCount, Is.EqualTo(3), "Table should still have 3 leftovers.");
        }

        // --- Test 3: Partial collection when player capacity is insufficient ---

        [UnityTest]
        public IEnumerator TableWasteTrigger_PartialCollection_WhenPlayerCapacityLimited()
        {
            var (table, _, _) = CreateTableWithWaste(1, 10);
            table.AddLeftovers(8);
            yield return null;

            var player = CreatePlayer(true, 5);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();

            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(player.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            yield return null;

            Assert.That(wasteInv.Count, Is.EqualTo(5), "Player should have collected 5 (full capacity).");
            Assert.That(table.LeftoverCount, Is.EqualTo(3), "Table should have 3 leftovers remaining.");
        }

        // --- Test 4: Table visual count decreases ---

        [UnityTest]
        public IEnumerator TableWasteTrigger_VisualsRefreshAfterCollection()
        {
            var (table, _, visuals) = CreateTableWithWaste(1, 10);
            table.AddLeftovers(4);
            yield return null;

            var activeVisualsField = typeof(TableWasteVisuals).GetField("_activeVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var activeVisuals = (List<GameObject>)activeVisualsField.GetValue(visuals);
            Assert.That(activeVisuals.Count, Is.EqualTo(4), "Should have 4 leftover visuals initially.");

            var player = CreatePlayer(true, 10);
            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(player.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            yield return null;

            Assert.That(table.LeftoverCount, Is.EqualTo(0), "Table should have 0 leftovers.");
            activeVisuals = (List<GameObject>)activeVisualsField.GetValue(visuals);
            Assert.That(activeVisuals.Count, Is.EqualTo(0), "Should have 0 leftover visuals after collection.");
        }

        // --- Test 5: Player waste disposed at TrashStation ---

        [UnityTest]
        public IEnumerator TrashStation_DisposesPlayerWaste()
        {
            var player = CreatePlayer(true, 10);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();
            wasteInv.TryAdd(7);
            yield return null;

            var stationGO = new GameObject("TrashStation");
            stationGO.SetActive(false);
            var station = stationGO.AddComponent<TrashStation>();
            var trashTarget = new GameObject("TrashTarget");
            trashTarget.transform.SetParent(stationGO.transform);
            SetPrivateField(station, "trashTarget", trashTarget.transform);
            SetPrivateField(station, "wasteVisualPrefab", CreateLeftoverPrefab());
            stationGO.SetActive(true);
            _toCleanup.Add(stationGO);

            station.TryDisposeWaste(wasteInv);
            yield return null;

            Assert.That(wasteInv.Count, Is.EqualTo(0), "Player waste should be 0 after disposal.");
        }

        // --- Test 6: Existing pizza disposal remains unchanged ---

        [UnityTest]
        public IEnumerator TrashStation_PizzaDisposalStillWorks()
        {
            var player = new GameObject("PizzaPlayer");
            player.tag = "Player";
            player.SetActive(false);
            var anchor = new GameObject("PizzaAnchor");
            anchor.transform.SetParent(player.transform);
            var pizzaInv = player.AddComponent<PlayerPizzaInventory>();
            SetPrivateField(pizzaInv, "capacity", 10);
            SetPrivateField(pizzaInv, "pizzaStackAnchor", anchor.transform);
            player.SetActive(true);
            _toCleanup.Add(player);

            pizzaInv.TryAdd(4);
            yield return null;

            _trashSettings = ScriptableObject.CreateInstance<STrashStation>();
            _trashSettings.animationDuration = 0.01f;
            _trashSettings.staggerDelay = 0.01f;

            var stationGO = new GameObject("TrashStation");
            stationGO.SetActive(false);
            var station = stationGO.AddComponent<TrashStation>();
            var trashTarget = new GameObject("TrashTarget");
            trashTarget.transform.SetParent(stationGO.transform);
            var pizzaVisualPrefab = CreateLeftoverPrefab();
            SetPrivateField(station, "sTrashStation", _trashSettings);
            SetPrivateField(station, "pizzaVisualPrefab", pizzaVisualPrefab);
            SetPrivateField(station, "trashTarget", trashTarget.transform);
            stationGO.SetActive(true);
            _toCleanup.Add(stationGO);

            station.TrashAllPizzas(pizzaInv);
            yield return null;

            Assert.That(pizzaInv.Count, Is.EqualTo(0), "Player pizzas should be 0 after disposal.");
        }

        // --- Test 7: Table becomes usable after waste removal ---

        [UnityTest]
        public IEnumerator Table_BecomesAvailableAfterLeftoversRemoved()
        {
            var (table, manager, _) = CreateTableWithWaste(1, 5);
            table.AddLeftovers(5);
            yield return null;

            Assert.That(table.CanAcceptLeftovers(1), Is.False, "Table at max capacity should not accept more.");

            var player = CreatePlayer(true, 10);
            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(player.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            yield return null;

            Assert.That(table.LeftoverCount, Is.EqualTo(0), "Table should have 0 leftovers after collection.");
            Assert.That(table.CanAcceptLeftovers(1), Is.True, "Table should accept new leftovers after cleanup.");
        }

        // --- Test 8: Duplicate trigger protection ---

        [UnityTest]
        public IEnumerator TableWasteTrigger_NoDuplicateCollection()
        {
            var (table, _, _) = CreateTableWithWaste(1, 10);
            table.AddLeftovers(5);
            yield return null;

            var player = CreatePlayer(true, 10);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();

            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(player.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            InvokePrivateMethod(trigger, "OnTriggerStay", triggerCollider);
            yield return null;

            Assert.That(wasteInv.Count, Is.EqualTo(5), "Player should have exactly 5 (no duplicates).");
            Assert.That(table.LeftoverCount, Is.EqualTo(0), "Table should have 0 leftovers.");
        }

        // --- Test 9: Player waste visuals pool ---

        [UnityTest]
        public IEnumerator PlayerWasteInventory_VisualsCreatedAndPooled()
        {
            var player = CreatePlayer(true, 3);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();
            yield return null;

            var visualsList = GetPrivateField<List<GameObject>>(wasteInv, "_wasteVisuals");
            Assert.That(visualsList.Count, Is.EqualTo(3), "Should have 3 pooled visuals for capacity 3.");
            Assert.That(visualsList[0].activeSelf, Is.False, "All visuals should start inactive.");
        }

        // --- Test 10: Player waste inventory zero capacity ---

        [UnityTest]
        public IEnumerator PlayerWasteInventory_ZeroCapacity_RejectsAll()
        {
            var player = CreatePlayer(true, 0);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();
            yield return null;

            var accepted = wasteInv.TryAdd(5);
            Assert.That(accepted, Is.EqualTo(0), "Zero capacity should accept nothing.");
            Assert.That(wasteInv.Count, Is.EqualTo(0));
        }

        // --- Test 11: Waste disposed when no visual prefab ---

        [UnityTest]
        public IEnumerator TrashStation_DisposesWasteWithoutVisuals()
        {
            var player = CreatePlayer(true, 5);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();
            wasteInv.TryAdd(3);
            yield return null;

            var stationGO = new GameObject("TrashStationNoVisuals");
            stationGO.SetActive(false);
            var station = stationGO.AddComponent<TrashStation>();
            var trashTarget = new GameObject("TrashTarget");
            trashTarget.transform.SetParent(stationGO.transform);
            SetPrivateField(station, "trashTarget", trashTarget.transform);
            SetPrivateField(station, "wasteVisualPrefab", (GameObject)null);
            stationGO.SetActive(true);
            _toCleanup.Add(stationGO);

            station.TryDisposeWaste(wasteInv);
            yield return null;

            Assert.That(wasteInv.Count, Is.EqualTo(0), "Waste should be 0 even without visuals.");
        }

        // --- Test 12: Null waste inventory is no-op ---

        [UnityTest]
        public IEnumerator TrashStation_NullWasteInventory_DoesNotError()
        {
            var stationGO = new GameObject("TrashStationNoOp");
            stationGO.SetActive(false);
            var station = stationGO.AddComponent<TrashStation>();
            stationGO.SetActive(true);
            _toCleanup.Add(stationGO);

            Assert.DoesNotThrow(() => station.TryDisposeWaste(null));
            yield return null;
        }

        // --- Test 13: Waste disposed raises event ---

        [UnityTest]
        public IEnumerator TrashStation_WasteDisposal_RaisesEvent()
        {
            var player = CreatePlayer(true, 5);
            var wasteInv = player.GetComponent<PlayerWasteInventory>();
            wasteInv.TryAdd(2);
            yield return null;

            var channel = ScriptableObject.CreateInstance<SVoidEventChannel>();
            var invocationCount = 0;
            channel.RegisterListener(() => invocationCount++);

            var stationGO = new GameObject("TrashStationEvent");
            stationGO.SetActive(false);
            var station = stationGO.AddComponent<TrashStation>();
            SetPrivateField(station, "wasteDisposedEvent", channel);
            stationGO.SetActive(true);
            _toCleanup.Add(stationGO);

            station.TryDisposeWaste(wasteInv);
            yield return new WaitForSeconds(0.1f);

            Assert.That(invocationCount, Is.EqualTo(1), "Waste disposed event should fire once.");
            Object.Destroy(channel);
        }

        // --- Test 14: Player without waste inventory ---

        [UnityTest]
        public IEnumerator TableWasteTrigger_PlayerWithoutWasteInventory_Ignored()
        {
            var (table, _, _) = CreateTableWithWaste(1, 10);
            table.AddLeftovers(3);
            yield return null;

            var player = CreatePlayer(false);

            var triggerGO = new GameObject("TableWasteTrigger");
            triggerGO.transform.SetParent(player.transform);
            var triggerCollider = triggerGO.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            _toCleanup.Add(triggerGO);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            SetPrivateField(trigger, "table", table);

            InvokePrivateMethod(trigger, "OnTriggerEnter", triggerCollider);
            yield return null;

            Assert.That(table.LeftoverCount, Is.EqualTo(3), "Table leftovers should remain unchanged.");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected {target.GetType().Name} to define '{methodName}'.");
            method.Invoke(target, arguments);
        }
    }
}
