using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorFollower : MonoBehaviour
{
    public Sprite defaultSprite;
    public Sprite movingSprite;
    public Sprite hoverSprite;

    public Image cursorImage;
    public Camera worldCamera;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        if (cursorImage != null)
            cursorImage.raycastTarget = false; // Ensure UI buttons remain clickable
    }

    void Update()
    {
        cursorImage.rectTransform.position = Input.mousePosition;
        UpdateCursorSprite();
    }

    void UpdateCursorSprite()
    {
        if (IsHoveringOverUIButton() || IsHoveringOverOutline())
        {
            cursorImage.sprite = hoverSprite;
        }
        else if (Input.GetMouseButton(1))
        {
            cursorImage.sprite = movingSprite;
        }
        else
        {
            cursorImage.sprite = defaultSprite;
        }
    }

    bool IsHoveringOverUIButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.GetComponent<Button>() != null)
                return true;
        }

        return false;
    }

    bool IsHoveringOverOutline()
    {
        Vector2 mouseWorldPos = worldCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Outline>() != null;
        }

        return false;
    }
}

