<?php

//------------------------------------------------------------------------------------------------------------------------------//
//													    VARIABLES                   							                //
//------------------------------------------------------------------------------------------------------------------------------//
$DBHOST	 		= "localhost";
$DBUSER			= "xlink";
$DBPASS			= "password";
$DATABASE		= "xlink";
$secretKey		= "secretkey" ;
$conn 			= mysqli_connect("$DBHOST","$DBUSER","$DBPASS","$DATABASE") ;
	
//PLAYER PROFILE
$email			= $_POST['email'] ;
$password		= $_POST['password'] ;
$name			= $_POST['name'] ;
$playerid       = $_POST['playerID'] ;
$rank           = $_POST['rank'] ;
	
//MATCH MAKING
$matchid 		= $_POST['matchID'] ;
$matchtype 		= $_POST['matchType'] ;
$matchresult    = $_POST['matchResult'] ;
$externalip 	= $_POST['externalIP'] ;
$port		    = $_POST['port'] ;
	
//REQUEST
$request		= $_POST['request'] ;
	
//SECURITY
$skey 			= $_POST['skey'] ;
$hash 			= $_POST['hash'] ;
$realhash 		= md5($secretKey.$email.$request.$secretKey);

//------------------------------------------------------------------------------------------------------------------------------//
//													    SCRIPT                   							                    //
//------------------------------------------------------------------------------------------------------------------------------//

// SECURITY CHECKS
if (mysqli_connect_errno()) { die ("SQL error") ;}
if ($realhash != $hash) { die("SQL Error") ; }


//REQUEST LOGIN
if ($request == "login") 
{
	$result = mysqli_query($conn,"SELECT * FROM users WHERE userEmail = '$email' LIMIT 1") ;
    if ($result)
    {
	    $count = mysqli_num_rows($result);
	    if ($count == 0)
	    {
		    echo "Create" ;
	    }
	    else
	    {
		    while ($row = mysqli_fetch_array($result))
		    {
			    if ($password != $row['userPassword'])
			    {
				    echo "Wrong Password" ;
			    }
			    else
			    {
				    echo "Logged In" ;
			    }
		    }
	    }
    }
    else
    {
        echo "SQL Error" ;
    }
}

//REQUEST REGISTER
if ($request == "register") 
{
	$result = mysqli_query($conn,"SELECT * FROM users WHERE userName = '$name' LIMIT 1") ;
    if ($result)
    {
	    $count = mysqli_num_rows($result);
	    if ($count == 0)
	    {
		    $result = mysqli_query($conn,"INSERT INTO users (userEmail,userPassword,userName) VALUES ('$email','$password','$name');") ;
		    $result = mysqli_query($conn,"SELECT * FROM users WHERE userEmail = '$email' LIMIT 1") ;
		    $count = mysqli_num_rows($result);
		    if ($count == 1)
		    {
			    echo "Created" ;
		    }
		    else
		    {
			    echo "Error During Profile Creation" ;
		    }
	    }
	    else
	    {
		    echo "Already Taken" ;
	    }
    }
    else
    {
        echo "SQL Error" ;
    }
}

//REQUEST GET PROFILE
if ($request == "getProfile") 
{
	$result = mysqli_query($conn,"SELECT * FROM users WHERE userEmail = '$email' LIMIT 1") ;
    if ($result)
    {
	    while ($row = mysqli_fetch_array($result, MYSQL_ASSOC))
	    {
            $row_array['userID'] = $row['userID'];
            $row_array['userEmail'] = $row['userEmail'];
            $row_array['userName'] = $row['userName'];
            $row_array['userRank'] = $row['userRank'];
            $row_array['userScore'] = $row['userScore'];
            $row_array['userVictories'] = $row['userVictories'];
            $row_array['userDefeats'] = $row['userDefeats'];

            $row_array['userUnlockedLink'] = $row['userUnlockedLink'];
            $row_array['userUnlockedCannon'] = $row['userUnlockedCannon'];
            $row_array['userUnlockedLaser'] = $row['userUnlockedLaser'];
            $row_array['userUnlockedBeam'] = $row['userUnlockedBeam'];
            $row_array['userUnlockedRepair'] = $row['userUnlockedRepair'];
            $row_array['userUnlockedShield'] = $row['userUnlockedShield'];
            $row_array['userUnlockedMagTank'] = $row['userUnlockedMagTank'];
            $row_array['userUnlockedDrone'] = $row['userUnlockedDrone'];
            $row_array['userUnlockedMagShield'] = $row['userUnlockedMagShield'];
            $row_array['userUnlockedSwarmer'] = $row['userUnlockedSwarmer'];
            $row_array['userUnlockedSpiderBot'] = $row['userUnlockedSpiderBot'];

            echo json_encode($row_array);
	    }
    }
    else
    {
        echo "SQL Error" ;
    }
}


//REQUEST GET OPPONENT
if ($request == "getOpponent") 
{
	$result = mysqli_query($conn,"SELECT * FROM users WHERE userID = '$playerid' LIMIT 1") ;
    if ($result)
    {
	    while ($row = mysqli_fetch_array($result, MYSQL_ASSOC))
	    {
            $row_array['userID'] = $row['userID'];
            $row_array['userName'] = $row['userName'];
            $row_array['userRank'] = $row['userRank'];
            $row_array['userScore'] = $row['userScore'];
            $row_array['userVictories'] = $row['userVictories'];
            $row_array['userDefeats'] = $row['userDefeats'];

            echo json_encode($row_array);
	    }
    }
    else
    {
        echo "SQL Error" ;
    }
}

//GET ONE MATCH
if ($request == "get1match")
{
	$result = mysqli_query($conn,"SELECT * FROM matches WHERE matchType = '$matchtype' AND abs(matchRank - '$rank') <= 10 AND matchInPlay = 0 LIMIT 1") ;
    if ($result)
    {
        $count = mysqli_num_rows($result) ;
        if ($count == 0)
        {
            echo "No Match Found" ;
        }
        else
        {
            while ($row = mysqli_fetch_array($result))
            {
                $toJoin = $row['matchID'];
            }

            $sql = mysqli_query($conn,"UPDATE matches SET matchClientID = '$playerid', matchInPlay = 1 WHERE matchID = '$toJoin'") ;
            if ($sql) 
            {
                $result = mysqli_query($conn,"SELECT * FROM matches WHERE matchID = '$toJoin' LIMIT 1") ;
                while ($row = mysqli_fetch_array($result))
                {
                    $row_array['matchID'] = $row['matchID'];
                    $row_array['matchType'] = $row['matchType'];
                    $row_array['matchRank'] = $row['matchRank'];
                    $row_array['matchExternalIP'] = $row['matchExternalIP'];
                    $row_array['matchHostID'] = $row['matchHostID'];
                    $row_array['matchClientID'] = $row['matchClientID'];

                    echo json_encode($row_array); 
                }
            }
            else
            {
                echo "SQL Error" ;
            }
        }
    }
    else
    {
        echo "SQL Error" ;
    }
}

//ADD MATCH
if ($request == "addmatch")
{
    $result = mysqli_query($conn,"SELECT * FROM matches WHERE matchHostID = '$playerid' AND matchInPlay = 0 AND matchType = '$matchtype' LIMIT 1") ;
    if ($result)
    {
        $count = mysqli_num_rows($result);
        if ($count == 0)
        {
            $sql="INSERT INTO matches (matchType,matchRank,matchExternalIP,matchHostID)
                    VALUES ('$matchtype','$rank','$externalip','$playerid')" ;
            if (mysqli_query($conn,$sql)) 
            {
                $result = mysqli_query($conn,"SELECT * FROM matches WHERE matchHostID = '$playerid' AND matchInPlay = 0 AND matchType = '$matchtype' LIMIT 1") ;
                while ($row = mysqli_fetch_array($result))
                {
                    $row_array['matchID'] = $row['matchID'];
                    $row_array['matchType'] = $row['matchType'];
                    $row_array['matchRank'] = $row['matchRank'];
                    $row_array['matchExternalIP'] = $row['matchExternalIP'];
                    $row_array['matchHostID'] = $row['matchHostID'];

                    echo json_encode($row_array); 
                }
            }
            else
            {
                echo "SQL Error" ;
            }
        }
        else
        {
            while ($row = mysqli_fetch_array($result))
            {
                $row_array['matchID'] = $row['matchID'];
                $row_array['matchType'] = $row['matchType'];
                $row_array['matchRank'] = $row['matchRank'];
                $row_array['matchExternalIP'] = $row['matchExternalIP'];
                $row_array['matchHostID'] = $row['matchHostID'];

                echo json_encode($row_array); 
            }
        }
    }
    else
    {
        echo "SQL Error" ;
    }
}

//UPDATE MATCH
if ($request == "updatematch")
{
    $result = mysqli_query($conn,"SELECT * FROM matches WHERE matchID = '$matchid' LIMIT 1") ;
    if ($result)
    {
        while ($row = mysqli_fetch_array($result))
        {
            $row_array['matchID'] = $row['matchID'];
            $row_array['matchType'] = $row['matchType'];
            $row_array['matchRank'] = $row['matchRank'];
            $row_array['matchExternalIP'] = $row['matchExternalIP'];
            $row_array['matchHostID'] = $row['matchHostID'];
            $row_array['matchClientID'] = $row['matchClientID'];

            echo json_encode($row_array); 
        }
    }
    else
    {
        echo "SQL Error" ;       
    }
}

//POST Host RESULT
if ($request == "hostPostResult")
{
    $result = mysqli_query($conn,"UPDATE matches SET matchHostResult = '$matchresult' WHERE matchID = '$matchid'") ;
    if ($result)
    { 
        if ($matchresult == 1)
        {
            $result2 = mysqli_query($conn,"UPDATE users SET userVictories = userVictories + 1 WHERE userID = '$playerid'") ;
            if ($result2)
            {
                echo"Success" ;   
            }
        }
        if ($matchresult == 2)
        {
            $result2 = mysqli_query($conn,"UPDATE users SET userDefeats = userDefeats + 1 WHERE userID = '$playerid'") ;
            if ($result2)
            {
                echo"Success" ;   
            }
        }
    }
    else
    {
        echo "SQL Error" ;
    }    
}

//POST Client RESULT
if ($request == "clientPostResult")
{
    $result = mysqli_query($conn,"UPDATE matches SET matchClientResult = '$matchresult' WHERE matchID = '$matchid'") ;
    if ($result) 
    {
        if ($matchresult == 2)
        {
            $result2 = mysqli_query($conn,"UPDATE users SET userVictories = userVictories + 1 WHERE userID = '$playerid'") ;
            if ($result2)
            {
                echo"Success" ;   
            }
        }
        if ($matchresult == 1)
        {
            $result2 = mysqli_query($conn,"UPDATE users SET userDefeats = userDefeats + 1 WHERE userID = '$playerid'") ;
            if ($result2)
            {
                echo"Success" ;   
            }
        }
    }
    else
    {
        echo "SQL Error" ;
    }    
}

//DELETE MATCH
if ($request == "deletematch")
{
    $result = mysqli_query($conn,"DELETE FROM matches WHERE matchID = '$matchid'") ; 
    if ($result) 
    {
        echo"Success" ;
    }
    else
    {
        echo "SQL Error" ;
    }
}
//------------------------------------------------------------------------------------------------------------------------------//
//													    END SCRIPT                   							                //
//------------------------------------------------------------------------------------------------------------------------------//

mysqli_close($conn);
?>