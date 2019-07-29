echo("running client.cs");

// make a circle of bricks
function serverCmdMakeCircle(%client, %lim)
  {
   $Assist::count=2;
   $Assist::current=0;
   $Assist::commands[0] = "makeCircleBack";
   $Assist::commands[1] = "makeCircleRight";
   $Assist::commands[2] = "makeCircleFront";
   MakeLine(%client, %lim);
   //%client.player.tempBrick.dump();
   echo(%client.player.tempBrick.getWorldBox());

  }
function MakeCircleBack(%client, %lim)
  {
   echo("makecircleback");
   commandToServer('RotateBrick', 1);
   commandToServer('ShiftBrick',-1,-1);
   schedule(50, 0, "makeline1", %client, %lim);
  }

function MakeCircleRight(%client, %lim)
  {
   echo("makecircleRight");
   commandToServer('RotateBrick', 1);
   commandToServer('ShiftBrick', -1, 1);
   schedule(50,0,"MakeCircleright1", %client, %lim);
  }

function MakeCircleright1(%client, %lim)
  {
   %rot       = %client.player.tempBrick.rotation;
   %db        = %client.player.tempBrick.getDatablock();
   echo("brick sizeR: " @ %db.brickSizeX SPC %db.brickSizeY SPC %db.brickSizeZ);
   echo("brick rotR: " @ %rot);
   if ( %rot $= "0 0 1 180")
     {
      %shift = %db.brickSizeX;
     }
   if ( %rot $= "0 0 -1 89.9999")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "0 0 1 180")
     {
      %shift = %db.brickSizeX;
     }
   if ( %rot $= "0 0 -1 90.0002")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "0 0 1 90.0002")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "1 0 0 0")
     {
      %shift = %db.brickSizeY;
     }
   %shift = "-"@%shift;                 // move the brick backwards
   echo("shift = " @ %shift);
   scheduleplantbrick(%client, 1, %lim, %shift, 0);
  }

//right to left line making
function MakeCircleFront(%client, %lim)
    {
     CommandToServer('RotateBrick', 1);
     commandToServer('ShiftBrick', 1, 1);
     schedule(50, 0, "MakeCircleFront1", %client, %lim);
    }

function MakeCircleFront1(%client, %lim)
  {
   //%client.player.tempBrick.dump();
   %rot       = %client.player.tempBrick.rotation;
   %db        = %client.player.tempBrick.getDatablock();
   echo("brick sizeF: " @ %db.brickSizeX SPC %db.brickSizeY SPC %db.brickSizeZ);
   echo("brick rotF: " @ %rot);
   if ( %rot $= "0 0 1 180")
     {
      %shift =  %db.brickSizeY;
     }
   if ( %rot $= "1 0 0 0")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "0 0 -1 90.0002")
     {
      %shift = %db.brickSizeX;
     }
   if ( %rot $= "0 0 1 90.0002")
     {
      %shift = %db.brickSizeX;
     }
   if ( %rot $= "0 0 -1 89.9999")
     {
      %shift = %db.brickSizeX;
     }

   echo("shiftF = " @ %shift);
   schedulePlantBrick(%client, 1, %lim, 0, %shift);

    }

// left to right line making
function serverCmdMakeLine1(%client, %lim)
  {
   $Assist:count=0;
   MakeLine1(%client, %lim);
  }

function MakeLine1(%client, %lim)
  {
   echo("Makeline1");
   //%client.player.tempBrick.dump();
   %rot       = %client.player.tempBrick.rotation;
   %db        = %client.player.tempBrick.getDatablock();
   echo("brick size1: " @ %db.brickSizeX SPC %db.brickSizeY SPC %db.brickSizeZ);
   echo("brick rot1: " @ %rot);
   if ( %rot $= "0 0 1 180")
     {
      %shift = "-"@ %db.brickSizeY;
     }
   if ( %rot $= "1 0 0 0")
     {
      %shift = "-"@%db.brickSizeY;
     }
   if ( %rot $= "0 0 -1 90.0002")
     {
      %shift = "-"@%db.brickSizeX;
     }
   if ( %rot $= "0 0 1 90.0002")
     {
      %shift = "-"@%db.brickSizeY;
     }
   if ( %rot $= "0 0 -1 89.9999")
     {
      %shift = "-"@%db.brickSizeX;
     }

   echo("shift1 = " @ %shift);
   schedulePlantBrick(%client, 1, %lim, 0, %shift);
  }

// front to back line making
function serverCmdMakeLine(%client, %lim)
  {
   $Assist:count=0;
   makeLine(%client, %lim);
   echo("Client: " @ %client);
  }

function makeLine(%client, %lim)
  {
   echo("Makeline");
   //%client.player.tempBrick.dump();
   %rot       = %client.player.tempBrick.rotation;
   %db        = %client.player.tempBrick.getDatablock();

   echo("brick size: " @ %db.brickSizeX SPC %db.brickSizeY SPC %db.brickSizeZ);
   echo("brick rot: " @ %rot);
   if ( %rot $= "0 0 1 180")
     {
      %shift = %db.brickSizeX;
     }
   if ( %rot $= "0 0 -1 89.9999")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "0 0 1 180")
     {
      %shift = %db.brickSizeX;
     }
   if ( %rot $= "0 0 -1 90.0002")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "0 0 1 90.0002")
     {
      %shift = %db.brickSizeY;
     }
   if ( %rot $= "1 0 0 0")
     {
      %shift = %db.brickSizeY;
     }
   echo("shift = " @ %shift);
   schedulePlantBrick(%client, 1, %lim, %shift, 0);
}

function schedulePlantBrick(%client, %cur, %lim, %shift, %shift1)
  {
   //echo("Schedule plant brick cur: " @ %cur @ " lim: " @ %lim @ " shift: " @ %shift @ " shift1: " @ %shift1);
   CommandToServer('PlantBrick');
   CommandToServer('ShiftBrick', %shift, %shift1);

   if ( %cur >= %lim)
     {
      echo("$Assist::count  = "@ $Assist::count);
      echo("$Assist::current= "@ $Assist::current);
     if ( $Assist::current <= $Assist::count)
       {
        schedule(10,0,$Assist::commands[$Assist::current], %client, %lim);
        $Assist::current++;
       }
     }
   else
     {
      schedule(60, 0, "schedulePlantBrick", %client, %cur+1, %lim, %shift, %shift1);
     }

  }
