using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject startScreenCanvas;

        void Update()
        {
           
            if (Input.anyKeyDown)
            {
                startScreenCanvas.SetActive(false);
                enabled = false; 
            }
        }
    }
}
