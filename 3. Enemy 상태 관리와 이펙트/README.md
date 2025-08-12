> # 📂 Enemy 폴더 구조
```cs
Enemy/
 ├── EnemyAI.cs                       # 상태 머신 기반 적 AI (Idle, Patrol, Chase, QTE, Investigate)
 ├── EnemyMovementController.cs       # 순찰/추격 이동, 충돌 회피, 순간이동
 ├── EnemyDetection.cs                # 시야·사운드 감지 로직
 ├── EnemyQTEHandler.cs               # QTE(잡힘) 시퀀스 제어
 ├── QTEEffectManager.cs               # QTE 시각 연출 (암전, 카메라 이동)
 ├── EnemyVolumeTrigger.cs            # 전역 위협 볼륨 트리거
 └── EnemyVolumeOverlay.cs            # 전역 위협 볼륨 UI 반영
```

> # 핵심 설계

### 1) 상태 머신 중심의 적 AI
**EnemyAI**
- Idle / Patrol / Chase / QTE / Investigate **5개 상태** 보유.
- `ChangeState`로 전환하며, `FixedUpdate`에서 QTE 중·일시정지 시 이동 로직 차단.
- 컷씬·퍼즐 연출은 **Investigate** 상태를 사용해 지정 위치 이동 + 복귀 코루틴 실행.

### 2) 자연스러운 순찰·추격 이동
**EnemyMovementController**
- 주기적인 **랜덤 드리프트**로 방향 미세 변경.
- `Raycast` 기반 축별 충돌 회피(막히면 해당 축만 제거).
- **막힘 감지** 시 짧은 Idle 후 재개해 부드러운 움직임 유지.
- 문 트리거 진입 시 **확률적 순간이동** + 도착지 안전 검사 + **일시 충돌 무시**로 끼임 방지.

### 3) 감지 모델: 듀얼 콘 + 전방위 사운드 모드
**EnemyDetection**
- 전방/후방 **듀얼 콘**으로 시야 판정, 플레이어 방향 반영.
- 사운드 추격 중엔 각도 무시, 반경 기반 **전방위 감지**로 전환.

### 4) 사운드 트리거형 추격 텔레포트
**SoundTrigger → EnemyAI**
- 사운드 범위 내 적에게 **순간이동 연출 후 Chase** 진입.
- 일정 시간 동안만 전방위 감지 유지, 종료 시 페이드 아웃 + 원위치 복귀.

### 5) QTE(잡힘) 시퀀스 & 연출
**EnemyQTEHandler + QTEEffectManager**
- QTE 동안 **암전·줌·카메라 이동**을 `UnscaledTime`으로 연출.
- 목숨 1개 시 즉사, 그 외엔 성공 시 목숨 1 감소 후 원위치 복귀.
- **세이프티 루프**와 **보정 이동**으로 빌드 환경에서도 안정 동작.

### 6) 전역 위협 오버레이
**EnemyVolumeTrigger → EnemyVolumeOverlay**
- 여러 적/구역이 겹칠 경우 **가장 강한 강도만** 전역 볼륨 반영.
- 가중치가 거의 0이면 볼륨 비활성화로 성능 절감.

> # 코드 예시
### 상태 머신 & 사운드 추격
```cs
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
```cs
// EnemyDetection.cs
public bool IsInVision(Transform target) { /* 전/후방 콘 판정 */ }
public bool IsWithinRadius(Transform t, float r) => Vector2.Distance(tr.position, t.position) <= r;
```

### 자연스러운 패트롤(막힘 → Idle → 재개)
```cs
// EnemyMovementController.cs
if (stuckTimer >= stuckMaxTime) {
    stuckTimer = 0f;
    PickRandomDirection();
    if (!briefIdleRunning) StartCoroutine(BriefIdleThenPatrol());
}
```

### 문 순간이동(안전 확인 + 충돌 무시)
```cs
// EnemyMovementController.cs
Vector3 dest = door.GetTargetDoor()?.position ?? (Vector3)door.GetTargetPos();
if (Physics2D.OverlapCircle(dest, teleportSafetyRadius, obstacleMask)) { 
    PickRandomDirection(); return; 
}
Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true); // 1초 뒤 복원
transform.position = dest;
```
### QTE 시퀀스(암전·줌·카메라 연출 + 안전 복귀)
```cs
// EnemyQTEHandler.cs
qteEffect.playerTarget = player;
qteEffect.enemyTarget  = transform;
qteEffect.StartQTEEffects();
yield return new WaitForSecondsRealtime(qteFreezeDuration);
bool success = qteUI != null && qteUI.IsSuccess();
```
### 전역 위협 오버레이
```cs
// EnemyVolumeOverlay.cs
private readonly Dictionary<int, float> intensities = new();
foreach (var kv in intensities) if (kv.Value > target) target = kv.Value;
overlayVolume.enabled = (currentWeight > enableEpsilon);
```

> # 설계 특징(요약)
- **빌드 안정성**: QTE/연출은 `Time.unscaledDeltaTime`, `WaitForSecondsRealtime`로 프레임·TimeScale 영향 최소화.
- **충돌 안전성**: 순간이동 시 도착지 `Overlap` 검사, 잠깐의 충돌 무시로 끼임 방지.
- **모듈화**: 감지/이동/QTE/연출/오버레이를 책임 단위로 분리.
- **퍼포먼스**: 가중치≈0일 땐 오버레이 완전 OFF, `Update` 내 `Find` 호출 없음.
- **확장성**: `EnemyAI` 상속 후 동작만 오버라이드하면 변형 적 추가 가능.
