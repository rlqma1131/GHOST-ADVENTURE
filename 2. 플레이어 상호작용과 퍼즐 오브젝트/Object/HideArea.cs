using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HideArea : BasePossessable
{
    [SerializeField] private AudioClip hideAreaEnterSFX;
    [SerializeField] private float energyConsumeCycle = 2f;
    [SerializeField] private int energyCost = 1;

    private Coroutine consumeCoroutine;
    protected bool isHiding = false;

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            isHiding = false;
            Unpossess();
        }
    }

    public override void OnQTESuccess()
    {
        Debug.Log("QTE 성공 - 빙의 완료");

        // 은신 효과음 (바스락)
        //SoundManager.Instance.PlaySFX(hideAreaEnterSFX);

        isHiding = true;

        PossessionStateManager.Instance.StartPossessionTransition();
        consumeCoroutine = StartCoroutine(ConsumeEnergyRoutine());
    }
    private IEnumerator ConsumeEnergyRoutine()
    {
        while (isHiding)
        {
            SoulEnergySystem.Instance.Consume(energyCost);
            yield return new WaitForSeconds(energyConsumeCycle);
        }
    }

    public override void Unpossess()
    {
        if (consumeCoroutine != null)
        {
            StopCoroutine(consumeCoroutine);
            consumeCoroutine = null;
        }

        isHiding = false;
        base.Unpossess();
    }

    public void OnMouseEnter() 
    {
        UIManager.Instance.HideAreaCursor();
    }
    public void OnMouseExit()
    {
        UIManager.Instance.SetDefaultCursor();
    }
}
