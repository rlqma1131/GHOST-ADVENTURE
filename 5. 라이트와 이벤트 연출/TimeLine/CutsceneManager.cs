using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutsceneManager : Singleton<CutsceneManager>
{

    public PlayableDirector director;
    public GameObject fadePanel;


    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public IEnumerator PlayCutscene()
    {

        Debug.Log("컷신 재생 시작");
        bool isDone = false;

        void OnPlayableDirectorStopped(PlayableDirector obj)
        {
            if (obj == director)
            {
                Debug.Log("컷신 재생 완료");
                isDone = true;
                director.stopped -= OnPlayableDirectorStopped;
            }
        }

        director.stopped += OnPlayableDirectorStopped;

        director.Play();

        yield return new WaitUntil(() => isDone);

        fadePanel.SetActive(false);
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"{scene.name} 씬 언로드됨");
        if (fadePanel != null)
            fadePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
