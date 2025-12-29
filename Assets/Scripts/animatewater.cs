using UnityEngine;

public class animateWater : MonoBehaviour
{
    [Header("Scroll Speed")]
    public float speedX = 0f;
    public float speedY = -0.2f;  // negative to move downward

    [Header("Texture Property")]
    // For URP/HDRP Lit: use "_BaseMap"
    // For Built-in Standard: use "_MainTex"
    public string textureProperty = "_BaseMap";

    private Renderer rend;
    private Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Start()
    {
        if (rend != null && rend.material != null)
        {
            offset = rend.material.GetTextureOffset(textureProperty);
        }
    }

    void Update()
    {
        if (rend == null || rend.material == null) return;

        offset.x = Mathf.Repeat(offset.x + speedX * Time.deltaTime, 1f);
        offset.y = Mathf.Repeat(offset.y + speedY * Time.deltaTime, 1f);

        rend.material.SetTextureOffset(textureProperty, offset);
    }
}
