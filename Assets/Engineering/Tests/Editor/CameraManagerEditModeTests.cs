using System;
using System.Reflection;
using Engineering.ScriptableObjects;
using Engineering.Scripts.Mono.Managers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Engineering.Tests
{
    public class CameraManagerEditModeTests
    {
        private GameObject _managerGo;
        private CameraManager _manager;
        private GameObject _targetGo;
        private GameObject _cameraGo;
        private SCameraSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _managerGo = new GameObject("CameraManager");
            _managerGo.SetActive(false);

            _targetGo = new GameObject("Target");
            _cameraGo = new GameObject("Camera");

            _manager = _managerGo.AddComponent<CameraManager>();

            _settings = ScriptableObject.CreateInstance<SCameraSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_managerGo != null)
                UnityEngine.Object.DestroyImmediate(_managerGo);
            if (_targetGo != null)
                UnityEngine.Object.DestroyImmediate(_targetGo);
            if (_cameraGo != null)
                UnityEngine.Object.DestroyImmediate(_cameraGo);
            if (_settings != null)
                UnityEngine.Object.DestroyImmediate(_settings);
        }

        private void SetReferences(Transform target, Transform cameraTransform)
        {
            var serialized = new SerializedObject(_manager);
            serialized.FindProperty("target").objectReferenceValue = target;
            serialized.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
            serialized.FindProperty("settings").objectReferenceValue = _settings;
            serialized.ApplyModifiedProperties();
        }

        private void SetField(string fieldName, object value)
        {
            var field = typeof(CameraManager).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_manager, value);
        }

        private T GetField<T>(string fieldName)
        {
            var field = typeof(CameraManager).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(_manager);
        }

        private void InvokeAwake()
        {
            var method = typeof(CameraManager).GetMethod("Awake",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_manager, null);
        }

        private void InvokeOnEnable()
        {
            var method = typeof(CameraManager).GetMethod("OnEnable",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_manager, null);
        }

        private void InvokeLateUpdate()
        {
            var method = typeof(CameraManager).GetMethod("LateUpdate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_manager, null);
        }

        [Test]
        public void Awake_WithNullTarget_ThrowsInvalidOperationException()
        {
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(null, _cameraGo.transform);

            var exception = Assert.Throws<TargetInvocationException>(() => InvokeAwake());
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Does.Contain("target Transform reference"));
        }

        [Test]
        public void Awake_WithNullCameraTransform_ThrowsInvalidOperationException()
        {
            _targetGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, null);

            var exception = Assert.Throws<TargetInvocationException>(() => InvokeAwake());
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Does.Contain("camera Transform reference"));
        }

        [Test]
        public void Awake_WithNullSettings_ThrowsInvalidOperationException()
        {
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = Vector3.zero;

            var serialized = new SerializedObject(_manager);
            serialized.FindProperty("target").objectReferenceValue = _targetGo.transform;
            serialized.FindProperty("cameraTransform").objectReferenceValue = _cameraGo.transform;
            serialized.FindProperty("settings").objectReferenceValue = null;
            serialized.ApplyModifiedProperties();

            var exception = Assert.Throws<TargetInvocationException>(() => InvokeAwake());
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Does.Contain("requires a"));
        }

        [Test]
        public void Awake_PositionsCameraAtTargetPlusFollowOffset()
        {
            _targetGo.transform.position = new Vector3(1f, 2f, 3f);
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, _cameraGo.transform);

            InvokeAwake();

            Vector3 expectedPosition = _targetGo.transform.position + _settings.FollowOffset;
            Assert.That(_cameraGo.transform.position, Is.EqualTo(expectedPosition));
        }

        [Test]
        public void Awake_CapturesInitialRotationCorrectly()
        {
            var expectedRotation = Quaternion.Euler(10f, 20f, 30f);
            _cameraGo.transform.rotation = expectedRotation;
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, _cameraGo.transform);

            InvokeAwake();

            Quaternion captured = GetField<Quaternion>("_initialRotation");
            Assert.That(Quaternion.Angle(captured, expectedRotation), Is.LessThan(0.001f));
        }

        [Test]
        public void Awake_ResetsVelocityToZero()
        {
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = new Vector3(5f, 5f, 5f);
            SetReferences(_targetGo.transform, _cameraGo.transform);

            InvokeAwake();

            Assert.That(GetField<Vector3>("_velocity"), Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void OnEnable_ResetsVelocityToZero()
        {
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = new Vector3(5f, 5f, 5f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            SetField("_velocity", new Vector3(100f, 200f, 300f));

            InvokeOnEnable();

            Assert.That(GetField<Vector3>("_velocity"), Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void LateUpdate_WithNullTarget_DoesNotChangeCameraTransform()
        {
            _cameraGo.transform.position = new Vector3(10f, 20f, 30f);
            _cameraGo.transform.rotation = Quaternion.Euler(5f, 10f, 15f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            Vector3 positionAfterAwake = _cameraGo.transform.position;
            Quaternion rotationAfterAwake = _cameraGo.transform.rotation;

            SetField("target", null);

            InvokeLateUpdate();

            Assert.That(_cameraGo.transform.position, Is.EqualTo(positionAfterAwake));
            Assert.That(_cameraGo.transform.rotation, Is.EqualTo(rotationAfterAwake));
        }

        [Test]
        public void LateUpdate_WithNullSettings_DoesNotChangeCameraTransform()
        {
            _cameraGo.transform.position = new Vector3(10f, 20f, 30f);
            _cameraGo.transform.rotation = Quaternion.Euler(5f, 10f, 15f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            Vector3 positionAfterAwake = _cameraGo.transform.position;
            Quaternion rotationAfterAwake = _cameraGo.transform.rotation;

            SetField("settings", null);

            InvokeLateUpdate();

            Assert.That(_cameraGo.transform.position, Is.EqualTo(positionAfterAwake));
            Assert.That(_cameraGo.transform.rotation, Is.EqualTo(rotationAfterAwake));
        }

        [Test]
        public void LateUpdate_PreservesInitialRotation()
        {
            _cameraGo.transform.rotation = Quaternion.Euler(15f, 30f, 45f);
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = new Vector3(0f, 2f, 0f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            _targetGo.transform.position = new Vector3(100f, 100f, 100f);

            InvokeLateUpdate();

            Assert.That(_cameraGo.transform.rotation, Is.EqualTo(Quaternion.Euler(15f, 30f, 45f)));
        }

        [Test]
        public void LateUpdate_WithDampingEnabled_DoesNotTeleportToTarget()
        {
            _targetGo.transform.position = new Vector3(0f, 0f, 0f);
            _cameraGo.transform.position = new Vector3(10f, 10f, 10f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            _targetGo.transform.position = new Vector3(100f, 100f, 100f);
            Vector3 expectedSnapPosition = _targetGo.transform.position + _settings.FollowOffset;

            InvokeLateUpdate();

            Assert.That(_cameraGo.transform.position, Is.Not.EqualTo(expectedSnapPosition));
        }

        [Test]
        public void LateUpdate_ComputeDampedTargetInCorrectDirection()
        {
            _targetGo.transform.position = new Vector3(1f, 2f, 3f);
            _cameraGo.transform.position = new Vector3(10f, 10f, 10f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            _targetGo.transform.position = new Vector3(4f, 5f, 6f);

            InvokeLateUpdate();

            Vector3 expectedTarget = _targetGo.transform.position + _settings.FollowOffset;
            Vector3 directionToTarget = expectedTarget - _cameraGo.transform.position;
            Assert.That(directionToTarget.x, Is.GreaterThan(0f), "Camera should move in +X toward damped target");
            Assert.That(directionToTarget.y, Is.GreaterThan(0f), "Camera should move in +Y toward damped target");
            Assert.That(directionToTarget.z, Is.GreaterThan(0f), "Camera should move in +Z toward damped target");
        }

        [Test]
        public void LateUpdate_TargetPositionOffset_AppliedToFollowTarget()
        {
            _targetGo.transform.position = new Vector3(2f, 3f, 4f);
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, _cameraGo.transform);
            InvokeAwake();

            Vector3 newTargetPos = new Vector3(50f, 60f, 70f);
            _targetGo.transform.position = newTargetPos;

            InvokeLateUpdate();

            Vector3 expectedFollowTarget = newTargetPos + _settings.FollowOffset;
            Assert.That(_cameraGo.transform.position, Is.Not.EqualTo(expectedFollowTarget));
        }

        [Test]
        public void Awake_PositionsCameraAtTargetPlusFollowOffset_WithDifferentTargetPosition()
        {
            _targetGo.transform.position = new Vector3(10f, 20f, -5f);
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, _cameraGo.transform);

            InvokeAwake();

            Vector3 expectedPosition = _targetGo.transform.position + _settings.FollowOffset;
            Assert.That(_cameraGo.transform.position, Is.EqualTo(expectedPosition));
        }
    }
}
