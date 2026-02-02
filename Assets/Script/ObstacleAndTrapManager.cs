using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    public class ObstacleAndTrapManager : MonoBehaviour
    {
        private InputSystem_Actions _actions;
        private List<ObstacleOrTrapInfo> _obstacles = new();
        private List<ObstacleOrTrapInfo> _traps = new();
        
        [SerializeField] private VisibilityState initialState = VisibilityState.ObstacleVisible;
        [SerializeField] private ObstacleHandleMode handleMode = ObstacleHandleMode.Toggle;
        [SerializeField] private VisibilityState stateOnPress = VisibilityState.TrapVisible;
        [SerializeField] private float obstacleVisibleOpacity = 1f;
        [SerializeField] private float obstacleHidedOpacity = 0.25f;
        [SerializeField] private float trapVisibleOpacity = 1f;
        [SerializeField] private float trapHidedOpacity = 0.25f;
        
        [SerializeField] private PlayerSpriteManager playerSpriteManager;
        
        private VisibilityState _currentState;
        private bool _wasPressedLastFrame;
        
        void Start()
        {
            _actions = GameManager.Instance.InputActions;
            
            var obstacleObjects = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (var obstacleObject in obstacleObjects)
                _obstacles.Add(new ObstacleOrTrapInfo(obstacleObject.GetComponent<SpriteRenderer>(), obstacleObject.GetComponent<Collider2D>()));
            
            var trapObjects = GameObject.FindGameObjectsWithTag("Trap");
            foreach (var trapObject in trapObjects)
                _traps.Add(new ObstacleOrTrapInfo(trapObject.GetComponent<SpriteRenderer>(), trapObject.GetComponent<Collider2D>()));
            
            if (_actions != null)
            {
                var action = _actions.Player.ToggleObstacle;
                if (action != null)
                    _wasPressedLastFrame = action.ReadValue<float>() > 0.5f;
            }

            // If not assigned in Inspector, attempt to auto-find a PlayerSpriteManager in the scene
            if (playerSpriteManager == null)
            {
                playerSpriteManager = GetComponent<PlayerSpriteManager>();
                if (playerSpriteManager == null)
                    playerSpriteManager = GetComponentInChildren<PlayerSpriteManager>();
                if (playerSpriteManager == null)
                    playerSpriteManager = UnityEngine.Object.FindFirstObjectByType<PlayerSpriteManager>();
            }

            // Initialize player sprite manager's left-button state so sprite is correct at start
            if (playerSpriteManager != null)
            {
                playerSpriteManager.LeftMouseButtonDown = _wasPressedLastFrame;
            }

            _currentState = initialState;
            SetVisibilityState(_currentState);
        }

        // Update is called once per frame
        void Update()
        {
            if (_actions == null) return;
            var action = _actions.Player.ToggleObstacle;
            if (action == null) return;

            bool pressed = action.ReadValue<float>() > 0.5f;

            // Always update the player's left-button sprite state every frame (avoid visual desync)
            if (playerSpriteManager == null)
            {
                playerSpriteManager = FindAnyObjectByType<PlayerSpriteManager>();
            }
            if (playerSpriteManager != null)
            {
                playerSpriteManager.LeftMouseButtonDown = pressed;
            }

            if (handleMode == ObstacleHandleMode.Toggle)
            {
                if (pressed && !_wasPressedLastFrame)
                {
                    _currentState = _currentState == VisibilityState.ObstacleVisible
                        ? VisibilityState.TrapVisible
                        : VisibilityState.ObstacleVisible;
                
                    SetVisibilityState(_currentState);
                    
                    // FMOD: 播放碰撞切換音效
                    if (FMODAudioManager.Instance != null)
                        FMODAudioManager.Instance.PlayCollisionToggle();
                }
            }
            else // PressRelease
            {
                if (pressed && !_wasPressedLastFrame)
                {
                    // Pressed
                    _currentState = stateOnPress;
                    SetVisibilityState(_currentState);
                    
                    // FMOD: 播放碰撞切換音效
                    if (FMODAudioManager.Instance != null)
                        FMODAudioManager.Instance.PlayCollisionToggle();
                }
                else if (!pressed && _wasPressedLastFrame)
                {
                    // Released
                    _currentState = stateOnPress == VisibilityState.ObstacleVisible 
                        ? VisibilityState.TrapVisible 
                        : VisibilityState.ObstacleVisible;
                    SetVisibilityState(_currentState);
                    
                    // FMOD: 播放碰撞切換音效
                    if (FMODAudioManager.Instance != null)
                        FMODAudioManager.Instance.PlayCollisionToggle();
                }
            }

            _wasPressedLastFrame = pressed;
        }
        
        private void SetVisibilityState(VisibilityState state)
        {
            Color colorCache;
            switch (state)
            {
                case VisibilityState.ObstacleVisible:
                    foreach (var obstacle in _obstacles)
                    {
                        colorCache = obstacle.SpriteRenderer.color;
                        colorCache.a = obstacleVisibleOpacity;
                        obstacle.SpriteRenderer.color = colorCache;
                        
                        obstacle.Collider.enabled = true;
                    }
                    foreach (var trap in _traps)
                    {
                        colorCache = trap.SpriteRenderer.color;
                        colorCache.a = trapHidedOpacity;
                        trap.SpriteRenderer.color = colorCache;
                        
                        trap.Collider.enabled = false;
                    }
                    break;
                case VisibilityState.TrapVisible:
                    foreach (var obstacle in _obstacles)
                    {
                        colorCache = obstacle.SpriteRenderer.color;
                        colorCache.a = obstacleHidedOpacity;
                        obstacle.SpriteRenderer.color = colorCache;
                        
                        obstacle.Collider.enabled = false;
                    }
                    foreach (var trap in _traps)
                    {
                        colorCache = trap.SpriteRenderer.color;
                        colorCache.a = trapVisibleOpacity;
                        trap.SpriteRenderer.color = colorCache;
                        
                        trap.Collider.enabled = true;
                    }
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        private struct ObstacleOrTrapInfo
        {
            public readonly SpriteRenderer SpriteRenderer;
            public readonly Collider2D Collider;

            public ObstacleOrTrapInfo(SpriteRenderer spriteRenderer, Collider2D collider)
            {
                SpriteRenderer = spriteRenderer;
                Collider = collider;
            }
        }
        
        public enum VisibilityState
        {
            ObstacleVisible,
            TrapVisible
        }

        public enum ObstacleHandleMode
        {
            Toggle,
            PressRelease
        }
    }
}
