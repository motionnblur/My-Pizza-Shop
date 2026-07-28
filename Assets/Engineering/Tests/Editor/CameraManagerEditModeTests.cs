using System;
using System.Reflection;
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

        [SetUp]
        public void SetUp()
        {
            _managerGo = new GameObject("CameraManager");
            _managerGo.SetActive(false);

            _targetGo = new GameObject("Target");
            _cameraGo = new GameObject("Camera");

            _manager = _managerGo.AddComponent<CameraManager>();
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
        }

        private void SetReferences(Transform target, Transform cameraTransform)
        {
            var serialized = new SerializedObject(_manager);
            serialized.FindProperty("target").objectReferenceValue = target;
            serialized.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
            serialized.ApplyModifiedProperties();
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

        private Vector3 GetVelocity()
        {
            var field = typeof(CameraManager).GetField("_velocity",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (Vector3)field.GetValue(_manager);
        }

        private void SetVelocity(Vector3 velocity)
        {
            var field = typeof(CameraManager).GetField("_velocity",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_manager, velocity);
        }

        private Vector3 GetPositionOffset()
        {
            var field = typeof(CameraManager).GetField("_positionOffset",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (Vector3)field.GetValue(_manager);
        }

        private Quaternion GetInitialRotation()
        {
            var field = typeof(CameraManager).GetField("_initialRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (Quaternion)field.GetValue(_manager);
        }

        [Test]
        public void Awake_WithNullTarget_ThrowsInvalidOperationException()
        {
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(null, _cameraGo.transform);

            Assert.That(() => _managerGo.SetActive(true), Throws.InvalidOperationException);
        }

        [Test]
        public void Awake_WithNullCameraTransform_ThrowsInvalidOperationException()
        {
            _targetGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, null);

            Assert.That(() => _managerGo.SetActive(true), Throws.InvalidOperationException);
        }

        [Test]
        public void Awake_CapturesPositionOffsetCorrectly()
        {
            _targetGo.transform.position = new Vector3(1f, 2f, 3f);
            _cameraGo.transform.position = new Vector3(5f, 10f, 15f);
            SetReferences(_targetGo.transform, _cameraGo.transform);

            _managerGo.SetActive(true);

            Vector3 expectedOffset = new Vector3(4f, 8f, 12f);
            Assert.That(GetPositionOffset(), Is.EqualTo(expectedOffset));
        }

        [Test]
        public void Awake_CapturesInitialRotationCorrectly()
        {
            _cameraGo.transform.rotation = Quaternion.Euler(10f, 20f, 30f);
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = Vector3.zero;
            SetReferences(_targetGo.transform, _cameraGo.transform);

            _managerGo.SetActive(true);

            Assert.That(GetInitialRotation(), Is.EqualTo(Quaternion.Euler(10f, 20f, 30f)));
        }

        [Test]
        public void OnEnable_ResetsVelocityToZero()
        {
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = new Vector3(5f, 5f, 5f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            _managerGo.SetActive(true);

            SetVelocity(new Vector3(100f, 200f, 300f));

            InvokeOnEnable();

            Assert.That(GetVelocity(), Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void LateUpdate_WithNullTarget_DoesNotChangeCameraTransform()
        {
            _cameraGo.transform.position = new Vector3(10f, 20f, 30f);
            _cameraGo.transform.rotation = Quaternion.Euler(5f, 10f, 15f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            _managerGo.SetActive(true);

            var serialized = new SerializedObject(_manager);
            serialized.FindProperty("target").objectReferenceValue = null;
            serialized.ApplyModifiedProperties();

            InvokeLateUpdate();

            Assert.That(_cameraGo.transform.position, Is.EqualTo(new Vector3(10f, 20f, 30f)));
            Assert.That(_cameraGo.transform.rotation, Is.EqualTo(Quaternion.Euler(5f, 10f, 15f)));
        }

        [Test]
        public void LateUpdate_PreservesInitialRotation()
        {
            _cameraGo.transform.rotation = Quaternion.Euler(15f, 30f, 45f);
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = new Vector3(0f, 2f, 0f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            _managerGo.SetActive(true);

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
            _managerGo.SetActive(true);

            _targetGo.transform.position = new Vector3(100f, 100f, 100f);
            Vector3 expectedSnapPosition = _targetGo.transform.position + GetPositionOffset();

            InvokeLateUpdate();

            Assert.That(_cameraGo.transform.position, Is.Not.EqualTo(expectedSnapPosition));
        }

        [Test]
        public void LateUpdate_TargetPositionOffset_AppliedToFollowTarget()
        {
            _targetGo.transform.position = new Vector3(2f, 3f, 4f);
            _cameraGo.transform.position = new Vector3(7f, 9f, 13f);
            SetReferences(_targetGo.transform, _cameraGo.transform);
            _managerGo.SetActive(true);

            Vector3 capturedOffset = GetPositionOffset();
            Vector3 newTargetPos = new Vector3(50f, 60f, 70f);
            _targetGo.transform.position = newTargetPos;

            InvokeLateUpdate();

            Vector3 expectedFollowTarget = newTargetPos + capturedOffset;
            Assert.That(_cameraGo.transform.position, Is.Not.EqualTo(expectedFollowTarget));
        }

        [Test]
        public void Awake_ResetsVelocityToZero()
        {
            _targetGo.transform.position = Vector3.zero;
            _cameraGo.transform.position = new Vector3(5f, 5f, 5f);
            SetReferences(_targetGo.transform, _cameraGo.transform);

            _managerGo.SetActive(true);

            Assert.That(GetVelocity(), Is.EqualTo(Vector3.zero));
        }
    }
}
