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
        [SerializeField] private float obstacleVisibleOpacity = 1f;
        [SerializeField] private float obstacleHidedOpacity = 0.25f;
        [SerializeField] private float trapVisibleOpacity = 1f;
        [SerializeField] private float trapHidedOpacity = 0.25f;
        
        private VisibilityState _currentState;
        
        void Start()
        {
            _actions = GameManager.Instance.InputActions;
            
            var obstacleObjects = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (var obstacleObject in obstacleObjects)
                _obstacles.Add(new ObstacleOrTrapInfo(obstacleObject,obstacleObject.GetComponent<SpriteRenderer>(), obstacleObject.GetComponent<Collider2D>()));
            
            var trapObjects = GameObject.FindGameObjectsWithTag("Trap");
            foreach (var trapObject in trapObjects)
                _traps.Add(new ObstacleOrTrapInfo(trapObject,trapObject.GetComponent<SpriteRenderer>(), trapObject.GetComponent<Collider2D>()));
            
            _currentState = initialState;
            SetVisibilityState(_currentState);
        }

        // Update is called once per frame
        void Update()
        {
            if (_actions.Player.ToggleObstacle.triggered)
            {
                _currentState = _currentState == VisibilityState.ObstacleVisible
                    ? VisibilityState.TrapVisible
                    : VisibilityState.ObstacleVisible;
                
                SetVisibilityState(_currentState);
            }
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
                        
                        obstacle.SpriteRenderer.enabled = true;
                        obstacle.Collider.enabled = true;
                    }
                    foreach (var trap in _traps)
                    {
                        colorCache = trap.SpriteRenderer.color;
                        colorCache.a = trapHidedOpacity;
                        trap.SpriteRenderer.color = colorCache;
                        
                        trap.SpriteRenderer.enabled = false;
                        trap.Collider.enabled = false;
                    }
                    break;
                case VisibilityState.TrapVisible:
                    foreach (var obstacle in _obstacles)
                    {
                        colorCache = obstacle.SpriteRenderer.color;
                        colorCache.a = obstacleHidedOpacity;
                        obstacle.SpriteRenderer.color = colorCache;
                        
                        obstacle.SpriteRenderer.enabled = false;
                        obstacle.Collider.enabled = false;
                    }
                    foreach (var trap in _traps)
                    {
                        colorCache = trap.SpriteRenderer.color;
                        colorCache.a = trapVisibleOpacity;
                        trap.SpriteRenderer.color = colorCache;
                        
                        trap.SpriteRenderer.enabled = true;
                        trap.Collider.enabled = true;
                    }
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        private struct ObstacleOrTrapInfo
        {
            public readonly GameObject GameObject;
            public readonly SpriteRenderer SpriteRenderer;
            public readonly Collider2D Collider;

            public ObstacleOrTrapInfo(GameObject obstacleObject, SpriteRenderer getComponent, Collider2D getComponent1)
            {
                GameObject = obstacleObject;
                SpriteRenderer = getComponent;
                Collider = getComponent1;
            }
        }
        
        public enum VisibilityState
        {
            ObstacleVisible,
            TrapVisible
        }
    }
}
