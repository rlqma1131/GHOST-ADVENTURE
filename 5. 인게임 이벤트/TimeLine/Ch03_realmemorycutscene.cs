using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Ch03_realmemorycutscene : MonoBehaviour
{
    [SerializeField] private Ch3_MemoryPuzzleUI ch3_MemoryPuzzleUI;
    private bool isCutsceneActive = false;
    [SerializeField] private PlayableDirector cutsceneDirector;

    private void Start()
    {
        cutsceneDirector.stopped += OnCutsceneStopped;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCutsceneActive && ch3_MemoryPuzzleUI.puzzlecompleted)
        {
            isCutsceneActive = true;
            PossessionSystem.Instance.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
            cutsceneDirector.Play();
            EnemyAI.PauseAllEnemies();
        }
    }

    void OnCutsceneStopped(PlayableDirector d)
    {
        EnemyAI.ResumeAllEnemies();
        PossessionSystem.Instance.CanMove = true;
        UIManager.Instance.PlayModeUI_OpenAll();
        isCutsceneActive = true;
    }
}
