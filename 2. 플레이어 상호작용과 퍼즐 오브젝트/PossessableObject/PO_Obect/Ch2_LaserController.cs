using UnityEngine;

public class Ch2_LaserController : BasePossessable
{
    [Header("레이저")]
    [SerializeField] private GameObject laser;

    [Header("레이저 스크린")]
    [SerializeField] private GameObject laserScreen;

    [Header("조작키")]
    [SerializeField] private GameObject qKey;
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;

    private Animator laserScreenAnimator;
    private SpriteRenderer spriteRenderer;

    public System.Action<Ch2_LaserController> OnLaserDeactivated;
    public bool IsLaserActive => laser.activeSelf;
    private bool wasLaserActive = true;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        laserScreenAnimator = laserScreen.GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            qKey.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool laserActive = !laser.activeSelf;
            laser.SetActive(laserActive);
            laserScreenAnimator.SetBool("Off", !laserActive);
            spriteRenderer.sprite = laserActive ? on : off;

            // 비활성화 이벤트 감지
            if (wasLaserActive && !laserActive)
            {
                OnLaserDeactivated?.Invoke(this);
            }

            wasLaserActive = laserActive;
        }
    }

    public void ActivateController()
    {
        hasActivated = true;
        MarkActivatedChanged();
    }

    public override void OnPossessionEnterComplete() 
    {
        qKey.SetActive(true);
    }
}
