using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// PressHighlight.cs (Assets/Scripts/Game): presentation only.
// Shows the highlight graphic only while a finger or mouse is held down over this object.
// It hides again on release or when the press is cancelled by sliding off.
// It does not change the Button's OnClick or any navigation.
public class PressHighlight : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Graphic highlight;
    [SerializeField] float fadeDuration = 0.06f;

    Selectable selectable;
    bool pointerDown;
    bool pointerInside;

    void Awake() => selectable = GetComponent<Selectable>();

    void OnEnable()
    {
        pointerDown = false;
        pointerInside = false;
        Apply(true);
    }

    void OnDisable()
    {
        pointerDown = false;
        Apply(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        pointerDown = true;
        pointerInside = true;
        Apply(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        Apply(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        Apply(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        Apply(false);
    }

    void Apply(bool instant)
    {
        if (highlight == null)
        {
            return;
        }

        bool interactable = selectable == null || selectable.IsInteractable();
        float alpha = pointerDown && pointerInside && interactable ? 1f : 0f;
        highlight.CrossFadeAlpha(alpha, instant ? 0f : fadeDuration, true);
    }
}
