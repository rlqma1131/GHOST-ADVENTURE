# GHOST-ADVENTURE
Assets/
└─ 01.Scripts/
   ├─ Enemy/
   │  ├─ EnemyAI.cs                 # 상태머신(Idle/Patrol/Chase/QTE/Investigate) + 사운드 텔레포트
   │  ├─ EnemyMovementController.cs # 자연스러운 패atrol/추격 이동 + 문 순간이동 처리
   │  ├─ EnemyDetection.cs          # 전/후방 듀얼 콘(시야) + 반경(전방위) 감지
   │  ├─ EnemyQTEHandler.cs         # 잡힘-QTE 시퀀스(목숨/애니메이션/복귀) 제어
   │  ├─ TeleporterEnemyAI.cs       # 플레이어 뒤로 정기 텔레포트하는 변형 AI
   │  ├─ EnemyVolumeTrigger.cs      # 구역별 적 위협 감지 → 오버레이/사운드 강도 보고
   │  └─ EnemyVolumeOverlay.cs      # 모든 적의 강도 중 최댓값만 전역 볼륨에 반영
   │
   ├─ Systems/
   │  └─ QTE/
   │     └─ QTEEffectManager.cs     # QTE 중 암전/줌/카메라 이동 연출 (Unscaled Time)
   │
   └─ World/
      └─ SoundTrigger.cs            # 사운드 이벤트 → 적 사운드 추격 텔레포트 유도
```

## 핵심 설계

### 1) 상태 머신 중심의 적 AI
- **EnemyAI** 가 Idle/Patrol/Chase/QTE/Investigate 5개 상태를 보유하고, `ChangeState`로 전환합니다.
- `FixedUpdate`는 QTE 중/일시정지 시 실행을 막아 **동기적·안정적 이동**을 보장합니다.
- 컷신/퍼즐 연출에는 **Investigate** 상태(위치 강제 및 복귀 코루틴)를 사용하여 게임 흐름과 결합합니다.

### 2) 자연스러운 순찰/추격 이동
- **EnemyMovementController**
  - 일정 간격으로 방향을 살짝 바꾸는 **랜덤 드리프트**.
  - `Raycast` 기반 축별 충돌 회피(막히면 해당 축 성분만 제거).
  - **막힘 검출** 시 짧은 Idle 후 재개(살짝 호흡을 주어 “짓뭉개짐” 제거).
  - 문 트리거 진입 시 **확률적 순간이동**, 도착지 충돌 검사 및 **일시적 충돌 무시**로 “벽에 끼임/튕김” 방지.

### 3) 감지 모델: 듀얼 콘 + 전방위(사운드 모드)
- **EnemyDetection**
  - 전방/후방 **듀얼 콘**으로 시야 판정, 플레이어 방향 반영.
  - 사운드 추격 중에는 각도 무시, **반경 기반 전방위** 감지로 변환.

### 4) 사운드 트리거형 추격 텔레포트
- **SoundTrigger → EnemyAI**
  - 사운드 범위 내 적에게 **순간이동 연출 후 Chase** 진입.
  - 일정 시간 동안만 **전방위 감지** 유지, 종료 시 페이드+원위치 복귀.

### 5) QTE(잡힘) 시퀀스 & 연출
- **EnemyQTEHandler + QTEEffectManager**
  - QTE 동안 **암전/줌/카메라 중앙 이동**을 UnscaledTime으로 연출.
  - **목숨 1개일 때 잡히면 즉사**, 그 외에는 탈출 성공 시 **목숨 1 감소** 후 원위치 복귀.
  - 애니메이션 이벤트 누락 대비 **세이프티 루프**와 **보정 이동**으로 빌드 환경에서도 안정.

### 6) 전역 위협 오버레이(볼륨)
- **EnemyVolumeTrigger → EnemyVolumeOverlay**
  - 여러 적/구역이 겹칠 때 **가장 강한 강도 하나만** 전역 볼륨에 반영(색상은 공통).
  - 가중치가 거의 0이면 볼륨을 **비활성화**하여 비용을 절감.

## 설계 특징(요약)

- **빌드 안정성**: QTE/연출은 `Time.unscaledDeltaTime`, `WaitForSecondsRealtime` 기반으로 프레임레이트/TimeScale 영향 최소화.
- **충돌 안전성**: 문 순간이동 시 목적지 **Overlap 검사**, 잠깐의 **레이어 충돌 무시**로 끼임/튕김 회피.
- **디커플링**: 감지/이동/QTE/연출/오버레이가 **책임 단위**로 분리되어 유지보수 용이.
- **퍼포먼스**: 오버레이 **가중치≈0**일 때 **완전 OFF**, Update 내 **Find 호출 없음**, 참조는 1회 캐시.
- **확장성**: `EnemyAI`를 상속해 행동만 오버라이드(예: **TeleporterEnemyAI**)하면 변형 적을 손쉽게 추가.

## 코드 예시

> 발표 슬라이드에 넣기 좋은 최소 예시만 발췌했습니다.

### 상태 머신 & 사운드 추격(발췌)
```csharp
// EnemyAI.cs
public IdleState IdleState { get; private set; }
public PatrolState PatrolState { get; private set; }
public ChaseState  ChaseState  { get; private set; }
public QTEState    QTEState    { get; private set; }
public InvestigateState InvestigateState { get; private set; }

public void StartSoundTeleport(Vector3 playerPos, float offset, float chaseDuration) {
    if (soundChaseCoroutine != null) return;
    soundChaseCoroutine = StartCoroutine(SoundTeleportRoutine(playerPos, offset, chaseDuration));
}
```

### 듀얼 콘 시야 & 전방위 반경
```csharp
// EnemyDetection.cs
public bool IsInVision(Transform target) { /* 전/후방 콘 판정 */ }
public bool IsWithinRadius(Transform t, float r) => Vector2.Distance(tr.position, t.position) <= r;
```

### 자연스러운 패트롤(막힘 → 짧은 Idle → 재개)
```csharp
// EnemyMovementController.cs
if (stuckTimer >= stuckMaxTime) {
    stuckTimer = 0f;
    PickRandomDirection();
    if (!briefIdleRunning) StartCoroutine(BriefIdleThenPatrol());
}
```

### 문 순간이동(도착지 안전 확인 + 일시 충돌 무시)
```csharp
// EnemyMovementController.cs
Vector3 dest = door.GetTargetDoor()?.position ?? (Vector3)door.GetTargetPos();
if (Physics2D.OverlapCircle(dest, teleportSafetyRadius, obstacleMask)) { PickRandomDirection(); return; }
Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true); // 1초 뒤 복원
transform.position = dest;
```

### 잡힘-QTE 시퀀스(암전/줌/카메라 연출 + 안전 복귀)
```csharp
// EnemyQTEHandler.cs
qteEffect.playerTarget = player;
qteEffect.enemyTarget  = transform;
qteEffect.StartQTEEffects();         // 암전/줌/카메라 이동
yield return new WaitForSecondsRealtime(qteFreezeDuration);
bool success = qteUI != null && qteUI.IsSuccess();
```

### 전역 위협 오버레이(최대 강도만 반영)
```csharp
// EnemyVolumeOverlay.cs
private readonly Dictionary<int, float> intensities = new();
foreach (var kv in intensities) if (kv.Value > target) target = kv.Value;
overlayVolume.enabled = (currentWeight > enableEpsilon);
```

## 사용 팁 / 권장 설정

- **Detection**: 전방 각도/거리 > 후방보다 넓/깊게. 플레이어 좌우 반전은 `transform.localScale.x` 활용.
- **Movement**: `obstacleMask`는 벽/가구 등 통과 불가 레이어만 지정. `stuckMaxTime`은 0.25~0.4s 권장.
- **Doors**: 도착지 `OverlapCircle` 체크 반경은 타일/콜라이더 두께에 맞춰 0.2~0.4 추천.
- **QTE**: 연출은 모두 **Unscaled Time** 기반. 애니메이션 이벤트 누락 시에도 **보정 로직**으로 복귀.
- **Overlay**: 여러 트리거가 겹칠 때 가장 강한 값만 사용(색상은 하나). `enableEpsilon`으로 OFF 임계 조정.

## 확장 포인트

- **적 변형 추가**: `EnemyAI` 상속 → `Update`/전용 타이머/텔레포트 규칙만 오버라이드.
- **다른 감지 방식**: `EnemyDetection`에 라인캐스트/가림 처리 등을 덧붙여 시야 품질 향상.
- **사운드 이벤트 체계**: `SoundEventConfig`(SO)로 맵/퍼즐별 범위/지속시간 프리셋 운영.

---

**문의/수정 포인트**  
- 실제 프로젝트 폴더명/경로가 다르면 상단 구조를 맞춰 드리겠습니다.  
- 발표 슬라이드 분량/톤에 맞춰 **한 줄 요약형** 버전도 제공합니다.


## 상태 일람 & 전환 규칙 (간단 요약)

> 발표용: 각 상태의 **의도/트리거/종료**만 콕 찍어 정리

### IdleState
- **의도**: 짧은 숨 고르기 후 자연스럽게 순찰로 전환
- **Enter**: `Animator.SetBool("IsIdle", true)`; 대기 시간 1~3초 랜덤
- **Update**: 대기 타이머 0이 되면 `Patrol` 전환
- **Exit**: `IsIdle` 해제

### PatrolState
- **의도**: 랜덤 드리프트 중심의 자연스러운 순찰
- **Enter**: `Animator.SetBool("IsWalking", true)`; `Movement.PickRandomDirection()`
- **FixedUpdate**: `Movement.PatrolMove()`
- **Update**: 플레이어가 실제 시야(거리/각도) 안이면 `Chase` 전환
- **Exit**: `IsWalking` 해제

### ChaseState
- **의도**: 플레이어 실시간 추격 + 놓쳤을 때 짧은 수색
- **Enter**: `IsWalking` 활성; `Movement.SetForcedTarget(null)`(실시간 추격)
- **Update**: 
  - 보이면 실시간 추격 유지(마지막 위치 갱신)
  - **놓치면** 마지막 위치로 **2초 수색**, 종료 시 `Patrol` 복귀
- **FixedUpdate**: `Movement.ChasePlayer()`
- **Exit**: `IsWalking` 해제; 강제 타깃 해제

### InvestigateState
- **의도**: 컷씬/퍼즐/사운드 등으로 지정 지점 **강제 이동**
- **Setup**: `Setup(Vector3 position)`으로 대상 설정
- **Enter**: `IsWalking` 활성; `Movement.SetForcedTarget(position)`
- **FixedUpdate**: `Movement.PatrolMove()`(강제 타깃을 향해 보행)
- **Exit**: `IsWalking` 해제; 강제 타깃 해제

### QTEState
- **의도**: 잡힘 시 QTE 시퀀스 시작(암전/줌/연출은 QTEEffectManager 담당)
- **Enter**: `QTEHandler.StartQTE()` 호출
- **Update/Exit**: 결과에 따라 게임 로직에서 후속 상태로 복귀(예: Idle/Patrol)

---

### 핵심 전환 흐름 (요약)
- `Idle → Patrol`: 대기 타이머 소진
- `Patrol → Chase`: 실제 시야 감지 성공
- `Chase → Patrol`: 목표 상실 후 **2초 수색** 종료
- `Any → Investigate`: 이벤트/연출로 강제 이동 필요 시
- `Any → QTE`: 플레이어에게 **잡힘** 발생 시
- `QTE → (Idle|Patrol)`: QTE 종료 후 복귀(성공/실패에 따라 분기)

```
            
(잡힘) Any ──▶ QTE ──▶ (Idle | Patrol | GameOver)
