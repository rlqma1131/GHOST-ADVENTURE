using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerMusicPuzzle : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float musicSegmentDuration = 4f;
    [SerializeField] private float musicPauseDuration = 1.5f;

    private Coroutine musicRoutine;
    private bool isPlaying = false;
    private float songStartTime;
    private bool movedDuringPlay = false;
    private int returnCount;

    [SerializeField] private Transform returnPoint;

    public void StartPuzzle()
    {
        if (musicRoutine == null)
        {
            musicRoutine = StartCoroutine(MusicLoop());
        }
    }

    public void StopPuzzle()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }

        musicSource.Stop();
        isPlaying = false;
        Debug.Log("퍼즐 중지");
    }

    private IEnumerator MusicLoop()
    {
        while (!Ch2_SewerPuzzleManager.Instance.IsPuzzleSolved())
        {
            yield return new WaitForSeconds(Random.Range(2f, 10f)); // 재생 간격

            // 음악 재생 준비
            float maxStart = Mathf.Max(1f, musicSource.clip.length - musicSegmentDuration);
            musicSource.time = Random.Range(0f, maxStart);
            musicSource.Play();

            isPlaying = true;
            songStartTime = Time.time;
            movedDuringPlay = false;

            yield return new WaitForSeconds(musicSegmentDuration);

            musicSource.Stop();
            isPlaying = false;

            if (movedDuringPlay)
            {
                TriggerPunishment();
            }

            yield return new WaitForSeconds(musicPauseDuration);
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        float elapsed = Time.time - songStartTime;

        // 경고 시간 이전 - 감시 X
        if (elapsed <= warningTime) return;

        // 경고 시간 이후 - 재생 끝나는 시점까지만 감시
        if (elapsed > warningTime && elapsed <= musicSegmentDuration)
        {
            if (IsPlayerMoving())
            {
                movedDuringPlay = true;
            }
        }
    }

    private bool IsPlayerMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
    }

    private string[] punishmentMessages =
    {
        "다시… 같은 공간…",
        "이 소리를… 무시하면 안돼.",
        "소리엔… 반응하면 안 돼.",
        "소리에 뭔가 규칙이 있는거 같은데..",
        "다시 처음부터...?",
        "소리가 들리면… 가만히… 가만히…"
    };
    
    private void TriggerPunishment()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && returnPoint != null)
        {
            player.transform.position = returnPoint.position;
            returnCount ++;
        }
        
        if (UIManager.Instance != null && UIManager.Instance.PromptUI != null)
        {   
            if(returnCount >= 2)
            {
                string msg = punishmentMessages[Random.Range(0, punishmentMessages.Length)];
                UIManager.Instance.PromptUI.ShowPrompt(msg, 2f);
            }
            else if(returnCount >= 5)
            {   
                UIManager.Instance.PromptUI.ShowPrompt_Random("노랫 소리가 들리면 움직이면 안돼", "전등을 따라 미로 끝까지 가보자");
            }
        }
    }
}
