using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour, IUIClosable
{
    private bool isOpen;

    public void Close()
    {
        Time.timeScale = 1f;
        isOpen = false;    
        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        UIManager.Instance.PlayModeUI_CloseAll();
    }


    void OnDisable()
    {
        EnemyAI.ResumeAllEnemies();
        // UIManager.Instance.SetDefaultCursor();
        UIManager.Instance.PlayModeUI_OpenAll();
    }

    public void GuideToggle()
    {
        if (isOpen)
            Close();
        else
            GuideOpen();
    }

    public void GuideOpen()
    {
        UIManager.Instance.SetDefaultCursor();
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
    }

    public bool IsOpen()
    {
        return gameObject.activeInHierarchy;
    }

    void Update()
    {
        if(IsOpen())
        {
            // UIManager.Instance.SwipeCursor();
            EnemyAI.PauseAllEnemies();
        }
    }
}
