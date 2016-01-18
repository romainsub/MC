//------------------------------------------------------------------------------------------------------------------------------//
//													    Item                  							                        //
//------------------------------------------------------------------------------------------------------------------------------//

//------------------------------------------------------------------------------------------------------------------------------//
//														INIT																	//
//------------------------------------------------------------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking ;
using HighlightingSystem;

public class Item : NetworkBehaviour
{
//------------------------------------------------------------------------------------------------------------------------------//
//														VARIABLES																//
//------------------------------------------------------------------------------------------------------------------------------//
    //PUBLIC
    public      LevelController                         levelController                             = null                      ;
    public      LevelHolder                             levelHolder                                 = null                      ;
    public      int                                     itemID                                      = 0                         ;
    public      int                                     team                                        = 1                         ;
    public      float                                   explodeTimer                                = 5.0f                      ;
    public      float                                   despawnTimer                                = 3.0f                      ;
    public      Life                                    life                                        = null                      ;
    public      Collider                                collide                                     = null                      ;
    public      Spawn                                   spawnCpn                                    = null                      ;
    public      Explode                                 explodeCpn                                  = null                      ;
    public      Transform                               targetPoint                                 = null                      ;
    public      bool                                    pauseMove                                   = false                     ;
    public      bool                                    highLight                                   = false                     ;
    private     bool                                    curHighLight                                = false                     ;
    public      List<Highlighter>                       highLightList                               = new List<Highlighter>()   ;
    public      Transform                               myTargetPoint                               = null                      ;

    //ITEM SPECIFIC
    public      LiveLink                                liveLink                                    = null                      ;
    public      LiveCannon                              liveCannon                                  = null                      ;
    public      LiveLaser                               liveLaser                                   = null                      ;
    public      LiveBeam                                liveBeam                                    = null                      ;
    public      LiveRepair                              liveRepair                                  = null                      ;
    public      LiveShield                              liveShield                                  = null                      ;

    public      LiveMagTank                             liveMagTank                                 = null                      ;
    public      LiveDrone                               liveDrone                                   = null                      ;
    public      LiveMagShield                           liveMagShield                               = null                      ;
    public      LiveSwarmer                             liveSwarmer                                 = null                      ;
    public      LiveSpiderBot                           liveSpiderBot                               = null                      ;

    public      LiveConverter                           liveConverter                               = null                      ;

    public      Transform                               swarmerTarget                               = null                      ;
    public      Transform                               myTransform                                 = null                      ;
    public      float                                   swarmerRotSpeed                             = 0.0f                      ;

    public     Section                                  section                                     = null                      ;
    public     int                                      sectionLayer                                = 0                         ;

    //MOVER
    [SyncVar]
    public      int                                     spawnID                                     = 0                         ;
    public      Mover                                   mover                                       = null                      ;
    
    //PRIVATE
    private     int                                     state                                       = 0                         ;
    private     int                                     hpMax                                       = 0                         ;
    [SyncVar]
    public      int                                     hpCurrent                                   = 0                         ;

    //BuildCol
    public      Renderer                                buildColRenderer                            = null                      ;
    public      bool                                    ray                                         = false                     ;

//------------------------------------------------------------------------------------------------------------------------------//
//													VOID USER DEFINED															//
//------------------------------------------------------------------------------------------------------------------------------//
    void ChangeState ()
    {
        switch (state)
        {
            //FROM INACTIVE TO SPAWNING
            case 0 :    curHighLight = false ;
                        highLight = false ;
                        if (life.killed) { life.killed = false ; }
                        StartCoroutine ("Spawning") ;
                        if (buildColRenderer.enabled) { buildColRenderer.enabled = false ; }
                        collide.enabled = true ;
                        break ;

            //FROM SPAWNING TO ACTIVE
            case 1 :    if (life.killed) { life.killed = false ; }
                        StartCoroutine("Living") ;
                        collide.enabled = true ;
                        break ;

            //FROM ACTIVE TO DEATH
            case 2 :    if (!life.killed) { life.killed = true ; }
                        StartCoroutine("Exploding") ;
                        collide.enabled = false ;
                         if (buildColRenderer.enabled) { buildColRenderer.enabled = false ; }
                        break ;

            //FROM SPAWNING TO DEATH
            case 3 :    if (!life.killed) { life.killed = true ; }
                        StartCoroutine("Despawning") ;
                        collide.enabled = false ;
                        if (buildColRenderer.enabled) { buildColRenderer.enabled = false ; }
                        break ;
        }
    }
    IEnumerator HighLight ()
    {
        while (true)
        {
            if (!highLight && curHighLight)
            {
                curHighLight = highLight ;
                for (int i=0; i < highLightList.Count ; i++)
                {
                    highLightList[i].ConstantSwitch();       
                }   
            }
            if (highLight && !curHighLight)
            {
                curHighLight = highLight ;
                for (int i=0; i < highLightList.Count ; i++)
                {
                    highLightList[i].ConstantSwitch();         
                }
            }
            yield return 0 ;
        }
    }
    IEnumerator Spawning()
    {
        float buildTime = 0.0f ;
        hpMax = GameSettings.spawnHP ;
        hpCurrent = hpMax ; 

        switch (itemID)
        {
            case 100 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Link_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Link_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 101 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Cannon_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Cannon_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 102 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Laser_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Laser_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 103 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Beam_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Beam_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 104 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Repair_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Repair_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 105 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Shield_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Shield_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 150 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Cannon_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Cannon_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 151 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Laser_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Laser_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 152 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Shield_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Shield_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 153 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Swarmer_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Swarmer_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 154 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Beam_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Beam_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;

            case 200 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Link_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Link_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 201 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Cannon_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Cannon_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 202 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Laser_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Laser_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 203 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Beam_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Beam_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 204 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Repair_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Repair_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 205 :  buildTime = Time.timeSinceLevelLoad + GameSettings.turret_Shield_BuildTime ; spawnCpn.spawnTimer = GameSettings.turret_Shield_BuildTime ; StartCoroutine("RayCastSectionTurret"); spawnCpn.StartVFXSpawn() ; break ;
            case 250 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Cannon_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Cannon_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 251 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Laser_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Laser_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 252 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Shield_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Shield_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 253 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Swarmer_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Swarmer_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;
            case 254 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Beam_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Beam_BuildTime ; spawnCpn.StartVFXSpawn() ; break ;

            case 301 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Converter_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Converter_BuildTime ; StartCoroutine("RayCastSectionConverter"); spawnCpn.StartVFXSpawn() ; break ;
            case 302 :  buildTime = Time.timeSinceLevelLoad + GameSettings.unit_Converter_BuildTime ; spawnCpn.spawnTimer = GameSettings.unit_Converter_BuildTime ; StartCoroutine("RayCastSectionConverter"); spawnCpn.StartVFXSpawn() ; break ;
        }

        spawnCpn.StartCoroutine("Spawner") ;

        while (true)
        {
            if (life.damage > 0) 
            { 
                hpCurrent -= life.damage ; 
                life.damage = 0 ; 
            }
            if (hpCurrent <= 0) 
            {
                state = 3 ;
                break ;
            }
            if (Time.timeSinceLevelLoad > buildTime)
            {
                state = 1 ;
                break ;   
            }
            yield return 0 ;
        }

        ChangeState () ;
    }
    IEnumerator Exploding ()
    {
        explodeCpn.StartCoroutine("Exploder") ;
        float stopTime = Time.timeSinceLevelLoad + explodeTimer ;
        while (true)
        {
            if (Time.timeSinceLevelLoad > stopTime) { break ; }
            yield return 0 ;     
        }
        Deactivate () ;
    }
    IEnumerator Despawning ()
    {
        spawnCpn.StopVFX();
        switch (itemID)
        {
            case 301 :  if (section != null) { section.busy = false ; } ; break ; 
            case 302 :  if (section != null) { section.busy = false ; } ; break ;
        }
        float stopTime = Time.timeSinceLevelLoad + despawnTimer ;
        while (true)
        {
            if (Time.timeSinceLevelLoad > stopTime) { break ; }
            yield return 0 ;     
        }
        Deactivate () ;
    }
    IEnumerator Living ()
    {
        switch (itemID)
        {
            case 100 :  hpMax = GameSettings.turret_Link_HpMax ;
                        liveLink.Init() ;
                        levelController.t01_Energy_Conso += GameSettings.turret_Link_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 101 :  hpMax = GameSettings.turret_Cannon_HpMax ; 
                        liveCannon.Init() ;
                        levelController.t01_Energy_Conso += GameSettings.turret_Cannon_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 102 :  hpMax = GameSettings.turret_Laser_HpMax ;
                        liveLaser.Init() ;
                        levelController.t01_Energy_Conso += GameSettings.turret_Laser_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 103 :  hpMax = GameSettings.turret_Beam_HpMax ;
                        liveBeam.Init() ;
                        levelController.t01_Energy_Conso += GameSettings.turret_Beam_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 104 :  hpMax = GameSettings.turret_Repair_HpMax ;
                        liveRepair.Init() ;
                        levelController.t01_Energy_Conso += GameSettings.turret_Repair_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 105 :  hpMax = GameSettings.turret_Shield_HpMax ;
                        liveShield.Init() ;
                        levelController.t01_Energy_Conso += GameSettings.turret_Shield_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 150 :  hpMax = GameSettings.unit_Cannon_HpMax ;
                        liveMagTank.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Cannon_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Ground_T01_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Ground_T01_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Ground_T01_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Ground_T01_Down) ; }
                        break ;
            case 151 :  hpMax = GameSettings.unit_Laser_HpMax ;
                        liveDrone.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Laser_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Fly_T01_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Fly_T01_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Fly_T01_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Fly_T01_Down) ; }
                        break ;
            case 152 :  hpMax = GameSettings.unit_Shield_HpMax ;
                        liveMagShield.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Shield_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Fly_T01_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Fly_T01_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Fly_T01_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Fly_T01_Down) ; }
                        break ;
            case 153 :  hpMax = GameSettings.unit_Swarmer_HpMax ;
                        liveSwarmer.Init() ;
                        StartCoroutine ("SwarmerTarget") ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Swarmer_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Swarm_T01_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Swarm_T01_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Swarm_T01_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Swarm_T01_Down) ; }
                        break ;
            case 154 :  hpMax = GameSettings.unit_Beam_HpMax ;
                        liveSpiderBot.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Beam_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Ground_T01_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Ground_T01_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Ground_T01_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Ground_T01_Down) ; }
                        break ;
            case 301 :  hpMax = GameSettings.unit_Converter_HpMax ;
                        liveConverter.Init() ;
                        StartCoroutine("ConverterStart") ;
                        break ;
            case 200 :  hpMax = GameSettings.turret_Link_HpMax ;
                        liveLink.Init() ;
                        levelController.t02_Energy_Conso += GameSettings.turret_Link_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 201 :  hpMax = GameSettings.turret_Cannon_HpMax ;
                        liveCannon.Init() ;
                        levelController.t02_Energy_Conso += GameSettings.turret_Cannon_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 202 :  hpMax = GameSettings.turret_Laser_HpMax ;
                        liveLaser.Init() ;
                        levelController.t02_Energy_Conso += GameSettings.turret_Laser_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 203 :  hpMax = GameSettings.turret_Beam_HpMax ;
                        liveBeam.Init() ;
                        levelController.t02_Energy_Conso += GameSettings.turret_Beam_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 204 :  hpMax = GameSettings.turret_Repair_HpMax ;
                        liveRepair.Init() ;
                        levelController.t02_Energy_Conso += GameSettings.turret_Repair_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 205 :  hpMax = GameSettings.turret_Shield_HpMax ;
                        liveShield.Init() ;
                        levelController.t02_Energy_Conso += GameSettings.turret_Shield_EnCon ;
                        StartCoroutine("HighLight") ;
                        break ;
            case 250 :  hpMax = GameSettings.unit_Cannon_HpMax ;
                        liveMagTank.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Cannon_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Ground_T02_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Ground_T02_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Ground_T02_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Ground_T02_Down) ; }
                        break ;
            case 251 :  hpMax = GameSettings.unit_Laser_HpMax ;
                        liveDrone.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Laser_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Fly_T02_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Fly_T02_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Fly_T02_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Fly_T02_Down) ; }
                        break ;
            case 252 :  hpMax = GameSettings.unit_Shield_HpMax ;
                        liveMagShield.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Shield_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Fly_T02_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Fly_T02_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Fly_T02_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Fly_T02_Down) ; }
                        break ;
            case 253 :  hpMax = GameSettings.unit_Swarmer_HpMax ;
                        liveSwarmer.Init() ;
                        StartCoroutine ("SwarmerTarget") ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Swarmer_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Swarm_T02_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Swarm_T02_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Swarm_T02_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Swarm_T02_Down) ; }
                        break ;
            case 254 :  hpMax = GameSettings.unit_Beam_HpMax ;
                        liveSpiderBot.Init() ;
                        if (mover.speed == 0.0f) { mover.speed = GameSettings.unit_Beam_MoveSpeed ; }
                        if (spawnID == 1) { mover.SetPath(levelHolder.path_Ground_T02_Front) ; }
                        if (spawnID == 2) { mover.SetPath(levelHolder.path_Ground_T02_Back) ; }
                        if (spawnID == 3) { mover.SetPath(levelHolder.path_Ground_T02_Top) ; }
                        if (spawnID == 4) { mover.SetPath(levelHolder.path_Ground_T02_Down) ; }
                        break ;
            case 302 :  hpMax = GameSettings.unit_Converter_HpMax ;
                        StartCoroutine("ConverterStart") ;
                        liveConverter.Init() ;
                        break ;
        }

        life.damage = 0 ;
        life.repair = 0 ;
        hpCurrent = hpMax ;

        while (true)
        {
            if (life.damage > 0) 
            { 
                hpCurrent -= life.damage ; 
                life.damage = 0 ; 
            }
            if (hpCurrent <= 0) 
            {
                //Make Sure Client Gets The Message ...
                if (isServer)
                {
                    RpcKillOnClient() ;
                }
                state = 2 ;
                break ;
            }
            else
            {
                if (life.repair > 0 && hpCurrent < hpMax)
                {
                    hpCurrent += life.repair ;
                    life.repair = 0;
                }
                else
                {
                    life.repair = 0;
                }
                hpCurrent = Mathf.Clamp(hpCurrent, 0, hpMax);
            }
            yield return 0 ;
        }

        switch (itemID)
        {
            case 100 :  liveLink.Stop() ; levelController.t01_Energy_Conso -= GameSettings.turret_Link_EnCon ; break ;
            case 101 :  liveCannon.Stop() ; levelController.t01_Energy_Conso -= GameSettings.turret_Cannon_EnCon ; break ;
            case 102 :  liveLaser.Stop() ; levelController.t01_Energy_Conso -= GameSettings.turret_Laser_EnCon ; break ;
            case 103 :  liveBeam.Stop() ; levelController.t01_Energy_Conso -= GameSettings.turret_Beam_EnCon ; break ;
            case 104 :  liveRepair.Stop() ; levelController.t01_Energy_Conso -= GameSettings.turret_Repair_EnCon ; break ;
            case 105 :  liveShield.Stop() ; levelController.t01_Energy_Conso -= GameSettings.turret_Shield_EnCon ; break ;
            case 150 :  liveMagTank.Stop() ; mover.Stop() ; break ;
            case 151 :  liveDrone.Stop() ; mover.Stop() ; break ;
            case 152 :  liveMagShield.Stop() ; mover.Stop() ; break ;
            case 153 :  liveSwarmer.Stop() ; mover.Stop() ; StopCoroutine ("SwarmerTarget") ; swarmerTarget = null ; break ;
            case 154 :  liveSpiderBot.Stop() ; mover.Stop() ; break ;
            case 301 :  liveConverter.Stop() ; if (section != null) { section.StartCoroutine("SwitchTeam", 0) ; section.busy = false ; } ; break ; 

            case 200 :  liveLink.Stop() ; levelController.t02_Energy_Conso -= GameSettings.turret_Link_EnCon ; break ;
            case 201 :  liveCannon.Stop() ; levelController.t02_Energy_Conso -= GameSettings.turret_Cannon_EnCon ; break ;
            case 202 :  liveLaser.Stop() ; levelController.t02_Energy_Conso -= GameSettings.turret_Laser_EnCon ; break ;
            case 203 :  liveBeam.Stop() ; levelController.t02_Energy_Conso -= GameSettings.turret_Beam_EnCon ; break ;
            case 204 :  liveRepair.Stop() ; levelController.t02_Energy_Conso -= GameSettings.turret_Repair_EnCon ; break ;
            case 205 :  liveShield.Stop() ; levelController.t02_Energy_Conso -= GameSettings.turret_Shield_EnCon ; break ;
            case 250 :  liveMagTank.Stop() ; mover.Stop() ; break ;
            case 251 :  liveDrone.Stop() ; mover.Stop() ; break ;
            case 252 :  liveMagShield.Stop() ; mover.Stop() ; break ;
            case 253 :  liveSwarmer.Stop() ; mover.Stop() ; StopCoroutine ("SwarmerTarget") ; swarmerTarget = null ; break ;
            case 254 :  liveSpiderBot.Stop() ; mover.Stop() ; break ;
            case 302 :  liveConverter.Stop() ; if (section != null) { section.StartCoroutine("SwitchTeam", 0) ; section.busy = false ; } ; break ; 
        }

        if (section != null) { section.itemList.Remove(GetComponent<Item>()); section = null ;}
        spawnID = 0 ;
        ChangeState () ;
    }
    IEnumerator SwarmerTarget ()
    {
        Vector3 aimVector ;
		Quaternion rotate ;
        RaycastHit hit ;
        int enemyLayer = 0 ;
        bool moveOnPause  = false ;
        int currentPoint = 0 ;

        if (swarmerRotSpeed == 0.0f) { swarmerRotSpeed = GameSettings.unit_Swarmer_RotSpeed ; }

        switch (team)
        {
            case 1  :   enemyLayer = 1 << 8 | 1 << 14 | 1 << 22 ; break ;
            case 2  :   enemyLayer = 1 << 8 | 1 << 13 | 1 << 21 ; break ;
        }

        while (true)
        {

            if (swarmerTarget != null)
            {
                if (!moveOnPause) { moveOnPause = true ; currentPoint = mover.currentPoint ; mover.Stop() ; }
                //Rotate To target
                aimVector = swarmerTarget.position - myTransform.position ;
			    rotate = Quaternion.LookRotation (aimVector , myTransform.up) ;
			    myTransform.rotation = Quaternion.RotateTowards ( myTransform.rotation , rotate , swarmerRotSpeed * Time.deltaTime) ;
                
                //Move To target
                myTransform.Translate (Vector3.forward * Time.deltaTime * mover.speed) ;

                //Raycast in Front
                if (Physics.Raycast (myTransform.position, myTransform.forward, out hit, 2, enemyLayer))
                {
                    switch (team)
                    {
                        case 1 : if (hit.collider.gameObject.CompareTag ("Team02"))
                                 {
                                    hit.collider.GetComponent<Life>().damage += GameSettings.unit_Swarmer_Damage ;
                                 }
                        break ;
                        case 2 : if (hit.collider.gameObject.CompareTag ("Team01"))
                                 {
                                    hit.collider.GetComponent<Life>().damage += GameSettings.unit_Swarmer_Damage ;
                                 }
                        break ;
                    }
                    myTransform.position = hit.point ;
                    life.damage += 10000 ;
                    break ; 
                }
            }
            else
            {
                if (moveOnPause) { moveOnPause = false ; mover.startPoint = currentPoint ; mover.moveToPath = true ; mover.StartMove () ; }
            }
            yield return 0 ;
        }
    }
    IEnumerator ConverterStart ()
    {
        if (section != null)
        {
            section.StartCoroutine("SwitchTeam", team) ;
        }
        else
        {
            life.damage += 1000 ;    
        }
        yield return 0 ;
    }
    IEnumerator RayCastSectionTurret()
    {
        RaycastHit hit ;
        switch (team)
        {
            case 1  :   sectionLayer = 1 << 23 ; break ;
            case 2  :   sectionLayer = 1 << 24 ; break ;
        }

        while (true)
        {
            if (Physics.Raycast (myTargetPoint.position, myTargetPoint.forward, out hit, 30, sectionLayer))
            {
                if (hit.collider.GetComponent<Section>())
                {
                    section = hit.collider.GetComponent<Section>() ;
                    section.itemList.Add(GetComponent<Item>());
                    break ;
                }
            }
            yield return new WaitForSeconds (0.1f) ;
        }
    }
    IEnumerator RayCastSectionConverter()
    {
        RaycastHit hit ;
        switch (team)
        {
            case 1  :   sectionLayer = 1 << 9 | 1 << 14 | 1 << 24 ; break ;
            case 2  :   sectionLayer = 1 << 9 | 1 << 13 | 1 << 23 ; break ;
        }

        while (true)
        {
            if (Physics.Raycast (myTargetPoint.position, myTargetPoint.forward, out hit, 30, sectionLayer))
            {
                if (hit.collider.GetComponent<Section>())
                {
                    section = hit.collider.GetComponent<Section>() ;
                    section.itemList.Add(GetComponent<Item>());
                    break ;
                }
            }
            yield return new WaitForSeconds (0.1f) ;
        }
    }
    void Deactivate ()
    {
        gameObject.SetActive(false) ;
    }
    void OnEnable ()
    {
        if (levelController == null) { levelController = GameObject.FindWithTag("LevelController").GetComponent<LevelController>() ; }
        if (levelHolder == null) { levelHolder = GameObject.FindWithTag("LevelHolder").GetComponent<LevelHolder>() ; }

        if (team == 1) {  transform.parent = levelHolder.itemT01_Parent ; }
        if (team == 2) {  transform.parent = levelHolder.itemT02_Parent ; }
        state = 0 ;
        ChangeState () ;
    }
    void OnDisable ()
    {
        if (life.killed) { life.killed = false ; }
        levelController.list_Items.Add(gameObject) ;
    }
    [ClientRpc] void RpcKillOnClient ()
    {
        hpCurrent = 0 ;   
    }
//------------------------------------------------------------------------------------------------------------------------------//
//															END																	//
//------------------------------------------------------------------------------------------------------------------------------//
} 
