using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopCameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;

    private Vector2 look;
    private Vector2 move;

    private void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            look += delta * lookSpeed * Time.deltaTime;
            transform.localRotation = Quaternion.Euler(-look.y, look.x, 0f);
        }

        Vector3 movement = new Vector3(move.x, 0f, move.y) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }
}
