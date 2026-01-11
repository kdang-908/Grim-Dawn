using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursor;
    public Vector2 hotSpot = Vector2.zero;

    void Awake()
    {
        Cursor.SetCursor(cursor, hotSpot, CursorMode.Auto);
    }
}
