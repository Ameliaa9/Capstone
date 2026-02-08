using UnityEngine;

public class GooseCollision : MonoBehaviour
{
    public GameObject featherUI;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Bike"))
        {
            featherUI.SetActive(true);
            Invoke(nameof(Hide), 2f);
        }
    }

    void Hide()
    {
        featherUI.SetActive(false);
    }
}
