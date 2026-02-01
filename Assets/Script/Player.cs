using System;
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

        [SerializeField] Camera playerCamera;
        public Camera PlayerCamera => playerCamera;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Trap" || other.tag == "Wall")
            {
                MovementTrialManager.Instance.EndCurrentTrial();
                PlayerSpawnManager.Instance.RespawnPlayer();
            }
        }
    }
}