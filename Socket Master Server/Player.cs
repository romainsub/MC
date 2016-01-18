/*
    Command List :
        Login           = email|password
        GetProfile      = 1
        GetMatch        = 2
        CancelMatch     = 3
        RegisterAccount = 5|email|username|password
        SetInMatch      = 6|0 , 6|1
        Exit            = 99
        Wrong Command   = 999
    
    Response List :
        LoggedIn        = 0|0
        LoginRefused    = 0|1
        LogginDontExist = 0|2
        LogginDBError   = 0|3
        SendProfile     = 1|username|rank|ladder
        SendInQueue     = 2
        MatchCancelled  = 3
        SendMatchInfo   = 4|playerPos|match.id|match.gsIp|match.player1.username|match.player1.rank|match.player1.ladder|match.levelName
        RegOk           = 5|0
        RegUsernameExist = 5|1
        SetMatchOk      = 6 

        InvalidRequest  = -1
        DblLogin        = -2
        Alive           = 88
*/

using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace MasterServer
{
    public class Player
    {
        //Net
        public Socket           socket;
        public BackgroundWorker receiver;
        public string           ip;
        public bool             inDisco = false ;
        public bool             loggedIn = false ;

        //Profile
        public int              id = -1 ;
        public string           username = "" ; 
        public string           email = "" ;
        public int              rank = 0 ;
        public int              ladder = 0 ;
        public bool             inMatch = false ;

        public Player(Socket passedSocket)
        {
            socket = passedSocket;
            socket.NoDelay = true;
            socket.SendTimeout = Settings.sendTimeout;
            ip = ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString();
            receiver = new BackgroundWorker();
            receiver.DoWork += new DoWorkEventHandler(Receive);
            receiver.RunWorkerAsync();
        }
        public void Receive(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (socket.Connected)
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
                        Commands(Encoding.ASCII.GetString(formatted));
                    }
                    else
                    {
                        if (!inDisco) { CleanMe() ;}
                        break ;    
                    }
                    
                }
                catch (SocketException)
                {
                    CleanMe();
                    break ;
                }
            }
        }
        public void Commands(string msg)
        {
            if (!loggedIn && msg.Split('|')[0] != "5") { Loggin(msg);  }
            else
            {
                string[] arr = msg.Split('|');
                switch (arr[0])
                {
                    #region GetProfile
                    case "1":
                        MasterServer.Log(2,"Player "+id+" | Received Get Profile");
                        SQL.GetPlayerProfile(this);
                        SendMsg("1|"+username+"|"+rank+"|"+ladder);
                        break;
                    #endregion
                    #region GetMatch
                    case "2":
                        MatchMaker.GetMatch(this);
                        break;
                    #endregion
                    #region Register
                    case "5" :
                        Register(msg);
                        break ;
                    #endregion
                    #region CancelMatch
                    case "3":
                        MasterServer.Log(2,"Player "+id+" | Received Cancel Match");
                        MatchMaker.CancelMatch(this);
                        SendMsg("3");
                        break;
                    #endregion
                    #region SetInMatch
                    case "6":
                        if (arr.Length == 2)
                        {
                            if (arr[1] == "0") { inMatch = false ; }
                            if (arr[1] == "1") { inMatch = true ; }
                            MasterServer.Log(2,"Player "+id+" | inMatch : " + inMatch);
                            SendMsg("6");
                        }
                        else
                        {
                            SendMsg("999");
                        }
                        
                        break ;
                    #endregion
                    #region Exit
                    case "99":
                        MasterServer.Log(2,"Player "+id+"| Received Exit Signal");
                        if (!inDisco)
                        {
                            CleanMe();
                        }
                        break ;
                    #endregion
                    #region WrongCommand
                    default:
                        MasterServer.Log(2,"Player "+id+"| Sent Wrong Command | "+msg.Split('|')[0]+" | "+ ip);
                        SendMsg("999");
                        break;
                    #endregion
                }
            }
        }

        public void Loggin(string msg)
        {
            string[] arr = msg.Split('|');
            if (arr.Length != 2)
            {
                SendMsg("-1") ;
                MasterServer.Log(0,"Player | Invalid Login Request | "+msg+" | "+ ip);
                CleanMe();
                return;
            }
            else
            {
                // -1 : wrong pass / -2 : email dont exist / -3 : Db Error / x : PlayerID
                int newId = SQL.GetPlayerID(msg.Split('|')[0],msg.Split('|')[1]) ;
                switch (newId)
                {
                    case -1 :   
                        SendMsg("0|1");
                        MasterServer.Log(2,"Player | Wrong Password | "+ ip);
                        break ;
                    case -2 :   
                        SendMsg("0|2");
                        break ;
                    case -3 :   
                        SendMsg("0|3");
                        MasterServer.Log(2,"Player | Loggin Database Error | "+ ip);
                        break ;
                    default :
                        //Check if user is not already logged in
                        int index = -1;
                        index = MasterServer.listPlayer.FindIndex(x => x.id.Equals(newId));
                        if (index > -1) 
                        {
                            //Check if it is not a connection lost
                            if (MasterServer.listPlayer[index].Alive())
                            {
                                MasterServer.Log(0,"Double Login Detected : Player with ID "+newId+" | "+ ip);
                                SendMsg("-2");
                                CleanMe();    
                            }
                            else
                            {
                                MasterServer.listPlayer[index].CleanMe();
                            }       
                        }
                        else
                        {
                            id = newId ;
                            email = msg.Split('|')[0] ;
                            MasterServer.Log(2,"Player "+id+" | Logged in") ;
                            SendMsg("0|0");
                            loggedIn = true ; 
                        }
                        break ;
                }
            }
        }
        public void Register(string msg)
        {
            //RegisterAccount = 5|email|username|password
            string[] arr = msg.Split('|');
            if (arr.Length != 4)
            {
                SendMsg("-1") ;
                MasterServer.Log(0,"Player | Invalid Register Request | "+msg+" | "+ ip);
                CleanMe();
                return;
            }
            else
            {
                if (SQL.Register(arr[1],arr[2],arr[3]))
                {
                    MasterServer.Log(2,"Player "+arr[1]+" | Registered") ;
                    SendMsg("5|0");
                }
                else
                {
                    SendMsg("5|1");
                }
            }
        }
        public void SendMatchInfo(Match match, int playerPos)
        {
            if (playerPos == 1)
            {
                SendMsg("4|"+playerPos+"|"+match.id+"|"+match.gsIp+"|"+match.player2.username+"|"+match.player2.rank+"|"+match.player2.ladder+"|"+match.levelName);
            }
            if (playerPos == 2)
            {
                SendMsg("4|"+playerPos+"|"+match.id+"|"+match.gsIp+"|"+match.player1.username+"|"+match.player1.rank+"|"+match.player1.ladder+"|"+match.levelName);
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
                CleanMe();
            }
        }
        public bool Alive()
        {
            try 
            {
                byte[] msg = Encoding.ASCII.GetBytes ("88");
                socket.Send(msg);
                return true ;
            }
            catch (SocketException)
            {
                return false ;
            }
        }
        public void CleanMe()
        {
            if (!inDisco)
            {
                inDisco = true ;
                MasterServer.CleanPlayerConnection(this);
            } 
        }
    }
}
