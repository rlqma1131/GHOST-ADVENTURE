## 🎬 연출 시스템 및 렌더링

본 프로젝트는 게임의 스토리텔링과 시각적 몰입감을 극대화하기 위해 진행되었습니다.  
타임라인 중심의 **모듈식 컷씬 시스템**을 구축하고, Additive 씬을 로드하여 기존 씬 위에 연출을 재생하는 방식을 채택했습니다.  
이 방식은 씬 전환 시 몰입감을 유지하고, 연출 종료 후에도 **원활한 게임플레이 복귀**를 보장합니다.

**URP(Universal Render Pipeline)**와 포스트 프로세싱을 도입하여 **비주얼 퀄리티와 성능**을 모두 개선했습니다.

---

## 💡 핵심 설계 및 설계 철학
- **컷씬 모듈화** + `TimelineControl`을 이용한 **중앙 관리**.
- 각 컷씬을 **독립적인 씬(Scene)** 으로 분리하여 모듈화.
- 컷씬 재생 동안 **TimeScale = 0** → UI, 사운드, 플레이어 제어 정지 → 종료 시 복구.
- 시스템 간 의존성을 낮추고, **안정적·확장 가능한 구조** 구현.
- 비주얼 퀄리티와 몰입감을 높이는 연출 설계에 집중.
- Unity Timeline + Cinemachine 활용, 제한된 인력/기간 내 높은 퀄리티 확보.
- 기획·제작·사운드 믹싱·코드 구현까지 **연출 파트 전담**.

---

## ✨ 주요 특징
1. **본 게임 환경 격리**
   - 컷씬 재생 시 메인 씬 `Time.timeScale = 0`
   - 컷씬 씬은 **Additive 로드**로 완전 분리

2. **타임라인 기반 제어**
   - 유니티 Timeline으로 애니메이션·카메라·사운드 제어
   - `Signal Emitter`를 통해 코드와 상호작용

3. **매니저 기반 상태 복원**
   - 컷씬 종료 시 SoundManager, UIManager, GameManager 등이 상태를 복구

4. **직관적인 스킵 기능**
   - `UnscaledGameTime` 기반, 3초간 키를 눌러야 스킵
   - 실수 스킵 방지, 직관적 UI/UX 구현

---

## 📂 폴더 구조
```cs
01.Scripts/TimeLine # 컷씬 제어 로직 (시작/종료/재생/일시정지/스킵)
01.Scripts/Image # 커스텀 이미지 처리 (PixelExploder 등)
01.Scripts/Camera # 카메라 제어 및 전환 스크립트
04.Images/Scene # 컷씬 이미지 리소스
06.Audio/TimeLine # 컷씬 오디오 리소스
10.TimeLine # Timeline 리소스 (PlayableAsset 등)
```
---

## 🛠 주요 기능 구현

### 1) 타임라인 기반 컷씬 시스템
- **문제**: 스크립트 기반 컷씬은 동기화 어려움 + 협업 효율 저하
- **개선**: Timeline 중심으로 시각 배치, 직관적 작업 환경 구축
- **결과**: 제작 시간 단축, 연출 완성도 및 디버깅 용이

### 2) Additive 씬 로드
- **문제**: 메인 씬에서 컷씬 재생 시 게임 로직 개입 오류
- **개선**: 컷씬 전용 씬 Additive 로드 + 메인 씬 Time 정지
- **결과**: 외부 개입 차단, 모듈화로 유지보수 용이

### 3) Signal Emitter 연동
- **문제**: 타임라인과 코드 이벤트 연동 불편
- **개선**: Signal Emitter로 특정 시점 코드 이벤트 실행
- **결과**: 연출-코드 유기적 연동, 이벤트 처리 용이

### 4) URP 도입
- **문제**: Built-in 파이프라인 성능 한계
- **개선**: URP 전환 + SRP Batcher 활성화
- **결과**: FPS 향상, Shader Graph로 생산성 증가

### 5) 포스트 프로세싱
- **문제**: URP 전환 후 화면 밋밋
- **개선**: Color Grading, Bloom 등 볼륨 기반 포스트 프로세싱 적용
- **결과**: 독창적 분위기 구축, 몰입감 강화

### 6) 자체 리소스 제작
- **문제**: 아트 스타일에 맞는 리소스 부재
- **개선**: 직접 녹음·촬영·편집하여 리소스 제작
- **결과**: 독창적 분위기 완성, 비용 절감, 디테일 향상

---

## 💻 코드 예시 및 분석

### TimelineControl.cs
- **역할**: Additive 씬의 타임라인 제어 + 종료 시 게임 상태 복원
- **핵심**
  - `UnscaledGameTime`로 업데이트 → TimeScale=0에서도 재생
  - `hold-to-Skip` 구현 (`Time.unscaledDeltaTime`)
  - `CloseScene()`에서 UI/사운드/플레이어 제어 복구

### CameraChange.cs
- **역할**: 영역 진입 시 카메라 전환
- **방식**: Trigger 진입 → 해당 카메라 Priority 상승 → 활성화

### CameraTargetSetter.cs
- **역할**: 카메라 Follow 타겟 안전 지정
- **방식**: 플레이어 생성 대기 후 Vcam의 Follow를 설정

### PlayerCamera.cs
- **역할**: 플레이어의 현재 카메라 상태 추적
- **방식**: Room 트리거 진입 시 해당 카메라 정보 저장

### CutsceneManager.cs
- **역할**: 컷씬 재생 및 상태 복원
- **특징**
  - `PlayableDirector` 재생/대기
  - `director.stopped` 이벤트로 종료 감지
  - Scene 언로드 시 안전 복구(TimeScale=1, UI 비활성화)

### TypewriterDialogue.cs
- **역할**: 타자기 효과 대화 시스템
- **기능**
  - 한 글자씩 출력, 흔들림 효과 지원
  - 수동/자동 모드
  - Timeline과 연동 → 대화 종료 시 재생 재개
  - `WaitForSecondsRealtime`로 TimeScale=0 상태에서도 정상 동작
