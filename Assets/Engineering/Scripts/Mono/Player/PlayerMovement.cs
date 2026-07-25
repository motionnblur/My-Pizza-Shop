using System;
using UnityEngine;

using Engineering.Scripts.Mono.Managers;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private const string MeshChildName = "Mesh";

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float meshYawAtStart = 45f;

        private InputManager _inputManager;
        private bool _isInputSubscribed;
        private Vector2 moveInput;
        private bool isSprinting;

        public void Initialize(InputManager inputManager)
        {
            if (inputManager == null)
                throw new ArgumentNullException(nameof(inputManager));

            if (_inputManager != null)
            {
                if (_inputManager != inputManager)
                    throw new InvalidOperationException(
                        $"{nameof(PlayerMovement)}: already initialized with a different {nameof(InputManager)}.");
                return;
            }

            _inputManager = inputManager;

            if (isActiveAndEnabled)
                SubscribeToInput();
        }

        private void OnEnable()
        {
            SubscribeToInput();
        }

        private void OnDisable()
        {
            UnsubscribeFromInput();
            moveInput = Vector2.zero;
            isSprinting = false;
        }

        private void SubscribeToInput()
        {
            if (_inputManager == null || _isInputSubscribed)
                return;

            _inputManager.MoveChanged += OnMoveChanged;
            _inputManager.SprintStarted += OnSprintStarted;
            _inputManager.SprintCanceled += OnSprintCanceled;
            _isInputSubscribed = true;
        }

        private void UnsubscribeFromInput()
        {
            if (!_isInputSubscribed || _inputManager == null)
                return;

            _inputManager.MoveChanged -= OnMoveChanged;
            _inputManager.SprintStarted -= OnSprintStarted;
            _inputManager.SprintCanceled -= OnSprintCanceled;
            _isInputSubscribed = false;
        }

        private void FixedUpdate()
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 direction = cameraForward * moveInput.y + cameraRight * moveInput.x;

            if (direction.magnitude > 0.1f)
            {
                float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
                Vector3 targetVelocity = direction.normalized * speed;
                targetVelocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = targetVelocity;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                _rb.angularVelocity = Vector3.zero;
            }
        }

        private void OnMoveChanged(Vector2 value)
        {
            moveInput = value;
        }

        private void OnSprintStarted()
        {
            isSprinting = true;
        }

        private void OnSprintCanceled()
        {
            isSprinting = false;
        }
    }
}
