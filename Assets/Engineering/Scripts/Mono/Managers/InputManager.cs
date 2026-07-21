using UnityEngine;

using System;
using UnityEngine.InputSystem;

namespace Engineering.Scripts.Mono.Managers
{
    public class InputManager : MonoBehaviour
    {
        private const string PlayerMapName = "Player";

        [SerializeField] private InputActionAsset inputActions;

        private InputActionMap playerActions;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction attackAction;
        private InputAction interactAction;
        private InputAction previousAction;
        private InputAction nextAction;
        private InputAction sprintAction;

        public event Action<Vector2> MoveChanged;
        public event Action<Vector2> LookChanged;
        public event Action AttackPressed;
        public event Action InteractPerformed;
        public event Action PreviousPressed;
        public event Action NextPressed;
        public event Action SprintStarted;
        public event Action SprintCanceled;

        private void Awake()
        {
            if (inputActions == null)
            {
                throw new InvalidOperationException($"{nameof(InputManager)} requires an {nameof(InputActionAsset)} reference.");
            }

            playerActions = inputActions.FindActionMap(PlayerMapName, true);
            moveAction = playerActions.FindAction("Move", true);
            lookAction = playerActions.FindAction("Look", true);
            attackAction = playerActions.FindAction("Attack", true);
            interactAction = playerActions.FindAction("Interact", true);
            previousAction = playerActions.FindAction("Previous", true);
            nextAction = playerActions.FindAction("Next", true);
            sprintAction = playerActions.FindAction("Sprint", true);
        }

        private void OnEnable()
        {
            moveAction.performed += OnMoveChanged;
            moveAction.canceled += OnMoveChanged;
            lookAction.performed += OnLookChanged;
            lookAction.canceled += OnLookChanged;
            attackAction.performed += OnAttackPerformed;
            interactAction.performed += OnInteractPerformed;
            previousAction.performed += OnPreviousPerformed;
            nextAction.performed += OnNextPerformed;
            sprintAction.started += OnSprintStarted;
            sprintAction.canceled += OnSprintCanceled;

            playerActions?.Enable();
        }

        private void OnDisable()
        {
            playerActions?.Disable();

            moveAction.performed -= OnMoveChanged;
            moveAction.canceled -= OnMoveChanged;
            lookAction.performed -= OnLookChanged;
            lookAction.canceled -= OnLookChanged;
            attackAction.performed -= OnAttackPerformed;
            interactAction.performed -= OnInteractPerformed;
            previousAction.performed -= OnPreviousPerformed;
            nextAction.performed -= OnNextPerformed;
            sprintAction.started -= OnSprintStarted;
            sprintAction.canceled -= OnSprintCanceled;
        }

        private void OnMoveChanged(InputAction.CallbackContext context)
        {
            MoveChanged?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnLookChanged(InputAction.CallbackContext context)
        {
            LookChanged?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPressed?.Invoke();
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            InteractPerformed?.Invoke();
        }

        private void OnPreviousPerformed(InputAction.CallbackContext context)
        {
            PreviousPressed?.Invoke();
        }

        private void OnNextPerformed(InputAction.CallbackContext context)
        {
            NextPressed?.Invoke();
        }

        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            SprintStarted?.Invoke();
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            SprintCanceled?.Invoke();
        }
    }
}
