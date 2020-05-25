$Dig_PickaxeColor = "0.000000 0.000000 0.900000 1.000000";
$Dig_CementAxeColor = "0.500000 0.500000 0.500000 1.000000";

if (isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
  {
   if (!$RTB::RTBR_ServerControl_Hook)
     exec("Add-Ons/System_ReturnToBlockland/RTBR_ServerControl_Hook.cs");

   RTB_registerPref("Brick Limit"      ,"Unlimited Mining","$Dig_Data_BrickLimit",    "int 0 350000","GameMode_Unlimited_Mining",250000,0,0);
   RTB_registerPref("Heatsuit Cost"    ,"Unlimited Mining","$Dig_Data_HeatsuitCost",  "int 0 5000",  "GameMode_Unlimited_Mining",200,0,0);
   RTB_registerPref("Radiation suit Cost","Unlimited Mining","$Dig_Data_RadsuitCost", "int 0 5000",  "GameMode_Unlimited_Mining",500,0,0);
   RTB_registerPref("Light Cost"       ,"Unlimited Mining", "$Dig_Data_LightCost",    "int 0 5000",  "GameMode_Unlimited_Mining",25,0,0);
   RTB_registerPref("Dirt Cost"        ,"Unlimited Mining", "$Dig_Data_DirtCost",     "int 0 5000",  "GameMode_Unlimited_Mining",1,0,0);
   RTB_RegisterPref("Kill Warning"     ,"Unlimited Mining", "$Dig_Data_KillWarning",  "int 1 500",   "GameMode_Unlimited_Mining",3,0,0);
   RTB_RegisterPref("Kill Limit"       ,"Unlimited Mining", "$Dig_Data_KillLimit",    "int 1 500",   "GameMode_Unlimited_Mining",4,0,0);
   RTB_RegisterPref("Low Spawn Limit"  ,"Unlimited Mining", "$Dig_Data_LowSpawnLimit","int 1 10000", "GameMode_Unlimited_Mining",7000,0,0);
   RTB_RegisterPref("High Spawn Limit" ,"Unlimited Mining", "$Dig_Data_HighSpawnLimit","int 1 40000","GameMode_Unlimited_Mining",25000,0,0);
  }
else
  {
   $Dig_Data_BrickLimit = 350000;   // reset server after this # of bricks
   $Dig_Data_lightCost=25;
   $Dig_Data_HeatSuitCost=200;
   $Dig_Data_DirtCost=1;
   $Dig_Data_radsuitCost=250;
   $Dig_Data_KillWarning=3;
   $Dig_Data_KillLimit=4;
   $Dig_Data_LowSpawnLimit=7000;
   $Dig_Data_HighSpawnLimit=25000;
  }

$Dig_Data_LowSpawnLimit=7000;
$Dig_Data_HighSpawnLimit=25000;
$Brick_Z = 50000;
$BrickOffset = "0 0 " @ $Brick_Z;

exec("./miningDatablocks.cs");
exec("./monitoring.cs");
exec("./BigNumber.cs");
exec("./MiningStats.cs");
exec("./Donating.cs");
exec("./Upgrading.cs");
exec("./Ores.cs");
exec("./playerOres.cs");
exec("./Bombs.cs");
exec("./DirtLottery.cs");
exec("./Drill.cs");
exec("./Admin.cs");
exec("./Structures.cs");
exec("./Acheivements.cs");
exec("./MiningAdminGui.cs");
exec("./help.cs");
exec("./Logging.cs");
exec("./BaseGenerator.cs");

function servercmdstartDig(%client)
   {
    if (%client.bl_id != getNumKeyID() && findLocalClient() !$= %client)
      {
       messageClient(%client, '', "You are not the host so you cannot start dig mode");
       return;
      }
    // check for required color set

    // is it activated?
    if ( getColorIDTable(54) !$= "0.803922 0.666667 0.486275 1.000000" || getColorIDTable(45) !$= "0.976471 0.976471 0.976471 1.000000" )
      {
       messageClient(%client, '', "<color:FF0000>You do not have Trueno's colorset<color:FFFFFF>Activated");
       messageClient(%client, '', "<color:FFFFFF>Activate the color set via RTB, or search the forums for instructions how to activate a colorset");
       return;
      }

    // check to make sure 4x4 cube exists
    if ( !isObject(brick4xCubeData) )
      {
       messageClient(%client, '', "<color:FF0000>You do not have the 4x4 cube brick loaded");
       messageClient(%client, '', "<color:FFFFFF>This is part of a default addon and should come standard with your copy of blockland.");
       messageClient(%client, '', "<color:FFFFFF>Activate the <color:FF0000>\"Brick_Large_Cubes\"<color:FF0000> addon to continue");
       return;
      }

    if ( !isFile("Add-ons/Tool_RPG/Lil_Pickaxe.dts") )
      {
       messageClient(%client, '', "<color:FF0000>You do not have the RPG_Tool addon installed");
       messageClient(%client, '', "<color:FFFFFF>Please download this addon and try again");
       return;
      }

    Dig_StartAnyway(%client);
    $Dig_NumCycles=0;  //server started manually - reset # of cycles
   }


function servercmdPowerMode(%client)
   {
    if (!%client.isAdmin)
      return;

    if (%client.dig_powermode)
      {
       messageClient(%client, '', "Power mode is now off");
       %client.dig_powermode = 0;
      }
    else
      {
       messageClient(%client, '', "Power mode is now on");
       %client.dig_powermode = 1;
      }
   }

// place a 'dirt' brick.  can be regular dirt, or an ore brick depending on random percent
function Dig_PlaceDirt(%pos)
   {
    %x = getWord(%pos, 0);
    %y = getWord(%pos, 1);
    %z = getWord(%pos, 2);

    %type = 0;
    %dirt=OreData.getObject(0);
    %random = getRandom() * 100;
    if ( %random < 85 )
        return %dirt.placeAt(%pos, %random);

    %random = getRandom() * 100;
    if ( $Dig_Lava.checkPlace(%pos, %random) )
      {
       return $Dig_Lava.placeAt(%pos);
      }

    %c = OreData.getCount();
    for ( %x=1; %x < %c; %x++)
      {
       %ore = OreData.getObject(%x);
       if ( %ore.checkPlace(%pos, %random) )
         {
          return %ore.placeAt(%pos);
         }
      }
    return %dirt.placeAt(%pos, getRandom() * 100);
   }

// replace dirt at all around %position
function Dig_rebuildDirt(%position)
   {
    %x = getWord(%position, 0);
    %y = getWord(%position, 1);
    %z = getWord(%position, 2);
    %position[0] = %x+2 SPC %y SPC %z;
    %position[1] = %x-2 SPC %y SPC %z;
    %position[2] = %x SPC %y+2 SPC %z;
    %position[3] = %x SPC %y-2 SPC %z;
    %position[4] = %x SPC %y SPC %z+2;
    %position[5] = %x SPC %y SPC %z-2;

    for (%a = 0; %a < 6; %a++)
     {
      %dirt = $Dig_placedDirt[%position[%a]];
      if ( strlen(%dirt) == 0 )
        {
         Dig_PlaceDirt(%position[%a]);
        }
      else
        {
         if ( mFloor(%dirt) == 0 )
           {
            generateOre(%dirt, %position[%a] );
           }
        }
     }
   }


// mode normal: %sound=0; lava=1; money=1; health=1; discover=1;
// powermode:   %sound=0; lava=0; money=0; health=0; discover=0;
// bomb:        %sound=1; lava=0; money=1; health=0; discover=0;
$ModeSound["normal"]=0;
$ModeSound["power"]=0;
$ModeSound["bomb"]=1;
$ModeSound["drill"]=0;
$Modelava["normal"]=1;
$Modelava["power"]=0;
$Modelava["bomb"]=0;
$ModeLava["drill"]=0;
$ModeMoney["normal"]=1;
$ModeMoney["power"]=0;
$ModeMoney["bomb"]=-0;
$ModeMoney["drill"]=0;
$ModeHealth["Normal"]=1;
$ModeHealth["power"]=0;
$ModeHealth["bomb"]=0;
$ModeHealth["drill"]=0;
$ModeDiscover["Normal"]=1;
$ModeDiscover["power"]=0;
$ModeDiscover["bomb"]=0;
$ModeDiscover["drill"]=0;

function Dig_mineBrick(%brick, %pos1, %client, %mode)
   {
    if (%brick.isMineable)
      {
       %pos = %brick.getPosition();
       %z=getword(%pos,2);
       if ( %z > $Dig_Data_Maxheight)
         {
          %brick.isMineable=false;
          return;
         }
       if ( %z < 5)
         {
          %brick.isMineable=false;
          return;
         }
       if ( !isObject(%client) )
          return;

       if (%mode $="")
         {
          %mode="normal";
         }
       if ( %client.dig_powermode == 1)
          {
           %mode = "power";
          }

       %sound = $ModeSound[%mode];
       %lava  = $ModeLava[%mode];

       if ( %sound==0 )
         {
          serverPlay3D(hammerHitSound, %pos);
         }
       %brick.type.mined(%client, %brick, %mode);
       if ( %brick.health < 1)
         {
          if ( %lava == 1)
            {
             commandToClient(%client, 'centerPrint', "Mined "@ %brick.type.getMineName(%brick), 5);
             %client.sendMiningStatus();
            }
          Dig_rebuildDirt(%pos);
          $Dig_placedDirt[%pos] = -2;   // flag as deleted
          %brick.delete();
          if ( %mode $= "normal")
            {
             %client.checkDepth();
            }
         }
       else
         commandToClient(%client, 'centerPrint', "Health: <color:FFFFFF>"@ %brick.health SPC %brick.type.getMineName(%brick), 3);
       return;
      }
    if ( %brick.isCement)
      {
       if ( %brick.getGroup().bl_id != %client.bl_id)
         messageClient(%client, '', "<color:FFFF00>Invulnerable cement -- leave it alone");
       else
         messageClient(%client, '', "<color:FFFF00>You cant mine cement, use your gun");
      }
   }

// display an error message for %client
function Dig_DisplayError(%client, %msg)
   {
    commandToClient(%client, 'centerPrint', "<color:FF0000>" @ %msg, 5);
   }


// buy an inventory item that goes in the tools slot
function buyThing(%client, %thing, %cost)
   {
    %discount = %client.RankDiscount(%cost);
    %price = %cost - %discount;
    if ( %client.getMoney() >= %price)
      {
       if ( %discount > 0)
         {
          MessageClient(%client, '', "<color:00FF00>Rank discount $" @ AddComma(%discount) );
         }
       %slot = -1;
       %item = %thing.getID();
       for ( %x=0; %x <= 4; %x++ )
         {
          if ( %client.player.tool[%x]==0)
            {
             %slot = %x;
            }
          if ( %client.player.tool[%x] == %item )
            {
             Dig_DisplayError(%client, "you already have a " @ %thing.uiName);
             return;
            }
         }
       if ( %slot == -1)
         {
          Dig_DisplayError(%client, "You dont have room in your inventory for a " @ %thing.uiName);
          return;
         }

       %client.AddMoney( -%price, "buy " @ %thing.uiName);
       %client.player.tool[%slot] = %item;
       MessageClient(%client, 'MsgItemPickup', '', %slot, %item );
       commandToClient(%client, 'centerPrint', "<color:00FF00>You bought a " @ %thing.uiName @ " for \c3$" @ %price, 3);
      }
    else
      {
       Dig_DisplayError(%client, "You cannot afford a " @ %thing.uiName @ " for \c3$" @ %price);
      }
   }

// buy something
function serverCmdBuy(%client, %thing, %level)
   {
    if ( ($Dig_on == 0) )
      {
       return; // dig mode is off
      }
    %amount = mfloor(mabs(%level));

    if ( %thing $= "gun")
      {
       if (%client.GunDisabled)
         {
          Dig_DisplayError(%client, "Your gun priveleges are revoked");
          return;
         }
       buyThing(%client, GunItem, 250);
       return;
      }
   if ( %thing $= "placer")
     {
      buything(%client, DiggingCementItem, 100);
      return;
     }
   if ( %thing $= "invplacer")
     {
      buything(%client, BlockadeItem, 100);
      return;
     }
   if ( %thing $= "light")
     {
      serverCmdBuyLight(%client);
      return;
     }
   if ( %thing $= "pick")
     {
      serverCmdUpgradePick(%client);
      return;
     }
   if ( %thing $= "heatsuit")
     {
      serverCmdBuyHeatsuit(%client, %amount);
      return;
     }
   if ( %thing $= "bomb")
     {
      buybomb(%client, %amount);
      return;
     }
   if ( %thing $= "dirt")
     {
      buydirt(%client, %amount);
      return;
     }
   if ( %thing $= "radsuit")
     {
      serverCmdBuyRadSuit(%client, %amount);
      return;
     }
   if ( %thing $= "insurance")
     {
      buyInsurance(%client, %amount);
      return;
     }
   serverCmdHelp(%client);
  }

// buy some dirt
function buyDirt(%client, %amount)
  {
   %price = mabs(%amount * $Dig_Data_DirtCost);
   if ( %client.getMoney().lessThan( %price))
     {
      Dig_DisplayError(%client, "You need $" @ %price @ " to buy " @ %amount @ " dirt");
      return;
     }

   commandToClient(%client, 'centerPrint', "You bought " @ %amount @ " dirt for $" @ %price, 3);
   %client.AddMoney( -%price, "buy dirt " @ %amount);
   %client.addDirt(%amount);
   %client.sendMiningStatus();
  }

// Buy a light
function serverCmdBuyLight(%client)
  {
   if (%client.mineLight)
     {
      Dig_DisplayError(%client, "You already bought a light!");
      return;
     }
   if ( %client.getMoney().lessThan($Dig_Data_LighCost) )
     {
      Dig_DisplayError(%client, "You cannot afford a light for \c3$" @ $Dig_Data_LightCost);
      return;
     }

   %client.mineLight = 1;
   commandToClient(%client, 'centerPrint', "You have bought a light for \c3$" @ $Dig_Data_LightCost, 3);
   %client.AddMoney( -$Dig_Data_LightCost, "buy light");
   serverCmdLight(%client);
   %client.sendMiningStatus();
  }

// buy some heatsuits
function serverCmdBuyHeatSuit(%client, %layerValue)
  {
   %layers = mFloor(%layerValue);
   if ( %layers < 1)
     {
      %layers = 1;
     }
   %price = $Dig_Data_HeatsuitCost * %layers;
   %discount = %client.RankDiscount(%price);
   if ( %discount > 0)
     {
      MessageClient(%client, '', "<color:00FF00>Rank discount $" @ AddComma(%discount) );
      %price -= %discount;
     }
   if ( %client.getMoney().greaterThan(%price) )
     {
      %client.addHS( %layers);
      %client.AddMoney( -%price, "buyheatsuit " @ %layers);
      if ( %layers ==1)
        {
         commandToClient(%client, 'centerPrint', "You have bought a heat suit for \c3$" @ %price, 3);
        }
      else
        {
         if ( %client.getHS()> %layers)
           commandToClient(%client, 'centerPrint', "You added " @ %layers @ " layers to your heat suit for \c3$" @ %price, 3);
         else
           commandToClient(%client, 'centerPrint', "You have bought a " @ %layers @ " layer heat suit for \c3$" @ %price, 3);
        }
      %client.sendMiningStatus();
     }
   else
     {
      Dig_DisplayError(%client, "You cannot afford a heat suit \c3$" @ %price );
     }
  }

// buy some radsuits
function serverCmdBuyradSuit(%client, %layerValue)
  {
   %layers = mFloor(%layerValue);
   if ( %layers < 1)
     {
      %layers = 1;
     }
   %price = $Dig_Data_radsuitCost * %layers;
   %discount = %client.RankDiscount(%price);
   if ( %discount > 0)
     {
      MessageClient(%client, '', "<color:00FF00>Rank discount $" @ AddComma(%discount) );
      %price -= %discount;
     }
   if ( %client.getMoney().greaterThan(%price) )
     {
      %client.AddRS(%layers);
      %client.AddMoney( -%price, "buyradsuit " @ %layers);
      if ( %layers == 1)
        {
         commandToClient(%client, 'centerPrint', "You have bought a rad suit for \c3$" @ %price, 3);
        }
      else
        {
         if ( %client.getRS() > %layers)
           commandToClient(%client, 'centerPrint', "<color:0000FF>You added " @ %layers @ " capacity to your rad suit for \c3$" @ %price, 3);
         else
           commandToClient(%client, 'centerPrint', "<color:0000FF>You have bought a " @ %layers @ " capacity rad suit for \c3$" @ %price, 3);
        }
      %client.sendMiningStatus();
     }
   else
    {
     Dig_DisplayError(%client, "You cannot afford a rad suit \c3$" @ %price );
    }
  }

// Penalize %client for cheating
function FineForCheating(%client, %amount)
   {
    Dig_DisplayError(%client, "You have been fined $" @ %amount @" for trying to cheat");
    %client.AddMoney( -%amount, "Fine cheating");
    %client.sendMiningStatus();
   }

function DiggingPickaxeImage::onFire(%this, %obj, %slot)
   {
    parent::onFire(%this, %obj, %slot);
    %obj.playThread(2, "armAttack");
   }

function DiggingPickaxeImage::onStopFire(%this, %obj, %slot)
  {
   %obj.playThread(2, "root");
  }

// "hack" into the print gun's onFire code to use the cement placer
function DiggingCementImage::onFire(%this, %arga, %argb)
  {
   PrintGunImage::onFire(%this, %arga, %argb);
  }

function BlockadeImage::onFire(%this, %arga, %argb)
  {
   PrintGunImage::onFire(%this, %arga, %argb);
  }

// return the cost for %client to upgrade pick to the next level
function Dig_PickCost(%client)
  {
   %num = %client.getPick() + 1;
   if ( %num < 10)
     {
      return 50;
     }
   if ( %client.stats.rank > 0)
     {
      return mFloor( mPow(%num,1.3) ) * %client.stats.rank;
     }
   return mFloor( mPow(%num,1.3) ) + 50;
  }

// monitor brick count and save stats every so often
function Dig_Monitor()
   {
    Dig_SaveStats();
    if ( !Dig_monitorBrickCount() )
      {
       return;
      }

    // save stats every 5min
    $Dig_MonitorTimer = schedule(1000*60*5, MissionGroup, Dig_Monitor);
   }

//save all stats to a file: config/server/miningStats.cs
function Dig_saveStats()
  {
   for (%c = 0; %c < ClientGroup.getCount(); %c++)
    {
     %client = ClientGroup.getObject(%c);
     %client.saveMiningStats();
    }
   export("$Dig_stats_Saved*", "config/server/miningStats.cs");
   FlushLog();
  }



// send a message box to the client displaying rules for them to read
function GameConnection::sendRules(%this)
  {
   if ($Dig_placedRead[%this.bl_id]==1)
     {
      %this.ruleNumber=0;
      commandToClient(%this,'MessageBoxOK', "Unlimited Infinite Mining Mod", "Use the pickaxe to mine bricks\n\n /upgradepick to level your pick\n\n /upgradeall for multiple upgrades (costs extra)\n\n /help for more");
     }
   else
     {
      %this.ruleNumber=getRandom(1,1000);
      commandToClient(%this,'MessageBoxOK', "Unlimited Infinite Mining Mod", "Use the pickaxe to mine bricks\n\n /upgradepick to level your pick\n\n /upgradeall for multiple upgrades (costs extra)\n\n /help for more\n say: /Ihavereadtherules " @ %this.rulenumber @ " for free stuff");
     }
  }

// give rewards for actually reading the rules
function serverCmdIHaveReadTheRules(%client, %code)
  {
   if ( %client.ruleNumber==0)
     {
      return;
     }
   if ( %code == %client.ruleNumber)
     {
      if ( %client.getRank() < 1)
        {
         messageClient(%client, '', "<color:0000FF>RTFM Bonus<color:00FF00>You got 10 pick levels");
         %client.addPick(10);
        }

      messageClient(%client, '', "<color:0000FF>RTFM Bonus<color:00FF00>You got $1,000");
      messageClient(%client, '', "<color:0000FF>RTFM Bonus<color:00FF00>This only works once");
      %client.addMoney(1000, "RTFM", 1);

      %client.ruleNumber=0;
      $Dig_placedRead[%client.bl_id]=1; // works once per server reset
     }
   else
     Dig_Displayerror(%client, "Wrong secret code");
  }

function DiggingPickaxeProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal)
  {
   if ( !$Dig_On)
     {
      return;
     }

   %client = %obj.client;
   if ( %client.bomb > 0)
     {
      if ( %client.bomb > ($Dig_Data_BombSizeMax+%client.stats.rank) )
        {
         AddLog(%client, "Abort bomb", "level " @ %client.bomb @ " bomb aborted", 1);
         %client.bomb=0;
         return;
        }
      Dig_DoBomb(%col, %client, %client.bomb);
      return;
     }
   if ( %client.MineDrill > 0 || %client.MineOreDrill > 0)
     {
      Dig_StartDrill(%col, %pos, %client, %normal);
      return;
     }

   Dig_mineBrick(%col, %pos, %client);
  }

package UnLag
  {
   function GameConnection::ResetVehicles(%this)
     {
      // package resetVehicles to do nothing because it seems to scan all bricks.
      // when the brick count is > 80k or so, the lag gets unbearable
     }
  };

package Dig_package
{
   // Spawn the player and give them whats needed
   function GameConnection::spawnPlayer(%this)
      {
       %parent = Parent::spawnPlayer(%this);
       if ( !$Dig_on)
         {
          echo("Dig off - no loading of stats " @ %this.getPlayerName() );
          return %parent;
         }
       if ( %this.gunDisabled)
         {
          %this.player.cleartools();

          // Give player a pick
          %this.player.tool[0] = DiggingPickaxeItem.getID();
          MessageClient(%this, 'MsgItemPickup', '', 0, DiggingPickaxeItem.getID() );
          echo("Spawn " @ %this.getPlayerName() @ " with no gun");
         }
       %this.canFire=false;
       if ( isObject(%this.minigame ))
         {
          %this.dig_enableCement();
          %this.radDamage=0;
          %this.stats.MineArmor = %this.getPick();
          %this.radDamage=0;
          %this.setRankPrefix();
          return;
         }
       // clear out player tools
       %this.player.cleartools();

       // Give player a pick
       %this.player.tool[0] = DiggingPickaxeItem.getID();
       MessageClient(%this, 'MsgItemPickup', '', 0, DiggingPickaxeItem.getID() );

       // Admins also get a wrench
       if ( %this.isAdmin)
         {
          %this.player.tool[1] = WrenchItem.getID();
          MessageClient(%this, 'MsgItemPickup', '', 1, WrenchItem.getID() );
         }

       // let player place cement
       %this.dig_enableCement();
       %this.stats.MineArmor = %this.getPick();
       %this.radDamage=0;
       %this.setRankPrefix();
      }

    // hook into autoAdminCheck to load player stats
    function GameConnection::autoAdminCheck(%this)
      {
       %parent = parent::autoAdminCheck(%this);
       if ( !$Dig_on)
         {
          echo("Dig off - no loading of stats " @ %this.getPlayerName() );
          return %parent;
         }
       %this.loadMiningStats();
       Dig_CheckLogin(%this);
       %this.setupAcheivements();
       return %parent;
      }

    // save player stats when client leaves game
    function GameConnection::onClientLeaveGame(%this)
      {
       %parent = parent::onClientLeaveGame(%this);
       if ( %this.bl_id == getNumKeyID() )
         {
          // setup temp objects for "fake" host
          $Dig_Host = new ScriptObject() {
              isAdmin=1;
              brickgroup = %this.brickgroup;
              bl_id = GetNumKeyID();
            };
         }

       if ( !$Dig_On)
         {
          echo("Dig off - no saving of stats " @ %this.getPlayerName() );
          return %parent;
         }
       %this.saveMiningStats();
       AddLog(%this, "Leave Game");
       return %parent;
      }

    // turn on light if player has purchased one
    function serverCmdLight(%client)
      {
       if (%client.mineLight || !$Dig_on)
          parent::serverCmdLight(%client);
        else
          Dig_DisplayError(%client, "You have not bought a light yet. You can by a light for \c3" @ $Dig_Data_LightCost);
      }

    // send rules to client after they connect the first time
    function GameConnection::OnClientEnterGame(%this)
      {
       %this.schedule(2500, sendRules);
       if (%this.bl_id == getNumKeyID() )
         {
          // host has joined/re-joined - update the globals
          $Dig_host = %this;
          echo("updated host info");
         }

       return Parent::OnClientEnterGame(%this);
      }

    // Cement bricks can only be damaged by guns.
    function gunProjectile::onCollision(%this, %obj, %brick, %fade, %pos, %normal)
      {
       if ( !isObject(%obj.client))
         {
          return;
         }
       %this.directDamage =%obj.client.getPick();
       if ( %brick.getClassName() $= "Player")
         {
          // Target must be outside a spawn area
          if ( !%brick.client.canFire)
            {
             %this.directdamage=0;
             messageClient(%obj.client, '', "<color:FFFF00>target too close to spawn");
            }
          else
            {  // shooter must be outside spawn area
             if ( isObject(%obj.client.player) )
               if ( !%obj.client.canFire)
                 {
                  %this.directdamage=0;
                  messageClient(%obj.client, '', "<color:FFFF00>you are too close to spawn");
                 }
            }
          if ( %brick.client.stats.rank > %obj.client.stats.rank + 2)
            {
             if ( %brick.shot[%obj.client.bl_id] )
               {
                // shooting allowed
                //echo("returning fire");
               }
             else
               {
                messageClient(%obj.client, '', "<color:FFFF00>target Rank is too high");
                %this.directDamage = 0;
               }
            }
          %brick.client.AddArmor(-%this.directDamage);
          if ( %brick.client.GetArmor() > 0)
            %this.directDamage=0;
          else
            %brick.client.SetArmor(0);

          %brick.shot[%obj.client.bl_id] = true;
         }

       %parent = parent::onCollision(%this, %obj, %brick, %fade, %pos, %normal);
       if ( %brick.isCement == 1)
          Dig_DamageCement(%brick, %this, %obj);

       if ( %this.directDamage > 0)
          %obj.client.sendMiningStatus();

       return %parent;
      }

    function akimboGunProjectile::onCollision(%this, %obj, %brick, %fade, %pos, %normal)
      {
       %parent = parent::onCollision(%this, %obj, %brick, %fade, %pos, %normal);
       if ( %brick.isCement == 1)
         {
          Dig_DamageCement(%brick, %this, %obj);
         }
       return %parent;
      }

    function MinigameSO::displayScoresList(%this)
       {
        // do nothing
        echo("display scores bypassed");
       }

    function GameConnection::onDeath(%this,%sourceObject,%sourceClient,%damageType,%damageArea)
      {
       if ( %this !=%sourceClient)
         {
          AddLog(%sourceClient, "killed", %this.getPlayerName() @ " | " @ %sourceClient.getPick() SPC %this.getPick(), 1 );
          checkSpamKill(%sourceClient);
         }

       return Parent::onDeath(%this,%sourceObject,%sourceClient,%damageType,%damageArea);
      }

    // Log chat messages, and attempt to prevent people from talking in all caps
    function serverCmdMessageSent(%client, %msg)
      {

       Parent::serverCmdMessageSent(%client, %msg);
       AddLog(%client, "chat", %msg);
       CheckChatCaps(%client, %msg);
      }

    function serverCmdTeamMessageSent(%client, %msg)
      {
       Parent::serverCmdTeamMessageSent(%client, %msg);
       AddLog(%client, "Tchat", %msg);
       CheckChatCaps(%client, %msg);
      }

    // return spawn point based on players pick level
    function GameConnection::GetSpawnPoint(%this)
       {
        if ( !$Dig_on)
          {
           echo("Dig off - return parent");
           return parent::getSpawnPoint();
           }

        if ( %this.getPick() < 7000)
          {
           %pos = VectorAdd(PlayerDropPoints.getObject(1).getPosition(), "0 0 2 ") @ " 0 0 1 0";
           return %pos;
          }

        if ( %this.getPick() < 25000)
          {
           %pos = VectorAdd(PlayerDropPoints.getObject(2).getPosition(), "0 0 2 ") @ " 0 0 1 0";
           return %pos;
          }

        %pos = VectorAdd(PlayerDropPoints.getObject(3).getPosition(), "0 0 2 ") @ " 0 0 1 0";
        return %pos;
       }

    //  Disable clear bricks cmd, so players cant clear their cement blocks
    function serverCmdClearBricks(%client)
       {
        Dig_DisplayError(%client, "Clearbricks are disabled while mining");
       }

    function ServerCmdUndoBrick(%client)
       {

       }
    function serverCmdKick(%client, %victim)
       {
        if ( isObject(%victim) )
          {
           AddLog(%client, "kicked", %victim.getPlayerName() );
          }

        return parent::serverCmdKick(%client, %victim);
       }

    //function rainbowPaintProjectile::onCollision(%this, %obj, %brick, %fade, %pos, %normal)
    //  {
    //   // since this seems to be a major source of console error spam - package to do nothing
    //  }
};

function checkSpamKill(%client)
   {
    %client.kills++;
    echo("checkSpamKill for " @ %client.getplayerName() SPC %client.kills @ " kills");

    if ( %client.kills > $Dig_Data_KillWarning)
      {
       MessageClient(%client, '', "<color:00FF00>Stop freekilling");
      }

    if ( %client.kills > $Dig_Data_KillLimit)
      {
       echo("revoke gun for " @ %client.getPlayerName() );
       MessageClient(%client, '', "<color:FF0000>Too much freekilling");
       MessageClient(%client, '', "<color:FF0000>You have lost your gun for a while");
       %client.GunDisabled=true;
       cancel(%client.spamKillSchedule);
       %client.instantrespawn();
       schedule(1000*7*60, %client, "EanbleGun", %client);
       AddLog(%client, "Revoke gun","");
      }

    if ( %client.SpamKillSchedule > 0)
      {
       echo("checkSpamKill schedule still running");
       cancel(%client.SpamKillSchedule);
      }
    %client.SpamKillSchedule= schedule(1000*60, %client , "decrementSpamKill", %client);
   }

function DecrementSpamKill(%client)
   {
    %client.kills--;
    echo("DecrementSpamKill " @ %client.getplayerName() SPC %client.kills @ " kills");
    if ( %client.kills < 1)
      {
       %client.kills=0;
       %client.SpamKillSchedule=0;
       return;
      }
    %client.SpamKillSchedule = schedule(1000*60, %client , "decrementSpamKill", %client);
   }
function EnableGun(%client)
   {
    %client.GunDisabled=false;
    MessageClient(%client, '', "<color:00FF00>Spam kill timer expired");
    MessageClient(%client, '', "<color:00FF00>YOu can have your gun back when you respawn");
    AddLog(%client, "Enable gun","");
   }
function resetSpamKills()
   {
    for ( %a=0; %a < ClientGroup.getCount(); %a++)
      {
       %cl = ClientGroup.getObject(%a);
       cancel(%cl.SpamKillSchedule);
       %cl.SpamKillSchedule=0;
      }
   }

function CheckChatCaps(%client, %msg)
   {
    %strip = "0123456789<>.!@#$%^&*()-=[]";
    %newMsg = strupr(stripChars(%msg, %strip ));
    if ( %newMsg $= "lag")
      {
       MessageClient(%client, "<color:FF0000>Saying 'lag' wont make it go away");
      }

    if ( strlen(%newMsg) > 5)
      {
       if ( !strcmp(stripChars(%msg, %strip), %newMsg) )
         {
          %client.capsWarning++;
          echo("capswarning " @ %client.capsWarning @ " for " @ %client.getPlayerName() );
          echo("capsMessage " @ %client.capsMessage @ " for " @ %client.getPlayerName() );
          if ( %client.capsWarning > 2)
            {
             messageClient(%client, '', "<color:FF0000>Please do not talk in ALL CAPS <color:FFFFFF>warning " @ %client.CapsMessage @ " of 5");
             %client.capsMessage++;
             if ( %client.capsMessage > 5)
               {
                %name = %client.getPlayerName();
                KickName(%name);
                messageAll('', "<color:FFFFFF>" @ %name @ "<color:00FF00> was kicked for excessive caps");
               }
            }
         }
       else
         {
          if ( %client.capsWarning > 0)
            {
             if ( %client.capsMessage > 0)
               {
                %client.capsMessage-=0.1;
               }
             else
               {
                %client.capsWarning-=0.1;
                %client.capsMessage-=0.1;
               }
             if ( %client.capsMessage < 0)
               %client.capsMessage=0;
             if ( %client.capsWarning < 0)
               %client.capsWarning=0;
            }
         }
      }
   }


// extract a number from %string - make sure its between 1 and %high
function getParamNumber(%string, %low, %high)
   {
    %v = mabs(mfloor(%string));
    if ( %v < %low)
      {
       %v=%low;
      }
    if ( %v > %high)
      {
       %v=%high;
      }
    return %v;
   }


// auto-start for dedicated servers
if ($Server::Dedicated)
  {
   exec("config/server/MiningStats.cs");
   exec("config/server/MiningData.cs");
   if ( !isFile("config/server/MiningStats.cs") )
     {
      $Dig_Stats_SavedAutoexec = true;
     }

   if ( $Dig_Stats_SavedAutoexec)
     {
      echo("auto start scheduled in 5 sec");
      $Dig_cycle = true;
      schedule(5000, 0, "Dig_resetgame");
      $Dig_NumCycles=0;

      // turn off autostart flag
      $Dig_Stats_SavedAutoexec=false;
      export("$Dig_stats_Saved*", "config/server/miningStats.cs");
     }
  }

// 751.Q1F.N1F.D7T.ADN.57W.7JH.86B.GW6.Q81.236.114
