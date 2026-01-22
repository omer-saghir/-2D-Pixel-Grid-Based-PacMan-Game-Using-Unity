using UnityEngine;

public class SoftwareCursor : MonoBehaviour
{
    public RectTransform cursorRect;

    void Start()
    {
        // This hides the standard operating system cursor
        Cursor.visible = false;

        // This keeps the mouse from leaving the game window (useful for pixel art games)
        Cursor.lockState = CursorLockMode.Confined; 
    }

    void Update()
    {
        // This makes your UI Image follow your mouse perfectly
        cursorRect.position = Input.mousePosition;
    }
}