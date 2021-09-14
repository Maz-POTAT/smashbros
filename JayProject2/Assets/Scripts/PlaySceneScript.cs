using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySceneScript : MonoBehaviour
{
    CoreScript CS;
    public GameObject Player1SpawnPoint;
    public GameObject Player2SpawnPoint;

    void Start()
    {
        CS = GameObject.Find("GameController").GetComponent<CoreScript>();
        CS.PlaySceneLoaded();
    }
}
