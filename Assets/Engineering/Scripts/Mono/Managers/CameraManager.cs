using System;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public sealed class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform cameraTransform;

        [SerializeField] private float xDamping = 0.18f;
        [SerializeField] private float yDamping = 0.25f;
        [SerializeField] private float zDamping = 0.18f;
        [SerializeField] private float maxFollowSpeed;

        private Vector3 _positionOffset;
        private Quaternion _initialRotation;
        private Vector3 _velocity;

        private void Awake()
        {
            ValidateReferences();
            CaptureOffsets();
            _velocity = Vector3.zero;
        }

        private void OnEnable()
        {
            _velocity = Vector3.zero;
        }

        private void ValidateReferences()
        {
            if (target == null)
                throw new InvalidOperationException(
                    $"{nameof(CameraManager)} requires a target Transform reference.");

            if (cameraTransform == null)
                throw new InvalidOperationException(
                    $"{nameof(CameraManager)} requires a camera Transform reference.");
        }

        private void CaptureOffsets()
        {
            _positionOffset = cameraTransform.position - target.position;
            _initialRotation = cameraTransform.rotation;
        }

        private void LateUpdate()
        {
            if (target == null || cameraTransform == null)
                return;

            Vector3 targetPosition = target.position + _positionOffset;
            float dt = Time.deltaTime;
            float maxSpeed = maxFollowSpeed > 0f ? maxFollowSpeed : float.PositiveInfinity;

            float clampedXDamping = Mathf.Max(xDamping, 0.001f);
            float clampedYDamping = Mathf.Max(yDamping, 0.001f);
            float clampedZDamping = Mathf.Max(zDamping, 0.001f);

            float newX = Mathf.SmoothDamp(
                cameraTransform.position.x, targetPosition.x, ref _velocity.x,
                clampedXDamping, maxSpeed, dt);
            float newY = Mathf.SmoothDamp(
                cameraTransform.position.y, targetPosition.y, ref _velocity.y,
                clampedYDamping, maxSpeed, dt);
            float newZ = Mathf.SmoothDamp(
                cameraTransform.position.z, targetPosition.z, ref _velocity.z,
                clampedZDamping, maxSpeed, dt);

            cameraTransform.SetPositionAndRotation(
                new Vector3(newX, newY, newZ),
                _initialRotation);
        }
    }
}
