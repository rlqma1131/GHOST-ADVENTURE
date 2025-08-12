using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ch2_MorseKey : BasePossessable
{
    [Header("UI 그룹")]
    [SerializeField] private CanvasGroup panelCanvasGroup; // 모스키 입력 판넬
    [SerializeField] private RectTransform inputAreaUI; // 입력 영영 버튼UI

    [Header("모스 부호 UI")]
    [SerializeField] private TMP_Text morseDisplayText;      // 화면 하단: 현재 입력 중인 모스부호

    [Header("알파벳 UI")]
    [SerializeField] private TextMeshProUGUI decodedDisplayText;    // 화면 중앙: 해석된 알파벳들
    
    [Header("모스키 입력 소리")]
    [SerializeField] private AudioClip dotInputSFX;
    [SerializeField] private AudioClip dashInputSFX;

    [Header("기억 조각")]
    [SerializeField] private GameObject handprint;
    [SerializeField] private AudioClip mudSFX;

    private Ch2_MemoryPositive_01_HandPrint memory;
    private bool isSuccessAnimating = false;
    private bool isTweening = false;
    private Coroutine shakeCoroutine;

    private Dictionary<string, char> morseToChar = new Dictionary<string, char>()
{
    { ".-", 'A' },
    { "-...", 'B' },
    { "-.-.", 'C' },
    { "-..", 'D' },
    { ".", 'E' },
    { "..-.", 'F' },
    { "--.", 'G' },
    { "....", 'H' },
    { "..", 'I' },
    { ".---", 'J' },
    { "-.-", 'K' },
    { ".-..", 'L' },
    { "--", 'M' },
    { "-.", 'N' },
    { "---", 'O' },
    { ".--.", 'P' },
    { "--.-", 'Q' },
    { ".-.", 'R' },
    { "...", 'S' },
    { "-", 'T' },
    { "..-", 'U' },
    { "...-", 'V' },
    { ".--", 'W' },
    { "-..-", 'X' },
    { "-.--", 'Y' },
    { "--..", 'Z' }
};

    private string currentMorseChar = "";
    private List<char> decodedLetters = new List<char>();

    private float lastInputTime = 0f;
    private const float letterGap = 1.5f; // 글자 간격
    private const float resetThreshold = 10f; // 전체 리셋 간격

    private bool isPressing = false;
    private float pressStartTime;
    private const float dashThreshold = 0.25f;

    private void Awake()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;          // 완전 투명
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        memory = handprint.GetComponent<Ch2_MemoryPositive_01_HandPrint>();
        handprint.SetActive(false);

        currentMorseChar = "";
        decodedLetters.Clear();
        UpdateUI();
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        // 입력 감지 (Dot / Dash)
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverInputArea())
                return;

            isPressing = true;
            pressStartTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.E) && !isTweening)
        {
            Unpossess();

            EnemyAI.ResumeAllEnemies();
            UIManager.Instance.PlayModeUI_OpenAll();
            StartCoroutine(FadeOutPanel(0.2f));

            // UI 초기화
            decodedLetters.Clear();
            currentMorseChar = "";
            UpdateUI();
        }
#if UNITY_EDITOR
        else if (Input.GetKeyDown(KeyCode.Q)) // 치트키 (에디터 전용)
        {
            decodedLetters.Clear();
            decodedLetters.AddRange(new char[] { 'H', 'E', 'L', 'P' });
            UpdateUI();

            StartSuccessShake(); // 진동 + 확대 + 빨갛게
        }
#endif

        if (isPressing && Input.GetMouseButtonUp(0))
        {
            isPressing = false;
            float heldTime = Time.time - pressStartTime;
            lastInputTime = Time.time;

            if (heldTime < dashThreshold)
            {
                OnDotInput();
                SoundManager.Instance.PlaySFX(dotInputSFX); // 점 입력 소리
            }
            else
            {
                OnDashInput();
                SoundManager.Instance.PlaySFX(dashInputSFX); // 대시 입력 소리
            }
        }

        float timeSinceLastInput = Time.time - lastInputTime;

        // 자동 글자 구분 처리
        if (currentMorseChar.Length > 0 && timeSinceLastInput > letterGap)
        {
            DecodeCurrentMorseChar();
        }

        // 전체 입력 리셋 처리
        if (!isSuccessAnimating && (decodedLetters.Count > 0 || currentMorseChar.Length > 0) 
            && timeSinceLastInput > resetThreshold)
        {
            Debug.Log("입력이 오래 멈췄습니다. 전체 초기화.");
            currentMorseChar = "";
            decodedLetters.Clear();
            UpdateUI();
        }
    }


    private void UpdateUI()
    {
        decodedDisplayText.text = new string(decodedLetters.ToArray());

        if (!isSuccessAnimating)
        {
            morseDisplayText.text = ConvertToVisualMorse(currentMorseChar);
        }
    }

    private string ConvertToVisualMorse(string rawMorse)
    {
        return rawMorse.Replace(".", "·").Replace("-", "–"); // 최종 출력 모스 부호
    }

    private void OnDotInput()
    {
        if (currentMorseChar.Length >= 4)
            return;

        currentMorseChar += ".";
        Debug.Log("입력: Dot (.)");
        UpdateUI();
    }

    private void OnDashInput()
    {
        if (currentMorseChar.Length >= 4)
            return;

        currentMorseChar += "-";
        Debug.Log("입력: Dash (-)");
        UpdateUI();
    }

    private void DecodeCurrentMorseChar()
    {
        if (morseToChar.TryGetValue(currentMorseChar, out char letter))
        {
            decodedLetters.Add(letter);
        }
        else
        {
            Debug.Log($"잘못된 모스부호 입력: {currentMorseChar}");
        }

        currentMorseChar = "";
        lastInputTime = Time.time;
        UpdateUI();

        // 최대 4자 초과 방지
        if (decodedLetters.Count >= 4)
        {
            string currentWord = new string(decodedLetters.ToArray());
            Debug.Log($"입력된 단어: {currentWord}");

            // 정답
            if (currentWord == "HELP")
            {
                StartSuccessShake(); // 진동 + 확대 + 빨갛게
            }
            // 오답
            else
            {
                Debug.Log("틀린 단어입니다. 초기화");
                StartShakeAndClear(); // 진동 + 초기화
            }
        }

    }

    private void StartShakeAndClear()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeAndClear());
    }

    private IEnumerator ShakeAndClear()
    {
        TMP_TextInfo textInfo = decodedDisplayText.textInfo;
        decodedDisplayText.ForceMeshUpdate();

        float duration = 0.5f; // 흔들리는 총 시간
        float timer = 0f;

        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 offset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ) * 3f; // 흔들림 세기

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalVertices[materialIndex][vertexIndex + j] + offset;
                }
            }

            // Mesh 업데이트
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                decodedDisplayText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }

        // 흔들림 후 텍스트 지우기
        decodedLetters.Clear();
        currentMorseChar = "";
        UpdateUI();
    }

    private void StartSuccessShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        UIManager.Instance.Inventory_PlayerUI.RemoveCluesByStage(GameManager.GetStageForCurrentChapter());
        shakeCoroutine = StartCoroutine(ShakeSuccess());
    }

    private IEnumerator ShakeSuccess()
    {
        isTweening = true;
        isSuccessAnimating = true;

        decodedDisplayText.ForceMeshUpdate();
        yield return null;

        TMP_TextInfo textInfo = decodedDisplayText.textInfo;

        float duration = 3f;
        float timer = 0f;

        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }

        float minShake = 0.5f;
        float maxShake = 10f;

        Color32 originalColor = decodedDisplayText.color;
        Color32 targetColor = new Color32(255, 0, 0, 255); // 빨강

        yield return new WaitForSeconds(1.5f); // 약간의 딜레이 후 시작

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float shakeStrength = Mathf.Lerp(minShake, maxShake, t);
            Color32 lerpedColor = Color32.Lerp(originalColor, targetColor, t); // 점점 빨개짐

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

                Vector3 offset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ) * shakeStrength;

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalVertices[materialIndex][vertexIndex + j] + offset;
                    vertexColors[vertexIndex + j] = lerpedColor; // 💡 색상 적용
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                decodedDisplayText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }

        yield return StartCoroutine(FadeOutPanel(0.2f));


        revealMemory(); // 기억 조각 나타남
        memory.ActivateHandPrint();

        Unpossess();

        isTweening = false;
        UIManager.Instance.PlayModeUI_OpenAll();

        hasActivated = false; // 더 이상 빙의 불가능
        MarkActivatedChanged();
    }

    private IEnumerator FadeInPanel(float duration)
    {
        isTweening = true;

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }


        panelCanvasGroup.alpha = 1f;

        isTweening = false;
    }

    private IEnumerator FadeOutPanel(float duration)
    {
        float startAlpha = panelCanvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t); // 점점 투명하게
            yield return null;
        }

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }

    // 입력 영역 눌렀는지 확인
    private bool IsPointerOverInputArea()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == inputAreaUI.gameObject || result.gameObject.transform.IsChildOf(inputAreaUI))
                return true;
        }

        return false;
    }

    void revealMemory()
    {
        SoundManager.Instance.PlaySFX(mudSFX); // 진흙 소리
        handprint.SetActive(true);
    }
    public override void OnPossessionEnterComplete() 
    { 
        EnemyAI.PauseAllEnemies();
        UIManager.Instance.PlayModeUI_CloseAll();
        StartCoroutine(FadeInPanel(1.0f)); // 판넬 페이드 인
    }
}
