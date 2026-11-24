using UnityEngine;

// UniformScaleFitter.cs (Assets/Scripts/Game): presentation only.
// Scales a fixed-size layout (for example the Home level map) uniformly so it fits inside
// its parent rect on any portrait aspect ratio, without stretching its artwork.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UniformScaleFitter : MonoBehaviour
{
    [SerializeField] float maxScale = 1.15f;

    RectTransform rect;

    void OnEnable() => Fit();

    void Update() => Fit();

    public void Fit()
    {
        if (rect == null)
        {
            rect = (RectTransform)transform;
        }

        RectTransform parent = rect.parent as RectTransform;
        Vector2 size = rect.rect.size;

        if (parent == null || size.x <= 0f || size.y <= 0f)
        {
            return;
        }

        Vector2 available = parent.rect.size;
        float scale = Mathf.Min(available.x / size.x, available.y / size.y, maxScale);
        scale = Mathf.Max(scale, 0.01f);

        if (!Mathf.Approximately(rect.localScale.x, scale) || !Mathf.Approximately(rect.localScale.y, scale))
        {
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
