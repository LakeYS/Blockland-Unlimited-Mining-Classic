// Drill.cs
// Mining drill

// max # of drills at once
$Dig_Data_DrillLimit=200;
$Dig_Data_ActiveDrills=0;
$Dig_Data_DrillSchedule=300;
$Dig_Data_DrillEnabled=true;

// enable drill
function serverCmdDrill(%client, %depth, %size)
{
  if ( %depth ==0)
  {
    %client.MineDrill = 0;
    %client.MineDrillSize = 0;
    %client.MineDrillDisc = 0;
    $Dig_Data_ActiveDrills-=%client.ActiveDrill;
    %client.ActiveDrill = 0;
    %client.DrillCancel = 1;
    %client.MineOreDrill = 0;
    %client.MineOreDrillSize = 0;
    %client.MineDOrerillDisc = 0;

    Dig_DisplayError(%client, "Drill cancelled");
    return;
  }

  if ( !$Dig_Data_DrillEnabled)
  {
    Dig_DisplayError(%client, "Drills are disabled");
    return;
  }

  %client.MineDrill = mabs(mFloor(%depth));
  %client.MineDrillSize = mabs(mFloor(%size));
  if ( !%client.isAdmin && !$Dig_AdminCheats )
  {
    %client.minedrill = GetParamNumber(%client.mineDrill,1, 100+%client.getRank()*2 );
    %client.MinedrillSize = GetParamNumber(%client.mineDrillSize,0, 5+ mFloor(%client.getRank() / 10) );
  }

  %price = mFloor( %client.mineDrillSize * 2);
  if ( %client.dirt.lessThan(%price) )
  {
    Dig_DisplayError(%client, "You need " @ %price @ " Dirt to start a drill of that size");
    %client.mineDrill=0;
    %client.mineDrillSize=0;
    return;
  }

  if ( %client.MineDrillSize > 0)
  {
    messageClient(%client, '', "<color:00FF00>Drill Enabled for depth: " @ %client.MineDrill @ " and width " @ %client.MineDrillSize);
  }
  else
  messageClient(%client, '', "<color:00FF00>Drill Enabled for depth: " @ %client.MineDrill );
  %client.sendMiningStatus();
  %client.MineDrillDisc = 0;
}

function serverCmdOreDrill(%client, %depth, %size)
{
  if ( %depth ==0)
  {
    %client.MineDrill = 0;
    %client.MineDrillSize = 0;
    %client.MineDrillDisc = 0;
    $Dig_Data_ActiveDrills-=%client.ActiveDrill;
    %client.ActiveDrill = 0;
    %client.DrillCancel = 1;
    %client.MineOreDrill = 0;
    %client.MineOreDrillSize = 0;
    %client.MineDOrerillDisc = 0;
    Dig_DisplayError(%client, "Ore Drill cancelled");
    return;
  }

  if ( !$Dig_Data_DrillEnabled)
  {
    Dig_DisplayError(%client, "Drills are disabled");
    return;
  }

  %client.MineOreDrill = mabs(mFloor(%depth));
  %client.MineOreDrillSize = mabs(mFloor(%size));
  if ( !%client.isSuperAdmin )
  {
    %client.mineOreDrill = GetParamNumber(%client.mineOreDrill,1, 100+%client.getRank()*2 );
    %client.MineOreDrillSize = GetParamNumber(%client.mineOreDrillSize,0, 5+ mFloor(%client.getRank() / 10) );
  }

  %price = mFloor( %client.mineDrillSize * 100);
  if ( %client.dirt.lessThan(%price) )
  {
    Dig_DisplayError(%client, "You need " @ %price @ " Dirt to start a drill of that size");
    %client.mineOreDrill=0;
    %client.mineOreDrillSize=0;
    return;
  }

  if ( %client.MineOreDrillSize > 0)
  {
    messageClient(%client, '', "<color:00FF00>Ore Drill Enabled for depth: " @ %client.MineOreDrill @ " and width " @ %client.MineOreDrillSize);
  }
  else
  messageClient(%client, '', "<color:00FF00>Ore Drill Enabled for depth: " @ %client.MineOreDrill );
  %client.sendMiningStatus();
}

// enable disc drill
function serverCmdDrillD(%client, %size)
{
  if ( !$Dig_Data_DrillEnabled)
  {
    Dig_DisplayError(%client, "Drills are disabled");
    return;
  }

  %client.MineDrill = 1;
  %client.mineOreDrill = 0;
  %client.MineDrillSize = mabs(mFloor(%size));
  if ( !%client.isAdmin && !$Dig_AdminCheats )
  {
    %client.MinedrillSize = GetParamNumber(%client.mineDrillSize,0, 10+ mFloor(%client.getRank() / 10) );
  }
  %client.MineDrillDisc=%client.MineDrillSize;
  messageClient(%client, '', "<color:00FF00>Disc Drill Enabled for size: " @ %client.MineDrillSize );
}

// start the drilling process
function Dig_StartDrill(%brick, %pos1, %client, %normal)
{
  if ( %client.activeDrill > 0)
  {
    Dig_DisplayError(%client, "You still have an active drill -- Wait for it to finish");
    return;
  }
  if ( $Dig_Data_ActiveDrills > $Dig_Data_DrillLimit)
  {
    Dig_DisplayError(%client, "There are too many active drills right now.  please wait for some to finish");
    return;
  }
  if ( !isObject(%brick) )
  return;

  if ( %client.mineOreDrill > 1)
  {
    echo(%client.getPlayerName() @ " mineOreDrill " @ %client.mineOreDrill);
    return Dig_StartOreDrill(%brick, %pos1, %client, %normal);
  }
  %client.DrillCancel=0;
  if ( mabs(getWord(%normal, 2)) == 1)
  {
    Dig_StartDrilZ(%brick, %client, %normal);
  }
  if ( mabs(getWord(%normal, 1)) == 1)
  {
    Dig_StartDrilY(%brick, %client, %normal);
  }
  if ( mabs(getWord(%normal, 0)) == 1)
  {
    Dig_StartDrilX(%brick, %client, %normal);
  }
  if ( %client.mineDrillDisc > 0)
  {
    %client.disCBrick=%brick;
    %client.discPos=%pos1;
    %client.DiscNormal=%normal;
  }
  %client.MineOreDrill=0;
  %client.MineDrill=0;
}

function Dig_StartDrilX(%brick, %client, %normal)
{
  %bPos = %Brick.getPosition();
  AddLog(%client, "X drill","depth " @ %client.MineDrill @ " size " @ %client.mineDrillSize @ " at " @ VectorSub(%bpos, $BrickOffset), 1);
  %x = getWord(%bPos, 0);
  %y = getWord(%bPos, 1);
  %z = getWord(%bPos, 2);
  %drillSize = %client.MineDrillSize*2;
  %client.activeDrill=0;

  for ( %y1= %y-%drillSize; %y1 <= %y+%drillSize; %y1+=2)
  {
    for ( %z1= %z-%drillSize; %z1 <= %z+%drillSize; %z1+=2)
    {
      %pos = %x SPC %y1 SPC %z1;
      %brickToUse = $Dig_PlacedDirt[%pos];
      if ( isObject(%BrickToUse) )
      {
        %dist = VectorDist(%pos, %bPos);
        if ( %dist <= %drillSize)
        {
          %client.activeDrill++;
          $Dig_Data_ActiveDrills++;
          Dig_HighlightDrill(%brickToUse);
          schedule($Dig_Data_DrillSchedule*2, 0, "Dig_DoDrill", %brickToUse, %pos, %client, %normal, %client.MineDrill);
        }
      }
    }
  }
}

function Dig_StartDrilY(%brick, %client, %normal)
{
  %bPos = %Brick.getPosition();
  AddLog(%client, "Y drill","depth " @ %client.MineDrill @ " size " @ %client.mineDrillSize @ " at " @ VectorSub(%bpos, $BrickOffset), 1);
  %x = getWord(%bPos, 0);
  %y = getWord(%bPos, 1);
  %z = getWord(%bPos, 2);
  %drillSize = %client.MineDrillSize*2;
  %client.activeDrill=0;

  for ( %x1= %x-%drillSize; %x1 <= %x+%drillSize; %x1+=2)
  {
    for ( %z1= %z-%drillSize; %z1 <= %z+%drillSize; %z1+=2)
    {
      %pos = %x1 SPC %y SPC %z1;
      %brickToUse = $Dig_PlacedDirt[%pos];
      if ( isObject(%BrickToUse) )
      {
        %dist = VectorDist(%pos, %bPos);
        if ( %dist <= %drillSize)
        {
          %client.activeDrill++;
          $Dig_Data_ActiveDrills++;
          Dig_HighlightDrill(%brickToUse);
          schedule($Dig_Data_DrillSchedule*2, 0, "Dig_DoDrill", %brickToUse, %pos, %client, %normal, %client.MineDrill);
        }
      }
    }
  }
}

function Dig_StartDrilZ(%brick, %client, %normal)
{
  %bPos = %Brick.getPosition();
  AddLog(%client, "Z drill","depth " @ %client.MineDrill @ " size " @ %client.mineDrillSize @ " at " @ VectorSub(%bpos, $BrickOffset), 1);
  %x = getWord(%bPos, 0);
  %y = getWord(%bPos, 1);
  %z = getWord(%bPos, 2);
  %drillSize = %client.MineDrillSize*2;
  %client.activeDrill=0;

  for ( %x1= %x-%drillSize; %x1 <= %x+%drillSize; %x1+=2)
  {
    for ( %y1= %y-%drillSize; %y1 <= %y+%drillSize; %y1+=2)
    {
      %pos = %x1 SPC %y1 SPC %z;
      %brickToUse = $Dig_PlacedDirt[%pos];
      if ( isObject(%BrickToUse) )
      {
        %dist = VectorDist(%pos, %bPos);
        if ( %dist <= %drillSize)
        {
          %client.activeDrill++;
          $Dig_Data_ActiveDrills++;
          Dig_HighlightDrill(%brickToUse);
          schedule($Dig_Data_DrillSchedule*2, 0, "Dig_DoDrill", %brickToUse, %pos, %client, %normal, %client.MineDrill);
        }
      }
    }
  }
}


function Dig_DoDrill(%brick, %pos1, %client, %normal, %level)
{
  if ( ($Dig_on == 0) )
  {
    return; // dig mode is off, cancel drills
  }

  if ( %level < 1 || %client.drillCancel == 1)
  {
    Dig_DecrementDrill(%client);
    return;
  }
  if ( !isObject(%client) )
  {
    Dig_DecrementDrill(%client);
    return;
  }
  if ( !isObject(%brick) )
  {
    // nothing to mine here -- just go onto the next spot
    %pos2 = vectorSub(%pos1,vectorScale(%normal,2));
  }
  else
  {
    if ( !%brick.isMineable)
    {
      //Dig_DecrementDrill(%client);
      %client.activeDrill--;
      $Dig_Data_ActiveDrills--;
      return;
    }
    //echo("drill " @ %level);
    if ( %brick.getClassName() !$= "fxDTSBrick")
    {
      Dig_DecrementDrill(%client);
      return;
    }
    if ( $Dig_AdminCheats == false || %client.isSuperAdmin == false )
    {
      if ( %client.dirt.greaterThan(2))
      {
        %client.dirt.subtract(1);
      }
      else
      {
        if ( isObject(%client))
        {
          $Dig_Data_ActiveDrills-= %client.activeDrill;
          messageClient(%client, '', "<color:FFFF00>your drill has run out of dirt");
          %client.activeDrill=0;
          %client.MineDrillDisc=0;
          %client.sendMiningStatus();
        }

        $Dig_Data_ActiveDrills--;

        //echo("drill out of dirt " @ %client.getPlayerName() );
        return;
      }
    }

    %pos = %brick.getPosition();
    %pos2 = vectorSub(%pos,vectorScale(%normal,2));
    Dig_MineBrick(%brick, %pos, %client, "drill");
  }

  %newbrick = $Dig_placedDirt[%pos2];
  if ( %client.TurboDrill > 0)
  %delay = %client.TurboDrill;
  else
  %delay = $Dig_Data_DrillSchedule*2;

  schedule(%delay, 0, "Dig_DoDrill", %newBrick, %pos2, %client, %normal, %level - 1);
}

function Dig_StartOreDrill(%brick, %pos1, %client, %normal)
{

  %client.DrillCancel=0;
  if ( mabs(getWord(%normal, 2)) == 1)
  {
    Dig_StartOreDrilZ(%brick, %client, %normal);
  }
  if ( mabs(getWord(%normal, 1)) == 1)
  {
    Dig_StartOreDrilY(%brick, %client, %normal);
  }
  if ( mabs(getWord(%normal, 0)) == 1)
  {
    Dig_StartOreDrilX(%brick, %client, %normal);
  }
  %client.MineOreDrill=0;
  %client.MineDrill=0;
}

function Dig_StartOreDrilX(%brick, %client, %normal)
{
  %bPos = %Brick.getPosition();
  AddLog(%client, "X OreDrill","depth " @ %client.mineOreDrill @ " size " @ %client.mineOreDrillSize @ " at " @ VectorSub(%bpos, $BrickOffset), 1);
  %x = getWord(%bPos, 0);
  %y = getWord(%bPos, 1);
  %z = getWord(%bPos, 2);
  %drillSize = %client.mineOreDrillSize*2;
  %client.activeDrill=0;

  for ( %y1= %y-%drillSize; %y1 <= %y+%drillSize; %y1+=2)
  {
    for ( %z1= %z-%drillSize; %z1 <= %z+%drillSize; %z1+=2)
    {
      %pos = %x SPC %y1 SPC %z1;
      %brickToUse = $Dig_PlacedDirt[%pos];
      if ( isObject(%BrickToUse) )
      {
        %dist = VectorDist(%pos, %bPos);
        if ( %dist <= %drillSize)
        {
          %client.activeDrill++;
          $Dig_Data_ActiveDrills++;
          Dig_HighlightOreDrill(%brickToUse);
          schedule($Dig_Data_DrillSchedule*2, 0, "Dig_DoOreDrill", %brickToUse, %pos, %client, %normal, %client.mineOreDrill);
        }
      }
    }
  }
}

function Dig_StartOreDrilY(%brick, %client, %normal)
{
  %bPos = %Brick.getPosition();
  AddLog(%client, "Y oreDrill","depth " @ %client.mineOreDrill @ " size " @ %client.mineOreDrillSize @ " at " @ VectorSub(%bpos, $BrickOffset), 1);
  %x = getWord(%bPos, 0);
  %y = getWord(%bPos, 1);
  %z = getWord(%bPos, 2);
  %drillSize = %client.mineOreDrillSize*2;
  %client.activeDrill=0;

  for ( %x1= %x-%drillSize; %x1 <= %x+%drillSize; %x1+=2)
  {
    for ( %z1= %z-%drillSize; %z1 <= %z+%drillSize; %z1+=2)
    {
      %pos = %x1 SPC %y SPC %z1;
      %brickToUse = $Dig_PlacedDirt[%pos];
      if ( isObject(%BrickToUse) )
      {
        %dist = VectorDist(%pos, %bPos);
        if ( %dist <= %drillSize)
        {
          %client.activeDrill++;
          $Dig_Data_ActiveDrills++;
          Dig_HighlightOreDrill(%brickToUse);
          schedule($Dig_Data_DrillSchedule*2, 0, "Dig_DoOreDrill", %brickToUse, %pos, %client, %normal, %client.mineOreDrill);
        }
      }
    }
  }
}

function Dig_StartOreDrilZ(%brick, %client, %normal)
{
  %bPos = %Brick.getPosition();
  AddLog(%client, "Z oreDrill","depth " @ %client.mineOreDrill @ " size " @ %client.mineOreDrillSize @ " at " @ VectorSub(%bpos, $BrickOffset), 1);
  %x = getWord(%bPos, 0);
  %y = getWord(%bPos, 1);
  %z = getWord(%bPos, 2);
  %drillSize = %client.mineOreDrillSize*2;
  %client.activeDrill=0;
  for ( %x1= %x-%drillSize; %x1 <= %x+%drillSize; %x1+=2)
  {
    for ( %y1= %y-%drillSize; %y1 <= %y+%drillSize; %y1+=2)
    {
      %pos = %x1 SPC %y1 SPC %z;
      %brickToUse = $Dig_PlacedDirt[%pos];
      if ( isObject(%BrickToUse) )
      {
        %dist = VectorDist(%pos, %bPos);
        if ( %dist <= %drillSize)
        {
          %client.activeDrill++;
          $Dig_Data_ActiveDrills++;
          Dig_HighlightOreDrill(%brickToUse);
          schedule($Dig_Data_DrillSchedule*2, 0, "Dig_DoOreDrill", %brickToUse, %pos, %client, %normal, %client.mineOreDrill);
        }
      }
    }
  }
}


// Drill that goes around all the ore blocks
function Dig_DoOreDrill(%brick, %pos1, %client, %normal, %level)
{
  if ( ($Dig_on == 0) )
  {
    return; // dig mode is off, cancel drills
  }

  if ( %level < 1 || %client.drillCancel == 1)
  {
    Dig_DecrementDrill(%client);
    return;
  }
  if ( !isObject(%client) )
  {
    Dig_DecrementDrill(%client);
    return;
  }

  if ( !isObject(%brick) )
  {
    // nothing to mine here -- just go onto the next spot
    %pos2 = vectorSub(%pos1,vectorScale(%normal,2));
  }
  else
  {
    if ( !%brick.isMineable)
    {
      %client.activeDrill--;
      $Dig_Data_ActiveDrills--;
      return;
    }
    //echo("drill " @ %level);
    if ( %brick.getClassName() !$= "fxDTSBrick")
    {
      Dig_DecrementDrill(%client);
      return;
    }
    if ( $Dig_AdminCheats == false || %client.isSuperAdmin == false )
    {
      if ( %client.dirt.greaterThan(30))
      {
        %client.dirt.subtract(30);
      }
      else
      {

        if ( isObject(%client))
        {
          $Dig_Data_ActiveDrills-= %client.activeDrill;
          messageClient(%client, '', "<color:FFFF00>your ore drill has run out of dirt");
          %client.activeDrill=0;
          %client.MineDrillDisc=0;
          %client.sendMiningStatus();
        }

        $Dig_Data_ActiveDrills--;
        return;
      }
    }

    %pos = %brick.getPosition();
    %pos2 = vectorSub(%pos,vectorScale(%normal,2));
    if ( isObject(%brick.dirtType))
    {
      Dig_MineBrick(%brick, %pos, %client, "drill");
    }
    else
    {
      //echo("bypassing " @ %brick.type.name);
      Dig_rebuildDirt(%pos1);
      %client.stats.mineMoney.subtract(100);
    }
  }

  %newbrick = $Dig_placedDirt[%pos2];
  schedule($Dig_Data_DrillSchedule, 0, "Dig_DoOreDrill", %newBrick, %pos2, %client, %normal, %level - 1);
}


function Dig_DecrementDrill(%client)
{
  $Dig_Data_ActiveDrills--;
  if ( $Dig_Data_ActiveDrills < 0)
  {
    $Dig_Data_ActiveDrills=0;
  }

  if ( !isobject(%client) )
  {
    return;
  }

  %client.activeDrill--;
  if ( %client.ActiveDrill < 1)
  {
    if ( %client.MineDrillDisc > 0 )
    {
      //echo("using disc drill data " @ %client.MineDrillDisc);
      %client.mineDrill=1;
      schedule($Dig_Data_DrillSchedule, 0, "Dig_StartDrill", %client.Discbrick, %client.discPos, %client, %client.discNormal);
    }
    else
    {
      messageClient(%client, '', "<color:FFFF00>your drill is done running");
      %client.sendMiningStatus();
    }
  }
}

// reset the drills
function resetDrills()
{
  $Dig_Data_DrillLimit=100;
  $Dig_Data_ActiveDrills=0;
}

// highlight %brick prior to starting a drill
function Dig_HighlightDrill(%brick)
{
  if ( %brick.isMineable)
  {
    %brick.setColorFx(3);
    %brick.setColor(2);
  }
}
// highlight %brick prior to starting a drill
function Dig_HighlightOreDrill(%brick)
{
  if ( %brick.isMineable && isObject(%brick.dirtType) )
  {
    %brick.setColorFx(3);
    %brick.setColor(2);
  }
}

function serverCmdTurbodrillOn(%client)
{
  if ( %client.isAdmin)
  {
    $Dig_Data_DrillSchedule=100;
    messageClient(%client, '', "<color:FFFF00>Turbo drill ON");
  }
}

function serverCmdTurbodrillOff(%client)
{
  if ( %client.isAdmin)
  {
    $Dig_Data_DrillSchedule=300;
    messageClient(%client, '', "<color:FFFF00>Turbo drill OFF");
  }
}

// Purchase a turbo drill for 10 pick levels
function serverCMdBuyTurboDrill(%client)
{
  if ( %client.getPick() < 11)
  {
    Dig_DisplayError(%client, "You need at least 11 pick levels to purchase a turbo drill");
    return;
  }

  if ( %client.TurboDrill == 150  || %client.TurboDrill == 1)
  {
    Dig_DisplayError(%client, "You already have a turbo drill or better");
    return;
  }

  MessageClient(%client, '', "<color:FFFF00>Turbo drill purchased for 10 pick levels");
  MessageClient(%client, '', "<color:FFFF00>Your drills will run 3x as fast");
  MessageClient(%client, '', "<color:FFFF00>This will last until you log out");
  %client.TurboDrill=150;
  %client.addPick(-10);
  AddLog(%client, "TurboDrill");
}

// Purchase a super turbo drill for 200 pick levels
function serverCMdBuySuperTurboDrill(%client)
{
  if ( %client.getPick() < 201)
  {
    Dig_DisplayError(%client, "You need at least 201 pick levels to purchase an instant drill");
    return;
  }

  if (%client.TurboDrill == 1)
  {
    Dig_DisplayError(%client, "You already have a super turbo drill");
    return;
  }
  MessageClient(%client, '', "<color:FFFF00>Instant Drill purchased for 200 pick levels");
  MessageClient(%client, '', "<color:FFFF00>Your drills will finish almost instantly (depending on server load)");
  MessageClient(%client, '', "<color:FFFF00>This will last until you log out");
  %client.TurboDrill=1;
  %client.addPick(-200);
  AddLog(%client, "TurboDrill | super");
}
