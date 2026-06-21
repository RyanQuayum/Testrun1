using UnityEngine;
using UnityEngine.InputSystem;

public class MobileCameraController : MonoBehaviour
{
    [Header("Pan")]
    public float dragSpeed = 0.01f;

    [Header("Zoom")]
    public float zoomSpeed = 0.01f;
    public float minZoomDistance = 5f;
    public float maxZoomDistance = 30f;

    [Header("Bounds")]
    public float minX = -200f;
    public float maxX = 200f;
    public float minZ = -200f;
    public float maxZ = 200f;

    private Vector2 lastPointerPosition;
    private bool isDragging;

    private Vector3 zoomOrigin;
    private float currentZoomDistance = 15f;

    void Start()
    {
        zoomOrigin = transform.position;
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    void HandlePan()
    {
        // Prevent panning while pinching
        if (Touchscreen.current != null)
        {
            int touches = 0;

            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                    touches++;
            }

            if (touches >= 2)
            {
                isDragging = false;
                return;
            }
        }

        bool pressed = false;
        Vector2 pointerPosition = Vector2.zero;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            pressed = true;
            pointerPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.isPressed)
        {
            pressed = true;
            pointerPosition =
                Mouse.current.position.ReadValue();
        }

        if (pressed && !isDragging)
        {
            lastPointerPosition = pointerPosition;
            isDragging = true;
        }
        else if (pressed && isDragging)
        {
            Vector2 delta = pointerPosition - lastPointerPosition;

            Pan(delta);

            lastPointerPosition = pointerPosition;
        }
        else if (!pressed)
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        // Mouse wheel in editor
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
            {
                Zoom(scroll * zoomSpeed);
            }
        }

        // Pinch zoom
        if (Touchscreen.current != null)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];

            if (touch0.press.isPressed &&
                touch1.press.isPressed)
            {
                Vector2 p0 = touch0.position.ReadValue();
                Vector2 p1 = touch1.position.ReadValue();

                Vector2 p0Prev = p0 - touch0.delta.ReadValue();
                Vector2 p1Prev = p1 - touch1.delta.ReadValue();

                float previousDistance =
                    Vector2.Distance(p0Prev, p1Prev);

                float currentDistance =
                    Vector2.Distance(p0, p1);

                float pinchAmount =
                    currentDistance - previousDistance;

                Zoom(pinchAmount * zoomSpeed);
            }
        }
    }

    internal void Pan(Vector2 delta)
    {
        Vector3 right = transform.right;
        Vector3 forward =
            Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        Vector3 movement =
            (-right * delta.x - forward * delta.y) * dragSpeed;

        transform.position += movement;
        zoomOrigin += movement;

        ClampPosition();
    }

    internal void Zoom(float amount)
    {
        currentZoomDistance -= amount;

        currentZoomDistance = Mathf.Clamp(
            currentZoomDistance,
            minZoomDistance,
            maxZoomDistance
        );

        transform.position =
            zoomOrigin - transform.forward * currentZoomDistance;

        ClampPosition();
    }

    void ClampPosition()
    {
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            transform.position.y,
            Mathf.Clamp(transform.position.z, minZ, maxZ)
        );
    }
}