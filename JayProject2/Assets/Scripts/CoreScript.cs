using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CoreScript : MonoBehaviour
{

    public static CoreScript instance = null;

    public TcpClient TcpClnt = new TcpClient();
    public InputField UserNameInput;
    public InputField PasswordInput;
    public Text FeedbackText;
    public GameObject LoginPanel;
    public GameObject MainMenuPanel;
    public string username;
    public int PlayerNumber;
    public PlaySceneScript PSS;
    public GameObject CharectorPreFab;
    public GameObject PlayerChar;
    public GameObject Enemy1Char;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MainMenuPanel.SetActive(false);
        LoginPanel.SetActive(true);
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        ServerResponceHandler();
    }

    public void Connect()
    {
        try
        {
            Debug.Log("Connecting.....");
            TcpClnt.Connect("50.83.15.160", 34000); // uses ipaddress for the server program
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to server.");
            FeedbackText.text = "Could not connect to server.";
            Debug.Log(e);
        }
        if (TcpClnt.Connected == true) // checks if connection was sucsessfull and runs connected script in main menu if it was.
        {
            Debug.Log("Connected to server.");
            FeedbackText.text = "Connected to server.";
        }
    }

    public void SendLoginData()
    {

        try
        {
            if (UserNameInput.text == "" || PasswordInput.text == "")
            {
                FeedbackText.text = "User name or password is blank.";
            }
            else
            {
                NetworkStream NtwrkStrm = TcpClnt.GetStream();
                IFormatter MyFormatter = new BinaryFormatter();
                username = UserNameInput.text;
                //Debug.Log("Login Send Started");
                FeedbackText.text = "Login Send Started";
                string type = "Login";
                MyFormatter.Serialize(NtwrkStrm, type);
                NtwrkStrm.Flush();
                MyFormatter.Serialize(NtwrkStrm, UserNameInput.text);
                NtwrkStrm.Flush();
                MyFormatter.Serialize(NtwrkStrm, PasswordInput.text);
                NtwrkStrm.Flush();
            }
        }
        catch (Exception e)
        {
            //Debug.Log(e);
        }
    } //1

    public void FindGame()
    {
        NetworkStream NtwrkStrm = TcpClnt.GetStream();
        IFormatter MyFormatter = new BinaryFormatter();

        FeedbackText.text = "Looking for game";
        string type = "FindGame";
        MyFormatter.Serialize(NtwrkStrm, type);
        NtwrkStrm.Flush();
        MyFormatter.Serialize(NtwrkStrm, username);
        NtwrkStrm.Flush();
    }

    public void PlaySceneLoaded()
    {
        NetworkStream NtwrkStrm = TcpClnt.GetStream();
        IFormatter MyFormatter = new BinaryFormatter();

        string type = "CheckMemory";
        MyFormatter.Serialize(NtwrkStrm, type);
        NtwrkStrm.Flush();
        PSS = GameObject.Find("Main Camera").GetComponent<PlaySceneScript>();
        if (PlayerNumber == 1) //player number is used to corrisponde the gameobject in between clients
        {
            PlayerChar = Instantiate(CharectorPreFab, PSS.Player1SpawnPoint.transform);
            PlayerChar.GetComponent<character>().MainPlayer = true;
            Enemy1Char = Instantiate(CharectorPreFab, PSS.Player2SpawnPoint.transform);
            Enemy1Char.GetComponent<character>().MainPlayer = false;
        }
        else if (PlayerNumber ==2)//using the spawn points as orgins so that the position of the player on all clients should be (close) to the same.
        {
            PlayerChar = Instantiate(CharectorPreFab, PSS.Player2SpawnPoint.transform);
            PlayerChar.GetComponent<character>().MainPlayer = true;
            Enemy1Char = Instantiate(CharectorPreFab, PSS.Player1SpawnPoint.transform);
            Enemy1Char.GetComponent<character>().MainPlayer = false;
        }
    }

    public void SendInputData(KeyCode key)
    {
        NetworkStream NtwrkStrm = TcpClnt.GetStream();
        IFormatter MyFormatter = new BinaryFormatter();

        string type = "GameData";
        MyFormatter.Serialize(NtwrkStrm, type);
        NtwrkStrm.Flush();
        MyFormatter.Serialize(NtwrkStrm, key.ToString()); ;
        NtwrkStrm.Flush();
    }

    public void ServerResponceHandler()
    {
        if (TcpClnt.Connected)
        {
            NetworkStream NtwrkStrm = TcpClnt.GetStream();
            IFormatter MyFormatter = new BinaryFormatter();
            if (NtwrkStrm.DataAvailable)
            {
                try
                {
                    string type = (string)MyFormatter.Deserialize(NtwrkStrm);
                    NtwrkStrm.Flush();
                    if (type == "Login")
                    {
                        //Debug.Log("Login Info Recived");
                        string response = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();
                        FeedbackText.text = response;
                        if (response == "Logging in")
                        {
                            NtwrkStrm.Flush();
                            Debug.Log("PlayerClass.Name = " + response);
                            MainMenuPanel.SetActive(true);
                            LoginPanel.SetActive(false);
                        }
                        else
                        {
                            FeedbackText.text = response;
                            username = null;
                        }
                        return;
                    }//2
                    else if (type == "AddUser")
                    {
                        string response = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();
                        FeedbackText.text = response;
                        return;
                    }
                    else if (type == "FindGame")
                    {
                        string response = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();
                        if (response == "PlayerFound")
                        {
                            int pnum = (int)MyFormatter.Deserialize(NtwrkStrm);
                            NtwrkStrm.Flush();
                            PlayerNumber = pnum;
                            SceneManager.LoadScene("PlayScene");
                        }
                    }
                    else if (type == "GameData")
                    {
                        string response = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();
                        Enemy1Char.GetComponent<character>().MovementPlayer2(response);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    //Debug.Log(e);
                    NtwrkStrm.Flush();
                    return;
                }
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }
    }
}

public class PlayerInfo
{
    public IPAddress Address { get; set; }
    public string UserName { get; set; }

    public PlayerInfo()
    {

    }

    public PlayerInfo(string name)
    {
        UserName = name;
    }
}
