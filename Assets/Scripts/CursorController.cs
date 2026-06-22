using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    public static bool IsPaused = false; 

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        if (IsPaused) return; 

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.None)
                LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}