using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 10f;
    public float xSpeed = 120f;
    public float ySpeed = 120f;

    private float x = 0f;
    private float y = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
        y = Mathf.Clamp(y, -20, 80);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0, 0, -distance) + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}
