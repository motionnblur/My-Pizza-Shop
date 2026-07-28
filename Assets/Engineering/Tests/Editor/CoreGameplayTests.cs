using System.Reflection;
using Engineering.Scripts.Mono.Items;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using Engineering.ScriptableObjects;
using NUnit.Framework;
using UnityEditor;
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
            SetPrivateField(_wallet, "money", 35);

            Assert.That(_wallet.Money, Is.EqualTo(35));
        }

        [Test]
        public void MoneyAnimationOriginPosition_ReturnsAssignedAnchorWorldPosition()
        {
            var anchor = new GameObject("MoneyAnimationOrigin");
            anchor.transform.position = new Vector3(1f, 2f, 3f);
            SetPrivateField(_wallet, "moneyAnimationOrigin", anchor.transform);

            Assert.That(_wallet.MoneyAnimationOriginPosition, Is.EqualTo(new Vector3(1f, 2f, 3f)));

            Object.DestroyImmediate(anchor);
        }

        [Test]
        public void MoneyAnimationOriginPosition_FallsBackToTransformPositionWhenNoAnchorAssigned()
        {
            _gameObject.transform.position = new Vector3(5f, 0f, 5f);

            Assert.That(_wallet.MoneyAnimationOriginPosition, Is.EqualTo(new Vector3(5f, 0f, 5f)));
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
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

    public class PlayerTriggerRelayTests
    {
        private GameObject _relayObject;
        private PlayerTriggerRelay _relay;

        [SetUp]
        public void SetUp()
        {
            _relayObject = new GameObject("PlayerTriggerRelayTest");
            _relayObject.AddComponent<BoxCollider>();
            _relay = _relayObject.AddComponent<PlayerTriggerRelay>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_relayObject);
        }

        [Test]
        public void OnTriggerEnter_SafeWhenPlayerTriggerNull()
        {
            var otherObject = new GameObject("OtherCollider");
            var otherCollider = otherObject.AddComponent<BoxCollider>();

            Assert.DoesNotThrow(() =>
            {
                var method = typeof(PlayerTriggerRelay).GetMethod("OnTriggerEnter",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method.Invoke(_relay, new object[] { otherCollider });
            });

            Object.DestroyImmediate(otherObject);
        }

        [Test]
        public void OnTriggerExit_SafeWhenPlayerTriggerNull()
        {
            var otherObject = new GameObject("OtherCollider");
            var otherCollider = otherObject.AddComponent<BoxCollider>();

            Assert.DoesNotThrow(() =>
            {
                var method = typeof(PlayerTriggerRelay).GetMethod("OnTriggerExit",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method.Invoke(_relay, new object[] { otherCollider });
            });

            Object.DestroyImmediate(otherObject);
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
            _movement.Initialize(inputManager);
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

    public class MoneyToCollectPrefabTests
    {
        [Test]
        public void MoneyToCollectPrefab_IsConfiguredAsATriggerWithTheDefaultCollectionValue()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Engineering/Prefabs/MoneyToCollect.prefab");

            Assert.That(prefab, Is.Not.Null);

            var moneyToCollect = prefab.GetComponent<MoneyToCollect>();
            var trigger = prefab.GetComponent<BoxCollider>();

            Assert.That(moneyToCollect, Is.Not.Null);
            Assert.That(trigger, Is.Not.Null);
            Assert.That(trigger.isTrigger, Is.True);

            var groundMoneyCollectedEvent = AssetDatabase.LoadAssetAtPath<SVoidEventChannel>(
                "Assets/Engineering/ScriptableObjects/Events/GroundMoneyCollected.asset");
            var moneyAnimationRequested = AssetDatabase.LoadAssetAtPath<SMoneyAnimationEventChannel>(
                "Assets/Engineering/ScriptableObjects/Events/MoneyAnimationRequested.asset");

            Assert.That(groundMoneyCollectedEvent, Is.Not.Null,
                "Expected GroundMoneyCollected.asset to exist.");
            Assert.That(moneyAnimationRequested, Is.Not.Null,
                "Expected MoneyAnimationRequested.asset to exist.");

            var serializedMoney = new SerializedObject(moneyToCollect)
                .FindProperty("moneyToCollect");
            var serializedGroundMoneyCollectedEvent = new SerializedObject(moneyToCollect)
                .FindProperty("groundMoneyCollectedEvent");
            var serializedMoneyAnimationRequested = new SerializedObject(moneyToCollect)
                .FindProperty("moneyAnimationRequested");

            Assert.That(serializedMoney, Is.Not.Null);
            Assert.That(serializedMoney.intValue, Is.EqualTo(5));
            Assert.That(serializedGroundMoneyCollectedEvent, Is.Not.Null);
            Assert.That(serializedGroundMoneyCollectedEvent.objectReferenceValue,
                Is.SameAs(groundMoneyCollectedEvent),
                "MoneyToCollect.prefab must reference GroundMoneyCollected.asset.");
            Assert.That(serializedMoneyAnimationRequested, Is.Not.Null);
            Assert.That(serializedMoneyAnimationRequested.objectReferenceValue,
                Is.SameAs(moneyAnimationRequested),
                "MoneyToCollect.prefab must reference MoneyAnimationRequested.asset.");
        }
    }

    public class CameraManagerTests
    {
        private GameObject _cameraObject;
        private GameObject _targetObject;
        private CameraManager _cameraManager;
        private SCameraSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("CameraManagerTest");
            _cameraObject.SetActive(false);
            _cameraObject.transform.position = new Vector3(10f, 9f, 8f);
            _cameraObject.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            _cameraManager = _cameraObject.AddComponent<CameraManager>();

            _targetObject = new GameObject("CameraTargetTest");
            _targetObject.transform.position = new Vector3(1f, 2f, 3f);

            _settings = ScriptableObject.CreateInstance<SCameraSettings>();

            SetPrivateField(_cameraManager, "target", _targetObject.transform);
            SetPrivateField(_cameraManager, "cameraTransform", _cameraObject.transform);
            SetPrivateField(_cameraManager, "settings", _settings);

            _cameraObject.SetActive(true);
            InvokePrivateMethod(_cameraManager, "Awake");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cameraObject);
            Object.DestroyImmediate(_targetObject);
            if (_settings != null)
                Object.DestroyImmediate(_settings);
        }

        [Test]
        public void LateUpdate_FollowsTargetWhilePreservingInitialOffsetAndRotation()
        {
            _targetObject.transform.position = new Vector3(4f, 5f, 6f);

            Vector3 snapPosition = _targetObject.transform.position +
                GetPrivateField<Vector3>(_cameraManager, "_positionOffset");

            InvokePrivateMethod(_cameraManager, "LateUpdate");

            Assert.That(_cameraObject.transform.position, Is.Not.EqualTo(snapPosition),
                "Damping must prevent instant teleport to the target position.");
            Assert.That(Quaternion.Angle(
                _cameraObject.transform.rotation,
                Quaternion.Euler(45f, 45f, 0f)), Is.LessThan(0.001f),
                "Initial rotation must be preserved across LateUpdate.");
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        [Test]
        public void Awake_RequiresTargetReference()
        {
            var missingTargetCamera = new GameObject("MissingTargetCamera");
            missingTargetCamera.SetActive(false);
            var manager = missingTargetCamera.AddComponent<CameraManager>();
            SetPrivateField(manager, "cameraTransform", missingTargetCamera.transform);

            var exception = Assert.Throws<TargetInvocationException>(() =>
                InvokePrivateMethod(manager, "Awake"));

            Assert.That(exception.InnerException.Message, Does.Contain("target Transform reference"));
            Object.DestroyImmediate(missingTargetCamera);
        }

        [Test]
        public void Awake_RequiresSettingsReference()
        {
            var missingSettingsCamera = new GameObject("MissingSettingsCamera");
            missingSettingsCamera.SetActive(false);
            var manager = missingSettingsCamera.AddComponent<CameraManager>();
            SetPrivateField(manager, "target", missingSettingsCamera.transform);
            SetPrivateField(manager, "cameraTransform", missingSettingsCamera.transform);

            var exception = Assert.Throws<TargetInvocationException>(() =>
                InvokePrivateMethod(manager, "Awake"));

            Assert.That(exception.InnerException.Message, Does.Contain("requires a"));
            Object.DestroyImmediate(missingSettingsCamera);
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
