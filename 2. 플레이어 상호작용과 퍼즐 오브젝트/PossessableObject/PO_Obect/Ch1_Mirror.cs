using UnityEngine;

public class Ch1_Mirror : MonoBehaviour
{
    [SerializeField] private Ch1_Shower shower;
    [SerializeField] private SpriteRenderer letterW;
    [SerializeField] private float fogDuration = 15f;

    private float fogTime = 0f;
    private bool revealed = false;
    private bool ShowPrompt = false;

    private void Start()
    {
        SetAlpha(0f);
    }

    private void Update()
    {
        if (letterW == null || shower == null || revealed)
            return;

        if (shower.IsHotWater)
        {
            fogTime += Time.deltaTime;
            fogTime = Mathf.Min(fogTime, fogDuration); // 최대값 제한
        }
        else
        {
            fogTime -= Time.deltaTime;
            fogTime = Mathf.Max(fogTime, 0f); // 0 이하로 떨어지지 않도록
        }

        float alpha = Mathf.Clamp01(fogTime / fogDuration);
        SetAlpha(alpha);
        if (!revealed && alpha >= 0.3f && !ShowPrompt)
        {
            UIManager.Instance.PromptUI.ShowPrompt("…글씨…?"); 
            ShowPrompt = true;      
        }

        if (!revealed && alpha >= 0.5f)
        {
            revealed = true;
            ChapterEndingManager.Instance.CollectCh1Clue("W");
            SaveManager.MarkPuzzleSolved("욕실");
        }
    }

    private void SetAlpha(float alpha)
    {
        Color alphaLetterW = this.letterW.color;
        alphaLetterW.a = alpha;
        this.letterW.color = alphaLetterW;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && SaveManager.IsPuzzleSolved("욕실"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("W", 3f);       
        }
    }
}
