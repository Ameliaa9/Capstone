using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultButtonOnEnable : MonoBehaviour
{
    public GameObject defaultButton;

    private void OnEnable()
    {
        if (EventSystem.current == null || defaultButton == null) return;

        EventSystem.current.SetSelectedGameObject(null);          
        EventSystem.current.SetSelectedGameObject(defaultButton);  
    }
}
