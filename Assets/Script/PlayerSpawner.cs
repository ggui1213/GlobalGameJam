using UnityEngine;

namespace Script
{
    public class PlayerSpawner : MonoBehaviour
    {
        public static PlayerSpawner Instance { get; private set; }
        [SerializeField] GameObject playerPrefab;
        [SerializeField] Transform spawnPoint;
        private GameObject _player;

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

        void Start()
        {
            RespawnPlayer();
        }
        
        public GameObject RespawnPlayer(GameObject player = null)
        {
            Debug.Log("[PlayerSpawner] RespawnPlayer called (player arg: " + (player != null ? player.name : "null") + ")");

            // decide which GameObject to operate on
            GameObject target = player ?? _player;

            // If no player available, instantiate one
            if (target == null)
            {
                if (playerPrefab == null)
                {
                    Debug.LogWarning("[PlayerSpawner] No player provided and playerPrefab is null. Cannot respawn.");
                    return null;
                }

                target = Instantiate(playerPrefab);
                target.name = playerPrefab.name;
                Debug.Log("[PlayerSpawner] Instantiated player prefab: " + target.name);
            }

            // Deactivate first to avoid physics/inputs during reposition
            if (target.activeSelf)
            {
                target.SetActive(false);
                Debug.Log("[PlayerSpawner] Deactivated target prior to respawn move: " + target.name);
            }

            // 2. Move to respawn point
            if (spawnPoint != null)
            {
                var rb2dForMove = target.GetComponent<Rigidbody2D>();
                if (rb2dForMove != null)
                {
                    rb2dForMove.position = spawnPoint.position;
                    rb2dForMove.rotation = spawnPoint.rotation.eulerAngles.z;
                    rb2dForMove.WakeUp();

                    target.transform.position = spawnPoint.position;
                    target.transform.rotation = spawnPoint.rotation;

                    Debug.Log("[PlayerSpawner] Moved target via Rigidbody2D to spawn point: " + spawnPoint.position);
                }
                else
                {
                    target.transform.position = spawnPoint.position;
                    target.transform.rotation = spawnPoint.rotation;
                    Debug.Log("[PlayerSpawner] Moved target via Transform to spawn point: " + spawnPoint.position);
                }
            }
            else
            {
                Debug.LogWarning("[PlayerSpawner] spawnPoint is null; player not moved.");
            }

            // 3. Stop handling gravity
            var gravityHandler = target.GetComponent<PlayerGravityHandler>();
            if (gravityHandler != null)
            {
                gravityHandler.SetGravityHandleEnable(false);
                Debug.Log("[PlayerSpawner] Gravity handler disabled on " + target.name);
            }

            // 4. Reset RB2D velocities and gravity scale
            var rb2d = target.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;

                rb2d.gravityScale = 0f;

                rb2d.Sleep();

                Debug.Log("[PlayerSpawner] Rigidbody linearVelocity cleared and gravityScale set to 0 on " + target.name);
            }

            // 5. Reset launcher state (clears force, enables input) - operate on components in children too
            var launchers = target.GetComponentsInChildren<DragLaunchBall2D>(true);
            if (launchers != null && launchers.Length > 0)
            {
                foreach (var l in launchers)
                {
                    if (l != null)
                    {
                        l.ResetLauncher();
                        Debug.Log("[PlayerSpawner] Launcher Reset on " + target.name + " (component: " + l.GetType().Name + ")");
                    }
                }
            }
            else
            {
                Debug.Log("[PlayerSpawner] No DragLaunchBall2D found on " + target.name + " or its children when respawning (ok if player doesn't use launcher)");
            }
            
            var trailers = target.GetComponentsInChildren<PlayerTrailer>(true);
            if (trailers != null && trailers.Length > 0)
            {
                foreach (var t in trailers)
                {
                    if (t != null)
                    {
                        t.StopTrailing();
                        Debug.Log("[PlayerSpawner] PlayerTrailer.StopTrailing invoked on " + target.name + " (component: " + t.GetType().Name + ")");
                    }
                }
            }
            else
            {
                Debug.Log("[PlayerSpawner] No PlayerTrailer found on " + target.name + " or its children when respawning");
            }

            // 6. Reactivate target after all resets
            target.SetActive(true);
            Debug.Log("[PlayerSpawner] Reactivated target after respawn: " + target.name);

            // remember this as our current player reference
            _player = target;

            // 7. Ensure camera snaps to the respawned player
            var playerComp = target.GetComponent<Player>();
            if (playerComp != null)
            {
                if (playerComp.PlayerCamera != null)
                {
                    var follow = playerComp.PlayerCamera.GetComponent<CameraFollow2D>();
                    if (follow != null)
                    {
                        follow.SetTarget(target.transform, true);
                        Debug.Log("[PlayerSpawner] CameraFollow2D target set to respawned player using PlayerCamera (snap)");
                    }
                    else
                    {
                        Debug.LogWarning("[PlayerSpawner] PlayerCamera does not have CameraFollow2D component. Can't snap camera.");
                    }
                }
                else
                {
                    Debug.LogWarning("[PlayerSpawner] Player.PlayerCamera is null on " + target.name + ". Not snapping camera (no Camera.main fallback per request).\nSet Player.playerCamera in the prefab or scene.");
                }
                
             }
             
             if (FMODAudioManager.Instance != null)
                FMODAudioManager.Instance.OnPlayerRespawn();

             return target;
         }
     }
 }
