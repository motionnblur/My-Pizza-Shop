using System.Reflection;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEngine;

namespace Engineering.Tests
{
    public class PlayerWalletTests
    {
        private GameObject _gameObject;
        private PlayerWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("PlayerWalletTest");
            _wallet = _gameObject.AddComponent<PlayerWallet>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void NewWallet_StartsWithConfiguredDefaultMoney()
        {
            Assert.That(_wallet.Money, Is.EqualTo(100));
        }

        [Test]
        public void Money_CanBeUpdatedForGameplayTransactions()
        {
            _wallet.Money = 35;

            Assert.That(_wallet.Money, Is.EqualTo(35));
        }
    }

    public class PlayerTriggerTests
    {
        private GameObject _playerObject;
        private GameObject _otherObject;
        private PlayerTrigger _playerTrigger;
        private Collider _otherCollider;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("PlayerTriggerTest");
            _playerTrigger = _playerObject.AddComponent<PlayerTrigger>();
            _otherObject = new GameObject("OtherCollider");
            _otherCollider = _otherObject.AddComponent<BoxCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_otherObject);
        }

        [Test]
        public void HandleTriggerEnter_NotifiesSubscribersWithTheEnteringCollider()
        {
            Collider receivedCollider = null;
            _playerTrigger.TriggerEnterEvent += collider => receivedCollider = collider;

            _playerTrigger.HandleTriggerEnter(_otherCollider);

            Assert.That(receivedCollider, Is.SameAs(_otherCollider));
        }

        [Test]
        public void HandleTriggerExit_NotifiesSubscribersWithTheExitingCollider()
        {
            Collider receivedCollider = null;
            _playerTrigger.TriggerExitEvent += collider => receivedCollider = collider;

            _playerTrigger.HandleTriggerExit(_otherCollider);

            Assert.That(receivedCollider, Is.SameAs(_otherCollider));
        }
    }

    public class PlayerMovementTests
    {
        private GameObject _playerObject;
        private GameObject _cameraObject;
        private GameObject _inputManagerObject;
        private PlayerMovement _movement;
        private Rigidbody _rigidbody;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("PlayerMovementTest");
            _playerObject.SetActive(false);
            _rigidbody = _playerObject.AddComponent<Rigidbody>();
            _movement = _playerObject.AddComponent<PlayerMovement>();

            _cameraObject = new GameObject("CameraTransform");
            _inputManagerObject = new GameObject("InputManagerTest");
            _inputManagerObject.SetActive(false);
            var inputManager = _inputManagerObject.AddComponent<InputManager>();
            SetPrivateField(_movement, "inputManager", inputManager);
            SetPrivateField(_movement, "_rb", _rigidbody);
            SetPrivateField(_movement, "cameraTransform", _cameraObject.transform);
            _playerObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_cameraObject);
            Object.DestroyImmediate(_inputManagerObject);
        }

        [Test]
        public void FixedUpdate_MovesForwardAtBaseSpeedAndPreservesVerticalVelocity()
        {
            _rigidbody.linearVelocity = new Vector3(0f, 2f, 0f);
            InvokePrivateMethod(_movement, "OnMoveChanged", Vector2.up);

            InvokePrivateMethod(_movement, "FixedUpdate");

            Assert.That(_rigidbody.linearVelocity.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(_rigidbody.linearVelocity.y, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void FixedUpdate_WhenSprinting_MultipliesHorizontalSpeed()
        {
            InvokePrivateMethod(_movement, "OnMoveChanged", Vector2.up);
            InvokePrivateMethod(_movement, "OnSprintStarted");

            InvokePrivateMethod(_movement, "FixedUpdate");

            Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(7.5f).Within(0.0001f));
        }

        [Test]
        public void FixedUpdate_WithoutMovementInput_StopsHorizontalMovementAndPreservesVerticalVelocity()
        {
            _rigidbody.linearVelocity = new Vector3(4f, 2f, 3f);

            InvokePrivateMethod(_movement, "FixedUpdate");

            Assert.That(_rigidbody.linearVelocity.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(_rigidbody.linearVelocity.y, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(_rigidbody.linearVelocity.z, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(_rigidbody.angularVelocity.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected {target.GetType().Name} to define '{methodName}'.");
            method.Invoke(target, arguments);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}