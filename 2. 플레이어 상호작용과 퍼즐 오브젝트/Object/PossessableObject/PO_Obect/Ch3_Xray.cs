using UnityEngine;
using UnityEngine.UI;

public class Ch3_Xray : BasePossessable
{
    [Header("X-Ray 기계")]
    [SerializeField] private GameObject[] xRayHead; // 머리 ~ 발 순서
    [SerializeField] private AudioClip scan;

    [Header("X-Ray 줌 화면")]
    [SerializeField] Image zoomPhotoScreen;

    [Header("사진")]
    [SerializeField] private Image[] zoomPhotos; // 머리 ~ 발 사진들
    
    [Header("조작키")]
    [SerializeField] private GameObject aKey;
    [SerializeField] private GameObject dKey;
    [SerializeField] private GameObject spaceKey;

    public int currentPhotoIndex = 0;

    private bool isFirstPossess = true;
    private bool isFirstScan = true;

    protected override void Start()
    {
        base.Start();

        aKey.SetActive(false);
        dKey.SetActive(false);
        spaceKey.SetActive(false);

        currentPhotoIndex = 0;
        UpdateXrayDisplay();
    }

    protected override void Update()
    {
        if (!isPossessed) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            aKey.SetActive(false);
            dKey.SetActive(false);
            spaceKey.SetActive(false);

            Unpossess();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentPhotoIndex < zoomPhotos.Length - 1)
            {
                currentPhotoIndex++;
            }

            UpdateXrayDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentPhotoIndex > 0)
            {
                currentPhotoIndex--;
            }

            UpdateXrayDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SoundManager.Instance.PlaySFX(scan, 1f);
            Scan();

            if (isFirstScan)
            {
                isFirstScan = false;
                UIManager.Instance.PromptUI.ShowPrompt("촬영됐다. 모니터를 확인해볼까?");
            }
        }
    }
    private void UpdateXrayDisplay()
    {
        // xRayHead 위치 이동
        for (int i = 0; i < xRayHead.Length; i++)
        {
            xRayHead[i].SetActive(i == currentPhotoIndex);
        }
    }

    private void Scan()
    {
        // 줌 이미지 전환
        zoomPhotoScreen.sprite = zoomPhotos[currentPhotoIndex].sprite;
    }

    public void ScanPrompt()
    {
        Sprite monitorPhoto = zoomPhotoScreen.sprite;

        if(monitorPhoto = zoomPhotos[0].sprite)
        {

        }
    }

    public override void OnPossessionEnterComplete()
    {
        aKey.SetActive(true);
        dKey.SetActive(true);
        spaceKey.SetActive(true);

        if (isFirstPossess)
        {
            isFirstPossess = false;
            UIManager.Instance.PromptUI.ShowPrompt("X-ray 기계인가? 촬영해볼까");
        }
    }
}
