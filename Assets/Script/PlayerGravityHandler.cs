using System;
using UnityEngine;

namespace Script
{
    public class PlayerGravityHandler : MonoBehaviour
    {
        private InputSystem_Actions _inputSystemActions;
        private Rigidbody2D _rigidbody2D;
        [SerializeField] private PlayerGravityHandleMode playerGravityHandleMode;
        [SerializeField] private float gravityScaleWhenOn = 1f;
        [SerializeField] private bool startWithGravityOn;
        [SerializeField] private PlayerGravityHandlingState stateOnPress = PlayerGravityHandlingState.GravityOn;
        [SerializeField] private bool enableGravityHandleOnStart;
        [SerializeField] private bool enableGravityHandleInputOnStart;
        [SerializeField] private bool debugInputLogs;
        private bool _acceptInput;
        private bool _gravityHandleEnabled;
        private bool _wasPressedLastFrame;
        private PlayerGravityHandlingState _currentGravityHandlingState;

        private void Start()
        {
            _inputSystemActions = GameManager.Instance.InputActions;
            // Ensure the input actions are enabled so actions produce values/events.
            if (_inputSystemActions != null)
            {
                _inputSystemActions.Enable();
            }
            _rigidbody2D = GetComponent<Rigidbody2D>();

            // Initialize starting gravity state
            if (startWithGravityOn)
                SetCurrentGravityState(PlayerGravityHandlingState.GravityOn);
            else
                SetCurrentGravityState(PlayerGravityHandlingState.GravityOff);

            if (enableGravityHandleInputOnStart)
                SetAcceptInputEnable(true);
            if (enableGravityHandleOnStart)
                SetGravityHandleEnable(true);

            // Initialize _wasPressedLastFrame to the current action state to avoid
            // falsely detecting a press or release on the first frame.
            if (_inputSystemActions != null)
            {
                try
                {
                    var action = _inputSystemActions.Player.ToggleGravity;
                    if (action != null)
                        _wasPressedLastFrame = action.ReadValue<float>() > 0.5f;
                }
                catch
                {
                    // ignore if action not available yet
                }
            }
        }

        private void Update()
        {
            // The generated input wrapper exposes a struct for Player (value type),
            // so the null-conditional operator cannot be used on .Player. Instead
            // explicitly ensure the wrapper itself is not null, then access the
            // action from the value-type Player wrapper.
            // If input actions weren't available at Start (GameManager not ready),
            // try to obtain them now (handles initialization order issues).
            if (_inputSystemActions == null)
            {
                try
                {
                    var gm = GameManager.Instance;
                    if (gm != null && gm.InputActions != null)
                    {
                        _inputSystemActions = gm.InputActions;
                        _inputSystemActions.Enable();
                        // Initialize _wasPressedLastFrame from the action if possible.
                        var initAction = _inputSystemActions.Player.ToggleGravity;
                        if (initAction != null)
                        {
                            _wasPressedLastFrame = initAction.ReadValue<float>() > 0.5f;
                        }
                    }
                    else
                    {
                        // Still not available, skip this frame.
                        return;
                    }
                }
                catch
                {
                    return;
                }
            }

            if (!_acceptInput)
            {
                if (debugInputLogs) Debug.Log($"PlayerGravityHandler: input ignored because _acceptInput is false (GameObject={gameObject.name})");
                return;
            }

             var action = _inputSystemActions.Player.ToggleGravity;
             if (action == null)
             {
                 if (debugInputLogs) Debug.Log($"PlayerGravityHandler: ToggleGravity action is null (GameObject={gameObject.name})");
                 return;
             }

             // Only respond if gravity handling is enabled
             if (!_gravityHandleEnabled)
            {
                if (debugInputLogs) Debug.Log($"PlayerGravityHandler: gravity handling disabled (_gravityHandleEnabled=false) (GameObject={gameObject.name})");
                return;
            }

            // Read pressed state once and use it for both Toggle and PressRelease modes.
            var pressed = action.ReadValue<float>() > 0.5f;

            // Handle Toggle mode: flip state on press edge
            if (playerGravityHandleMode == PlayerGravityHandleMode.Toggle)
            {
                if (pressed && !_wasPressedLastFrame)
                {
                    if (debugInputLogs) Debug.Log($"PlayerGravityHandler: Toggle press edge detected (GameObject={gameObject.name})");
                    if (_currentGravityHandlingState == PlayerGravityHandlingState.GravityOn)
                        SetCurrentGravityState(PlayerGravityHandlingState.GravityOff);
                    else
                        SetCurrentGravityState(PlayerGravityHandlingState.GravityOn);
                }
            }
            // Handle PressRelease mode: gravity set on press, inverted on release
            else if (playerGravityHandleMode == PlayerGravityHandleMode.PressRelease)
            {
                // Press started this frame
                if (pressed && !_wasPressedLastFrame)
                {
                    if (debugInputLogs) Debug.Log($"PlayerGravityHandler: Press started (stateOnPress={stateOnPress}) (GameObject={gameObject.name})");
                    SetCurrentGravityState(stateOnPress);
                }
                // Release happened this frame
                else if (!pressed && _wasPressedLastFrame)
                {
                    if (debugInputLogs) Debug.Log($"PlayerGravityHandler: Press released (stateOnPress={stateOnPress}) (GameObject={gameObject.name})");
                    // Invert state
                    SetCurrentGravityState(stateOnPress == PlayerGravityHandlingState.GravityOn 
                        ? PlayerGravityHandlingState.GravityOff 
                        : PlayerGravityHandlingState.GravityOn);
                }
            }

            // Update last-frame pressed state for edge detection next frame.
            _wasPressedLastFrame = pressed;
         }


        public void SetGravityHandleEnable(bool enable)
        {
            _gravityHandleEnabled = enable;
            
            if (_rigidbody2D == null) _rigidbody2D = GetComponent<Rigidbody2D>();

            if (!enable)
            {
                // Turn off gravity effect
                SetCurrentGravityState(PlayerGravityHandlingState.GravityOff);
            }
            else
            {
                // Apply current state
                SetCurrentGravityState(_currentGravityHandlingState);
            }
        }

        public void SetAcceptInputEnable(bool enable)
        {
            _acceptInput = enable;
        }
        
        private void SetCurrentGravityState(PlayerGravityHandlingState newHandlingState)
        {
            _currentGravityHandlingState = newHandlingState;
            switch (newHandlingState)
            {
                case PlayerGravityHandlingState.GravityOn:
                    OnGravityOn();
                    break;
                case PlayerGravityHandlingState.GravityOff:
                    OnGravityOff();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newHandlingState), newHandlingState, null);
            }
        }

        private void OnGravityOn()
        {
            _rigidbody2D.gravityScale = gravityScaleWhenOn;
        }

        private void OnGravityOff()
        {
            _rigidbody2D.gravityScale = 0f;
        }


        #region Nested Types

        public enum PlayerGravityHandlingState
        {
            GravityOn,
            GravityOff
        }
        
        public enum PlayerGravityHandleMode
        {
            Toggle,
            PressRelease
        }

        #endregion
    }
}