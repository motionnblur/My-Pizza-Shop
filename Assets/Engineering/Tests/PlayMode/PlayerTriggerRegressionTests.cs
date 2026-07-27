using System.Collections;
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
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    [PrebuildSetup(typeof(PlayerPrefabPrebuildSetup))]
    public class PlayerTriggerRegressionTests
    {
        private readonly List<GameObject> _toCleanup = new();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in _toCleanup)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _toCleanup.Clear();

            yield return null;
        }

        // ---------------------------------------------------------------------
        // Contract-level tests: verify triggers resolve inventories under
        // Player/Scripts when the collider belongs to Player/Mesh.
        // These invoke OnTriggerEnter via reflection for speed; they validate
        // the hierarchy contract, not the Unity physics message dispatch.
        // ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator GrillPlate_ResolvesPizzaInventoryViaGetComponentInChildren()
        {
            var grillFixture = PlayerPrefabTestFixture.CreateGrillStationFixture(
                maxReadyPizzas: 5, productionInterval: 0.02f);
            _toCleanup.Add(grillFixture.StationObject);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(5),
                "GrillStation should produce 5 pizzas before collection.");

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(grillFixture.Plate, "OnTriggerEnter", collider);

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(5),
                "GrillPlate must collect pizzas into the PlayerPizzaInventory under Scripts.");
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(0),
                "All ready pizzas should be collected.");

            grillFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator PlateTrigger_DepositsPizzasFromScriptsInventory()
        {
            var serveFixture = PlayerPrefabTestFixture.CreateServePlateFixture(
                maxStoredPizzas: 10, pricePerPizza: 5);
            _toCleanup.Add(serveFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 6);

            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(serveFixture.PlateTrigger, "OnTriggerEnter", collider);
            yield return null;

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "PlateTrigger must deposit all pizzas from PlayerPizzaInventory under Scripts.");
            Assert.That(serveFixture.Station.StoredPizzaCount, Is.EqualTo(6),
                "ServeStation storage should receive the deposited pizzas.");

            serveFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TrashPlate_RemovesPizzasFromScriptsInventory()
        {
            var trashFixture = PlayerPrefabTestFixture.CreateTrashPlateFixture();
            _toCleanup.Add(trashFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 3);

            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(trashFixture.Plate, "OnTriggerEnter", collider);
            yield return null;

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "TrashPlate must remove pizzas from PlayerPizzaInventory under Scripts.");

            trashFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TrashPlate_RemovesWasteFromScriptsInventory()
        {
            var trashFixture = PlayerPrefabTestFixture.CreateTrashPlateFixture();
            _toCleanup.Add(trashFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddWaste(player, 4);

            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(trashFixture.Plate, "OnTriggerEnter", collider);
            yield return null;

            var wasteInv = PlayerPrefabTestFixture.GetWasteInventory(player);
            Assert.That(wasteInv.Count, Is.EqualTo(0),
                "TrashPlate must remove waste from PlayerWasteInventory under Scripts.");

            trashFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TableWasteTrigger_TransfersLeftoversIntoWasteInventoryOnScripts()
        {
            var tableFixture = PlayerPrefabTestFixture.CreateTableWasteFixture(
                maxLeftovers: 10, initialLeftovers: 5);
            _toCleanup.Add(tableFixture.TableObject);
            _toCleanup.Add(tableFixture.TriggerObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);

            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(tableFixture.Trigger, "OnTriggerEnter", collider);
            yield return null;

            var wasteInv = PlayerPrefabTestFixture.GetWasteInventory(player);
            Assert.That(wasteInv.Count, Is.EqualTo(5),
                "TableWasteTrigger must transfer leftovers into PlayerWasteInventory under Scripts.");
            Assert.That(tableFixture.Table.LeftoverCount, Is.EqualTo(0),
                "All leftovers should be removed from the table.");

            tableFixture.Destroy();
        }

        // ---------------------------------------------------------------------
        // Mutual-exclusion tests: player cannot carry pizzas and waste at once.
        // ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator GrillPlate_DoesNotCollectPizzas_WhenPlayerHasWaste()
        {
            var grillFixture = PlayerPrefabTestFixture.CreateGrillStationFixture(
                maxReadyPizzas: 3, productionInterval: 0.02f);
            _toCleanup.Add(grillFixture.StationObject);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3));

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddWaste(player, 2);

            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(grillFixture.Plate, "OnTriggerEnter", collider);
            yield return null;

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "GrillPlate must not collect pizzas when player carries waste.");
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3),
                "GrillStation pizzas must be preserved.");

            grillFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TableWasteTrigger_DoesNotCollectWaste_WhenPlayerHasPizzas()
        {
            var tableFixture = PlayerPrefabTestFixture.CreateTableWasteFixture(
                maxLeftovers: 10, initialLeftovers: 5);
            _toCleanup.Add(tableFixture.TableObject);
            _toCleanup.Add(tableFixture.TriggerObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 3);

            var collider = PlayerPrefabTestFixture.GetPlayerCollider(player);

            InvokePrivateMethod(tableFixture.Trigger, "OnTriggerEnter", collider);
            yield return null;

            var wasteInv = PlayerPrefabTestFixture.GetWasteInventory(player);
            Assert.That(wasteInv.Count, Is.EqualTo(0),
                "TableWasteTrigger must not collect waste when player carries pizzas.");
            Assert.That(tableFixture.Table.LeftoverCount, Is.EqualTo(5),
                "Table leftovers must be preserved when player has pizzas.");

            tableFixture.Destroy();
        }

        // ---------------------------------------------------------------------
        // Negative tests: non-Player colliders do not alter state.
        // ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator GrillPlate_IgnoresNonPlayerCollider()
        {
            var grillFixture = PlayerPrefabTestFixture.CreateGrillStationFixture(
                maxReadyPizzas: 3, productionInterval: 0.02f);
            _toCleanup.Add(grillFixture.StationObject);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3));

            var nonPlayerCollider = new GameObject("NonPlayerCollider");
            nonPlayerCollider.tag = "Untagged";
            var box = nonPlayerCollider.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayerCollider);

            InvokePrivateMethod(grillFixture.Plate, "OnTriggerEnter", box);

            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3),
                "Non-Player collider must not change GrillStation ready pizza count.");

            grillFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator PlateTrigger_IgnoresNonPlayerCollider()
        {
            var serveFixture = PlayerPrefabTestFixture.CreateServePlateFixture(
                maxStoredPizzas: 10, pricePerPizza: 5);
            _toCleanup.Add(serveFixture.StationObject);

            var nonPlayerCollider = new GameObject("NonPlayerCollider");
            nonPlayerCollider.tag = "Untagged";
            var box = nonPlayerCollider.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayerCollider);

            InvokePrivateMethod(serveFixture.PlateTrigger, "OnTriggerEnter", box);
            yield return null;

            Assert.That(serveFixture.Station.StoredPizzaCount, Is.EqualTo(0),
                "Non-Player collider must not deposit pizzas into the ServeStation.");

            serveFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TrashPlate_IgnoresNonPlayerCollider()
        {
            var trashFixture = PlayerPrefabTestFixture.CreateTrashPlateFixture();
            _toCleanup.Add(trashFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            player.transform.position = new Vector3(0f, -999f, 0f);
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 3);

            var nonPlayerCollider = new GameObject("NonPlayerCollider");
            nonPlayerCollider.tag = "Untagged";
            var box = nonPlayerCollider.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayerCollider);

            InvokePrivateMethod(trashFixture.Plate, "OnTriggerEnter", box);
            yield return null;

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(3),
                "Non-Player collider must not trash pizzas.");

            trashFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TableWasteTrigger_IgnoresNonPlayerCollider()
        {
            var tableFixture = PlayerPrefabTestFixture.CreateTableWasteFixture(
                maxLeftovers: 10, initialLeftovers: 5);
            _toCleanup.Add(tableFixture.TableObject);
            _toCleanup.Add(tableFixture.TriggerObject);

            var nonPlayerCollider = new GameObject("NonPlayerCollider");
            nonPlayerCollider.tag = "Untagged";
            var box = nonPlayerCollider.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayerCollider);

            InvokePrivateMethod(tableFixture.Trigger, "OnTriggerEnter", box);
            yield return null;

            Assert.That(tableFixture.Table.LeftoverCount, Is.EqualTo(5),
                "Non-Player collider must not collect leftovers from the table.");

            tableFixture.Destroy();
        }

        // ---------------------------------------------------------------------
        // Physics regression tests: use real Unity trigger colliders, a
        // kinematic Rigidbody, Physics.SyncTransforms, and WaitForFixedUpdate.
        // OnTriggerEnter is NOT invoked via reflection in these tests.
        // ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator GrillPlatePhysics_TakesPizzasOnTriggerEnter()
        {
            var grillFixture = PlayerPrefabTestFixture.CreateGrillStationFixture(
                maxReadyPizzas: 4, productionInterval: 0.02f);
            _toCleanup.Add(grillFixture.StationObject);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(4));

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);

            PlayerPrefabTestFixture.SetPlayerKinematic(player);

            player.transform.position = Vector3.zero;
            grillFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(4),
                "GrillPlate must collect pizzas via real physics OnTriggerEnter.");
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(0));

            grillFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator PlateTriggerPhysics_DepositsPizzasOnTriggerEnter()
        {
            var serveFixture = PlayerPrefabTestFixture.CreateServePlateFixture(
                maxStoredPizzas: 10, pricePerPizza: 5);
            _toCleanup.Add(serveFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 6);
            PlayerPrefabTestFixture.SetPlayerKinematic(player);

            player.transform.position = Vector3.zero;
            serveFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "PlateTrigger must deposit via real physics OnTriggerEnter.");
            Assert.That(serveFixture.Station.StoredPizzaCount, Is.EqualTo(6));

            serveFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TrashPlatePhysics_RemovesPizzasOnTriggerEnter()
        {
            var trashFixture = PlayerPrefabTestFixture.CreateTrashPlateFixture();
            _toCleanup.Add(trashFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 3);
            PlayerPrefabTestFixture.SetPlayerKinematic(player);

            player.transform.position = Vector3.zero;
            trashFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "TrashPlate must remove pizzas via real physics OnTriggerEnter.");

            trashFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TrashPlatePhysics_RemovesWasteOnTriggerEnter()
        {
            var trashFixture = PlayerPrefabTestFixture.CreateTrashPlateFixture();
            _toCleanup.Add(trashFixture.StationObject);

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddWaste(player, 4);
            PlayerPrefabTestFixture.SetPlayerKinematic(player);

            player.transform.position = Vector3.zero;
            trashFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var wasteInv = PlayerPrefabTestFixture.GetWasteInventory(player);
            Assert.That(wasteInv.Count, Is.EqualTo(0),
                "TrashPlate must remove waste via real physics OnTriggerEnter.");

            trashFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator TableWasteTriggerPhysics_TransfersWasteOnTriggerEnter()
        {
            var tableFixture = PlayerPrefabTestFixture.CreateTableWasteFixture(
                maxLeftovers: 10, initialLeftovers: 5);
            _toCleanup.Add(tableFixture.TableObject);
            _toCleanup.Add(tableFixture.TriggerObject);

            tableFixture.TriggerObject.transform.position = Vector3.zero;

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.SetPlayerKinematic(player);
            player.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            var wasteInv = PlayerPrefabTestFixture.GetWasteInventory(player);
            Assert.That(wasteInv.Count, Is.EqualTo(5),
                "TableWasteTrigger must transfer leftovers via real physics OnTriggerEnter.");
            Assert.That(tableFixture.Table.LeftoverCount, Is.EqualTo(0));

            tableFixture.Destroy();
        }

        // ---------------------------------------------------------------------
        // Physics mutual-exclusion tests.
        // ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Physics_GrillPlate_DoesNotCollectPizzas_WhenPlayerHasWaste()
        {
            var grillFixture = PlayerPrefabTestFixture.CreateGrillStationFixture(
                maxReadyPizzas: 3, productionInterval: 0.02f);
            _toCleanup.Add(grillFixture.StationObject);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3));

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddWaste(player, 2);
            PlayerPrefabTestFixture.SetPlayerKinematic(player);

            player.transform.position = Vector3.zero;
            grillFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            var pizzaInv = PlayerPrefabTestFixture.GetPizzaInventory(player);
            Assert.That(pizzaInv.Count, Is.EqualTo(0),
                "Physics: GrillPlate must not collect pizzas when player carries waste.");
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3),
                "Physics: GrillStation pizzas must be preserved.");

            grillFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator Physics_TableWasteTrigger_DoesNotCollectWaste_WhenPlayerHasPizzas()
        {
            var tableFixture = PlayerPrefabTestFixture.CreateTableWasteFixture(
                maxLeftovers: 10, initialLeftovers: 5);
            _toCleanup.Add(tableFixture.TableObject);
            _toCleanup.Add(tableFixture.TriggerObject);

            tableFixture.TriggerObject.transform.position = Vector3.zero;

            var player = PlayerPrefabTestFixture.InstantiatePlayer();
            _toCleanup.Add(player);
            PlayerPrefabTestFixture.AddPizzas(player, 3);
            PlayerPrefabTestFixture.SetPlayerKinematic(player);
            player.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            var wasteInv = PlayerPrefabTestFixture.GetWasteInventory(player);
            Assert.That(wasteInv.Count, Is.EqualTo(0),
                "Physics: TableWasteTrigger must not collect waste when player carries pizzas.");
            Assert.That(tableFixture.Table.LeftoverCount, Is.EqualTo(5),
                "Physics: Table leftovers must be preserved when player has pizzas.");

            tableFixture.Destroy();
        }

        // ---------------------------------------------------------------------
        // Physics negative tests: non-Player collider interactions.
        // ---------------------------------------------------------------------

        [UnityTest]
        public IEnumerator Physics_GrillPlate_IgnoresNonPlayerCollider()
        {
            var grillFixture = PlayerPrefabTestFixture.CreateGrillStationFixture(
                maxReadyPizzas: 3, productionInterval: 0.02f);
            _toCleanup.Add(grillFixture.StationObject);

            yield return new WaitForSeconds(0.2f);
            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3));

            var nonPlayer = new GameObject("NonPlayerPhysics");
            nonPlayer.tag = "Untagged";
            var rb = nonPlayer.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var box = nonPlayer.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayer);

            nonPlayer.transform.position = Vector3.zero;
            grillFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(grillFixture.Station.ReadyPizzaCount, Is.EqualTo(3),
                "Physics: non-Player collider must not trigger GrillPlate collection.");

            grillFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator Physics_PlateTrigger_IgnoresNonPlayerCollider()
        {
            var serveFixture = PlayerPrefabTestFixture.CreateServePlateFixture(
                maxStoredPizzas: 10, pricePerPizza: 5);
            _toCleanup.Add(serveFixture.StationObject);

            var nonPlayer = new GameObject("NonPlayerPhysics");
            nonPlayer.tag = "Untagged";
            var rb = nonPlayer.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var box = nonPlayer.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayer);

            nonPlayer.transform.position = Vector3.zero;
            serveFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(serveFixture.Station.StoredPizzaCount, Is.EqualTo(0),
                "Physics: non-Player collider must not trigger PlateTrigger deposit.");

            serveFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator Physics_TrashPlate_IgnoresNonPlayerCollider()
        {
            var trashFixture = PlayerPrefabTestFixture.CreateTrashPlateFixture();
            _toCleanup.Add(trashFixture.StationObject);

            var nonPlayer = new GameObject("NonPlayerPhysics");
            nonPlayer.tag = "Untagged";
            var rb = nonPlayer.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var box = nonPlayer.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayer);

            nonPlayer.transform.position = Vector3.zero;
            trashFixture.StationObject.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            var isWasteAnimating = typeof(TrashStation).GetField("_isWasteAnimating",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(isWasteAnimating, Is.Not.Null);
            Assert.That((bool)isWasteAnimating.GetValue(trashFixture.Station), Is.False,
                "Physics: non-Player collider must not trigger TrashPlate.");

            trashFixture.Destroy();
        }

        [UnityTest]
        public IEnumerator Physics_TableWasteTrigger_IgnoresNonPlayerCollider()
        {
            var tableFixture = PlayerPrefabTestFixture.CreateTableWasteFixture(
                maxLeftovers: 10, initialLeftovers: 5);
            _toCleanup.Add(tableFixture.TableObject);
            _toCleanup.Add(tableFixture.TriggerObject);

            var nonPlayer = new GameObject("NonPlayerPhysics");
            nonPlayer.tag = "Untagged";
            var rb = nonPlayer.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var box = nonPlayer.AddComponent<BoxCollider>();
            box.isTrigger = false;
            _toCleanup.Add(nonPlayer);

            tableFixture.TriggerObject.transform.position = Vector3.zero;
            nonPlayer.transform.position = Vector3.zero;

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(tableFixture.Table.LeftoverCount, Is.EqualTo(5),
                "Physics: non-Player collider must not trigger TableWasteTrigger.");

            tableFixture.Destroy();
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private static void InvokePrivateMethod(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null,
                "Expected {0} to define method '{1}'.", target.GetType().Name, methodName);
            method.Invoke(target, arguments);
        }
    }
}
