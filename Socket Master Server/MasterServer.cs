using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MasterServer
{
    class MasterServer
    {
        public static Socket listenerSocketGameServer ;
        public static Socket listenerSocketPlayers ;
        public static Socket listenerSocketManager ;
        public static List<Manager> listManager = new List<Manager>();
        public static List<GameServer> listGameServer = new List<GameServer>();
        public static List<Player> listPlayer = new List<Player>();
        public static List<Match> listMatch = new List<Match>();
        public static List<Player> listQueue = new List<Player>();
        public static List<string> listLevel = new List<string>();

        static void Main(string[] args)
        {
            //Get IP V4
            Settings.serverIP = GetIP4Address();
            Log(0,"Starting up XLink Master Server on "+Settings.serverIP);

            //Set Sockets and EndPoints
            listenerSocketGameServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocketPlayers = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocketManager = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint epGS = new IPEndPoint(IPAddress.Parse(Settings.serverIP), Settings.LGSPort);
            IPEndPoint epPL = new IPEndPoint(IPAddress.Parse(Settings.serverIP), Settings.LPPort);
            IPEndPoint epMA = new IPEndPoint(IPAddress.Parse(Settings.serverIP), Settings.MPort);
            listenerSocketGameServer.Bind(epGS);
            listenerSocketPlayers.Bind(epPL);
            listenerSocketManager.Bind(epMA);

            //Start the listener threads
            Thread gsListen = new Thread(ListenerGameServer);
            Thread playerListen = new Thread(ListenerPlayer);
            Thread managerListen = new Thread(ListenerManager);
            gsListen.Start();
            playerListen.Start();
            managerListen.Start();

            //Open Connection to DB
            SQL.SQLConnect();

            //Get Level List
            SQL.GetLevelList();

            //Wait Infinitly
            var mre = new ManualResetEvent(false);
            mre.WaitOne(); 
        }

        public static void ListenerGameServer()
        {
            Log(0,"Listener GameServer Started");

            while (true)
            {
                listenerSocketGameServer.Listen(10);
                IncomingConnection (listenerSocketGameServer.Accept(),1); 
            }
        }
        public static void ListenerPlayer()
        {
            Log(0,"Listener Player Started");

            while (true)
            {
                listenerSocketPlayers.Listen(10);
                IncomingConnection (listenerSocketPlayers.Accept(),2); 
            }
        }
        public static void ListenerManager()
        {
            Log(0,"Listener Manager Started");

            while (true)
            {
                listenerSocketManager.Listen(10);
                IncomingConnection (listenerSocketManager.Accept(),3); 
            }
        }
        public static void IncomingConnection(Socket socket, int type)
        {
            switch (type)
            {
                #region GameServer
                case 1:
                    if (Settings.acceptGameServer) 
                    {
                        Log(1,"Game Server Connection Accepted | "+ ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString()) ;
                        GameServer server = new GameServer(socket);
                        listGameServer.Add(server);
                        server.id = listGameServer.Count - 1;
                    }
                    else
                    {
                        Log(1,"Game Server Connection Refused | "+ ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString()) ;
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    break;
                #endregion
                #region Player
                case 2:
                    if (Settings.acceptPlayer) 
                    {
                        Log(1,"Player Connection Accepted | "+ ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString()) ;
                        Player player = new Player(socket);
                        listPlayer.Add(player);
                    }
                    else
                    {
                        Log(1,"Player Connection Refused | "+ ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString()) ;
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    break;
                #endregion
                #region Manager
                case 3:
                    if (Settings.acceptManager) 
                    {
                        Log(1,"Manager Connection Accepted | "+ ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString()) ;
                        Manager manager = new Manager(socket);
                        listManager.Add(manager);
                    }
                    else
                    {
                        Log(1,"Manager Connection Refused | "+ ((IPEndPoint)(socket.RemoteEndPoint)).Address.ToString()) ;
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    break ;
                #endregion
            }
        }

        public static void CleanManagerConnection(Manager manager)
        {
            Log(2,"Manager Connection Cleaned | "+ manager.ip);
            listManager.Remove(manager);
            manager.socket.Shutdown(SocketShutdown.Both);
            manager.socket.Close();
            manager.receiver.Dispose();
            manager = null ;
        }
        public static void CleanGameServerConnection(GameServer server)
        {
            Log(2,"GameServer Connection Cleaned | "+ server.ip);
            listGameServer.Remove(server);
            server.socket.Shutdown(SocketShutdown.Both);
            server.socket.Close();
            server.receiver.Dispose();
            server = null ;
        }
        public static void CleanPlayerConnection(Player player)
        {
            
            if (player.id >= 0) 
            {
                Log(2,"Player "+player.id+" | Disconnected");   
                MatchMaker.CancelMatch(player); 
            }
            else
            {
                Log(2,"Player Disconnected | "+ player.ip);     
            }

            listPlayer.Remove(player);
            player.socket.Shutdown(SocketShutdown.Both);
            player.socket.Close();
            player.receiver.Dispose();
            player = null ;
        }

        public static void Log(int imp, string msg)
        {
            if (Settings.logLevel >= imp)
            {
                if (Settings.useConsole) { Console.WriteLine(DateTime.Now + " --> " +msg) ;}    
            }
        }
        public static string GetIP4Address()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach(IPAddress i in ips)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                return i.ToString();
            }
            return "127.0.0.1";
        }
    }
}
