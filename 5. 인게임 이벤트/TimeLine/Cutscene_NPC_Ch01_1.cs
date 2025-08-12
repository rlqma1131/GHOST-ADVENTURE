using UnityEngine;
using UnityEngine.Playables;
public class Cutscene_NPC : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    public bool isCutscenePlaying = false;
    
    public RoomInfo roomInfo;
    [SerializeField] private GameObject GarageDoor;
    void Start()
    {

        // 타임라인 재생 끝났을 때 호출될 함수 등록
        director.stopped += OnTimelineStopped;
        
    }





    private void Play_NPCscene()
    {


        if (director != null)
        {
            director.Play();
            EnemyAI.PauseAllEnemies();
            GarageDoor.SetActive(false);
            isCutscenePlaying = true;
            PossessionSystem.Instance.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
        }
        else
        {
            Debug.LogError("PlayableDirector is not assigned or missing.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCutscenePlaying && roomInfo.roomCount >= 1)
        {
            Play_NPCscene();
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {


        PossessionSystem.Instance.CanMove = true;
        UIManager.Instance.PlayModeUI_OpenAll();
        EnemyAI.ResumeAllEnemies();
        UIManager.Instance.PromptUI.ShowPrompt("차고의 문이 조금 열렸어.", 2f); 
    }
}
