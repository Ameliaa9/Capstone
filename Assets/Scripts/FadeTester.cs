using System.Collections;
using UnityEngine;

public class FadeTester : MonoBehaviour
{
    public ScreenFader screenFader;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine(screenFader.FadeOut());
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(screenFader.FadeIn());
        }
    }
}
