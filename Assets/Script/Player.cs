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
                // FMOD: 播放失败音效（同时 BGM 切换到高切效果）
                if (FMODAudioManager.Instance != null)
                    FMODAudioManager.Instance.PlayFail();
                    
                MovementTrialManager.Instance.EndCurrentTrial();
                PlayerSpawner.Instance.RespawnPlayer();
            }
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // FMOD: 播放碰撞音效（碰到边框时）
            if (FMODAudioManager.Instance != null)
                FMODAudioManager.Instance.PlayCollisionHit();
        }
    }
}