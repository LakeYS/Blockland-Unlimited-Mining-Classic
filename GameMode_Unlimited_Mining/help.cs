// help.cs
//
// help text for unlimited mining

// register the help gui
if (isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
{
  if (!$RTB::RTBR_GUITransfer_Hook)
  exec("Add-Ons/System_ReturnToBlockland/RTBR_GUITransfer_Hook.cs");

  // since RTB gui transfer doesnt do tabs -- this kludge is needed
  RTB_registerGUI("Add-ons/Script_testing/miningGui.gui");
  RTB_registerGUI("Add-ons/Script_testing/miningGui1.gui");
  RTB_registerGUI("Add-ons/Script_testing/miningGui2.gui");
}

// help text and/or display a GUI if client has RTB
function serverCmdHelp(%client, %command)
{
  if ( %command $="drillmore")
  {
    messageClient(%client, '', "<color:FFFF00>Drill help <color:00FF00>(advanced)");
    messageClient(%client, '', "<color:FF0000>Q: <color:FFFFFF>I did /drill 100 5 and only got a tiny hole.  What happened?");
    messageClient(%client, '', "<color:FF0000>A: <color:FFFFFF>Drills need a flat surface to start.  A drill of size 5 needs 11 bricks across (5 brick radius + 1 for center");

    messageClient(%client, '', "<color:FF0000>Q: <color:FFFFFF>My Drill ran out of dirt? ... huh?");
    messageClient(%client, '', "<color:FF0000>A: <color:FFFFFF>Drills use up your dirt while they run. 1 dirt for every brick");
    return;
  }

  if ( getSubstr(%command,0,5) $= "drill")
  {
    messageClient(%client, '', "<color:FFFF00>How To Drill");
    messageClient(%client, '', "<color:FFFF00>1)  <color:FFFFFF>say: /drill 15");
    messageClient(%client, '', "<color:FFFF00>2)  <color:FFFFFF>Hit any brick.  If you have 30 dirt, this makes a hole 15 bricks deep");
    messageClient(%client, '', "<color:FFFF00>Drills do not give you money for ores or lottos");
    messageClient(%client, '', "<color:FFFF00><depth>  and <size> are measured in bricks.  /drill 100 makes a hole 100 bricks deep");
    messageClient(%client, '', "<color:FFFF00>say: /help drillmore   for more help on drills");
    return;
  }
  if ( getSubstr(%command,0,9) $= "insurance")
  {
    messageClient(%client, '', "<color:FFFF00>Lotto insurance");
    messageClient(%client, '', "<color:FFFF00>   say: <color:FFFFFF>/buy insurance ##");
    messageClient(%client, '', "<color:FFFF00>   twhen you mine a 'bad' lotto block, the insurance will absorb all or most of the loss");
    messageClient(%client, '', "<color:FFFF00>   Lotto insurance is not saved when you logout");
    return;
  }
  if ( getSubstr(%command,0,4) $= "rank")
  {
    messageClient(%client, '', "<color:FFFF00>Ranks");
    messageClient(%client, '', "<color:FFFF00>Everyone starts at rank 0");
    messageClient(%client, '', "<color:FFFF00>When your pick reaches 65,000, you will gain rank 1");
    messageClient(%client, '', "<color:FFFF00>Ranks give you certian abilities, A discount on bombs is one");
    messageClient(%client, '', "<color:FFFF00>The others are left for you to discover");

    %rankLimit = 65000;
    %rankLimit += %client.rankDiscount(%rankLimit);
    messageClient(%client, '',"<color:FFFF00>You have " @ %rankLimit - %client.getPick() @ " picks until next rank");

    return;
  }
  messageClient(%client, '', "<color:FFFFFF>Infinite Mining Mod help");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>upgrade pick:<lmargin:120> <color:FF0088>/upgradepick");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>fast upgrade:<lmargin:120> <color:FF0088>/upgradeall<color:FFFFFF> (costs extra)   -or- <color:FF0088>/upgradeall <number>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>sell picks<lmargin:120> <color:FF0088>/sellPick number   Sell some of your pick levels for money");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>heat suit:<lmargin:120> <color:FF0088>/buyheatsuit <number> \c3$"@$Dig_Data_HeatsuitCost @ "/layer");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Light:<lmargin:120> <color:FF0088>/buylight \c3$"@ $Dig_Data_LightCost);
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Cement placer:<lmargin:120> <color:FF0088>/buy placer \c3$100");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Gun:<lmargin:120> <color:FF0088>/buy gun\c3$250");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Help1<lmargin:150> <color:FF0088>/Help1 for more<lmargin:0>");
  commandtoclient(%client,'RTB_OpenGUI',"MiningGui");
}

function serverCmdHelp1(%client)
{
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Donate:<lmargin:120> <color:FF0088>/donate player amount");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Donate hs:<lmargin:120> <color:FF0088>/donateHS player amount");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Donate rs:<lmargin:120> <color:FF0088>/donateRS player amount");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Donate dirt:<lmargin:120> <color:FF0088>/donateDirt player amount");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>you can shorten big numbers by using k or m.  ex: \c312m  or 50k");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Bomb:<lmargin:120> <color:FF0088>/buy bomb <size> \c3$" @ $Dig_Data_BombCost @ "<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Drill:<lmargin:120> <color:FF0088>/Drill <depth> <width>   Costs dirt to run. <color:FFFFFF>/help drill for more");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Reset:<lmargin:120> <color:FF0088>/resetMyStats  start over<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Help2<lmargin:120> <color:FF0088>/Help2 for even more<lmargin:0>");
  commandtoclient(%client,'RTB_OpenGUI',"MiningGui1");
}

function serverCmdHelp2(%client)
{
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Rad suit:<lmargin:120> <color:FF0088>/buyradsuit number \c3$"@$Dig_Data_RadsuitCost @ "/capacity");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>cure:<lmargin:120> <color:FF0088>/CureRad  Cure radation Sickness<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Inv Cement placer:<lmargin:120> <color:FF0088>/buy invplacer \c3$100 <color:FFFFFF>Nobody can kill invulnerable cement");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Platform:<lmargin:120> <color:FF0088>/platform width depth");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Wall:<lmargin:120> <color:FF0088>/Wall height Length");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Platform#2:<lmargin:120> <color:FF0088>/platform2 width depth height");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Room:<lmargin:120> <color:FF0088>/room width depth height");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Teleporter:<lmargin:120> <color:FF0088>/Teleporter to place a personal teleporter <color:FFFFFF>Cement placer required<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Help3<lmargin:120> <color:FF0088>/Help3 for even more<lmargin:0>");
  commandtoclient(%client,'RTB_OpenGUI',"MiningGui2");
}

function serverCmdHelp3(%client)
{
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Sell Pick:<lmargin:120> <color:FF0088>/sellpick <number>  sell your pick levels for money");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Teleport Invite:<lmargin:120> <color:FF0088>/inviteTeleport player   let player use your teleporter once");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Buy Teleport:<lmargin:120> <color:FF0088>/buyTeleport distance    +/- distance to teleport up or down<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Buy Teleport:<lmargin:120> <color:FF0088>/buyTeleport X Y Z   +/- teleport X,Y,Z bricks from where you are<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Turbo Drill:<lmargin:120> <color:FF0088>/buyTurboDrill  Costs 10 picks.  your drills go 3x speed until you log out<lmargin:0>");
  messageClient(%client, '', "<lmargin:0><color:FFFFFF>Instant Drill:<lmargin:120> <color:FF0088>/buySuperTurboDrill  Costs 200 picks.  Your drills complete almost instantly.  Lasts until you log out<lmargin:0>");
}
