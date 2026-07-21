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
        private InputAction crouchAction;
        private InputAction jumpAction;
        private InputAction previousAction;
        private InputAction nextAction;
        private InputAction sprintAction;

        public Vector2 Move => moveAction.ReadValue<Vector2>();
        public Vector2 Look => lookAction.ReadValue<Vector2>();

        public bool AttackPressedThisFrame => attackAction.WasPressedThisFrame();
        public bool InteractPressedThisFrame => interactAction.WasPressedThisFrame();
        public bool CrouchPressedThisFrame => crouchAction.WasPressedThisFrame();
        public bool JumpPressedThisFrame => jumpAction.WasPressedThisFrame();
        public bool PreviousPressedThisFrame => previousAction.WasPressedThisFrame();
        public bool NextPressedThisFrame => nextAction.WasPressedThisFrame();
        public bool SprintIsHeld => sprintAction.IsPressed();

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
            crouchAction = playerActions.FindAction("Crouch", true);
            jumpAction = playerActions.FindAction("Jump", true);
            previousAction = playerActions.FindAction("Previous", true);
            nextAction = playerActions.FindAction("Next", true);
            sprintAction = playerActions.FindAction("Sprint", true);
        }

        private void OnEnable()
        {
            playerActions?.Enable();
        }

        private void OnDisable()
        {
            playerActions?.Disable();
        }
    }
}
