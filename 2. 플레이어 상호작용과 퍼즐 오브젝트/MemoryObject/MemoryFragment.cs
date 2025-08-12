using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryFragment : BaseInteractable
{
    public MemoryData data;
    
    [SerializeField] protected bool isScannable = false; // 디버깅용
    public bool IsScannable => isScannable;

    [SerializeField] private bool canStore = false;
    public bool CanStore => canStore;

    public AudioClip audioSource1; // 스캔 사운드 재생용
    public AudioClip audioSource2; // 스캔 사운드 재생용
    [Header("드랍 조각 프리팹")]
    [SerializeField] private GameObject fragmentDropPrefab;

    [Header("드랍 연출 설정")]
    [SerializeField] private Vector3 dropOffset = Vector3.zero;
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceDuration = 0.5f;

    [Header("회전 연출 설정")]
    [SerializeField] private float rotateTime = 2f;
    [SerializeField] private float ellipseRadiusX = 0.8f;
    [SerializeField] private float ellipseRadiusZ = 1.5f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (CanStore)
        {
            // 실행 중에 isScannable을 수동으로 true로 바꾼 경우 처리
            Debug.Log($"[DEBUG] 인스펙터에서 isScannable 체크됨: {data.memoryID}");

            // 등록되지 않았으면 등록
            if (!MemoryManager.Instance.IsCanStore(data))
            {
                MemoryManager.Instance.TryCollect(data);
            }
        }
    }
#endif

    // 상호작용 메시지 대상
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isScannable)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    public void IsScannedCheck()
    {
        if (!isScannable) return;
        isScannable = false;
        canStore = true;

        MemoryManager.Instance.TryCollect(data);

        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);

        var chapter = DetectChapterFromScene(SceneManager.GetActiveScene().name);
        ChapterEndingManager.Instance.RegisterScannedMemory(data.memoryID, chapter);

        SaveManager.SaveWhenScanAfter(data.memoryID, data.memoryTitle,
            SceneManager.GetActiveScene().name,
            GameManager.Instance.Player.transform.position,
            checkpointId: data.memoryID,
            autosave: true);

        Debug.Log($"[MemoryFragment] 진행도 저장됨 : {data.memoryID} / {data.memoryTitle}");


        Sprite dropSprite = GetFragmentSpriteByType(data.type);
        if (fragmentDropPrefab == null || dropSprite == null) return;

        StartCoroutine(InstantiateDrop(dropSprite));

        //GameObject drop = Instantiate(fragmentDropPrefab, transform.position + dropOffset, Quaternion.identity);

        //if (drop.TryGetComponent(out SpriteRenderer sr))
        //    sr.sprite = dropSprite;
        //    sr.sortingOrder = 150; // 드랍 조각의 정렬 순서 설정

        //StartCoroutine(PlayDropSequence(drop));
    }

    private IEnumerator InstantiateDrop(Sprite dropSprite)
    {
        yield return new WaitForSeconds(0.1f); // 잠시 대기 후 드랍 생성

        GameObject drop = Instantiate(fragmentDropPrefab, transform.position + dropOffset, Quaternion.identity);
        if (drop.TryGetComponent(out SpriteRenderer sr))
        {
            sr.sprite = dropSprite;
            sr.sortingOrder = 150; // 드랍 조각의 정렬 순서 설정
        }

        StartCoroutine(PlayDropSequence(drop));
    }

    private IEnumerator PlayDropSequence(GameObject drop)
    {
        if (drop == null) yield break;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        SoundManager.Instance.FadeOutAndStopBGM(1f); // BGM 페이드아웃
        SoundManager.Instance.PlaySFX(audioSource1); // 스캔 사운드 재생
        Vector3 startPos = drop.transform.position;
        EnemyAI.PauseAllEnemies();
        PossessionSystem.Instance.CanMove = false; // 플레이어 이동 비활성화
        UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
         // === 1. 튕기기 애니메이션 ===
        var bounceSeq = DOTween.Sequence()
            .Append(drop.transform.DOMoveY(startPos.y + bounceHeight, bounceDuration / 2f).SetEase(Ease.OutQuad))
            .Append(drop.transform.DOMoveY(startPos.y, bounceDuration / 2f).SetEase(Ease.InQuad))
            .Join(drop.transform.DOPunchScale(Vector3.one * 0.1f, bounceDuration, 5, 1));

        yield return bounceSeq.WaitForCompletion();
        bounceSeq.Kill();

        // === 2. 회전 궤도 진입 및 상승 ===
        Vector3 center = startPos;
        Vector3 local = drop.transform.position - center;

        float startAngleRad = Mathf.Atan2(local.z / ellipseRadiusZ, local.x / ellipseRadiusX);
        float startAngleDeg = startAngleRad * Mathf.Rad2Deg;
        float currentAngle = startAngleDeg;
        float targetAngle = startAngleDeg - 720f; // 2바퀴 회전

        float rad = startAngleDeg * Mathf.Deg2Rad;
        Vector3 initialOffset = new Vector3(
            Mathf.Cos(rad) * ellipseRadiusX,
            0f,
            Mathf.Sin(rad) * ellipseRadiusZ
        );

        // 튕겨진 현재 y 위치 그대로 유지
        Vector3 initialPos = new Vector3(
            center.x + initialOffset.x,
            drop.transform.position.y,
            center.z + initialOffset.z
        );

        yield return drop.transform.DOMove(initialPos, 0.1f).SetEase(Ease.InOutSine).WaitForCompletion();

        float startY = drop.transform.position.y;
        float targetY = startY + 4f;

        Tween rotate = DOTween.To(() => currentAngle, x =>
        {
            currentAngle = x;

            float progress = Mathf.InverseLerp(startAngleDeg, targetAngle, currentAngle);
            float y = Mathf.Lerp(startY, targetY, progress);

            float r = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(r) * ellipseRadiusX,
                0f,
                Mathf.Sin(r) * ellipseRadiusZ
            );

            // X 좌표를 원래 center.x로 점점 보정
            float correctedX = Mathf.Lerp(center.x + offset.x, center.x, progress);

            drop.transform.position = new Vector3(
                correctedX,
                y,
                center.z + offset.z
            );

            if (drop.TryGetComponent(out SpriteRenderer sr))
                sr.sortingOrder = 100;

        }, targetAngle, rotateTime).SetEase(Ease.InOutSine);
        yield return rotate.WaitForCompletion();
        rotate.Kill();
        SoundManager.Instance.PlaySFX(audioSource2); // 스캔 사운드 재생
        drop.GetComponent<PixelExploder>()?.Explode(); // 픽셀 폭발 효과 적용

        Destroy(drop);
        StartCoroutine(CutsceneManager.Instance.PlayCutscene()); // 페이드인 줌인

        yield return new WaitForSeconds(5f); // 흡수 될때까지 기다림

        SceneManager.LoadScene(data.CutSceneName, LoadSceneMode.Additive); // 스캔 완료 후 씬 전환
        UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
        Time.timeScale = 0;
        ApplyMemoryEffect(); // 메모리 효과 적용
        PlusAction();

    }

    private Sprite GetFragmentSpriteByType(MemoryData.MemoryType type)
    {
        return type switch
        {
            MemoryData.MemoryType.Positive => data.PositiveFragmentSprite,
            MemoryData.MemoryType.Negative => data.NegativeFragmentSprite,
            MemoryData.MemoryType.Fake => data.FakeFragmentSprite,
            _ => null
        };
    }

    public void ApplyMemoryEffect()
    {
        switch (data.type)
        {
            case MemoryData.MemoryType.Positive:

                UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 기억 조각을 수집했습니다.");
                Debug.Log($"MemoryFragment: {data.memoryID} - 스캔 완료!"); // 디버그용 로그
                //퍼즐 조건 해금
                break;

            case MemoryData.MemoryType.Negative:
                UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 기억 조각을 수집했습니다.");
                //ApplyDebuff(); // 적 추적 활성화 등
                break;

            case MemoryData.MemoryType.Fake:
                UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 기억 조각을 수집했습니다. 그러나… 뭔가 이상합니다.");
                //FakeEndingManager.Instance.CollectFakeMemory(data.memoryID);
                break;
        }
    }

    public virtual void AfterScan() { }

    protected int DetectChapterFromScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return 0;
        if (sceneName.Contains("Ch01")) return 1;
        if (sceneName.Contains("Ch02")) return 2;
        if (sceneName.Contains("Ch03")) return 3;
        return 0;
    }

    protected virtual void PlusAction(){}

    public void ApplyFromSave(bool scannable)
    {
        isScannable = scannable;
    }
}
