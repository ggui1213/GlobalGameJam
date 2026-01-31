using System;
using UnityEngine;

namespace Script
{
    public class Player : MonoBehaviour
    {
        private InputSystem_Actions _inputSystemActions;


        private void Start()
        {
            _inputSystemActions = GameManager.Instance.InputActions;
        }

        private void Update()
        {
            
        }


        /// <summary>
        /// Detect collision with traps
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Trap")) return;
            
            OnEnterTrap();
        }

        private void OnEnterTrap()
        {
            
        }
    }
}