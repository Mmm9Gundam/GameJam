using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        //跳转到GameScene
        SceneManager.LoadScene("GameScene");
    }
}
