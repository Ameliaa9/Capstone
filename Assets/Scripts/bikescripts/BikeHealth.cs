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
    }
}
