using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ch3_LockerSelector : MonoBehaviour
{
    public int RemainingOpens = 2;
    [SerializeField] private GameObject b1fDoor;
    [SerializeField] private LockedDoor lockedb1fDoor;
    public bool IsSolved { get; private set; } = false;
    
    // [Header("정답 시 등장 오브젝트 연출")]
    // [SerializeField] private Transform rewardObject;   // 기억조각 오브젝트
    // [SerializeField] private Transform targetPoint;    // 플레이어 앞 위치
    // [SerializeField] private float dropDuration = 2f;
    //
    // [Header("카메라 이동 관련")]
    // [SerializeField] private Cinemachine.CinemachineVirtualCamera playerCam;
    // [SerializeField] private Cinemachine.CinemachineVirtualCamera rewardCam;
    //
    // [SerializeField] private float cameraReturnDelay = 2f;
    
    [SerializeField] private List<ClueData> requiredClues;
    private List<Ch3_Locker> openedLockers = new List<Ch3_Locker>();
    public bool IsPenaltyActive { get; private set; } = false;
    
    [SerializeField] private int opensPerAttempt = 2;
    private int currentAttempt = 1;

    [SerializeField] private Light2D globalLight;
    [SerializeField] private float blackoutHoldSeconds = 10f;
    [SerializeField] private float fadeOutTime = 0.6f;
    [SerializeField] private float fadeInTime = 1.0f;
    
    [SerializeField] private LockedDoor penaltyDoor;
    
    private float _origGlobalIntensity;
    private Color  _origGlobalColor;
    private bool   _globalLightCached = false;
    
    private Light2D _playerLight;

    [SerializeField] private float gameOverDelay = 4f;
    private bool gameOverQueued = false;
    private Collider2D _possessionTrigger;
    
    private void Awake()
    {
        currentAttempt = 1;
        RemainingOpens = Mathf.Max(1, opensPerAttempt);
    }
    
    public void OnCorrectBodySelected()
    {
        b1fDoor.SetActive(true);
        IsSolved = true;
        ConsumeClue(requiredClues);
        UIManager.Instance.PromptUI.ShowPrompt("※ 시신 확인 완료. 지하실 입구가 활성화되었습니다.", 2f);

        var lockers = FindObjectsOfType<Ch3_Locker>();
        foreach (var locker in lockers)
        {
            locker.SetActivateState(false);
            if (!locker.IsCorrectBody)
            {
                locker.TryClose();
            }
            else
            {
                var col = locker.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
        
        StartCoroutine(DelaySolvePuzzle());
    }

    private IEnumerator DelaySolvePuzzle()
    {
        yield return null; // 다음 프레임
        lockedb1fDoor.SolvePuzzle();
    }
    
    public void OnWrongBodySelected()
    {
        if (RemainingOpens > 0) return;

        if (currentAttempt >= 3)
        {
            if (!gameOverQueued)
            {
                gameOverQueued = true;

                EnsurePossessionTrigger();
                if (_possessionTrigger != null) _possessionTrigger.enabled = false;

                UIManager.Instance.PromptUI.ShowPrompt("더 이상은 힘들어", gameOverDelay);
                StartCoroutine(GameOverAfterPrompt(gameOverDelay));
            }
            return;
        }
        
        UIManager.Instance.PromptUI.ShowPrompt("불이 꺼졌어... 무서워...", 2f);
        StartCoroutine(ResetLockersAfterPenalty());
    }
    
    private void EnsurePossessionTrigger()
    {
        if (_possessionTrigger != null) return;
        var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (player == null) return;
        _possessionTrigger = player.GetComponent<Collider2D>();
    }
    
    private IEnumerator GameOverAfterPrompt(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        PlayerLifeManager.Instance.HandleGameOver();
    }
    
    private IEnumerator ResetLockersAfterPenalty()
    {
        IsPenaltyActive = true;
        
        var lockers = FindObjectsOfType<Ch3_Locker>();
        foreach (var locker in lockers)
            locker.SetActivateState(false);

        bool isSecondAttemptFail = (currentAttempt == 2);
        
        if (isSecondAttemptFail && penaltyDoor != null)
            penaltyDoor.LockPair();
        
        CacheGlobalLightIfNeeded();
        SetPlayerLightEnabled(true);
        
        yield return StartCoroutine(FadeGlobalLightIntensity(0f, fadeOutTime));
        
        yield return new WaitForSeconds(blackoutHoldSeconds);
        
        if (isSecondAttemptFail && penaltyDoor != null)
            penaltyDoor.SolvePuzzle();
        
        foreach (var opened in openedLockers)
        {
            opened.TryClose();
            opened.SetActivateState(HasAllClues());
        }
        openedLockers.Clear();
        RemainingOpens = Mathf.Max(1, opensPerAttempt);
        
        yield return StartCoroutine(FadeGlobalLightIntensity(_origGlobalIntensity, fadeInTime));
        
        SetPlayerLightEnabled(false);
        
        currentAttempt = Mathf.Min(3, currentAttempt + 1);
        IsPenaltyActive = false;
    }
    
    private void CacheGlobalLightIfNeeded()
    {
        if (globalLight == null) return;

        if (!_globalLightCached)
        {
            _origGlobalIntensity = globalLight.intensity;
            _origGlobalColor     = globalLight.color;
            _globalLightCached   = true;
        }
    }

    private IEnumerator FadeGlobalLightIntensity(float target, float duration)
    {
        if (globalLight == null || duration <= 0f)
        {
            if (globalLight != null) globalLight.intensity = target;
            yield break;
        }

        float start = globalLight.intensity;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            globalLight.intensity = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        globalLight.intensity = target;
    }

    private void SetPlayerLightEnabled(bool on)
    {
        if (_playerLight == null)
        {
            var player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player != null)
                _playerLight = player.GetComponentInChildren<Light2D>(true);
        }

        if (_playerLight != null)
            _playerLight.enabled = on;
    }
    
    public bool HasAllClues()
    {
        foreach (var clue in requiredClues)
        {
            if (!UIManager.Instance.Inventory_PlayerUI.collectedClues.Contains(clue))
                return false;
        }
        return true;
    }
    
    private void ConsumeClue(List<ClueData> clues)
    {
        UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());
    }

    public void RegisterOpenedLocker(Ch3_Locker locker)
    {
        if (!openedLockers.Contains(locker))
            openedLockers.Add(locker);
    }
}
