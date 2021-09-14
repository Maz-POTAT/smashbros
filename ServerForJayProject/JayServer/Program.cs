using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;

namespace JayServer
{
    [Serializable]
    public class Program
    {
        private static string exitString;
        private static Thread ShutdownThread;
        public static bool exitBool;
        public static List<PlayerInfo> PlayersOnline = new List<PlayerInfo>();
        public static List<PlayerInfo> PlayersLookingForGame = new List<PlayerInfo>();
        public static Dictionary<string, TcpClient> ClientTransfers = new Dictionary<string, TcpClient>();
        //public static Dictionary<string, TcpClient> ConnectedClients = new Dictionary<string, TcpClient>();


        static void Main() // program starts form here
        {
            ShutdownThread = new Thread(Shutdown);//creat new thread and check if user has typed stop
            ShutdownThread.Start();
            ConnectToUnity.ClientConnectorTCP();
        }

        static void Shutdown()
        {
            do
            {
                exitString = Console.ReadLine();
                if (exitString == "stop")
                {
                    exitBool = true;
                }

            } while (!exitBool);

            Environment.Exit(0);
            PlayersOnline.Clear();
        }//used to shut down the server
    }

    public class ConnectToUnity
    {
        public static void ClientConnectorTCP()
        {
            try
            {
                /* Initializes Variables */
                TcpListener ServerSocket = new TcpListener(IPAddress.Any, 34000);
                TcpClient ClientSocket = default(TcpClient);
                int ConnectedCount = 0;
                /* start Listener */
                ServerSocket.Start();
                /* start listening for clients */
                Console.WriteLine("The local End point is  :" + ServerSocket.LocalEndpoint);
                Console.WriteLine("Waiting for a connections.....");
                /* runs while loop to keep checking for new clients */
                while (!Program.exitBool)
                {
                    /* used to keep track of how many people have connected sense the server was started */
                    ConnectedCount++;
                    /* code will pause here untell a connection comes through */
                    ClientSocket = ServerSocket.AcceptTcpClient();
                    Console.WriteLine("Client No : " + Convert.ToString(ConnectedCount) + " connected to TCP.");
                    /* sends client info to handler */
                    HandleClientsTCP hndleclient = new HandleClientsTCP();
                    hndleclient.HandleClientComm(ClientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        } /* used to wait for clients to connect, then starts new thread for each client */
    }

    [Serializable]
    public class HandleClientsTCP
    {
        /* initalizes variables */
        TcpClient ClientSocket;
        TcpClient EnemyClient; //do these need to be down in data manager? so the data per thread is independent form each other.
        string type;

        public void HandleClientComm(TcpClient ClntSoc)
        {
            ClientSocket = ClntSoc;
            Thread ClientThread = new Thread(DataManager);
            ClientThread.Start();
        } /* new thread starts here */


        public void DataManager() /* This is new thread */
        {
            /* initalizes variables */
            NetworkStream NtwrkStrm = ClientSocket.GetStream();
            IFormatter MyFormatter = new BinaryFormatter();
            DataBaseHandlerTCP Dbh = new DataBaseHandlerTCP();
            PlayerInfo PlayerClass = new PlayerInfo();
            /* starts loop to check for data being recieved from client */
            while (true)
            {
                try
                {
                    /* client will send data in this order: type of data, info,info,info........ */
                    type = (string)MyFormatter.Deserialize(NtwrkStrm);//Recieve
                    NtwrkStrm.Flush();
                    /* type of data is what i use to know where to send the data on the server side */
                    if (type == "Login")
                    {
                        /* client is attempting to log in, next 7 lines get the data */
                        Console.WriteLine("Type of Data is " + type);
                        string DataUserName = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();
                        string DataPassword = (string)MyFormatter.Deserialize(NtwrkStrm);
                        Console.WriteLine("Checking if user name " + DataUserName + " is already online");
                        NtwrkStrm.Flush();
                        /* gets the responce form methode that checks if a client is already logged into this username and password */
                        string responce = Dbh.CheckIfPlayerIsOnline(DataUserName);

                        if (responce == "Player is not logged in")
                        {
                            /* checks if username and password are in database */
                            responce = Dbh.CheckIfUserExist(DataUserName, DataPassword);
                            Console.WriteLine(responce);
                            if (responce == "Logging in")
                            {
                                /* if user succsefuly loggs on saves the username for later and adds player to currently online players */
                                PlayerClass = Dbh.GetPlayerInfo(DataUserName, ClientSocket);
                                Dbh.AddOnlinePlayer(PlayerClass);
                                /* send responce back to the client if responce is = logging in*/
                                MyFormatter.Serialize(NtwrkStrm, type);
                                NtwrkStrm.Flush();
                                MyFormatter.Serialize(NtwrkStrm, responce);
                                NtwrkStrm.Flush();
                            }
                            else if (responce == "User name does not exist")
                            {
                                type = "AddUser";
                                responce = Dbh.AddUser(DataUserName, DataPassword);
                                MyFormatter.Serialize(NtwrkStrm, type);
                                NtwrkStrm.Flush();
                                MyFormatter.Serialize(NtwrkStrm, responce);
                                NtwrkStrm.Flush();
                            }
                            else
                            {/* send responce back to the client if responce is != logging in , but dont send player info*/
                                MyFormatter.Serialize(NtwrkStrm, type);
                                NtwrkStrm.Flush();
                                MyFormatter.Serialize(NtwrkStrm, responce);
                                NtwrkStrm.Flush();
                            }
                        }
                        else
                        {
                            /* sends responce back to the client saying someone is already logged into that username*/
                            Console.WriteLine(responce);
                            MyFormatter.Serialize(NtwrkStrm, type);
                            NtwrkStrm.Flush();
                            MyFormatter.Serialize(NtwrkStrm, responce);
                            NtwrkStrm.Flush();
                            MyFormatter.Serialize(NtwrkStrm, DataUserName);
                            NtwrkStrm.Flush();
                        }

                    }
                    else if (type == "AddUser") /* the rest of the else if do the same as the first, handle the data. */
                    {
                        Console.WriteLine("Type of Data is " + type);
                        string DataUserName = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();
                        string DataPassword = (string)MyFormatter.Deserialize(NtwrkStrm);
                        Console.WriteLine("Checking if user name " + DataUserName + " is in the database");
                        NtwrkStrm.Flush();
                        string responce = Dbh.AddUser(DataUserName, DataPassword);
                        //send responce back
                        Console.WriteLine(responce);
                        MyFormatter.Serialize(NtwrkStrm, type);
                        NtwrkStrm.Flush();
                        MyFormatter.Serialize(NtwrkStrm, responce);
                        NtwrkStrm.Flush();
                    }
                    else if (type == "FindGame")
                    {
                        Console.WriteLine("Type of Data is " + type);
                        if (Program.PlayersLookingForGame.Count != 0) //if there is already someone looking for match
                        {
                            Console.WriteLine("Player Found!");
                            EnemyClient = Program.PlayersLookingForGame[0].Client; //get the person at position 0 on list (may need to change this latter, i think list adds in order.)
                            string temp = Program.PlayersLookingForGame[0].UserName;
                            Console.WriteLine("temp = " + temp);
                            TcpClient temptcp = new TcpClient();
                            temptcp = ClientSocket;
                            Program.ClientTransfers.Add(temp, ClientSocket);
                            Program.PlayersLookingForGame.Remove(Program.PlayersLookingForGame[0]);
                            NetworkStream NtwrkStrmEnemy = EnemyClient.GetStream();
                            IFormatter MyFormatterEnemy = new BinaryFormatter();
                            MyFormatterEnemy.Serialize(NtwrkStrmEnemy, type);
                            NtwrkStrm.Flush();
                            MyFormatterEnemy.Serialize(NtwrkStrmEnemy, "PlayerFound"); //let enemy know
                            NtwrkStrm.Flush();
                            MyFormatterEnemy.Serialize(NtwrkStrmEnemy, 1); //let enemy know
                            NtwrkStrm.Flush();
                            MyFormatter.Serialize(NtwrkStrm, type);
                            NtwrkStrm.Flush();
                            MyFormatter.Serialize(NtwrkStrm, "PlayerFound"); //let player know
                            NtwrkStrm.Flush();
                            MyFormatter.Serialize(NtwrkStrm, 2); //let player know
                            NtwrkStrm.Flush();
                            
                        }
                        else
                        {
                            Console.WriteLine("Player not found, adding client to waiting list");
                            Program.PlayersLookingForGame.Add(PlayerClass);
                        }
                    }
                    else if (type == "GameData")
                    {
                        NetworkStream NtwrkStrmEnemy = EnemyClient.GetStream();
                        IFormatter MyFormatterEnemy = new BinaryFormatter();

                        Console.WriteLine("Type of Data is " + type);
                        string Key = (string)MyFormatter.Deserialize(NtwrkStrm);
                        NtwrkStrm.Flush();

                        MyFormatterEnemy.Serialize(NtwrkStrmEnemy, type);
                        NtwrkStrm.Flush();
                        MyFormatterEnemy.Serialize(NtwrkStrmEnemy, Key);
                        NtwrkStrm.Flush();
                    }
                    else if (type == "CheckMemory")
                    {
                        foreach (var item in Program.ClientTransfers)
                        {
                            Console.WriteLine("dictionary");
                            if (item.Key == PlayerClass.UserName)
                            {
                                EnemyClient = item.Value;
                                Console.WriteLine("Getting Value");
                            }
                        }
                        if (Program.ClientTransfers.Count != 0)
                        {
                            Program.ClientTransfers.Remove(PlayerClass.UserName);
                        }
                    }
                }
                catch (Exception e) // if client disconnects stops the thread and tells the console
                {
                    Console.WriteLine("Client " + PlayerClass.UserName + " lost");
                    Dbh.RemoveFromOnlinePLayer(PlayerClass);
                    //Console.WriteLine(e);
                    return;
                }
            }
        }
        [Serializable]
        public class DataBaseHandlerTCP
        {
            Dictionary<string, string> UserNamesAndPasswords = new Dictionary<string, string>();
            IFormatter MyFormatter = new BinaryFormatter();

            public string CheckIfUserExist(string Login, string Pass)
            {
                try
                {
                    if (File.Exists("UserNameAndPasswords"))
                    {
                        FileStream stream = File.OpenRead("UserNameAndPasswords");
                        if (stream.Length != 0)
                        {
                            UserNamesAndPasswords = (Dictionary<string, string>)MyFormatter.Deserialize(stream);
                        }
                        stream.Flush();
                        stream.Dispose();
                        if (UserNamesAndPasswords.ContainsKey(Login))
                        {
                            string value;
                            if (UserNamesAndPasswords.TryGetValue(Login, out value))
                            {
                                if (value == Pass)
                                {
                                    return "Logging in";
                                }
                                else
                                {
                                    return "Password incorect";
                                }
                            }
                            else
                            {
                                return "Could not get password";
                            }
                        }
                        else
                        {
                            return "User name does not exist";
                        }
                    }
                    else
                    {
                        FileStream stream = File.OpenWrite("UserNameAndPasswords");
                        MyFormatter.Serialize(stream, UserNamesAndPasswords);
                        stream.Flush();
                        stream.Dispose();
                        return "YAAAYYY!!! You are the first person to ever use the server!! Sadly you must create an account befor you log in.";

                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                    throw;
                }
            } // runs when client attempts to log in

            public string AddUser(string UserName, string Password)
            {
                try
                {
                    if (File.Exists("UserNameAndPasswords"))
                    {
                        FileStream stream = File.OpenRead("UserNameAndPasswords");
                        //checks to make sure file is not empty befor trying to deseralize it
                        if (stream.Length != 0)
                        {
                            UserNamesAndPasswords = (Dictionary<string, string>)MyFormatter.Deserialize(stream);
                            stream.Flush();
                            stream.Dispose();
                            if (!UserNamesAndPasswords.ContainsKey(UserName))
                            {
                                UserNamesAndPasswords.Add(UserName, Password);
                                Console.WriteLine("user name: " + UserName + " and password added");
                                FileStream streem = File.OpenWrite("UserNameAndPasswords");
                                MyFormatter.Serialize(streem, UserNamesAndPasswords);
                                streem.Flush();
                                streem.Dispose();
                                return "Username Created";
                            }
                            else
                            {
                                return "Username is already taken";
                            }
                        }
                        else
                        {
                            UserNamesAndPasswords.Add(UserName, Password);
                            FileStream streem = File.OpenWrite("UserNameAndPasswords");
                            MyFormatter.Serialize(streem, UserNamesAndPasswords);
                            Console.WriteLine("User name and password file did exist, but was empty, added user");
                            streem.Flush();
                            streem.Dispose();
                            return "Username Created";
                        }


                    }
                    else
                    {
                        Console.WriteLine("User name and password file did not exist, one was created");
                        FileStream stream = File.OpenWrite("UserNameAndPasswords");
                        UserNamesAndPasswords.Add(UserName, Password);
                        MyFormatter.Serialize(stream, UserNamesAndPasswords);
                        stream.Flush();
                        stream.Dispose();
                        return "Username Created: " + UserName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            } // runs when client attempts to creat username and password

            public void AddOnlinePlayer(PlayerInfo playerClass)
            {
                try
                {
                    Program.PlayersOnline.Add(playerClass);
                    Console.WriteLine("Added " + playerClass.UserName + " to Players Online");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            } // adds player to dictionary containing all the players currently online

            public void RemoveFromOnlinePLayer(PlayerInfo playerInfosRFOP)
            {
                try
                {
                    Program.PlayersOnline.Remove(playerInfosRFOP);
                    Console.WriteLine("Removed " + playerInfosRFOP.UserName + " from Players Online");
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            } // does what it says

            public string CheckIfPlayerIsOnline(string username)
            {
                try
                {
                    foreach (PlayerInfo PF in Program.PlayersOnline)
                    {
                        if (PF.UserName == username)
                        {
                            return "Player is already Logged in";
                        }
                    }
                    return "Player is not logged in";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "Player is already Logged in";
                }
            } // checks to make sure 2 people are not logged in with the same username

            public PlayerInfo GetPlayerInfo(string name, TcpClient client)
            {
                try
                {
                    if (File.Exists(name))
                    {
                        FileStream stream = File.OpenRead(name);
                        PlayerInfo player = (PlayerInfo)MyFormatter.Deserialize(stream);
                        player.Client = client;
                        stream.Dispose();
                        Console.WriteLine("Player file " + player.UserName + " loaded");
                        return player;
                    }
                    else
                    {
                        Console.WriteLine("File " + name + " does not exsist, it will be created when the player loggs off or is disconected");
                        PlayerInfo player = new PlayerInfo(name);
                        player.Client = client;
                        return player;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            } //loads player info when player logs in
        }
    }

    public class PlayerInfo
    {
        public TcpClient Client { get; set; }
        public string UserName { get; set; }

        public PlayerInfo()
        {

        }

        public PlayerInfo(string name)
        {
            UserName = name;
        }
    }
}
