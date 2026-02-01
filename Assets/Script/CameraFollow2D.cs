// csharp

using UnityEngine;

namespace Script
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        public string targetTag = "Player";
        public Vector3 offset = new Vector3(0f, 0f, -10f);
        public float smoothTime = 0.15f;

        private Camera cam;
        private Rigidbody2D target;
        private Vector2 physicsPos;
        private Vector3 smoothVelocity;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError("[CameraFollow2D] 缺少 Camera 组件");
                enabled = false;
                return;
            }

            // 强制使用正交相机
            cam.orthographic = true;
            // 保证初始朝向无旋转
            transform.rotation = Quaternion.identity;
        }

        void OnValidate()
        {
            // 在编辑器中也强制为正交，方便调试
            Camera c = GetComponent<Camera>();
            if (c != null)
            {
                c.orthographic = true;
            }
            transform.rotation = Quaternion.identity;
        }

        // 在渲染前再一次强制，避免被其它脚本在 LateUpdate 后修改
        void OnPreCull()
        {
            if (cam != null)
            {
                cam.orthographic = true;
            }
            transform.rotation = Quaternion.identity;
        }

        void Start()
        {
            FindTarget();
        }

        void Update()
        {
            if (target == null)
            {
                FindTarget();
            }
        }

        void FixedUpdate()
        {
            if (target != null)
            {
                physicsPos = target.position;
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPos = new Vector3(
                physicsPos.x + offset.x,
                physicsPos.y + offset.y,
                offset.z
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref smoothVelocity,
                smoothTime
            );

            transform.rotation = Quaternion.identity;
        }

        void FindTarget()
        {
            GameObject go = GameObject.FindGameObjectWithTag(targetTag);
            if (go == null) return;

            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning(
                    $"[CameraFollow2D] 找到带有 {targetTag} 标签的物体，但没有 Rigidbody2D"
                );
                return;
            }

            target = rb;
            physicsPos = rb.position;
        }

        public void SetTarget(Transform newTarget, bool snap = false)
        {
            if (newTarget == null) return;

            Rigidbody2D rb = newTarget.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning($"[CameraFollow2D] 尝试设置目标 {newTarget.name}，但没有 Rigidbody2D");
                return;
            }

            target = rb;
            physicsPos = rb.position;

            if (snap)
            {
                Vector3 targetPos = new Vector3(
                    physicsPos.x + offset.x,
                    physicsPos.y + offset.y,
                    offset.z
                );
                transform.position = targetPos;
                transform.rotation = Quaternion.identity;
                smoothVelocity = Vector3.zero;
            }
        }
    }
}
