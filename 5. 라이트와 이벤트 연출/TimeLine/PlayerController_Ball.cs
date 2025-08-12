using UnityEngine;

public class PlayerController_Ball : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private ParticleSystem moveParticle;
    [SerializeField] private GameObject player;

    private Rigidbody2D rb; // Rigidbody2D 변수 추가
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 가져오기

        if (moveParticle == null)
        {
            Debug.LogError("Move Particle System is not assigned in the inspector.");
        }
    }
     
    private void Update()
    {
        // 키 입력은 여기서 받고
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveInput = new Vector2(h, v);

        // 파티클 회전
        if (moveInput.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            var shape = moveParticle.shape;
            shape.rotation = new Vector3(0, 0, angle);
        }

        // 파티클 재생/정지
        if (moveInput.magnitude > 0.01f)
        {
            if (!moveParticle.isPlaying) moveParticle.Play();
        }
        else
        {
            if (moveParticle.isPlaying) moveParticle.Stop();
        }
    }

    private void FixedUpdate()
    {

        Vector2 newPos = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    public void changeSpedd()
    {
        transform.position = player.transform.position;
        moveSpeed = 20f;

    }
}
