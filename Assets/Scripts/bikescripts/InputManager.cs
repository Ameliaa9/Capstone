using UnityEngine;

namespace KikiNgao.SimpleBikeControl
{
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [Tooltip("1 = Joystick 1, 2 = Joystick 2, etc.")]
        public int playerIndex = 1;

        public KeyCode enterExitKey = KeyCode.F;
        public KeyCode speedUpKey = KeyCode.LeftShift;

        [HideInInspector] public float horizontal;
        [HideInInspector] public float vertical;
        [HideInInspector] public bool enterExitVehicle;
        [HideInInspector] public bool speedUp;

        private bool hasMountedOnce = false;
        private static InputManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            horizontal = Input.GetAxis($"Joystick{playerIndex}Horizontal");
            vertical = -Input.GetAxis($"Joystick{playerIndex}Vertical");

            if (Mathf.Abs(horizontal) < 0.2f) horizontal = 0f;
            if (Mathf.Abs(vertical) < 0.2f) vertical = 0f;

            if (Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;

            horizontal = Mathf.Clamp(horizontal, -1f, 1f);
            vertical = Mathf.Clamp(vertical, -1f, 1f);

            if (!hasMountedOnce)
            {
                enterExitVehicle = true;
                hasMountedOnce = true;
            }
            else
            {
                enterExitVehicle = Input.GetKeyDown(enterExitKey);
            }

            speedUp = Input.GetKey(speedUpKey);
        }
    }
}

