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

        public event Action Fire;
        public Vector2 Selected => selectAction.ReadValue<Vector2>();

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            selectAction = playerInput.actions["Select"];
            fireAction = playerInput.actions["Fire"];

            fireAction.performed += OnFire;
        }

        void Destroy()
        {
            fireAction.performed -= OnFire;
        }

        void OnFire(InputAction.CallbackContext obj) => Fire?.Invoke();

    }
}

