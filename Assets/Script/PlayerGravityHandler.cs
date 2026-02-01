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
        [SerializeField] private PlayerSpriteManager playerSpriteManager;
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

            // Initialize the player sprite manager's right-button state to match
            // the current input state so we don't show an incorrect sprite on start.
            if (playerSpriteManager != null)
            {
                playerSpriteManager.RightMouseButtonDown = _wasPressedLastFrame;
            }
            else
            {
                // Try to auto-find the PlayerSpriteManager on the same GameObject or its children
                playerSpriteManager = GetComponent<PlayerSpriteManager>() ?? GetComponentInChildren<PlayerSpriteManager>();
                if (playerSpriteManager != null)
                    playerSpriteManager.RightMouseButtonDown = _wasPressedLastFrame;
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

            // Ensure we have a reference to playerSpriteManager; try to auto-find if missing
            if (playerSpriteManager == null)
            {
                playerSpriteManager = GetComponent<PlayerSpriteManager>() ?? GetComponentInChildren<PlayerSpriteManager>();
            }

            // Always update player sprite manager to reflect current pressed state (keeps UI consistent)
            if (playerSpriteManager != null)
            {
                // Only set and optionally log; setting every frame is cheap and keeps sprite consistent
                playerSpriteManager.RightMouseButtonDown = pressed;
                if (debugInputLogs) Debug.Log($"PlayerGravityHandler: RightMouseButtonDown set to {pressed} (GameObject={gameObject.name})");
            }

            // Update player sprite manager when the pressed state changes (press or release)
            // (edge-detection preserved for other logic; sprite already updated above)

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
            // Handle PressRelease mode: gravity set on press, inverted on release (Continuous)
            else if (playerGravityHandleMode == PlayerGravityHandleMode.PressRelease)
            {
                var targetState = pressed 
                    ? stateOnPress 
                    : (stateOnPress == PlayerGravityHandlingState.GravityOn 
                        ? PlayerGravityHandlingState.GravityOff 
                        : PlayerGravityHandlingState.GravityOn);
                
                // Only apply if state changed (or to ensure consistency)
                if (_currentGravityHandlingState != targetState)
                {
                    if (debugInputLogs) Debug.Log($"PlayerGravityHandler: PressRelease state update (pressed={pressed}, target={targetState})");
                    SetCurrentGravityState(targetState);
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
            var previousState = _currentGravityHandlingState;
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
            
            // FMOD: 播放重力切换音效（仅当状态实际发生变化时）
            if (previousState != newHandlingState && FMODAudioManager.Instance != null)
            {
                FMODAudioManager.Instance.PlayGravityToggle();
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