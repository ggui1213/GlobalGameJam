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

        bool isDragging = false;
        Vector2 dragStartWorld;
        Vector2 currentForce;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            line = GetComponent<LineRenderer>();

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            if (line != null) line.enabled = false;
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
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

                if (hit != null && hit.gameObject == gameObject)
                {
                    isDragging = true;
                    dragStartWorld = rb.position;
                    if (line != null) line.enabled = true;
                }
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dragVector = dragStartWorld - mouseWorld;

                // Clamp the drag vector to the configured maximum distance
                if (maxDragDistance > 0f)
                {
                    float mag = dragVector.magnitude;
                    if (mag > maxDragDistance)
                    {
                        dragVector = dragVector.normalized * maxDragDistance;
                    }
                }

                currentForce = dragVector * forceMultiplier;

                line.SetPosition(0, dragStartWorld);
                line.SetPosition(1, dragStartWorld + dragVector);
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                // keep currentForce so launch can be done with space; hide line if desired
                if (line != null) line.enabled = false;
            }
        }

        void HandleLaunchInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Invoke onLaunch first so external listeners (like GravityHandler) can prepare
                // e.g. turning gravity back on (Dynamic body) so AddForce works.
                onLaunch.Invoke();

                rb.AddForce(currentForce, ForceMode2D.Impulse);
                line.enabled = false;
                enabled = false; // ????
            }
        }

        public void ResetLauncher()
        {
            enabled = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isDragging = false;
            currentForce = Vector2.zero;
            if (line != null) line.enabled = false;
        }

        public UnityEvent onLaunch;
    }
}