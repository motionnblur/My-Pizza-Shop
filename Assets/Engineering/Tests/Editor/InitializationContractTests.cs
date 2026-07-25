using System;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.Scripts.Mono.Items;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Engineering.Tests
{
    public class CurrencyServiceInitializationTests
    {
        private GameObject _gameObject;
        private CurrencyService _service;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("CurrencyServiceInitTest");
            _service = _gameObject.AddComponent<CurrencyService>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_NullWallet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var wallet = new GameObject().AddComponent<PlayerWallet>();
            Assert.DoesNotThrow(() => _service.Initialize(wallet));
            Assert.That(_service.Wallet, Is.SameAs(wallet));
            Object.DestroyImmediate(wallet.gameObject);
        }

        [Test]
        public void Initialize_SameWalletTwice_DoesNotThrow()
        {
            var wallet = new GameObject().AddComponent<PlayerWallet>();
            _service.Initialize(wallet);
            Assert.DoesNotThrow(() => _service.Initialize(wallet));
            Object.DestroyImmediate(wallet.gameObject);
        }

        [Test]
        public void Initialize_DifferentWallet_ThrowsInvalidOperationException()
        {
            var wallet1 = new GameObject().AddComponent<PlayerWallet>();
            var wallet2 = new GameObject().AddComponent<PlayerWallet>();
            _service.Initialize(wallet1);
            Assert.Throws<InvalidOperationException>(() => _service.Initialize(wallet2));
            Object.DestroyImmediate(wallet1.gameObject);
            Object.DestroyImmediate(wallet2.gameObject);
        }
    }

    public class EconomyManagerInitializationTests
    {
        private GameObject _gameObject;
        private EconomyManager _manager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("EconomyManagerInitTest");
            _manager = _gameObject.AddComponent<EconomyManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_NullCurrencyService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var cs = new GameObject().AddComponent<CurrencyService>();
            Assert.DoesNotThrow(() => _manager.Initialize(cs));
            Object.DestroyImmediate(cs.gameObject);
        }

        [Test]
        public void Initialize_SameInstanceTwice_DoesNotThrow()
        {
            var cs = new GameObject().AddComponent<CurrencyService>();
            _manager.Initialize(cs);
            Assert.DoesNotThrow(() => _manager.Initialize(cs));
            Object.DestroyImmediate(cs.gameObject);
        }

        [Test]
        public void Initialize_DifferentInstance_ThrowsInvalidOperationException()
        {
            var cs1 = new GameObject().AddComponent<CurrencyService>();
            var cs2 = new GameObject().AddComponent<CurrencyService>();
            _manager.Initialize(cs1);
            Assert.Throws<InvalidOperationException>(() => _manager.Initialize(cs2));
            Object.DestroyImmediate(cs1.gameObject);
            Object.DestroyImmediate(cs2.gameObject);
        }
    }

    public class PlayerMovementInitializationTests
    {
        private GameObject _playerObject;
        private PlayerMovement _movement;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("PlayerMovementInitTest");
            _playerObject.SetActive(false);
            _playerObject.AddComponent<Rigidbody>();
            _movement = _playerObject.AddComponent<PlayerMovement>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
        }

        [Test]
        public void Initialize_NullInputManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _movement.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var im = new GameObject().AddComponent<InputManager>();
            Assert.DoesNotThrow(() => _movement.Initialize(im));
            Object.DestroyImmediate(im.gameObject);
        }

        [Test]
        public void Initialize_SameInstanceTwice_DoesNotThrow()
        {
            var im = new GameObject().AddComponent<InputManager>();
            _movement.Initialize(im);
            Assert.DoesNotThrow(() => _movement.Initialize(im));
            Object.DestroyImmediate(im.gameObject);
        }

        [Test]
        public void Initialize_DifferentInstance_ThrowsInvalidOperationException()
        {
            var im1 = new GameObject().AddComponent<InputManager>();
            var im2 = new GameObject().AddComponent<InputManager>();
            _movement.Initialize(im1);
            Assert.Throws<InvalidOperationException>(() => _movement.Initialize(im2));
            Object.DestroyImmediate(im1.gameObject);
            Object.DestroyImmediate(im2.gameObject);
        }

        [Test]
        public void Initialize_WhileActiveAndEnabled_SubscribesImmediately()
        {
            var im = new GameObject().AddComponent<InputManager>();
            _playerObject.SetActive(true);
            _movement.Initialize(im);
            var isSubscribed = GetPrivateField<bool>(_movement, "_isInputSubscribed");
            Assert.That(isSubscribed, Is.True,
                "Initialize while active should subscribe to input immediately.");
            Object.DestroyImmediate(im.gameObject);
        }

        [Test]
        public void RepeatedInitialize_DoesNotCreateDuplicateSubscription()
        {
            var im = new GameObject().AddComponent<InputManager>();
            _playerObject.SetActive(true);
            _movement.Initialize(im);
            _movement.Initialize(im);
            var isSubscribed = GetPrivateField<bool>(_movement, "_isInputSubscribed");
            Assert.That(isSubscribed, Is.True);
            Object.DestroyImmediate(im.gameObject);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            return (T)field.GetValue(target);
        }
    }

    public class UIManagerInitializationTests
    {
        private GameObject _gameObject;
        private UIManager _uiManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("UIManagerInitTest");
            _gameObject.SetActive(false);
            _uiManager = _gameObject.AddComponent<UIManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_NullWallet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _uiManager.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var wallet = new GameObject().AddComponent<PlayerWallet>();
            Assert.DoesNotThrow(() => _uiManager.Initialize(wallet));
            Object.DestroyImmediate(wallet.gameObject);
        }

        [Test]
        public void Initialize_SameInstanceTwice_DoesNotThrow()
        {
            var wallet = new GameObject().AddComponent<PlayerWallet>();
            _uiManager.Initialize(wallet);
            Assert.DoesNotThrow(() => _uiManager.Initialize(wallet));
            Object.DestroyImmediate(wallet.gameObject);
        }

        [Test]
        public void Initialize_DifferentInstance_ThrowsInvalidOperationException()
        {
            var wallet1 = new GameObject().AddComponent<PlayerWallet>();
            var wallet2 = new GameObject().AddComponent<PlayerWallet>();
            _uiManager.Initialize(wallet1);
            Assert.Throws<InvalidOperationException>(() => _uiManager.Initialize(wallet2));
            Object.DestroyImmediate(wallet1.gameObject);
            Object.DestroyImmediate(wallet2.gameObject);
        }

        [Test]
        public void Initialize_WhileActiveAndEnabled_SubscribesImmediately()
        {
            var wallet = new GameObject().AddComponent<PlayerWallet>();
            _gameObject.SetActive(true);
            _uiManager.Initialize(wallet);
            var isSubscribed = GetPrivateField<bool>(_uiManager, "_isWalletSubscribed");
            Assert.That(isSubscribed, Is.True,
                "Initialize while active should subscribe to wallet events immediately.");
            Object.DestroyImmediate(wallet.gameObject);
        }

        [Test]
        public void RepeatedInitialize_DoesNotCreateDuplicateSubscription()
        {
            var wallet = new GameObject().AddComponent<PlayerWallet>();
            _gameObject.SetActive(true);
            _uiManager.Initialize(wallet);
            _uiManager.Initialize(wallet);
            var isSubscribed = GetPrivateField<bool>(_uiManager, "_isWalletSubscribed");
            Assert.That(isSubscribed, Is.True);
            Object.DestroyImmediate(wallet.gameObject);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            return (T)field.GetValue(target);
        }
    }

    public class BuyingAreaInitializationTests
    {
        private GameObject _gameObject;
        private BuyingArea _area;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("BuyingAreaInitTest");
            _area = _gameObject.AddComponent<BuyingArea>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_NullEconomyManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _area.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var em = new GameObject().AddComponent<EconomyManager>();
            Assert.DoesNotThrow(() => _area.Initialize(em));
            Object.DestroyImmediate(em.gameObject);
        }

        [Test]
        public void Initialize_SameInstanceTwice_DoesNotThrow()
        {
            var em = new GameObject().AddComponent<EconomyManager>();
            _area.Initialize(em);
            Assert.DoesNotThrow(() => _area.Initialize(em));
            Object.DestroyImmediate(em.gameObject);
        }

        [Test]
        public void Initialize_DifferentInstance_ThrowsInvalidOperationException()
        {
            var em1 = new GameObject().AddComponent<EconomyManager>();
            var em2 = new GameObject().AddComponent<EconomyManager>();
            _area.Initialize(em1);
            Assert.Throws<InvalidOperationException>(() => _area.Initialize(em2));
            Object.DestroyImmediate(em1.gameObject);
            Object.DestroyImmediate(em2.gameObject);
        }
    }

    public class MoneyToCollectInitializationTests
    {
        private GameObject _gameObject;
        private MoneyToCollect _pickup;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("MoneyToCollectInitTest");
            _pickup = _gameObject.AddComponent<MoneyToCollect>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_NullCurrencyService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _pickup.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var cs = new GameObject().AddComponent<CurrencyService>();
            Assert.DoesNotThrow(() => _pickup.Initialize(cs));
            Object.DestroyImmediate(cs.gameObject);
        }

        [Test]
        public void Initialize_SameInstanceTwice_DoesNotThrow()
        {
            var cs = new GameObject().AddComponent<CurrencyService>();
            _pickup.Initialize(cs);
            Assert.DoesNotThrow(() => _pickup.Initialize(cs));
            Object.DestroyImmediate(cs.gameObject);
        }

        [Test]
        public void Initialize_DifferentInstance_ThrowsInvalidOperationException()
        {
            var cs1 = new GameObject().AddComponent<CurrencyService>();
            var cs2 = new GameObject().AddComponent<CurrencyService>();
            _pickup.Initialize(cs1);
            Assert.Throws<InvalidOperationException>(() => _pickup.Initialize(cs2));
            Object.DestroyImmediate(cs1.gameObject);
            Object.DestroyImmediate(cs2.gameObject);
        }
    }

    public class ServeStationInitializationTests
    {
        private GameObject _gameObject;
        private ServeStation _station;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("ServeStationInitTest");
            _station = _gameObject.AddComponent<ServeStation>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Initialize_NullCurrencyService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _station.Initialize(null));
        }

        [Test]
        public void Initialize_FirstCall_Succeeds()
        {
            var cs = new GameObject().AddComponent<CurrencyService>();
            Assert.DoesNotThrow(() => _station.Initialize(cs));
            Object.DestroyImmediate(cs.gameObject);
        }

        [Test]
        public void Initialize_SameInstanceTwice_DoesNotThrow()
        {
            var cs = new GameObject().AddComponent<CurrencyService>();
            _station.Initialize(cs);
            Assert.DoesNotThrow(() => _station.Initialize(cs));
            Object.DestroyImmediate(cs.gameObject);
        }

        [Test]
        public void Initialize_DifferentInstance_ThrowsInvalidOperationException()
        {
            var cs1 = new GameObject().AddComponent<CurrencyService>();
            var cs2 = new GameObject().AddComponent<CurrencyService>();
            _station.Initialize(cs1);
            Assert.Throws<InvalidOperationException>(() => _station.Initialize(cs2));
            Object.DestroyImmediate(cs1.gameObject);
            Object.DestroyImmediate(cs2.gameObject);
        }
    }
}
