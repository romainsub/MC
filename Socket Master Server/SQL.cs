using System;
using MySql.Data.MySqlClient;
using System.Data;

namespace MasterServer
{
    class SQL
    {
        public static MySqlConnection sqlCon = null ;
        public static string con = "server=10.0.0.1;port=3306;database=XLink;userid=xlink;password=xldb2016;";
        public static int tf_player_id = 0 ;
        public static int tf_player_username = 1 ;
        public static int tf_player_email = 2 ;
        public static int tf_player_password = 3 ;
        public static int tf_player_rank = 4 ;
        public static int tf_player_ladder = 5 ;

        public static bool SQLConnect ()
        {

            if (sqlCon == null)
            {
                try
                {
                    sqlCon = new MySqlConnection(con);
                    sqlCon.Open();
                    MasterServer.Log(2,"Connected to Database");
                    return true;
                }
                catch (Exception ex)
                {
                    MasterServer.Log(0,"Error while connection to Database : " +ex);
                    return false ;
                }    
            }
            else
            {
                if (sqlCon.State != ConnectionState.Open)
                {
                    MasterServer.Log(0,"Connection to Database exist but is not opened");
                }    
                return true;
            }
        }
        public static void GetLevelList ()
        {
            if (SQLConnect())
            {
                MySqlDataReader reader = null;
                string sqlQuery = "SELECT * FROM XLink.level";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, sqlCon);
                reader = cmd.ExecuteReader();
                if(reader.HasRows)
                {
                    while(reader.Read())
                    {
                        MasterServer.listLevel.Add(reader.GetString(1));
                        MasterServer.Log(0,"Level Added To List : "+ reader.GetString(1));
                    }
                }
                reader.Close();
            }
        }
        public static int GetPlayerID (string email, string password)
        {
            // -1 : wrong pass / -2 : email dont exist / -3 : Db Error / x : PlayerID

            int id = -1;
            if (SQLConnect())
            {
                //Check if email exist
                MySqlDataReader reader = null;
                string sqlQuery = "SELECT * FROM XLink.player WHERE email = '"+email+"'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, sqlCon);
                reader = cmd.ExecuteReader();
                string passRead = "";
                if(reader.HasRows)
                {
                    //Check If Password is the same
                    while (reader.Read())
                    {
                        passRead = reader.GetString(tf_player_password);
                        if (passRead == password)
                        {
                            id = Convert.ToInt32(reader.GetString(tf_player_id));
                        }
                    } 
                }
                else // email does not exist
                {
                    id = -2 ;
                }
                reader.Close();
            }
            else
            {
                id = -3;
            }
            return id;
        }
        public static void GetPlayerProfile (Player player)
        {
            if (SQLConnect())
            {
                MySqlDataReader reader = null;
                string sqlQuery = "SELECT * FROM XLink.player WHERE id = '"+player.id+"'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, sqlCon);
                reader = cmd.ExecuteReader();
                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        player.username = reader.GetString(tf_player_username);
                        player.rank = Convert.ToInt32(reader.GetString(tf_player_rank));
                        player.ladder = Convert.ToInt32(reader.GetString(tf_player_ladder));
                    }   
                }
                reader.Close();
            }
        }
        public static bool Register (string email, string username, string password)
        {
            bool result = false ;
            if (SQLConnect())
            {
                //Check if username exist
                MySqlDataReader reader1 = null;
                string sqlQuery1 = "SELECT * FROM XLink.player WHERE username = '"+username+"'";
                MySqlCommand cmd1 = new MySqlCommand(sqlQuery1, sqlCon);
                reader1 = cmd1.ExecuteReader();
                if(reader1.HasRows)
                {
                    result = false ;
                }
                else // username does not exist
                {
                    result =  true ;
                }
                reader1.Close();

                //Create the account
                if (result)
                {
                    string sqlQuery2 = "INSERT INTO XLink.player (email, username, password) VALUES ('"+email+"','"+username+"','"+password+"')";
                    MySqlCommand cmd2 = new MySqlCommand(sqlQuery2, sqlCon);
                    int aff = cmd2.ExecuteNonQuery();
                    if (aff <= 0)
                    {
                        result = false ;
                    }
                    else
                    {
                        result = true ;    
                    }
                }
            }
            return result ;
        }
    }
}
