using System;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public sealed class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform cameraTransform;

        private Vector3 _positionOffset;
        private Quaternion _initialRotation;

        private void Awake()
        {
            if (target == null)
                throw new InvalidOperationException(
                    $"{nameof(CameraManager)} requires a target Transform reference.");

            if (cameraTransform == null)
                throw new InvalidOperationException(
                    $"{nameof(CameraManager)} requires a camera Transform reference.");

            _positionOffset = cameraTransform.position - target.position;
            _initialRotation = cameraTransform.rotation;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            cameraTransform.SetPositionAndRotation(
                target.position + _positionOffset,
                _initialRotation);
        }
    }
}
