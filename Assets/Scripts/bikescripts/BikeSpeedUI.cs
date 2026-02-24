using UnityEngine;
using TMPro;

public class BikeSpeedUI : MonoBehaviour
{
    [Header("References")]
    public BikeMovement bike;
    public TMP_Text speedText;

    [Header("Display")]
    public bool useKmh = true;
    public float smoothSpeed = 6f;

    float displayedSpeed;

    void Update()
    {
        if (bike == null || speedText == null) return;

        float speed = bike.GetBikeSpeedKm();
        displayedSpeed = Mathf.Lerp(displayedSpeed, speed, Time.deltaTime * smoothSpeed);

        if (!useKmh)
            speedText.text = $"{Mathf.RoundToInt(displayedSpeed * 0.621371f)} mph";
        else
            speedText.text = $"{Mathf.RoundToInt(displayedSpeed)} km/h";
    }
}