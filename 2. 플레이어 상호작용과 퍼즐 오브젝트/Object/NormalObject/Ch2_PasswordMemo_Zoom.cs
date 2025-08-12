using UnityEngine;

public class Ch2_PasswordMemo_Zoom : MonoBehaviour
{
    [SerializeField] private GameObject highlight;

    private Vector3 offset;
    private bool isDragging = false;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        highlight?.SetActive(false);
    }

    void OnMouseEnter()
    {
        if (!isDragging)
            highlight?.SetActive(true);
    }

    void OnMouseExit()
    {
        if (!isDragging)
            highlight?.SetActive(false);
    }

    void OnMouseDown()
    {
        isDragging = true;
        highlight?.SetActive(false);

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        offset = transform.position - mouseWorldPos;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        Vector3 targetPos = mouseWorldPos + offset;

        // 카메라 바깥으로 나가지 않도록 제한
        Vector3 viewPos = mainCam.WorldToViewportPoint(targetPos);
        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);

        Vector3 clampedWorldPos = mainCam.ViewportToWorldPoint(viewPos);
        clampedWorldPos.z = transform.position.z;

        transform.position = clampedWorldPos;
    }

    void OnMouseUp()
    {
        isDragging = false;
        highlight?.SetActive(true);
    }
}
