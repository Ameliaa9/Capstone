using KikiNgao.SimpleBikeControl;
using TMPro;
using UnityEngine;

public class BikeHealth : MonoBehaviour
{
    [SerializeField]
    private SimpleBike player;
    [SerializeField]
    private RectTransform healthBar;
    [SerializeField]
    private TextMeshProUGUI value;
    [SerializeField] private Transform respawnPoint;   


    public float Health;
    public float maxHealth;

    public float width;
    public float height;

    public void Start()
    {
        SetMaxHealth(player.bikeHealth);
        Health = maxHealth;

    }


    public void SetMaxHealth(float setMaxHealth)
    {
        maxHealth = setMaxHealth;
    }

    public void SetHealth(float setHealth)
    {
        Health += setHealth;
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
        // teleport bike
        if (player != null && respawnPoint != null)
        {
            var rb = player.GetRigidbody();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;
        }

        // reset health back to full
        Health = maxHealth;          
        float newWidth = (Health / maxHealth) * width;
        healthBar.sizeDelta = new Vector2(newWidth, height);
        value.text = Health.ToString();
    }

}
