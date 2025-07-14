using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 500f;
    public float rotationSpeed = 100f;

    private Vector3 dragOrigin;

    void Update()
    {
        // Перемещение камеры (WASD или стрелки)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime, Space.World);

        // Зум колесиком мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.transform.Translate(Vector3.forward * scroll * zoomSpeed * Time.deltaTime, Space.Self);

        // Перетаскивание правой кнопкой мыши
        if (Input.GetMouseButtonDown(1))
            dragOrigin = Input.mousePosition;

        if (Input.GetMouseButton(1))
        {
            Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(-difference.x * moveSpeed, 0, -difference.y * moveSpeed);
            transform.Translate(move, Space.World);
            dragOrigin = Input.mousePosition;
        }

        // Вращение средней кнопкой (или Alt+ЛКМ)
        if (Input.GetMouseButton(2))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotX, Space.World);
        }
    }
}
