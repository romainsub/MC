//------------------------------------------------------------------------------------------------------------------------------//
//													    Manager Game                  							                //
//------------------------------------------------------------------------------------------------------------------------------//
/*
 * SpawnIDs:
 *      1 : Front
 *      2 : Back
 *      3 : Top
 *      4 : Down
*/ 
//------------------------------------------------------------------------------------------------------------------------------//
//														INIT																	//
//------------------------------------------------------------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking ;

public class LevelController : NetworkBehaviour
{
//------------------------------------------------------------------------------------------------------------------------------//
//														VARIABLES																//
//------------------------------------------------------------------------------------------------------------------------------//
            public LevelHolder                      levelHolder                                 = null                          ;
            public GameController                   gameController                              = null                          ;

            //ITEM LIST
            public List<GameObject>                 list_Items				                    = new List<GameObject>()        ;
            public List<TurretQueue>                list_TurretQueue                            = new List<TurretQueue>()       ;
            public List<UnitQueueT01>               list_UnitQueueT01                           = new List<UnitQueueT01>()      ;
            public List<UnitQueueT02>               list_UnitQueueT02                           = new List<UnitQueueT02>()      ;

            //LINK INTs
[SyncVar]   public int                              counter_T01_Link				            = 0                             ;
[SyncVar]   public int                              counter_T02_Link				            = 0                             ;

            //ENERGY
[SyncVar]   public int                              t01_Energy_Current                           = 0                             ;
[SyncVar]   public int                              t01_Energy_Conso                             = 0                             ;
[SyncVar]   public int                              t01_Energy_Max                               = 0                             ;
[SyncVar]   public int                              t01_Energy_Gen                               = 0                             ;
[SyncVar]   public int                              t01_Sections                                 = 0                             ;
[SyncVar]   public int                              t01_Energy_Diff                              = 0                             ;
[SyncVar]   public bool                             t01_Blackout                                 = false                         ;

[SyncVar]   public int                              t02_Energy_Current                           = 0                             ;
[SyncVar]   public int                              t02_Energy_Conso                             = 0                             ;
[SyncVar]   public int                              t02_Energy_Max                               = 0                             ;
[SyncVar]   public int                              t02_Energy_Gen                               = 0                             ;
[SyncVar]   public int                              t02_Sections                                 = 0                             ;
[SyncVar]   public int                              t02_Energy_Diff                              = 0                             ;
[SyncVar]   public bool                             t02_Blackout                                 = false                         ;
//------------------------------------------------------------------------------------------------------------------------------//
//													VOID USER DEFINED															//
//------------------------------------------------------------------------------------------------------------------------------//
    void Start ()
    {
        levelHolder = GameObject.FindWithTag("LevelHolder").GetComponent<LevelHolder>() ;
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>() ;
        if (gameController.playerT01Cpn != null)
        {
            gameController.playerT01Cpn.levelController = GetComponent<LevelController>() ;
            gameController.playerT01Cpn.levelHolder = levelHolder ;
        }
        if (gameController.playerT02Cpn != null)
        {
            gameController.playerT02Cpn.levelController = GetComponent<LevelController>() ;
            gameController.playerT02Cpn.levelHolder = levelHolder ;
        }
        
        gameController.levelController = GetComponent<LevelController>() ;

        for (int i = 0 ; i < levelHolder.sectionList.Count ; i ++)
        {
            levelHolder.sectionList[i].GetComponent<Section>().levelController = GetComponent<LevelController>() ;
        }

        if (isServer)
        {
            levelHolder.coreT01.StartCoroutine("Init") ;
            levelHolder.coreT02.StartCoroutine("Init") ;

            //UNTIL RPC BUG FIX
            RpcStartCore() ;

            StartCoroutine("EnergyCalc") ;
            StartCoroutine("BuildTurret") ;
            StartCoroutine("BuildUnitT01") ;
            StartCoroutine("BuildUnitT02") ;
            StartCoroutine ("CoreLife") ;
            if (levelHolder.isAILevel) { StartCoroutine ("AI") ; }
        }
        if (gameController.playerT01Cpn != null) { gameController.playerT01Cpn.StartPlayer () ; }
        if (gameController.playerT02Cpn != null) { gameController.playerT02Cpn.StartPlayer () ; }
        
    }
    IEnumerator EnergyCalc ()
    { 
        while (true)
        {
            //TEAM 01
            t01_Energy_Max = GameSettings.t01_Min_Energy_Max + (GameSettings.turret_Link_EnProv * counter_T01_Link) ;
            t01_Energy_Gen = GameSettings.t01_Min_Energy_Gen + (GameSettings.sectionEnergy * t01_Sections)  ;
            t01_Energy_Diff = t01_Energy_Gen - t01_Energy_Conso ;
            if (t01_Energy_Current + t01_Energy_Diff > 0) 
            {
                if (t01_Energy_Current + t01_Energy_Diff < t01_Energy_Max )
                {
                    t01_Energy_Current += t01_Energy_Diff ;
                } 
                else
                {
                    t01_Energy_Current = t01_Energy_Max ;
                }
            }
            else
            {
                t01_Energy_Current = 0 ;    
            }
            t01_Energy_Current = Mathf.Clamp(t01_Energy_Current, 0 , t01_Energy_Max) ;
            if (t01_Energy_Current == 0 && t01_Energy_Diff < 0 ) { t01_Blackout = true ; } else { t01_Blackout = false ; }

            //TEAM 
            if (!levelHolder.isAILevel)
            {
                t02_Energy_Max = GameSettings.t02_Min_Energy_Max + (GameSettings.turret_Link_EnProv * counter_T02_Link) ;
                t02_Energy_Gen = GameSettings.t02_Min_Energy_Gen + (GameSettings.sectionEnergy * t02_Sections)  ;
                t02_Energy_Diff = t02_Energy_Gen - t02_Energy_Conso ;
                if (t02_Energy_Current + t02_Energy_Diff > 0) 
                {
                    if (t02_Energy_Current + t02_Energy_Diff < t02_Energy_Max )
                    {
                        t02_Energy_Current += t02_Energy_Diff ;
                    } 
                    else
                    {
                        t02_Energy_Current = t02_Energy_Max ;
                    }
                }
                else
                {
                    t02_Energy_Current = 0 ;    
                }
                t02_Energy_Current = Mathf.Clamp(t02_Energy_Current, 0 , t02_Energy_Max) ;
                if (t02_Energy_Current == 0 && t02_Energy_Diff < 0 ) { t02_Blackout = true ; } else { t02_Blackout = false ; }
            }
            else
            {
                t02_Energy_Max = 10000 ;
                t02_Energy_Gen = 10000 ;
                t02_Energy_Current = 10000 ;
                t02_Energy_Conso = 0 ;
            }


            //CALC EVERY SECONDS
            yield return new WaitForSeconds(1) ;
        }
    }
    IEnumerator BuildTurret ()
    {
        GameObject itemGO = null ;

        while (true)
        {
            if (list_TurretQueue.Count > 0)
            {
                for (int i=0; i < list_Items.Count ; i++)
                {
                    if (list_Items[i].GetComponent<Item>().itemID == list_TurretQueue[0].itemID)
                    {
                        itemGO = list_Items[i] ;
                        list_Items.Remove(itemGO) ;
                        RpcReactiveTurret(itemGO, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) ;
                        break ;
                    }
                }

                if (itemGO == null)
                {
                    switch (list_TurretQueue[0].itemID)
                    {
                        case 100 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Link_T01, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 101 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Cannon_T01, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 102 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Laser_T01, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 103 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Beam_T01, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 104 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Repair_T01, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 105 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Shield_T01, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;

                        case 200 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Link_T02, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 201 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Cannon_T02, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 202 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Laser_T02, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 203 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Beam_T02, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 204 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Repair_T02, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                        case 205 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Shield_T02, list_TurretQueue[0].itemPos, Quaternion.Euler(list_TurretQueue[0].itemRot)) as GameObject ; break ;
                    }
                    NetworkServer.Spawn(itemGO) ;

                    itemGO = null ;
                    list_TurretQueue.RemoveAt(0) ;
                }
                else
                {
                    itemGO = null ;
                    list_TurretQueue.RemoveAt(0) ;       
                }
            }
            yield return 0 ;
        }
    }
    IEnumerator BuildUnitT01 ()
    {
        GameObject itemGO = null ;

        while (true)
        {
            if (list_UnitQueueT01.Count > 0)
            {
                for (int i=0; i < list_Items.Count ; i++)
                {
                    if (list_Items[i].GetComponent<Item>().itemID == list_UnitQueueT01[0].itemID)
                    {
                        itemGO = list_Items[i] ;
                        list_Items.Remove(itemGO) ;
                        break ;
                    }
                }

                while (true)
                { 
                    if (levelHolder.spawn_T01_Front_Cpn.ready)
                    {
                        levelHolder.spawn_T01_Front_Cpn.ready = false ;
                        levelHolder.spawn_T01_Front_Cpn.timer = list_UnitQueueT01[0].buildTime ;
                        if (itemGO == null)
                        {
                            switch (list_UnitQueueT01[0].itemID)
                            { 
                                case 150 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T01) as GameObject ; break ;
                                case 151 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T01) as GameObject ;  break ;
                                case 152 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T01) as GameObject ;  break ;
                                case 153 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T01) as GameObject ;  break ;
                                case 154 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T01) as GameObject ; break ;
                            }
                            itemGO.transform.position = levelHolder.spawn_T01_Front.transform.position ;
                            itemGO.transform.rotation = levelHolder.spawn_T01_Front.transform.rotation ;
                            itemGO.GetComponent<Item>().spawnID = 1 ;
                            NetworkServer.Spawn(itemGO) ;
                            list_UnitQueueT01.RemoveAt(0) ;
                            itemGO = null ;
                        }
                        else
                        {
                            RpcReactiveUnit (itemGO, levelHolder.spawn_T01_Front.transform.position, levelHolder.spawn_T01_Front.transform.rotation , 1) ;
                            list_UnitQueueT01.RemoveAt(0) ; 

                            itemGO = null ;
                        }
                        break ;
                    }
                    else
                    {
                        if (levelHolder.spawn_T01_Back_Cpn.ready)
                        {
                            levelHolder.spawn_T01_Back_Cpn.ready = false ;
                            levelHolder.spawn_T01_Back_Cpn.timer = list_UnitQueueT01[0].buildTime ;
                            if (itemGO == null)
                            {
                                switch (list_UnitQueueT01[0].itemID)
                                { 
                                    case 150 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T01) as GameObject ; break ;
                                    case 151 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T01) as GameObject ; break ;
                                    case 152 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T01) as GameObject ; break ;
                                    case 153 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T01) as GameObject ; break ;
                                    case 154 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T01) as GameObject ; break ;
                                }
                                itemGO.transform.position = levelHolder.spawn_T01_Back.transform.position ;
                                itemGO.transform.rotation = levelHolder.spawn_T01_Back.transform.rotation ;
                                itemGO.GetComponent<Item>().spawnID = 2 ;
                                NetworkServer.Spawn(itemGO) ;
                                list_UnitQueueT01.RemoveAt(0) ;
                                itemGO = null ;
                            }
                            else
                            {
                                RpcReactiveUnit (itemGO, levelHolder.spawn_T01_Back.transform.position, levelHolder.spawn_T01_Back.transform.rotation , 2) ;
                                list_UnitQueueT01.RemoveAt(0) ;

                                itemGO = null ;
                            }
                            break ;
                        }
                        else
                        {
                            if (levelHolder.spawn_T01_Top_Cpn.ready)
                            {
                                levelHolder.spawn_T01_Top_Cpn.ready = false ;
                                levelHolder.spawn_T01_Top_Cpn.timer = list_UnitQueueT01[0].buildTime ;
                                if (itemGO == null)
                                {
                                    switch (list_UnitQueueT01[0].itemID)
                                    { 
                                        case 150 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T01) as GameObject ; break ;
                                        case 151 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T01) as GameObject ; break ;
                                        case 152 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T01) as GameObject ; break ;
                                        case 153 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T01) as GameObject ; break ;
                                        case 154 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T01) as GameObject ; break ;
                                    }
                                    itemGO.transform.position = levelHolder.spawn_T01_Top.transform.position ;
                                    itemGO.transform.rotation = levelHolder.spawn_T01_Top.transform.rotation ;
                                    itemGO.GetComponent<Item>().spawnID = 3 ;
                                    NetworkServer.Spawn(itemGO) ;
                                    list_UnitQueueT01.RemoveAt(0) ;
                                    itemGO = null ;
                                }
                                else
                                {
                                    RpcReactiveUnit (itemGO, levelHolder.spawn_T01_Top.transform.position, levelHolder.spawn_T01_Top.transform.rotation , 3) ;
                                    list_UnitQueueT01.RemoveAt(0) ;

                                    itemGO = null ;
                                }
                                break ;
                            }
                            else
                            {
                                if (levelHolder.spawn_T01_Down_Cpn.ready)
                                {
                                    levelHolder.spawn_T01_Down_Cpn.ready = false ;
                                    levelHolder.spawn_T01_Down_Cpn.timer = list_UnitQueueT01[0].buildTime ;
                                    if (itemGO == null)
                                    {
                                        switch (list_UnitQueueT01[0].itemID)
                                        { 
                                            case 150 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T01) as GameObject ; break ;
                                            case 151 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T01) as GameObject ; break ;
                                            case 152 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T01) as GameObject ; break ;
                                            case 153 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T01) as GameObject ; break ;
                                            case 154 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T01) as GameObject ; break ;
                                        }
                                        itemGO.transform.position = levelHolder.spawn_T01_Down.transform.position ;
                                        itemGO.transform.rotation = levelHolder.spawn_T01_Down.transform.rotation ;
                                        itemGO.GetComponent<Item>().spawnID = 4 ;
                                        NetworkServer.Spawn(itemGO) ;
                                        list_UnitQueueT01.RemoveAt(0) ;
                                        itemGO = null ;
                                    }
                                    else
                                    {
                                        RpcReactiveUnit (itemGO, levelHolder.spawn_T01_Down.transform.position, levelHolder.spawn_T01_Down.transform.rotation , 4) ;
                                        list_UnitQueueT01.RemoveAt(0) ;

                                        itemGO = null ;
                                    }
                                    break ;
                                }    
                            }
                        }
                    }
                yield return 0 ;
                }
   
            }
            yield return 0 ;
        }
    }
    IEnumerator BuildUnitT02 ()
    {
        GameObject itemGO = null ;

        while (true)
        {
            if (list_UnitQueueT02.Count > 0)
            {
                for (int i=0; i < list_Items.Count ; i++)
                {
                    if (list_Items[i].GetComponent<Item>().itemID == list_UnitQueueT02[0].itemID)
                    {
                        itemGO = list_Items[i] ;
                        list_Items.Remove(itemGO) ;
                        break ;
                    }
                }

                while (true)
                { 
                    if (levelHolder.spawn_T02_Front_Cpn.ready)
                    {
                        levelHolder.spawn_T02_Front_Cpn.ready = false ;
                        levelHolder.spawn_T02_Front_Cpn.timer = list_UnitQueueT02[0].buildTime ;
                        if (itemGO == null)
                        {
                            switch (list_UnitQueueT02[0].itemID)
                            { 
                                case 250 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T02) as GameObject ; break ;
                                case 251 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T02) as GameObject ;  break ;
                                case 252 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T02) as GameObject ;  break ;
                                case 253 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T02) as GameObject ;  break ;
                                case 254 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T02) as GameObject ; break ;
                            }
                            itemGO.transform.position = levelHolder.spawn_T02_Front.transform.position ;
                            itemGO.transform.rotation = levelHolder.spawn_T02_Front.transform.rotation ;
                            itemGO.GetComponent<Item>().spawnID = 1 ;
                            NetworkServer.Spawn(itemGO) ;
                            list_UnitQueueT02.RemoveAt(0) ;
                            itemGO = null ;
                        }
                        else
                        {
                            RpcReactiveUnit (itemGO, levelHolder.spawn_T02_Front.transform.position, levelHolder.spawn_T02_Front.transform.rotation , 1) ;
                            list_UnitQueueT02.RemoveAt(0) ; 

                            itemGO = null ;
                        }
                        break ;
                    }
                    else
                    {
                        if (levelHolder.spawn_T02_Back_Cpn.ready)
                        {
                            levelHolder.spawn_T02_Back_Cpn.ready = false ;
                            levelHolder.spawn_T02_Back_Cpn.timer = list_UnitQueueT02[0].buildTime ;
                            if (itemGO == null)
                            {
                                switch (list_UnitQueueT02[0].itemID)
                                { 
                                    case 250 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T02) as GameObject ; break ;
                                    case 251 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T02) as GameObject ; break ;
                                    case 252 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T02) as GameObject ; break ;
                                    case 253 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T02) as GameObject ; break ;
                                    case 254 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T02) as GameObject ; break ;
                                }
                                itemGO.transform.position = levelHolder.spawn_T02_Back.transform.position ;
                                itemGO.transform.rotation = levelHolder.spawn_T02_Back.transform.rotation ;
                                itemGO.GetComponent<Item>().spawnID = 2 ;
                                NetworkServer.Spawn(itemGO) ;
                                list_UnitQueueT02.RemoveAt(0) ;
                                itemGO = null ;
                            }
                            else
                            {
                                RpcReactiveUnit (itemGO, levelHolder.spawn_T02_Back.transform.position, levelHolder.spawn_T02_Back.transform.rotation , 2) ;
                                list_UnitQueueT02.RemoveAt(0) ;

                                itemGO = null ;
                            }
                            break ;
                        }
                        else
                        {
                            if (levelHolder.spawn_T02_Top_Cpn.ready)
                            {
                                levelHolder.spawn_T02_Top_Cpn.ready = false ;
                                levelHolder.spawn_T02_Top_Cpn.timer = list_UnitQueueT02[0].buildTime ;
                                if (itemGO == null)
                                {
                                    switch (list_UnitQueueT02[0].itemID)
                                    { 
                                        case 250 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T02) as GameObject ; break ;
                                        case 251 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T02) as GameObject ; break ;
                                        case 252 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T02) as GameObject ; break ;
                                        case 253 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T02) as GameObject ; break ;
                                        case 254 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T02) as GameObject ; break ;
                                    }
                                    itemGO.transform.position = levelHolder.spawn_T02_Top.transform.position ;
                                    itemGO.transform.rotation = levelHolder.spawn_T02_Top.transform.rotation ;
                                    itemGO.GetComponent<Item>().spawnID = 3 ;
                                    NetworkServer.Spawn(itemGO) ;
                                    list_UnitQueueT02.RemoveAt(0) ;
                                    itemGO = null ;
                                }
                                else
                                {
                                    RpcReactiveUnit (itemGO, levelHolder.spawn_T02_Top.transform.position, levelHolder.spawn_T02_Top.transform.rotation , 3) ;
                                    list_UnitQueueT02.RemoveAt(0) ;

                                    itemGO = null ;
                                }
                                break ;
                            }
                            else
                            {
                                if (levelHolder.spawn_T02_Down_Cpn.ready)
                                {
                                    levelHolder.spawn_T02_Down_Cpn.ready = false ;
                                    levelHolder.spawn_T02_Down_Cpn.timer = list_UnitQueueT02[0].buildTime ;
                                    if (itemGO == null)
                                    {
                                        switch (list_UnitQueueT02[0].itemID)
                                        { 
                                            case 250 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagTank_T02) as GameObject ; break ;
                                            case 251 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Drone_T02) as GameObject ; break ;
                                            case 252 :  itemGO = GameObject.Instantiate (levelHolder.prefab_MagShield_T02) as GameObject ; break ;
                                            case 253 :  itemGO = GameObject.Instantiate (levelHolder.prefab_Swarmer_T02) as GameObject ; break ;
                                            case 254 :  itemGO = GameObject.Instantiate (levelHolder.prefab_SpiderBot_T02) as GameObject ; break ;
                                        }
                                        itemGO.transform.position = levelHolder.spawn_T02_Down.transform.position ;
                                        itemGO.transform.rotation = levelHolder.spawn_T02_Down.transform.rotation ;
                                        itemGO.GetComponent<Item>().spawnID = 4 ;
                                        NetworkServer.Spawn(itemGO) ;
                                        list_UnitQueueT02.RemoveAt(0) ;
                                        itemGO = null ;
                                    }
                                    else
                                    {
                                        RpcReactiveUnit (itemGO, levelHolder.spawn_T02_Down.transform.position, levelHolder.spawn_T02_Down.transform.rotation , 4) ;
                                        list_UnitQueueT02.RemoveAt(0) ;

                                        itemGO = null ;
                                    }
                                    break ;
                                }    
                            }
                        }
                    }
                yield return 0 ;
                }
   
            }
            yield return 0 ;
        }
    }
    IEnumerator AI ()
    {
        int waveNumber = 1 ;
        float waitTimer = 0.0f ;
        float waitBetweenWaves = 60.0f ;
        while (true)
        {
            //Wait 3 Seconds and Send Converters
            yield return new WaitForSeconds(3) ;
            for (int i=0; i < levelHolder.converterAIList.Count ; i++)
            {
                SpawnConverter(levelHolder.converterAIList[i],2) ;     
            }

            //Wait 12 Seconds and Spawn The Turrets
            yield return new WaitForSeconds(12) ;
            for (int i=0; i < levelHolder.turretAIList.Count ; i++)
            {
                list_TurretQueue.Add (new TurretQueue(levelHolder.turretAIList[i].itemID, levelHolder.turretAIList[i].itemPos, levelHolder.turretAIList[i].itemRot)) ;
            }

            //Wait 12 Seconds and Start the unit Waves
            yield return new WaitForSeconds(12) ;
            while (true)
            {
                //Random Unit
                for (int i=0 ; i < waveNumber; i++)
                {
                    int randUnit = Random.Range(250, 254) ;
                    list_UnitQueueT02.Add (new UnitQueueT02(randUnit, GameSettings.unit_Cannon_BuildTime)) ;
                    list_UnitQueueT02.Add (new UnitQueueT02(randUnit, GameSettings.unit_Cannon_BuildTime)) ;
                    list_UnitQueueT02.Add (new UnitQueueT02(randUnit, GameSettings.unit_Cannon_BuildTime)) ;
                    list_UnitQueueT02.Add (new UnitQueueT02(randUnit, GameSettings.unit_Cannon_BuildTime)) ;
                }
                waveNumber ++ ;
                waitTimer = waveNumber + waitBetweenWaves ;
                yield return new WaitForSeconds(waitTimer) ; 
            }
        }
    }
    IEnumerator CoreLife ()
    { 
        while (true)
        {
            if (levelHolder.coreT01.hpCurrent <= 0) 
            {
                if (!levelHolder.isAILevel) { gameController.victory = 2 ;  }
                break ; 
            }
            if (levelHolder.coreT02.hpCurrent <= 0) 
            { 
                if (!levelHolder.isAILevel) { gameController.victory = 1 ;  }
                break ; 
            }
            yield return 0 ;
        }
        RpcVictory(gameController.victory) ;
    }
    public void SpawnConverter(int index, int team)
    {
        RpcBusySection(index) ;
        Section section = levelHolder.sectionList[index] ;
        section.busy = true ;
        GameObject itemGO = null ;
        if (team == 1)
        {
            itemGO = Instantiate (levelHolder.prefab_Converter_T01) as GameObject ;
            itemGO.transform.position = section.convertPoint.position ;
            itemGO.transform.rotation = section.convertPoint.rotation ;
            NetworkServer.Spawn(itemGO) ;        
        }
        
        if (team == 2)
        {
            itemGO = GameObject.Instantiate (levelHolder.prefab_Converter_T02) as GameObject ;
            itemGO.transform.position = levelHolder.sectionList[index].GetComponent<Section>().convertPoint.position ;
            itemGO.transform.rotation = levelHolder.sectionList[index].GetComponent<Section>().convertPoint.rotation ;
            NetworkServer.Spawn(itemGO) ;
        }    
    }

    [ClientRpc] public void RpcVictory (int victor)
    {
        if (!levelHolder.isAILevel) { gameController.victory = victor ; }
        gameController.StartCoroutine("TakeOver") ;   
    }
    [ClientRpc] public void RpcBusySection (int index)
    {
        Section section = levelHolder.sectionList[index] ;
        section.busy = true ;    
    }
    [ClientRpc] public void RpcStartCore ()
    {
        levelHolder.coreT01.StartCoroutine("Init") ;
        levelHolder.coreT02.StartCoroutine("Init") ;
    }
    [ClientRpc] public void RpcReactiveTurret (GameObject item, Vector3 pos, Quaternion rot)
    {
        item.transform.position = pos ;
        item.transform.rotation = rot ;
        item.SetActive(true) ;      
    }
    [ClientRpc] public void RpcReactiveUnit (GameObject item, Vector3 pos, Quaternion rot, int spawnID)
    {
        item.transform.position = pos ;
        item.transform.rotation = rot ;
        item.GetComponent<Item>().spawnID = spawnID ;
        item.SetActive(true) ;      
    }
//------------------------------------------------------------------------------------------------------------------------------//
//															END																	//
//------------------------------------------------------------------------------------------------------------------------------//
}
