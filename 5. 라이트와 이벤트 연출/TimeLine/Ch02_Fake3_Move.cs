using UnityEngine;

public class Ch02_Fake3_Move : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 input;



    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D 또는 ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S 또는 ↑/↓

        // X축과 Z축 방향 이동
        Vector3 move = new Vector3(h, 0, v).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}
