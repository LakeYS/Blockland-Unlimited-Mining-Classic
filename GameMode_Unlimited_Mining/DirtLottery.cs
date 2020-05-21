// Unlimited Mining -- Dirt lottery
// Lotto bricks give random rewards
// The magnitude of the reward is based on depth


if (isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
  {
   if (!$RTB::RTBR_ServerControl_Hook)
     exec("Add-Ons/System_ReturnToBlockland/RTBR_ServerControl_Hook.cs");

	 RTB_registerPref("Lotto Blocks","Unlimited Mining","$Dig_Data_LottoBlocksEnabled","bool","GameMode_Unlimited_Mining",true,0,0,"ToggleLottoBlocks");
  }
else
  {
   $Dig_Data_LottoblocksEnabled=true;
  }

if ( !isObject($Dig_LottoBlock) )
  {
   $Dig_LottoBlock = new ScriptObject(MineLotto)
       {
        name="Lotto Block";
        value=0;
        color=4;
        minPercent=0;
        maxPercent=0.1;
        depth=0;
        colorfx=3;
        radioactive=false;
       };
   OreData.add($Dig_LottoBlock);
  }

if ( !isObject($Dig_RadioLottoBlock) )
  {
   $Dig_RadioLottoBlock = new ScriptObject(RadioactiveLotto)
     {
      name="Radioactive Lotto Block";
      value=0;
      color=4;
      minPercent=0.2;
      maxPercent=0.3;
      depth=-10000;
      colorfx=3;
      shapefx=1;
      radioactive=true;
     };
   OreData.add($Dig_RadioLottoBlock);
  }

// return true of this block should be placed
function mineLotto::CheckPlace(%this, %pos, %random)
   {
    if ( %this.disabled==1)
      return false;
    //echo("check place lotto");

    return MineOre::CheckPlace(%this, %pos, %random);
   }
function RadioactiveLotto::CheckPlace(%this, %pos, %random)
   {
    return mineLotto::CheckPlace(%this, %pos, %random);
   }

// place the block
function MineLotto::placeAt(%this, %pos, %random)
   {
    %z = mabs(getword(%pos, 2) - $Brick_Z);
    if ( %z < 1)
      %z=1;

    // amount of hp for lotto blocks.  -- very high so players dont accidently mine them
    %health = mfloor(getRandom(50,150)*%z*0.5);
    return PlaceBrick(%pos, %health, %this.color, %this.colorFx, %this.ShapeFx, %this);
   }
function RadioactiveLotto::placeAt(%this, %pos, %random)
   {
    %brick = MineLotto::placeAt(%this, %pos, %random);

    %brick.multiplier=getRandom(5,20);
    %brick.health*=%brick.multiplier;
    %brick.radioactive=true;
    //echo("spawn radioactive lotto: " @ %pos);
    return %brick;
   }

function MineLotto::getMineName(%this)
   {
    return "<color:11FFAA>" @ %this.name;
   }
function RadioactiveLotto::getMineName(%this)
   {
    return "<color:FFFF00>" @ %this.name;
   }

// Mine a lotto brick
function MineLotto::mined(%this, %client, %brick, %mode)
   {
    %health=$modeHealth[%mode];

    if ( %health > 0)
      {
       %brick.health -= %client.getPick();
			 }
    %brick.health*= %health;

    // if the brick is dead - give rewards
    if (( %mode !$= "bomb") && (%mode !$="drill") )
    if ( %brick.health < 1)
      {
       %this.LottoReward(%brick, %client);
       if ( isObject(%client))
           %client.AddOreLog(%this, 0 , true);
      }
    }

// based on whats in %dirtRecord - make a new ore block
function RuinsLotto::placeContinue(%this, %pos, %rad, %radMult)
   {
    //echo("RuinsLotto::placeContinue " @ %pos @ " rad: " @ %rad @ " radmult: " @ %radMult);
    return %this.placeAt(%pos, 0);
   }
function RuinsLotto::placeAt(%this,%pos, %random)
   {
    %mult = getRandom(100,200);

    %brick = PlaceBrick(%pos, %this.value * 4, %this.color, %this.colorFx, %this.ShapeFx, %this);
    %brick.multiplier= %Mult*40;
    %brick.health = 10000 * %brick.multiplier;

    %brick.radioactive=true;
    //echo("RuinsLotto::placeAt " @ %pos @ " radmult: " @ %brick.multiplier);
    return %brick;
   }

function RuinsLotto::getMineName(%this)
   {
    return "<color:FFFF00>" @ %this.name;
   }

function RadioactiveLotto::mined(%this, %client, %brick, %mode)
   {
    %health=$modeHealth[%mode];
    if ( %health > 0)
      {
       if ( %brick.warning[%client.bl_id] )
         {
          %brick.health -= %client.getPick();
         }
       else
         {
          RadioWarning(%client, %brick);
          return;
         }
      }
    %brick.health*= %health;

    // if the brick is dead - give rewards
    if (( %mode !$= "bomb") && (%mode !$="drill") )
      if ( %brick.health < 1)
        {
         %this.LottoReward(%brick, %client);
         if ( isObject(%client) )
           %client.AddOreLog(%this, 0 , true);

         MineRadioactive(%this, %client, %brick, %mode);
        }
   }

// display a warning message to the client -- radioactive lotto blocks are hazardous
function radioWarning(%client, %brick)
   {
    %client.radioBrick=%brick;
    commandToClient(%client,'MessageBoxYesNo', "RADIOACTIVE LOTTO", "Radioactive lottos are DANGEROUS\n Are you sure?", 'confirmRadio');
   }

function serverCmdConfirmRadio(%client)
   {
    %brick = %client.radioBrick;
    %brick.warning[%client.bl_id] = true;
    %client.radioBrick=0;
   }

function RuinsLotto::mined(%this, %client, %brick, %mode)
   {
    %health=$modeHealth[%mode];
    if ( %health > 0)
      {
       %brick.health -= %client.getPick();
      }
    %brick.health*= %health;

    // if the brick is dead - give rewards
    if (( %mode !$= "bomb") && (%mode !$="drill") )
      if ( %brick.health < 1)
        {
         %this.LottoReward(%brick, %client);
         if ( isObject(%client) )
           %client.AddOreLog(%this, 0 , true);

         MineRadioactive(%this, %client, %brick, %mode);
        }
   }

function RuinsLotto::CheckPlace(%this, %pos, %random)
   {
    return false;
   }


// player has mined a lottery block - give an "interesting" reward
function MineLotto::LottoReward(%this, %brick, %client)
   {
    %reward = getrandom(1,11);
    //echo("lotto reward: " @ %reward @ " for " @ %client.getPlayerName() );

     switch ( %reward)
       {
        case 1:
           Dig_DeathReward(%client, %brick);
        case 2:
          Dig_CashReward(%client, %brick, 0.5);
        case 3:
          Dig_HeatsuitReward(%client, %brick, 0.5);
        case 4:
          Dig_RadSuitReward(%client, %brick, 0.5);
       case 5:
          Dig_DirtReward(%client, %brick, 0.5);
       case 6:
          Dig_bombReward(%client, %brick);
       case 7:
          Dig_NoReward(%client, %brick);
       case 8:
           Dig_Pickreward(%client, %brick);
       case 9:
          Dig_GlobalReward(%client, %brick, 1);
       case 10:
          Dig_LottoOre(%client, %brick, 1);
       case 11:
          Dig_LottoTeleport(%client, %brick, 1);
       }
   }

// player has mined a lottery block - give an "interesting" reward
function RadioactiveLotto::LottoReward(%this, %brick, %client)
   {
    %reward = getrandom(1,11);
    //echo("rad lotto reward: " @ %brick.multiplier @ " for " @ %client.getPlayerName() );
    switch ( %reward)
      {
       case 1:
          Dig_RadioDeathReward(%client, %brick);
       case 2:
          Dig_CashReward(%client, %brick, %brick.multiplier);
       case 3:
          Dig_RadioPickreward(%client, %brick, %brick.multiplier);
       case 4:
          Dig_RadSuitReward(%client, %brick, %brick.multiplier);
       case 5:
          Dig_DirtReward(%client, %brick, %brick.multiplier);
       case 6:
          Dig_RadiobombReward(%client, %brick, %brick.multiplier);
       case 7:
          Dig_HeatsuitReward(%client, %brick, %brick.multiplier);
       case 8:
          Dig_GlobalReward(%client, %brick, %brick.multiplier);
       case 9  :
          Dig_LottoDiscoverOre(%client, %brick, %brick.multiplier);
       case 10 :
          Dig_LottoTeleport(%client, %brick, %brick.multiplier*0.5);
       case 11 :
          Dig_LottoInsurance(%client, %brick, %brick.multiplier);
      }
   }

// player has mined a lottery block - give an "interesting" reward
function RuinsLotto::LottoReward(%this, %brick, %client)
   {
    %reward = getrandom(1,11);
    //echo("ruins lotto reward: " @ %brick.multiplier @ " for " @ %client.getPlayerName() );
    switch ( %reward)
      {
       case 1:
          Dig_CashReward(%client, %brick, %brick.multiplier);
       case 2:
          Dig_RadSuitReward(%client, %brick, %brick.multiplier);
       case 3:
          Dig_DirtReward(%client, %brick, %brick.multiplier);
       case 4:
          Dig_bombReward(%client, %brick);
       case 5:
          Dig_HeatsuitReward(%client, %brick, %brick.multiplier);
       case 6:
          Dig_GlobalReward(%client, %brick, %brick.multiplier);
       case 7  :
          Dig_LottoDiscoverOre(%client, %brick, %brick.multiplier);
       case 8 :
          Dig_LottoTeleport(%client, %brick, %brick.multiplier*0.5);
       case 9 :
          Dig_LottoInsurance(%client, %brick, %brick.multiplier);
       case 10 :
          Dig_LottoRemoveInsurance(%client, %brick, %brick.multiplier);
       case 11 :
          Dig_LottoRankReward(%client, %brick, %brick.multiplier);
      }
   }

function Dig_LottoRankReward(%client, %brick, %level)
   {
    //Add/subtract 1-5 ranks from player
    %client.stats.Dig_LottoRank++;
    if ( %client.stats.Dig_LottoRank > 1)
      %client.stats.Dig_LottoRank=0;

    %amount = getRandom(1,5);
    if ( %client.stats.Dig_LottoRank==0)
      { }
    else
      %amount = 0-%amount;

    // Insurance cannot protect from de-ranking
%amount = Dig_LottoMessage(%client, "picks", %amount, true, 5*(%client.getRank()+1) );
    %amount = mFloor(%anAmount);

    AddLog(%client, "Ruins Lotto" , %rank @ " | " @ %amount, 1 );

    if ( %amount > 0)
      {
       messageClient(%client, '', "<color:0000FF>" @ %type @ ":<color:00FF00>You gained " @ %amount @ " ranks");
      }
    else
      {
       messageClient(%client, '', "<color:0000FF>" @ %type @ ":<color:00FF00>You LOST " @ %amount @ " ranks");
       messageClient(%client, '', "<color:0000FF> Insurance cannot protect you from this");
      }
    %oldRank = %client.rank;
    %client.stats.rank += %amount;

    if ( %client.getRank < 1)
      {
       %client.rank = 0;
      }
    if ( %oldRank == %client.getRank() )
      {
       return;
      }
    if ( %amount > 0)
      {
       MessageAll('', "<color:00FF00>Rank UP " @ %client.getPlayerName() @ " is now rank " @ %client.GetRank() );
      }
    else
      {
       MessageAll('', "<color:00FF00>Rank DOWN " @ %client.getPlayerName() @ " is now rank " @ %client.GetRank() );
      }
   }

function Dig_LottoRemoveInsurance(%client, %brick, %level)
   {
    //echo("Dig_LottoInsurance " @ %client.getPlayerName() SPC %brick SPC %level);

    echo("Dig_LottoRemoveInsurance " @ %client @ " %brick: " @ %brick @ " level: " @ %level);
    messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00> cancels your insurance");
    %client.insurance=0;

    AddLog(%client, "R Lotto", "cancel insurance", 1);
   }

function Dig_LottoInsurance(%client, %brick, %level)
   {
    //echo("Dig_LottoInsurance " @ %client.getPlayerName() SPC %brick SPC %level);
    %client.stats.Dig_RadLottoInsurance++;
    if ( %client.stats.Dig_RadLottoInsurance > 1)
      %client.stats.Dig_RadLottoInsurance=0;

    if ( %client.stats.Dig_radLottoInsurance == 1)
      {
       Dig_Noreward(%client, %brick, %level);
       return;
      }
    echo("Dig_LottoInsurance " @ %client @ " %brick: " @ %brick @ " level: " @ %level);
    %pos = %brick.getPosition();
    %amount = mabs(getWord(%pos, 2) ) * 8 * %level;
    messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00> Eats " @ %amount @ " of your insurance");
    %client.insurance-=%amount;

    AddLog(%client, "R Lotto", "insurance | " @ %amount, 1);
   }


// change the value of the players discovered ore
function Dig_LottoOre(%client, %brick, %level)
   {
    for ( %a=0; %a < OreData.getCount(); %a++)
      {
       %newOre = OreData.getObject(%a);
       if ( %newOre.bl_id == %client.bl_id)
         {
          %amount = mFloor(mAbs(getWord(%brick.getPosition(), 2)) / 50) ;

          if ( %client.stats.Dig_LottoOre == 0)
            {
             messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>RAISES the value of your " @ %newOre.Name @ " by " @ %amount);
             %newOre.value += %amount;
            }
          else
            {
             messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>LOWERS the value of your " @ %newOre.Name @ " by " @ %amount);
             %amount = %client.checkInsurance(%amount, 500);
             %newOre.value -= %amount;
            }
          AddLog(%client, "Ore Value", %amount);
          %client.stats.Dig_LottoOre++;
          if ( %client.stats.Dig_LottoOre > 1)
            %client.stats.Dig_LottoOre=0;

          return;
         }
      }
    Dig_Noreward(%client, %brick, %level);
   }

// Discover ore on positive result
function Dig_LottoDiscoverOre(%client, %brick, %level)
   {
    %client.stats.Dig_LottoDiscover++;
    if ( %client.stats.Dig_LottoDiscover > 1)
      %client.stats.Dig_LottoDiscover=0;

    if ( %client.stats.Dig_LottoDiscover == 1)
      {
       %found = false;
       for ( %a=0; %a < OreData.getCount(); %a++)
         {
          %newOre = OreData.getObject(%a);
          if ( %newOre.bl_id == %client.bl_id)
            {
             %found = true;
            }
         }
       if ( !%found)
         {
          AddLog(%client, "Lotto", "Discover Ore", 1);
          messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>Inside the lotto brick you find a NEW ORE!");
          Dig_DiscoverOre(%client);
          return;
         }
      }
    Dig_NoReward(%client, %brick, %level);
   }

function Dig_LottoTeleport(%client, %brick, %level)
   {
    // teleport player in a random direction
    // %level = number of bricks to move
    if ( isObject(%brick) )
      echo("lotto brick exists");
    else
      error("lotto brick does NOT exist");

    %client.stats.Dig_Teleport++;
    if ( %client.stats.Dig_Teleport > 1)
      %client.stats.Dig_Teleport=0;

    %pos = %brick.getPosition();
    echo("startig pos: " @ %pos);
    %level = mFloor(%level * getWord(%pos, 2) * 0.5);
    if ( mabs(%level) > (mabs($Dig_Data_MaxDepth) + mabs($Dig_Data_MaxHeight)))
      {
       %level = mabs($Dig_Data_MaxDepth) + mabs($Dig_Data_MaxHeight);
      }
    if ( %client.stats.Dig_Teleport == 1)
      {
       %level = -%level;
      }
    %level = mFloor(%level/2) * 2; // make sure level is an even number
    %dir = getRandom(1,3);
    switch ( %dir )
      {
       case 1 :
         %offset = %level SPC "0 0";
       case 2 :
         %offset = "0 " @ %level @ " 0";
       case 3 :
         %offset = "0 0 " @ %level;
      }

    %newPos = VectorAdd(%pos, %offset);
    %z = getWord(%newPos, 2);
    echo("level: " @ %level @ " offset: " @ %offset @ " dir: " @ %dir);
    if (%z < $Dig_Data_MaxDepth)
      {
       echo("Adjusting for too deep of a teleport");
       %newPos = getWord(%pos, 0) SPC getWord(%pos, 1) SPC $Dig_DataMaxDepth + 10;
      }
    if (%z > $Dig_Data_MaxHeight)
      {
       echo("Adjusting for too high of a teleport");
       %newPos = getWord(%pos, 0) SPC getWord(%pos, 1) SPC $Dig_DataMaxHeight - 10;
      }
    if ( %pos $= %newPos)
      {
       MessageClient(%client, "<color:0000FF>Lotto Brick:<color:00FF00>Teleport malfunction");
       error("cant teleport to same position");
       return;
      }
    messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>Teleports you " @ mAbs(%level) @ " units away");
    AddLog(%client, "Lotto", "Teleport from " @ %pos @ " to " @ %newPos, 1);
    TeleportPlayer(%client, %newPos);
    if ( isObject(%brick) )
      echo("lotto brick exists (end of LottoTeleport)");
    else
      error("lotto brick does NOT exist (end of LottoTeleport)");
   }

// teleport client to the given pos, encased in bricks
function TeleportPlayer(%client, %newPos)
   {
    if ( !isObject(%client.player) )
      {
       Dig_DisplayError(%client, "Teleport fail -- no player object");
       return;
      }
    %newBrick = $Dig_placedDirt[%newPos];
    if ( !isObject( %newBrick) )
      {
       Dig_PlaceDirt(%newPos);
       %newBrick = $Dig_placedDirt[%newPos];
       %newBrick.health=0;
      }

    Dig_mineBrick(%newBrick, %newPos, %client);

    %newPos1 = VectorAdd(%newPos, "0 0 2");
    %newbrick1 = $Dig_placedDirt[%newPos1];
    if ( isObject(%newBrick1) )
      {
       %newBrick1.health=0;
       Dig_mineBrick(%newbrick1, %newbrick1.getPosition(), %client);
      }

    %client.player.schedule(100, setTransform, %newPos @ " 0 0 1 0" );
   }

function Dig_GlobalReward(%client, %brick, %level)
   {
    %client.stats.Dig_LottoBuff++;
    if ( %client.stats.Dig_LottoBuff > 1)
      %client.stats.Dig_LottoBuff=0;

    %amount = %level + 1;
    if ( %amount > 20)
      {
       %amount = 20;
      }
    if ( %client.stats.Dig_LottoBuff == 1)
      {
       if ( $ModeMoney["normal"] < 0)
         {
          $ModeMoney["normal"]=1;
         }

       $ModeMoney["normal"] *= %amount;
       if ( $ModeMoney["normal"] == 0)
         {
          $ModeMoney["normal"] = %amount;
         }
       $ModeMoney["bomb"]   = 0;
       messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>All ores are worth " @ $ModeMoney["normal"] @ "x $$ for " @ %amount @ " min");
       messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Found a lotto block buff - all ores are worth " @ $ModeMoney["normal"] @ "x for "  @ %amount @ " mins");
       if ( %level > 2)
         AddLog(%client, "R Lotto", "Buff | x" @ $ModeMoney["normal"] @" for " @ %amount, 1);
       else
         AddLog(%client, "Lotto", "Buff | x" @ $ModeMoney["normal"] @" for " @ %amount, 1);
       $Dig_bombSave = $Dig_Data_BombSizeBuyLimit;
       $Dig_Data_BombSizeBuyLimit = 0;
      }
    else
      {
       $ModeMoney["normal"]=0;
       $ModeMoney["power"]=0;
       $ModeMoney["bomb"]=0;
       $ModeMoney["drill"]=0;
       if ( %level > 1)
         {
          // R lotto buff > 10 gives a negative buff
          if ( %level > 9)
            {
             %amount = mFloor(%amount / 2);
             AddLog(%client, "Lotto", "Negative Buff | x-" @ %amount @ " for " @ %amount, 1);
             messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>All ores TAKE MONEY for " @ %amount @ " min");
             messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Found a lotto block buff - all ores TAKE MONEY for " @ %amount @ " mins");
             $ModeMoney["normal"]=(0 - %amount);
            }
          else
            {
             AddLog(%client, "R Lotto", "Buff | x0 for " @ %amount, 1);
             messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>All ores are worth NOTHING for " @ %amount @ " min");
             messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Found a lotto block buff - all ores are worth NOTHING for " @ %amount @ " mins");
            }
         }
       else
         {
          messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>All ores are worth NOTHING for " @ %amount @ " min");
          messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Found a lotto block buff - all ores are worth NOTHING for " @ %amount @ " mins");
          AddLog(%client, "Lotto", "Buff | x0 for " @ %amount, 1);
         }
      }

    if ( $Dig_Buff > 0)
      {
       cancel($Dig_Buff);
       $Dig_buff = 0;
      }

    ScheduleMin(%amount, "Dig_RestoreMoney");

    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      {
       %client = ClientGroup.getObject(%c);
       if (%client.hasSpawnedonce )
         {
          %client.sendMiningStatus();
         }
      }
   }

// setup a schedule for a number of minuites.  call %func when done
function ScheduleMin(%mins, %func)
   {
    //echo("scheduleMin " @ %mins SPC %func);
    if ( %mins < 1)
      {
       schedule(0, 0, %func);
       $Dig_Buff = 0;
       return;
      }
    MessageAll('', "<color:00FF00>buff timer has " @ %mins @ "min left");
    $Dig_Buff = schedule(1000*60, 0, "ScheduleMin", %mins-1, %func);
   }

// restore the lotto "buff" back to its origional state
function Dig_RestoreMoney()
   {
    $ModeMoney["normal"]=1;
    $ModeMoney["power"]=0;
    $ModeMoney["bomb"]=-1;
    $ModeMoney["drill"]=0;
    messageAll('', "<color:00FF00>All ores are now worth their original amount");

    for (%c = 0; %c < ClientGroup.getCount(); %c++)
      {
       %client = ClientGroup.getObject(%c);
       if (%client.hasSpawnedonce )
         {
          %client.sendMiningStatus();
         }
      }
    if ( $Dig_BombSave > 0)
      {
       $Dig_Data_BombSizeBuyLimit = $Dig_bombSave;
       echo("$Dig_bombSave " @ $Dig_bombSave);
       echo("$Dig_Data_BombSizeBuyLimit set to " @ $Dig_Data_BombSizeBuyLimit);
       if ( $Dig_Data_BombSizeBuyLimit == 0)
         {
          $Dig_Data_BombSizeBuyLimit=15;
          error("bomb size limit reset to 15");
         }
      }
   }

function Dig_Noreward(%client, %brick)
   {
    switch ( getRandom(1,8) )
      {
       case 1:
         %msg = "The lotto block gives you a message on your screen";
       case 2:
         %msg = "Lotto block growls at you";
       case 3:
         %msg = "The lotto block says <color:FFFFFF>hi ";
       case 4 :
         %msg = "The lotto block says <color:FFFFFF>I'll get you next time";
       case 5 :
         %msg = "The lotto block says <color:FFFFFF>Do you mind? I was sleeping";
       case 6 :
         %msg = "Lotto block Spits on you.....";
       case 7 :
         %msg = "Lotto block SLAPS you with a wet fish";
       case  8:
         %msg = "Lotto block ties you up and tickles you";
      }

    messageClient(%client, '', "<color:00FF00>" @ %msg);
   }

// lotto block blows up
function Dig_BombReward(%client, %brick)
   {
    %client.stats.Dig_LottoBomb++;
    if ( %client.stats.Dig_LottoBomb > 1)
      %client.stats.Dig_LottoBomb=0;

    // Give / explode a bomb based on depth
    %pos = %brick.getPosition();
    %z= getWord(%pos, 2) - $Brick_Z;

    if ( %z > 0)
      {
       %size = $Dig_Data_BombSizeMax * ( %z / $Dig_Data_Maxheight);
      }
    if ( %z < 0)
      {
       %size = $Dig_Data_BombSizeMax * ( %z / $Dig_Data_MaxDepth);
      }
    %size=mfloor(mabs(%size));
    if ( %size < 1)
      %size = 1;

    AddLog(%client, "Lotto", "bomb | " , %size, 1);
    if ( %client.stats.Dig_Lottobomb)
      {
       %client.bomb=%size;
       messageClient(%client, '', "<color:00FF00>the lotto block gave you a level " @ %size @ " bomb");
      }
    else
      {
       %brick.health=1;
       dig_DoBombAnyway(%brick, %client, %size+1);
       if ( %client.checkInsurance( (%size+1) * $Dig_Data_BombCost, $Dig_Data_BombCost) == 0)
         {
          MessageClient(%client, '', "<color:0000F0>Lotto Insurance:<color:FFFFFF> Saved you from being killed");
         }
       else
         {
          messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Was killed by a lotto Bomb");
          %client.player.kill();
         }
      }
   }

// either give the player a bomb, or fine them based on the bombs price
function Dig_RadioBombReward(%client, %brick, %level)
   {
    %client.stats.Dig_RadLottoBomb++;
    if ( %client.stats.Dig_RadLottoBomb > 1)
      %client.stats.Dig_RadLottoBomb=0;

    if ( %client.stats.Dig_RadLottobomb)
      {
       %client.bomb=%level * 2;
       messageClient(%client, '', "<color:00FF00>the lotto block gave you a level " @ %client.bomb @ " bomb");
       AddLog(%client, "R Lotto", "bomb |" @ %client.bomb );
      }
    else
      {
       %price = mFloor($Dig_Data_BombCost * mPow(%level*2 , 2.6));
       messageClient(%client, '', "<color:0000FF>Lotto Brick:<color:00FF00>Fines you " @ %price @ " For the price of a size " @ %level @ " bomb");
       %price = %client.CheckInsurance(%price);
       AddLog(%client, "R Lotto", "bomb fine | " @ %client.bomb @ "|" @ %price);
       %client.AddMoney( -%price, "bomb fine");
      }
   }

// lotto block kills player
function Dig_DeathReward(%client, %brick)
   {
    %client.stats.Dig_LottoDeath++;
    if ( %client.stats.Dig_LottoDeath> 1)
      %client.stats.Dig_LottoDeath=0;

    if ( %client.stats.Dig_LottoDeath)
      {
       messageClient(%client, '', "<color:00FF00>The Hostile Lotto block fails to kill you");
       AddLog(%client, "Lotto", "kill | failed");
      }
    else
      {
       if ( getRandom(1,2) == 2)
         {
          %ins = %client.CheckInsurance(500);
          if ( %ins > 0)
            {
             %client.instantRespawn();
             messageClient(%client, '', "<color:00FF00>The Hostile Lotto block Punts you back to start");
            }
          else
            MessageClient(%client, '', "<color:0000F0>Lotto Insurance:<color:FFFFFF> Saved you from respawning");
          return;
         }
       %ins = %client.CheckInsurance(1000);
       if ( %ins > 0)
         {
          %client.player.schedule(100, "kill");
          messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Was killed by a hostile lotto block");
          AddLog(%client, "Lotto", "kill");
         }
       else
         MessageClient(%client, '', "<color:0000F0>Lotto Insurance:<color:FFFFFF> Saved you from being killed");
      }

    %p = new Projectile()
      {
       dataBlock = Dig_BombProjectile;
       initialPosition = %brick.getposition();
       initialVelocity = "0 0 0";
       sourceObject = 0;
       client = %obj.client;
       sourceSlot = 0;
      };
    MissionCleanup.add(%p);

    %p.explode();
   }

// lotto block kills player
function Dig_RadioDeathReward(%client, %brick)
   {
    %client.stats.Dig_RadLottoDeath++;
    if ( %client.RadDig_LottoDeath> 1)
      %client.stats.Dig_RadLottoDeath=0;

    if ( %client.stats.Dig_RadLottoDeath)
      {
       %ins = %client.CheckInsurance(1000);
       if ( %ins > 0)
         {
          messageClient(%client, '', "<color:00FF00>The Pissed off Lotto block fails to kill you -- respawn instead");
          AddLog(%client, "R Lotto", "kill failed");
          %client.instantRespawn();
         }
       else
         MessageClient(%client, '', "<color:0000F0>Lotto Insurance:<color:FFFFFF> Saved you from respawning");
       return;
      }
    else
      {
       if ( getRandom(1,2) == 2)
         {
          CommandToClient(%client, 'MessageBoxOk', "Radioactive Lotto", "<color:00FF00>The Pissed off Lotto block Punts you out of the GAME");
          messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Was booted from the game by a Seriously Pissed Off Lotto Block");
          AddLog(%client, "R Lotto" , "kick");
          kickName(%client.getPlayerName());
          return;
         }
       %ins = %client.CheckInsurance(2000);
       if ( %ins > 0)
         {
          %client.player.schedule(100, "kill");
          messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Was killed by a Pissed Off Lotto block");
          %client.addPick(-1);
          AddLog(%client, "R Lotto" , "kill");
         }
       else
         MessageClient(%client, '', "<color:0000F0>Lotto Insurance:<color:FFFFFF> Saved you from being killed");
      }

    %p = new Projectile()
     {
      dataBlock = Dig_BombProjectile;
      initialPosition = %brick.getposition();
      initialVelocity = "0 0 0";
      sourceObject = 0;
      client = %obj.client;
      sourceSlot = 0;
     };
    MissionCleanup.add(%p);
    %p.explode();
   }

// Award +/- a pick level
function Dig_Pickreward(%client, %brick)
   {
    %client.stats.Dig_LottoPick++;
    if ( %client.stats.Dig_LottoPick > 1)
      %client.stats.Dig_LottoPick=0;

    %amount = getRandom(1,5);
    if ( %client.stats.Dig_LottoPick==0)
      { }
    else
      %amount = 0-%amount;

    %amount = Dig_LottoMessage(%client, "pick", %amount, false, 5*(%client.rank+1) );

    %client.addPick(%amount);
   }

// Award +/- a pick level
function Dig_RadioPickreward(%client, %brick, %level)
   {
    %client.stats.Dig_RadLottoPick++;
    if ( %client.stats.Dig_RadLottoPick > 2)
      %client.stats.Dig_RadLottoPick=0;

    %pos = %brick.getPosition();
    %reward = mabs(getWord(%pos, 2)-$Brick_Z) * 0.5;

    if ( %client.stats.Dig_RadLottoPick==0)
      %amount = 0 - (%reward*%level);
    else
      %amount = %reward;

    //%amount = mFloor((%amount / 2) * %level);
    %amount = Dig_LottoMessage(%client, "picks", %amount, true, 5*(%client.getRank()+1) );

    %client.addPick(%amount);
    %client.sendMiningStatus();
   }

// Award +/- cash based on depth
function Dig_CashReward(%client, %brick, %level)
   {
    %client.stats.Dig_LottoCash++;
    if ( %client.stats.Dig_LottoCash > 1)
      %client.stats.Dig_LottoCash =0;

    %pos = %brick.getPosition();
    %reward= mabs(getWord(%pos, 2)-$Brick_Z ) * 8;
    if ( %reward < 1)
      %reward=1;

    if ( %client.stats.Dig_LottoCash ==0)
      %amount = 0-%reward;
    else
      %amount = %reward;

    %amount= mFloor(%amount * %level);
    %amount = Dig_LottoMessage(%client, "$", %amount, (%level > 1), 1 );

    if ( %level > 1)
      %client.AddMoney( %amount, "R Lotto ", "cash | " @ %amount, 1);
    else
      %client.AddMoney( %amount, "Lotto ", "cash |" @ %amount, 1);
   }

// Award +/- heatsuits based on depth
function Dig_HeatsuitReward(%client, %brick, %level)
   {
    %client.stats.Dig_LottoHeat++;
    if ( %client.stats.Dig_Lottoheat > 1)
      %client.stats.Dig_LottoHeat=0;

    %pos = %brick.getPosition();
    %reward= mFloor(mabs(getWord(%pos, 2)-$Brick_Z ) * 0.25);
    if ( %reward < 1)
      %reward=1;

    if ( %client.stats.Dig_LottoHeat==0)
      %amount = %reward;
    else
      %amount = 0-%reward;

    %amount=mfloor(%amount*%level);
    %amount = Dig_LottoMessage(%client, "heatsuits", %amount, (%level > 1), mFloor($Dig_Data_HeatSuitCost/10) );

    %client.addHS( %amount);
   }

// Award +/- radsuits based on depth
function Dig_radsuitReward(%client, %brick, %level)
   {
    %client.stats.Dig_Lottorad++;
    if ( %client.stats.Dig_Lottorad > 1)
      %client.stats.Dig_Lottorad=0;

    %pos = %brick.getPosition();
    %reward= mFloor(mabs(getWord(%pos, 2)-$Brick_Z ) * 0.15);
    if ( %reward < 1)
       %reward=1;

    if ( %client.stats.Dig_Lottorad==0)
      %amount = %reward;
    else
      %amount = 0-%reward;
    %amount=mfloor(%amount*%level);
    %amount = Dig_LottoMessage(%client, "radsuits", %amount, (%level > 1), mFloor($Dig_Data_radsuitCost/10) );

    %client.addRS(%amount);
   }

// Award +/- dirt based on depth
function Dig_DirtReward(%client, %brick, %level)
   {
    %Dig_LottoDirt = getRandom(1,2);

    %pos = %brick.getPosition();
    %reward= mFloor(mabs(getWord(%pos, 2)-$Brick_Z ) );
    if ( %reward < 1)
      %reward=1;

    if ( %Dig_LottoDirt==2)
      %amount = %reward;
    else
      %amount = 0-%reward;

    %amount= mFloor(%amount * %level);
    %amount = Dig_LottoMessage(%client, "dirt", %amount, (%level > 1), 1 );

    %client.AddDirt(%amount);
   }

// display a message to the client and add a server log
function Dig_LottoMessage(%client, %thing, %anAmount, %rad, %mult)
   {
    %amount = mFloor(%anAmount);
    if ( %rad)
      {
      %type ="Radioactive Lotto Brick";
       AddLog(%client, "R Lotto" , %thing @ " | " @ %amount, 1 );
      }
    else
      {
       %type ="Lotto Brick";
       AddLog(%client, "Lotto" , %thing @ " | " @ %amount, 1 );
      }

    if ( %amount > 0)
      {
       messageClient(%client, '', "<color:0000FF>" @ %type @ ":<color:00FF00>You got " @ addComma(%amount) SPC %thing );
      }
    else
      {
       messageClient(%client, '', "<color:0000FF>" @ %type @ ":<color:00FF00>You LOST " @ AddComma(mAbs(%amount)) SPC %thing );
       %amount = 0 - %client.CheckInsurance(%amount, %mult);
      }
    return %amount;
   }

// Allows players to reset everything back to zero
function serverCmdResetMyStats(%client)
   {
    if ( %client.getPick() < 0)
      {
       messageClient(%client, '',"<color:FFFFFF>Your ability to reset stats has been revoked");
       echo(%client.getPlayername() @ " reset stats revoked");
       return;
      }

    commandToClient(%client,'MessageBoxYesNo', "Reset stats?", "Are you sure?", 'resetConfirm');
   }

// confirm stats reset
function serverCmdResetConfirm(%client)
   {
    if ( %client.getPick() < 0)
      {
       messageClient(%client, '',"<color:FFFFFF>Your ability to reset stats has been revoked");
       echo(%client.getPlayername() @ " reset stats revoked");
       return;
      }

    messageClient(%client, '',"<color:FFFFFF>Reset confirmed");
    AddLog(%client, "reset","stats", 1);
    %client.stats.reset();
    %client.bomb=0;
    %client.sendMiningStatus();
    messageClient(%client, '',"<color:FFFFFF>Your stats have been reset to 0");
   }

// modify %amount according to this client's lotto insurance amount
function Gameconnection::CheckInsurance(%this, %amount, %mult)
   {
    %amount = mabs(%amount);
    if ( %mult < 1)
      {
       %mult = 1;
      }
    if ( %this.insurance < 1)
      {
       return %amount;
      }
    %used = %amount * %mult;
    if ( %this.insurance >= %used )
      {
       %this.insurance -= %used;
       %absorb = %amount;
       %amount = 0;
      }
    else
      {
       %amount-= mFloor((%used - %this.insurance) / %mult);
       %absorb = mFloor((%used - %this.insurance) / %mult);
       %this.insurance =0;
      }
    MessageClient(%this, '', "<color:0000F0>Lotto Insurance:<color:FFFFFF> Absorbs " @ addComma(%absorb) @ " of the loss");
    AddLog(%this, "insurance", " use | " @ %absorb * %mult, 1);
    %this.sendMiningStatus();
    return %amount;
   }

// client buys some lotto insurance
function BuyInsurance(%client, %amount)
   {
    if ( %amount < 0)
      {
       FineForCheating(%client, mabs(%amount) );
       return;
      }
    if ( %amount < 5)
      {
       Dig_DisplayError(%client, "cant buy less than 5 insurance");
       return;
      }
    %price = mFloor(%amount * 0.8);
    if ( %client.getMoney().lessThan(%price) )
      {
       Dig_DisplayError(%client, "you need $" @ %price @ " to buy " @ %amount @ " insurance");
       return;
      }
    %client.AddMoney(-%price, "insurance");
    %client.insurance+=%amount;
    messageClient(%client, '', "You bought " @ %amount @ " lotto insurance for $" @ %price @ " (insurance does not save)");
    %client.sendMiningStatus();
   }

// allow players to buy a teleport up or down
function ServerCmdBuyTeleport(%client, %xi, %yi, %zi)
   {
    echo("ServerCmdBuyTeleport " @ %client.getPlayerName() @ " x: " @ %xi @ " y: " @ %yi @ " z: " @ %zi);
    if ( $Dig_TotalBlobs < 23)
      {
       Dig_DisplayError(%client, "Cannot buy teleports -- yet");
       return;
      }

    if ( strlen(expandNumber(mabs(%xi))) > 5 || strlen(expandNumber(mabs(%yi))) > 5 || strlen(expandNumber(mabs(%zi))) > 5 )
      {
       Dig_DisplayError(%client, "teleport distance too far");
       return;
      }
    if ( %client.teleportInProgress == true)
      {
       Dig_DisplayError(%client, "Teleport in progress - please wait");
       return;
      }
    if ( %yi $= "" && %zi $= "")
      {
       %zi=mFloor(%xi);
       %xi=0; %yi=0;
      }


    //echo("offset: " @ %xi SPC %y1 SPC %zi);
    %dist = VectorLen(%xi SPC %yi SPC %zi);

    if ( %dist < 10)
      {
       Dig_DisplayError(%client, "Min teleport distance is 10");
       return;
      }
    %xT = mFloor(%xi / 2) * 2;
    %yT = mFloor(%yi / 2) * 2;
    %zT = mFloor(%zi / 2) * 2;

    %amount = VectorLen(%xT SPC %yT SPC %zT);
    %cost = mabs(%amount * 4000);

    %discount = %client.RankDiscount(%cost);
    %cost -= %discount;

    if ( %discount > 0)
      {
       MessageClient(%client, '', "<color:00FF00>Rank discount $" @ AddComma(%discount) );
      }

    if ( %client.getMoney().lessThan(%cost) )
      {
       Dig_DisplayError(%client, "You cannot teleport: it costs \c3$" @ %cost);
       return;
      }
    if ( mabs(%amount) < 1)
      {
       echo("Error: invalid teleport " @ %xi SPC %yi SPC %zi);
       return;
      }

    %pos = %client.player.getPosition();
    %x = mFloor(getWord(%pos, 0) / 2) * 2;
    %y = mFloor(getWord(%pos, 1) / 2) * 2;
    %z = mFloor(getWord(%pos, 2) / 2) * 2;
    %x++; %y++;
    echo(%pos @ " pos: " @ %x SPC %y SPC %z);
    if ( (%z + %Zt) - $Brick_Z < $Dig_Data_MaxDepth)
      {
       Dig_DisplayError(%client, "Cannot teleport below the map");
       return;
      }
    if ( (%z + %zt) - $Brick_Z > $Dig_Data_MaxHeight)
      {
       Dig_DisplayError(%client, "Cannot teleport above the map");
       return;
      }

    %client.TeleportInProgress = true;
    %client.addMoney(-%cost, "teleport");

    %newPos = %x + %xT SPC %y + %yT SPC %z + %zT;
    MessageClient(%client, '', "<color:FFFFFF>Teleporting " @ %amount @ " units " @ %xT SPC %YT SPC %ZT);

    AddLog(%client, "Buy Teleport", %amount @ " | " @ %newPos, 1);
    schedule(2000, 0, EnableTeleport, %client);
    TeleportPlayer(%client, %newPos);
   }

function EnableTeleport(%client)
   {
    %client.TeleportInProgress = false;
   }
