using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float dragSpeed = 2f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 dragOrigin;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleMouseDrag();
        HandleZoom();
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPosition = transform.position + difference;

            if (useBounds)
                newPosition = ClampPositionToBounds(newPosition);

            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);

            if (useBounds)
            {
                Vector3 clampedPosition = ClampPositionToBounds(transform.position);
                transform.position = new Vector3(clampedPosition.x, clampedPosition.y, transform.position.z);
            }
        }
    }

    private Vector3 ClampPositionToBounds(Vector3 position)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        position.x = Mathf.Clamp(position.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        position.y = Mathf.Clamp(position.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        return position;
    }
}



