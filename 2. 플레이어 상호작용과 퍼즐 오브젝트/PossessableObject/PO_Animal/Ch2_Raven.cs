using UnityEngine;

public class Ch2_Raven : MoveBasePossessable
{

    [SerializeField] private Animator highlightAnim;
    [SerializeField] private GameObject SandCastle; // 모래성
    [SerializeField] private Transform startSpot; // 스타트지점 (빙의해제시 되돌아감)

    private bool sandCastleBreakAble = false;
    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        hasActivated = true;
        rb = GetComponent<Rigidbody2D>();        
    }

    protected override void Update()
    {
        if (!hasActivated)
        {
            return;
        }
        if (sandCastleBreakAble)
        {
            Vector2 catPos = this.transform.position;
            catPos.y += 0.5f;
            // q_Key.SetActive(true);
        }

        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        { 
            if(isPossessed)
            {
                anim.SetTrigger("Attack");
            }
        }
    }

    void FixedUpdate()
    {
        if (!isPossessed)
        {
            Vector2 direction = (startSpot.position - transform.position);
            
            // 거리가 충분히 가까우면 멈춤
            if (direction.magnitude <= 0.05f)
            {
                anim.SetBool("Move", false);
                rb.velocity = Vector2.zero; // 혹시 남아있는 물리속도가 있을 경우
                return;
            }

            // 방향 벡터 정규화 후 이동
            direction.Normalize();
            rb.MovePosition(rb.position + direction * (moveSpeed-1) * Time.fixedDeltaTime);

            anim.SetBool("Move", true);

            // 방향에 따라 스프라이트 뒤집기
            spriteRenderer.flipX = (startSpot.position.x < transform.position.x);
        }
        
    }

    protected override void Move()
    {
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        // 이동 여부 판단
        bool isMoving = move.sqrMagnitude > 0.01f;

        if (anim != null)
        {
             if (isPossessed)
            {
                anim.SetBool("Move", true);
            }
            else
            {
                anim.SetBool("Move", isMoving);
            }
        }
        if (isMoving)
        {
            transform.position += move * moveSpeed * Time.deltaTime;

            // 좌우 Flip
            if (spriteRenderer != null && Mathf.Abs(h) > 0.01f)
            {
                spriteRenderer.flipX = h < 0f;
            }
        }
    }

    public override void OnQTESuccess()
    {
        SoulEnergySystem.Instance.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 문 근처에 있는지 확인
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if(collision.gameObject == SandCastle)
        {
            sandCastleBreakAble = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if(collision.gameObject == SandCastle)
        {
            sandCastleBreakAble = false;
        }
    }

    public override void Unpossess()
    {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 해제", 2f);
        isPossessed = false;
        PossessionStateManager.Instance.StartUnpossessTransition();
    }
}
