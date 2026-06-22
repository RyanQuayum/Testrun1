using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public MobileCameraController cameraController;
    public Camera mainCamera;

    public float tapMoveThreshold = 10f;

    private Vector2 lastPointerPosition;
    private Vector2 pressStartPosition;
    private bool isDragging;

    void Update()
    {
        HandleDrag();
        HandleZoom();
        HandleInteraction();
    }

    void HandleDrag()
    {
        if (IsPinching())
        {
            isDragging = false;
            return;
        }

        bool pressed = TryGetPointerPosition(out Vector2 pointerPosition);

        if (pressed && !isDragging)
        {
            lastPointerPosition = pointerPosition;
            pressStartPosition = pointerPosition;
            isDragging = true;
        }
        else if (pressed && isDragging)
        {
            Vector2 delta = pointerPosition - lastPointerPosition;

            cameraController.Pan(delta);

            lastPointerPosition = pointerPosition;
        }
        else if (!pressed)
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
                cameraController.Zoom(scroll);
        }

        if (Touchscreen.current != null)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];

            if (touch0.press.isPressed && touch1.press.isPressed)
            {
                Vector2 p0 = touch0.position.ReadValue();
                Vector2 p1 = touch1.position.ReadValue();

                Vector2 p0Prev = p0 - touch0.delta.ReadValue();
                Vector2 p1Prev = p1 - touch1.delta.ReadValue();

                float previousDistance = Vector2.Distance(p0Prev, p1Prev);
                float currentDistance = Vector2.Distance(p0, p1);

                float pinchAmount = currentDistance - previousDistance;

                cameraController.Zoom(pinchAmount);
            }
        }
    }

    void HandleInteraction()
    {
        if (IsPinching())
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 releasePosition = Mouse.current.position.ReadValue();

            if (Vector2.Distance(pressStartPosition, releasePosition) <= tapMoveThreshold)
                CheckInteraction(releasePosition);
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            Vector2 releasePosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            if (Vector2.Distance(pressStartPosition, releasePosition) <= tapMoveThreshold)
                CheckInteraction(releasePosition);
        }
    }

    bool TryGetPointerPosition(out Vector2 pointerPosition)
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.isPressed)
        {
            pointerPosition =
                Mouse.current.position.ReadValue();
            return true;
        }

        pointerPosition = Vector2.zero;
        return false;
    }

    bool IsPinching()
    {
        if (Touchscreen.current == null)
            return false;

        int touchCount = 0;

        foreach (var touch in Touchscreen.current.touches)
        {
            if (touch.press.isPressed)
                touchCount++;
        }

        return touchCount >= 2;
    }

    void CheckInteraction(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Interactable interactable =
                hit.collider.GetComponent<Interactable>();

            if (interactable != null)
                interactable.Interact();
        }
    }
}