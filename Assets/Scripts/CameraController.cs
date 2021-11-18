using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float speed = 10.0f;
    [SerializeField] float minSize = 1f;

    private new Camera camera;
    private Vector3 mouseBegin;

    private void Start()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * speed;

        if (scroll != 0)
        {
            camera.orthographicSize = Mathf.Max(minSize, camera.orthographicSize - scroll);
        }

        if (Input.GetMouseButtonDown(1))
        {
            mouseBegin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseBegin.z = transform.position.z;

        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position -= (mousePos - mouseBegin);
        }
    }
}
