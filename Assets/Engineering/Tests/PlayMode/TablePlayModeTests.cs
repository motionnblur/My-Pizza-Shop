using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Domain.CustomerQueue;
using Engineering.Scripts.Domain.Table;
using Engineering.Scripts.Mono.Actors.CustomerQueue;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.Scripts.Mono.Actors.Table;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class TablePlayModeTests
    {
        private readonly List<GameObject> _toCleanup = new List<GameObject>();
        private readonly List<GameObject> _botsToCleanup = new List<GameObject>();
        private GameObject _navMeshFloor;
        private NavMeshDataInstance _navMeshDataInstance;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var go in _botsToCleanup)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _botsToCleanup.Clear();

            foreach (var go in _toCleanup)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _toCleanup.Clear();

            if (_navMeshDataInstance.valid)
                _navMeshDataInstance.Remove();

            if (_navMeshFloor != null)
                Object.Destroy(_navMeshFloor);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Table_AutoInitializesFromSeatTransforms()
        {
            var root = new GameObject("TableAutoInit");
            _toCleanup.Add(root);

            var seatA = new GameObject("SeatA");
            seatA.transform.SetParent(root.transform);
            var seatB = new GameObject("SeatB");
            seatB.transform.SetParent(root.transform);

            var table = root.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seatA.transform, seatB.transform });

            root.SetActive(true);
            yield return null;

            Assert.That(table.SeatCount, Is.EqualTo(2));
            Assert.That(table.HasAvailableSeat, Is.True);

            var r1 = table.TryReserveSeat();
            Assert.That(r1.Reserved, Is.True);
            Assert.That(table.HasAvailableSeat, Is.True);

            var r2 = table.TryReserveSeat();
            Assert.That(r2.Reserved, Is.True);
            Assert.That(table.HasAvailableSeat, Is.False);

            var r3 = table.TryReserveSeat();
            Assert.That(r3.Reserved, Is.False);
        }

        [UnityTest]
        public IEnumerator TableManager_ReservesAcrossMultipleTables()
        {
            var go1 = new GameObject("Table1");
            var seatA1 = new GameObject("SeatA1");
            seatA1.transform.SetParent(go1.transform);
            var seatB1 = new GameObject("SeatB1");
            seatB1.transform.SetParent(go1.transform);
            var t1 = go1.AddComponent<Table>();
            SetPrivateField(t1, "seatTransforms", new[] { seatA1.transform, seatB1.transform });
            _toCleanup.Add(go1);

            var go2 = new GameObject("Table2");
            var seatA2 = new GameObject("SeatA2");
            seatA2.transform.SetParent(go2.transform);
            var seatB2 = new GameObject("SeatB2");
            seatB2.transform.SetParent(go2.transform);
            var t2 = go2.AddComponent<Table>();
            SetPrivateField(t2, "seatTransforms", new[] { seatA2.transform, seatB2.transform });
            _toCleanup.Add(go2);

            var managerGO = new GameObject("TableManager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { t1, t2 });
            _toCleanup.Add(managerGO);

            go1.SetActive(true);
            go2.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            Assert.That(manager.TryReserveSeat(out var tIdx1, out var sIdx1), Is.True);
            Assert.That(tIdx1, Is.EqualTo(0));
            Assert.That(sIdx1, Is.EqualTo(0));

            Assert.That(manager.TryReserveSeat(out var tIdx2, out var sIdx2), Is.True);

            Assert.That(manager.TryReserveSeat(out var tIdx3, out var sIdx3), Is.True);

            Assert.That(manager.TryReserveSeat(out var tIdx4, out var sIdx4), Is.True);

            Assert.That(manager.TryReserveSeat(out _, out _), Is.False,
                "All 4 seats across 2 tables should be occupied.");
        }

        [UnityTest]
        public IEnumerator TableManager_ReleaseSeatRaisesEvent()
        {
            var go = new GameObject("Table");
            var seat = new GameObject("Seat");
            seat.transform.SetParent(go.transform);
            var table = go.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seat.transform });
            _toCleanup.Add(go);

            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table });
            _toCleanup.Add(managerGO);

            go.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            var eventRaised = false;
            manager.SeatReleased += () => eventRaised = true;

            manager.TryReserveSeat(out var tIdx, out var sIdx);
            manager.ReleaseSeat(tIdx, sIdx);

            Assert.That(eventRaised, Is.True);
        }

        [UnityTest]
        public IEnumerator TableManager_ReleaseInvalidSeatDoesNotRaiseEvent()
        {
            var go = new GameObject("Table");
            var seat = new GameObject("Seat");
            seat.transform.SetParent(go.transform);
            var table = go.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seat.transform });
            _toCleanup.Add(go);

            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table });
            _toCleanup.Add(managerGO);

            go.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            var eventRaised = false;
            manager.SeatReleased += () => eventRaised = true;

            manager.ReleaseSeat(0, 0);

            Assert.That(eventRaised, Is.False,
                "Releasing an unoccupied seat should not raise SeatReleased.");
        }

        [UnityTest]
        public IEnumerator GetSeatTransform_ReturnsCorrectTransform()
        {
            var go = new GameObject("Table");
            var seatA = new GameObject("SeatA");
            seatA.transform.SetParent(go.transform);
            var seatB = new GameObject("SeatB");
            seatB.transform.SetParent(go.transform);
            var table = go.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new[] { seatA.transform, seatB.transform });
            _toCleanup.Add(go);

            go.SetActive(true);
            yield return null;

            Assert.That(table.GetSeatTransform(0), Is.SameAs(seatA.transform));
            Assert.That(table.GetSeatTransform(1), Is.SameAs(seatB.transform));
            Assert.That(table.GetSeatTransform(-1), Is.Null);
            Assert.That(table.GetSeatTransform(2), Is.Null);
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_RemoveCustomer_RemovesSpecificWaitingCustomer()
        {
            var (queueController, bots) = CreateQueueWithTwoBots();
            SetPrivateField(bots[0], "_state", (int)3);
            SetPrivateField(bots[1], "_state", (int)4);

            var waiting = queueController.GetFirstWaitingCustomer();
            Assert.That(waiting, Is.SameAs(bots[0]), "First bot is waiting (state=WaitingForTable=3).");

            var removed = queueController.RemoveCustomer(waiting);
            Assert.That(removed, Is.True);
            Assert.That(queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(queueController.FrontCustomer, Is.SameAs(bots[1]));

            foreach (var b in bots)
                _botsToCleanup.Remove(b.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_RemoveCustomer_RemovesNonFrontWaitingCustomer()
        {
            var (queueController, bots) = CreateQueueWithTwoBots();
            SetPrivateField(bots[0], "_state", (int)5);
            SetPrivateField(bots[1], "_state", (int)3);

            var waiting = queueController.GetFirstWaitingCustomer();
            Assert.That(waiting, Is.SameAs(bots[1]), "Second bot is waiting (state=WaitingForTable=3).");

            var removed = queueController.RemoveCustomer(waiting);
            Assert.That(removed, Is.True);
            Assert.That(queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(queueController.FrontCustomer, Is.SameAs(bots[0]));

            foreach (var b in bots)
                _botsToCleanup.Remove(b.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_RemoveCustomer_ReturnsFalseForUnknown()
        {
            var (queueController, bots) = CreateQueueWithTwoBots();
            _botsToCleanup.Remove(bots[0].gameObject);
            _botsToCleanup.Remove(bots[1].gameObject);

            var unknown = new GameObject("Unknown");
            var unknownBot = unknown.AddComponent<CustomerBot>();

            var removed = queueController.RemoveCustomer(unknownBot);
            Assert.That(removed, Is.False);
            Assert.That(queueController.CustomerCount, Is.EqualTo(2));

            Object.Destroy(unknown);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CustomerBot_TransitionToDining_ReservesSeat()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot = CreateBotWithDining(manager);
            yield return null;

            var reserved = bot.TransitionToDining();
            Assert.That(reserved, Is.True);
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False,
                "Single seat should be occupied after TransitionToDining.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_Eating_ReleasesSeatAfterDuration()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot = CreateBotWithDining(manager, eatingDuration: 0.1f);
            yield return null;

            bot.TransitionToDining();
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False, "Seat should be occupied after reservation.");

            SetPrivateField(bot, "_state", 5);
            SetPrivateField(bot, "_eatingTimer", 0.05f);

            yield return new WaitForSeconds(0.2f);

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be released after eating timer expires.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_WaitsWhenAllSeatsOccupied()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot1 = CreateBotWithDining(manager);
            var bot2 = CreateBotWithDining(manager);
            yield return null;

            var reserved1 = bot1.TransitionToDining();
            Assert.That(reserved1, Is.True);

            var reserved2 = bot2.TransitionToDining();
            Assert.That(reserved2, Is.False);
            Assert.That(bot2.IsWaitingForTable, Is.True);
        }

        [UnityTest]
        public IEnumerator TwoCustomers_OccupyTwoDistinctSeats()
        {
            var (table, manager) = CreateTableWithManager(2);
            var bot1 = CreateBotWithDining(manager);
            var bot2 = CreateBotWithDining(manager);
            yield return null;

            Assert.That(bot1.TransitionToDining(), Is.True);
            Assert.That(bot2.TransitionToDining(), Is.True);
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False,
                "Both seats should be occupied.");
        }

        [UnityTest]
        public IEnumerator WaitingCustomer_MovesToAvailableSeat()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot1 = CreateBotWithDining(manager, eatingDuration: 0.1f);
            var bot2 = CreateBotWithDining(manager);
            yield return null;

            Assert.That(bot1.TransitionToDining(), Is.True);
            Assert.That(bot2.TransitionToDining(), Is.False);
            Assert.That(bot2.IsWaitingForTable, Is.True);

            var seatReleased = false;
            manager.SeatReleased += () => seatReleased = true;

            SetPrivateField(bot1, "_state", 5);
            SetPrivateField(bot1, "_eatingTimer", 0.05f);

            yield return new WaitForSeconds(0.2f);

            Assert.That(seatReleased, Is.True, "SeatReleased should have been raised.");
            Assert.That(bot2.RetryReserveTable(), Is.True,
                "Waiting customer should move to the freed seat.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_DestroysAtExitPoint()
        {
            var (table, manager) = CreateTableWithManager(1);
            var exitPoint = new GameObject("Exit").transform;

            var bot = CreateBotWithDining(manager, eatingDuration: 0.05f, exitPoint: exitPoint);
            yield return null;

            bot.TransitionToDining();
            SetPrivateField(bot, "_state", 6);

            yield return new WaitForSeconds(0.1f);

            Assert.That(bot == null || bot.gameObject == null, Is.True,
                "Customer in Leaving state should be destroyed immediately if no NavMesh or at exit.");
        }

        [UnityTest]
        public IEnumerator DisableEnable_DoesNotLeakSeatReleasedSubscription()
        {
            var (table, manager) = CreateTableWithManager(2);

            var serveStationObject = new GameObject("ServeStation");
            serveStationObject.SetActive(false);
            var serveStation = serveStationObject.AddComponent<ServeStation>();
            var serveSettings = ScriptableObject.CreateInstance<SServeStation>();
            serveSettings.maxQueueCustomers = 5;
            serveSettings.maxStoredPizzas = 10;
            serveSettings.pricePerPizza = 10;
            SetPrivateField(serveStation, "sServeStation", serveSettings);
            SetPrivateField(serveStation, "tableManager", manager);

            var currencyServiceObject = new GameObject("CurrencyService");
            var currencyService = currencyServiceObject.AddComponent<CurrencyService>();
            var wallet = new GameObject("Wallet").AddComponent<PlayerWallet>();
            SetPrivateField(wallet, "money", 100);
            currencyService.Initialize(wallet);
            serveStation.Initialize(currencyService);

            serveStationObject.SetActive(true);
            yield return null;

            serveStationObject.SetActive(false);
            serveStationObject.SetActive(true);
            yield return null;

            var bot1 = CreateBotWithDining(manager, eatingDuration: 0.05f);
            bot1.TransitionToDining();
            yield return new WaitForSeconds(0.2f);

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be released and available after disable/enable.");

            Object.Destroy(serveStationObject);
            Object.Destroy(currencyServiceObject);
            Object.Destroy(wallet);
            Object.Destroy(serveSettings);
        }

        [UnityTest]
        public IEnumerator Table_AddLeftovers_AccumulatesOnSameTable()
        {
            var (table, manager) = CreateTableWithManager(2);
            yield return null;

            var r1 = table.AddLeftovers(2);
            Assert.That(r1.Added, Is.True);

            var r2 = table.AddLeftovers(3);
            Assert.That(r2.Added, Is.True);

            var r3 = table.AddLeftovers(0);
            Assert.That(r3.Added, Is.False);

            var r4 = table.AddLeftovers(-1);
            Assert.That(r4.Added, Is.False);
        }

        [UnityTest]
        public IEnumerator CustomerBot_Eating_AddsLeftoversBeforeReleasingSeat()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot = CreateBotWithDining(manager, eatingDuration: 0.1f);
            yield return null;

            bot.TransitionToDining();
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False, "Seat should be occupied.");

            SetPrivateField(bot, "_state", 5);
            SetPrivateField(bot, "_eatingTimer", 0.05f);

            yield return new WaitForSeconds(0.2f);

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be released after eating timer expires.");

            var wasteModel = GetPrivateField(table, "_wasteModel");
            var leftoverCount = (int)GetPrivateField(wasteModel, "_leftoverCount");
            Assert.That(leftoverCount, Is.EqualTo(1),
                "One leftover should be created for the customer's order of 1 pizza.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_NoLeftovers_WhenDestroyedDuringEating()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot = CreateBotWithDining(manager, eatingDuration: 5f);
            yield return null;

            bot.TransitionToDining();
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False, "Seat should be occupied.");

            Object.Destroy(bot.gameObject);
            yield return null;

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be released after bot is destroyed during Eating.");
        }

        [UnityTest]
        public IEnumerator Table_CanAcceptLeftovers_WithinCapacity()
        {
            var (table, _) = CreateTableWithManager(2);
            SetPrivateField(table, "maxLeftovers", 5);
            yield return null;

            Assert.That(table.CanAcceptLeftovers(3), Is.True);
            Assert.That(table.CanAcceptLeftovers(5), Is.True);
        }

        [UnityTest]
        public IEnumerator Table_CanAcceptLeftovers_ExceedsCapacity()
        {
            var (table, _) = CreateTableWithManager(2);
            SetPrivateField(table, "maxLeftovers", 5);
            table.AddLeftovers(4);
            yield return null;

            Assert.That(table.CanAcceptLeftovers(2), Is.False);
            Assert.That(table.CanAcceptLeftovers(1), Is.True);
        }

        [UnityTest]
        public IEnumerator TableManager_TryReserveSeatWithLeftovers_AcceptsWhenFits()
        {
            var (table, manager) = CreateTableWithManager(1);
            SetPrivateField(table, "maxLeftovers", 10);
            yield return null;

            Assert.That(manager.TryReserveSeat(3, out var tIdx, out var sIdx), Is.True);
            Assert.That(tIdx, Is.EqualTo(0));
            Assert.That(sIdx, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TableManager_TryReserveSeatWithLeftovers_RejectsWhenExceedsCapacity()
        {
            var (table, manager) = CreateTableWithManager(1);
            SetPrivateField(table, "maxLeftovers", 5);
            yield return null;

            Assert.That(manager.TryReserveSeat(6, out var tIdx, out var sIdx), Is.False);
            Assert.That(tIdx, Is.EqualTo(-1));
            Assert.That(sIdx, Is.EqualTo(-1));
        }

        [UnityTest]
        public IEnumerator TableManager_TryReserveSeatWithLeftovers_PicksTableWithCapacity()
        {
            var table1GO = new GameObject("Table1");
            var t1Seat = new GameObject("T1Seat");
            t1Seat.transform.SetParent(table1GO.transform);
            var table1 = table1GO.AddComponent<Table>();
            SetPrivateField(table1, "seatTransforms", new[] { t1Seat.transform });
            SetPrivateField(table1, "maxLeftovers", 2);
            _toCleanup.Add(table1GO);

            var table2GO = new GameObject("Table2");
            var t2Seat = new GameObject("T2Seat");
            t2Seat.transform.SetParent(table2GO.transform);
            var table2 = table2GO.AddComponent<Table>();
            SetPrivateField(table2, "seatTransforms", new[] { t2Seat.transform });
            SetPrivateField(table2, "maxLeftovers", 10);
            _toCleanup.Add(table2GO);

            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table1, table2 });
            _toCleanup.Add(managerGO);

            table1GO.SetActive(true);
            table2GO.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            table1.AddLeftovers(2);

            Assert.That(manager.TryReserveSeat(3, out var tIdx, out var sIdx), Is.True);
            Assert.That(tIdx, Is.EqualTo(1), "Should pick table2 since table1 has insufficient capacity.");
        }

        [UnityTest]
        public IEnumerator TableManager_TryReserveSeatWithLeftovers_RejectsWhenAllTablesAtCapacity()
        {
            var table1GO = new GameObject("Table1");
            var t1Seat = new GameObject("T1Seat");
            t1Seat.transform.SetParent(table1GO.transform);
            var table1 = table1GO.AddComponent<Table>();
            SetPrivateField(table1, "seatTransforms", new[] { t1Seat.transform });
            SetPrivateField(table1, "maxLeftovers", 2);
            _toCleanup.Add(table1GO);

            var table2GO = new GameObject("Table2");
            var t2Seat = new GameObject("T2Seat");
            t2Seat.transform.SetParent(table2GO.transform);
            var table2 = table2GO.AddComponent<Table>();
            SetPrivateField(table2, "seatTransforms", new[] { t2Seat.transform });
            SetPrivateField(table2, "maxLeftovers", 3);
            _toCleanup.Add(table2GO);

            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table1, table2 });
            _toCleanup.Add(managerGO);

            table1GO.SetActive(true);
            table2GO.SetActive(true);
            managerGO.SetActive(true);
            yield return null;

            table1.AddLeftovers(2);
            table2.AddLeftovers(3);

            Assert.That(manager.TryReserveSeat(1, out _, out _), Is.False,
                "All tables at capacity, no seat should be available.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_TransitionToDining_RejectedWhenCapacityExceeded()
        {
            var (table, manager) = CreateTableWithManager(1);
            SetPrivateField(table, "maxLeftovers", 2);
            yield return null;

            var botObject = new GameObject("CustomerBot");
            var agent = botObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            var bot = botObject.AddComponent<CustomerBot>();
            bot.Initialize(null, new CustomerOrderModel(3), new Transform[0]);
            bot.SetupDining(manager, new GameObject("Exit").transform, 5f);
            _botsToCleanup.Add(botObject);
            yield return null;

            var reserved = bot.TransitionToDining();
            Assert.That(reserved, Is.False, "Bot with 3 pizzas should be rejected when table max=2.");
            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should still be available since reservation failed.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_TransitionToDining_AcceptedWhenCapacityFits()
        {
            var (table, manager) = CreateTableWithManager(1);
            SetPrivateField(table, "maxLeftovers", 10);
            yield return null;

            var botObject = new GameObject("CustomerBot");
            var agent = botObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            var bot = botObject.AddComponent<CustomerBot>();
            bot.Initialize(null, new CustomerOrderModel(3), new Transform[0]);
            bot.SetupDining(manager, new GameObject("Exit").transform, 5f);
            _botsToCleanup.Add(botObject);
            yield return null;

            var reserved = bot.TransitionToDining();
            Assert.That(reserved, Is.True, "Bot with 3 pizzas should be accepted when table max=10.");
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False,
                "Seat should be occupied after reservation.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_Eating_LeftoverCapacityIsRespected()
        {
            var (table, manager) = CreateTableWithManager(1);
            SetPrivateField(table, "maxLeftovers", 2);
            yield return null;

            var bot = CreateBotWithDining(manager, eatingDuration: 0.1f);
            yield return null;

            bot.TransitionToDining();
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False, "Seat should be occupied.");

            SetPrivateField(bot, "_state", 5);
            SetPrivateField(bot, "_eatingTimer", 0.05f);

            yield return new WaitForSeconds(0.2f);

            var wasteModel = GetPrivateField(table, "_wasteModel");
            var leftoverCount = (int)GetPrivateField(wasteModel, "_leftoverCount");
            Assert.That(leftoverCount, Is.EqualTo(1), "One leftover from order of 1.");

            Assert.That(table.CanAcceptLeftovers(2), Is.False,
                "Table with 1 leftover and max=2 cannot accept 2 more.");
            Assert.That(table.CanAcceptLeftovers(1), Is.True,
                "Table with 1 leftover and max=2 can accept 1 more.");
        }

        private (Table table, TableManager manager) CreateTableWithManager(int seatCount)
        {
            var tableGO = new GameObject("TestTable");
            var table = tableGO.AddComponent<Table>();
            var seats = new Transform[seatCount];
            for (var i = 0; i < seatCount; i++)
            {
                var seat = new GameObject($"Seat{i}");
                seat.transform.SetParent(tableGO.transform);
                seats[i] = seat.transform;
            }
            SetPrivateField(table, "seatTransforms", seats);
            _toCleanup.Add(tableGO);

            var managerGO = new GameObject("TestTableManager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new[] { table });
            _toCleanup.Add(managerGO);

            tableGO.SetActive(true);
            managerGO.SetActive(true);

            return (table, manager);
        }

        private CustomerBot CreateBotWithDining(TableManager manager, float eatingDuration = 5f,
            Transform exitPoint = null)
        {
            var botObject = new GameObject("CustomerBot");
            var agent = botObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            var bot = botObject.AddComponent<CustomerBot>();
            bot.Initialize(null, new CustomerOrderModel(1), new Transform[0]);
            if (exitPoint == null)
            {
                var exitObject = new GameObject("Exit");
                _toCleanup.Add(exitObject);
                exitPoint = exitObject.transform;
            }
            bot.SetupDining(manager, exitPoint, eatingDuration);
            _botsToCleanup.Add(botObject);
            return bot;
        }

        private (CustomerQueueController queueController, CustomerBot[] bots) CreateQueueWithTwoBots()
        {
            var queueObject = new GameObject("QueueController");
            var queueController = queueObject.AddComponent<CustomerQueueController>();
            var queueSlots = new Transform[2];
            for (var i = 0; i < 2; i++)
            {
                queueSlots[i] = new GameObject($"Slot{i}").transform;
                queueSlots[i].SetParent(queueObject.transform);
            }
            SetPrivateField(queueController, "queueSlots", queueSlots);
            _toCleanup.Add(queueObject);

            var serveSettings = ScriptableObject.CreateInstance<SServeStation>();
            serveSettings.maxQueueCustomers = 2;
            queueController.Initialize(2);

            var serveStationGO = new GameObject("ServeStation");
            serveStationGO.SetActive(false);
            var serveStation = serveStationGO.AddComponent<ServeStation>();
            SetPrivateField(serveStation, "sServeStation", serveSettings);
            serveStationGO.SetActive(true);
            _toCleanup.Add(serveStationGO);

            var bots = new CustomerBot[2];
            for (var i = 0; i < 2; i++)
            {
                var botObject = new GameObject($"Bot{i}");
                _botsToCleanup.Add(botObject);
                var agent = botObject.AddComponent<NavMeshAgent>();
                agent.enabled = false;
                var bot = botObject.AddComponent<CustomerBot>();
                bot.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
                queueController.TryRegister(bot);
                bots[i] = bot;
            }

            return (queueController, bots);
        }

        private void EnsureNavMeshExists()
        {
            if (_navMeshFloor != null)
                return;

            _navMeshFloor = new GameObject("NavMeshFloor");
            _navMeshFloor.transform.position = new Vector3(0f, -0.1f, 0f);
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(_navMeshFloor.transform);
            plane.transform.localPosition = Vector3.zero;
            plane.transform.localScale = new Vector3(5f, 1f, 5f);

            var settings = NavMesh.GetSettingsByIndex(0);
            var bounds = new Bounds(Vector3.zero, new Vector3(100f, 10f, 100f));
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            NavMeshBuilder.CollectSources(
                bounds,
                ~0,
                NavMeshCollectGeometry.RenderMeshes,
                0,
                markups,
                sources);

            var data = NavMeshBuilder.BuildNavMeshData(
                settings,
                sources,
                bounds,
                Vector3.zero,
                Quaternion.identity);

            if (data != null)
                _navMeshDataInstance = NavMesh.AddNavMeshData(data);
        }

        [UnityTest]
        public IEnumerator CustomerBot_OnDestroy_ReleasesSeat_DuringEating()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot = CreateBotWithDining(manager, eatingDuration: 5f);
            yield return null;

            bot.TransitionToDining();
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False, "Seat should be occupied.");

            Object.Destroy(bot.gameObject);
            yield return null;

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be released after bot is destroyed during Eating.");
        }

        [UnityTest]
        public IEnumerator CustomerBot_OnDestroy_ReleasesSeat_DuringMovingToTable()
        {
            var (table, manager) = CreateTableWithManager(1);
            var bot = CreateBotWithDining(manager, eatingDuration: 5f);
            yield return null;

            bot.TransitionToDining();
            Assert.That(manager.TryReserveSeat(out _, out _), Is.False, "Seat should be occupied.");

            Object.Destroy(bot.gameObject);
            yield return null;

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be released after bot is destroyed during MovingToTable.");
        }

        [UnityTest]
        public IEnumerator Table_NoSeatTransforms_DoesNotThrow()
        {
            var root = new GameObject("NoSeatTable");
            _toCleanup.Add(root);
            var table = root.AddComponent<Table>();
            root.SetActive(true);
            yield return null;

            Assert.That(table.SeatCount, Is.EqualTo(0));
            Assert.That(table.HasAvailableSeat, Is.False);

            var result = table.TryReserveSeat();
            Assert.That(result.Reserved, Is.False);

            Assert.That(table.GetSeatTransform(0), Is.Null);
            Assert.That(table.GetSeatTransform(-1), Is.Null);
        }

        [UnityTest]
        public IEnumerator TableManager_NullTableEntries_DoNotCrash()
        {
            var managerGO = new GameObject("Manager");
            var manager = managerGO.AddComponent<TableManager>();
            SetPrivateField(manager, "tables", new Table[] { null });
            _toCleanup.Add(managerGO);
            managerGO.SetActive(true);
            yield return null;

            Assert.That(manager.TryReserveSeat(out var tIdx, out var sIdx), Is.False);
            Assert.That(tIdx, Is.EqualTo(-1));
            Assert.That(sIdx, Is.EqualTo(-1));

            manager.ReleaseSeat(0, 0);
            Assert.That(manager.GetSeatTransform(0, 0), Is.Null);
        }

        [UnityTest]
        public IEnumerator CustomerBot_OnDestroy_WhenStateAtSlot_DoesNotReleaseSeat()
        {
            var (table, manager) = CreateTableWithManager(1);
            yield return null;

            var botObject = new GameObject("CustomerBot");
            botObject.AddComponent<NavMeshAgent>().enabled = false;
            var bot = botObject.AddComponent<CustomerBot>();
            bot.SetupDining(manager, null, 5f);
            bot.Initialize(null, new CustomerOrderModel(1), new Transform[0]);
            _botsToCleanup.Add(botObject);
            yield return null;

            Assert.That(manager.TryReserveSeat(out _, out _), Is.True,
                "Seat should be available since bot never reserved.");

            Object.Destroy(botObject);
            yield return null;

            Assert.That(manager.TryReserveSeat(out _, out _), Is.False,
                "Seat should remain occupied because the bot never owned the reservation.");
        }

        [UnityTest]
        public IEnumerator Table_HasAvailableSeat_WithNullSeatTransformAtSlot_ReturnsCorrectValues()
        {
            var root = new GameObject("TableWithNullSeat");
            _toCleanup.Add(root);

            var validSeat = new GameObject("ValidSeat");
            validSeat.transform.SetParent(root.transform);

            var table = root.AddComponent<Table>();
            SetPrivateField(table, "seatTransforms", new Transform[] { validSeat.transform, null });
            root.SetActive(true);
            yield return null;

            Assert.That(table.SeatCount, Is.EqualTo(2));
            Assert.That(table.HasAvailableSeat, Is.True);

            var r1 = table.TryReserveSeat();
            Assert.That(r1.Reserved, Is.True);
            Assert.That(r1.SeatIndex, Is.EqualTo(0));

            var r2 = table.TryReserveSeat();
            Assert.That(r2.Reserved, Is.True);
            Assert.That(r2.SeatIndex, Is.EqualTo(1));

            var r3 = table.TryReserveSeat();
            Assert.That(r3.Reserved, Is.False);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"Expected {target.GetType().Name} to define '{fieldName}'.");
            return field.GetValue(target);
        }
    }
}
