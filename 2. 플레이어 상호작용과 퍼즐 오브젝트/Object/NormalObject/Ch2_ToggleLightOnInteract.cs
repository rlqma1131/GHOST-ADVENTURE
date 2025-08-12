using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ch2_ToggleLightOnInteract : MonoBehaviour
{
    [SerializeField] private Light2D lightToToggle;
    [SerializeField] private GameObject e_Key;

    private bool isPlayerInRange = false;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (lightToToggle != null)
            {
                lightToToggle.enabled = !lightToToggle.enabled;
                UIManager.Instance.PromptUI.ShowPrompt_Random("불이… 켜졌어? 여긴… 어디지?", "이곳이… 미로였던 거야…?");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && other.gameObject == GameManager.Instance.Player)
        {
            e_Key.SetActive(true);
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (GameManager.Instance != null && other.gameObject == GameManager.Instance.Player)
        {
            e_Key.SetActive(false);
            isPlayerInRange = false;
        }
    }
}
