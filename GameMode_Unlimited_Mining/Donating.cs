//
// Donate commands for unlimited mining
//
// donate (give) money to another player
function serverCmdDonate(%client, %Name, %name1, %Name2, %value)
{
  %v = GetDonateAmount(%name, %name1, %name2, %value);
  %playerName = getField(%v, 0);
  %amount = expandNumber( getField(%v, 1) );
  if ( strlen(%amount) < 6)
  {
    %fee = mFloor(%amount * 0.01);
  }
  else
  %fee = getSubStr(%amount, 0, strlen(%amount) - 2);

  %newClient = DonateCheckBigNumber(%client, %playerName, %amount, %client.getMoney(), "money",  %fee);
  if ( !isObject(%newClient) )
  {
    return;
  }

  AddLog(%client, "donate" , %amount @ "(" @ %Name SPC %Name1 SPC %Name2 SPC %value @") to " @ %playerName, 1);
  %fee = "-"@%fee;
  %client.AddMoney( %fee, "Donate fee", 1);
  %client.AddMoney( "-" @ %amount, "Donate to " @ %newClient.getPlayerName() );
  %newClient.AddMoney( %amount, "Receive Donate " @ %client.getPlayerName() );
  DonateMsg(%client, %newClient, %amount, "$");
  %client.sendMiningStatus();
  %newClient.sendMiningStatus();
}

// donateHS - give heatsuit layers to a player
function serverCmdDonateHS(%client, %Name, %name1, %Name2, %value)
{
  %v = GetDonateAmount(%name, %name1, %name2, %value);
  %playerName = getField(%v, 0);
  %amount = getField(%v, 1);

  %newClient = DonateCheckBigNumber(%client, %playerName, %amount, %client.getHS(), "heatsuit layers", 1);
  if ( !isObject(%newClient) )
  {
    return;
  }

  AddLog(%client, "donateHS " , %amount @ "(" @ %Name SPC %Name1 SPC %Name2 SPC %value @") to " @ %playerName, 1);
  AddLog(%newClient, "receive DonateHS", %amount @ " from " @ %client.getPlayerName() );
  %client.AddHS(-1);
  %client.AddHS(-%amount);
  %newClient.addHS(%amount);
  DonateMsg(%client, %newClient, %amount, "heatsuit layers");
  %client.sendMiningStatus();
  %newClient.sendMiningStatus();
}

// donateRS - give radsuit layers to a player
function serverCmdDonateRS(%client, %Name, %name1, %Name2, %value)
{
  %v = GetDonateAmount(%name, %name1, %name2, %value);
  %playerName = getField(%v, 0);
  %amount = getField(%v, 1);

  %newClient = DonateCheckBigNumber(%client, %playerName, %amount, %client.getRS(), "radsuit layers", 1);
  if ( !isObject(%newClient) )
  return;

  AddLog(%client, "donateRS " , %amount @ "(" @ %Name SPC %Name1 SPC %Name2 SPC %value @") to " @ %playerName, 1);
  AddLog(%newClient, "receive DonateRS", %amount @ " from " @ %client.getPlayerName() );
  %client.AddRS(-1);
  %client.addRS(-%amount);
  %newClient.addRS(%amount);
  DonateMsg(%client, %newClient, %amount, "radsuit layers");
  %client.sendMiningStatus();
  %newClient.sendMiningStatus();
}

// donateDirt - give dirt to a player
function serverCmdDonateDirt(%client, %Name, %name1, %Name2, %value)
{
  %v = GetDonateAmount(%name, %name1, %name2, %value);
  %playerName = getField(%v, 0);
  %amount = getField(%v, 1);

  %newClient = DonateCheckBigNumber(%client, %playerName, %amount, %client.dirt, "Dirt", 0);
  if ( !isObject(%newClient) )
    return;

  AddLog(%client, "donateDirt " , %amount @ "(" @ %Name SPC %Name1 SPC %Name2 SPC %value @") to " @ %playerName, 1);
  AddLog(%newClient, "receive DonateDirt", %amount @ " from " @ %client.getPlayerName() );
  %client.AddDirt( -%amount);
  %newClient.addDirt( %amount);
  DonateMsg(%client, %newClient, %amount, "Dirt");
  %client.sendMiningStatus();
  %newClient.sendMiningStatus();
}

// given up to 4 arguments, return a name and amount.
// allows for names with spaces in them
function GetDonateAmount(%name, %name1, %name2, %value)
{
  %playerName = %Name;
  if ( mFloor(%Name1) $= "0")
  {
    %playerName = %playerName SPC %name1; //arg 2 is a word
    if ( mFloor(%name2) $= "0")
    {
      %playerName = %playerName SPC %name2; //arg 3 is a word
      %amount = %value; // change strings to '0'
    }
    else
    %amount = %Name2; // change strings to '0'
  }
  else
  %amount = %Name1; // change strings to '0'

  %newAmount = strReplace(%amount, "k", "000");
  %newAmount = strReplace(%newAmount, "m", "000000");
  %newAmount = filterString(%newAmount, "0123456789");

  return %playerName TAB %newAmount;
}

function DonateMsg(%client, %newClient, %amount, %desc)
{
  %comma = addComma(%amount);
  messageClient(%client, '', "<color:00FF00>You donated<color:00FFFF> " @ %comma SPC %desc @ " <color:00FF00>to <color:00FFFF>" @ %newClient.getPlayerName() );
  messageClient(%newClient, '', "<color:00FFFF>" @ %client.getPlayerName() @ " <color:00FF00>Gave you <color:00FFFF>" @ %comma SPC %desc);
}


// return a newclient if %client can donate to %playerName
function DonateCheckBigNumber(%client, %PlayerName, %Amount, %clientAmount, %desc, %fee)
{
  %time = getSimtime();
  if ( %client.lastDonate > 0)
  {
    //echo(%client.getPlayerName() SPC %client.lastDonate SPC %time SPC (%time - %client.lastDonate) );
    if ( (%time - %client.lastDonate) < 5000)
    {
      Dig_DisplayError(%client, "Please wait before donating again");
      return false;
    }
  }
  %client.lastDonate = %time;

  %newClient = findclientbyname(%playerName);
  //if ( strlen(%Amount) > 11)
  //  {
  //   Dig_DisplayError(%client, "Number too big");
  //   return false;
  //  }
  if ( !isObject(%newClient) )
  {
    Dig_DisplayError(%client, "Nobody named <color:OOFFOO>" @ %playerName @ " <color:FFOOOO>Is currently playing");
    return false;
  }
  if ( %newClient == %client || %newClient.bl_id == %client.bl_id )
  {
    Dig_DisplayError(%client, "Ha Ha, funny<color:OOFFOO> Cannot donate " @ %desc @ " to yourself");
    AddLog(%client, "Donate", "Donate self denied", 1);
    return false;
  }
  %temp = BigNumberFromString(%amount).add(%fee);
  if (%clientAmount.lessThan(%temp.display() ) )
  {
    Dig_DisplayError(%client, "You dont have enough " @ %desc @ " to donate to<color:OOFFOO> " @ %playerName);
    return false;
  }

  if ( %amount < 0)
  {
    return false;
  }

  return %newClient;
}

function filterString(%string,%allowed)
{
  for(%i=0;%i<strLen(%string);%i++)
  {
    %char = getSubStr(%string,%i,1);
    if(strPos(%allowed,%char) >= 0)
    %return = %return@%char;
  }
  return %return;
}
