using TMPro;
using UnityEngine;

public class Ch1_Shower : BasePossessable
{
    public bool IsHotWater => isWater && temperature == 3;

    [SerializeField] private GameObject water;
    [SerializeField] private GameObject steamEffect;
    [SerializeField] private GameObject temperatureArch;
    [SerializeField] private GameObject Needle;
    [SerializeField] private AudioClip onWaterSound; // 물소리
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject q_key;

    private bool isWater = false;
    private bool isWaterSoundPlaying = false;
    private bool isPlayerNear = false;

    private int temperature = 0;

    private Quaternion initialNeedleRotation;

    protected override void Start()
    {
        base.Start();

        UI.SetActive(false);
        q_key.SetActive(false);
        water.SetActive(false);
        temperatureArch.SetActive(false);
        Needle.SetActive(false);
        initialNeedleRotation = Needle.transform.rotation; // 바늘 회전값 저장 & 초기화
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        // 조작키 UI 상태
        q_key.SetActive(!isWater);
        water.SetActive(isWater);
        UI.SetActive(isWater);
        temperatureArch.SetActive(isWater);
        Needle.SetActive(isWater);

        if (Input.GetKeyDown(KeyCode.D))
        {
            temperature = Mathf.Max(-3, temperature - 1);
            UpdateNeedleRotation();
            Debug.Log("온도 조절: " + temperature);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            temperature = Mathf.Min(3, temperature + 1);
            UpdateNeedleRotation();
            Debug.Log("온도 조절: " + temperature);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isWater = !isWater;

            // 물소리 껐다 켰다
            if (!isWaterSoundPlaying)
            {
                SoundManager.Instance.PlayLoopingSFX(onWaterSound);
                isWaterSoundPlaying = true;
            }
            else
            {
                SoundManager.Instance.FadeOutAndStopLoopingSFX();
                isWaterSoundPlaying = false;
            }
        }

        if (steamEffect != null)
        {
            steamEffect.SetActive(isWater && temperature == 3);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            temperatureArch.SetActive(false);
            Needle.SetActive(false);

            temperature = 0;
            UpdateNeedleRotation();
            UI.SetActive(false);
            q_key.SetActive(false);
            return;
        }

    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated) return;

        if (other.CompareTag("Player"))
        {
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
            isPlayerNear = true;

            if (isWater)
            {
                SoundManager.Instance.PlayLoopingSFX(onWaterSound);
                isWaterSoundPlaying = true;
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
            isPlayerNear = false;

            if (PossessionSystem.Instance.CanMove)
            {
                SoundManager.Instance.FadeOutAndStopLoopingSFX();
                isWaterSoundPlaying = false;
            }
        }
    }

    private void UpdateNeedleRotation()
    {
        if (Needle != null)
        {
            float zAngle = temperature * 20f;
            Needle.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
        }
    }

    public override void OnPossessionEnterComplete() 
    {
        q_key.SetActive(!isWater);
    }
}
