using System;
using UnityEngine;

namespace Script
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        public static PlayerSpawnManager Instance { get; private set; }
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
            if (_player != null)
            {
                if (spawnPoint != null)
                {
                    _player.transform.position = spawnPoint.position;
                    _player.transform.rotation = spawnPoint.rotation;
                    // reset physics velocities if present
                    var rb2d = _player.GetComponent<Rigidbody2D>();
                    if (rb2d != null)
                    {
                        rb2d.linearVelocity = Vector2.zero;
                        rb2d.angularVelocity = 0f;
                    }
                    var rb = _player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }

                _player.GetComponent<DragLaunchBall2D>().enabled = true;
                _player.GetComponent<PlayerGravityHandler>().SetGravityHandleEnable(false);
                _player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                
                return _player;
            }
             
            if (playerPrefab == null) return null;
            if (spawnPoint == null) return null;

            _player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            if (_player != null)
            {
                // Ensure the spawned object is active and identifiable in hierarchy
                _player.SetActive(true);
                _player.name = playerPrefab.name;
                // Optionally ensure it has Player tag if such convention exists
                // if (!string.IsNullOrEmpty("Player") && _player.tag != "Player") _player.tag = "Player";
            }
            
            return _player;
        }
    }
}