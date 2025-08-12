using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MoveScene : MonoBehaviour
{
    [SerializeField] Image skip; // 스킵 진행 상태를 표시할 이미지

    private float skipTimer = 0f; // S 키를 누른 시간을 측정하는 타이머
    private const float SKIP_DURATION = 3.0f; // 스킵에 필요한 시간 (3초)
    [SerializeField] private string nextSceneName;
    private bool isSkipActive = false; // 스킵 활성화 여부

    [SerializeField] Image space1;
    [SerializeField] Image space2;
    private LoadSceneMode currentLoadMode = LoadSceneMode.Single;
    private bool isHolding = false;
    private Coroutine flashingCoroutine;

    private void Awake()
    {
        if (skip != null)
            skip.fillAmount = 1f;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        flashingCoroutine = StartCoroutine(FlashImages());

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {

            isHolding = true;
            skipTimer += Time.unscaledDeltaTime;
            if (skip != null)
                skip.fillAmount = 1.0f - (skipTimer / SKIP_DURATION);

            if (skipTimer >= SKIP_DURATION)
            {
                isSkipActive = true;
            }
            ShowImage2Only();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // 타이머와 이미지 fillAmount 초기화
            isHolding = false;
            skipTimer = 0f;
            if (skip != null)
            {
                skip.fillAmount = 1f;
            }

            if (flashingCoroutine != null)
                StopCoroutine(flashingCoroutine);
            flashingCoroutine = StartCoroutine(FlashImages());
        }

        if (isSkipActive)
        {
            if (currentLoadMode != LoadSceneMode.Additive)
            {
                GoScene(nextSceneName);
            }
            //GoScene(nextSceneName);
            isSkipActive = false; // 스킵 상태 초기화
        }
    }

    public void GoScene(string Scenename)
    {
        // 타임라인이 종료되면 씬 이동
        GameManager.LoadThroughLoading(Scenename);
        //if (!GameManager.Instance.Player.activeSelf)
        //{
        //    //GameManager.Instance.Player.gameObject.SetActive(true); // 플레이어 활성화
        
        Debug.Log("씬 이동: " + Scenename);

        if(GameManager.Instance.Player != null)
        {
        PossessionSystem.Instance.CanMove = true; // 플레이어 이동 가능하도록 설정
        
        
        }

        //}
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentLoadMode = mode; // 로드 모드 저장
    }
    private IEnumerator FlashImages()
    {
        while (!isHolding)
        {
            if (space1 != null) space1.enabled = true;
            if (space2 != null) space2.enabled = false;

            yield return new WaitForSeconds(0.7f);

            if (space1 != null) space1.enabled = false;
            if (space2 != null) space2.enabled = true;
            yield return new WaitForSeconds(0.7f);
        }
    }

    private void ShowImage2Only()
    {
        if (flashingCoroutine != null)
            StopCoroutine(flashingCoroutine);

        if (space1 != null) space1.enabled = false;
        if (space2 != null) space2.enabled = true;
    }
}
