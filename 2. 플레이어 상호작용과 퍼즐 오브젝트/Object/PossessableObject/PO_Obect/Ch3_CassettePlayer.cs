using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ch3_CassettePlayer : BasePossessable
{
    [Header("소리 목록")]
    [SerializeField] private AudioClip dial;
    [SerializeField] private AudioClip buttonPush;
    [SerializeField] private AudioClip talkingSound; // 대화 소리
    [SerializeField] private AudioClip glitchSound; // 퍼즐 해결 후 소리

    [Header("자막 셋팅")]
    [SerializeField] private TMP_Text typingText;
    [SerializeField][TextArea] private string[] correctSentences;

    [Header("오디오 퍼즐 셋팅")]
    [SerializeField] private float minDistort = 0f;
    [SerializeField] private float maxDistort = 1f;
    [SerializeField] private float minPitch = 0.4f;
    [SerializeField] private float maxPitch = 2f;
    [SerializeField] private float distortionValue = 0.5f; // 초기값
    [SerializeField] private float playbackPitch = 0.6f;     // 초기 재생속도 (pitch)

    [Header("줌 화면")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject pitchDial;
    [SerializeField] private GameObject distortDial;
    [SerializeField] private GameObject playBtn;
    [SerializeField] private GameObject clueScreen;

    private bool isPlaying = false; // 재생 여부
    private bool isSolved = false; // 문제 해결 여부
    private bool isTalking = false;

    private float inputCooldown = 0.5f; // 조작키 입력 텀
    private float inputTimer = 0f;
    private bool canAdjust = true;
    private bool isFirstPossessin = true;

    private float answerDistortion = 0f; // 정답 주파수 조정값
    private float answerPitch = 1f; // 정답 재생 속도

    private AudioDistortionFilter distortion;
    private AudioSource audioSource;
    private Coroutine typingCoroutine;

    protected override void Start()
    {
        base.Start();
        zoomCamera.Priority = 5;

        audioSource = SoundManager.Instance.GetComponentInChildren<AudioSource>();
        distortion = SoundManager.Instance.GetComponentInChildren<AudioDistortionFilter>();

        distortionValue = 0.5f; // 초기값
        playbackPitch = 0.6f;     // 초기 재생속도 (pitch)

        typingText.text = "";
        distortion.distortionLevel = distortionValue;
        audioSource.pitch = playbackPitch;

        clueScreen.SetActive(false);
        playBtn.SetActive(false);
        UpdateDialRotation();
    }

    protected override void Update()
    {
        if (!isPossessed) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTalking)
                return;

            EnemyAI.ResumeAllEnemies();
            Unpossess();

            zoomCamera.Priority = 5;
            UIManager.Instance.PlayModeUI_OpenAll();

            if (isSolved)
            {
                UIManager.Instance.PromptUI.ShowPrompt("일단 이 정보를 입력해보려면 치료실을 찾아보자");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SoundManager.Instance.PlaySFX(buttonPush);

            if (isSolved)
            {
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.ChangeBGM(glitchSound);
            }
            else
            {
                if (!isPlaying)
                {
                    playBtn.SetActive(true);
                    PlayTalkingSound();
                    StartTyping();
                }
                else
                {
                    playBtn.SetActive(false);
                    StopTalkingSound();
                }
            }
        }

        if (!isPlaying)
            return;

        // 입력 쿨타임 처리
        if (!canAdjust)
        {
            inputTimer -= Time.deltaTime;
            if (inputTimer <= 0f)
                canAdjust = true;
            return; // 쿨타임 중엔 아래 조작 막음
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            distortionValue += 0.1f;
            distortionValue = Mathf.Clamp(distortionValue, minDistort, maxDistort);
            distortion.distortionLevel = distortionValue;
            TriggerInputCooldown();
            SoundManager.Instance.PlaySFX(dial);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            distortionValue -= 0.1f;
            distortionValue = Mathf.Clamp(distortionValue, minDistort, maxDistort);
            distortionValue = Mathf.Round(distortionValue * 100f) / 100f;
            distortion.distortionLevel = distortionValue;
            TriggerInputCooldown();
            SoundManager.Instance.PlaySFX(dial);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            playbackPitch += 0.2f;
            playbackPitch = Mathf.Clamp(playbackPitch, minPitch, maxPitch);
            audioSource.pitch = playbackPitch;
            TriggerInputCooldown();
            SoundManager.Instance.PlaySFX(dial);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playbackPitch -= 0.2f;
            playbackPitch = Mathf.Clamp(playbackPitch, minPitch, maxPitch);
            audioSource.pitch = playbackPitch;
            TriggerInputCooldown();
            SoundManager.Instance.PlaySFX(dial);
        }

        UpdateDialRotation();
        CheckSolved();
    }

    void TriggerInputCooldown()
    {
        canAdjust = false;
        inputTimer = inputCooldown;
    }

    void UpdateDialRotation()
    {
        //Distort 회전
        float distortT = Mathf.InverseLerp(minDistort, maxDistort, distortionValue); // 0~1 비율
        float distortAngle = Mathf.Lerp(-60f, 60f, distortT); // 각도 변환
        distortDial.transform.localRotation = Quaternion.Euler(0f, 0f, distortAngle); // Z축 회전

        //Pitch 회전
        float pitchT = Mathf.InverseLerp(minPitch, maxPitch, playbackPitch);
        float pitchAngle = Mathf.Lerp(-60f, 60f, pitchT);
        pitchDial.transform.localRotation = Quaternion.Euler(0f, 0f, pitchAngle);
    }

    private void StartTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypingRoutine());
    }

    IEnumerator TypingRoutine()
    {
        typingText.text = "";

        while (!isSolved)
        {
            yield return new WaitForSeconds(1f);

            float proximity = CalculateProximity(); // 정답과의 근접도 (0~1)
            string generatedText = GenerateCorruptedText(proximity);
            typingText.text = "";

            foreach (char c in generatedText)
            {
                typingText.text += c;
                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(1f);
        }

        // 정답 도달
        UIManager.Instance.PromptUI.ShowPrompt("상담 내용..? 왜 이렇게 익숙하지..");
        isTalking = true;

        // 정상 문장 출력
        for (int i = 0; i < correctSentences.Length; i++)
        {
            string sentence = correctSentences[i];
            typingText.text = "";

            if (i == correctSentences.Length - 1)
            {
                typingText.color = Color.red; // 마지막 문장은 빨간색
                yield return StartCoroutine(ShakeAndType(sentence, 0.02f));
            }
            else
            {
                typingText.color = Color.white; // 기본색
                foreach (char c in sentence)
                {
                    typingText.text += c;
                    yield return new WaitForSeconds(0.05f);
                }

                yield return new WaitForSeconds(2f);
            }
        }
    }

    IEnumerator ShakeAndType(string sentence, float interval)
    {
        typingText.text = "";
        typingText.ForceMeshUpdate();
        TMP_TextInfo textInfo = typingText.textInfo;

        for (int i = 0; i < sentence.Length; i++)
        {
            typingText.text += sentence[i];
            typingText.ForceMeshUpdate();

            // 진동 효과 (빠르게 흔들림)
            float duration = 0.1f;
            float timer = 0f;

            Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
            for (int m = 0; m < textInfo.meshInfo.Length; m++)
                originalVertices[m] = textInfo.meshInfo[m].vertices.Clone() as Vector3[];

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float shakeStrength = 0.5f;

                for (int j = 0; j < textInfo.characterCount; j++)
                {
                    if (!textInfo.characterInfo[j].isVisible) continue;

                    int meshIndex = textInfo.characterInfo[j].materialReferenceIndex;
                    int vertexIndex = textInfo.characterInfo[j].vertexIndex;

                    Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;

                    Vector3 offset = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        0f) * shakeStrength;

                    vertices[vertexIndex + 0] += offset;
                    vertices[vertexIndex + 1] += offset;
                    vertices[vertexIndex + 2] += offset;
                    vertices[vertexIndex + 3] += offset;
                }

                typingText.UpdateVertexData();
                yield return null;
            }

            yield return new WaitForSeconds(interval);
        }

        typingText.text = ""; // 진동 후 텍스트 초기화
        SoundManager.Instance.ChangeBGM(glitchSound);

        yield return new WaitForSeconds(2f);

        // 녹음본 내용 출력 끝
        Unpossess();

        isTalking = false;

        EnemyAI.ResumeAllEnemies();
        zoomCamera.Priority = 5;
        UIManager.Instance.PlayModeUI_OpenAll();
        UIManager.Instance.PromptUI.ShowPrompt("일단 이 정보를 입력해보려면 치료실을 찾아보자");
    }

    string GenerateCorruptedText(float proximity)
    {
        string[] jamos = { "ㄱ", "ㅁ", "ㄷ", "ㅂ", "ㅅ", "ㅈ", "ㅊ", "ㅋ", "ㅌ" };
        string[] partialHangul = { "어", "기", "분", "어떻", "자", "시", "아이", "죽", "납","치" };
        string symbols = "!@#$%^&*()-_+=~<>?";

        int length = Random.Range(20, 30);
        string result = "";

        for (int i = 0; i < length; i++)
        {
            float r = Random.value;

            if (proximity < 0.3f)
            {
                result += symbols[Random.Range(0, symbols.Length)];
            }
            else if (proximity < 0.8f)
            {
                if (r < 0.5f)
                    result += symbols[Random.Range(0, symbols.Length)];
                else if (r < 0.75f)
                    result += jamos[Random.Range(0, jamos.Length)];
                else
                    result += partialHangul[Random.Range(0, partialHangul.Length)];
            }
            else
            {
                if (r < 0.3f)
                    result += symbols[Random.Range(0, symbols.Length)];
                else
                    result += partialHangul[Random.Range(0, partialHangul.Length)];
            }
        }

        return result;
    }

    float CalculateProximity()
    {
        float d = Mathf.Abs(distortionValue - answerDistortion);
        float p = Mathf.Abs(playbackPitch - answerPitch);

        float dScore = 1f - Mathf.InverseLerp(0f, 1f, d);
        float pScore = 1f - Mathf.InverseLerp(0f, 1.6f, p);

        return Mathf.Min(dScore, pScore); // 둘 중 더 작은 근접도로 안정성 확보
    }

    void PlayTalkingSound()
    {
        isPlaying = true;
        SoundManager.Instance.ChangeBGM(talkingSound, 1f, 1.2f);
    }

    void StopTalkingSound()
    {
        isPlaying = false;
        SoundManager.Instance.StopBGM();
    }

    void CheckSolved()
    {
        if (Mathf.Abs(distortionValue - answerDistortion) < 0.05f && Mathf.Abs(playbackPitch - answerPitch) < 0.05f)
        {
            hasActivated = false;
            MarkActivatedChanged();

            isPlaying = false;
            isSolved = true;

            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(talkingSound);

            clueScreen.SetActive(true);
        }
    }

    public override void OnPossessionEnterComplete() 
    {
        typingText.text = "";

        EnemyAI.PauseAllEnemies();
        UIManager.Instance.PlayModeUI_CloseAll();
        zoomCamera.Priority = 20;

        if (isFirstPossessin)
        {
            isFirstPossessin = false;
            UIManager.Instance.PromptUI.ShowPrompt("잡음 투성이야. 주파수랑 속도를 맞추면..");
        }
    }
}

