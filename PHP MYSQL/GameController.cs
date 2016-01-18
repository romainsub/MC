//------------------------------------------------------------------------------------------------------------------------------//
//													    TITLE                   							                    //
//------------------------------------------------------------------------------------------------------------------------------//
/*

*/
//------------------------------------------------------------------------------------------------------------------------------//
//														INIT																	//
//------------------------------------------------------------------------------------------------------------------------------//
using UnityEngine ;
using System.Collections ;
using System.Text.RegularExpressions;
using UnityEngine.Networking ;
using UnityEngine.Networking.NetworkSystem ;
using Newtonsoft.Json ;
using Cross.Net ;
using Mono.Nat;

public class GameController : MonoBehaviour
{
//------------------------------------------------------------------------------------------------------------------------------//
//														VARIABLES																//
//------------------------------------------------------------------------------------------------------------------------------//
    private GameHolder                              gameHolder                                  = null                          ;
    
    //NETWORK
    private bool                                    matchCreated                                = false                         ;
    public  int                                     maxPlayers                                  = 2                             ;
    public  NetworkClient                           client                                                                      ;
    public  int                                     playerCount                                 = 0                             ;
    public  int                                     playerReady                                 = 0                             ;
    public  static string                           levelName                                   = string.Empty                  ;
    public  AsyncOperation                          levelchangeOp                               = new AsyncOperation()          ;
    public  int                                     gotOpponent                                 = 0                             ;
    public  bool                                    useDebugPanel                               = false                         ;                      

    public  bool                                    useMonoNat                                  = false                         ;
    public  bool                                    useCrossNet                                 = false                         ;

    public  bool                                    upnpDone                                    = false                         ;
    public  bool                                    upnpStatus                                  = false                         ;

    //PLAYERS
    public  GameObject                              playerT01                                   = null                          ;
    public  GameObject                              playerT02                                   = null                          ;
    public  NetPlayer                               playerT01Cpn                                = null                          ;
    public  NetPlayer                               playerT02Cpn                                = null                          ;

    //LEVEL
    public  LevelController                         levelController                             = null                          ;
    public  LevelHolder                             levelHolder                                 = null                          ;
    public  int                                     victory                                     = 0                             ;
//------------------------------------------------------------------------------------------------------------------------------//
//													    AWAKE           														//
//------------------------------------------------------------------------------------------------------------------------------//
    void Awake ()
    {
        gameHolder = GetComponent<GameHolder>() ; 
        Application.runInBackground = true ;
    }
//------------------------------------------------------------------------------------------------------------------------------//
//													    START           														//
//------------------------------------------------------------------------------------------------------------------------------//
    void Start ()
    {
        if (useDebugPanel) { gameHolder.debugGo.SetActive(true) ; } 
        if (useMonoNat) 
        {
            NatUtility.DeviceFound += DeviceFound ;
            NatUtility.DeviceLost += DeviceLost ;
            NatUtility.StartDiscovery () ;
        }
        if (useCrossNet)
        {
            StartCoroutine ("InitNetwork") ;    
        } 
    }
//------------------------------------------------------------------------------------------------------------------------------//
//													VOID USER DEFINED															//
//------------------------------------------------------------------------------------------------------------------------------//
    //UI
    public void ButtonLogin ()
    {
        gameHolder.playerEmail = gameHolder.inputEmail.text ;
        gameHolder.playerPass =  gameHolder.inputPassword.text ;
        if (!IsValidEmailAddress (gameHolder.playerEmail))
        {
            gameHolder.emailError.SetActive(true) ;
            return ; 
        }
        else
        {
            StartCoroutine("Login") ;
        }   
    }
    public void ButtonRegister ()
    {
        gameHolder.playerName = gameHolder.inputName.text ;
        gameHolder.playerConfirmed = gameHolder.inputConfirmedPassword.text ;
        if (gameHolder.inputConfirmedPassword.text != gameHolder.playerPass)
        {
            gameHolder.errorPassword.SetActive(true) ;
            return ;
        }
        StartCoroutine("Register") ;
    }
    public void ButtonPlayRanked ()
    {
        StartCoroutine("FindAMatch", 1) ;
    }
    public void ButtonPractice()
    {
        StartCoroutine("Practice") ;
    }
    public void ButtonPlayNonRanked ()
    {
        StartCoroutine("FindAMatch", 2) ;
    }
    public void ResetButtons ()
    {
        gameHolder.emailError.SetActive(false) ;
        gameHolder.errorName.SetActive(false) ;
        gameHolder.errorPassword.SetActive(false) ;
        gameHolder.goAccountRegister.SetActive(false) ;
        gameHolder.errorPassword2.SetActive(false) ;
        gameHolder.goAccountLogin.SetActive(true) ;
    }
    
    IEnumerator InitNetwork()
    {
        gameHolder.internalIP = Network.player.ipAddress ;

        if(!string.IsNullOrEmpty(gameHolder.internalIP))
        {
            Upnp.Discover(gameHolder.internalIP,GameSettings.gamePort.ToString(),"XLink","add",this);
        }

        WWW webRequest = new WWW(GameSettings.urlIP);
        yield return webRequest;
        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                string str = webRequest.text;
                string[] str1 = str.Split ('|');
                gameHolder.externalIP = str1[1];
            } 
        }
    }
    void DeviceFound(object sender, DeviceEventArgs args)
    {
        INatDevice device = args.Device;
        gameHolder.externalIP = device.GetExternalIP().ToString() ;
        upnpStatus = true ;
        device.CreatePortMap(new Mapping(Protocol.Udp, GameSettings.gamePort, GameSettings.gamePort));    
    }
    void DeviceLost(object sender, DeviceEventArgs args)
    {
        INatDevice device = args.Device;
        device.DeletePortMap(new Mapping(Protocol.Tcp, GameSettings.gamePort,GameSettings.gamePort));
    }
    
    IEnumerator Login ()
    {
        string request = "login" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.playerEmail+request+GameSettings.gamekey);
        string pass = Md5Sum (GameSettings.gamekey+gameHolder.playerPass+GameSettings.gamekey);

        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.playerEmail);
	    form.AddField("password", pass);
        form.AddField("hash", hash);
        WWW webRequest = new WWW(GameSettings.urlGame, form);
        yield return webRequest;

        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :      print("SQL Error") ;
                                            if (useDebugPanel) { gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    case "Create"   :       gameHolder.goAccountLogin.SetActive(false) ;
                                            gameHolder.goAccountRegister.SetActive(true) ;
                    break ;
                    case "Wrong Password" : gameHolder.errorPassword2.SetActive(true) ; 
                    break ;
                    case "Logged In" :      StartCoroutine("GetProfile") ;
                    break ;
                                        
                }
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
            }
        }

    }
    IEnumerator Register ()
    {
        string request = "register" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.playerEmail+request+GameSettings.gamekey);
        string pass = Md5Sum (GameSettings.gamekey+gameHolder.playerPass+GameSettings.gamekey);

        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.playerEmail);
	    form.AddField("password", pass);
        form.AddField("name", gameHolder.playerName) ;
        form.AddField("hash", hash);
        WWW webRequest = new WWW(GameSettings.urlGame, form);
        yield return webRequest;

        print (webRequest.text) ;

        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :      print("SQL Error") ;
                                            if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    case "Created" :        gameHolder.debug.NewDebug("SQL : Profile Created");
                                            gameHolder.goAccount.SetActive(false) ;
                                            gameHolder.goMainUI.SetActive(true) ;
                                            StartCoroutine("GetProfile") ;
                    break ;
                    case "Already Taken" :  gameHolder.errorName.SetActive(true) ;
                    break ;
                }
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
            }
        }
    }
    IEnumerator GetProfile ()
    {
        gameHolder.goAccount.SetActive(false) ;
        gameHolder.goMainUI.SetActive(true) ;

        string request = "getProfile" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.playerEmail+request+GameSettings.gamekey);

        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.playerEmail) ;
        form.AddField("hash", hash);
        WWW webRequest = new WWW(GameSettings.urlGame, form) ;
        yield return webRequest;

        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :  print("SQL Error") ;
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    default :           gameHolder.userProfile = JsonConvert.DeserializeObject<UserProfile>(webRequest.text); 
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("SQL : Received Profile {0} ", gameHolder.userProfile.userID.ToString())); }
                                        gameHolder.uiUserName.text = gameHolder.userProfile.userName ;
                                        gameHolder.uiUserRank.text = gameHolder.userProfile.userRank.ToString() ;
                                        gameHolder.uiUserScore.text = gameHolder.userProfile.userScore.ToString() ;
                                        gameHolder.uiUserVictories.text = gameHolder.userProfile.userVictories.ToString() ;
                                        gameHolder.uiUserDefeats.text = gameHolder.userProfile.userDefeats.ToString() ;
                    break ;
                }
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
            }
        }      
    }
    IEnumerator GetOpponent ()
    {
        string request = "getOpponent" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.playerEmail+request+GameSettings.gamekey);

        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.playerEmail) ;
        form.AddField("hash", hash);
        if (matchCreated) { form.AddField("playerID", gameHolder.playerMatch.matchClientID); }
        else { form.AddField("playerID", gameHolder.playerMatch.matchHostID); }

        WWW webRequest = new WWW(GameSettings.urlGame, form) ;
        yield return webRequest;

        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :  print("SQL Error") ;
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    default :           gameHolder.opponentProfile = JsonConvert.DeserializeObject<UserProfile>(webRequest.text);
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug("Received Opponent Profile"); }
                    break ;
                }
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
            }
        }      
    }
    IEnumerator UpdateMatch ()
    {
        string request = "updatematch" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.userProfile.userEmail+request+GameSettings.gamekey);

        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.userProfile.userEmail);
        form.AddField("hash", hash);
        form.AddField("matchID", gameHolder.playerMatch.matchID);
        WWW webRequest = new WWW(GameSettings.urlGame, form);
        yield return webRequest ;

        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :  print("SQL Error") ;
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    default :           gameHolder.playerMatch = JsonConvert.DeserializeObject<MatchInfo>(webRequest.text) ;
                    break ;
                }    
            }
        }
    }
    IEnumerator AddMatch (int type)
    {
        string request = "addmatch" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.userProfile.userEmail+request+GameSettings.gamekey);
        
        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.userProfile.userEmail);
        form.AddField("hash", hash);
        form.AddField("rank", gameHolder.userProfile.userRank);
        form.AddField("externalIP", gameHolder.externalIP);
        form.AddField("matchType", type);
        form.AddField("playerID", gameHolder.userProfile.userID);
        WWW webRequest = new WWW(GameSettings.urlGame, form);
        yield return webRequest ;

        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :  print("SQL Error") ;
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    default :           gameHolder.playerMatch = JsonConvert.DeserializeObject<MatchInfo>(webRequest.text) ;
                                        matchCreated = true ;
                    break ;
                }
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
            }
        }
    }
    IEnumerator DeleteMatch (int id)
    {
        string request = "deletematch" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.userProfile.userEmail+request+GameSettings.gamekey);
        
        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.userProfile.userEmail);
        form.AddField("hash", hash);
        form.AddField("matchID", id);
        WWW webRequest = new WWW(GameSettings.urlGame, form);
        yield return webRequest ;
        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :  print("SQL Error") ;
                                        if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    default :           string msg = string.Format("SQL : Match {0} Deleted", id.ToString()) ;
                                        if (useDebugPanel) { gameHolder.debug.NewDebug(msg) ; } 
                                        gameHolder.playerMatch = null ;
                                        matchCreated = false ;
                    break ;                       
                }
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
            }
        }

    }
    IEnumerator FindAMatch(int type)
    {
        StopClient () ;
        StopHost () ;

        string request = "get1match" ;
        string hash = Md5Sum (GameSettings.gamekey+gameHolder.userProfile.userEmail+request+GameSettings.gamekey) ;
        
        WWWForm form = new WWWForm();
        form.AddField("request", request) ;
        form.AddField("email", gameHolder.userProfile.userEmail);
        form.AddField("hash", hash);
        form.AddField("playerID", gameHolder.userProfile.userID) ;
        form.AddField("matchType", type);
        form.AddField("rank", gameHolder.userProfile.userRank);

        WWW webRequest = new WWW(GameSettings.urlGame, form);
        yield return webRequest;
        
        if (webRequest.isDone)
        {
            if(!string.IsNullOrEmpty(webRequest.text))
            {
                switch (webRequest.text)
                {
                    case "SQL Error" :      print("SQL Error") ;
                                            if (useDebugPanel) {  gameHolder.debug.NewDebug("SQL Error") ; }
                    break ;
                    case "No Match Found" : if (useDebugPanel) {  gameHolder.debug.NewDebug("No Match Found") ; }
                                            if (upnpStatus) { CreateHost(type) ; }
                                            else { if (useDebugPanel) {  gameHolder.debug.NewDebug("Can't Host, Waiting 15 seconds to retry") ; } yield return new WaitForSeconds(15) ; StartCoroutine("FindAMatch",type) ; }
                    break ;
                    default               : gameHolder.playerMatch = JsonConvert.DeserializeObject<MatchInfo>(webRequest.text) ;
                                            gameHolder.networkInfo.text = "Connecting" ;
                                            CreateRemoteClient() ;
                                            yield return new WaitForSeconds (10.0f) ;
                                            if (!client.isConnected)
                                            {
                                                StopClient () ;
                                                yield return StartCoroutine("DeleteMatch",gameHolder.playerMatch.matchID);
                                                StartCoroutine("FindAMatch",type) ;
                                            }
                                            else
                                            {
                                                yield break ;
                                            }
                    break ;
                }   
            }
            else
            {
                print ("Request Returned Empty") ;
                if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ;}
            }
        }  
    }
    IEnumerator EndMatch ()
    {
            string request = "" ;
            if (matchCreated)
            {
                request = "hostPostResult" ;
            }
            else
            {
                request = "clientPostResult" ;
            }
            
            string hash = Md5Sum (GameSettings.gamekey+gameHolder.userProfile.userEmail+request+GameSettings.gamekey);

            WWWForm form = new WWWForm();
            form.AddField("request", request) ;
            form.AddField("email", gameHolder.userProfile.userEmail);
            form.AddField("hash", hash);
            form.AddField("matchID", gameHolder.playerMatch.matchID);
            form.AddField("matchResult", victory);
            form.AddField("playerID", gameHolder.userProfile.userID);

            WWW webRequest = new WWW(GameSettings.urlGame, form);
            yield return webRequest ;

            if (webRequest.isDone)
            {
                if(!string.IsNullOrEmpty(webRequest.text))
                {
                    switch (webRequest.text)
                    {
                        case "Success" :    if (useDebugPanel) { gameHolder.debug.NewDebug("Results Posted") ; }
                                            matchCreated = false ;
                                            gameHolder.playerMatch = null ;
                                            victory = 0 ;
                                            yield return StartCoroutine("GetProfile") ;
                        break ;
                        case "SQL Error" :  if (useDebugPanel) { gameHolder.debug.NewDebug("SQL Error") ; }
                        break ;
                    }    
                }
                else
                {
                    if (useDebugPanel) {  gameHolder.debug.NewDebug("Request Returned Empty") ; }
                }
            } 
    }
    IEnumerator Practice()
    {
        if (useDebugPanel) {  gameHolder.debug.NewDebug("Launching Practice Against AI") ; }
        NetworkServer.Configure(gameHolder.netConnection, maxPlayers - 1);
        NetworkServer.Listen(GameSettings.gamePort);
        ServerCallBacks () ;
        CreateLocalClient() ;
        int randLevel = Random.Range(0, gameHolder.levelListAI.Count) ;
        levelName = gameHolder.levelListAI[randLevel] ;
        levelchangeOp = Application.LoadLevelAsync(levelName) ;
        while (!levelchangeOp.isDone) { yield return 0 ; }
        ClientScene.Ready(client.connection) ;
        while (true)
        {
            if (levelHolder == null) { yield return null ; } else { break ; }
        }
        while (true)
        {
            if (playerT01Cpn != null)
            {
                if (playerT01Cpn.ready)
                {
                    break ;
                }
            }
            yield return 0 ;
        }
        GameObject controller = Instantiate (gameHolder.levelController) as GameObject ;
        NetworkServer.Spawn(controller) ;
    }

    string Md5Sum(string key)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(key);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);

		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
    public bool IsValidEmailAddress(string s)
    {
	    var regex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
	    return regex.IsMatch(s);
    }

    //NETWORK SERVER CALLBACKS
    void ServerCallBacks ()
    {
        NetworkServer.RegisterHandler(MsgType.Connect, SCBConnect) ;
        NetworkServer.RegisterHandler(MsgType.Disconnect, SCBDisconnect) ;
        NetworkServer.RegisterHandler(MsgType.Ready, SCBReady) ;
        NetworkServer.RegisterHandler(MsgType.AddPlayer, SCBAddPlayer) ;
        NetworkServer.RegisterHandler((short) 42, new NetworkMessageDelegate(SCBGotProfile));
    }
    void SCBConnect (NetworkMessage netMsg)
    {
        playerCount ++ ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("SERVER : Client Connected on {0} from {1}", netMsg.conn.connectionId.ToString(), netMsg.conn.address)) ; }

        //If We have two players, Load a Level
        if (playerCount == 2) { StartCoroutine("ExchangeInfoServer") ;}
    }
    void SCBGotProfile (NetworkMessage netMsg)
    {
        gotOpponent ++ ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug("SERVER : Client got Profile") ;  }
    }
    void SCBDisconnect (NetworkMessage netMsg)
    {
        playerCount -- ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("SERVER : Client Disconnected on {0} from {1}", netMsg.conn.connectionId.ToString(), netMsg.conn.address)) ; }
    }
    void SCBReady (NetworkMessage netMsg)
    {
        playerReady ++ ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("SERVER : Client Ready on {0} from {1}", netMsg.conn.connectionId.ToString(), netMsg.conn.address)) ; }

        //If Both players are ready, Send the message to create their players.
        if (playerReady == playerCount) { NetworkServer.SendToAll((short) 41, (MessageBase) new IntegerMessage(1)); }
    }
    void SCBAddPlayer (NetworkMessage netMsg)
    {
        if (netMsg.conn.connectionId == -1)
        {
            GameObject player = Instantiate (gameHolder.playerPrefab) as GameObject;
            player.GetComponent<NetPlayer>().team = 1 ;
            NetworkServer.AddPlayerForConnection(netMsg.conn, player , 0) ;
            if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("SERVER : Local Client Added Player on {0}", netMsg.conn.connectionId.ToString())) ; }
        }
        if (netMsg.conn.connectionId == 1)
        {
            GameObject player = Instantiate (gameHolder.playerPrefab) as GameObject;
            player.GetComponent<NetPlayer>().team = 2 ;
            NetworkServer.AddPlayerForConnection(netMsg.conn, player , 0) ;
            if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("SERVER : Remote Client Added Player on {0}", netMsg.conn.connectionId.ToString())) ; }
        }  
    }

    //NETWORK CLIENT CALLBACKS
    void ClientCallBacks ()
    {
        client.RegisterHandler(MsgType.Connect, CCBConnect);
        client.RegisterHandler(MsgType.Disconnect, CCBDisconnect);
        client.RegisterHandler((short) 39, new NetworkMessageDelegate(CCBLoad));
        client.RegisterHandler((short) 41, new NetworkMessageDelegate(CCBReady));
    } 
    void CCBConnect (NetworkMessage netMsg)
    {
        gameHolder.networkInfo.text = "Connected To Opponent" ;
        StartCoroutine("ExchangeInfoClient") ;
        RegisterPrefabs () ;
    }
    void CCBDisconnect (NetworkMessage netMsg)
    {
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("CLIENT : Disconnected from {0}", netMsg.conn.address)) ; }
    }
    void CCBLoad (NetworkMessage netMsg)
    {
        gameHolder.networkInfo.text = "Loading level" ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("CLIENT : Received Load Scene Message from {0}", netMsg.conn.address)) ; }
        levelName = netMsg.reader.ReadString();
        StartCoroutine("ChangeSceneClient") ; 
    }
    void CCBReady (NetworkMessage netMsg)
    {
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("CLIENT : Received Server Ready Message from {0}", netMsg.conn.address)) ; }
        ClientScene.AddPlayer(client.connection, (short) 0);
    }

    //NETWORK LOCAL CLIENT CALLBACKS
    void LocalClientCallBacks ()
    {
        client.RegisterHandler(MsgType.Connect, LCCBConnect);
        client.RegisterHandler((short) 41, new NetworkMessageDelegate(LCCBReady));
    }
    void LCCBConnect (NetworkMessage netMsg)
    {
        playerCount ++ ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug("LOCALCLIENT : Connected to Server") ; }
        RegisterPrefabs () ;
    }
    void LCCBReady (NetworkMessage netMsg)
    {
        if (useDebugPanel) {  gameHolder.debug.NewDebug("LOCALCLIENT : Received Server Ready Message, Requesting Player") ; }
        ClientScene.AddPlayer(client.connection, (short) 0);
    }

    //CREATE NETWORK
    void CreateHost (int type)
    {
        playerCount = 0 ;
        playerReady = 0 ;
        NetworkServer.Configure(gameHolder.netConnection, maxPlayers - 1);
        NetworkServer.Listen(GameSettings.gamePort);
        if (!NetworkServer.active) 
        {
            if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : failed to listen on port {0}", GameSettings.gamePort.ToString())) ; }
        }
        else 
        {
            ServerCallBacks () ;
            StartCoroutine("AddMatch", type) ;
            if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : Started on {0}:{1}", gameHolder.externalIP, GameSettings.gamePort.ToString())) ; }
            CreateLocalClient() ;
            StartCoroutine("ServerWaitOpponent");
        }
    }
    void CreateLocalClient ()
    {   
        client = new NetworkClient();
        client.Configure(gameHolder.netConnection, 1) ;
        client = ClientScene.ConnectLocalServer();
        LocalClientCallBacks () ;
    }
    void CreateRemoteClient ()
    {
        client = new NetworkClient();
        client.Configure(gameHolder.netConnection, 1) ;
        client.Connect(gameHolder.playerMatch.matchExternalIP, GameSettings.gamePort);
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format ("CLIENT : Connecting to {0}", gameHolder.playerMatch.matchExternalIP)) ; }
        ClientCallBacks() ;
    }
    void RegisterPrefabs ()
    {
        ClientScene.RegisterPrefab(gameHolder.playerPrefab) ;
        ClientScene.RegisterPrefab(gameHolder.levelController) ;
        foreach (GameObject go in gameHolder.itemList)
        {
            ClientScene.RegisterPrefab(go) ;
        }
    }

    //NETWORK STOP CONNECTIONS
    void StopHost ()
    {
        if (NetworkServer.active)
        {
            NetworkServer.DisconnectAll() ;
            NetworkServer.Shutdown() ;
            if (useDebugPanel) {  gameHolder.debug.NewDebug("SERVER : Stopped") ; }
        }
    }
    void StopClient ()
    {
        if (NetworkClient.active)
        {
            if (client != null)
            {
                if (client.isConnected) { client.Disconnect() ; }
                client.Shutdown () ;
                client = null ;
            } 
        }
        NetworkClient.ShutdownAll () ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug("CLIENT : Stopped") ; }
    }

    //NETWORK GAME LOGIC
    IEnumerator ExchangeInfoServer ()
    {
        yield return StartCoroutine("UpdateMatch");
        yield return StartCoroutine("GetOpponent");
        gotOpponent ++ ;
    }
    IEnumerator ExchangeInfoClient ()
    {
        yield return StartCoroutine("UpdateMatch");
        yield return StartCoroutine("GetOpponent");
        client.Send((short) 42, (MessageBase) new IntegerMessage(1)) ;
    }
    IEnumerator ServerWaitOpponent ()
    {
        gameHolder.networkInfo.text = "Waiting Opponent" ;
        while (true)
        {
            if (gotOpponent == 2) { break ; }
            yield return new WaitForSeconds(1) ;
        }
        ServerLoadALevel () ;
    }
    void ServerLoadALevel ()
    {
        int randLevel = Random.Range(0, gameHolder.levelList.Count) ;
        levelName = gameHolder.levelList[randLevel] ;
        StartCoroutine("ChangeSceneServer") ;
        NetworkServer.SendToAll((short) 39, (MessageBase) new StringMessage(levelName));
    }
    IEnumerator ChangeSceneServer ()
    {
        gameHolder.networkInfo.text = "Loading level" ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : Loading {0}", levelName)) ; }
        levelchangeOp = Application.LoadLevelAsync(levelName) ;
        while (!levelchangeOp.isDone) { yield return 0 ; }
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : Loaded {0}", levelName)) ; }

        //Wait For everything to be ready before launching the Level Controller
        StartCoroutine ("ServerWaitToStart") ;

        //Set Local Client Ready
        ClientScene.Ready(client.connection) ;
    }
    IEnumerator ChangeSceneClient ()
    {
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("CLIENT : Loading {0}", levelName)) ; }
        levelchangeOp = Application.LoadLevelAsync(levelName) ;
        while (!levelchangeOp.isDone) { yield return 0 ; }
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("CLIENT : Loaded {0}", levelName)) ; }

        //Set Client Ready
        ClientScene.Ready(client.connection) ;   
    }
    IEnumerator ServerWaitToStart ()
    {
        //Wait For Level Holder
        while (true)
        {
            if (levelHolder == null) { yield return null ; } else { break ; }
        }

        //Wait For Player 1
        while (true)
        {
            if (playerT01Cpn != null)
            {
                if (playerT01Cpn.ready)
                {
                    break ;
                }
            }
            yield return 0 ;
        }

        //Wait For Player 2
        while (true)
        {
            if (playerT02Cpn != null)
            {
                if (playerT02Cpn.ready)
                {
                    break ;
                }
            }
            yield return 0 ;
        }

        GameObject controller = GameObject.Instantiate (gameHolder.levelController) as GameObject ;
        NetworkServer.Spawn(controller) ;

        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : Level Controller Spawned, Handed over control")) ; }
    }

    //PLAYER LOGIC
    public void HandOver ()
    {
        gameHolder.cameraGO.SetActive(false) ;
        gameHolder.uiGO.SetActive(false) ;    
    }
    public IEnumerator TakeOver ()
    {  
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("GAME FINISHED : Victory for team {0}", victory.ToString())) ; }
        levelchangeOp = Application.LoadLevelAsync("Lobby") ;
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : Loading {0}", levelName)) ; }
        while (!levelchangeOp.isDone) { yield return 0 ; }
        if (useDebugPanel) {  gameHolder.debug.NewDebug(string.Format("SERVER : Loaded {0}", levelName)) ; }
        gameHolder.cameraGO.SetActive(true) ;
        gameHolder.uiGO.SetActive(true) ;

        //Reset Variables and Network
        StopHost() ;
        StopClient () ;
        playerT01 = null ;
        playerT02 = null ;
        playerT01Cpn = null ;
        playerT02Cpn = null ;
        client = null ;
        playerCount = 0 ;
        playerReady = 0 ;
        gotOpponent = 0 ;
        levelName = string.Empty ;
        
        if (victory != 0) { StartCoroutine ("EndMatch") ; } 
    }
//------------------------------------------------------------------------------------------------------------------------------//
//															END																	//
//------------------------------------------------------------------------------------------------------------------------------//
} 
