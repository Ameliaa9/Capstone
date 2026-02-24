using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Clock : MonoBehaviour
{

    public bool showcurrentime;
    public TextMeshProUGUI timer;
    
    void Start()
    {
        StartCoroutine(currentime());
    }

    IEnumerator currentime()
    {
        while (showcurrentime == true)
        {
            timer.text = System.DateTime.Now.ToShortTimeString();

            yield return null;
        }
    }
}
