using UnityEngine;

public class DisappearOnTrigger : MonoBehaviour
{
    public string playerTag = "Bike";

    public TaskManager coinTaskManager;

    public float boostAmount = 12f;
    public float boostDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (coinTaskManager != null)
        {
            coinTaskManager.OnCoinCollected();
        }

        BikeMovement bike = other.GetComponent<BikeMovement>();
        if (bike != null)
        {
            bike.ApplySpeedBoost(boostAmount, boostDuration);
        }

        gameObject.SetActive(false);
    }
}