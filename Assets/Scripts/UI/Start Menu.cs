using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public void StartGame()
    {
        //跳转到GameScene
        SceneManager.LoadScene("GameScene")
    }
}
