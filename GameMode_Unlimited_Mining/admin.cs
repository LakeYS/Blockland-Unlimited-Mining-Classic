// Admin.cs
// console commands for the admin

function GiveDirt(%amount)
   {
   for (%c = 0; %c < ClientGroup.getCount(); %c++)
     {
     %cl = ClientGroup.getObject(%c);
      if (%cl.hasSpawnedOnce)
        {
         %cl.AddDirt(%amount);
         %cl.sendMiningStatus();

         messageClient(%cl, '', "<color:FFFFFF>You got <color:FF0000>" @ %amount @ " <color:FFFFFF>dirt");
        }
     }
   }

function givebombs(%num)
   {
    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      {
       %cl = ClientGroup.getObject(%c);
       if (%cl.hasSpawnedOnce)
         {
          %cl.bomb=%num;
          %cl.sendMiningStatus();

          messageClient(%cl, '', "<color:FFFFFF>free level <color:FF0000>" @ %num @ " <color:FFFFFF>bomb");
         }
      }
   }

function giveHS(%num)
   {
    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      {
       %cl = ClientGroup.getObject(%c);
       if (%cl.hasSpawnedOnce)
         {
          %cl.addHS(%num);
          %cl.sendMiningStatus();

          messageClient(%cl, '', "<color:FFFFFF>you got <color:FF0000>" @ %num @ " <color:FFFFFF>heatsuits");
         }
      }
   }

// return a string exactly %len chars long, with space padding to the right
function makeNumField(%data, %len)
   {
    return MakeField(AddComma(%data), %len);
   }

function makeField(%data, %len)
   {
    if ( strlen(%data) > %len)
      {
       return " " @ getsubstr(%data, 0, %len-1);
      }
    return makePadString(" ", %len - strlen(%data)) @ %data;
   }

// display list of who is online & some info
function listMiners()
   {
    %str = MakeField("BL_ID", 6) @ MakeField("Name", 15) @ MakeField("Pick",8) @ MakeField("cash", 20) @ MakeField("HS", 12) @ MakeNumField("RS", 12)@MakeField("Dirt",10) @ MakeField("Rank", 5) @ MakeField("Location",11);
    echo(%str);
    echo( makePadString("-",strlen(%str) ));

    for ( %a=0; %a < ClientGroup.getCount(); %a++)
      {
       %cl = ClientGroup.getObject(%a);
       %str = MakeField(%cl.bl_id,6) @ MakeField(%cl.getSimpleName(), 15);
       %str = %str @ MakeNumField(%cl.getPick(), 8);
       %str = %str @ MakeNumField(%cl.getMoney().display(), 20) @ MakeNumField(%cl.getHS().display(), 12);
       %str = %str @ MakeNumField(%cl.getRS().display(), 12) @ MakeNumField(%cl.dirt.display(), 10);
       %str = %str @ MakeNumField(%cl.getRank(), 5);
       if ( isObject(%cl.player)  )
         {
          %pos = vectorSub(%cl.player.getPosition(), $BrickOffset);
         }

       %str = %str SPC mfloor(getword(%pos, 0)) SPC mFloor(getWord(%pos, 1)) SPC mFloor(getWord(%pos, 2));
       echo(%str);
      }
    echo( makePadString("-",strlen(%str)));
    echo("Bbricks: " @ getRealBrickCount() @ " Jobs: " @ $JobQueue.entries.getCount() @ " work: " @ $JobQueue.totalWork);
    echo("Drills : " @ $Dig_Data_ActiveDrills @ " buff x" @ $ModeMoney["normal"]);
   }

function kickName(%playername)
   {
    %client = findclientbyname(%playername);
    if ( !isObject(%client) )
      {
       echo("nobody named: " @ %playername);
       return;
      }
    %fakeClient= new ScriptObject(FakeClient) {
        isAdmin=1;
        bl_id=getNumKeyID();
        name="Console";
       };
    serverCmdKick(%fakeClient, %client);
   }

// This doesnt work yet
function BanName(%playerName, %reason)
   {
    %client = findclientbyname(%playername);
    if ( !isObject(%client) )
      {
       echo("nobody named: " @ %playername);
       return;
      }
    %fakeClient= new ScriptObject(FakeClient) {
     isAdmin=1;
     bl_id=getNumKeyID();
     name="Console";
    };
    //Entering serverCmdBan(4950749, 0, 21419, 60, test)
    //   BAN issued by Red_Guy BL_ID:20646 IP:98.210.14.49
    //     +- victim name = ALLON
    //     +- victim bl_id = 21419
    //     +- ban time = 60
    //     +- ban reason = test
    //   Entering BanManagerSO::addBan(4199, 4950749, 0, 21419, test, 60)
    //      Entering BanManagerSO::RemoveBanBL_ID(4199, 21419)
    //      Leaving BanManagerSO::RemoveBanBL_ID() - return 4199
    //      Entering getCurrentMinuteOfYear()
    //      Leaving getCurrentMinuteOfYear() - return 307967
    //      Entering getCurrentYear()
    //      Leaving getCurrentYear() - return 10
    //   Leaving BanManagerSO::addBan() - return 5
    //   Entering BanManagerSO::saveBans(4199)
    //   Leaving BanManagerSO::saveBans() - return
    //   Entering MessageAll(134, 128, Red_Guy, ALLON, 21419, 60, test)
    //      Entering messageClient(4950749, 134, 128, Red_Guy, ALLON, 21419, 60, test, , , , , , , , )
    //      Leaving messageClient() - return
    //   Leaving MessageAll() - return
    //Leaving serverCmdBan() - return 4950749
    servercmdban(%fakeClient,%client,%Client.BLID,60,%reason);
   }

// for admin only
function serverCmdEditOre(%client)
   {
    if ( %client.isAdmin)
      commandToClient(%client, 'EditOre');
   }

function serverCmdGetMiningPlayerList(%client)
   {
    if ( %client.isAdmin)
      {
       commandToclient(%client, 'ReceiveMiningPlayer', "Player" TAB "pick" TAB "Money" TAB "Heatsuit" TAB "Radsuit" TAB "Rank");
       for ( %a=0; %a < ClientGroup.getCount(); %a++)
         {
          %cl = ClientGroup.getobject(%a);
          commandToclient(%client, 'ReceiveMiningPlayer', %cl.getPlayerName() TAB %cl.getPick() TAB %cl.getMoney().display() TAB %cl.getHS().display() TAB %cl.getRS().display() TAB %cl.getRank() );
         }
      }
   }

function serverCmdSetDig_Value(%client, %playerName, %thing, %value)
   {
    // only superadmin can change player stats
    if ( %client.isSuperAdmin)
     {
      echo("SetDig_Value(" @ %client @ " , " @ %playerName @ " , " @ %thing @ " , " @ %value);
      if ( !%client.isAdmin )
        {
         return;
        }
      %playerClient = findClientByName(%playerName);

      switch$ (%thing)
        {
         case "money" :
           %playerClient.stats.MineMoney = bigNumberFromString(%value);
         case "pick" :
           %playerClient.stats.pickAxeNum = %value;
         case "heatsuit" :
           %playerClient.stats.heatsuit  = BigNumberFromString(%value);
         case "radsuit" :
           %playerClient.stats.radsuit   = BigNumberFromString(%value);
         case "Rank" :
           %playerClient.stats.rank = %value;
         case "Bomb" :
           %playerClient.bomb = %value;
        }

      %playerClient.sendMiningStatus();
     }
   }

// send the list of ores to %client
function serverCmdGetMiningOresList(%client)
   {
    for ( %a=0; %a < OreData.getCount(); %a++)
      {
       %ore = OreData.getObject(%a);
       commandToClient(%client, 'ReceiveOreData', %ore.getClientRecord() );
      }
   }

function MineOre::getClientRecord(%this)
   {
    return %this TAB %this.name TAB %this.value TAB %this.color TAB %this.minPercent TAB %this.maxPercent TAB %this.depth TAB %this.colorfx TAB %this.veinLength TAB %this.minPick TAB %this.bl_id TAB %this.timeout TAB %this.disabled;
   }

function MineDirt::getClientRecord(%this)
   {
    return MineOre::getClientRecord(%this);
   }
function MineBomb::getClientRecord(%this)
   {
    return MineOre::getClientRecord(%this);
   }
function MineLotto::getClientRecord(%this)
   {
    return MineOre::getClientRecord(%this);
   }
function RadioactiveLotto::getClientRecord(%this)
   {
    return MineOre::getClientRecord(%this);
   }

// make a copy of the ore and send back a new one
function serverCmdNewOre(%client, %oreRecord)
   {
    if ( %client.isSuperAdmin)
      {
       %ore = getField(%OreRecord, 0);
       %newOre = %ore.copy();
       %newOre.name = "Copy Of " @ %ore.name;
       OreData.add(%newOre);
       commandToClient(%client, 'ReceiveOreData', %newOre.getClientRecord() );
      }
   }

// Update the ore
function serverCmdUpdateOre(%this, %oreRecord)
   {
    if ( %this.isSuperAdmin)
      {
       %ore = getField(%OreRecord, 0);
       %ore.name = getField(%oreRecord, 1);
       %ore.value = getField(%oreRecord, 2);
       %ore.color = getField(%oreRecord, 3);
       %ore.minPercent= getField(%oreRecord, 4);
       %ore.maxPercent= getField(%oreRecord, 5);
       %ore.depth     = getField(%oreRecord, 6);
       %ore.colorfx   = getField(%oreRecord, 7);
       %ore.veinLength= getField(%oreRecord, 8);
       %ore.minPick   = getField(%oreRecord, 9);
       %ore.bl_id     = getField(%oreRecord, 10);
       %ore.timeout   = getField(%oreRecord, 11);
       %ore.disabled  = getField(%oreRecord, 12);
       echo("Updated: " @ %ore.name);
      }
   }

// Delete the ore from the server
function serverCmdDeleteOre(%client, %oreRecord)
   {
    if ( %client.isSuperAdmin)
      {
       echo("oreRecord: " @ %oreRecord);
       %ore = getField(%oreRecord, 0);
       OreData.remove(%ore);
       // dont delete the ore, as it may be referenced by bricks in game.
       echo("Deleted: " @ %ore.name);
      }
   }

// save all the ores to config/server/MiningData.cs
function serverCmdSaveOres(%client)
   {
    if ( %client.isSuperAdmin)
      {
       OreData.save("config/server/miningData.cs");
      }
   }

function cancelAllDrills()
   {
   for (%c = 0; %c < ClientGroup.getCount(); %c++)
     {
     %cl = ClientGroup.getObject(%c);
      if (%cl.hasSpawnedOnce)
        {

         serverCmdDrill(%cl, 0, 0);
        }
     }
    ResetDrills();
   }
function cancelAllBombs()
   {
    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      {
       %cl = ClientGroup.getObject(%c);
       if (%cl.hasSpawnedOnce)
         {
          serverCmdBomb(%cl, 0);
         }
      }
   }

// ================================
// misc semi-helpful admin commands
// ================================

// punt player back to start
function serverCmdPunt(%client, %playerName)
   {
    if ( %client.isAdmin)
      {
       %playerClient = findclientbyname(%playername);
       if ( !isObject(%playerClient))
         {
          messageClient(%client, '', "<color:FFFFFFFF>Nobody named " @ %playerName @ " found");
          return;
         }
       %playerClient.instantrespawn();
      }
   }

// kill the named player
function servercmdKill(%client, %playername)
   {
    if ( %client.isAdmin)
      {
       %playerClient = findclientbyname(%playername);
       if ( !isObject(%playerClient))
         {
          messageClient(%client, '', "<color:FFFFFFFF>Nobody named " @ %playerName @ " found");
          return;
         }
       %playerClient.player.kill();
      }
   }

function ServerCmdAutoKick(%client, %playerName)
   {
    if ( %client.isAdmin)
      {
       return ;Autokick(%playerName);
      }
    messageClient(%client, '', "<color:FF0000>Access Denied<color:FFFFFF> Admin-only function");
   }

// kick %name everytime they log in
function AutoKick(%name)
   {
    %client = findclientbyname(%name);
    if ( isObject(%client) )
      {
       if ( %client.hasspawnedOnce)
         kickname(%name);
      }

    schedule(2000, 0, "Autokick", %name);
   }

function mute(%name)
   {
    %client = findclientbyname(%name);
    if ( isObject(%client) )
      {
       %client.isMuted = true;
       echo("Muted " @ %client.getPlayerName() );
      }
    else
      echo("nobody named " @ %name);
   }

function msg(%name, %text)
   {
    %client = findclientbyname(%name);
    if ( isObject(%client) )
      {
       messageClient(%client, '', "<color:FFFFFF>priv msg from console:<color:00FF00> " @ %text);
       echo("message sent to " @ %client.getPlayerName() );
     }

   }
