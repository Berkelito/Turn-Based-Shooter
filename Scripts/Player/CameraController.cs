using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float maxZoom;

    [SerializeField] private Transform orientation;

    private Vector3 clickedPosition = Vector3.zero;

    void Move() 
    {
        Vector2 direction = Vector2.zero;

        direction = playerInput.actions.FindAction("Move").ReadValue<Vector2>();

        direction.Normalize();

        direction *= moveSpeed * Time.deltaTime * 10;

        transform.position += orientation.forward * direction.y + orientation.right * direction.x;
    }

    void Rotate()
    {
        float rotation = playerInput.actions.FindAction("Rotate").ReadValue<float>();

        orientation.rotation = Quaternion.Euler(0, orientation.eulerAngles.y + rotation * rotationSpeed * Time.deltaTime, 0);

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, orientation.eulerAngles.y, transform.eulerAngles.z);
    }

    void MouseMove()
    {
        if (clickedPosition == Vector3.zero)
        {
            clickedPosition = References.Instance.CursorInWorld.transform.position;
        }
        else
        {
            Vector3 delta = References.Instance.CursorInWorld.transform.position - clickedPosition;
            delta.y = 0;
            
            transform.position -= delta;
            References.Instance.CinemaCamera.ForceCameraPosition(transform.position);
        }
    }

    void Zoom()
    {
        float zoom = playerInput.actions.FindAction("Zoom").ReadValue<float>();

        transform.position -= Vector3.up * zoom * zoomSpeed * Time.deltaTime;

        transform.position = new Vector3(transform.position.x,Mathf.Clamp(transform.position.y, 5, maxZoom), transform.position.z);
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0)
            return;
        Rotate();
        Move();
        Zoom();

        if (playerInput.actions.FindAction("MouseMove").ReadValue<float>() == 1)
            MouseMove();
        else
            clickedPosition = Vector3.zero;
    }
}
