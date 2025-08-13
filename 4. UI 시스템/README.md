# 4. UI 시스템

플레이 모드 전반의 UI(프롬프트·인벤토리·단서뷰어·튜토리얼·알림·설정·커서 등)를 통합 관리하는 모듈입니다.  
`UIManager`를 중심으로 화면에 상시 표시되는 요소와 상황 기반 팝업을 일관된 방식으로 제어합니다.


  
---


> ## 📂 폴더 구조

UI/  
├─ Core/ # UIManager, 공용 인터페이스/이벤트  
└─ MemoryStorage/ # 기억저장소(책 형식. 2개씩 페이징)  
├─ Inventory/  
│  ├─ Player/ # 플레이어 단서 인벤토리(페이지/키 바인딩)  
│  └─ Possessable/ # 빙의 오브젝트 아이템 인벤토리(포커스/선택)  
├─ ClueViewer/ # 단서 확대 뷰어(열기/닫기 이벤트)  
├─ EscMenu / # 옵션(오디오 볼륨) 
├─ Tutorial/ # 튜토리얼 단계 관리/강제 클릭 가이드  
├─ Prompt/ # 중앙 알림(페이드 인/아웃)  
├─ Notice/ # 화면 우상단 토스트/프롬프트  


---

> ## 🛠 핵심 설계

- **UIManager 단일 진입점**  
  각 UI 모듈을 `UIManager.Instance`가 보유/중계하며 전역 제어 가능.
- **이벤트 기반 결합**  
  `OnClueHidden` 등 이벤트를 통해 외부 로직과 느슨하게 연결.
- **상태 기반 표기**  
  인벤토리 포커스·빙의 상태·튜토리얼 단계 등 게임 상태를 기반으로 UI 노출.
- **큐/스택형 프롬프트**  
  누적 시 이전 항목을 위로 밀고 작게/옅게 표시.
- **페이지/플립 애니메이션**  
  기억저장소는 책 넘기기 연출과 입력 잠금 처리.
- **입력/상호작용 보호**  
  튜토리얼에서 특정 버튼 외 영역 인터랙션 차단.

---

> ## 💻 코드 예시

### 1) 프롬프트(토스트) 누적 표현
```csharp
public void ShowPrompt(string line, float display = 2f)
{
    for (int i = items.Count - 1; i >= 0; i--)
    {
        var it = items[i];
        it.running?.Kill();
        it.baseY += shiftUpY;
        it.scale *= shrinkFactor;
        it.alpha *= fadeFactor;
        it.running = DOTween.Sequence()
            .Join(it.rt.DOAnchorPosY(it.baseY, 0.18f))
            .Join(it.rt.DOScale(it.scale, 0.18f))
            .Join(it.rt.GetComponent<CanvasGroup>().DOFade(it.alpha, 0.18f));
    }

    var go = Instantiate(panel, panel.parent);
    go.gameObject.SetActive(true);
    var cg = go.GetComponent<CanvasGroup>();
    cg.alpha = 0f;

    var item = new Item { rt = go };
    items.Add(item);

    item.running = DOTween.Sequence()
        .Append(cg.DOFade(1f, 0.15f))
        .AppendInterval(display)
        .AppendCallback(() => RemoveOldestIfNeeded());
}
```

### 2) 단서 뷰어(이벤트 연동)
```csharp
public void HideClue()
{
    EnemyAI.ResumeAllEnemies();
    cluePanel.SetActive(false);
    isShowing = false;
    OnClueHidden?.Invoke();
}
```

### 3) 튜토리얼 단계 관리
```csharp
public void Show(TutorialStep step, bool once = true)
{
    if (once && completed.Contains(step)) return;
    UIManager.Instance.TutorialUI_Open(step);
    UIBlocker.MaskExcept(UIManager.Instance.GetTutorialTarget(step));
    completed.Add(step);
}
```

### 4) 기억저장소 책 넘기기
```csharp
IEnumerator PageTurnCoroutine(Action onComplete)
{
    isFlipping = true;
    foreach (var sp in NextSprites)
    {
        pageTurnImage.sprite = sp;
        yield return null;
    }
    isFlipping = false;
    onComplete?.Invoke();
}
```

### 5) 인벤토리(플레이어/빙의) 포커스와 키 바인딩
```csharp
public enum InvSide { Player, Possess }
public static class InventoryInputFocus { public static InvSide Current = InvSide.Player; }

public class Inventory_PossessableObject : MonoBehaviour
{
    void Update()
    {
        if (InventoryInputFocus.Current != InvSide.Possess) return;
        // 숫자키(1~4) 처리, 선택 슬롯 반영, Q키 상호작용 조건부 사용
    }
}

public class Inventory_Player : MonoBehaviour
{
    KeyCode[] frontKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    public KeyCode GetKey(int clueIndex)
    {
        // 인덱스 → 키 매핑(없으면 None)
        return (clueIndex >= 0 && clueIndex < frontKeys.Length) ? frontKeys[clueIndex] : KeyCode.None;
    }
}
```


> ## ✨ 설계 특징

- **느슨한 결합 / 강한 응집** : UI 간 통신은 `UIManager`와 이벤트만 사용.
- **비동기 안전성** : 트윈·코루틴 재진입 방지 플래그 사용.
- **상황 재현성** : 튜토리얼/프롬프트는 중복 방지 및 1회성·조건부 반복 구분.
- **모드 전환 안정화** : 씬 전환 시 `PlayModeUI_CloseAll()` 호출로 버그 예방.

---
