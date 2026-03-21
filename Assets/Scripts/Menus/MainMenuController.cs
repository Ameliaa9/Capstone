using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject startScreenCanvas;
        public GameObject helpScreenCanvas;

        private bool showingHelp = false;

        void Update()
        {
            if (!showingHelp && Input.anyKeyDown)
            {
                startScreenCanvas.SetActive(false);
                helpScreenCanvas.SetActive(true);

                showingHelp = true;
                return; 
            }

            if (showingHelp && Input.anyKeyDown)
            {
                helpScreenCanvas.SetActive(false);
                enabled = false;
            }
        }
    }
}