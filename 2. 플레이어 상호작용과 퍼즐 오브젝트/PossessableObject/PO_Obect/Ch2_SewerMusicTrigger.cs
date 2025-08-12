using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ch2_SewerMusicTrigger : MonoBehaviour
{
    // [SerializeField] private Light2D globalLight;

    private Ch2_SewerMusicPuzzle musicPuzzle;
    // private Color originalGlobalLightColor;
    // private bool hasStoredOriginal = false;
    // private Light2D playerLight;
    private GameObject player;

    private void Awake()
    {
        musicPuzzle = FindObjectOfType<Ch2_SewerMusicPuzzle>();
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            // if (playerLight == null)
            // {
            //     playerLight = other.GetComponentInChildren<Light2D>(includeInactive: true);
            // }
            //
            // if (globalLight != null)
            // {
            //     if (!hasStoredOriginal)
            //     {
            //         originalGlobalLightColor = globalLight.color;
            //         hasStoredOriginal = true;
            //     }
            //
            //     globalLight.color = Color.black;
            //     globalLight.intensity = 1f;
            // }
            //
            // if (playerLight != null)
            // {
            //     playerLight.enabled = true;
            // }

            musicPuzzle?.StartPuzzle();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            // // Global Light 복원
            // if (globalLight != null && hasStoredOriginal)
            // {
            //     globalLight.color = originalGlobalLightColor;
            // }
            //
            // // Player Light 끄기
            // if (playerLight != null)
            // {
            //     playerLight.enabled = false;
            // }

            musicPuzzle?.StopPuzzle();
        }
    }
}
