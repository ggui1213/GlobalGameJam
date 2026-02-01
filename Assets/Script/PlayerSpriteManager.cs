using UnityEngine;

namespace Script
{
    public class PlayerSpriteManager : MonoBehaviour
    {
        [SerializeField] private Sprite defualtSprite;
        [SerializeField] private Sprite leftPressedSprite;
        [SerializeField] private Sprite rightPressedSprite;
        [SerializeField] private Sprite bothPressedSprite;
        [SerializeField] private Sprite defualtLevelSprite;
        [SerializeField] private Sprite leftPressedLevelSprite;
        [SerializeField] private Sprite rightPressedLevelSprite;
        [SerializeField] private Sprite bothPressedLevelSprite;
        [SerializeField] private bool debugLogs;
        
        [SerializeField] private SpriteRenderer levelSpriteRenderer;
        
        private SpriteRenderer _spriteRenderer;
        private bool _leftMouseButtonDown;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogError("[PlayerSpriteManager] No SpriteRenderer found on the GameObject.");
            }

            // Warn if any of the pressed/both sprites are missing — common cause of missing 'rightPressed' appearance
            if (debugLogs)
            {
                if (rightPressedSprite == null) Debug.LogWarning("[PlayerSpriteManager] rightPressedSprite is not assigned in the Inspector.");
                if (leftPressedSprite == null) Debug.LogWarning("[PlayerSpriteManager] leftPressedSprite is not assigned in the Inspector.");
                if (bothPressedSprite == null) Debug.LogWarning("[PlayerSpriteManager] bothPressedSprite is not assigned in the Inspector.");
                if (defualtSprite == null) Debug.LogWarning("[PlayerSpriteManager] defualtSprite is not assigned in the Inspector.");
            }

            UpdateSprite();
        }

        public bool LeftMouseButtonDown
        {
            get { return _leftMouseButtonDown; }
            set
            {
                _leftMouseButtonDown = value; 
                if (debugLogs) Debug.Log($"[PlayerSpriteManager] LeftMouseButtonDown set to {_leftMouseButtonDown}");
                UpdateSprite();
            }
        }
        
        private bool _rightMouseButtonDown;
        public bool RightMouseButtonDown
        {
            get { return _rightMouseButtonDown; }
            set
            {
                _rightMouseButtonDown = value; 
                if (debugLogs) Debug.Log($"[PlayerSpriteManager] RightMouseButtonDown set to {_rightMouseButtonDown}");
                UpdateSprite();
            }
        }
        
        
        
        private void UpdateSprite()
        {
            if (_spriteRenderer == null)
            {
                if (debugLogs) Debug.LogWarning("[PlayerSpriteManager] Cannot update sprite because SpriteRenderer is missing.");
                return;
            }

            if (debugLogs)
            {
                Debug.Log($"[PlayerSpriteManager] Updating sprite. L={_leftMouseButtonDown} R={_rightMouseButtonDown}");
                Debug.Log($"[PlayerSpriteManager] Sprites: default={(defualtSprite!=null)}, left={(leftPressedSprite!=null)}, right={(rightPressedSprite!=null)}, both={(bothPressedSprite!=null)}");
            }

            if (_leftMouseButtonDown && _rightMouseButtonDown)
            {
                if (bothPressedSprite != null)
                    _spriteRenderer.sprite = bothPressedSprite;
                if (bothPressedLevelSprite != null)
                    levelSpriteRenderer.sprite = bothPressedLevelSprite;
            }
            else if (_leftMouseButtonDown)
            {
                if (leftPressedSprite != null)
                    _spriteRenderer.sprite = leftPressedSprite;
                if (leftPressedLevelSprite != null)
                    levelSpriteRenderer.sprite = leftPressedLevelSprite;
            }
            else if (_rightMouseButtonDown)
            {
                if (rightPressedSprite != null)
                    _spriteRenderer.sprite = rightPressedSprite;
                if  (rightPressedLevelSprite != null)
                    levelSpriteRenderer.sprite = rightPressedLevelSprite;
            }
            else
            {
                if (defualtSprite != null)
                    _spriteRenderer.sprite = defualtSprite;
                if (defualtLevelSprite != null)
                    levelSpriteRenderer.sprite = defualtLevelSprite;
            }
        }

        public void ResetToDefaultSprite()
        {
            if (_spriteRenderer != null && defualtSprite != null)
            {
                _spriteRenderer.sprite = defualtSprite;
                if (debugLogs) Debug.Log("[PlayerSpriteManager] Reset to default sprite.");
            }
        }
    }
}
