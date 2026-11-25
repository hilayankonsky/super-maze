using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelHoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Texture2D handCursor;
    [SerializeField] Vector2 hotspot;

    static LevelHoverCursor current;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData is not ExtendedPointerEventData pointer ||
            pointer.pointerType != UIPointerType.MouseOrPen ||
            handCursor == null || !GetComponent<Button>().IsInteractable())
        {
            return;
        }

        current = this;
        Cursor.SetCursor(handCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData is ExtendedPointerEventData pointer && pointer.pointerType == UIPointerType.MouseOrPen)
        {
            ResetCursor();
        }
    }

    void OnDisable() => ResetCursor();

    void ResetCursor()
    {
        if (current != this)
        {
            return;
        }

        current = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
