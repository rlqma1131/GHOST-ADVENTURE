using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void GoScene(string Scenename)
    {

        SceneManager.LoadScene(Scenename);

    }
}
