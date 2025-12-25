using UnityEngine;

public class DepotUIPause : MonoBehaviour
{
  //  [Header("Player Cursors")]
    //public JoystickCursor player1Cursor;
    //public JoystickCursor player2Cursor;

    void OnEnable()
    {
        Time.timeScale = 0f;

       // if (player1Cursor != null)
         //   player1Cursor.gameObject.SetActive(true);

      //  if (player2Cursor != null)
        //    player2Cursor.gameObject.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;

       // if (player1Cursor != null)
         //   player1Cursor.gameObject.SetActive(false);

        //if (player2Cursor != null)
          //  player2Cursor.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}