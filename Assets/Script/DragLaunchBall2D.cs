using UnityEngine;
using UnityEngine.Events;

public class DragLaunchBall2D : MonoBehaviour
{
    public float forceMultiplier = 10f;

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
                line.enabled = true;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = dragStartWorld - mouseWorld;

            currentForce = dragVector * forceMultiplier;

            line.SetPosition(0, dragStartWorld);
            line.SetPosition(1, dragStartWorld + dragVector);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            // ??? currentForce
        }
    }

    void HandleLaunchInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(currentForce, ForceMode2D.Impulse);
            line.enabled = false;
            enabled = false; // ?????
            
            onLaunch.Invoke();
        }
    }

    public UnityEvent onLaunch;
}