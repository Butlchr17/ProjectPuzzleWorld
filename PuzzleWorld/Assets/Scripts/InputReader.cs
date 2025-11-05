using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleWorld
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputReader :MonoBehaviour
    {
        PlayerInput playerInput;
        InputAction selectAction;
        InputAction fireAction;

        //public event Action Fire;

        public event Action OnPointerDownEvent;
        public event Action OnPointerUpEvent;

        public Vector2 Selected => selectAction.ReadValue<Vector2>();

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            selectAction = playerInput.actions["Select"];
            fireAction = playerInput.actions["Fire"];

            //fireAction.performed += OnFire;
            fireAction.started += ctx => OnPointerDownEvent?.Invoke();
            fireAction.canceled += ctx => OnPointerUpEvent?.Invoke();
        }

        //void Destroy()
        //{
        //    fireAction.performed -= OnFire;
        //}

        void OnDestroy()
        {
            fireAction.started -= ctx => OnPointerDownEvent?.Invoke();
            fireAction.canceled -= ctx => OnPointerUpEvent?.Invoke();
        }

        //void OnFire(InputAction.CallbackContext obj) => Fire?.Invoke();

    }
}

