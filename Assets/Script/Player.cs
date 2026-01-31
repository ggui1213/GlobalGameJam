using System;
using UnityEngine;

namespace Script
{
    public class Player : MonoBehaviour
    {
        private InputSystem_Actions _inputSystemActions;
        private Rigidbody2D _rigidbody2D;
        [SerializeField] private float gravityScale = 1;
        [SerializeField] private PlayerGravityState initialGravityState = PlayerGravityState.GravityOn;
        private PlayerGravityState _currentGravityState;


        private void Start()
        {
            _inputSystemActions = GameManager.Instance.InputActions;
            _rigidbody2D = GetComponent<Rigidbody2D>();
            
            _currentGravityState = initialGravityState;
            _rigidbody2D.gravityScale = initialGravityState == PlayerGravityState.GravityOn ? gravityScale : 0;
        }

        private void Update()
        {
            if (_inputSystemActions.Player.ToggleGravity.triggered)
            {
                _currentGravityState = _currentGravityState == PlayerGravityState.GravityOn
                    ? PlayerGravityState.GravityOff
                    : PlayerGravityState.GravityOn;
                
                _rigidbody2D.gravityScale = _currentGravityState == PlayerGravityState.GravityOn ? gravityScale : 0;
            }
        }

        private void OnEnterTrap()
        {
            throw new NotImplementedException();
        }

        
        


        /// <summary>
        /// Detect collision with traps
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Trap")) return;
            
            OnEnterTrap();
        }
        
        public enum PlayerGravityState
        {
            GravityOn,
            GravityOff
        }
    }
}