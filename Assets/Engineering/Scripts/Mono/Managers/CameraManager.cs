using System;
using Engineering.ScriptableObjects;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public sealed class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private SCameraSettings settings;

        private Quaternion _initialRotation;
        private Vector3 _velocity;

        private void Awake()
        {
            ValidateReferences();
            _initialRotation = cameraTransform.rotation;
            cameraTransform.position = target.position + settings.FollowOffset;
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

            if (settings == null)
                throw new InvalidOperationException(
                    $"{nameof(CameraManager)} requires a {nameof(SCameraSettings)} reference.");
        }

        private void LateUpdate()
        {
            if (target == null || cameraTransform == null || settings == null)
                return;

            Vector3 targetPosition = target.position + settings.FollowOffset;
            float dt = Time.deltaTime;
            float maxSpeed = settings.MaxFollowSpeed > 0f ? settings.MaxFollowSpeed : float.PositiveInfinity;

            float clampedXDamping = Mathf.Max(settings.XDamping, 0.001f);
            float clampedYDamping = Mathf.Max(settings.YDamping, 0.001f);
            float clampedZDamping = Mathf.Max(settings.ZDamping, 0.001f);

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
