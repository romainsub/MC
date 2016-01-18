using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MasterServer
{
    public class Manager
    {
        public Socket socket;
        public BackgroundWorker receiver;
        public string ip;
        public string username = "romain";
        public string password = "xlManager2016";
        public bool loggedin = false ;
        public bool inDisco = false ;
        System.Timers.Timer loginTimer = null;

        public Manager(Socket passedSocket)
        {
            socket = passedSocket;
            socket.NoDelay = true;
            socket.SendTimeout = Settings.sendTimeout;
            ip = ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString();
            receiver = new BackgroundWorker();
            receiver.DoWork += new DoWorkEventHandler(Receive);
            receiver.RunWorkerAsync();

            //Login Timer
            loginTimer = new System.Timers.Timer();
            loginTimer.Interval = Settings.loginTimer;
            loginTimer.Elapsed += LoginTimer;
            loginTimer.AutoReset = false;
            loginTimer.Enabled = true;
        }
        public void Receive(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    byte [] buffer = new byte[socket.SendBufferSize] ;
                    int bytesRead = socket.Receive(buffer);
                    if (bytesRead <= 0)
                    {
                        throw new SocketException();
                    }
                    byte [] formatted = new byte[bytesRead];
                    for (int i = 0; i <bytesRead; i++)
                    {
                        formatted[i] = buffer[i];
                    }
                    
                    if (!loggedin) { Loggin(Encoding.ASCII.GetString(formatted)); }
                    else { Commands(Encoding.ASCII.GetString(formatted)); }
                }
                catch (SocketException)
                {
                    if (!inDisco)
                    {
                        CleanMe();
                    }
                    break ;
                }
            }
        }
        public void Commands(string msg)
        {
            switch (msg.Split('|')[0])
            {
                #region AddLevel
                case "AddLevel":
                    MasterServer.Log(0,"Manager Added Level | "+ msg.Split('|')[1]) ;
                    MasterServer.listLevel.Add(msg.Split('|')[1]);
                    SendMsg("Level Added To List : " +msg.Split('|')[1]);
                    break ;
                #endregion
                #region RemoveLevel
                case "RemoveLevel":
                    MasterServer.Log(0,"Manager Removed Level | "+ msg.Split('|')[1]) ;
                    MasterServer.listLevel.Remove(msg.Split('|')[1]);
                    SendMsg("Level Remove From List : " +msg.Split('|')[1]);
                    break ;
                #endregion
                #region List
                case "List":
                    string[] arr = msg.Split('|');
                    if (arr.Length != 2)
                    {
                        SendMsg("Wrong List Request") ;
                        return;
                    }
                    switch (msg.Split('|')[1])
                    {
                        case "Manager":
                            SendMsg("Manager Count : " + MasterServer.listManager.Count);
                            break ;
                        case "GameServer":
                            SendMsg("GameServer Count : " + MasterServer.listGameServer.Count);
                            break ;
                        case "Player":
                            SendMsg("Player Count : " + MasterServer.listPlayer.Count);
                            break ;
                        case "Match":
                            SendMsg("Match Count : " + MasterServer.listMatch.Count);
                            break ;
                        case "Queue":
                            SendMsg("Queue Count : " + MasterServer.listQueue.Count);
                            break ;
                        case "Level":
                            if (MasterServer.listLevel.Count > 0)
                            {
                                for (int i = 0 ; i < MasterServer.listLevel.Count; i++)
                                {
                                    SendMsg("Level "+i+" : " +MasterServer.listLevel[i]);
                                }
                            }
                            else
                            {
                                SendMsg("No Level in List");   
                            }
                            
                            break ;                        
                        default:
                            SendMsg("Wrong List Request");
                            break ;
                    }
                    break;
                #endregion
                #region SetLogLevel
                case "SetLogLevel":
                    arr = msg.Split('|');
                    if (arr.Length != 2)
                    {
                        SendMsg("Wrong SetLogLevel Request") ;
                        return;
                    }
                    switch (msg.Split('|')[1])
                    {
                        case "0":
                            Settings.logLevel = 0;
                            break;
                        case "1":
                            Settings.logLevel = 1;
                            break;
                        case "2":
                            Settings.logLevel = 2;
                            break;
                    }
                    MasterServer.Log(0,"Server Setting | SetLogLevel : " +Settings.logLevel);
                    break;
                #endregion
                #region StartListenGS
                case "StartListenGS":
                    Settings.acceptGameServer = true ;
                    MasterServer.Log(0,"Server Setting | Listener GameServer : " +Settings.acceptGameServer);
                    SendMsg("Started Accepting new GameServers");
                    break ;
                #endregion
                #region StopListenGS
                case "StopListenGS":
                    Settings.acceptGameServer = false ;
                    MasterServer.Log(0,"Server Setting | Listener GameServer : " +Settings.acceptGameServer);
                    SendMsg("Stopped Accepting new GameServers");
                    break ;
                #endregion
                #region StartListenPL
                case "StartListenPL":
                    Settings.acceptPlayer = true ;
                    MasterServer.Log(0,"Server Setting | Listener Player : " +Settings.acceptPlayer);
                    SendMsg("Started Accepting new Players");
                    break ;
                #endregion
                #region StopListenPL
                case "StopListenPL":
                    Settings.acceptPlayer = false ;
                    MasterServer.Log(0,"Server Setting | Listener Player : " +Settings.acceptPlayer);
                    SendMsg("Stopped Accepting new Players");
                    break ;
                #endregion
                #region StartListenMA
                case "StartListenMA":
                    Settings.acceptManager = true ;
                    MasterServer.Log(0,"Server Setting | Listener Manager : " +Settings.acceptManager);
                    SendMsg("Started Accepting new Managers");
                    break ;
                #endregion
                #region StopListenMA
                case "StopListenMA":
                    Settings.acceptManager = false ;
                    MasterServer.Log(0,"Server Setting | Listener Manager : " +Settings.acceptManager);
                    SendMsg("Stopped Accepting new Managers");
                    break ;
                #endregion
                #region Exit
                case "exit":
                    MasterServer.Log(1,"Manager | Received Exit Signal | "+ ip);
                    CleanMe();
                    break ;
                #endregion
                #region WrongCommand
                default:
                    MasterServer.Log(1,"Manager | Sent Wrong Command | "+msg.Split('|')[0]+" | "+ ip);
                    SendMsg("Wrong Command");
                    break;
                #endregion
            }
        }
        public void SendMsg(string msg)
        {
            try 
            {
                byte[] response = Encoding.ASCII.GetBytes (msg);
                socket.Send(response);   
            }
            catch (SocketException)
            {
                if (!inDisco)
                {
                    CleanMe();
                }
            }
        }
        public void Loggin(string msg)
        {
            string[] arr = msg.Split('|');
            if (arr.Length < 2)
            {
                SendMsg("Refused") ;
                MasterServer.Log(0,"Manager LoggedIn Refused | Reason : Wrong String Sent"+ ip) ;
                CleanMe();
                return;
            }
            if (msg.Split('|')[0] == username && msg.Split('|')[1] == password) 
            { 
                SendMsg("Logged In") ; 
                loggedin = true ;
                loginTimer.Enabled = false;
                MasterServer.Log(0,"Manager Logged In | "+ ip) ;
            }
            else 
            { 
                SendMsg("Refused") ;
                MasterServer.Log(0,"Manager LoggedIn Refused | Reason : Wrong Username/Password | "+ ip) ;
                CleanMe();
            }
        }
        public void LoginTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (!loggedin)
            {
                MasterServer.Log(1,"Manager Disconnected | Reason : Loggin Timeout | "+ ip);   
                CleanMe();       
            }
        }
        public void CleanMe()
        {
            inDisco = true ;
            loginTimer.Enabled = false;
            MasterServer.CleanManagerConnection(this);   
        }
    }
}
