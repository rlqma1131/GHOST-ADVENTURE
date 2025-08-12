using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeleporterEnemyAI : EnemyAI
{
    [Header("Teleport Settings")]
    public float teleportInterval = 30f;
    public float teleportDistanceBehindPlayer = 5f;
    public Vector2 allowedMinBounds;
    public Vector2 allowedMaxBounds;

    private float teleportTimer;
    private bool initialDelayPassed = false;
    
    private Transform player;
    private BoxCollider2D col;

    protected override void Start()
    {
        base.Start();
        teleportTimer = teleportInterval;
        player = GameManager.Instance.Player.transform;
        col = GetComponent<BoxCollider2D>();
    }

    protected override void Update()
    {
        base.Update();

        if (QTEHandler != null && QTEHandler.IsRunning())
            return;
        
        if (currentState == InvestigateState)
            return;

        teleportTimer -= Time.deltaTime;

        if (teleportTimer <= 0f)
        {
            if (!initialDelayPassed)
            {
                initialDelayPassed = true;
                teleportTimer = teleportInterval;
                return;
            }
            
            if (IsPlayerInBoundary())
            {
                TeleportBehindPlayer();
            }

            teleportTimer = teleportInterval;
        }
    }

    private void TeleportBehindPlayer()
    {
        if (isTeleporting) return;
        isTeleporting = true;

        Vector2 movementDir;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb != null && playerRb.velocity != Vector2.zero)
            movementDir = playerRb.velocity.normalized;
        else
            movementDir = new Vector2(player.localScale.x, 0);

        Vector2 targetTeleportPos = (Vector2)player.position - movementDir * teleportDistanceBehindPlayer;
        targetTeleportPos.x = Mathf.Clamp(targetTeleportPos.x, allowedMinBounds.x, allowedMaxBounds.x);
        targetTeleportPos.y = Mathf.Clamp(targetTeleportPos.y, allowedMinBounds.y, allowedMaxBounds.y);

        if (Physics2D.OverlapCircle(targetTeleportPos, 0.5f, LayerMask.GetMask("Wall")))
            targetTeleportPos += Vector2.up * 1f;
        
        transform.position = targetTeleportPos;
        col.isTrigger = true;
        Animator.SetTrigger("Teleport");
    }
    
    public void OnTeleportAnimationEnd()
    {
        ChangeState(ChaseState);
        col.isTrigger = false;
        isTeleporting = false;
    }

    private bool IsPlayerInBoundary()
    {
        return player.position.x >= allowedMinBounds.x && player.position.x <= allowedMaxBounds.x &&
               player.position.y >= allowedMinBounds.y && player.position.y <= allowedMaxBounds.y;
    }
}
