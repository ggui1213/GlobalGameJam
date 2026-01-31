using System;
using UnityEngine;

namespace Script
{
    public class Player : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Trap" || other.tag == "Wall")
            {
                PlayerSpawnManager.Instance.RespawnPlayer();
            }
        }
    }
}