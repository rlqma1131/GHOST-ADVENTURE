using UnityEngine;

public class PlayerController_ghost : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    public Animator animator { get; private set; }

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 키 입력 받기
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveInput = new Vector2(h, v);

        // 회전 방향 설정 (좌우 반전)
        if (h > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (h < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // 애니메이션 상태 설정
        bool isMoving = moveInput.magnitude > 0.01f;
        animator.SetBool("Move", isMoving);
    }

    private void FixedUpdate()
    {
        // Rigidbody2D로 이동 처리
        Vector2 newPos = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}
