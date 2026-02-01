using System;
using UnityEngine;

namespace Script
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        private InputSystem_Actions _actions;
        public InputSystem_Actions InputActions => _actions;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            _actions = new InputSystem_Actions();
            _actions.Enable();
        }

        private void OnDestroy()
        {
            _actions.Disable();
        }
    }
}