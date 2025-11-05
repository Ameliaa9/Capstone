using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance { get => instance; }

        [SerializeField] InputManager inputManager;
        [SerializeField] EventManager eventManager;

        public InputManager GetInputManager => inputManager;
        public EventManager GetEventManager => eventManager;

        private void Awake()
        {
            if (instance != null) Destroy(gameObject);
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        protected void OnDestroy()
        {
            if (Instance == this) instance = null;
        }

        public static void LockCursor()
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.ShowCursors(false);
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public static void UnlockCursor()
        {
            if (CursorManager.Instance != null)
                CursorManager.Instance.ShowCursors(true);
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
