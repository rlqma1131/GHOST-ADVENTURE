using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryStorage : MonoBehaviour, IUIClosable
{
    [SerializeField] private AudioClip pageFlip;
    [SerializeField] private GameObject memoryNodePrefab;
    [SerializeField] private Transform leftPageSlot;
    [SerializeField] private Transform rightPageSlot;
    [SerializeField] private Sprite defaultPageSprite;               // 기본 페이지 이미지
[SerializeField] private Image pageTurnImage;                        // 페이지 넘김 효과 표시할 이미지       
    [SerializeField] private Sprite[] NextpageTurnSprites;           // 다음페이지 스프라이트
    [SerializeField] private Sprite[] PrevpageTurnSprites;           // 이전페이지 스프라이트
    [SerializeField] private float frameInterval = 0.05f;            // 프레임 간 간격

    private bool isFlipping = false;

    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    public enum PageTurnDirection { Next, Prev }
    [SerializeField] private Button closeButton;
    private MemoryData.Chapter currentChapter = MemoryData.Chapter.Chapter1; 

    private List<MemoryData> collectedMemories = new();
    public List<MemoryData> CollectedMemories => collectedMemories;
    public List<MemoryData> chaterMemories;

    private int currentPageIndex = 0; // 0: 첫 페이지, 1: 다음 페이지...

    private void OnEnable()
    {
        MemoryManager.Instance.OnMemoryCollected += OnMemoryCollected;
        RedrawStorage();
    }

    private void OnDisable()
    {
        MemoryManager.Instance.OnMemoryCollected -= OnMemoryCollected;
        EnemyAI.ResumeAllEnemies();
        PossessionSystem.Instance.CanMove = true;
    }
    
    void Update()
    {
        if(IsOpen())
        {
            EnemyAI.PauseAllEnemies();
            PossessionSystem.Instance.CanMove = false;
        }
    }

    private void OnMemoryCollected(MemoryData memory)
    {
        collectedMemories.Add(memory);
        ShowPage(currentPageIndex); // 현재 페이지 갱신
    }

    private void RedrawStorage()
    {
        collectedMemories = MemoryManager.Instance.GetCollectedMemories();
        currentPageIndex = 0;
        ShowPage(currentPageIndex);
    }

    private void ShowPage(int pageIndex)
    {
        ClearPage();
        chaterMemories = collectedMemories.FindAll(m => m.chapter == currentChapter);

        int leftIndex = pageIndex * 2;
        int rightIndex = leftIndex + 1;

        if (leftIndex < chaterMemories.Count)
            CreateMemoryNode(leftPageSlot, chaterMemories[leftIndex]);

        if (rightIndex < chaterMemories.Count)
            CreateMemoryNode(rightPageSlot, chaterMemories[rightIndex]);

        // 버튼 활성화 여부
        prevPageButton.interactable = pageIndex > 0;
        nextPageButton.interactable = (pageIndex + 1) * 2 < chaterMemories.Count;
    }

    private void CreateMemoryNode(Transform parent, MemoryData memory)
    {
        GameObject node = Instantiate(memoryNodePrefab, parent);
        node.GetComponent<MemoryNode>().Initialize(memory);
    }

    private void ClearPage()
    {
        foreach (Transform child in leftPageSlot) Destroy(child.gameObject);
        foreach (Transform child in rightPageSlot) Destroy(child.gameObject);
    }

    public void OnClickNextPage()
    {
        // TODO: 책장 넘김 애니메이션
        if (isFlipping || currentPageIndex + 1 >= NextpageTurnSprites.Length) return;

        currentPageIndex++;
        PlayPageTurnAnimation(PageTurnDirection.Next, () => ShowPage(currentPageIndex));
        SoundManager.Instance.PlaySFX(pageFlip);
    }

    public void OnClickPrevPage()
    {
        if (isFlipping || currentPageIndex + 1 >= PrevpageTurnSprites.Length) return;

        currentPageIndex--;
        PlayPageTurnAnimation(PageTurnDirection.Prev, () => ShowPage(currentPageIndex));
        SoundManager.Instance.PlaySFX(pageFlip);
        // TODO: 책장 넘김 애니메이션
    }

    public void Close()
    {
        gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public bool IsOpen() => gameObject.activeSelf;
    
    public void PlayPageTurnAnimation(PageTurnDirection direction, System.Action onComplete = null)
    {
        if (isFlipping) return;
        StartCoroutine(PageTurnCoroutine(direction, onComplete));
    }

    private IEnumerator PageTurnCoroutine(PageTurnDirection direction, System.Action onComplete)
    {
        isFlipping = true;
        pageTurnImage.gameObject.SetActive(true);

        Sprite[] frames = (direction == PageTurnDirection.Next) ? NextpageTurnSprites : PrevpageTurnSprites;

        foreach (Sprite sprite in frames)
        {
            pageTurnImage.sprite = sprite;
            yield return new WaitForSeconds(frameInterval);
        }

        // pageTurnImage.gameObject.SetActive(false);
        // isFlipping = false;
        // onComplete?.Invoke();
        
        pageTurnImage.sprite = defaultPageSprite;

        onComplete?.Invoke();
        isFlipping = false;
    }

    public void SetChapter(MemoryData.Chapter chapter)
    {
        currentChapter = chapter;
        currentPageIndex = 0;
        ShowPage(currentPageIndex);
    }


}
