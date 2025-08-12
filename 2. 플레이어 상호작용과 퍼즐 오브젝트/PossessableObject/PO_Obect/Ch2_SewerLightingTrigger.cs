using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ch2_SewerLightingTrigger : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;

    private Color originalGlobalColor;
    private bool hasStoredOriginal = false;
    private Light2D playerLight;
    private GameObject player;

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            // Light2D 자동 탐색
            if (playerLight == null)
            {
                playerLight = other.GetComponentInChildren<Light2D>(includeInactive: true);
            }

            // Global Light 어둡게
            if (globalLight != null)
            {
                if (!hasStoredOriginal)
                {
                    originalGlobalColor = globalLight.color;
                    hasStoredOriginal = true;
                }

                globalLight.color = Color.black;
                globalLight.intensity = 1f;
            }

            // Player Light 켜기
            if (playerLight != null)
            {
                playerLight.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player && !other.gameObject.activeInHierarchy)
            return;
        
        if (other.gameObject == player)
        {
            if (globalLight != null && hasStoredOriginal)
            {
                globalLight.color = originalGlobalColor;
            }
            
            if (playerLight != null)
            {
                playerLight.enabled = false;
            }
        }
    }
    
    public void ForceRestoreLighting()
    {
        if (globalLight != null && hasStoredOriginal)
        {
            globalLight.color = originalGlobalColor;
        }

        if (playerLight == null)
        {
            playerLight = player?.GetComponentInChildren<Light2D>(includeInactive: true);
        }

        if (playerLight != null)
        {
            playerLight.enabled = false;
        }
    }
}
