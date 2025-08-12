using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQTEHandler : MonoBehaviour
{
    private QTEUI2 qteUI;
    private QTEEffectManager qteEffect;
    private Animator animator;
    private EnemyAI enemy;
    private Rigidbody2D rb;
    private Transform player;
    private Vector3 startPosition;
    
    public float qteFreezeDuration = 3f;
    private bool isQTERunning = false;
    private bool successAnimEnded = false;
    private bool hasReturned = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyAI>();
        startPosition = transform.position;
        
    }

    private void Start()
    {
        qteUI = UIManager.Instance.QTE_UI_2;
        qteEffect = QTEEffectManager.Instance;
        player = GameManager.Instance.Player.transform;
    }

    public void StartQTE()
    {
        // 목숨이 1개 남았을 때 잡히면 즉시 게임오버
        if (PlayerLifeManager.Instance.GetCurrentLives() <= 1)
        {
            animator.SetTrigger("QTEFail");
            PlayerLifeManager.Instance.HandleGameOver();
            return;
        }
        
        if (!isQTERunning)
            StartCoroutine(StartQTESequence());
    }

    private IEnumerator StartQTESequence()
    {
        isQTERunning = true;
        hasReturned = false;
        successAnimEnded = false;
        
        enemy.ChangeState(enemy.QTEState);
        animator.SetTrigger("QTEIn");

        PossessionSystem.Instance.CanMove = false;
        // var playerCtrl = GameManager.Instance.Player.GetComponent<PlayerController>();
        // if (playerCtrl != null) playerCtrl.enabled = false;
        
        rb.velocity = Vector2.zero;

        if (qteEffect != null&& player != null)
        {
            qteEffect.playerTarget = player;
            qteEffect.enemyTarget = transform;
            qteEffect.StartQTEEffects();
        }

        if (qteUI != null)
        {
            qteUI.ResetState();  
            qteUI.gameObject.SetActive(true);
            qteUI.StartQTE();
        }
    
        yield return new WaitForSecondsRealtime(qteFreezeDuration);

        bool success = qteUI != null && qteUI.IsSuccess();

        if (success)
        {
            // if (playerCtrl != null) playerCtrl.enabled = true;
            PossessionSystem.Instance.CanMove = true;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), true);
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetTrigger("QTESuccess");
            PlayerLifeManager.Instance.LosePlayerLife();
            
            const float safetyMax = 5f; // 이벤트 누락 대비 상한
            float waited = 0f;
            while (!successAnimEnded && waited < safetyMax)
            {
                var st = animator.GetCurrentAnimatorStateInfo(0);
                // 상태명이 정확히 "QTESuccess"가 아니어도, 현재 상태가 비루프 & 0.99 지나면 종료로 간주
                if (!st.loop && st.normalizedTime >= 0.99f)
                {
                    break;
                }

                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            
            if (!successAnimEnded)
            {
                // 혹시 중간 이벤트가 누락됐으면 한 번은 원위치로 보정
                if (!hasReturned)
                {
                    hasReturned = true;
                    transform.position = startPosition;
                }

                qteEffect?.EndQTEEffects(true);
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), false);
                enemy.ChangeState(enemy.PatrolState);
                animator.updateMode = AnimatorUpdateMode.Normal;
                isQTERunning = false;
                successAnimEnded = true;
            }
        }
        else
        {
            animator.SetTrigger("QTEFail");
            qteEffect?.EndQTEEffects(true);
            PlayerLifeManager.Instance.HandleGameOver();
            isQTERunning = false;
            yield break;
        }

        qteEffect?.EndQTEEffects(true);
        qteUI?.gameObject.SetActive(false);
        qteUI?.ResetState(); 
        // isQTERunning = false;
    }

    public void OnQTESuccessAnimationMiddle()
    {
        if (!hasReturned)
        {
            hasReturned = true;
            transform.position = startPosition;
        }
        qteEffect?.EndQTEEffects(true);
    }
    
    public void OnQTESuccessAnimationEnd()
    {
        successAnimEnded = true;
        enemy.ChangeState(enemy.PatrolState);
        animator.updateMode = AnimatorUpdateMode.Normal;
        isQTERunning = false;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player"), false);
    }
    
    public bool IsRunning()
    {
        return isQTERunning;
    }
}
