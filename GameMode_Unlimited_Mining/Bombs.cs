 // Bombs.cs
// Bombs for the unlimited mining mod

if (ForceRequiredAddOn("Vehicle_Tank") == $Error::AddOn_NotFound)
  {
   error("ERROR: GameMode_UnlimitedMining - required add-on Vehicle_Tank not found");
   error("ERROR: GameMode_UnlimitedMining - Bombs are disabled");
   $Dig_Data_BombSizeMax=0;
   $Dig_Data_BombSizeBuyLimit=0;
   return;
  }

if (isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
  {
   if (!$RTB::RTBR_ServerControl_Hook)
     exec("Add-Ons/System_ReturnToBlockland/RTBR_ServerControl_Hook.cs");

   RTB_registerPref("Bomb Cost","Unlimited Mining","$Dig_Data_BombCost","int 0 5000","GameMode_UnlimitedMining",200,0,0);
   RTB_registerPref("Bomb Size Max","Unlimited Mining","$Dig_Data_BombSizeMax","int 0 100","GameMode_UnlimitedMining",35,0,0);
   RTB_registerPref("Bomb Size Buy Limit","Unlimited Mining","$Dig_Data_BombSizeBuyLimit","int 0 100","GameMode_UnlimitedMining",20,0,0);
   RTB_registerPref("Domant Bombs","Unlimited Mining","$Dig_Data_DormantBombs","bool","GameMode_UnlimitedMining",true,0,0,"ToggleDormantBombs");
  }
else
  {
   $Dig_Data_BombCost=200;
   $Dig_Data_BombSizeMax=35;
   $Dig_Data_BombSizeBuyLimit=20;
   $Dig_Data_DormantBombs=true;
  }

if ( !isObject($Dig_BombOre) )
  {
   $Dig_BombOre= new ScriptObject(MineBomb)
      {
       name="Dormant Bomb";
       value=0;
       color=11;
       minPercent=1;
       maxPercent=1.05;
       depth=-75;
       colorfx=3;
      };
   if ( $Dig_Data_Dormantbombs)
     $Dig_BombOre.disabled=false;
   else
     $Dig_BombOre.disabled=true;

   OreData.add($Dig_BombOre);
  }

function MineBomb::CheckPlace(%this, %pos, %random)
   {
    if ( %this.disabled==1)
      return;

    return MineOre::CheckPlace(%this, %pos, %random);
   }

function MineBomb::PlaceAt(%this, %pos)
   {
    %health= 500;
    return PlaceBrick(%pos, %health, %this.color, %this.colorFx, %this.ShapeFx, %this);
   }

function Minebomb::getMineName(%this, %brick)
   {
    return MineOre::getMineName(%this);
   }

function MineBomb::mined(%this, %client, %brick, %mode)
   {
    MineOre::mined(%this, %client, %brick, %mode);
    if ( %mode $= "bomb" || %mode $= "drill" || %mode $= "power")
      {
       return;
      }

    if ( %brick.health < 1)
      {
       if ( %brick.AlreadyMined==1) // prevent spamming in a laggy server
         return;

       %brick.alreadyMined=1;
       AddLog(%client, "mined bomb | mode " @ %mode, %size, 1);
       echo("mined bomb mode: " @ %mode);
       // blow up
       %brick.health=1;
       %size = getRandom(1,10);
       %pos = %brick.getPosition();

       //if ($Dig_Data_BombSizeBuyLimit==0)
       //  return;

       dig_DoBombAnyway(%brick, %client, mFloor(mabs(%size)) );
      }
   }

//re-use the tank projectile with a few tweaks
datablock ProjectileData(Dig_BombProjectile)
  {
   projectileShapeName = "Add-ons/Vehicle_Tank/tankBullet.dts";
   directDamage        = 0;
   directDamageType = $DamageType::TankShellDirect;
   radiusDamageType = 0;
   impactImpulse	   = 1000;
   verticalImpulse	   = 1000;
   explosion           = tankShellExplosion;
   particleEmitter     = rocketTrailEmitter;

	 // dont destroy any bricks with this explosion
   brickExplosionRadius = 0;
   brickExplosionImpact = false;
   brickExplosionForce  = 0;
   brickExplosionMaxVolume = 0;
   brickExplosionMaxVolumeFloating = 0;

   sound = rocketLoopSound;

   muzzleVelocity      = 120;
   velInheritFactor    = 1.0;

   armingDelay         = 0;
   lifetime            = 4000;
   fadeDelay           = 4000;
   bounceElasticity    = 0.5;
   bounceFriction      = 0.20;
   isBallistic         = true;
   gravityMod = 1.0;

   hasLight    = true;
   lightRadius = 5.0;
   lightColor  = "1 0.5 0.0";

   explodeOnDeath = 1;

   uiName = "Dig Bomb";
  };


function buyBomb(%client, %size)
   {

    // %client buys a bomb
    if ( %size < 1)
      {
       %size = 1;
      }
    if ($Dig_Data_BombSizeBuyLimit==0)
       {
        Dig_DisplayError(%client, "Bombs are disabled");
        return;
       }

    if ( %size > ($Dig_Data_BombSizeBuyLimit+ mabs(%client.stats.rank/2) ) || %size < 1)
      {
       Dig_DisplayError(%client, "Bomb too big.  please dont lag the server");
       return;
      }
    if ( %client.bomb > 0)
      {
       Dig_DisplayError(%client, "You already have a bomb");
       return;
      }
    %client.bombCount=0;
    %price = mFloor($Dig_Data_BombCost * ( (4/3) * 3.14159265 * %size * %size * %size) );
    if ( %client.getMoney().lessThan(%price) )
      {
       Dig_DisplayError(%client, "You cannot afford a size " @ %size @ " bomb for \c3$" @ AddComma(%price), 3);
       return;
      }
    %client.AddMoney( -%price, "Buy Bomb " @ %size, 1);
    %client.bomb = %size;
    if ( %size > 1)
      {
       messageClient(%client, '', "<color:00FF00>You bought a size "@ %size @ " Bomb for \c3$" @ AddComma(%price) );
      }
    %client.sendMiningStatus();
   }

function serverCmdBomb(%client, %size)
   {
    if ( %size > 0)
      {
       // Normal bomb purchase
       return servercmdBuy(%client, "bomb", %size);
      }

    // turn on the cancel flag
    %client.CancelBomb=1;
    messageClient(%client, '', "<color:FFFF00>Bomb Cancelled");
    %client.bomb=0;
    %client.bombCount=0;
   }

function Dig_doBomb(%brick, %client, %level)
   {
    if ( %client.CanTriggerBomb==1)
      {
       Dig_DisplayError(%client, "ERROR<color:FFFFFF>Cant trigger another bomb yet");
       return;   // spam protection
      }
    %pos = %brick.getPosition();
    for ( %a=0; %a < PlayerDropPoints.getcount(); %a++)
      {
       %spawn = PlayerDropPoints.getObject(%a);
       %pos2 = VectorSub(%spawn.position, %pos);
       if ( VectorLen(%pos2) < 30 + %client.bomb)
         {
          Dig_DisplayError(%client, "ERROR<color:FFFFFF>Bomb Too close to spawn");
          return;
         }
      }

    Dig_DoBombAnyway(%brick, %client, %level);
   }

function dig_DoBombAnyway(%brick, %client, %level)
   {
    AddLog(%client, "trigger bomb " , %level, 1);

    %client.CancelBomb=0;
    %client.bomb=0;
    %center = %brick.getPosition();

    // first make the explosion
    %p = new Projectile()
      {
       dataBlock = Dig_BombProjectile;
       initialPosition = %center;
       initialVelocity = "0 0 0";
       sourceObject = 0;
       client = %obj.client;
       sourceSlot = 0;
      };
    MissionCleanup.add(%p);

    %p.explode();

    // then make a "hole" in the bricks

    schedule(10, 0, "Dig_ExplodeBlock", %brick, %center, %client, (%level*2) + 1, (%level*2) + 1);

    %client.CanTriggerBomb=1;
    %client.schedule(1000*30,"Dig_EnableBomb");  // max 1 bomb per min
    commandToClient(%client, 'CenterPrint', "", -1);
   }

function GameConnection::Dig_enableBomb(%client)
   {
    %client.CanTriggerBomb=0;
    messageClient(%client, '', "<color:FFFF00>You can now trigger another bomb");
   }

function Dig_explodeBlock(%brick, %pos1, %client, %level, %final)
   {
    // "explode" the block at %position, and mine the 6 bricks around it
    if ( !$Dig_On)
      {
       return;  // dig mode is off, cancel exploding bombs
      }
    if ( %level < 1 || %client.cancelBomb == 1)
      {
       return;
      }
    if ( !isObject(%brick) )
      {
       return;
      }
    if ( !%brick.isMineable)
      {
       return;
      }
    if ( %brick.isBombable $= "0")
      {
       return;
      }
    if ( !isObject(%client))
      {
       return;
      }

    %pos = %brick.getPosition();
    Dig_MineBrick(%brick, %pos, %client, "bomb");

    %x = getWord(%pos, 0);
    %y = getWord(%pos, 1);
    %z = getWord(%pos, 2);
    %position[0] = %x+2 SPC %y SPC %z;
    %position[1] = %x-2 SPC %y SPC %z;
    %position[2] = %x SPC %y+2 SPC %z;
    %position[3] = %x SPC %y-2 SPC %z;
    %position[4] = %x SPC %y SPC %z+2;
    %position[5] = %x SPC %y SPC %z-2;
    for(%a = 0; %a < 6; %a++)
      {
       if ( $Dig_PlacedDirt[%position[%a]] > 2)
         {
          %dist = VectorDist(%position[%a], %pos1);
          if ( %dist < %final)
            {
             schedule(200,0,"Dig_explodeBlock", $Dig_PlacedDirt[%position[%a]] , %pos1, %client, %level-1, %final );
            }
         }
      }
   }

