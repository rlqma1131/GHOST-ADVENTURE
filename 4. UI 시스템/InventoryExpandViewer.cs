using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryExpandViewer : MonoBehaviour
{
    [SerializeField] private GameObject cluePanel; // 단서 뷰어 패널(검은 배경)
    [SerializeField] private Image clueImage; // 단서 이미지
    [SerializeField] private TextMeshProUGUI clueName; // 단서 이름
    [SerializeField] private TextMeshProUGUI clueDescription; // 단서 설명
    private bool isShowing = false; // "패널 클릭시 닫기" 기능용 불값
    public System.Action OnClueHidden; // 이벤트

    // public static InventoryExpandViewer Instance; // 싱글톤(수정예정)

    private void Awake()
    {
        // cluePanel.SetActive(false);
    }

    // 단서 크게 보여주기
    public void ShowClue(ClueData clue)
    {
        Debug.Log("[ShowClue] enter", this);

    if (clue == null) { Debug.LogError("ShowClue: clue is null"); return; }
    if (clueImage == null || clueName == null || clueDescription == null || cluePanel == null)
    { Debug.LogError("ShowClue: UI refs not assigned"); return; }

    // EnemyAI 쪽 예외로 전체가 중단되는 걸 방지 (로그만 남기고 진행)
    try { EnemyAI.PauseAllEnemies(); }
    catch (System.Exception e) { Debug.LogError($"PauseAllEnemies failed: {e.Message}"); }

    // 스프라이트/텍스처 확인
    var sprite = clue.clue_Image;
    if (sprite == null) { Debug.LogError("ShowClue: clue sprite null"); return; }

    clueImage.sprite = sprite;
    clueName.text = clue.clue_Name ?? "";
    clueDescription.text = clue.clue_Description ?? "";

    // 안전한 사이즈 계산
    var tex = sprite.texture;
    if (tex != null)
    {
        float h = tex.height;
        if (h > 800f)
        {
            float w = tex.width;
            float ratio = (h <= 0f) ? 1f : w / h;
            float fixedH = 800f;
            clueImage.rectTransform.sizeDelta = new Vector2(fixedH * ratio, fixedH);
        }
        else
        {
            // 원본 크기 적용
            clueImage.SetNativeSize();
        }
    }
    else
    {
        // 텍스처가 null이어도 보이게는 만들기
        clueImage.SetNativeSize();
        Debug.LogWarning("ShowClue: sprite.texture is null (Addressables/atlas timing?)");
    }

    cluePanel.SetActive(true);
    isShowing = true;

    // 최종 상태 로그
    Debug.Log($"[ShowClue] done. panelActive={cluePanel.activeInHierarchy}");
    }

    // 단서패널 닫기
    public void HideClue()
    {
        EnemyAI.ResumeAllEnemies();
        cluePanel.SetActive(false);
        isShowing = false;

        OnClueHidden?.Invoke();
        OnClueHidden = null; //자동해제
    }

    public bool IsShowing() => isShowing;


    // EnemyAI.PauseAllEnemies();
    //     clueImage.sprite = clue.clue_Image;
    //     clueName.text = clue.clue_Name;
    //     clueDescription.text = clue.clue_Description;
    //     clueImage.SetNativeSize();  
    //       // 이미지가 클 경우 크기 조정
    //     float originalHeight = clue.clue_Image.texture.height;
    //     if(originalHeight > 800f)
    //     {
    //         float fixedHeight = 800;
    //         float originalWidth = clue.clue_Image.texture.width;
    //         float aspectRatio = originalWidth / originalHeight;
    //         float calculatedWidth = fixedHeight * aspectRatio;

    //         clueImage.rectTransform.sizeDelta = new Vector2(calculatedWidth, fixedHeight);
    //           }
    //     cluePanel.SetActive(true);
    //     isShowing = true;
}
