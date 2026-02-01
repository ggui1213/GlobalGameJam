using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    public class MouseTrialTrigger : MonoBehaviour
    {
        [SerializeField] private Camera referenceCamera;
        private InputSystem_Actions _actions;

        private void Start()
        {
            _actions = new InputSystem_Actions();
            // Enable the Player action map and subscribe to the mouse left click events
            _actions.Player.Enable();
            _actions.Player.MouseLeftClick.started += OnMouseLeftStarted;
            _actions.Player.MouseLeftClick.canceled += OnMouseLeftCanceled;
        }

        private void OnDestroy()
        {
            if (_actions == null) return;
            // Unsubscribe and dispose
            _actions.Player.MouseLeftClick.started -= OnMouseLeftStarted;
            _actions.Player.MouseLeftClick.canceled -= OnMouseLeftCanceled;
            _actions.Player.Disable();
            _actions.Dispose();
            _actions = null;
        }

        private void Update()
        {
            // While the action is pressed, add positions to the current trial
            if (_actions != null && _actions.Player.MouseLeftClick.ReadValue<float>() > 0f)
            {
                var mousePosition = Mouse.current.position.ReadValue();
                var point = referenceCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
                MovementTrialManager.Instance.AddNewPositionToCurrentTrial(point);
            }
        }

        private void OnMouseLeftStarted(InputAction.CallbackContext ctx)
        {
            MovementTrialManager.Instance.InitiateNewTrial();
        }

        private void OnMouseLeftCanceled(InputAction.CallbackContext ctx)
        {
            MovementTrialManager.Instance.EndCurrentTrial();
        }
    }
}