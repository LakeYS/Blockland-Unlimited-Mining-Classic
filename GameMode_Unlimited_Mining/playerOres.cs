// Unlimited Mining Mod -- player discovered ores
OreData.DiscoveryColor = 5;
OreData.DiscoveryPercent = 10;

// check to see if %client has discovered a new ore
function Dig_CheckDiscoverOre(%client)
   {
    if ( getrandom(1,100) == 42)
      {
       if ( getrandom(1,100) == 42)
         {
          Dig_DiscoverOre(%client);
         }
      }
   }

// Player Discovers an ore
function Dig_DiscoverOre(%client)
   {
    for ( %a=0; %a < OreData.getCount(); %a++)
      {
       %newOre = OreData.getObject(%a);
       if ( %newOre.bl_id == %client.bl_id)
         return;
      }
    messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>Has discovered a NEW ORE!");
    messageClient(%client, '', "<color:FFFFFF>You have discovered a<color:00FF00>NEW ORE!");

    %newName = filterString(%client.getPlayerName() @ "ium","ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_");

    %depth = mFloor(getWord(%client.player.getPosition(), 2)) - $Brick_Z;
    messageClient(%client, '', "<color:FFFFFF>the new ore will be named after you: <color:00FF00>"@ %newName);
    messageClient(%client, '', "<color:FFFFFF>type: <color:00FF00>/renameore<color:FFFFFF> to change the name");
    // find the first MineOre instance and copy it
    %newOre = -1;
    for ( %a=0; %a < OreData.GetCount(); %a++)
      {
       %Ore = Oredata.getObject(%a);
       if ( %Ore.getName() $= "MineOre")
         {
          %newOre = %ore;
          break;
         }
      }

    %newOre = %newOre.copy();
    %newOre.bl_id = %client.bl_id;
    %newOre.value = Oredata.getCount() * 15;
    %newOre.color = OreData.DiscoveryColor;
    %newOre.veinLength=5;
    %newOre.minPercent = OreData.DiscoveryPercent;
    %newOre.maxPercent = OreData.DiscoveryPercent + 0.1;
    %newOre.name = %newName;
    %newOre.depth = %depth;
    %newOre.timeout=20;
    %newOre.minPick=mFloor(%client.getPick() * 0.01);

    OreData.DiscoveryColor++;
    if ( Oredata.DiscoveryColor > 60)
      OreData.DiscoveryColor=1;

    OreData.DiscoveryPercent+=0.1;
    if (OreData.DiscoveryPercent >= 100)
      OreData.DiscoveryPercent=1;

    OreData.add(%newOre);
    OreData.save("config/server/miningData.cs");
    AddLog(%client, "Discover ore ", %newOre.name);

    %pos = %client.player.getPosition();
    %x = mFloor(getWord(%pos, 0) / 2) * 2;
    %y = mFloor(getWord(%pos, 1) / 2) * 2;
    %z = mFloor(getWord(%pos, 2) / 2) * 2;
    %x++; %y++;

    //MakeOreBlob(%newOre, %x SPC %y SPC %z);
   }

// find the player's ore
function serverCmdRenameOre(%client, %oreName)
   {
    %newOre = -1;
    for ( %a=0; %a < OreData.getCount(); %a++)
      {
       %ore = OreData.getObject(%a);
       if ( %ore.bl_id == %client.bl_id)
         {
          %newOre = %ore;
          break;
         }
      }
    if ( %newOre == -1)
      {
       Dig_DisplayError(%client, "You do not have a discovered Ore to rename");
       return;
      }
    if ( %newOre.disabled == 2)
      {
       Dig_DisplayError(%client, "your ability to rename your ore has been revoked");
       return;
      }

     %newName = filterString(%oreName,"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_");

    if ( strlen(%newName) < 2)
      {
       Dig_DisplayError(%client, "That ore name is too short");
       return;
      }
    if ( strlen(%newName) > 30)
      {
       Dig_DisplayError(%client, "That ore name is too long");
       return;
      }
    %result = chatFilter(0, %oreName, $Pref::Server::ETardList);
    if ( %result == 0)
      {
       Dig_Displayerror(%client, "That ore name is not acceptable");
       return;
      }

    for ( %a=0; %a < OreData.getCount(); %a++)
      {
       %ore = OreData.getObject(%a);
       if ( %ore.name $= %newName )
         {
          Dig_Displayerror(%client, "That ore name is taken");
          return;
         }
      }

    messageClient(%client, '', "<color:FFFFFF>Your ore will now be known as " @ %newName);
    %newOre.name=%newName;
    OreData.save("config/server/miningData.cs");
    AddLog(%client, "rename ore" , %newOre.name, 1);
   }

// check if %client has a discovered ore and display some info about it
function serverCmdCheckOre(%client)
   {
	  // find the player's ore
	  %newOre = -1;
	  for ( %a=0; %a < OreData.getCount(); %a++)
	    {
 			 %ore = OreData.getObject(%a);
			 if ( %ore.bl_id == %client.bl_id)
			   {
				  %newOre = %ore;
				  break;
			   }
	    }
	  if ( %newOre == -1)
	    {
			 Dig_DisplayError(%client, "You do not have a discovered Ore");
			 return;
	    }
    %msg =        "<color:00FF00>Your Ore: <color:FFFFFF>" @ %NewOre.name;
    %msg = %msg @ "<color:00FF00> value: <color:FFFFFF>" @ %newOre.value;
    %msg = %msg @ "<color:00FF00> depth: <color:FFFFFF>" @ %newOre.depth;
    %msg = %msg @ "<color:00FF00> ColorID: <color:FFFFFF>" @ %newOre.color;

		messageClient(%client, '', %msg);
   }

// check existing player ores and remove any which belong to a player who has not logged in recently
function Dig_ExpireOres()
   {
		%size = OreData.getCount();
		if ( %size < 80)
      {
       echo("Ore count < 80 - no expire");
       return;
      }

    %toRemove= new SimSet();
		for ( %a=0; %a < %size; %a++)
		  {
			 %ore = OreData.getObject(%a);
			 if ( %ore.bl_id > 0)
			   {
					%ore.timeout--;
					echo(%ore.name @ " timeout: " @ %ore.timeout);
					if ( %ore.timeout < 1)
					  {
             %toRemove.add(%ore);
					  }
			   }
		  }
    for ( %a=0; %a < %toRemove.getCount(); %a++)
      {
       %ore = %toRemove.getObject(%a);
       OreData.remove(%ore);
       AddLog(0, "Expire Ore", %ore.name, 1);
      }
    OreData.save("config/server/miningData.cs");
    %toRemove.clear();
   }

// %client has logged in- check ores and increment their owned ore so it doesnt expire
function Dig_CheckLogin(%client)
   {
		echo("check ores for " @ %client.getPlayerName() );
		%size = Oredata.getCount();
		for ( %a=0; %a < %size; %a++)
		  {
			 %ore = OreData.getObject(%a);
			 if ( %ore.bl_id == %client.bl_id)
			   {
          %ore.timeout+=2;
					echo(%ore.name @ " timeout: " @ %ore.timeout);
          if ( %ore.timeout > 20)
					  {
             %ore.timeout = 20;
					  }
					break;
			   }
		  }
   }

function listOres()
   {
    for ( %a=0; %a < OreData.getCount(); %a++)
      {
       %ore = Oredata.getObject(%a);
       echo(%a SPC %ore.name SPC %ore.value);
      }
   }
function removeOre(%number)
   {
    OreData.remove(OreData.getObject(%number));
   }

function reloadOres()
   {
    echo("reloading oreData");
    OreData.delete();
    exec("config/server/MiningData.cs");
   }
