// Strctures.cs
// Functions for creating strcutures in mining mod

if (isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
  {
   if (!$RTB::RTBR_ServerControl_Hook)
     exec("Add-Ons/System_ReturnToBlockland/RTBR_ServerControl_Hook.cs");

   RTB_RegisterPref("Inv Cement Cost","Unlimited Mining", "$Dig_CementCostt","int 1 5000","GameMode_UnlimitedMining",$Dig_CementCost,0,0);
  }
else
  {
   $Dig_CementCost=$Dig_CementCost;
  }

$Dig_CementColor=50;

// cement placer placed a brick
function DiggingCementImage::onHitObject(%this, %obj, %col, %brick, %pos, %normal)
  {
   if ( %obj.client.structure==1)
     {
      Dig_Structure(%obj, %brick, %normal, false);
      return;
     }
   Dig_DropCement(%obj.client, %brick, %normal, false);
  }

// inv cement placer placed a brick
function BlockadeImage::onHitObject(%this, %obj, %col, %brick, %pos, %normal)
  {
   if ( %obj.client.structure==1)
     {
      Dig_Structure(%obj, %brick, %normal, true);
      return;
     }
   Dig_DropCement(%obj.client, %brick, %normal, true);
  }

// create a cement block
function Dig_DropCement(%client, %brick, %normal, %invuln)
  {
   if ( !$Dig_on)
     {
      return;
     }

   if (%brick.isMineable || %brick.isCement)
     {
      // spam protection
      if ( %client.canPlaceCement == 1)
        {
         return;
        }
      if ( %client.dirt.lessThan(5) )
        {
         Dig_DisplayError(%client, "5 Dirt<color:FFFFFF> required to place a cement. you have " @ %client.dirt.Display() );
         return;
        }
      if ( %invuln)
        {
         if ( %client.getMoney().lessThan($Dig_CementCost) )
           {
            Dig_DisplayError(%client, "$$Dig_CementCost<color:FFFFFF> required to place an invulnerable cement. you have " @ %client.getMoney().display() );
            return;
           }
        }

      %pos = getWords(%brick.getTransform(),0,2);
      %pos2 = vectorAdd(%pos,vectorScale(%normal,2));
      if ( !Dig_CanPlaceCement(%pos2))
  			{
         Dig_DisplayError(%client, "ERROR<color:FFFFFF>Cant place cement there");
         return;
        }

			if ( isObject($Dig_placedDirt[%pos2]) )
			  {
				 // somebody already placed a block
				 return;
			  }
      %brick = Dig_PlaceCement(%pos2, "brick4xCubeData", %client);
      if ( %invuln)
        {
         %brick.owner = %client;
         %client.AddMoney(-$Dig_CementCost, "Inv Cement");
        }

      %brick.isCement=1;
      %brick.health=200+%client.getPick();
      if ( %client.isAdmin)
			  {
         // admin powers!
				 %brick.health*=2;
			  }
      %client.dirt.subtract(5);
      %client.sendMiningStatus();

      // let client place another brick in 1.5 seconds.  prevents spamming
      if ( %client.isAdmin)
        {
         %client.canPlaceCement=0;
        }
			else
				{
         %client.canPlaceCement=1;
         %client.schedule(1500,"Dig_EnableCement");
				}
     }
  }

// re-enable cement placing for %client
function GameConnection::Dig_enableCement(%client)
  {
   %client.canPlaceCement=0;
  }

// return true if its ok to place cement at %pos
function Dig_CanPlaceCement(%pos)
   {
    if ( !$Dig_on)
      {
       return false;
      }
    %brick = $Dig_PlacedDirt[%pos];
    if ( %brick == -3)
      {
       // Cant place cement in disabled spots
       return false;
      }
    if ( %brick.isCement) // already a cement block in place
      {
       return false;
      }

    for ( %a=0; %a < PlayerDropPoints.getcount(); %a++)
      {
       %spawn = PlayerDropPoints.getObject(%a);
       %pos2 = VectorSub(%spawn.position, %pos);
       if ( VectorLen(%pos2) < 25)
         {
          return false;
         }
      }
    return true;
   }

// projectile hit cement - apply damage
function Dig_DamageCement(%brick, %projectile, %obj)
  {
    if ( !$Dig_on)
      {
       return false;
      }
   if ( %brick.owner > 0)
     {
      if ( %brick.getGroup().bl_id != %obj.client.bl_id)
        {
         Dig_DisplayError(%obj.client, "You cant kill that brick");
         return;
        }
     }

   if ( %brick.isCement == 1)
     {
      %brick.health-= %projectile.DirectDamage;
      if ( %brick.health < 0)
        {
         if ( %obj.client.getMoney().lessThan(50) )
           {
            Dig_DisplayError(%obj.client, "you need $50 to destroy a cement block");
            %brick.health+= %projectile.DirectDamage;
            return;
           }
         %obj.client.AddMoney( -50, "kill cement");
         %oldType=%brick.oldType;

          // cement brick used to be something else -- change it back
         if ( %brick.oldType > 1)
           {
            //echo("brick.oldType: " @ %brick.oldType);
            %brick.type=%oldType;
            %brick.setColor(%brick.type.color);
            %brick.setColorFX(%brick.type.colorFx);
            %brick.setShapeFX(%brick.type.ShapeFx);
            %brick.health = %brick.type.value * 4;
            %brick.isCement=0;
            %brick.isMineable=1;
           }
         else
           {
            if ( strlen(%brick.oldType) > 4)
              {
               %data   = getField(%brick.oldType, 1);
               %newType   = getWord(%data, 0);
               %brick.type = %newType;

            %brick.setColor(%brick.type.color);
            %brick.setColorFX(%brick.type.colorFx);
            %brick.setShapeFX(%brick.type.ShapeFx);
            %brick.health = %brick.type.value * 4;
            %brick.isCement=0;
            %brick.isMineable=1;

               return %newBrick;
              }

           %pos = %brick.getPosition();
           $Dig_placedDirt[%pos] = -2;   // flag as deleted
           %brick.delete();

            if ( %oldType $= "")
              {
               Dig_PlaceDirt(%pos);
              }
           }

         %obj.client.sendMiningStatus();
        }
     }
  }

// place cement bricks for the spawn area
function Dig_PlaceCement(%pos, %db, %client)
  {
   if ( !isObject(%client) )
     {
      %client = $Dig_Host;
     }
   //else
   //  {
   //   if ( !isObject(%client.brickgroup) )
   //     {
   //      %client.brickGroup = new SimSet("Brickgroup_"@ %client.bl_id)
   //        {
   //         bl_id = %client.bl_id;
   //         name="\c1BL_ID: " @ %client.bl_id @ "\c1\c0";
   //         };
   //
   //      MainBrickGroup.add(%client.brickgroup);
   //      error("made new brickgroup for " @ %client.getPlayerName() );
   //     }
   //  }

    //%pos1 = VectorAdd(%pos, $BrickOffset);
    %brick = new fxDTSBrick()
      {
       client = %client;
       datablock = %db;
       position = %pos;
       rotation = "0 0 0 0";
       colorID = $Dig_CementColor;
       scale = "1 1 1";
       angleID = "0";
       colorfxID = "0";
       shapefxID = "0";
       isPlanted = 1;
       stackBL_ID = %client.bl_id;
     };

   %brick.plant();
   //echo("plant result: " @ %r @ " Dig_PlaceCement(\"" @ %pos @ "\" , " @ %db.getName() @ " , " @ %client @ ");");
   //echo("client: " @ %client.getPlayerName() );

   %brick.setTrusted(1);
   %brick.oldType=$Dig_PlacedDirt[%pos];
   $Dig_PlacedDirt[%pos] = %brick;
   %client.brickgroup.add(%brick);
	 return %brick;
  }

// place a "platform" cement block at %pos
function Dig_PlacePlatformCement(%pos, %client, %invuln)
   {
    if ( !Dig_CanPlaceCement(%pos))
      return;

    %brick = $Dig_PlacedDirt[%pos];
    if ( %brick.isCement)
      {
       return;  // already a cement brick, dont do anything
      }

    if ( %client.dirt.LessThan(5) )
      {
       return false;
      }
    if ( %invuln)
      {
       if ( %client.getMoney().lessThan($Dig_CementCost) )
         {
          return false;
         }
       %client.getMoney().subtract($Dig_CementCost);
      }

    %client.dirt.subtract(5);

		if ( isObject(%brick) )
		  {
       // existing brick -- change it to a cement block
       if ( !isObject(%brick.oldType))
         {
           %brick.oldType = %brick.type;
         }
       %brick.isCement=true;
       %brick.isMineable = 0;
       %brick.type = 0;
       %brick.setColor($Dig_CementColor);
       %brick.setColorFx(0);
		  }
		else
			{
       %brick = Dig_PlaceCement(%pos, "brick4xCubeData", %client);
       %brick.isCement=true;
			}

    if ( %invuln)
      {
       %brick.owner = %client;
      }

    %brick.health=200 + %client.getPick();
   }

//dispatcher for structure placement
function Dig_Structure(%obj, %brick, %normal, %invuln)
   {
    if ( !$Dig_on)
      {
       return;
      }

    %pos = getWords(%brick.getTransform(),0,2);
    %pos2 = vectorAdd(%pos,vectorScale(%normal,2));
    if ( $Dig_PlacedDirt[%pos2] == -3 || !%obj.client.canFire)
      {
       Dig_DisplayError(%obj.client, "ERROR<color:FFFFFF>Cant place Cement there");
       return false;
      }

    %client = %obj.client;
    if ( %client.platform ==1)
		  {
       if ( !Dig_Platform(%client, %brick, %pos2, %normal, %invuln) )
			   return;
		  }
    if ( %client.wall==1)
		  {
       if ( !Dig_Wall(%client, %brick, %pos2, %normal, %invuln))
			   return;
		  }
    if ( %client.platform2==1)
		  {
       if ( !Dig_platform2(%client, %brick, %pos2, %normal, %invuln))
			   return;
		  }
    if ( %client.room==1)
		  {
       if ( !Dig_room(%client, %brick, %pos2, %normal, %invuln) )
			   return;
		  }
    if ( %client.teleport==1)
      {
       if (!Dig_Teleport(%client, %brick, %pos2, %normal, %invuln) )
         return;
      }
    %client.sendMiningStatus();
    %client.structure=0;
   }

// enable placing a platform
function serverCmdPlatform(%client, %x, %y)
   {
    %xd = getParamNumber(%x,1, 25);
    %yd = getParamNumber(%y,1, 25);

		%client.platform=1;
		%client.structure=1;
		%client.XD=%xd;
		%client.YD=%yd;
		messageClient(%client, '', "<color:00FF00>Platform: " @ %xd @ " by " @ %yd);
   }

// enable placing a wall
function serverCmdWall(%client, %x, %y)
   {
    %xd = getParamNumber(%x,1, 25);
    %yd = getParamNumber(%y,1, 25);

		%client.Wall=1;
    %client.structure=1;
		%client.XD=%xd;
		%client.YD=%yd;
		messageClient(%client, '', "<color:00FF00>Wall " @ %xd @ "Tall by " @ %yd @ " Long");
   }

// enable placing a dual platform
function serverCmdPlatform2(%client, %x, %y, %z)
   {
    %xd = getParamNumber(%x,1, 15);
    %yd = getParamNumber(%y,1, 15);
    %xh = getParamNumber(%z,1, 15);

    %client.platform2=1;
    %client.structure=1;
    %client.XD=%xd;
    %client.YD=%yd;
    %client.XH=%xh;
    messageClient(%client, '', "<color:00FF00>Platform #2: " @ %xd @ " by " @ %yd @ " and " @ %xh @ " high");
   }

// enable creating a room
function serverCmdRoom(%client, %x, %y, %z)
   {
    %xd = getParamNumber(%x,1, 25);
    %yd = getParamNumber(%y,1, 25);
    %xh = getParamNumber(%z,1, 25);

    %client.room=1;
    %client.structure=1;
    %client.XD=%xd;
    %client.YD=%yd;
    %client.XH=%xh;
    messageClient(%client, '', "<color:00FF00>Room: " @ %xd @ " by " @ %yd @ " and " @ %xh @ " high");
   }

// setup to place the teleporter brick
function serverCmdTeleporter(%client)
   {
    if ( isObject(%client.teleporter))
      {
       messageClient(%client, '', "<color:00FF00>You already have a teleporter.  say: <color:FFFFFF>/teleport<color:00FF00> to use it");
       return;
      }
    %id = DiggingCementItem.getID();
    for ( %x=0; %x <= 4; %x++ )
      {
       if ( %client.player.tool[%x] == %id)
         {
          %client.teleport=1;
          %client.structure=1;
          messageClient(%client, '', "<color:00FF00>Teleporter placement enabled.  Place your personal teleporter anywhere");
          return;
         }
      }
    messageClient(%client, '', "<color:FF0000>you need to buy a cement placer before you can place a teleporter");
   }

// create a Wall out of cement of size x,y starting at %pos
function Dig_Wall(%client, %brick, %pos2, %normal, %invuln)
   {
    %pos = getWords(%brick.getTransform(),0,2);
		if ( getWord(%normal, 2) != 1)
		  {
       Dig_DisplayError(%client, "Please shoot a Floor");
			 return false;
		  }

    %id = getAngleIDFromPlayer(%client.player);
		switch(%id)
		  {
			 case 0 :
				 %xN = -1;
				 %yn = 0;
			 case 1 :
				 %xN = 0;
				 %yn = -1;
			 case 2 :
				 %xN = 1;
				 %yn = 0;
			 case 3 :
				 %xN = 0;
				 %yn = 1;
      }
    %client.wall=0;
    Dig_MakeWall(%pos2, %client.YD, %client.XD, %xn, %yn, %client, %invuln);
		return true;
	 }

// make a wall
function Dig_MakeWall(%pos2, %length, %height, %xN, %yN, %client, %invuln)
   {
    AddLog(%client, "Wall", %length SPC %height, 1);
		%x = getWord(%pos2, 0);
		%y = getWord(%pos2, 1);
		%z = getWord(%pos2, 2);

		for ( %len=0; %len < %length; %len++)
		  {
			 for ( %z1=0; %z1 < %height; %z1++)
			   {
					%xc = (%len*2) * %xN;
					%yc = (%len*2) * %yN;

					%pos2 = %x + %xc SPC %y + %yc SPC %z+ (%z1*2);
          Dig_PlacePlatformCement(%pos2, %client, %invuln);
         }
		  }
   }

// create a platform out of cement of size x,y starting at %pos
function Dig_Platform(%client, %brick, %pos2, %normal, %invuln)
   {
    %pos = getWords(%brick.getTransform(),0,2);
		if ( getWord(%normal, 2) != 0)
		  {
       Dig_DisplayError(%client, "Please shoot a wall");
			 return false;
		  }

    AddLog(%client, "Platform", %client.xd SPC %client.yd, 1 );

		%cross = VectorCross("0 0 1", %normal);
		%newVec = VectorAdd(%normal, %cross);

		%xN = getWord(%newVec, 0);
		%yN = getWord(%newVec, 1);
    Dig_MakePlatform(%pos2, %normal, %client.xD, %client.yD, %xN, %yN, %client, %invuln);
    %client.platform=0;
		return true;
	 }

// create the actual platform
function Dig_MakePlatform(%pos2, %normal, %xD, %yD, %xN, %yN, %client, %invuln)
   {
		%x = getWord(%pos2, 0);
		%y = getWord(%pos2, 1);
		%z = getWord(%pos2, 2);

		for ( %x1=0; %x1 < %xD; %x1++)
		  {
			 for ( %y1=0; %y1 < %yD; %y1++)
			   {
					%xc = (%x1*2) * %xN;
					%yc = (%y1*2) * %yN;

					%pos2 = %x + %xc SPC %y + %yc SPC %z;
		 	    %brick = $Dig_PlacedDirt[%pos2];
          Dig_PlacePlatformCement(%pos2, %client, %invuln);
			   }
		  }
   }

// make 2 platforms, one above the other
function Dig_Platform2(%client, %brick, %pos2, %normal, %invuln)
   {
    %pos = getWords(%brick.getTransform(),0,2);
		if ( getWord(%normal, 2) != 0)
		  {
       Dig_DisplayError(%client, "Please shoot a wall");
			 return false;
		  }

    AddLog(%client, "Platform2", %client.xd SPC %client.yd SPC %client.XH, 1 );

		%cross = VectorCross("0 0 1", %normal);
		%newVec = VectorAdd(%normal, %cross);

		%xN = getWord(%newVec, 0);
		%yN = getWord(%newVec, 1);
    Dig_MakePlatform(%pos2, %normal, %client.xD, %client.yD, %xN, %yN, %client, %invuln);
    %pos2 = vectorAdd(%pos2, "0 0 " @ %client.XH*2);
    Dig_MakePlatform(%pos2, %normal, %client.xD, %client.yD, %xN, %yN, %client, %invuln);

    %client.platform2=0;
		return true;
	 }

// make room with outside dimensions set by the client
function Dig_Room(%client, %brick, %pos2, %normal, %invuln)
   {
    %pos = getWords(%brick.getTransform(),0,2);
		if ( getWord(%normal, 2) != 0)
		  {
       Dig_DisplayError(%client, "Please shoot a wall");
			 return false;
		  }

		%cross = VectorCross("0 0 1", %normal);
		%newVec = VectorAdd(%normal, %cross);
    AddLog(%client, "Room", %client.XD SPC %client.YD SPC %client.XH, 1);

		%xN = getWord(%newVec, 0);
		%yN = getWord(%newVec, 1);
    // floor
    Dig_MakePlatform(%pos2, %normal, %client.xD, %client.yD, %xN, %yN, %client, %invuln);
    %pos3 = vectorAdd(%pos2, "0 0 " @ (%client.XH-1)*2);

    // ceiling
    Dig_MakePlatform(%pos3, %normal, %client.xD, %client.yD, %xN, %yN, %client, %invuln);
		%floor= VectorAdd(%pos2, "0 0 2");

    // walls
		%x = getWord(%pos2, 0);
		%y = getWord(%pos2, 1);
		%z = getWord(%floor, 2);

    for ( %x1=0; %x1 < %client.xD; %x1++)
		  {
       for ( %y1=0; %y1 < %client.yD; %y1++)
			   {
          if ( !(( %x1> 0 && %x1 < (%client.XD-1) ) && (%y1 > 0 && %y1 < (%client.yD-1) )) )
					  {
             for ( %z1=0; %z1 < %client.XH-2; %z1++)
						   {
					      %xc = (%x1*2) * %xN;
					      %yc = (%y1*2) * %yN;

                if (!( %x1==1 && %y1 >0 && %z1 <2))
                  {
                   %pos2 = %x + %xc SPC %y + %yc SPC %z+(%z1*2);
                   %brick = $Dig_PlacedDirt[%pos2];
                   Dig_PlacePlatformCement(%pos2, %client, %invuln);
                  }
							 }
					  }
			   }
		  }

    %client.room=0;
		return true;
   }

// place a teleporter brick
function Dig_Teleport(%client, %brick, %pos2, %normal)
   {
    %pos = getWords(%brick.getTransform(),0,2);
    if ( getWord(%normal, 2) != 1)
      {
       Dig_DisplayError(%client, "Please shoot a Floor");
       return false;
      }

    if ( isObject($Dig_placedDirt[%pos2]) )
      {
       // somebody already placed a block
       return false;
      }
    if ( isObject($Dig_PlacedDirt[ getWord(%pos2, 0) SPC getWord(%pos2, 1) SPC getWord(%pos2,2)+2] ) )
      {
       Dig_DisplayError(%client, "Insufficent clearance for teleporter");
       return;
      }
    if ( isObject($Dig_PlacedDirt[ getWord(%pos2, 0) SPC getWord(%pos2, 1) SPC getWord(%pos2,2)+4] ) )
      {
       Dig_DisplayError(%client, "Insufficent clearance for teleporter");
       return;
      }

    %client.teleport=0;

    %brick = Dig_PlaceCement(%pos2, "brick4xCubeData", %client);
    %brick.isTeleporter=true;
    %brick.isCement=true;
    %brick.owner=%client;
    %brick.health=200+%client.getPick();
    if (isObject(%brick))
      {
       %client.dirt.subtract(5);
       $Dig_placedDirt[%brick.getPosition()] = -1;   // flag as occupied
      }
    %client.sendMiningStatus();
    %client.teleporter=%brick;
    %brick.setName("Teleporter" @ %client.bl_id);

    messageClient(%client, '', "<color:00FF00>Teleporter placed.  say /teleport to use");
    AddLog(%client, "teleporter", "place teleport brick", 1);
   }

// Actually USE the teleporter
function serverCmdTeleport(%client)
   {
    if ( !isObject(%client.teleporter))
      {
       messageClient(%client, '', "<color:00FF00>You do not have a teleporter placed");
       return;
      }
    if ( %client.dirt.lessThan(5) )
      {
       Dig_DisplayError(%client, "5 Dirt<color:FFFFFF> required to use your teleporter. you have " @ %client.dirt.display() );
       return;
      }

    %client.AddDirt( -5);
    %brick = %client.teleporter;

    // set vars so setplayerTransform will work
    %brick.timeout=0;
    %brick.lastuserr = %client.player;
    %client.player.lasttelbrick=%brick;
    %client.player.lasttelbricktouched=%brick;

    %brick.setPlayerTransform(0, 0, 0, %client);
    AddLog(%client, "teleporter", "use teleport brick", 1);
   }

// invite somebody else to use your teleporter
function serverCmdInviteTeleport(%client, %player)
   {
    if ( !isObject(%client.teleporter))
      {
       messageClient(%client, '', "<color:00FF00>You do not have a teleporter placed");
       return;
      }
    %time = getSimtime();
    if ( %client.lastTeleport > 0)
      {
      //echo(%client.getPlayerName() SPC %client.lastDonate SPC %time SPC (%time - %client.lastDonate) );
       if ( (%time - %client.lastTeleport) < 5000)
         {
          Dig_DisplayError(%client, "Please wait before sendinig a teleport invite");
          return false;
         }
     }
    %client.lastTeleport = %time;

    %newClient = findclientbyname(%player);
    if ( !isObject(%newClient) )
      {
       MessageClient(%client, '', "<color:FF0000>Nobody named <color:FFFFFF>" @ %player @ "<color:FF0000> is playing");
       return;
      }
    // both players must have 5 dirt
    if ( %client.dirt.lessThan(5) )
      {
       Dig_DisplayError(%client, "5 Dirt<color:FFFFFF> required to use your teleporter. you have " @ %client.dirt.display() );
       return;
      }
    if ( %newClient.dirt.lessThan(5) )
      {
       Dig_DisplayError(%client, "<color:FFFFFF>" @ %newClient.getPlayerName() @" needs 5 Dirt to use your teleporter. They have " @ %newClient.dirt.Display() );
       return;
      }
    MessageClient(%client, '', "<color:FFFFFF>You invite <color:00FF00>" @ %newClient.getPlayerName() @ "<color:FFFFFF> to use your teleporter for 1 time");
    AddLog(%client, "Teleporter", "send teleport invite | " @ %newClient.getPlayerName(), 1 );
    commandToClient(%newClient,'MessageBoxYesNo', "Teleporter", "Teleport to " @ %client.getPlayername() @ "'s Teleporter?", 'teleportConfirm');
    %newClient.teleportFrom = %client;
   }

function serverCmdTeleportConfirm(%client)
   {
    %client.teleportFrom.addDirt(-5);
    %client.addDirt(-5);

    %brick = %client.teleportFrom.teleporter;

    // set vars so setplayerTransform will work
    %brick.timeout=0;
    %brick.lastuserr = %client.player;
    %client.player.lasttelbrick=%brick;
    %client.player.lasttelbricktouched=%brick;

    %brick.setPlayerTransform(0, 0, 0, %client);
    AddLog(%client, "teleporter", "accept teleport invite | " @ %client.teleportFrom.getPlayerName() , 1);
   }


// Write out the spawn location coordinates
function MakeSpawnLocation(%pos)
   {
    %xo = getWord(%pos, 0);
    %yo = getWord(%pos, 1);
    %zo = getWord(%pos, 2);
    %count=1;
    for (%x = -5.5; %x <= 5.5; %x++)
      {
       for (%y = -5.5; %y <= 5.5; %y++)
         {
          for ( %z=2; %z < 10; %z+=2 )
            {
             %pos1 = (%x*2) + %xo SPC (%y*2) + %yo SPC %zo+%z;
             echo("$SpawnLocation[" @ %count @ "]=\"" @ %pos1@ "\" TAB \"Dirt\";" ); %count++;
             Dig_placeDirt(%pos1);
            }

          %pos1= (%x*2) + %xo SPC (%y*2) + %yo SPC %zo; Dig_PlaceCement( %pos1, "brick4xCubeData");
          echo("$SpawnLocation[" @ %count @ "]=\"" @ %pos1@ "\" TAB \"Cement\";" ); %count++;

          %pos1= (%x*2) + %xo SPC (%y*2) + %yo SPC (10+%zo); Dig_PlaceCement( %pos1, "brick4xCubeData");
          echo("$SpawnLocation[" @ %count @ "]=\"" @ %pos1@ "\" TAB \"Cement\";" ); %count++;
         }
      }

    %pos[0] = 10+%xo SPC 10+%yo;
    %pos[1] = 10+%xo SPC (-10) + %yo;
    %pos[2] = (-10)+%xo SPC 10+%yo;
    %pos[3] = (-10)+%xo SPC (-10)+%yo;

    for (%a = 0; %a < 4; %a++)
      {
       Dig_PlaceCement(%pos[%a] SPC 3+%zo, "brick8xCubeData");
       echo("$SpawnLocation[" @ %count @ "]=\"" @ %pos[%a] SPC 3 + %zo @ "\" TAB \"Cement8\";" ); %count++;

       Dig_PlaceCement(%pos[%a] SPC 7+%zo, "brick8xCubeData");
       echo("$SpawnLocation[" @ %count @ "]=\"" @ %pos[%a] SPC 7 + %zo @ "\" TAB \"Cement8\";" ); %count++;
      }

    %light = Dig_PlaceCement(%xo SPC %yo SPC %zo + 8.75, "brick2x2FRoundData");
    %light.setLight(PlayerLight);
    echo("$SpawnLocation[" @ %count @ "]=\"" @ %xo SPC %yo SPC %zo+8.75 @ "\" TAB \"Light\";" ); %count++;

    echo("$SpawnLocation[0]=" @ %count @ ";");
   }

// generate a set of spawn location bricks
exec("./SpawnData.cs");
function GenSpawnLocation(%pos)
   {
    for ( %a=1; %a <= $SpawnLocation[0]; %a++)
      {
       %rec = $SpawnLocation[%a];
       if ( strlen(%rec) > 1)
         {
          %org = getField(%rec, 0);
          %thing = getField(%rec, 1);
          %pos1 = VectorAdd(%pos, %org);
          switch$ (%thing)
            {
             case "Empty" :
               $Dig_PlacedDirt[ %pos1 ] = -3;
             case  "Dirt" :
               Dig_PlaceDirt(%pos1);
             case  "Cement" :
               Dig_PlaceCement( %pos1, "brick4xCubeData");
             case  "Cement8" :
               Dig_PlaceCement( %pos1, "brick8xCubeData");
             case  "Light" :
               %brick = Dig_PlaceCement( %pos1, "brick2x2FRoundData");
               %brick.setLight(PlayerLight);

            }
         }
      }

    %trigger = new Trigger()
       {
        canSaveDynamicFields = "1";
        position = VectorAdd(%pos , "-15 15 -5");
        rotation = "1 0 0 0";
        scale = "30 30 20";
        dataBlock = "NoFireZoneTrigger";
        polyhedron = "0.0000000 0.0000000 0.0000000 1.0000000 0.0000000 0.0000000 0.0000000 -1.0000000 0.0000000 0.0000000 0.0000000 1.0000000";
       };
    MissionCleanup.add(%trigger);

    if ( %pos !$= "0 0 0")
      {
       %spawner = new SpawnSphere()
         {
          rotation = "1 0 0 0";
          scale = "1 1 1";
          position = %pos;
          dataBlock = "SpawnSphereMarker";
          radius = "5";
          sphereWeight = "1";
          indoorWeight = "1";
          outdoorWeight = "1";
          RayHeight = "3";
         };
       PlayerDropPoints.add(%spawner);
      }
   }

// nofirezone triggers
function NoFireZoneTrigger::onEnterTrigger(%this,%trigger,%obj)
  {
   Parent::onEnterTrigger(%this,%trigger,%obj);
   %obj.client.CanFire=false;
  }

function NoFireZoneTrigger::onLeaveTrigger(%this,%trigger,%obj)
  {
   %obj.client.CanFire=true;
   Parent::onLeaveTrigger(%this,%trigger,%obj);
  }
