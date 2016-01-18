using System;

namespace MasterServer
{
    public class Match
    {
        public  int         id          ;
        public  Player      player1     ;
        public  Player      player2     ;
        public  int         gsId        ;
        public  string      gsIp        ;
        public  string      levelName   ;

        public Match
        (
            int     newId,
            Player  newPlayer1,
            Player  newPlayer2,
            int     newGsId,
            string  newGsIp,
            string  newName
        )
        {
            id      = newId;
            player1 = newPlayer1;
            player2 = newPlayer2;
            gsId    = newGsId;
            gsIp    = newGsIp;
            levelName = newName;
        }
    }

    class MatchMaker
    {
        public static void GetMatch(Player player2)
        {
            MasterServer.Log(2,"Player "+player2.id+" | Received Match Request");

            //Is the player already in a queue ?
            if (MasterServer.listQueue.Contains(player2))
            {
                MasterServer.Log(2,"Player "+player2.id+" | Was already in Queue");
                MasterServer.listQueue.Remove(player2);
            }

            //Is there someone else waiting ?
            if (MasterServer.listQueue.Count > 0 && MasterServer.listGameServer.Count > 0)
            {
                for (int i=0; i<MasterServer.listQueue.Count; i ++)
                {
                    //Is its rank close enough?
                    if ( Math.Abs(player2.rank - MasterServer.listQueue[i].rank) <= Settings.maxRankDiff)
                    {
                        Player player1 = MasterServer.listQueue[i];
                        //Is the connection still alive?
                        if (player1.Alive()) //Connection to player 1 is Alive, Create the match
                        {
                            //Find GS and Load Balance the match
                            int gsid = 9999 ;
                            for (int g=0; g<MasterServer.listGameServer.Count; g++) {if (MasterServer.listGameServer[g].matchCount < gsid && MasterServer.listGameServer[g].serverStarted) { gsid = g ; }  }
                            
                            //Make sure we got a running game server
                            if (gsid == 9999) {  break ;}

                            //Pick a Random Level
                            int randomMap = new Random().Next( 0, MasterServer.listLevel.Count );

                            //Create the match
                            Match match = new Match(MasterServer.listMatch.Count, player1, player2, gsid, MasterServer.listGameServer[gsid].ip, MasterServer.listLevel[randomMap]);
                            MasterServer.listMatch.Add(match);

                            //Send Match Info
                            player1.SendMatchInfo(match,1);
                            player2.SendMatchInfo(match,2);

                            MasterServer.Log(1,"New Match : "+match.id+" | "+player1.id+" vs "+player2.id+ " On Map "+match.levelName+" | GameServer " +match.gsId);
                            MasterServer.listQueue.RemoveAt(i);
                            return ;
                        }
                        else
                        {
                            player1.CleanMe(); 
                        }
                    }
                } 
            }

            //Nobody in the queue, Put the player in the queue
            MasterServer.Log(2,"Player "+player2.id+" | Has been put in queue");
            MasterServer.listQueue.Add(player2);

            //Send the message to the player
            player2.SendMsg("2");
        }
        public static void CancelMatch(Player player)
        {
            if (MasterServer.listQueue.Contains(player))
            {
                MasterServer.Log(2,"Player "+player.id+" | Removed from match queue");
                MasterServer.listQueue.Remove(player);
            }    
        }
    }
}
