// upgrading.cs
// functions to handle pick upgrades

// upgrade pick by 1 level
function serverCmdUpgradePick(%client)
   {
    if (!upgradeCheck(%client, 200) )
      {
       return;
      }
    %cost = Dig_PickCost(%client);
    if (%client.getMoney().lessThan(%cost) )
      {
       Dig_DisplayError(%client, "You do not have enough money. An upgrade costs $" @ %cost);
       return;
      }
    %client.addPick(1);

    %client.AddMoney( -%cost, "upgradepick " @ %client.getPick() );
    messageClient(%client, '', "Pick upgraded to level "@ %client.getPick() @" ($"@ %cost @")");

    %client.sendMiningStatus();
   }

function serverCmdSellPick(%client)
{
  messageClient(%client, '', "Pick selling is currently unavailable.");
}

// upgrade pick w/o any chat spam
function serverCmdUpgradePickS(%client)
   {
    if ( !upgradeCheck(%client, 500) )
      {
       return;
      }
    %cost = Dig_PickCost(%client);
    if (%client.getMoney().lessThan(%cost) )
      {
       Dig_DisplayError(%client, "You do not have enough money. An upgrade costs $" @ %cost);
       return;
      }
    %client.addPick(1);
    %client.AddMoney( -%cost, "upgradepick " @ %client.getPick() );
    %client.sendMiningStatus();
   }

// upgradeall w/o the chat spam
function serverCmdUpgradeAll(%client, %count)
   {
    if ( !upgradeCheck(%client, 5000) )
      {
       return;
      }

    if ( %count > 3000)
      {
       Dig_DisplayError(%client, "Please upgrade in smaller chunks");
       return;
      }
    if ( %count < 1)
      {
       %fee = getUpgradeAllFee(%client);
       if ( %client.getMoney().lessThan( %fee) || %client.getMoney().sign == -1)
         {
          Dig_DisplayError(%client, "You cant afford to upgrade all at once. try: <color:FF0000>/upgradepick instead");
          return;
         }
       %client.AddMoney( "-" @ %fee, "upgradeall | fee");
       %count = 3000;
      }
    else
      AddLog(%client, "upgradeall", %count, 1);

    %start = %client.getPick();
    %cost = Dig_PickCost(%client);
    for ( %a=0; %a < %count; %a++)
      {
       if ( %client.getMoney().lessThan(%cost))
         {
          break;
         }
       %client.addpick(1);
       %client.getMoney().subtract(%cost);
       %cost = Dig_PickCost(%client);
      }

    %client.sendMiningStatus();
    // generates a log entry reflecting the final money
    %client.AddMoney(1, "upgradeall", 0);
    AddLog(%client, "upgradeall", "from " @ %start @ " to " @ %client.getPick(), 1);
   }

// return the fee for %client to do and /upgradeall
function getUpgradeallFee(%client)
   {
    %money = %client.getMoney().Display();
    if ( strlen(%money) < 6)
      %fee = mFloor(%money * 0.01);
    else
      %fee = getSubstr(%money, 0, strlen(%money) - 2);

    %cost = Dig_PickCost(%client);
    %tmp = BigNumberFromString(%fee);
    %tmp.add(%cost);
    %fee = %tmp.display();
    %tmp.delete();
    return %fee;
   }

function upgradeCheck(%client, %interval)
   {
    if ( ($Dig_on == 0) )
      {
       return false; // dig mode is off
      }
    if ( !isObject(%client) )
      {
       return false;
      }

    %time = getSimtime();
    //echo("%client.lastupgrade " @ %client.lastUpgrade);
    if ( %client.lastupgrade > 0)
      {
       if ( (%time - %client.lastupgrade) < %interval)
         {
          messageClient(%client, '', "<color:FFFFFF>Please wait before upgrading again");
          %client.upgradeWait++;
          echo("upgrade wait " @ %client.upgradeWait SPC %client.getPlayerName() );

          return false;
         }
     }
    %client.LastUpgrade = %time;
    %client.upgradeWait=0;
    return true;
   }

function serverCmdUpgradeJob(%client)
   {
    if ( $Dig_TotalBlobs < 23)
      {
       Dig_DisplayError(%client, "Cannot do upgrade job -- yet");
       return;
      }
    if ( %client.upgradeJob)
      {
       MessageClient(%client, '', "<color:FFFFFF>Upgrade job in progress.  There are <color:00FF00>" @ $JobQueue.entries.getCount() @ " jobs <color:FFFFFF>waiting in line");
       return;
      }

    // create the job here
    %job = new ScriptObject(UpgradeAllJob)
      {
       client = %client;
       start = %client.getPick();
      };
    MessageClient(%client, '', "<color:FFFFFF>Upgrade job submitted.  There are <color:00FF00>" @ $JobQueue.entries.getCount() @ " jobs <color:FFFFFF>waiting in line");
    MessageClient(%client, '', "<color:FFFFFF>say /cancelUpgrade to stop");
    $JobQueue.AddJob(%job);
    %client.upgradeJob = true;
    AddLog(%client, "upgrade job", "start");
   }

function serverCmdCancelUpgrade(%client)
   {
    if ( %client.upgradeJob)
      {
       %client.cancelUpgrade=true;
       MessageClient(%client, '', "<color:FFFFFF>Upgrade job Cancelled");
       AddLog(%client, "upgrade job", "cancel");
       return;
      }
    else
      Dig_DisplayError(%client, "You do not have an upgrade job running");
   }
