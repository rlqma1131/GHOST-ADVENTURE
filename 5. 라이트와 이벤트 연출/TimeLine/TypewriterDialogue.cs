using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class TypewriterDialogue : MonoBehaviour
{
    public static TypewriterDialogue Instance { get; private set; }

    public TimelineControl timelineControl; //타임라인 컨트롤
    //public GameObject dialoguePanel; //대화창 패널
    public TextMeshProUGUI dialogueText; //대화 텍스트 컴포넌트
    public float typingSpeed = 0.05f; // 타이핑 속도 
    private bool isFinished = false;

    private bool isAuto = false; // 자동 대화
    public float autoNextDelay = 0.7f; // 한 대사 완료 후 다음으로 넘어가기까지의 지연 시간

    [System.Serializable] // 구조체를 인스펙터에 노출시키기 위해 필요
    public class DialogueSet
    {
        public string setName; // 인스펙터에서  대사를 구분하기 위한 이름 
        [TextArea(3, 10)]
        public string[] dialogues;
    }

    public DialogueSet[] allDialogueSets; // 모든 대사 묶음을 담을 배열
    private string[] dialogues;
    private int index = 0;
    private bool isTyping = false;
    private bool skipTyping = false;
    private bool isShakingDialogue = false; //

    void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //}
        //else
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        if (timelineControl == null)
        {
            timelineControl = FindObjectOfType<TimelineControl>();
            if (timelineControl == null)
            {
                Debug.LogError("TimelineControl을 씬에서 찾을 수 없습니다! TimelineManager 오브젝트에 TimelineControl 스크립트가 있는지 확인해주세요.");
            }
        }
    }

    void Start()
    {
        dialogueText.text = "";
        //dialoguePanel.SetActive(false);
        //dialogueText.gameObject.SetActive(true); // 대화 텍스트를 비활성화
    }

    void Update()
    {
        // 자동 대화 모드일 때는 클릭 입력을 무시합니다.
        if (isAuto) return;

        // 마우스 왼쪽 버튼을 클릭했을 때
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 1. 현재 대사가 타이핑 중이면, 즉시 전체 문장을 보여줍니다.
                skipTyping = true;
            }
            else if (!isFinished)
            {
                // 2. 타이핑이 끝났고 아직 보여줄 대사가 남았다면, 다음 대사를 시작합니다.
                NextLine();
            }
            else // isFinished == true
            {
                // 3. 모든 대화가 끝났다면, 대화창을 닫고 타임라인을 재개합니다.
                dialogueText.text = "";
                dialogueText.gameObject.SetActive(false);

                if (timelineControl != null)
                {
                    timelineControl.ResumeTimeline();
                }
                else
                {
                    Debug.LogWarning("TimelineControl이 할당되지 않아 타임라인을 재개할 수 없습니다.");
                }

                isFinished = false; // 상태 초기화
            }
        }
    }


    /// 지정된 인덱스의 대사 묶음을 가져와 일반적인 타이핑 효과로 대화창을 시작

    /// <param name="dialogueSetIndex">allDialogueSets 배열에서 사용할 대사 묶음의 인덱스</param>
    public void StartDialogueByIndex(int dialogueSetIndex)
    {
        StopAllCoroutines();

        if (allDialogueSets == null || dialogueSetIndex < 0 || dialogueSetIndex >= allDialogueSets.Length)
        {
            Debug.LogError($"잘못된 대화 묶음 인덱스: {dialogueSetIndex}. 대화 시작 실패.");
            return;
        }

        this.dialogues = allDialogueSets[dialogueSetIndex].dialogues; // 해당 인덱스의 대사 묶음을 현재 dialogues로 설정
        index = 0;
        dialogueText.gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }


    /// <param name="dialogueSetIndex">allDialogueSets 배열에서 사용할 대사 묶음의 인덱스</param>
    public void StartShakeDialogueByIndex(int dialogueSetIndex) // <-- int 인자로 변경
    {
        StopAllCoroutines();

        if (allDialogueSets == null || dialogueSetIndex < 0 || dialogueSetIndex >= allDialogueSets.Length)
        {
            Debug.LogError($"잘못된 대화 묶음 인덱스: {dialogueSetIndex}. 흔들림 대화 시작 실패.");
            return;
        }

        this.dialogues = allDialogueSets[dialogueSetIndex].dialogues; // 해당 인덱스의 대사 묶음을 현재 dialogues로 설정
        index = 0;
        dialogueText.gameObject.SetActive(true);
        StartCoroutine(TypeLineShake());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        isShakingDialogue = false;
        dialogueText.text = "";

        if (dialogues == null || dialogues.Length <= index)
        {
            Debug.LogWarning("대사 배열이 비어있거나 인덱스가 범위를 벗어났습니다. 대화 종료.");
            dialogueText.gameObject.SetActive(false);
            isTyping = false;
            skipTyping = false;
            if (timelineControl != null) timelineControl.ResumeTimeline();
            yield break;
        }

        foreach (char letter in dialogues[index].ToCharArray())
        {
            if (skipTyping)
            {
                dialogueText.text = dialogues[index];
                break;
            }
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        skipTyping = false;
    }

    IEnumerator TypeLineShake()
    {
        isTyping = true;
        isShakingDialogue = true;
        dialogueText.text = "";

        if (dialogues == null || dialogues.Length <= index)
        {
            Debug.LogWarning("대사 배열이 비어있거나 인덱스가 범위를 벗어났습니다. 대화 종료.");
            dialogueText.gameObject.SetActive(false);
            isTyping = false;
            skipTyping = false;
            if (timelineControl != null) timelineControl.ResumeTimeline();
            yield break;
        }

        foreach (char letter in dialogues[index].ToCharArray())
        {
            if (skipTyping)
            {
                dialogueText.text = dialogues[index];
                break;
            }
            dialogueText.text += letter;
            ShakeDialogue(0.2f, 8f);
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        skipTyping = false;
    }

    void NextLine()
    {
        if ((dialogues == null || dialogues.Length == 0))
        {
            Debug.LogWarning("dialogues 배열이 비어 있거나 null입니다.");
            return;
        }

        if (index < dialogues.Length - 1)
        {
            index++;
            StopAllCoroutines();

            if (isShakingDialogue)
            {
                StartCoroutine(TypeLineShake());
            }
            else
            {
                StartCoroutine(TypeLine());
            }
        }
        else
        {
            
            isFinished = true;
        }
    }



    public void StartAutoDialogueByIndex(int dialogueSetIndex)
    {
        StopAllCoroutines();

        if (allDialogueSets == null || dialogueSetIndex < 0 || dialogueSetIndex >= allDialogueSets.Length)
        {
            Debug.LogError($"잘못된 대화 묶음 인덱스: {dialogueSetIndex}. 자동 대화 시작 실패.");
            return;
        }

        this.dialogues = allDialogueSets[dialogueSetIndex].dialogues;
        index = 0;
        isAuto = true; // 자동 모드 ON
        dialogueText.gameObject.SetActive(true);
        StartCoroutine(TypeLineAuto());


    }

    public void StartShakeAutoDialogueByIndex(int dialogueSetIndex)
    {
        StopAllCoroutines();

        if (allDialogueSets == null || dialogueSetIndex < 0 || dialogueSetIndex >= allDialogueSets.Length)
        {
            Debug.LogError($"잘못된 대화 묶음 인덱스: {dialogueSetIndex}. 자동 대화 시작 실패.");
            return;
        }

        this.dialogues = allDialogueSets[dialogueSetIndex].dialogues;
        index = 0;
        isAuto = true; // 자동 모드 ON
        dialogueText.gameObject.SetActive(true);
        StartCoroutine(ShakeTypeLineAuto());

    }

    IEnumerator ShakeTypeLineAuto()
    {
        isTyping = true;
        isShakingDialogue = false;
        dialogueText.text = "";

        if (dialogues == null || dialogues.Length <= index)
        {
            Debug.LogWarning("대사 배열이 비어있거나 인덱스가 범위를 벗어났습니다. 대화 종료.");
            dialogueText.gameObject.SetActive(false);
            isTyping = false;
            skipTyping = false;
            if (timelineControl != null) timelineControl.ResumeTimeline();
            yield break;
        }

        foreach (char letter in dialogues[index].ToCharArray())
        {
            dialogueText.text += letter;
            ShakeDialogue(0.2f, 8f);
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        skipTyping = false;


        if (isAuto)
        {
            yield return new WaitForSecondsRealtime(autoNextDelay);
            NextLineAuto();
        }
    }

    void NextLineAuto()
    {
        if (index < dialogues.Length - 1)
        {
            index++;
            StopAllCoroutines();
            StartCoroutine(TypeLineAuto());
        }
        else
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
            isAuto = false;
            if (timelineControl != null)
            {
                timelineControl.ResumeTimeline();
            }
            else
            {
                Debug.LogWarning("TimelineControl이 할당되지 않아 타임라인을 재개할 수 없습니다.");
            }
        }
    }
    IEnumerator TypeLineAuto()
    {
        isTyping = true;
        isShakingDialogue = false;
        dialogueText.text = "";

        if (dialogues == null || dialogues.Length <= index)
        {
            Debug.LogWarning("대사 배열이 비어있거나 인덱스가 범위를 벗어났습니다. 대화 종료.");
            dialogueText.gameObject.SetActive(false);
            isTyping = false;
            skipTyping = false;
            if (timelineControl != null) timelineControl.ResumeTimeline();
            yield break;
        }

        foreach (char letter in dialogues[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        skipTyping = false;


        if (isAuto)
        {
            yield return new WaitForSecondsRealtime(autoNextDelay);
            NextLineAuto();
        }
    }



    void ShakeDialogue(float duration = 0.2f, float strength = 8f) //텍스트 흔들기 메서드
    {
        dialogueText.gameObject.transform.DOShakePosition(
            duration,
            strength,
            vibrato: 150,
            randomness: 90,
            snapping: false,
            fadeOut: true
        ).SetUpdate(true);
    }
}

