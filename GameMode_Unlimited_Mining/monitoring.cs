// monitoring.cs
// Functions for Monitoring the brick count and to do a game reset as needed

// start dg mode regardless of who the client is
function Dig_StartAnyway(%client)
   {
    if ( $Dig_On)
      {
       error("Dig already running - cant start a 2nd time");
       return;
      }
    if ( $Dig_host != %client)
      {
       $Dig_host = %client;
      }

    %spawnsphere = playerDropPoints.getobject(0);
    %spawnsphere.radius = 5;
    %spawnsphere.RayHeight = 3;

    deleteVariables("$Dig_placedDirt*");

    CheckDirt();

    exec("config/server/MiningStats.cs");
    if ( isFile("config/server/MiningData.cs") )
      {
       OreData.delete();
      }
    exec("config/server/MiningData.cs");
    Dig_CheckHost();

    GenSpawnLocation( VectorAdd("0 0 10", $BrickOffset) );
    GenSpawnLocation(VectorAdd("500 0 10", $BrickOffset) );
    GenSpawnLocation(VectorAdd("0 500 10", $BrickOffset) );

    activatepackage(Dig_package);
    activatepackage(UnLag);

    // respawn the active players
    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      if (ClientGroup.getObject(%c).hasSpawnedOnce)
         ClientGroup.getObject(%c).schedule(5000, "instantRespawn");

    $Dig_on = 1;
    messageAll('', "<color:00FF00>Dig mode is now enabled");

    // load mining stats for all clients
    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      {
       %cl = ClientGroup.getObject(%c);
       %cl.LoadMiningStats();
       %cl.SetupAcheivements();
       Dig_CheckLogin(%cl);
      }

    Dig_ExpireOres();
    $Pref::Server::WelcomeMessage="Welcome to Unlimited mining %1.  \c3Type /help for info.";

    setRayTracerCenter("0 0 0");

    // start job processor
    $Dig_TotalBlobs=0;
    LavaProcessor::Start();

    // Start monitoring timer
    Dig_Monitor();
   }

function servercmdStopDig(%client)
   {
    if (%client.bl_id != getNumKeyID() && findLocalClient() !$= %client)
      {
       messageClient(%client, '', "You are not the host so you cannot stop dig mode");
       return;
      }
    $Dig_stopDig=true;
    Dig_Stopanyway(%client);
    cancel($Dig_MonitorTimer);

   }

// stopdig regardless of who %client is
function Dig_stopanyway()
   {
    messageAll('', "<color:00FF00>Dig mode is off");
    $Dig_on = false;
    deactivatepackage(Dig_MiningAdminButton);
    deactivatePackage(Dig_Package);

    if ( $Dig_NumCycles > 3)
      {
       messageAll('', "<color:FFFFFF>-- **<color:FF0000>Server Reboot<color:FFFFFF>**--");
       messageAll('', "<color:FFFFFF>-- **<color:FF0000>Server Reboot<color:FFFFFF>**--");
       messageAll('', "<color:FFFFFF>-- **<color:FF0000>Server Reboot<color:FFFFFF>**--");
       messageAll('', "<color:FFFFFF>-- **<color:FF0000>Server Reboot<color:FFFFFF>**--");
       messageAll('', "<color:FFFFFF>-- **<color:FF0000>Server Reboot<color:FFFFFF>**--");
       messageAll('', "<color:FFFFFF>-- **<color:FF0000>Server Reboot<color:FFFFFF>**--");
       messageAll('', "<color:FFFFFF>-- ** Server will return eventually ** --");
       $RestartNow=true;
       export("$Restart*","config/server/restartnow.txt");
       schedule(3000, 0, "quit");

       // turn on autostart flag
       $Dig_Stats_SavedAutoexec=true;
       export("$Dig_stats_Saved*", "config/server/miningStats.cs");
       OreData.save("config/server/miningData.cs");
       return;
      }

    // Clear all bricks
    for ( %a=0; %a < MainBrickGroup.getCount(); %a++)
      {
       %group = MainBrickGroup.getObject(%a);
       if ( %group.getCount() > 0)
         {
          %group.chainDeleteAll();
         }
      }

    Dig_saveStats();

    if ( isObject($LavaQueue) )
       {
        $LavaQueue.delete();
       }
    ResetDrills();
    ResetSpamKills();

    $Pref::Server::WelcomeMessage="";
    $Dig_on = 0;
    echo("Delete Dig_PlacedDirt1*"); deleteVariables("$Dig_placedDirt1*");
    echo("Delete Dig_PlacedDirt2*"); deleteVariables("$Dig_placedDirt2*");
    echo("Delete Dig_PlacedDirt3*"); deleteVariables("$Dig_placedDirt3*");
    echo("Delete Dig_PlacedDirt4*"); deleteVariables("$Dig_placedDirt4*");
    echo("Delete Dig_PlacedDirt5*"); deleteVariables("$Dig_placedDirt5*");
    echo("Delete Dig_PlacedDirt6*"); deleteVariables("$Dig_placedDirt6*");
    echo("Delete Dig_PlacedDirt7*"); deleteVariables("$Dig_placedDirt7*");
    echo("Delete Dig_PlacedDirt8*"); deleteVariables("$Dig_placedDirt8*");
    echo("Delete Dig_PlacedDirt9*"); deleteVariables("$Dig_placedDirt9*");
    echo("Delete Dig_PlacedDirt*");  deleteVariables("$Dig_placedDirt*");
    Dig_resetGame();
   }

// Reset the game.  Wait for all the bricks to delete and then start over
function Dig_resetGame()
   {
    %bricks = getRealBrickCount();
    echo("Dig_changeMapTry %" @ %bricks @ " bricks left");
    if (%bricks < 1)
      {
       messageAll('', "<color:FFFFFF>"@ %bricks @ " Bricks left");

       // clear all players bottomBrint stuff
       for (%c = 0; %c < ClientGroup.getCount(); %c++)
         {
          %client = ClientGroup.getObject(%c);
          commandtoClient(%client, 'bottomPrint', "", 1);
         }
       Dig_CheckHost();
       Dig_RestoreMoney();

       if ($Dig_stopDig) // /stopdig entered from the client - stop here
         {
          echo("stop sent from client, stopping for real");
          return;
         }

       // restart dig mode in 15 seconds
       echo("dig_start anyway in 15 sec");
       schedule(1000*15,0, "dig_startanyway", $Dig_Host);
       AddLog(0, "Reset Game", "");
       return;
      }
    messageAll('', "<color:FFFFFF>"@ %bricks @ " Bricks left");
    schedule(5000, 0, Dig_ResetGame);
   }

// return the actual number of bricks
function getRealBrickCount()
   {
    %bricks = 0;
    for (%i=0;%i<mainBrickGroup.getCount();%i++)
      {
       %group = mainBrickGroup.getObject(%i);
       %bricks += %group.getCount();
      }
    return %bricks;
   }

// re-create the fake client if needed
function Dig_CheckHost()
   {
    // set or reset the host object if needed
    if ( !isObject($Dig_Host) )
      {
       // no host client -- make one
       echo("no host object - faking it");
       %bl_id = GetNumKeyID();
       $Dig_Host = new ScriptObject() {
         isAdmin=1;
         brickgroup = 0;
         bl_id = %bl_id;
       };
      }
    else
      echo("$Dig_Host check passed");

    //re-create the clients brickgroup if needed
    if ( !isObject($Dig_Host.brickgroup) )
      {
       echo("Making new client brickgroup");
       %bl_id = getNumKeyID();
       %brickgroup=new SimGroup("BrickGroup_" @ %bl_id)
       {
        client=0;
        bl_id = %bl_id;
        name="\c1BL_ID: " @ %bl_id @ "\c1\c0";
       };
       $Dig_Host.brickgroup = %brickgroup;
       if ( isObject(mainBrickGroup) )
         {
          mainbrickgroup.add($Dig_host.brickgroup);
         }
      }
    else
      echo("$Dig_Host.brickgroup check passed");
   }

// check brick count every so often and reset things if needed
function Dig_MonitorBrickCount()
  {
   Dig_CheckHost();
   %bricks = getRealBrickCount();
   messageAll('', "Brick count <color:FFFFFF>" @ %bricks @ " of " @ $Dig_Data_BrickLimit);
   AddLog(0, "Brick Count" , %bricks @ " of " @ $Dig_Data_BrickLimit, 1);
   AddLog(0, "Job Size", $JobQueue.entries.getCount() @ " jobs", 1);
   if ( $Dig_Data_BrickLimit > 0)
     if ( %bricks > $Dig_Data_BrickLimit )
       {
        messageAll('', "<color:FFFFFF>-- **<color:FF0000>WARNING<color:FFFFFF>**--");
        messageAll('', "<color:FFFFFF>-- Brick count is above limit, server will be reset soon");
        messageAll('', "<color:FFFFFF>-- **<color:FF0000>WARNING<color:FFFFFF>**--");
        messageAll('', "<color:FFFFFF>-- **<color:FF0000>WARNING<color:FFFFFF>**--");
        messageAll('', "<color:FFFFFF>-- Brick count is above limit, server will be reset soon");
        messageAll('', "<color:FFFFFF>-- **<color:FF0000>WARNING<color:FFFFFF>**--");

        for (%c = 0; %c < ClientGroup.getCount(); %c++)
          {
           %client = ClientGroup.getObject(%c);
           commandToClient(%client,'MessageBoxOK', "SERVER RESET IN 30 SECONDS", "-- Server reset in 30 seconds --\n SERVER RESET\n\nServer RESET IN 30 SECONDS");
          }

        schedule(1000*30,0,"Dig_CycleServer");
        return false;
       }

   return true;
  }

// Start the reset process
function Dig_CycleServer()
  {
   BlobUseReport();
   messageAll('', "<color:FF0000>server Reset NOW");
   AddLog(0, "Cycle Server", "Cycle_Server");
   $Dig_cycle=true;
   $Dig_NumCycles++;
   $Dig_CycleCount=0;
   Dig_Stopanyway();
  }

