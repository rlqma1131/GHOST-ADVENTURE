using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
public class Ch3_ClearDoor : BaseInteractable
{
    public GameObject openDoor;
    public GameObject closeDoor; // 닫힌 문 오브젝트
    [SerializeField] private PlayableDirector playableDirector; 
    [SerializeField] private bool isPlayerNear = false;
    [SerializeField] private bool isOpen = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && isOpen && isPlayerNear)
        {
            GameManager.Instance.Player.SetActive(false); // 플레이어 비활성화
            playableDirector.Play();
            UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
        }
    }

    private void Start()
    {
        playableDirector.stopped += OnTimelineEnd;
    }

    void OnTimelineEnd(PlayableDirector director)
    {
        SceneManager.LoadScene("Ch03_To_Ch04");
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
