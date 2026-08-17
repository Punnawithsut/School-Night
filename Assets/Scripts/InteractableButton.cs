using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableButton : MonoBehaviour
{
    public bool isUpButton = true; // true = ปุ่มขึ้น, false = ปุ่มลง
    public float interactDistance = 2.5f;
    public Camera playerCamera;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame || 
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Press();
            }
        }
    }

    void Press()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        GameManager.Instance.PlayerMadeChoice(!isUpButton);
    }
}