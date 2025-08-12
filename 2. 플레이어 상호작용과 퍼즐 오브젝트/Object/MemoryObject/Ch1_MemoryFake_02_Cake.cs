using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ch1_MemoryFake_02_Cake : MemoryFragment
{
    private Animator anim;
    private ParticleSystem _particleSystem;
    private Light2D _light;

    void Start()
    {
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _light = GetComponentInChildren<Light2D>();
    }

    public void ActivateCake()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan()
    {
        ChapterEndingManager.Instance.CollectCh1Clue("H");


        anim.SetTrigger("Show");

        AfterScanEffect(); // 애니메이션 재생 후 효과 실행

        Debug.Log("[Cake] AfterScan 호출됨");

        Invoke("HighlightOff", 1f);

        base.AfterScan();
    }

    void HighlightOff()
    {
        highlight.SetActive(false); // 하이라이트 비활성화
        Debug.Log("하이라이트 오프함");
    }

    protected override void PlusAction()
    {
        UIManager.Instance.PromptUI.ShowPrompt("H");
    }

    private void AfterScanEffect()
    {
        _particleSystem.Play();
        _light.enabled = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Player") && !SaveManager.IsPuzzleSolved("후라이팬"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("쥐를 먼저 쫒아내야겠어");
        }
        else if(other.CompareTag("Player") && SaveManager.IsPuzzleSolved("후라이팬"))
        {
            TutorialManager.Instance.Show(TutorialStep.Cake_Prompt);
        } 
    }
}
