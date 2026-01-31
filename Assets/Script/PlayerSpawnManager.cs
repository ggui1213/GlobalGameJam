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
                Destroy(_player);
            
            if (playerPrefab != null && spawnPoint != null)
                _player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            else
                throw new NullReferenceException("PlayerPrefab or SpawnPoint is not assigned.");
            
            return _player;
        }
    }
}