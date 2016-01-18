//------------------------------------------------------------------------------------------------------------------------------//
//													    LiveMagTank                   							                //
//------------------------------------------------------------------------------------------------------------------------------//
/*

*/
//------------------------------------------------------------------------------------------------------------------------------//
//														INIT																	//
//------------------------------------------------------------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System ;
using UnityEngine.Audio ;


public class LiveMagTank : MonoBehaviour
{
//------------------------------------------------------------------------------------------------------------------------------//
//														VARIABLES																//
//------------------------------------------------------------------------------------------------------------------------------//
    public  int                                         team                                            = 1                     ;
    public  GameObject                                  part                                            = null                  ;
    public  Item                                        item                                            = null                  ;

    //SFX
    public  AudioSource                                 audioCpn                                        = null                  ;
    public  float                                       pitchRandom                                     = 0.1f                  ;

    //TARGETING
    public  SphereCollider                              sphereDetect                                    = null                  ;
    public  Transform                                   turret_RotY                                     = null                  ;
    public  Transform                                   turret_RotX                                     = null                  ;
    public  Transform                                   shootingPoint                                   = null                  ;
    public  GameObject                                  targetGo                                        = null                  ;
    public  Life                                        targetLife                                      = null                  ;
    public  Transform                                   targetPoint                                     = null                  ;
    public  bool                                        onSight                                         = false                 ;                                            
    public  List<GameObject> 		                    targetList                                      = new List<GameObject>();
    private float                                       turretRotSpeed                                  = 0.0f                  ;
    private int                                         enemyLayer                                      = 0                     ;
    private int                                         maxTargetCount                                  = 0                     ;
    private int                                         targetCount                                     = 0                     ;
    private float                                       backSpeed                                       = 0.0f                  ;
    private float                                       lastShot                                        = 0.0f                  ;
    private float                                       rate                                            = 0.0f                  ;

    //Projectiles
    private int                                         currentProj                                     = 1                     ;
    public  Transform                                   projParent                                      = null                  ;
    public  GameObject                                  proj01Go                                        = null                  ;
    public  GameObject                                  proj02Go                                        = null                  ;
    public  GameObject                                  proj03Go                                        = null                  ;
    public  GameObject                                  proj04Go                                        = null                  ;
    public  GameObject                                  proj05Go                                        = null                  ;
    public  Transform                                   proj01Tr                                        = null                  ;
    public  Transform                                   proj02Tr                                        = null                  ;
    public  Transform                                   proj03Tr                                        = null                  ;
    public  Transform                                   proj04Tr                                        = null                  ;
    public  Transform                                   proj05Tr                                        = null                  ;
    public  Projectile                                  proj01Cpn                                       = null                  ;
    public  Projectile                                  proj02Cpn                                       = null                  ;
    public  Projectile                                  proj03Cpn                                       = null                  ;
    public  Projectile                                  proj04Cpn                                       = null                  ;
    public  Projectile                                  proj05Cpn                                       = null                  ;

    //Fx
    public  PKFxFX                                      fx_Birth                                        = null                  ;
//------------------------------------------------------------------------------------------------------------------------------//
//													VOID USER DEFINED															//
//------------------------------------------------------------------------------------------------------------------------------//
    public void Init ()
    {
        if (turretRotSpeed == 0.0f) {  turretRotSpeed = GameSettings.item_Rotation_Speed ; }
        if (backSpeed == 0.0f) { backSpeed = GameSettings.item_RotationBack_Speed ; }
        if (maxTargetCount == 0)    {maxTargetCount = GameSettings.targetCountMax ; }
        if (targetCount > 0 ) { targetCount = 0 ; }
        if (onSight) { onSight = false ; }
        if (targetList.Count > 0) { targetList.Clear() ; }

        switch (team)
        {
            case 1  :   sphereDetect.radius = GameSettings.item_Cannon_Range ; enemyLayer = 1 << 8 | 1 << 14 ; break ;
            case 2  :   sphereDetect.radius = GameSettings.item_Cannon_Range ; enemyLayer = 1 << 8 | 1 << 13 ; break ;
        }
        
        sphereDetect.enabled = true ;
        
        //Projectiles
        proj01Tr.parent = item.levelHolder.proj_Parent ;
        proj02Tr.parent = item.levelHolder.proj_Parent ;
        proj03Tr.parent = item.levelHolder.proj_Parent ;
        proj04Tr.parent = item.levelHolder.proj_Parent ;
        proj05Tr.parent = item.levelHolder.proj_Parent ;

        part.SetActive(true) ;
        StartCoroutine ("TrackTarget") ;
        StartCoroutine ("Shoot") ;
    }
    public void Stop ()
    {
        StopCoroutine ("TrackTarget") ;
        StopCoroutine ("Shoot") ;
        sphereDetect.enabled = false ;
        targetGo = null ;
        targetLife = null ;
        targetPoint = null ;
        if (targetCount > 0 ) { targetCount = 0 ; }
        if (onSight) { onSight = false ; }
        if (targetList.Count > 0) { targetList.Clear() ; }

        part.SetActive(false) ; 
    }
    IEnumerator Shoot ()
    {
        if (rate == 0.0f) { rate = GameSettings.item_Cannon_Rate ; }
        lastShot = 0.0f ;
        currentProj = 1 ;

        while (true)
        {
            if (onSight && Time.timeSinceLevelLoad > lastShot + rate)
            {
                audioCpn.pitch = UnityEngine.Random.Range(1.0f - pitchRandom, 1.0f + pitchRandom);
                audioCpn.Play() ;
                fx_Birth.StopEffect() ;
                fx_Birth.StartEffect() ;
                switch (currentProj)
                {
                    case 1  :   lastShot = Time.timeSinceLevelLoad ;
                                proj01Tr.position = shootingPoint.position ;
                                proj01Tr.rotation = shootingPoint.rotation ;
                                proj01Go.SetActive(true);
                                proj01Cpn.StartCoroutine("Run") ;
                                currentProj = 2 ;
                    break ;
                    case 2  :   lastShot = Time.timeSinceLevelLoad ;
                                proj02Tr.position = shootingPoint.position ;
                                proj02Tr.rotation = shootingPoint.rotation ;
                                proj02Go.SetActive(true);
                                proj02Cpn.StartCoroutine("Run") ;
                                currentProj = 3 ;
                    break ;
                    case 3  :   lastShot = Time.timeSinceLevelLoad ;
                                proj03Tr.position = shootingPoint.position ;
                                proj03Tr.rotation = shootingPoint.rotation ;
                                proj03Go.SetActive(true);
                                proj03Cpn.StartCoroutine("Run") ;
                                currentProj = 4;
                    break ;
                    case 4  :   lastShot = Time.timeSinceLevelLoad ;
                                proj04Tr.position = shootingPoint.position ;
                                proj04Tr.rotation = shootingPoint.rotation ;
                                proj04Go.SetActive(true);
                                proj04Cpn.StartCoroutine("Run") ;
                                currentProj = 5 ;
                    break ;
                    case 5  :   lastShot = Time.timeSinceLevelLoad ;
                                proj05Tr.position = shootingPoint.position ;
                                proj05Tr.rotation = shootingPoint.rotation ;
                                proj05Go.SetActive(true);
                                proj05Cpn.StartCoroutine("Run") ;
                                currentProj = 1 ;
                    break ;
                }
            }
            yield return 0 ;
        }
    }
    IEnumerator TrackTarget()
    {
        Vector3 aimVectorY ;
		Vector3 aimVectorX ;
		Quaternion rotateY ;
		Quaternion rotateX ;
        RaycastHit hit ;
		Vector3 dest ;

        while (true)
        {
            //Remove Deactivated Objects From List
            for (int i=0; i < targetList.Count ; i++) { if (!targetList[i].activeSelf) { targetList.Remove(targetList[i]) ; } }

            //If we have a target
            if (targetGo != null)
            {
                //Check If Target Is Still Active
                if (!targetGo.activeSelf)
                {
                    targetGo = null ;
                    targetLife = null ;
                    targetPoint = null ;
                }
                else
                {
                    //Check if target is still alive
                    if (targetLife.killed) { targetList.Remove(targetGo) ; }

                    //Check if target still in range
                    if (targetList.Contains(targetGo))
                    {
                        //Rotate the Turret
                        aimVectorY = targetPoint.position - turret_RotY.position ;
			            rotateY = Quaternion.LookRotation (aimVectorY , turret_RotY.up) ;
			            turret_RotY.rotation = Quaternion.RotateTowards ( turret_RotY.rotation , rotateY , turretRotSpeed * Time.deltaTime) ;
			            turret_RotY.localEulerAngles = new Vector3(0, turret_RotY.localEulerAngles.y, 0);

			            aimVectorX = targetPoint.position - turret_RotX.position ;
			            rotateX = Quaternion.LookRotation (aimVectorX , turret_RotX.up) ;
			            turret_RotX.rotation = Quaternion.RotateTowards ( turret_RotX.rotation , rotateX , turretRotSpeed * Time.deltaTime) ;
			            turret_RotX.localEulerAngles = new Vector3(turret_RotX.localEulerAngles.x, 0, 0);

                        //Check the Onsight
                        if (Physics.Raycast (shootingPoint.position, shootingPoint.forward, out hit, sphereDetect.radius, enemyLayer))
                        {
                            switch (team)
                            {
                                case 1 : if (hit.collider.gameObject.CompareTag ("Team02"))
                                         {
                                            //Is it the same cached target
                                            if (hit.collider.gameObject != targetGo)
                                            {
                                                targetGo = hit.collider.gameObject ;

                                                //DEBUG
                                                if (!targetGo.transform.Find("TargetPoint")) { print(targetGo + "Does not have a TargetPoint"); }

                                                targetPoint = targetGo.transform.Find("TargetPoint") ;
                                                targetLife = targetGo.GetComponent<Life>() ;        
                                            }
                                            onSight = true ;
                                            targetCount = 0 ;  
                                         }
                                         else
                                         {
                                            if (onSight) { onSight = false ; } 
                                            targetCount ++ ;
                                            if (targetCount > maxTargetCount)
                                            {
                                                targetCount = 0 ;
                                                targetGo = null ;
                                                targetLife = null ;
                                                targetPoint = null ;  
                                            }
                                         }  

                                break ;

                                case 2 : if (hit.collider.gameObject.CompareTag ("Team01"))
                                         {
                                            //Is it the same cached target
                                            if (hit.collider.gameObject != targetGo)
                                            {
                                                targetGo = hit.collider.gameObject ;
                                                targetPoint = targetGo.transform.Find("TargetPoint") ;
                                                targetLife = targetGo.GetComponent<Life>() ;        
                                            }
                                            onSight = true ;
                                            targetCount = 0 ;   
                                         }
                                         else
                                         {
                                            if (onSight) { onSight = false ; } 
                                            targetCount ++ ;
                                            if (targetCount > maxTargetCount)
                                            {
                                                targetCount = 0 ;
                                                targetGo = null ;
                                                targetLife = null ;
                                                targetPoint = null ;  
                                            }
                                         }

                                break ;
                            }
                        }
                        else
                        {
                            if (onSight) { onSight = false ; } 
                            targetCount ++ ;
                            if (targetCount > maxTargetCount)
                            {
                                targetCount = 0 ;
                                targetGo = null ;
                                targetLife = null ;
                                targetPoint = null ;  
                            }
                        }

                    }
                    else
                    {
                        if (onSight) { onSight = false ; }
                        targetCount = 0 ;
                        targetGo = null ;
                        targetLife = null ;
                        targetPoint = null ;    
                    }
                } 
            }
            //If we dont have a target
            else
            {
                //Turret Back Move
                turret_RotY.rotation = Quaternion.RotateTowards ( turret_RotY.rotation , transform.rotation , backSpeed * Time.deltaTime) ;
			    turret_RotY.localEulerAngles = new Vector3(0, turret_RotY.localEulerAngles.y, 0);
			    turret_RotX.rotation = Quaternion.RotateTowards ( turret_RotX.rotation , transform.rotation , backSpeed * Time.deltaTime) ;
			    turret_RotX.localEulerAngles = new Vector3(turret_RotX.localEulerAngles.x, 0, 0);

                if (onSight) { onSight = false ; }
                targetCount = 0 ;
                //Check if the list is populated
                if (targetList.Count > 0)
                {
                    //Raycast to one in the target List
                    for (int i=0; i < targetList.Count ; i++)
		            {
                        dest = targetList[i].transform.Find("TargetPoint").position - shootingPoint.position ;    
                        if (Physics.Raycast (shootingPoint.position, dest, out hit, sphereDetect.radius, enemyLayer))
                        {
                            switch (team)
                            {
                                case 1 : if (hit.collider.gameObject.CompareTag ("Team02"))
                                         {
                                            targetGo = hit.collider.gameObject ;
                                            targetPoint = targetGo.transform.Find("TargetPoint") ;
                                            targetLife = targetGo.GetComponent<Life>() ;   
                                         }  

                                break ;

                                case 2 : if (hit.collider.gameObject.CompareTag ("Team01"))
                                         {
                                            targetGo = hit.collider.gameObject ;
                                            targetPoint = targetGo.transform.Find("TargetPoint") ;
                                            targetLife = targetGo.GetComponent<Life>() ;   
                                         } 

                                break ;
                            }
                            break ; 
                        }       
                    }
                }          
            }
            
            yield return 0 ;
        }
    }
    void OnTriggerEnter(Collider enemy)
    {
        targetList.Add(enemy.gameObject);   
    }
    void OnTriggerExit (Collider enemy)
	{
		targetList.Remove(enemy.gameObject) ;
	}
//------------------------------------------------------------------------------------------------------------------------------//
//															END																	//
//------------------------------------------------------------------------------------------------------------------------------//
} 
