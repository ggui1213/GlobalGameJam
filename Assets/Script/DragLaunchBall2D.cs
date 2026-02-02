using UnityEngine;
using UnityEngine.Events;

namespace Script
{
    public class DragLaunchBall2D : MonoBehaviour
    {
        public float forceMultiplier = 10f;
        [SerializeField, Tooltip("Maximum drag distance in world units. Drag vector will be clamped to this length.")]
        private float maxDragDistance = 3f;

        Rigidbody2D rb;
        LineRenderer line;
        Camera _cachedCamera;

        bool isDragging = false;
        Vector2 dragStartWorld;
        Vector2 currentForce;

        Camera GetCamera()
        {
            if (Player.Instance != null && Player.Instance.PlayerCamera != null)
                return Player.Instance.PlayerCamera;

            if (_cachedCamera != null) return _cachedCamera;
            _cachedCamera = Camera.main;
            return _cachedCamera;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            line = GetComponent<LineRenderer>();

            if (rb == null)
            {
                Debug.LogWarning($"{nameof(DragLaunchBall2D)} on {gameObject.name} needs a Rigidbody2D.");
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            if (line != null) line.enabled = false;
            _cachedCamera = Camera.main;
        }

        void Update()
        {
            HandleDragInput();
            HandleLaunchInput();
        }

        void HandleDragInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var cam = GetCamera();
                if (cam == null) return;

                Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

                if (hit != null && (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)))
                {
                    isDragging = true;
                    dragStartWorld = rb != null ? rb.position : (Vector2)transform.position;
                    if (line != null) line.enabled = true;
                    
                    // FMOD: 播放蓄力音效
                    if (FMODAudioManager.Instance != null)
                        FMODAudioManager.Instance.PlayEjectionCharging();
                }
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                var cam = GetCamera();
                if (cam == null) return;

                Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dragVector = dragStartWorld - mouseWorld;

                if (maxDragDistance > 0f)
                {
                    float mag = dragVector.magnitude;
                    if (mag > maxDragDistance)
                        dragVector = dragVector.normalized * maxDragDistance;
                }

                currentForce = dragVector * forceMultiplier;

                if (line != null)
                {
                    line.SetPosition(0, dragStartWorld);
                    line.SetPosition(1, dragStartWorld + dragVector);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }

        void HandleLaunchInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                onLaunch?.Invoke();

                // FMOD: 播放发射音效（同时 BGM 切换到全频段）
                if (FMODAudioManager.Instance != null)
                    FMODAudioManager.Instance.PlayEjectionReleased();

                rb.AddForce(currentForce, ForceMode2D.Impulse);
                line.enabled = false;
                enabled = false; // ????
            }
        }

        public void ResetLauncher()
        {
            enabled = true;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            isDragging = false;
            currentForce = Vector2.zero;
            if (line != null) line.enabled = false;
        }

        public UnityEvent onLaunch;
    }
}
