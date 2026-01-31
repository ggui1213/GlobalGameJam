using System;
using UnityEngine;

namespace Script
{
    public class PlayerGravityHandler : MonoBehaviour
    {
        private InputSystem_Actions _inputSystemActions;
        private Rigidbody2D _rigidbody2D;
        [SerializeField] private float gravityScale = 1;
        [SerializeField] private PlayerGravityState initialGravityState = PlayerGravityState.GravityOn;
        [SerializeField] private bool handlingGravityToggleOnStart = false;
        private PlayerGravityState _currentGravityState;
        
        private bool _isHandlingGravityToggle;


        private void Start()
        {
            _inputSystemActions = GameManager.Instance.InputActions;
            _rigidbody2D = GetComponent<Rigidbody2D>();
            
            _currentGravityState = initialGravityState;
            _isHandlingGravityToggle = handlingGravityToggleOnStart;
            if (_isHandlingGravityToggle)
                BeginHandlingGravityToggle();
        }
        
        public void BeginHandlingGravityToggle() => BeginHandlingGravityToggle(initialGravityState);
        public void BeginHandlingGravityToggle(PlayerGravityState startingGravityStateOverride)
        {
            _isHandlingGravityToggle = true;
            _currentGravityState = startingGravityStateOverride;
            // Set initial gravity state
            _rigidbody2D.gravityScale = _currentGravityState == PlayerGravityState.GravityOn ? gravityScale : 0;
        }

        public void EndHandlingGravityToggle() => EndHandlingGravityToggle(_currentGravityState);
        public void EndHandlingGravityToggle(PlayerGravityState endingState)
        {
            _isHandlingGravityToggle = false;
            _currentGravityState = endingState;
            // Set final gravity state
            _rigidbody2D.gravityScale = _currentGravityState == PlayerGravityState.GravityOn ? gravityScale : 0;
        }

        private void Update()
        {
            if (!_isHandlingGravityToggle)
                return;
                
            if (_inputSystemActions.Player.ToggleGravity.triggered)
            {
                _currentGravityState = _currentGravityState == PlayerGravityState.GravityOn
                    ? PlayerGravityState.GravityOff
                    : PlayerGravityState.GravityOn;
                
                _rigidbody2D.gravityScale = _currentGravityState == PlayerGravityState.GravityOn ? gravityScale : 0;
            }
        }
        
        public enum PlayerGravityState
        {
            GravityOn,
            GravityOff
        }
    }
}