using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("Player Cursors")]
    public RectTransform player1Cursor;
    public RectTransform player2Cursor;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void ShowCursors(bool show)
    {
        if (player1Cursor) player1Cursor.gameObject.SetActive(show);
        if (player2Cursor) player2Cursor.gameObject.SetActive(show);

        // optionally hide system cursor
        Cursor.visible = !show;
    }
}
