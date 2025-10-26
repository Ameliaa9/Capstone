using UnityEngine;
using UnityEngine.UI;

public class MinimapIconTracker : MonoBehaviour
{
    public RectTransform minimapRect;      
    public RectTransform iconRect;        
    public Transform target;             
    public Camera minimapCamera;     
    public Image arrowImage;               

    private Vector2 minimapSize;

    void Start()
    {
        minimapSize = minimapRect.sizeDelta / 2f; 
    }

    void Update()
    {
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(target.position);

        bool inView = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

        iconRect.gameObject.SetActive(inView);
        arrowImage.gameObject.SetActive(!inView);

        if (inView)
            return;

        Vector3 dir = (target.position - minimapCamera.transform.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrowImage.rectTransform.rotation = Quaternion.Euler(0, 0, -angle);

        Vector2 screenPos = new Vector2((viewportPos.x - 0.5f) * minimapRect.sizeDelta.x, (viewportPos.y - 0.5f) * minimapRect.sizeDelta.y);
        Vector2 clampedPos = Vector2.ClampMagnitude(screenPos, minimapSize.magnitude * 0.9f); 
        arrowImage.rectTransform.anchoredPosition = clampedPos;
    }
}
