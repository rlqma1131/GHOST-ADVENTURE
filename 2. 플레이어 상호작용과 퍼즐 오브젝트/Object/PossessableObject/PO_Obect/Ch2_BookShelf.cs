using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class Ch2_BookShelf : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    
    [SerializeField] private Ch2_BookSlot[] bookSlots;
    [SerializeField] private List<Ch2_BookSlot> correctSequence;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private GameObject doorToOpen;
    private List<Ch2_BookSlot> clickedSequence = new List<Ch2_BookSlot>();
    
    [SerializeField] private Transform moveTargetPosition;
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.3f;
    // private int currentIndex = 0;
    
    [SerializeField] private List<Transform> moveTargets;

    private bool isControlMode = false;

    [SerializeField] private List<ClueData> needClues;
    private bool promptShown = false;

    protected override void Update()
    {
        CheckCollectedAllClue();
        
        if (!isPossessed || !hasActivated)
        {
            //q_Key.SetActive(false);
            return;
        }
        
        if (!promptShown && HasAllClues())
        {
            UIManager.Instance.PromptUI.ShowPrompt_2("이 책들은 여기 없는데?", "책장을 잘 살펴보자");
            promptShown = true;
        }
        
        if(!isControlMode) 
            EnterControlMode();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            //isControlMode = false;
            ExitControlMode();
            Unpossess();
            promptShown = false;
        }
        // if (!isControlMode) return;
        // q_Key.SetActive(true);
    }

    private void EnterControlMode()
    {
        isControlMode = true;
        zoomCamera.Priority = 20;
        UIManager.Instance.PlayModeUI_CloseAll();
        EnemyAI.PauseAllEnemies();
    }

    private void ExitControlMode()
    {
        isControlMode = false;
        zoomCamera.Priority = 5;
        UIManager.Instance.PlayModeUI_OpenAll();
        EnemyAI.ResumeAllEnemies();
    }
    
    private void LateUpdate()
    {
        if (!isControlMode || !Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider == null) return;

        var slot = hit.collider.GetComponent<Ch2_BookSlot>();  // :contentReference[oaicite:0]{index=0}
        if (slot == null) return;

        if (slot.IsResetBook)
        {
            ResetAllBooks();
            return;
        }

        if (!HasAllClues())
        {
            UIManager.Instance.PromptUI.ShowPrompt("아직 단서가 부족해", 1.5f);
            return;
        }

        // ▷ 6글자 초과 선택 제한
        if (!slot.IsPushed && clickedSequence.Count >= correctSequence.Count)
        {
            UIManager.Instance.PromptUI.ShowPrompt($"정답은 {correctSequence.Count}글자입니다", 1.5f);
            return;
        }

        // 토글 후 시퀀스 업데이트
        slot.ToggleBook();  // :contentReference[oaicite:1]{index=1}
        if (slot.IsPushed)
        {
            clickedSequence.Add(slot);
            // ▷ 선택 글자 moveTargets 위치로 이동·확대
            int idx = clickedSequence.Count - 1;
            if (idx < moveTargets.Count)
            {
                var txtT = slot.booknameRenderer.transform;
                // world로 분리하고
                txtT.SetParent(null);
                txtT.DOMove(moveTargets[idx].position, 0.2f).SetEase(Ease.InOutSine);
                txtT.DOScale(1.2f,         0.2f).SetEase(Ease.OutQuad);
            }
        }
        else
        {
            clickedSequence.Remove(slot);
            slot.ResetNameTransform();
        }

        CheckPuzzleSolved();
    }

    private void CheckPuzzleSolved()
    {
        if (clickedSequence.Count != correctSequence.Count) return;

        // ▷ 오답 처리
        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (clickedSequence[i] != correctSequence[i])
            {
                UIManager.Instance.PromptUI.ShowPrompt("힌트들을 보고 더 생각해보자...", 2f);
                ResetAllBooks();
                clickedSequence.Clear();
                return;
            }
        }
        
        Sequence seq = DOTween.Sequence().AppendInterval(0.5f);
        for (int i = 0; i < correctSequence.Count; i++)
        {
            var slot   = correctSequence[i];
            var target = moveTargets[i];
            var txtT   = slot.booknameRenderer.transform;

            seq.Join(txtT.DOMove(target.position, 0.5f).SetEase(Ease.InOutSine));
            seq.Join(txtT.DOScale(1.5f,           0.5f).SetEase(Ease.OutQuad));
        }
        seq.AppendInterval(1.5f);
        seq.OnComplete(() =>
        {
            doorToOpen.SetActive(true);
            ExitControlMode();

            hasActivated = false;
            MarkActivatedChanged();

            Unpossess();
            ConsumeClue(needClues);

            // 책장 흔들고 이동
            transform.DOShakePosition(shakeDuration, shakeStrength)
                     .OnComplete(() =>
                     {
                         transform.DOMove(moveTargetPosition.position, moveDuration)
                                  .SetEase(Ease.InOutSine);
                     });
        });
    }
    
    public void ResetAllBooks()
    {
        foreach (var slot in bookSlots)
        {
            if (slot.IsPushed)
                slot.ToggleBook();
            
            slot.ResetNameTransform();
        }
        // currentIndex = 0;
        clickedSequence.Clear();
    }

    private void ConsumeClue(List<ClueData> clues)
    {
        UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());
    }

    private bool HasAllClues()
    {
        foreach (var clue in needClues)
        {
            if (!UIManager.Instance.Inventory_PlayerUI.collectedClues.Contains(clue))
                return false;
        }
        return true;
    }

    private void CheckCollectedAllClue()
    {
        if(
            SaveManager.IsPuzzleSolved("“닫힌 문... 잃어버린 무언가... 계속 이어지는 느낌이야.”") &&
            SaveManager.IsPuzzleSolved("“혹시 나도 잃어버린 존재...?”") &&
            SaveManager.IsPuzzleSolved("책 제목같아... 무슨의미일까?") &&
            SaveManager.IsPuzzleSolved("메모3"))
        {
            TutorialManager.Instance.Show(TutorialStep.CollectedAllMemoClue);
        }
    }
}