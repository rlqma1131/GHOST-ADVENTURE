> ## 📂 Object 폴더 구조
```cs
Object/
 ├── BaseInteractable.cs              # 모든 상호작용 오브젝트 공통 베이스 (하이라이트, 플레이어 감지)
 ├── UniqueId.cs                      # 저장/로드용 고유 식별자 부여
 │
 ├── PossessableObject/               # 빙의 가능한 객체 공통 베이스
 │    ├── BasePossessable.cs          # 빙의 상태, 저장 연동, 해제 처리
 │    └── MoveBasePossessable.cs      # 이동 로직 포함 빙의 객체 베이스
 │
 ├── PO_Object/                       # 빙의 가능한 사물 (고유 기능 구현)
 │    ├── Ch1_Shower.cs               # 샤워기 - 물 켜기/끄기
 │    ├── Ch2_MorseKey.cs             # 모스키 퍼즐 입력
 │    └── ...
 │
 ├── PO_Animal/                       # 빙의 가능한 동물
 │    ├── Cat.cs
 │    └── ...
 │
 ├── PO_Person/                       # 빙의 가능한 사람/NPC
 │    ├── Nurse.cs
 │    └── ...
 │
 ├── MemoryObject/                    # 기억 조각 관련
 │    ├── MemoryFragment.cs           # 기억 조각 최상위, 스캔·수집 로직
 │    ├── MemoryData.cs               # 기억 조각 데이터 (ScriptableObject)
 │    ├── Memory_Positive.cs          # 긍정 기억 고유 기능
 │    ├── Memory_Negative.cs          # 부정 기억 고유 기능
 │    └── Memory_Fake.cs              # 가짜 기억 고유 기능
 │
 └── NormalObject/                    # 빙의/기억 X, 일반 상호작용 오브젝트
      ├── ClosetDoor.cs               # 문 열기/닫기
      ├── LightSwitch.cs              # 불 켜기/끄기
      └── ...
```
---
> ## 핵심 설계

### 1) 공통 상호작용 시스템
**BaseInteractable**
- 모든 상호작용 가능한 오브젝트의 **하이라이트 표시**, **플레이어 감지** 담당.
- `OnTriggerEnter2D` / `OnTriggerExit2D` 로 **PlayerInteractSystem**과 연동.
- `highlight` 오브젝트를 켜고/끄는 **SetHighlight** 메서드 제공.

### 2) 빙의 가능한 오브젝트 베이스
**BasePossessable**
- `isPossessed`, `hasActivated` 상태 보유.
- 플레이어가 빙의할 수 있는지 여부를 제어하고, 빙의/해제 시 **UI 연출** 호출.
- `UniqueId`를 통해 저장/로드 시 상태(`hasActivated`)를 복원.
- `MarkActivatedChanged()`로 상태 변경 시 저장 데이터 갱신.

### 3) 이동형 빙의 오브젝트
**MoveBasePossessable**
- `BasePossessable` 상속 + **이동 로직** 포함.
- `Update`에서 방향키 입력 → `Move()` 메서드로 위치 변경.
- `CinemachineVirtualCamera` 우선순위를 변경하여 **줌/시점 전환** 구현.
- `spriteRenderer.flipX`로 좌우 반전 처리.

### 4) 기억 조각 시스템
**MemoryFragment**
- 스캔 가능 여부(`isScannable`), 수집 가능 여부(`canStore`) 상태 보유.
- 스캔 시:
  - `MemoryManager`를 통해 데이터 등록.
  - `UniqueId` 저장, `ChapterEndingManager`에 스캔 기록.
  - 컷씬/연출 실행 + **드랍 애니메이션** 표시.
- `ApplyFromSave()`로 저장된 스캔 가능 상태 복원.

### 5) 기억 조각 데이터 관리
**MemoryData**
- **ScriptableObject** 기반의 기억 조각 데이터.
- 타입(`Positive`, `Negative`, `Fake`), 챕터, 컷씬 이름, 이미지, 회복량 등 메타데이터 포함.
- 에디터에서 연결한 씬/이미지를 **런타임 문자열**로 저장하여 로딩 시 사용.

### 6) 일반 상호작용 오브젝트
**NormalObject**
- 빙의나 기억 조각이 아니면서 **단순한 상호작용만 수행**하는 오브젝트.
- 예: `ClosetDoor`(문 열기/닫기), `LightSwitch`(불 켜기/끄기).

---
> ## 코드 예시

### 공통 상호작용 베이스
```csharp
// BaseInteractable.cs
public class BaseInteractable : MonoBehaviour
{
    public GameObject highlight;

    void Start() => highlight?.SetActive(false);
    public void SetHighlight(bool pop) => highlight?.SetActive(pop);

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (gameObject.CompareTag("HideArea") && col.CompareTag("Player"))
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            SetHighlight(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
```

### 빙의 가능 오브젝트
```csharp
// BasePossessable.cs
public abstract class BasePossessable : BaseInteractable
{
    [SerializeField] protected bool hasActivated;
    public bool isPossessed;

    public virtual void Unpossess()
    {
        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("빙의 해제", 2f);
        isPossessed = false;
        PossessionStateManager.Instance.StartUnpossessTransition();
    }

    protected void MarkActivatedChanged()
    {
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetPossessableState(uid.Id, hasActivated);
    }
}
```
### 이동 로직 포함
```csharp
// MoveBasePossessable.cs
protected virtual void Move()
{
    float h = Input.GetAxis("Horizontal");
    Vector3 move = new Vector3(h, 0, 0);

    if (anim) anim.SetBool("Move", move.sqrMagnitude > 0.01f);

    if (move.sqrMagnitude > 0.01f)
        transform.position += move * moveSpeed * Time.deltaTime;
}
```

### 기억 조각 스캔
```cs
// MemoryFragment.cs
public void IsScannedCheck()
{
    if (!isScannable) return;
    isScannable = false;
    canStore = true;

    MemoryManager.Instance.TryCollect(data);

    if (TryGetComponent(out UniqueId uid))
        SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);

    ChapterEndingManager.Instance.RegisterScannedMemory(
        data.memoryID, DetectChapterFromScene(SceneManager.GetActiveScene().name)
    );
}
```

### 기억 조각 데이터
```cs
// MemoryData.cs
[CreateAssetMenu(menuName = "Memory/MemoryData")]
public class MemoryData : ScriptableObject
{
    public enum MemoryType { Positive, Negative, Fake }
    public Chapter chapter;
    public MemoryType type;
    public string memoryID;
    public string memoryTitle;
    public Sprite MemoryObjectSprite;
    public Sprite MemoryCutSceneImage;
    public string CutSceneName;
}
```
---

> ## 설계 특징(요약)
- **데이터 복원**: 모든 상호작용/빙의 오브젝트는 `UniqueId`를 통해 저장·로드 상태를 유지.
- **모듈화**: 입력 처리, 상태 저장, 연출을 각각의 책임 단위로 분리.
- **확장성**: 새로운 오브젝트 추가 시 베이스 상속 + 고유 기능 구현만으로 손쉽게 확장.
- **UI 일관성**: 빙의/상호작용/스캔 시 UIManager와 통합된 프롬프트·컷씬 연출 사용.
- **성능 최적화**: 트리거 기반 감지로 불필요한 Update 호출 최소화.
