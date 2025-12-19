using UnityEngine;
using UnityEngine.UI;

public class MinimapIconTracker : MonoBehaviour
{
    public RectTransform minimapRect;
    public RectTransform iconRect;
    public Transform target;
    public Camera minimapCamera;
    public Image arrowImage;

    public DeliverySystem deliverySystem;

    public bool alwaysShow = false;

    private Vector2 minimapHalfSize;
    public float edgePadding = 10f;

    void Start()
    {
        minimapHalfSize = minimapRect.sizeDelta * 0.5f;

        if (arrowImage != null)
            arrowImage.rectTransform.rotation = Quaternion.identity;

        if (alwaysShow)
        {
            if (iconRect != null) iconRect.gameObject.SetActive(true);
            if (arrowImage != null) arrowImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (target == null || minimapCamera == null) return;

        bool deliveryActive = alwaysShow ||
                              deliverySystem == null ||
                              deliverySystem.hasPackage;

        if (!deliveryActive)
        {
            if (iconRect != null) iconRect.gameObject.SetActive(false);
            if (arrowImage != null) arrowImage.gameObject.SetActive(false);
            return;
        }

        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(target.position);

        bool inView = viewportPos.z > 0 &&
                      viewportPos.x > 0 && viewportPos.x < 1 &&
                      viewportPos.y > 0 && viewportPos.y < 1;

        if (iconRect != null)
            iconRect.gameObject.SetActive(inView);

        if (arrowImage == null) return;

        arrowImage.gameObject.SetActive(true);

        arrowImage.rectTransform.rotation = Quaternion.identity;

        Vector2 screenPos = new Vector2(
            (viewportPos.x - 0.5f) * minimapRect.sizeDelta.x,
            (viewportPos.y - 0.5f) * minimapRect.sizeDelta.y
        );

        float maxX = minimapHalfSize.x - edgePadding;
        float maxY = minimapHalfSize.y - edgePadding;

        screenPos.x = Mathf.Clamp(screenPos.x, -maxX, maxX);
        screenPos.y = Mathf.Clamp(screenPos.y, -maxY, maxY);

        arrowImage.rectTransform.anchoredPosition = screenPos;
    }
}
