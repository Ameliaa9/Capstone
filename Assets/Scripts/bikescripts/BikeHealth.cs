using TMPro;
using UnityEngine;

public class BikeHealth : MonoBehaviour
{
    [SerializeField]
    private BikeMovement player;
    [SerializeField]
    private RectTransform healthBar;
    [SerializeField]
    private TextMeshProUGUI value;
    [SerializeField]
    private Transform respawnPoint;

    [Header("Death Popup")]
    [SerializeField]
    private TextMeshProUGUI deathPopup;
    [SerializeField]
    private float popupDuration = 2f;

    public float Health;
    public float maxHealth;

    public float width;
    public float height;

    public AudioClip DeathSFX;
    [SerializeField] private AudioSource audioSource;

    public void Start()
    {
        width = healthBar.sizeDelta.x;
        height = healthBar.sizeDelta.y;

        SetMaxHealth(player.maxHealth);
        Health = maxHealth;

        float newWidth = (Health / maxHealth) * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
        value.text = Health.ToString();

        if (deathPopup != null)
            deathPopup.gameObject.SetActive(false);
    }

    public void SetMaxHealth(float setMaxHealth)
    {
        maxHealth = setMaxHealth;
    }

    public void SetHealth(float setHealth)
    {
        Health += setHealth;
        Health = Mathf.Clamp(Health, 0f, maxHealth);

        float newWidth = (Health / maxHealth) * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
        value.text = Health.ToString();

        if (Health <= 0)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        if (audioSource != null && DeathSFX != null)
        {
            audioSource.PlayOneShot(DeathSFX, 2f);
        }

        if (player != null && respawnPoint != null)
        {
            // teleport bike
            var rb = player.GetRigidbody();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;
        }

        ShowDeathPopup();

        // reset health back to full
        Health = maxHealth;

        float newWidth = (Health / maxHealth) * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
        value.text = Health.ToString();
    }

    private void ShowDeathPopup()
    {
        if (deathPopup == null) return;

        deathPopup.gameObject.SetActive(true);
        deathPopup.text = "WASTED!";

        CancelInvoke(nameof(HideDeathPopup));
        Invoke(nameof(HideDeathPopup), popupDuration);
    }

    private void HideDeathPopup()
    {
        if (deathPopup != null)
            deathPopup.gameObject.SetActive(false);
    }
}