using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Engineering.Scripts.Mono.Actors.CustomerQueue;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
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
        private SServeStation _serveSettings;
        private SVoidEventChannel _pizzaServedEvent;
        private PlayerPizzaInventory _playerInventory;
        private readonly List<GameObject> _botsToCleanup = new List<GameObject>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var bot in _botsToCleanup)
            {
                if (bot != null)
                    Object.Destroy(bot);
            }
            _botsToCleanup.Clear();

            if (_serveStationObject != null)
                Object.Destroy(_serveStationObject);

            if (_playerObject != null)
                Object.Destroy(_playerObject);

            if (_economyManagerObject != null)
                Object.Destroy(_economyManagerObject);

            if (_currencyServiceObject != null)
                Object.Destroy(_currencyServiceObject);

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

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            SetPrivateField(serveStation, "queueSlots", null);

            var customer = CreateCustomerBot(3);
            var registered = serveStation.RegisterCustomer(customer);

            Assert.That(registered, Is.False);
            Assert.That(serveStation.CustomerCount, Is.EqualTo(0));
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
            SetPrivateField(serveStation, "queueSlots", slots);

            var customer1 = CreateAndRegisterBot(serveStation, 3);

            var customer2 = CreateCustomerBot(3);
            customer2.Initialize(serveStation, 3, new Transform[0]);
            var registered = serveStation.RegisterCustomer(customer2);

            Assert.That(registered, Is.False);
            Assert.That(serveStation.CustomerCount, Is.EqualTo(1));
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

            var deposited = serveStation.TryDepositPizzas(_playerInventory);

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

            var deposited = serveStation.TryDepositPizzas(_playerInventory);
            Assert.That(deposited, Is.EqualTo(10));
            Assert.That(serveStation.StoredPizzaCount, Is.EqualTo(10));
            Assert.That(_playerInventory.Count, Is.EqualTo(0));

            _playerInventory.TryAdd(5);
            var secondDeposit = serveStation.TryDepositPizzas(_playerInventory);
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
            SetPrivateField(serveStation, "pizzaVisualPrefab", visualPrefab);

            var pizzaVisualsField = typeof(ServeStation).GetField("_pizzaVisuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(serveStation);
            pizzaVisuals.Clear();

            InvokePrivateMethod(serveStation, "CreateVisualPool");
            InvokePrivateMethod(serveStation, "RefreshVisuals");

            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(serveStation);
            Assert.That(pizzaVisuals.Count, Is.EqualTo(10));

            var activeCount = CountActive(pizzaVisuals);
            Assert.That(activeCount, Is.EqualTo(0));

            serveStation.TryDepositPizzas(_playerInventory);
            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(serveStation);
            Assert.That(CountActive(pizzaVisuals), Is.EqualTo(3));

            _playerInventory.TryAdd(5);
            serveStation.TryDepositPizzas(_playerInventory);
            pizzaVisuals = (List<GameObject>)pizzaVisualsField.GetValue(serveStation);
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
            serveStation.TryDepositPizzas(_playerInventory);

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

            serveStation.TryDepositPizzas(_playerInventory);
            var result = serveStation.TryServeFrontCustomer();

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

            serveStation.TryDepositPizzas(_playerInventory);
            var result = serveStation.TryServeFrontCustomer();

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

            serveStation.TryDepositPizzas(_playerInventory);
            var result = serveStation.TryServeFrontCustomer();

            Assert.That(result, Is.EqualTo(3));
            Assert.That(serveStation.CustomerCount, Is.EqualTo(1));
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

            serveStation.TryDepositPizzas(_playerInventory);
            serveStation.TryServeFrontCustomer();
            yield return null;

            Assert.That(serveStation.CustomerCount, Is.EqualTo(1));
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

            serveStation.TryDepositPizzas(_playerInventory);
            var result = serveStation.TryServeFrontCustomer();

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

            var result = serveStation.TryServeFrontCustomer();

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

            var result = serveStation.TryServeFrontCustomer();
            Assert.That(result, Is.EqualTo(0));
            Assert.That(_playerInventory.Count, Is.EqualTo(3));

            serveStation.TryDepositPizzas(_playerInventory);
            result = serveStation.TryServeFrontCustomer();
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
                var registered = serveStation.RegisterCustomer(customer);
                Assert.That(registered, Is.True, $"Customer {i} should register.");
            }

            var extraCustomer = CreateCustomerBot(3);
            var rejected = serveStation.RegisterCustomer(extraCustomer);

            Assert.That(rejected, Is.False);
            Assert.That(serveStation.CustomerCount, Is.EqualTo(3));
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
                Assert.That(serveStation.RegisterCustomer(customer), Is.True, $"Customer {i} should register.");
            }

            Assert.That(serveStation.CustomerCount, Is.EqualTo(10));

            var extraCustomer = CreateCustomerBot(3);
            Assert.That(serveStation.RegisterCustomer(extraCustomer), Is.False);
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
            Assert.That(serveStation.RegisterCustomer(rejectedCustomer), Is.False);

            serveStation.TryDepositPizzas(_playerInventory);
            serveStation.TryServeFrontCustomer();
            yield return null;

            Assert.That(serveStation.CustomerCount, Is.EqualTo(1));

            var newCustomer = CreateAndRegisterBot(serveStation, orderAmount: 4);
            Assert.That(serveStation.CustomerCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_AwardsMoneyForDeliveredPizzas()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 3, startMoney: 50);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var customer = CreateAndRegisterBot(serveStation, orderAmount: 5);

            SetPrivateField(customer, "_hasReachedAssignedSlot", true);

            serveStation.TryDepositPizzas(_playerInventory);
            serveStation.TryServeFrontCustomer();
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

            serveStation.TryDepositPizzas(_playerInventory);
            serveStation.TryServeFrontCustomer();

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

            serveStation.TryDepositPizzas(_playerInventory);
            serveStation.TryServeFrontCustomer();

            Assert.That(invocationCount, Is.EqualTo(1));
            Assert.That(_playerObject.GetComponent<PlayerWallet>().Money, Is.EqualTo(70));
            Assert.That(serveStation.CustomerCount, Is.EqualTo(1));
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

            serveStation.TryDepositPizzas(_playerInventory);

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

            serveStation.TryDepositPizzas(_playerInventory);
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

            var customers = GetPrivateField<List<CustomerBot>>(serveStation, "_customers");
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
            var slots = GetPrivateField<Transform[]>(serveStation, "queueSlots");

            var customer1 = CreateAndRegisterBot(serveStation, 3);
            var assigned1 = GetPrivateField<Transform>(customer1, "_assignedSlot");
            Assert.That(assigned1, Is.SameAs(slots[0]));

            var customer2 = CreateAndRegisterBot(serveStation, 3);
            var assigned2 = GetPrivateField<Transform>(customer2, "_assignedSlot");
            Assert.That(assigned2, Is.SameAs(slots[1]));

            Assert.That(assigned1, Is.Not.SameAs(assigned2));
            Assert.That(serveStation.CustomerCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator CustomerBot_StopsWhenReachingAssignedSlot()
        {
            CreateQueueFixture(maxQueueCustomers: 2, pricePerPizza: 10, playerPizzaCount: 0);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var bot = CreateCustomerBot(3);

            SetPrivateField(bot, "_hasReachedAssignedSlot", true);

            Assert.That(bot.HasReachedAssignedSlot, Is.True);
        }

        [UnityTest]
        public IEnumerator TryServeFrontCustomer_AdvancesRemainingCustomers()
        {
            CreateQueueFixture(maxQueueCustomers: 3, pricePerPizza: 10, playerPizzaCount: 5);
            yield return null;

            var serveStation = _serveStationObject.GetComponent<ServeStation>();
            var queueSlots = GetPrivateField<Transform[]>(serveStation, "queueSlots");

            var firstCustomer = CreateAndRegisterBot(serveStation, orderAmount: 2);
            var secondCustomer = CreateAndRegisterBot(serveStation, orderAmount: 3);
            _botsToCleanup.Remove(firstCustomer.gameObject);
            _botsToCleanup.Remove(secondCustomer.gameObject);

            SetPrivateField(firstCustomer, "_hasReachedAssignedSlot", true);
            SetPrivateField(secondCustomer, "_hasReachedAssignedSlot", true);

            serveStation.TryDepositPizzas(_playerInventory);
            serveStation.TryServeFrontCustomer();
            yield return null;

            Assert.That(serveStation.CustomerCount, Is.EqualTo(1));

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

            _serveStationObject = new GameObject("ServeStationTest");
            _serveStationObject.SetActive(false);
            var serveStation = _serveStationObject.AddComponent<ServeStation>();
            SetPrivateField(serveStation, "sServeStation", _serveSettings);
            SetPrivateField(serveStation, "pizzaServedEvent", _pizzaServedEvent);

            var queueSlots = new Transform[10];
            for (var i = 0; i < 10; i++)
            {
                var slotObject = new GameObject($"QueueSlot_{i}");
                slotObject.transform.SetParent(_serveStationObject.transform);
                slotObject.transform.localPosition = new Vector3(0f, 0f, i * -1.5f);
                queueSlots[i] = slotObject.transform;
            }
            SetPrivateField(serveStation, "queueSlots", queueSlots);

            var plateObject = new GameObject("ServePlateTest");
            plateObject.transform.SetParent(_serveStationObject.transform);
            plateObject.transform.localPosition = Vector3.zero;
            var plateCollider = plateObject.AddComponent<BoxCollider>();
            plateCollider.center = new Vector3(0f, 0.25f, 0f);
            plateCollider.size = new Vector3(2f, 0.5f, 2f);
            SetPrivateField(serveStation, "plateCollider", plateCollider);

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
            _currencyServiceObject.AddComponent<CurrencyService>();
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
            var waypoints = new Transform[0];
            bot.Initialize(station, orderAmount, waypoints);
            station.RegisterCustomer(bot);
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
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
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
