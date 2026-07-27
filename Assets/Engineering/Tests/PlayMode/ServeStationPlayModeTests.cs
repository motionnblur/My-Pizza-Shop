using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Engineering.Scripts.Domain.CustomerQueue;
using Engineering.Scripts.Mono.Actors.CustomerQueue;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.Scripts.Mono.Actors.Table;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class ServeStationPlayModeTests
    {
        private GameObject _serveStationObject;
        private GameObject _playerObject;
        private GameObject _economyManagerObject;
        private GameObject _currencyServiceObject;
        private CurrencyService _currencyService;
        private SServeStation _serveSettings;
        private SVoidEventChannel _pizzaServedEvent;
        private PlayerPizzaInventory _playerInventory;
        private GameObject _navMeshFloor;
        private NavMeshDataInstance _navMeshDataInstance;
        private readonly List<GameObject> _botsToCleanup = new List<GameObject>();
        private CustomerQueueController _queueController;
        private ServeStationVisuals _stationVisuals;
        private GameObject _tableManagerObject;
        private TableManager _tableManager;
        private Transform _exitPoint;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var bot in _botsToCleanup)
            {
                if (bot != null)
                    Object.Destroy(bot);
            }
            _botsToCleanup.Clear();

            if (_navMeshDataInstance.valid)
                _navMeshDataInstance.Remove();

            if (_navMeshFloor != null)
                Object.Destroy(_navMeshFloor);

            if (_serveStationObject != null)
                Object.Destroy(_serveStationObject);

            if (_playerObject != null)
                Object.Destroy(_playerObject);

            if (_economyManagerObject != null)
                Object.Destroy(_economyManagerObject);

            if (_currencyServiceObject != null)
                Object.Destroy(_currencyServiceObject);

            if (_tableManagerObject != null)
                Object.Destroy(_tableManagerObject);

            if (_serveSettings != null)
                Object.Destroy(_serveSettings);

            if (_pizzaServedEvent != null)
                Object.Destroy(_pizzaServedEvent);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RegisterCustomer_SafeWhenQueueSlotsNull()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            SetPrivateField(_queueController, "queueSlots", null);

            var customer = CreateCustomerBot(3);
            var registered = _serveStationObject.GetComponent<ServeStation>().TryRegisterCustomer(customer);

            Assert.That(registered, Is.False);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator RegisterCustomer_SafeWhenQueueSlotElementNull()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            var slots = new Transform[3];
            slots[0] = new GameObject("Slot0").transform;
            slots[1] = null;
            slots[2] = new GameObject("Slot2").transform;
            SetPrivateField(_queueController, "queueSlots", slots);

            var customer1 = CreateAndRegisterBot(serveStation, 3);

            var customer2 = CreateCustomerBot(3);
            customer2.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            var registered = serveStation.TryRegisterCustomer(customer2);

            Assert.That(registered, Is.False);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CustomerSpawner_SafeWhenSpawnPointNull()
        {
            CreateQueueFixture(maxQueueCustomers: 10, pricePerPizza: 10, playerPizzaCount: 0);
            _serveSettings.customerSpawnInterval = 0.5f;
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var spawnerObject = new GameObject("Spawner");
            spawnerObject.SetActive(false);
            var spawner = spawnerObject.AddComponent<CustomerSpawner>();

            var prefabTemplate = new GameObject("CustomerPrefab");
            prefabTemplate.SetActive(true);
            var agent = prefabTemplate.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            prefabTemplate.AddComponent<CustomerBot>();
            SetPrivateField(spawner, "customerPrefab", prefabTemplate);
            SetPrivateField(spawner, "spawnPoint", null);
            SetPrivateField(spawner, "station", serveStation);
            SetPrivateField(spawner, "sServeStation", _serveSettings);

            spawnerObject.SetActive(true);
            yield return new WaitForSeconds(1.2f);

            Assert.That(serveStation.CustomerCount, Is.EqualTo(0));

            spawnerObject.SetActive(false);
            Object.Destroy(prefabTemplate);
        }

        [UnityTest]
        public IEnumerator TryDepositPizzas_WorksWhenNoCustomer()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            var deposited = serveStation.DepositFrom(_playerInventory);

            Assert.That(deposited, Is.EqualTo(3));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));
            Assert.That(_playerInventory.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TryDepositPizzas_DoesNotExceedMaxStored()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 10);
            _serveSettings.maxStoredPizzas = 10;
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            var deposited = serveStation.DepositFrom(_playerInventory);
            Assert.That(deposited, Is.EqualTo(10));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(10));
            Assert.That(_playerInventory.Count, Is.EqualTo(0));

            _playerInventory.TryAdd(5);
            var secondDeposit = serveStation.DepositFrom(_playerInventory);
            Assert.That(secondDeposit, Is.EqualTo(0));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(10));
        }

        [UnityTest]
        public IEnumerator StationPizzaVisuals_FollowStoredCount()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var visualPrefab = new GameObject("PizzaVisual");
            visualPrefab.AddComponent<MeshRenderer>();
            SetPrivateField(_stationVisuals, "pizzaVisualPrefab", visualPrefab);

            var pizzaVisualsField = typeof(ServeStationVisuals).GetField("_pizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            pizzaVisuals.Clear();

            InvokePrivateMethod(_stationVisuals, "CreateVisualPool", 10);
            InvokePrivateMethod(_stationVisuals, "Refresh", 0);

            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            Assert.That(pizzaVisuals.Count, Is.EqualTo(10));

            var activeCount = CountActive(pizzaVisuals);
            Assert.That(activeCount, Is.EqualTo(0));

            serveStation.DepositFrom(_playerInventory);
            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(3));

            _playerInventory.TryAdd(5);
            serveStation.DepositFrom(_playerInventory);
            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(8));

            Object.Destroy(visualPrefab);
        }

        [UnityTest]
        public IEnumerator TryDepositPizzas_DoesNotAwardMoneyOrRaiseEvent()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3, startMoney: 50);
            var invocationCount = 0;
            _pizzaServedEvent.RegisterListener(() => invocationCount++);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            serveStation.DepositFrom(_playerInventory);

            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(50));
            Assert.That(invocationCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_TransfersPizzasToFrontCustomer()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            var result = serveStation.ServeFrontCustomer();

            Assert.That(result, Is.EqualTo(3));
            Assert.That(_playerInventory.Count, Is.EqualTo(0));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_TransfersToFirstCustomerOnly()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 5);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var firstCustomer = CreateAndRegisterBot(serveStation, orderAmount: 5);
            var secondCustomer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(firstCustomer, "_hasReachedAssignedSlot", true);
            SetPrivateField(secondCustomer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            var result = serveStation.ServeFrontCustomer();

            Assert.That(result, Is.EqualTo(5));
            Assert.That(firstCustomer.RemainingPizzaCount, Is.EqualTo(0));
            Assert.That(secondCustomer.RemainingPizzaCount, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_PartialDeliveryDoesNotAdvanceQueue()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            var result = serveStation.ServeFrontCustomer();

            Assert.That(result, Is.EqualTo(3));
            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(2));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_CompletedOrderRemovesAndReassigns()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var firstCustomer = CreateAndRegisterBot(serveStation, orderAmount: 2);
            var secondCustomer = CreateAndRegisterBot(serveStation, orderAmount: 5);
            _botsToCleanup.Remove(firstCustomer.gameObject);
            _botsToCleanup.Remove(secondCustomer.gameObject);

            SetPrivateField(firstCustomer, "_hasReachedAssignedSlot", true);
            SetPrivateField(secondCustomer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();
            yield return null;

            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(secondCustomer.RemainingPizzaCount, Is.EqualTo(5));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_ReturnsZeroWhenCustomerNotAtSlot()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            CreateAndRegisterBot(serveStation, orderAmount: 5);
            yield return null;

            serveStation.DepositFrom(_playerInventory);
            var result = serveStation.ServeFrontCustomer();

            Assert.That(result, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_ReturnsZeroWhenStationEmpty()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            var result = serveStation.ServeFrontCustomer();

            Assert.That(result, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_TakesFromStationStorageOnly()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            var result = serveStation.ServeFrontCustomer();
            Assert.That(result, Is.EqualTo(0));
            Assert.That(_playerInventory.Count, Is.EqualTo(3));

            serveStation.DepositFrom(_playerInventory);
            result = serveStation.ServeFrontCustomer();
            Assert.That(result, Is.EqualTo(3));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
            Assert.That(_playerInventory.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator RegisterCustomer_RejectsBeyondCapacity()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 5);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            for (var i = 0; i < 3; i++)
            {
                var customer = CreateCustomerBot(3);
                customer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
                var registered = serveStation.TryRegisterCustomer(customer);
                Assert.That(registered, Is.True, $"Customer {i} should register.");
            }

            var extraCustomer = CreateCustomerBot(3);
            extraCustomer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            var rejected = serveStation.TryRegisterCustomer(extraCustomer);

            Assert.That(rejected, Is.False);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator RegisterCustomer_TenCustomerCapacity()
        {
            CreateQueueFixture(maxQueueCustomers: 10, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            for (var i = 0; i < 10; i++)
            {
                var customer = CreateCustomerBot(3);
                customer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
                Assert.That(serveStation.TryRegisterCustomer(customer), Is.True, $"Customer {i} should register.");
            }

            Assert.That(_queueController.CustomerCount, Is.EqualTo(10));

            var extraCustomer = CreateCustomerBot(3);
            extraCustomer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            Assert.That(serveStation.TryRegisterCustomer(extraCustomer), Is.False);
        }

        [UnityTest]
        public IEnumerator RegisterCustomer_AcceptsAfterFrontCompletes()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 5);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer1 = CreateAndRegisterBot(serveStation, orderAmount: 1);
            _botsToCleanup.Remove(customer1.gameObject);
            var customer2 = CreateAndRegisterBot(serveStation, orderAmount: 3);
            _botsToCleanup.Remove(customer2.gameObject);

            SetPrivateField(customer1, "_hasReachedAssignedSlot", true);
            SetPrivateField(customer2, "_hasReachedAssignedSlot", true);

            var rejectedCustomer = CreateCustomerBot(3);
            Assert.That(serveStation.TryRegisterCustomer(rejectedCustomer), Is.False);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();
            yield return null;

            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));

            var newCustomer = CreateAndRegisterBot(serveStation, orderAmount: 4);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_AwardsMoneyForDeliveredPizzas()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3, startMoney: 50);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();
            yield return null;

            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(80));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_RaisesPizzaServedEventOnce()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            var invocationCount = 0;
            _pizzaServedEvent.RegisterListener(() => invocationCount++);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();

            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_CompleteOrderPaysEventRemovesAndAdvances()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 5, startMoney: 50);
            var invocationCount = 0;
            _pizzaServedEvent.RegisterListener(() => invocationCount++);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var firstCustomer = CreateAndRegisterBot(serveStation, orderAmount: 2);
            var secondCustomer = CreateAndRegisterBot(serveStation, orderAmount: 3);
            _botsToCleanup.Remove(firstCustomer.gameObject);
            _botsToCleanup.Remove(secondCustomer.gameObject);

            SetPrivateField(firstCustomer, "_hasReachedAssignedSlot", true);
            SetPrivateField(secondCustomer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();

            Assert.That(invocationCount, Is.EqualTo(1));
            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(70));
            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));
            Assert.That(secondCustomer.RemainingPizzaCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ServeTrigger_ServesFrontCustomerWithPlayerTag()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);

            var serveTriggerObj = new GameObject("ServeTrigger");
            serveTriggerObj.transform.SetParent(_serveStationObject.transform);
            var serveTrigger = serveTriggerObj.AddComponent<ServeTrigger>();
            SetPrivateField(serveTrigger, "serveStation", serveStation);

            var playerCollider = _playerObject.AddComponent<CapsuleCollider>();
            InvokePrivateMethod(serveTrigger, "OnTriggerEnter", playerCollider);
            yield return null;

            Assert.That(_playerInventory.Count, Is.EqualTo(0));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator ServeTrigger_DoesNotServeWithNonPlayerTag()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));

            var serveTriggerObj = new GameObject("ServeTrigger");
            serveTriggerObj.transform.SetParent(_serveStationObject.transform);
            var serveTrigger = serveTriggerObj.AddComponent<ServeTrigger>();
            SetPrivateField(serveTrigger, "serveStation", serveStation);

            var nonPlayer = new GameObject("NonPlayer");
            var nonPlayerCollider = nonPlayer.AddComponent<CapsuleCollider>();
            InvokePrivateMethod(serveTrigger, "OnTriggerEnter", nonPlayerCollider);
            Object.Destroy(nonPlayer);
            yield return null;

            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(5));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator PlateTrigger_DepositsAndServeTrigger_Serves()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            var playerCollider = _playerObject.AddComponent<CapsuleCollider>();

            Assert.That(_playerInventory.Count, Is.EqualTo(3));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(5));

            var plateTriggerObj = new GameObject("PlateTrigger");
            plateTriggerObj.transform.SetParent(_serveStationObject.transform);
            var plateTrigger = plateTriggerObj.AddComponent<PlateTrigger>();
            SetPrivateField(plateTrigger, "serveStation", serveStation);
            InvokePrivateMethod(plateTrigger, "OnTriggerEnter", playerCollider);

            Assert.That(_playerInventory.Count, Is.EqualTo(0));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));
            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(5));

            var serveTriggerObj = new GameObject("ServeTrigger");
            serveTriggerObj.transform.SetParent(_serveStationObject.transform);
            var serveTrigger = serveTriggerObj.AddComponent<ServeTrigger>();
            SetPrivateField(serveTrigger, "serveStation", serveStation);
            InvokePrivateMethod(serveTrigger, "OnTriggerEnter", playerCollider);

            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
            Assert.That(customer.RemainingPizzaCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator PlateTrigger_FindsInventoryOnPlayerScriptsSibling()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var playerRoot = new GameObject("Player");
            playerRoot.tag = "Player";

            var scriptsObject = new GameObject("Scripts");
            scriptsObject.transform.SetParent(playerRoot.transform);
            var inventory = scriptsObject.AddComponent<PlayerPizzaInventory>();
            inventory.TryAdd(3);

            var meshObject = new GameObject("Mesh");
            meshObject.transform.SetParent(playerRoot.transform);
            meshObject.tag = "Player";
            var playerCollider = meshObject.AddComponent<BoxCollider>();

            var plateTriggerObject = new GameObject("PlateTrigger");
            plateTriggerObject.transform.SetParent(_serveStationObject.transform);
            var plateTrigger = plateTriggerObject.AddComponent<PlateTrigger>();
            SetPrivateField(plateTrigger, "serveStation", serveStation);

            InvokePrivateMethod(plateTrigger, "OnTriggerEnter", playerCollider);

            Assert.That(inventory.Count, Is.EqualTo(0));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));

            Object.Destroy(playerRoot);
            Object.Destroy(plateTriggerObject);
        }

        [UnityTest]
        public IEnumerator CustomerSpawner_OrdersWithinRange()
        {
            CreateQueueFixture(maxQueueCustomers: 10, pricePerPizza: 10, playerPizzaCount: 0);
            _serveSettings.minPizzasPerOrder = 1;
            _serveSettings.maxPizzasPerOrder = 5;
            _serveSettings.customerSpawnInterval = 0.5f;
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var spawnerObject = new GameObject("Spawner");
            spawnerObject.SetActive(false);
            var spawner = spawnerObject.AddComponent<CustomerSpawner>();

            var prefabTemplate = new GameObject("CustomerPrefab");
            prefabTemplate.SetActive(true);
            var agent = prefabTemplate.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            prefabTemplate.AddComponent<CustomerBot>();
            SetPrivateField(spawner, "customerPrefab", prefabTemplate);

            var spawnPoint = new GameObject("SpawnPoint").transform;
            SetPrivateField(spawner, "spawnPoint", spawnPoint);
            SetPrivateField(spawner, "station", serveStation);
            SetPrivateField(spawner, "sServeStation", _serveSettings);

            spawnerObject.SetActive(true);
            yield return new WaitForSeconds(1.2f);

            var customers = GetPrivateField<List<CustomerBot>>(_queueController, "_customers");
            Assert.That(customers.Count, Is.GreaterThan(0));
            foreach (var bot in customers)
            {
                Assert.That(bot.RemainingPizzaCount, Is.InRange(1, 5));
            }

            spawnerObject.SetActive(false);
            Object.Destroy(prefabTemplate);
        }

        [UnityTest]
        public IEnumerator CustomerSpawner_DoesNotExceedCapacity()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 0);
            _serveSettings.minPizzasPerOrder = 1;
            _serveSettings.maxPizzasPerOrder = 3;
            _serveSettings.customerSpawnInterval = 0.5f;
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var spawnerObject = new GameObject("Spawner");
            spawnerObject.SetActive(false);
            var spawner = spawnerObject.AddComponent<CustomerSpawner>();

            var prefabTemplate = new GameObject("CustomerPrefab");
            prefabTemplate.SetActive(true);
            var agent = prefabTemplate.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            prefabTemplate.AddComponent<CustomerBot>();
            SetPrivateField(spawner, "customerPrefab", prefabTemplate);

            var spawnPoint = new GameObject("SpawnPoint").transform;
            SetPrivateField(spawner, "spawnPoint", spawnPoint);
            SetPrivateField(spawner, "station", serveStation);
            SetPrivateField(spawner, "sServeStation", _serveSettings);

            spawnerObject.SetActive(true);
            yield return new WaitForSeconds(2.5f);

            Assert.That(serveStation.CustomerCount, Is.GreaterThan(0));
            Assert.That(serveStation.CustomerCount, Is.LessThanOrEqualTo(3));

            spawnerObject.SetActive(false);
            Object.Destroy(prefabTemplate);
        }

        [UnityTest]
        public IEnumerator CustomerSpawner_DisabledStopsSpawning()
        {
            CreateQueueFixture(maxQueueCustomers: 10, pricePerPizza: 10, playerPizzaCount: 0);
            _serveSettings.minPizzasPerOrder = 1;
            _serveSettings.maxPizzasPerOrder = 1;
            _serveSettings.customerSpawnInterval = 0.5f;
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var spawnerObject = new GameObject("Spawner");
            spawnerObject.SetActive(false);
            var spawner = spawnerObject.AddComponent<CustomerSpawner>();

            var prefabTemplate = new GameObject("CustomerPrefab");
            prefabTemplate.SetActive(true);
            var agent = prefabTemplate.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            prefabTemplate.AddComponent<CustomerBot>();
            SetPrivateField(spawner, "customerPrefab", prefabTemplate);

            var spawnPoint = new GameObject("SpawnPoint").transform;
            SetPrivateField(spawner, "spawnPoint", spawnPoint);
            SetPrivateField(spawner, "station", serveStation);
            SetPrivateField(spawner, "sServeStation", _serveSettings);

            spawnerObject.SetActive(true);
            yield return new WaitForSeconds(1.2f);

            var countBeforeDisable = serveStation.CustomerCount;
            Assert.That(countBeforeDisable, Is.GreaterThanOrEqualTo(1));

            spawnerObject.SetActive(false);
            yield return new WaitForSeconds(2f);

            Assert.That(serveStation.CustomerCount, Is.EqualTo(countBeforeDisable));

            Object.Destroy(prefabTemplate);
        }

        [UnityTest]
        public IEnumerator RegisterCustomer_GivesDistinctSlots()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var slots = GetPrivateField<Transform[]>(_queueController, "queueSlots");

            var customer1 = CreateAndRegisterBot(serveStation, 3);
            var assigned1 = GetPrivateField<Transform>(customer1, "_assignedSlot");
            Assert.That(assigned1, Is.SameAs(slots[0]));

            var customer2 = CreateAndRegisterBot(serveStation, 3);
            var assigned2 = GetPrivateField<Transform>(customer2, "_assignedSlot");
            Assert.That(assigned2, Is.SameAs(slots[1]));

            Assert.That(assigned1, Is.Not.SameAs(assigned2));
            Assert.That(_queueController.CustomerCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CustomerBot_StopsWhenReachingAssignedSlot()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var bot = CreateCustomerBot(3);

            SetPrivateField(bot, "_hasReachedAssignedSlot", true);

            Assert.That(bot.HasReachedAssignedSlot, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomerBot_ApproachStartsAfterRegistration()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var waypointObject = new GameObject("Waypoint");
            var waypoints = new[] { waypointObject.transform };
            var orderModel = new CustomerOrderModel(3);
            var bot = CreateCustomerBot(3);

            bot.Initialize(serveStation, orderModel, waypoints);
            var approachStarted = GetPrivateField<bool>(bot, "_approachStarted");
            Assert.That(approachStarted, Is.False, "Approach must not start before registration.");

            var registered = serveStation.TryRegisterCustomer(bot);
            Assert.That(registered, Is.True);
            approachStarted = GetPrivateField<bool>(bot, "_approachStarted");
            Assert.That(approachStarted, Is.False, "Approach must not start immediately after registration.");

            bot.BeginApproach();
            approachStarted = GetPrivateField<bool>(bot, "_approachStarted");
            Assert.That(approachStarted, Is.True, "Approach must start after BeginApproach is called.");

            Object.Destroy(waypointObject);
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_AdvancesRemainingCustomers()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 5);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var queueSlots = GetPrivateField<Transform[]>(_queueController, "queueSlots");

            var firstCustomer = CreateAndRegisterBot(serveStation, orderAmount: 2);
            var secondCustomer = CreateAndRegisterBot(serveStation, orderAmount: 3);
            _botsToCleanup.Remove(firstCustomer.gameObject);
            _botsToCleanup.Remove(secondCustomer.gameObject);

            SetPrivateField(firstCustomer, "_hasReachedAssignedSlot", true);
            SetPrivateField(secondCustomer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();
            yield return null;

            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));

            var remainingSlot = GetPrivateField<Transform>(secondCustomer, "_assignedSlot");

            Assert.That(remainingSlot, Is.SameAs(queueSlots[0]));
            Assert.That(secondCustomer.RemainingPizzaCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator PlateTrigger_IgnoresNonPlayerColliders()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            var plateTriggerObj = new GameObject("PlateTrigger");
            plateTriggerObj.transform.SetParent(_serveStationObject.transform);
            var plateTrigger = plateTriggerObj.AddComponent<PlateTrigger>();
            SetPrivateField(plateTrigger, "serveStation", serveStation);

            var nonPlayer = new GameObject("NonPlayer");
            var nonPlayerCollider = nonPlayer.AddComponent<CapsuleCollider>();
            InvokePrivateMethod(plateTrigger, "OnTriggerEnter", nonPlayerCollider);
            Object.Destroy(nonPlayer);
            yield return null;

            Assert.That(_playerInventory.Count, Is.EqualTo(3));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator PlateTrigger_SafeWhenServeStationNull()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var plateTriggerObj = new GameObject("PlateTrigger");
            plateTriggerObj.transform.SetParent(_serveStationObject.transform);
            var plateTrigger = plateTriggerObj.AddComponent<PlateTrigger>();

            var playerCollider = _playerObject.AddComponent<CapsuleCollider>();
            InvokePrivateMethod(plateTrigger, "OnTriggerEnter", playerCollider);
            yield return null;

            Assert.That(_playerInventory.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ServeTrigger_SafeWhenServeStationNull()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveTriggerObj = new GameObject("ServeTrigger");
            serveTriggerObj.transform.SetParent(_serveStationObject.transform);
            serveTriggerObj.AddComponent<ServeTrigger>();

            var playerCollider = _playerObject.AddComponent<CapsuleCollider>();
            InvokePrivateMethod(serveTriggerObj.GetComponent<ServeTrigger>(), "OnTriggerEnter", playerCollider);
            yield return null;

            Assert.That(_playerInventory.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_ReturnsZeroWhenCurrencyServiceMissing()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            SetPrivateField(_serveStationObject.GetComponent<ServeStation>(), "_currencyService", null);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            var result = serveStation.ServeFrontCustomer();

            Assert.That(result, Is.EqualTo(0));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(3));
            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator DisableEnable_RetainsStorage()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            var visualPrefab = new GameObject("PizzaVisual");
            visualPrefab.AddComponent<MeshRenderer>();
            SetPrivateField(_stationVisuals, "pizzaVisualPrefab", visualPrefab);

            var pizzaVisualsField = typeof(ServeStationVisuals).GetField("_pizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            pizzaVisuals.Clear();

            InvokePrivateMethod(_stationVisuals, "CreateVisualPool", 10);
            InvokePrivateMethod(_stationVisuals, "Refresh", 0);

            serveStation.DepositFrom(_playerInventory);
            var storedBefore = serveStation.StoredPizzaCount;
            Assert.That(storedBefore, Is.GreaterThan(0));

            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(storedBefore));

            _serveStationObject.SetActive(false);
            yield return null;

            _serveStationObject.SetActive(true);
            yield return null;

            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(storedBefore));

            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(storedBefore));

            Object.Destroy(visualPrefab);
        }

        [UnityTest]
        public IEnumerator RuntimeMaxQueueCustomersChange_ControlsAcceptReject()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            for (var i = 0; i < 2; i++)
            {
                var customer = CreateCustomerBot(3);
                customer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
                Assert.That(serveStation.TryRegisterCustomer(customer), Is.True, $"Customer {i} should register.");
            }

            var rejectCustomer = CreateCustomerBot(3);
            rejectCustomer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            Assert.That(serveStation.TryRegisterCustomer(rejectCustomer), Is.False);

            _serveSettings.maxQueueCustomers = 5;

            _serveStationObject.SetActive(false);
            _serveStationObject.SetActive(true);
            yield return null;

            for (var i = 2; i < 5; i++)
            {
                var customer = CreateCustomerBot(3);
                customer.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
                Assert.That(serveStation.TryRegisterCustomer(customer), Is.True, $"Customer {i} should register after capacity increase.");
            }

            var finalReject = CreateCustomerBot(3);
            finalReject.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            Assert.That(serveStation.TryRegisterCustomer(finalReject), Is.False);

            _serveSettings.maxQueueCustomers = 3;

            _serveStationObject.SetActive(false);
            _serveStationObject.SetActive(true);
            yield return null;

            var stillReject = CreateCustomerBot(3);
            stillReject.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            Assert.That(serveStation.TryRegisterCustomer(stillReject), Is.False);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator RuntimePriceChange_AffectsNextServe()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 5, playerPizzaCount: 5, startMoney: 50);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);
            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(5));

            _serveSettings.pricePerPizza = 20;

            var walletBefore = _playerObject.GetComponent<PlayerWallet>().Money;
            serveStation.ServeFrontCustomer();

            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(walletBefore + 100));
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_TryRegister_Succeeds()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var bot = CreateCustomerBot(3);
            bot.Initialize(_serveStationObject.GetComponent<ServeStation>(), new CustomerOrderModel(3), new Transform[0]);
            var registered = _queueController.TryRegister(bot);

            Assert.That(registered, Is.True);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(_queueController.FrontCustomer, Is.SameAs(bot));
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_TryRegister_RejectsNullCustomer()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var registered = _queueController.TryRegister(null);

            Assert.That(registered, Is.False);
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_TryRegister_RejectsWhenQueueFull()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();

            for (var i = 0; i < 2; i++)
            {
                var bot = CreateCustomerBot(3);
                bot.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
                Assert.That(_queueController.TryRegister(bot), Is.True, $"Customer {i} should register.");
            }

            var extraBot = CreateCustomerBot(3);
            extraBot.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            var registered = _queueController.TryRegister(extraBot);

            Assert.That(registered, Is.False);
            Assert.That(_queueController.CustomerCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_TryRegister_RejectsInvalidQueueSlot()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            SetPrivateField(_queueController, "queueSlots", null);

            var bot = CreateCustomerBot(3);
            bot.Initialize(_serveStationObject.GetComponent<ServeStation>(), new CustomerOrderModel(3), new Transform[0]);
            var registered = _queueController.TryRegister(bot);

            Assert.That(registered, Is.False);
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_RemoveFrontCustomer_RemovesAndDestroys()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var bot = CreateAndRegisterBot(serveStation, 3);
            var botObject = bot.gameObject;
            _botsToCleanup.Remove(botObject);

            _queueController.RemoveFrontCustomer();
            yield return null;

            Assert.That(_queueController.CustomerCount, Is.EqualTo(0));
            Assert.That(botObject == null, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomerQueueController_RemoveFrontCustomer_ReassignsSlots()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var slots = GetPrivateField<Transform[]>(_queueController, "queueSlots");

            var first = CreateAndRegisterBot(serveStation, 3);
            var second = CreateAndRegisterBot(serveStation, 3);
            _botsToCleanup.Remove(first.gameObject);
            _botsToCleanup.Remove(second.gameObject);

            Assert.That(GetPrivateField<Transform>(first, "_assignedSlot"), Is.SameAs(slots[0]));
            Assert.That(GetPrivateField<Transform>(second, "_assignedSlot"), Is.SameAs(slots[1]));

            _queueController.RemoveFrontCustomer();
            yield return null;

            Assert.That(_queueController.CustomerCount, Is.EqualTo(1));
            Assert.That(GetPrivateField<Transform>(second, "_assignedSlot"), Is.SameAs(slots[0]));
        }

        [UnityTest]
        public IEnumerator ServeStationVisuals_Initialize_CreatesPool()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var visualPrefab = new GameObject("PizzaVisual");
            SetPrivateField(_stationVisuals, "pizzaVisualPrefab", visualPrefab);

            _stationVisuals.Initialize(5);

            var pizzaVisualsField = typeof(ServeStationVisuals).GetField("_pizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);

            Assert.That(pizzaVisuals.Count, Is.EqualTo(5));
            Object.Destroy(visualPrefab);
        }

        [UnityTest]
        public IEnumerator ServeStationVisuals_Refresh_ActivatesCorrectCount()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var visualPrefab = new GameObject("PizzaVisual");
            SetPrivateField(_stationVisuals, "pizzaVisualPrefab", visualPrefab);

            _stationVisuals.Initialize(5);
            _stationVisuals.Refresh(3);

            var pizzaVisualsField = typeof(ServeStationVisuals).GetField("_pizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);

            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(3));

            _stationVisuals.Refresh(1);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(1));

            _stationVisuals.Refresh(0);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(0));

            Object.Destroy(visualPrefab);
        }

        [UnityTest]
        public IEnumerator ServeStationVisuals_MissingPrefab_Safe()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            SetPrivateField(_stationVisuals, "pizzaVisualPrefab", null);

            _stationVisuals.Initialize(5);

            var pizzaVisualsField = typeof(ServeStationVisuals).GetField("_pizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(_stationVisuals);

            Assert.That(pizzaVisuals.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator CustomerBot_OrderText_ShowsInitialCount()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var bot = CreateCustomerBot(3);
            var canvasObject = new GameObject("OrderUICanvas");
            canvasObject.transform.SetParent(bot.transform);
            canvasObject.AddComponent<Canvas>();
            var textObject = new GameObject("OrderText");
            textObject.transform.SetParent(canvasObject.transform);
            var tmpText = textObject.AddComponent<TextMeshProUGUI>();
            SetPrivateField(bot, "orderText", tmpText);
            SetPrivateField(bot, "orderTextFormat", "{0}");

            bot.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);

            Assert.That(tmpText.text, Is.EqualTo("3"));
            Assert.That(textObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomerBot_OrderText_UpdatesAfterServingOne()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 1);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var bot = CreateCustomerBot(3);
            var canvasObject = new GameObject("OrderUICanvas");
            canvasObject.transform.SetParent(bot.transform);
            canvasObject.AddComponent<Canvas>();
            var textObject = new GameObject("OrderText");
            textObject.transform.SetParent(canvasObject.transform);
            var tmpText = textObject.AddComponent<TextMeshProUGUI>();
            SetPrivateField(bot, "orderText", tmpText);
            SetPrivateField(bot, "orderTextFormat", "{0}");

            bot.Initialize(serveStation, new CustomerOrderModel(3), new Transform[0]);
            serveStation.TryRegisterCustomer(bot);
            SetPrivateField(bot, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();

            Assert.That(tmpText.text, Is.EqualTo("2"));
            Assert.That(textObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomerBot_OrderText_DisablesOnCompleteOrder()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 1);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var bot = CreateCustomerBot(1);
            var canvasObject = new GameObject("OrderUICanvas");
            canvasObject.transform.SetParent(bot.transform);
            canvasObject.AddComponent<Canvas>();
            var textObject = new GameObject("OrderText");
            textObject.transform.SetParent(canvasObject.transform);
            var tmpText = textObject.AddComponent<TextMeshProUGUI>();
            SetPrivateField(bot, "orderText", tmpText);
            SetPrivateField(bot, "orderTextFormat", "{0}");

            bot.Initialize(serveStation, new CustomerOrderModel(1), new Transform[0]);
            serveStation.TryRegisterCustomer(bot);
            SetPrivateField(bot, "_hasReachedAssignedSlot", true);

            serveStation.DepositFrom(_playerInventory);
            serveStation.ServeFrontCustomer();

            Assert.That(textObject.activeSelf, Is.False);
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

        private void CreateQueueFixture(
            int maxQueueCustomers,
            int pricePerPizza,
            int playerPizzaCount,
            int startMoney = 100)
        {
            _serveSettings = ScriptableObject.CreateInstance<SServeStation>();
            _serveSettings.maxQueueCustomers = maxQueueCustomers;
            _serveSettings.maxPizzasPerOrder = 5;
            _serveSettings.minPizzasPerOrder = 1;
            _serveSettings.pricePerPizza = pricePerPizza;
            _serveSettings.maxStoredPizzas = 10;

            _pizzaServedEvent = ScriptableObject.CreateInstance<SVoidEventChannel>();

            EnsureNavMeshExists();

            _serveStationObject = new GameObject("ServeStationTest");
            _serveStationObject.SetActive(false);
            var serveStation = _serveStationObject.AddComponent<ServeStation>();
            SetPrivateField(serveStation, "sServeStation", _serveSettings);
            SetPrivateField(serveStation, "pizzaServedEvent", _pizzaServedEvent);

            _queueController = _serveStationObject.AddComponent<CustomerQueueController>();
            _stationVisuals = _serveStationObject.AddComponent<ServeStationVisuals>();

            SetPrivateField(serveStation, "queueController", _queueController);
            SetPrivateField(serveStation, "stationVisuals", _stationVisuals);

            var queueSlots = new Transform[10];
            for (var i = 0; i < 10; i++)
            {
                var slotObject = new GameObject($"QueueSlot_{i}");
                slotObject.transform.SetParent(_serveStationObject.transform);
                slotObject.transform.localPosition = new Vector3(0f, 0f, i * -1.5f);
                queueSlots[i] = slotObject.transform;
            }
            SetPrivateField(_queueController, "queueSlots", queueSlots);

            var plateObject = new GameObject("ServePlateTest");
            plateObject.transform.SetParent(_serveStationObject.transform);
            plateObject.transform.localPosition = Vector3.zero;
            var plateCollider = plateObject.AddComponent<BoxCollider>();
            plateCollider.center = new Vector3(0f, 0.25f, 0f);
            plateCollider.size = new Vector3(2f, 0.5f, 2f);
            SetPrivateField(_stationVisuals, "plateCollider", plateCollider);

            _tableManagerObject = new GameObject("TableManager");
            _tableManager = _tableManagerObject.AddComponent<TableManager>();
            var tableObject = new GameObject("Table");
            tableObject.transform.SetParent(_tableManagerObject.transform);
            var tableComponent = tableObject.AddComponent<Table>();
            var seatA = new GameObject("SeatA");
            seatA.transform.SetParent(tableObject.transform);
            var seatB = new GameObject("SeatB");
            seatB.transform.SetParent(tableObject.transform);
            var seatC = new GameObject("SeatC");
            seatC.transform.SetParent(tableObject.transform);
            var seatD = new GameObject("SeatD");
            seatD.transform.SetParent(tableObject.transform);
            SetPrivateField(tableComponent, "seatTransforms",
                new[] { seatA.transform, seatB.transform, seatC.transform, seatD.transform });
            SetPrivateField(_tableManager, "tables", new[] { tableComponent });

            var exitPointObject = new GameObject("ExitPoint");
            _exitPoint = exitPointObject.transform;

            SetPrivateField(serveStation, "tableManager", _tableManager);

            _serveStationObject.SetActive(true);

            _playerObject = new GameObject("ServePlayerTest");
            _playerObject.SetActive(false);
            _playerObject.tag = "Player";
            var wallet = _playerObject.AddComponent<PlayerWallet>();
            SetPrivateField(wallet, "money", startMoney);
            _playerInventory = _playerObject.AddComponent<PlayerPizzaInventory>();
            SetPrivateField(_playerInventory, "capacity", 10);
            _playerObject.SetActive(true);

            _playerInventory.TryAdd(playerPizzaCount);

            _economyManagerObject = new GameObject("ServeEconomyManagerTest");
            _economyManagerObject.AddComponent<EconomyManager>();

            _currencyServiceObject = new GameObject("ServeCurrencyServiceTest");
            _currencyService = _currencyServiceObject.AddComponent<CurrencyService>();
            _currencyService.Initialize(wallet);

            serveStation.Initialize(_currencyService);
        }

        private CustomerBot CreateCustomerBot(int orderAmount)
        {
            var botObject = new GameObject("CustomerBotTest");
            botObject.transform.position = Vector3.zero;
            var agent = botObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            var bot = botObject.AddComponent<CustomerBot>();
            _botsToCleanup.Add(botObject);
            return bot;
        }

        private CustomerBot CreateAndRegisterBot(ServeStation station, int orderAmount)
        {
            var bot = CreateCustomerBot(orderAmount);
            var orderModel = new CustomerOrderModel(orderAmount);
            var waypoints = new Transform[0];
            bot.Initialize(station, orderModel, waypoints);
            if (_tableManager != null && _exitPoint != null)
                bot.SetupDining(_tableManager, _exitPoint, 5f);
            station.TryRegisterCustomer(bot);
            return bot;
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
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected {target.GetType().Name} to define '{methodName}'.");
            method.Invoke(target, arguments);
        }

        private static int CountActive(List<GameObject> visuals)
        {
            var count = 0;
            foreach (var visual in visuals)
                if (visual.activeSelf) count++;
            return count;
        }
    }
}
