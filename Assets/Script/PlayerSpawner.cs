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
        
        public GameObject RespawnPlayer()
        {
            // 1. Check if player exists, if not create one
            if (_player == null)
            {
                if (playerPrefab == null) return null;
                _player = Instantiate(playerPrefab);
                _player.name = playerPrefab.name;
            }

            _player.SetActive(true);

            // 2. Move to respawn point
            if (spawnPoint != null)
            {
                _player.transform.position = spawnPoint.position;
                _player.transform.rotation = spawnPoint.rotation;
            }

            // 3. Stop handling gravity
            var gravityHandler = _player.GetComponent<PlayerGravityHandler>();
            if (gravityHandler != null)
            {
                gravityHandler.SetGravityHandleEnable(false);
            }

            // 4. Reset RB2D velocities and gravity scale
            var rb2d = _player.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                rb2d.gravityScale = 0f;
            }

            // 5. Reset launcher state (clears force, enables input)
            var launcher = _player.GetComponent<DragLaunchBall2D>();
            if (launcher != null)
            {
                launcher.ResetLauncher();
            }
            
            var trailer = _player.GetComponent<PlayerTrailer>();
            if (trailer != null)
            {
                trailer.StopTrailing();
            }
            
            // FMOD: 玩家重生，BGM 切换回准备状态（高切效果）
            if (FMODAudioManager.Instance != null)
                FMODAudioManager.Instance.OnPlayerRespawn();
            
            return _player;
        }
    }
}