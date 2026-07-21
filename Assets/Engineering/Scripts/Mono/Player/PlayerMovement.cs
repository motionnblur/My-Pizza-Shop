using UnityEngine;

using Engineering.Scripts.Mono.Managers;

namespace Engineering.Scripts.Mono.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;

        private void OnEnable()
        {
            inputManager.MoveChanged += OnMoveChanged;
            inputManager.LookChanged += OnLookChanged;
            inputManager.AttackPressed += OnAttackPressed;
            inputManager.InteractPerformed += OnInteractPerformed;
            inputManager.PreviousPressed += OnPreviousPressed;
            inputManager.NextPressed += OnNextPressed;
            inputManager.SprintStarted += OnSprintStarted;
            inputManager.SprintCanceled += OnSprintCanceled;
        }

        private void OnDisable()
        {
            inputManager.MoveChanged -= OnMoveChanged;
            inputManager.LookChanged -= OnLookChanged;
            inputManager.AttackPressed -= OnAttackPressed;
            inputManager.InteractPerformed -= OnInteractPerformed;
            inputManager.PreviousPressed -= OnPreviousPressed;
            inputManager.NextPressed -= OnNextPressed;
            inputManager.SprintStarted -= OnSprintStarted;
            inputManager.SprintCanceled -= OnSprintCanceled;
        }

        private void OnMoveChanged(Vector2 value) { }

        private void OnLookChanged(Vector2 value) { }

        private void OnAttackPressed() { }

        private void OnInteractPerformed() { }

        private void OnPreviousPressed() { }

        private void OnNextPressed() { }

        private void OnSprintStarted() { }

        private void OnSprintCanceled() { }
    }
}