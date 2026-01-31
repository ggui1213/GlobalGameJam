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
        [SerializeField] private bool handlingGravityOnStart = false;
        [SerializeField] private bool allowToggleGravityOnStart = true;
        private PlayerGravityState _currentGravityState;
        
        private bool _allowToggleGravity;
        private bool _isHandlingGravityToggle;


        private void Start()
        {
            _inputSystemActions = GameManager.Instance.InputActions;
            _rigidbody2D = GetComponent<Rigidbody2D>();
            
            _currentGravityState = initialGravityState;
            
            
            _allowToggleGravity = allowToggleGravityOnStart;
            
            _isHandlingGravityToggle = handlingGravityOnStart;
            if (_isHandlingGravityToggle)
                BeginHandlingGravity();
        }

        private void Update()
        {
                
            if (_inputSystemActions.Player.ToggleGravity.triggered)
                TryToggleGravity();
        }

        
        public void EnableToggleGravity() => _allowToggleGravity = true;
        public void DisableToggleGravity() => _allowToggleGravity = false;
        
        public void BeginHandlingGravity() => BeginHandlingGravity(_currentGravityState);
        public void BeginHandlingGravity(PlayerGravityState startingGravityStateOverride)
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

        public bool TryToggleGravity()
        {
            if (!_allowToggleGravity)
                return false;
            _currentGravityState = _currentGravityState == PlayerGravityState.GravityOn? PlayerGravityState.GravityOff : PlayerGravityState.GravityOn;
            
            if (_isHandlingGravityToggle)
                _rigidbody2D.gravityScale = _currentGravityState == PlayerGravityState.GravityOn ? gravityScale : 0;
            return true;
        }
        public enum PlayerGravityState
        {
            GravityOn,
            GravityOff
        }
    }
}