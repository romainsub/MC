/*
    From GameServer
        ChangeMatchCount    = 0|1 / 0|-1
        GetPort             = 1
        SetStarted          = 2|0 / 2|1
        MatchResult         = 3|match.id|Player.pos
           
    To GameServer
        GetPort             = 1|Settings.gameServerPort
*/
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace MasterServer
{
    public class GameServer
    {
        public Socket socket ;
        public BackgroundWorker receiver ;
        public string ip ;
        public int id = 0 ;
        public int matchCount = 0 ;
        public bool inDisco = false ;
        public bool serverStarted = false ;

        public GameServer(Socket passedSocket)
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
                #region ChangeMatchCount
                case "0":
                    if (msg.Split('|')[1] == "1") { matchCount += 1; }
                    if (msg.Split('|')[1] == "-1") { matchCount -= 1; }
                    MasterServer.Log(2,"GameServer "+id+" | Changed Match Count | "+ matchCount);
                    break ;
                #endregion
                #region GetPort
                case "1":
                    MasterServer.Log(2,"GameServer "+id+" | Requested GameServer Port | "+ Settings.gameServerPort);
                    SendMsg("1|"+Settings.gameServerPort);
                    break ;
                #endregion
                #region SetStarted
                case "2":
                    if (msg.Split('|')[1] == "0") 
                    { 
                        serverStarted = true ;
                        matchCount = 0 ;
                        MasterServer.Log(0,"GameServer "+id+" | Has been started");

                        //Resend the GetMatch from players in case no GameServer was available when they got put in the queue
                        for (int i=0; i<MasterServer.listQueue.Count; i ++) 
                        { 
                            if (MasterServer.listQueue[i].Alive())
                            {
                                MatchMaker.GetMatch(MasterServer.listQueue[i]);
                            }
                            else
                            {
                                MasterServer.listQueue[i].CleanMe();   
                            }
                        }
                    }
                    if (msg.Split('|')[1] == "1") 
                    { 
                        serverStarted = false ; 
                        matchCount = 0 ;
                        MasterServer.Log(0,"GameServer "+id+" | Has been stopped"); 
                    }
                    break ;
                #endregion
                #region MatchResult
                case "3":
                    MasterServer.Log(2,"Received Match Result | Match "+msg.Split('|')[1]+ " | Winner " +msg.Split('|')[2]) ;
                    break ;
                #endregion
                #region WrongCommand
                default:
                    MasterServer.Log(2,"GameServer | Sent Wrong Command | "+msg.Split('|')[0]+" | "+ ip);
                    SendMsg("999");
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
        public void CleanMe()
        {
            inDisco = true ;
            MasterServer.CleanGameServerConnection(this);   
        }
    }
}
