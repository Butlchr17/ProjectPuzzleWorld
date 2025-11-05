using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleWorld
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputReader : MonoBehaviour
    {
        PlayerInput playerInput;
        InputAction selectAction;
        InputAction fireAction;

        public event Action PointerPressed;
        public event Action PointerReleased;

        public Vector2 Selected => selectAction.ReadValue<Vector2>();

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            selectAction = playerInput.actions["Select"];
            fireAction = playerInput.actions["Fire"];

            fireAction.started += ctx => PointerPressed?.Invoke();
            fireAction.canceled += ctx => PointerReleased?.Invoke();
        }

        void OnDestroy()
        {
            fireAction.started -= ctx => PointerPressed?.Invoke();
            fireAction.canceled -= ctx => PointerReleased?.Invoke();
        }
    }
}
