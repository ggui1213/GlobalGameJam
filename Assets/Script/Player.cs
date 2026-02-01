using UnityEngine;

namespace Script
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            // Prefer the explicitly assigned playerCamera. Do NOT use Camera.main per project convention.
            if (playerCamera != null)
            {
                var follow = playerCamera.GetComponent<CameraFollow2D>();
                if (follow != null)
                {
                    follow.SetTarget(this.transform, true);
                    Debug.Log("[Player] Snapped CameraFollow2D to player using Player.playerCamera in Start");
                }
                else
                {
                    Debug.LogWarning("[Player] playerCamera assigned but lacks CameraFollow2D component. Cannot snap camera.");
                }
            }
            else
            {
                Debug.LogWarning("[Player] playerCamera not assigned on prefab/scene. Per request we won't use Camera.main fallback.");
            }

            // Ensure player is moved to spawn point at Start. Prefer singleton, fallback to FindObjectOfType.
            if (PlayerSpawner.Instance != null)
            {
                PlayerSpawner.Instance.RespawnPlayer(this.gameObject);
                Debug.Log("[Player] Requested RespawnPlayer via PlayerSpawner.Instance in Start");
            }
            else
            {
                var sp = FindFirstObjectByType<PlayerSpawner>();
                if (sp != null)
                {
                    sp.RespawnPlayer(this.gameObject);
                    Debug.Log("[Player] Requested RespawnPlayer via FindObjectOfType fallback in Start");
                }
                else
                {
                    Debug.LogWarning("[Player] No PlayerSpawner found in scene during Start; player won't be moved to spawn point.");
                }
            }
        }

        [SerializeField] Camera playerCamera;
        public Camera PlayerCamera => playerCamera;

        void DoRespawn(Collider2D otherCollider = null)
        {
            var otherName = otherCollider != null ? otherCollider.gameObject.name : "(unknown)";
            Debug.Log($"[Player] Respawn requested due to collision with {otherName}");

            MovementTrialManager.Instance.EndCurrentTrial();

            if (PlayerSpawner.Instance != null)
            {
                PlayerSpawner.Instance.RespawnPlayer(this.gameObject);
            }
            else
            {
                var sp = FindFirstObjectByType<PlayerSpawner>();
                if (sp != null) sp.RespawnPlayer(this.gameObject);
                else Debug.LogWarning("[Player] No PlayerSpawner found to handle respawn");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Trap") || other.CompareTag("Wall"))
            {
                DoRespawn(other);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // FMOD: Play collision hit sound on any collision
            if (FMODAudioManager.Instance != null)
                FMODAudioManager.Instance.PlayCollisionHit();

            // Handle trap/wall collisions with respawn logic
            if (collision.collider.CompareTag("Trap") || collision.collider.CompareTag("Wall"))
            {
                Debug.Log($"[Player] OnCollisionEnter2D with {collision.gameObject.name}");
                if (FMODAudioManager.Instance != null)
                    FMODAudioManager.Instance.PlayFail();
                    
                MovementTrialManager.Instance.EndCurrentTrial();
                PlayerSpawner.Instance.RespawnPlayer();
                
                DoRespawn(collision.collider);
            }
        }
    }
}