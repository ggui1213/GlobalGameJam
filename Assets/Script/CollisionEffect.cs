using System;
using UnityEngine;

namespace Script
{
    public class CollisionEffect : MonoBehaviour
    {
        [SerializeField] Sprite[] collisionSprites1;
        [SerializeField] Sprite[] collisionSprites2;
        [SerializeField] float lifeTime = 0.5f;
        
        private SpriteRenderer spriteRenderer;
        private int effectType = 1; // 1 or 2
        private float spawnTime;
        private float lastFrameStartTime;

        public void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spawnTime = Time.time;
            lastFrameStartTime = Time.time;
            spriteRenderer.sprite = effectType == 1 ? collisionSprites1[0] : collisionSprites2[0];
        }

        private void Update()
        {
            if (Time.time > spawnTime + lifeTime)
                Destroy(gameObject);
            // Each frame have same amount of time to live
            if (Time.time > lastFrameStartTime + lifeTime / (effectType == 1 ? collisionSprites1.Length : collisionSprites2.Length))
            {
                lastFrameStartTime = Time.time;
                int frameIndex = (int)((Time.time - spawnTime) / (lifeTime / (effectType == 1 ? collisionSprites1.Length : collisionSprites2.Length)));
                if (effectType == 1 && frameIndex < collisionSprites1.Length)
                {
                    spriteRenderer.sprite = collisionSprites1[frameIndex];
                }
                else if (effectType == 2 && frameIndex < collisionSprites2.Length)
                {
                    spriteRenderer.sprite = collisionSprites2[frameIndex];
                }
            }
        }
    }
}