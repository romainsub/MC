namespace MasterServer
{
    class Settings
    {
        public static int       logLevel = 2;
        public static bool      useConsole = true ;
        public static string    serverIP = "127.0.0.1";
        public static int       LGSPort = 18000 ;
        public static int       LPPort = 18001 ;
        public static int       MPort = 18002 ;
        public static int       gameServerPort = 18003 ;
        public static bool      acceptGameServer = true ;
        public static bool      acceptPlayer = true ;
        public static bool      acceptManager = true ;
        public static int       loginTimer = 10*1000;
        public static int       sendTimeout = 1000;
        public static int       maxRankDiff = 3;
    }
}
