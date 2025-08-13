using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ESCMenu : MonoBehaviour, IUIClosable
{
    public static ESCMenu Instance {get; private set;}

    [SerializeField] private GameObject escMenuUI; // ESCMenu Canvas
    // [SerializeField] private SoundMenu soundMenu;
    [SerializeField] private GameObject general;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject sound;
    [SerializeField] private GameObject keybind;
    
    [Header("Sound")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    private const float DEFAULT_SLIDER = 0.5f;

    [Header("Language")]
    [SerializeField] private Dropdown languageDropdown;

    [Header("Keybind")]
    [SerializeField] private Button rebindJumpButton;

    private bool isPaused = false;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 기본 키 설정 (Clue1~5)
        for (int i = 0; i < 5; i++)
        {
            clueKeyBindings[i] = KeyCode.Alpha1 + i;
        }
    }

    void Start()
    {
        escMenuUI.SetActive(false);

        const float DEFAULT_SLIDER = 0.5f;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            float masterVal = PlayerPrefs.GetFloat("MasterVol", DEFAULT_SLIDER);
            masterVolumeSlider.SetValueWithoutNotify(masterVal);
            AudioListener.volume = masterVal;
            masterVolumeSlider.onValueChanged.AddListener(v => {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("MasterVol", v);
            });
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            float bgmVal = PlayerPrefs.GetFloat("BGMVol", DEFAULT_SLIDER);
            bgmSlider.SetValueWithoutNotify(bgmVal);
            SoundManager.Instance.SetBGMVolume(bgmVal);
            bgmSlider.onValueChanged.AddListener(v => {
                SoundManager.Instance.SetBGMVolume(v);
                PlayerPrefs.SetFloat("BGMVol", v);
            });
        }

        if (sfxSlider != null)
        {
            float sfxVal = PlayerPrefs.GetFloat("SFXVol", 0.5f);
            sfxSlider.SetValueWithoutNotify(sfxVal);
            SoundManager.Instance.SetSFXVolume(sfxVal);
            sfxSlider.onValueChanged.AddListener(v => {
                SoundManager.Instance.SetSFXVolume(v);
                PlayerPrefs.SetFloat("SFXVol", v);
            });
        }
    }

    // 일반 버튼
    public void GeneralButton()
    {
        general.SetActive(true);
        sound.SetActive(false);
        keybind.SetActive(false);
    }

    // 사운드 버튼
    public void SoundButton()
    {
        gameObject.SetActive(true);
        general.SetActive(false);
        optionMenu.SetActive(true);
        sound.SetActive(true);
        keybind.SetActive(false);
    }

    // 키설정 버튼
    public void KeyBindButton()
    {
        general.SetActive(false);
        optionMenu.SetActive(true);
        sound.SetActive(false);
        keybind.SetActive(true);
    }

    // ESC메뉴 토글(열기/닫기)
    public void ESCMenuToggle()
    {
        if (isPaused)
            Close();
        else
            ESCMenu_Open();
    }

    // ESC메뉴 열기
    public void ESCMenu_Open()
    {
        UIManager.Instance.SetDefaultCursor();
        escMenuUI.SetActive(true);
        general.SetActive(true);
        optionMenu.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }
    // ESC메뉴 닫기
    public void Close()
    {
        escMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;      
    }

    // ESC메뉴가 열려있는지 확인
    public bool IsOpen()
    {
        return UIManager.Instance.ESCMenuUI.gameObject.activeInHierarchy;
    }

    // 타이틀로
    public void GoToMainMenu()
    {
        UIManager.Instance.Inventory_PossessableObjectUI.Clear();
        UIManager.Instance.Inventory_PossessableObjectUI.HideInventory();
        UIManager.Instance.Inventory_PlayerUI.RemoveClueBeforeStage();
        UIManager.Instance.PlayModeUI_CloseAll();
        escMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        PossessionSystem.Instance.CanMove = true; // 임시완
        if (PlayerLifeManager.Instance != null) // 플레이어 생명 불러오기 
        {
            PlayerLifeManager.Instance.ResetLives();
        }
        SceneManager.LoadScene("StartScene");
    }

    // 게임종료
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        Debug.Log($"Master Volume: {value}");
    }

    private void OnLanguageChanged(int index)
    {
        string selectedLang = languageDropdown.options[index].text;
        Debug.Log($"Language changed to: {selectedLang}");

        // TODO: 언어 변경 적용
    }

    private void StartRebind(string actionName)
    {
        Debug.Log($"Rebinding key for: {actionName}");
    }

    private int waitingSlot = -1;

    public void OnClick_RebindClueKey(int slotIndex)
    {
        waitingSlot = slotIndex;
        Debug.Log($"단서 {slotIndex + 1} 키 변경 대기 중...");
    }

    void Update()
    {
        if(IsOpen())
        {
            UIManager.Instance.SetDefaultCursor();
        }
    }

    // private void Update()
    // {
    //     if (waitingSlot != -1)
    //     {
    //         foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
    //         {
    //             if (Input.GetKeyDown(key))
    //             {
    //                 keybindConfig.SetKeyForSlot(waitingSlot, key);
    //                 PlayerPrefs.SetString($"ClueKey{waitingSlot}", key.ToString());
    //                 Debug.Log($"단서 {waitingSlot + 1} 키가 {key}로 설정됨");
    //                 waitingSlot = -1;
    //                 break;
    //             }
    //         }
    //     }
    // }


    // using System;
    // using System.Collections.Generic;
    // using UnityEngine;

    // public class KeyBindingManager : MonoBehaviour, IUIClosable
    // {
    //     public static KeyBindingManager Instance;

    public Dictionary<int, KeyCode> clueKeyBindings = new Dictionary<int, KeyCode>();



    public void SetKey(int clueIndex, KeyCode key)
    {
        clueKeyBindings[clueIndex] = key;
    }

    public KeyCode GetKey(int clueIndex)
    {
        return clueKeyBindings.ContainsKey(clueIndex) ? clueKeyBindings[clueIndex] : KeyCode.None;
    }

    internal string GetDisplayName(KeyCode key)
    {
        throw new NotImplementedException();
    }

    public static class KeyNameHelper
    {
        public static string GetDisplayName(KeyCode key)
        {
            string name = key.ToString();

            // 숫자 키 (Alpha1 ~ Alpha9 → 1 ~ 9)
            if (name.StartsWith("Alpha") && name.Length == 6)
            {
                return name.Substring(5);
            }

            // // Keypad 숫자 (Keypad1 ~ Keypad9 → KP1 ~ KP9)
            // if (name.StartsWith("Keypad") && name.Length == 7)
            // {
            //     return "KP" + name.Substring(6);
            // }

                // // Keypad 숫자 (Keypad1 ~ Keypad9 → KP1 ~ KP9)
            // if (name.StartsWith("F") && name.Length == 2)
            // {
            //     return name;
            // }
            // // 스페이스 등 특수 키 포맷 조정 (예시)
            // if (key == KeyCode.Space)
            // {
            //     return "Space";
            // }

            // 기본 이름 그대로
            return name;
        }
    }

    private void OnMouseEnter()
    {
        UIManager.Instance.SetDefaultCursor();
    }
        
}

