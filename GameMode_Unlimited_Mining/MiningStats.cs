//
//  MiningStats.cs
//  functions for player stats in mining mod
//

// send updated mining stats to the client
function GameConnection::sendMiningStatus(%this)
{
  %msg = "<font:arial bold:14>\c6Cash \c3$"@ AddComma(%this.getMoney().display() );
  %msg = %msg @ "   \c6Dirt \c3"@ AddComma(%this.dirt.display() );
  %msg = %msg @ "   \c6Pick level \c3"@ %this.getPick();
  %msg = %msg @ "   \c6Next pick \c3$"@ Dig_pickCost(%this);
  %msg = %msg @ "   \c6LOC \c3";

  if ( isObject(%this.player) )
  {
    %pos = VectorSub(%this.player.getPosition(), $BrickOffset);
    %msg = %msg @ mfloor(getword(%pos, 0)) SPC mFloor(getWord(%pos, 1)) SPC mFloor(getWord(%pos, 2));
  }
  %msg = %msg @ "   \c6HS: \c3" @ AddComma(%this.getHS().display() );
  %msg = %msg @ "   \c6RS: \c3" @ AddComma(%this.getRS().display() );
  if ( %this.player.radDamage > 0)
  {
    %msg = %msg @ " <color:0000FF>Rad: " @ %this.player.radDamage;
  }
  %msg1 = %msg1 @ "\c2Rank \c6 " @ %this.getRank();

  if ( %this.getArmor() < %this.getPick())
  {
    %msg1 = %msg1 @ " \c0Armor: " @ %this.getArmor() @ " of " @ %this.getPick();
  }
  if ( $ModeMoney["normal"] != 1)
  {
    %msg1 = %msg1 @ "   \c2Lotto buff: x" @ $ModeMoney["normal"];
  }
  if ( %this.insurance != 0)
  {
    %msg1 = %msg1 @ "    \c6Insurance: \c3" @ %this.insurance;
  }
  commandtoClient(%this, 'bottomPrint', %msg @ "<br>" @ %msg1, -1);
  %this.score=%this.getPick();

  if ( %this.bomb > 0)
  {
    commandToClient(%this, 'CenterPrint', "<color:FFFFFF>Bomb Mode - size:" @ %this.bomb @ "\nhit any brick", -1);
  }
}

// save mining stats for client to a global variable
function GameConnection::saveMiningStats(%this)
{
  if (!%this.hasSpawnedonce)
  {
    echo("player has not spawned, not saving " @ %this.getPlayerName() );
    return;
  }
  if ( %this.getPick() ==1 && %this.getMoney().display() $= "0")
  {
    echo("Nothing to save for " @ %this.getPlayerName() );
    return;
  }
  %this.stats.saveStats(%this);
  %this.setRankPrefix();
  %this.WriteOreLogs();
}

// load mining stats from global variable into the client
function GameConnection::loadMiningStats(%this)
{
  %id = %this.bl_id;
  MiningStats::ForPlayer(%this);
  %this.dirt = BigNumberFromString("0");
  %this.sendMiningStatus();
}

function GameConnection::AddHS(%this, %amount)
{
  //echo(%this.getPlayerName() @ " addHS " @ %amount);
  %this.stats.heatsuit.add(%amount);
  return %this.stats.heatsuit;
}
function GameConnection::GetHS(%this)
{
  //if ( !isObject(%this.stats.heatsuit) )
  //  {
  //   %this.stats.heatsuit = BigNumberFromString("0");
  //  }
  return %this.stats.heatsuit;
}
function GameConnection::AddRS(%this, %amount)
{
  //echo(%this.getPlayerName() @ " addRS " @ %amount);
  %this.stats.radsuit.add(%amount);
  return %this.stats.radsuit;
}
function GameConnection::getRS(%this)
{
  //if ( !isObject(%this.stats.radsuit) )
  //  {
  //   %this.stats.radsuit = BigNumberFromString("0");
  //  }
  return %this.stats.radsuit;
}
function GameConnection::getRank(%this)
{
  return %this.stats.rank;
}
// add some pick levels to the client
function GameConnection::AddPick(%this, %amount)
{
  %this.stats.pickAxeNum += mFloor(%amount);

  %this.score = %this.getPick();
  %rankLimit = 65000;
  %rankLimit += %this.rankDiscount(%rankLimit);
  if ( %this.stats.pickaxeNum > %rankLimit)
  {
    if ( %this.stats.rank > 95)
    {
      MessageClient(%this, '', "<color:00FF00>You are at max rank and cannot rank up anymore");
      %this.stats.pickaxeNum = %rankLimit;
    }
    else
    {
      %this.stats.rank++;
      MessageClient(%this, '', "<color:00FF00>Rank up!!");
      MessageClient(%this, '', "<color:00FF00>You are now rank <color:FF0000>" @ %this.getRank() );
      %this.stats.pickaxeNum -= %rankLimit;
      %this.stats.pickaxeNum += %this.stats.rank;
      MessageAll('', "<color:00FF00>Rank UP " @ %this.getPlayerName() @ " is now rank " @ %this.GetRank() );
      %this.setRankprefix();

      %rankLimit = 65000;
      %rankLimit += %this.rankDiscount(%rankLimit);
      if ( %this.stats.pickaxeNum > %rankLimit)
      {
        %this.addPick(0);
      }
    }
  }

  if ( %this.stats.pickaxeNum < 0)
    if ( %this.getRank() > 0)
    {
      %this.stats.rank--;
      %rankLimit = 65000;
      %rankLimit += %this.rankDiscount(%rankLimit);

      %this.stats.pickaxeNum = %rankLimit + %this.stats.PickAxeNum;

      MessageClient(%this, '', "<color:00FF00>Rank Down");
      MessageClient(%this, '', "<color:00FF00>You are now rank <color:FF0000>" @ %this.getRank() );

      MessageAll('', "<color:00FF00>Rank DOWN " @ %this.getPlayerName() @ " is now rank " @ %this.GetRank() );
      %this.setRankprefix();
      if ( %this.stats.pickaxeNum < 0)
      {
        %this.addPick(0);
      }
    }
  else
    %this.stats.pickaxeNum=1;

  %this.stats.mineArmor = %this.stats.pickaxeNum;
  %this.score = %this.stats.pickaxeNum;
  return %this.stats.pickaxeNum;
}

// Add dirt to the client
function GameConnection::AddDirt(%this, %amount)
{
  if ( %amount==0)
  {
    return;
  }
  %this.dirt.add(%amount);
  %this.checkDirt();
  return %this.dirt;
}

// give client some money
function GameConnection::addMoney(%this, %amount, %reason, %echo)
{
  if ( %amount == 0)
  return;

  %this.stats.mineMoney.add(%amount);
  AddLog(%this, "money" , %this.stats.mineMoney.display() @ " | " @ %amount @ " | " @ %reason, %echo);
  return %this.stats.mineMoney;
}

function GameConnection::getMoney(%this)
{
  return %this.stats.MineMoney;
}
function GameConnection::getPick(%this)
{
  return %this.stats.PickAxeNum;
}
function GameConnection::getArmor(%this)
{
  return %this.stats.MineArmor;
}
function GameConnection::addArmor(%this, %value)
{
  %this.stats.MineArmor += %value;
}
function GameConnection::setArmor(%this, %value)
{
  %this.stats.MineArmor = %value;
}
function GameConnection::setRankprefix(%this)
{
  if(%this.plainClanPrefix $= "" && %this.clanPrefix $= "")
    %this.plainClanPrefix = %this.clanPrefix;
    
  if ( %this.stats.rank > 0)
  {
    %this.clanPrefix = "[" @ %this.stats.rank @ "] " @ %this.plainClanPrefix;
  }
  else
  {
    %this.clanPrefix = "";
  }
}
// return discount amount of the given price
function GameConnection::rankDiscount(%this, %price)
{
  return mFloor(%price * (%this.getRank() / 200));
}

function MiningStats::ForPlayer(%client)
{
  %id = %client.bl_id;

  if ( isObject($Dig_Stats[%id]) )
  {
    %stats = $Dig_Stats[%id];
    %client.stats = %stats;
    %client.setRankPrefix();

    AddLog(%client, "Reuse stats", "bl_id " @ %id @ " pick " @ %stats.pickAxeNum @ " $" @ %stats.mineMoney.display() @ " HS " @ %stats.heatsuit.display() @ " RS " @ %stats.radsuit.display() @ " Rank" @ %stats.rank , 1 );
    return;
  }
  %stats = new ScriptObject(MiningStats);
  %stats.bl_id = %id;
  if ( !$Dig_stats_Saved[%id] )
  {
    %stats.pickaxeNum = 1;
    %stats.mineMoney  = BigNumberFromString("0");
    %stats.heatsuit   = BigNumberFromString("0");
    %stats.radsuit    = BigNumberFromString("0");
    %stats.rank = 0;
  }
  else
  {
    %stats.pickaxeNum = getWord($Dig_stats_Saved[%id], 0);
    %stats.mineMoney  = BigNumberFromString( getWord($Dig_stats_Saved[%id], 1) );
    %stats.heatsuit   = BigNumberFromString( getWord($Dig_Stats_Saved[%id], 2) );
    %stats.radsuit    = BigNumberFromString( getWord($Dig_Stats_Saved[%id], 3) );
    %stats.rank       = getWord($Dig_Stats_Saved[%id], 4);
    %stats.mineArmor  = %stats.pickaxenum;
    %stats.MineMoney.p1=mFloor(%stats.MineMoney.p1);
  }
  if ( strpos(%stats.rank, "/") > -1 )
  {
    %stats.rank = 0;
  }
  %client.score = %stats.pickaxeNum;
  %client.setRankprefix();
  AddLog(%client, "Load stats", "bl_id " @ %id @ " pick " @ %stats.pickAxeNum @ " $" @ %stats.mineMoney.display() @ " HS " @ %stats.heatsuit.display() @ " RS " @ %stats.radsuit.display() @ " Rank " @ %stats.rank , 1 );
  %client.stats = %stats;
  $Dig_Stats[%id] = %stats;
}

function MiningStats::saveStats(%this, %client)
{
  %this.heatsuit.changed=true;
  %this.radsuit.changed=true;
  $Dig_stats_Saved[%this.bl_id] = %this.pickAxeNum SPC %this.MineMoney.display() SPC %this.heatsuit.display() SPC %this.radsuit.display() SPC %this.rank SPC getDateTime();
  AddLog(%client, "save stats", "bl_id " @ %this.bl_id @ " pick " @ %this.pickAxeNum @ " $" @ %this.mineMoney.display() @ " HS " @ %this.heatsuit.display() @ " RS " @ %this.radsuit.display() @ " Rank " @ %this.rank, 1 );
}

// reset stats
function MiningStats::reset(%this)
{
  %this.pickaxenum= 1;
  %this.MineMoney = BigNumberFromString("0");
  %this.heatsuit  = BigNumberFromString("0");
  %this.radsuit   = BigNumberFromString("0");
}
