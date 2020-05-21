// lava.cs
//
// Lava functions and processing for unlimited mining
//
$Dig_BlobSize=600;
$Dig_BlobSchedule=2000;

if (isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
  {
   if (!$RTB::RTBR_ServerControl_Hook)
     exec("Add-Ons/System_ReturnToBlockland/RTBR_ServerControl_Hook.cs");

   RTB_registerPref("Lava Method","Unlimited Mining","$Dig_LavaMethod","list Blobs Blocks ","GameMode_Unlimited_Mining","Blobs",0,0);
  }
else
  {
   $Dig_LavaMethod="Blobs";
  }

$dig_lavamethod="blobs";
$Dig_lavaDiv = mabs($Dig_Data_MaxDepth / 50);

// Return true if a lava should be placed at %pos
function MineLava::CheckPlace(%this, %pos, %random)
   {
    %low = %this.minPercent;

    if ( $Dig_LavaMethod $= "Blocks")
      {
       %z = getWord(%pos, 2) - $Brick_Z;
       %low -= mfloor(mabs(%z / $Dig_LavaDiv));
      }

    if ( (%random <= %this.maxPercent) && (%random >= %low ) )
      {
       if ( %this.depth == 0)
         {
          return true;
         }
       %z = getWord(%pos, 2) - $Brick_Z;
       //echo("Check Place lava " @ %pos SPC %random SPC %low SPC %this.maxPercent @ "z: " @ %z @ " pos: " @ %pos);
       if ( %this.depth < 0)
         {
          if ( %z <= %this.depth)
            {
             return true;
            }
         }
       else
         if ( %z >= %this.depth)
           {
            return true;
           }
      }
    return false;
   }


// place a lava brick at %pos, and start a lava blob
function MineLava::placeAt(%this, %pos, %random)
   {
    %health=%this.value * 4;
    %lava = PlaceBrick(%pos, %health, %this.color, %this.colorFx, %this.ShapeFx, %this);

    CheckRadioActive(%lava);

    if ( $Dig_LavaMethod $= "Blobs" )
      {
       %this.MakeBlob(%lava);
      }
    return %lava;
   }

function MineLava::PlaceContinue(%this, %pos, %rad, %radMult)
   {
    %health=%this.value * 4;
    %lava = PlaceBrick(%pos, %health, %this.color, %this.colorFx, %this.ShapeFx, %this);
    %lava.radioactive = %rad;
    %lava.multiplier = %radMult;
    return %lava;
   }

function MineLava::MakeBlob(%this, %lava)
   {
    // place a new lava globe
    %rad = 2;
    %x = getWord(%lava.position, 0);
    %y = getWord(%lava.position, 1);
    %z = getWord(%lava.position, 2);
    %rad+= mFloor(mabs(%z - $Brick_Z) / $Dig_LavaDiv);
    if ( %rad > 46)
      {
      %rad = 46;
      }
    if ( (%rad/2) != mFloor(%rad/2))
      {
       %rad++;
      }
    %rand = getRandom(1,6);
    switch ( %rand )
      {
       case 1 :
         %center= %x+%rad SPC %y SPC %z;
       case 2 :
         %center= %x-%rad SPC %y SPC %z;
       case 3 :
         %center= %x SPC %y+%rad SPC %z;
       case 4 :
         %center= %x SPC %y-%rad SPC %z;
       case 5 :
         %center= %x SPC %y SPC %z+%rad;
       case 6 :
         %center= %x SPC %y SPC %z-%rad;
      }

    //if ( %lava.radioactive)
    //  echo("start radioactive lava blob center: " @ %Center @ " rad: " @ %rad );
    //else
    //  echo("start lava blob center: " @ %Center @ " rad: " @ %rad );

    %z = getWord(%center, 2);
    %blob = new ScriptObject(LavaBlob) {
        center = %center;
        rad = %rad;
        type = "MineLava" TAB $Dig_Lava SPC %lava.radioactive SPC %lava.multiplier;
        dirtStart=1;
       };
    $JobQueue.AddJob(%blob);
   }

function MineLava::getMineName(%this, %brick)
   {
    return MineOre::getMineName(%this, %brick);
   }

// mine a lava block
function MineLava::mined(%this, %client, %brick, %mode)
   {
    if ( $modeLava[%mode] ==1) // only if we are taking lava damage
      {
       %hs = %client.getHS();
       if ( %hs.greaterThan(0) )
         {
          if ( %brick.radioactive)
            {
             %hs = %client.addHS(-%brick.multiplier);
            }
          else
            %hs = %client.AddHS(-1);

          if ( %hs.lessThan(1))
            {
             messageClient(%client, '', "<color:88FF88>The Lava pocket burned off your heat suit<color:FF0000> -- you need to buy a new one!!");
             %client.stats.heatsuit.zero();
            }
          else
            {
             if ( %hs.LessThan(20) )
               messageClient(%client, '', "<color:5555FF>The Lava pocket burned a layer off your heat suit -- <color:FF0000>you have " @ %hs.display() @ " left");
            }
         }
       else
         {
          %client.player.kill();
          messageAll('', "<color:FFFFFF>"@ %client.getPlayerName() @" <color:00FF00>mined a lava pocket!");
          %client.stats.heatsuit.zero();
         }
      }
    %brick.health=0;
   }

// for some reason the $Dig_Lava global keeps getting deleted
function CheckDirt()
   {
    if ( isObject($Dig_Lava) )
      {
       return;
      }
    $Dig_Lava = new ScriptObject(MineLava)
        {
         name="Lava";
         value=1;
         color=0;
         minPercent=89.5;
         maxPercent=90;
         depth=-50;
         colorfx=3;
         blockdepth=10;
         blockSize=50;
        };
   }

// add a new lava blob to the processing queue
function LavaProcessor::AddJob(%this, %job)
   {
    %this.totalWork+=%job.rad;
    %this.entries.add(%job);
   }

// startup the job processor
function LavaProcessor::Start(%this)
   {

    echo("Making new Job Queue");
    if ( isObject($JobQueue) )
      {
       echo("deleting old job queue");
       $JobQueue.delete();
      }
    if ( isObject($CurrentJob) )
      {
       $CurrentJob.delete();
       echo("deleting leftover job");
      }
    $JobQueue = new ScriptObject(LavaProcessor);
    $JobQueue.entries = new SimSet();
    $JobWarning[5] = 5;
    $JobWarning[4] = 5;
    $JobWarning[3] = 5;
    $JobWarning[2] = 5;
    $JobWarning[1] = 5;
    $JobWarningActive=false;
    $Dig_Data_DrillEnabled=true;
    $Dig_Data_BombSizeMax=35;
    $Dig_Data_BombSizeBuyLimit=20;

    PreCalculateLava($JobQueue, 2);
    //$JobQueue.processNextJob();

    schedule(2500, 0, "processNextJob", $JobQueue);
    //$JobQueue.schedule(25000, ProcessNextJob );
   }

// job processor.  Grab next job from the queue
function ProcessNextJob(%this)
   {
    if ( !isObject(%this) )
      {
       error( %this @ " not an object");
       return;
      }
    if ( $Dig_on != 1)
      {
       echo("Stopping job processor");
       return;
      }
    if ( $Dig_LavaMethod $= "Blocks")
      {
       return;
      }
    if ( %this.TotalWork < 1000)
      {
       // lava queue empty, reset warnings and undo all limits
       if ( $JobWarningActive)
         {
          $JobWarning[4] = 5;
          $JobWarning[3] = 5;
          $JobWarning[2] = 5;
          $JobWarning[1] = 5;
          $Dig_Data_DrillEnabled=true;
          $Dig_Data_BombSizeMax=35;
          $Dig_Data_BombSizeBuyLimit=20;
          messageAll('', "Server is back to normal, bombs & drills re-enabled");
          echo("bomb & drill restored");
          $JobWarningActive=false;
         }
      }

    if ( !isObject($CurrentJob) )
      {
       if ( %this.entries.getCount() > 0)
         {
          $CurrentJob = %this.entries.getObject(0);
          %this.entries.remove($CurrentJob);
          if ( %this.entries.getCount() > 3)
            {
             echo("processing job " @ %this.entries.getcount() SPC $CurrentJob.getName() SPC $CurrentJob.rad SPC %this.totalWork);
            }
         }
      }

    // job returns true when its done, false if it needs to be re-scheduled
    if ( isObject($CurrentJob) )
      {
      if ($CurrentJob.runJob() )
        {
         %this.TotalWork-=$CurrentJob.rad;
         $CurrentJob.delete();
        }
      }

    if ( %this.TotalWork > 11000)
      {
       if ( $JobWarning[5] > 0)
         {
          messageAll('', "<color:00FF00>Dig mode is off");
          $Dig_on=0;

          messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is fubared -- GIVING UP!");
          messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is fubared -- GIVING UP!");
          messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is fubared -- GIVING UP!");
          messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is fubared -- GIVING UP!");
          messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is fubared -- GIVING UP!");
          AddLog(0, "overload WarnFail", "Queue size " @ %this.entries.getCount() );


          messageAll('', "<color:FFFFFF>reset");
          messageAll('', "<color:FFFFFF>reset");
          messageAll('', "<color:FFFFFF>reset");
          messageAll('', "<color:FFFFFF>reset");

          $Dig_Stats_SavedAutoexec=true;
          export("$Dig_stats_Saved*", "config/server/miningStats.cs");
          OreData.save("config/server/miningData.cs");

          quit();
          return; // this should never be run, but you never know....
         }
      }
    else
    if ( %this.TotalWork > 8000)
      {
       if ( $JobWarning[4] > 0)
         {
          messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is majorly overloaded -- DISABLING bombs/drills");
          cancelAllDrills();
          cancelAllBombs();
          $Dig_Data_BombSizeMax=0;
          $Dig_Data_BombSizeBuyLimit=0;
          $Dig_Data_DrillEnabled=false;
          $JobWarning[4]--;
          $JobWarning[3]=0;
          $JobWarning[2]=0;
          $JobWarning[1]=0;
          $JobWarningActive=true;
          AddLog(0, "overload Warn4", "Queue size " @ %this.entries.getCount() );
         }
      }
    else
      if ( %this.TotalWork > 6000)
        {
         if ( $JobWarning[3] > 0)
           {
            messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is seriously overloaded -- CANCELLING all bombs/drills");
            cancelAllDrills();
            cancelAllBombs();
            $JobWarning[3]--;
            $JobWarning[2]=0;
            $JobWarning[1]=0;
            $JobWarningActive=true;
            AddLog(0, "overload Warn3", "Queue size " @ %this.entries.getCount() );
           }
        }
      else
        if ( %this.TotalWork > 4000)
          {
           if ( $JobWarning[2] > 0)
             {
              messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is overloaded - bombs are limited to size 10");
              $Dig_Data_BombSizeMax=10;
              $Dig_Data_BombSizeBuyLimit=10;
              $JobWarning[2]--;
              $JobWarning[1]=0;
              $JobWarningActive=true;
              AddLog(0, "overload Warn2", "Queue size " @ %this.entries.getCount() );
             }
          }
        else
          if ( %this.TotalWork > 2000)
            {
             if ( $JobWarning[1] > 0)
               {
                messageAll('', "<color:FF0000>WARNING:<color:FFFFFF>The server is getting overloaded. Stop bombing/drilling so much");
                $JobWarning[1]--;
                $JobWarningActive=true;
                AddLog(0, "overload Warn1", "Queue size " @ %this.entries.getCount() );
               }
            }

    schedule($Dig_BlobSchedule, 0, "processNextJob", $JobQueue);
   }

// Job to replicate a lava blob
function LavaBlob::runJob(%this)
   {
    if ( $Dig_on != 1)
      {
       return true;
      }

    if ( !isObject($LavaBlobPoints[%this.rad] ))
      {
       error("points for size: " @ %this.rad @ " not found");
       return true;
      }

    %size = $LavaBlobPoints[%this.rad].getCount();
    %points = $LavaBlobPoints[%this.rad];
    //echo("Replicate blob: " @ %this.center @ " size: "@ %this.rad @ " from: " @ %this.dirtStart @ " to " @ %size);
    for ( %a=1; %a < $Dig_BlobSize*2; %a++)
      {
       %pos = %points.getObject(%this.dirtStart);
       %newPos = vectorAdd(%pos.pos, %this.center);
       %spot = $Dig_PlacedDirt[%newPos];
       if ( getSubstr(%spot, 0, 8) $= "MineLava")
         {
          //echo("Lava Replicate Stopped " @ %this.center @ " size: " @ %this.rad);
          $LavaBlobPoints[%this.rad].uses++;
          return true;
         }
       if ( %spot $= "")
         {
          $Dig_placedDirt[%newPos] = %this.type;
         }
       else
         {
          if ( isObject(%spot.type) )
            {
             if ( %spot.type.getName() $= "DirtLayer" )
               {
                %data = getField(%this.type, 1);
                %spot.type = $Dig_Lava;
                %spot.setColor($Dig_Lava.color);
                %spot.setColorFx($Dig_Lava.colorFx);

                %spot.radioactive=getword(%data, 1);
                %spot.multiplier=getWord(%data,2);
               }
            }
         }
       %this.dirtStart++;
       if ( %this.dirtStart >= %size)
         {
          //echo("Lava Replicate Done " @ %this.center @ " size: " @ %this.rad);
          $LavaBlobPoints[%this.rad].uses++;
          return true;
         }
      }

    // job is not done yet
    return false;
   }

//Job to Create lavapoints
function LavaPoints::runJob(%this)
   {
    if ( $Dig_on != 1)
      {
       return true;
      }

    //echo("Generate Points for size: " @ %this.size @ " cur: " @ %this.currX SPC %this.CurrY SPC %this.currZ);
    if ( !isObject(%this.points) )
      {
       %this.points = new SimSet() {};
      }

    for ( %a=0; %a < $Dig_BlobSize*3; %a++)
      {
       %pos = %this.CurrX SPC %this.CurrY SPC %this.CurrZ;

       if ( VectorLen( %pos) <= %this.size)
         {
          %obj = new ScriptObject() {
            pos = %pos;
          };
          %this.points.add(%obj);
         }

       %this.CurrX+=2;
       if ( %this.CurrX > %this.EndX)
         {
          %this.CurrX = - %this.size;
          %this.CurrY+=2;
          if ( %this.CurrY > %this.EndY)
            {
             %this.CurrY = - %this.size;
             %this.currZ+=2;
            }
         }

       if ( %this.CurrZ > %this.EndZ)
         {
          break;
         }
      }

    if ( %this.CurrZ > %this.EndZ)
      {
       echo("Created LavaBlobPoints for size "@  %this.size);
       $LavaBlobPoints[%this.size] = %this.points;
       $LavaBlobPoints[%this.size].uses=0;
       $Dig_TotalBlobs++;
       if ( $Dig_TotalBlobs > 22)
         {
          MessageAll('', "<color:00FF00>/buyTeleport and /upgradejob is now available");
         }
       %blob.points = 0;
       return true;
      }

    // job is not done yet
    return false;
   }

// upgradeall job for players
// allows unlimited upgrade amounts w/o crashing the server
function UpgradeAllJob::runJob(%this)
   {
    if ( !isObject(%this.client) )
      {
       return true;
      }
    if ( %this.client.cancelUpgrade)
      {
       %this.client.upgradejob=false;
       %this.client.cancelUpgrade=false;
       echo("upgrade job cancelled");
       return true;
      }

    %this.count = 0;
    %start = %this.client.getPick();
    %startMoney = %this.client.getMoney().display();

    //deduct upgrade job fee
    if ( strlen(%startMoney) < 6)
      {
       %fee = mFloor(%startMoney * 0.01);
      }
    else
      %fee = getSubstr(%startMoney, 0, strlen(%startMoney) - 3);

    //echo("Subtract fee " @ %fee);
    %this.client.getMoney().subtract(%fee);
    %cost = Dig_PickCost(%this.client);
    while ( %this.client.getMoney().greaterThan(%cost) )
      {
       //echo("upgrade job " @ %this.client.getPick() @ " cost: " @ %cost @ " $" @ %this.client.getMoney().display() );

       %this.client.addpick(1);
       %this.client.getMoney().subtract(%cost);
       if ( %this.count > $Dig_BlobSize * 2)
         {
          // job has run long enough... stop for a while
          messageClient(%this.client, '', "Upgrade Job progress: " @ %start @ " to "@ %this.client.getPick() @ "  " @ $JobQueue.entries.getCount() @ " jobs waiting in line");
          echo("UpgradeAll "@ %this.client.GetPlayerName() @ " progress: " @ %start @ " to "@ %this.client.getPick() @ " $" @ %this.client.getMoney().display() );
          %this.client.sendMiningStatus();

          // make a new job and add it to the end of the queue
          %newJob = new ScriptObject(UpgradeAllJob)
             {
              client = %this.client;
              start = %this.start;
             };
          $JobQueue.addJob(%newJob);
          return true;
         }

       %this.count++;
       %cost = expandNumber(Dig_PickCost(%this.client));
      }
    AddLog(%this.client, "upgradeallJob", "from " @ %start @ " to " @ %this.client.getPick() );
    messageClient(%this.client, '', "Pick upgraded from " @ %this.start @ " to "@ %this.client.getPick() );
    return true;
   }

// Pre-calculate the lava blob points
function PrecalculateLava(%this, %size)
   {
    if ( $Dig_on != 1)
      {
       return;
      }
    if ( $Dig_LavaMethod $= "Blocks")
      {
       return;
      }

    if (%this.entries.getCount() < 10)
      {
       for ( %a=0; %a < 15; %a+=2)
         {
          %rad = %a+%size;

          if ( %rad > 47)
            {
             // all blob points submitted
             return;
            }

          %job = new ScriptObject(LavaPoints)
            {
             size = %rad;
             currX = -%rad;
             endX  =  %rad;
             currY = -%rad;
             endY  =  %rad;
             currZ = -%rad;
             endZ  =  %rad;
            };
          if ( !isObject($LavaBlobPoints[%rad]) )
            {
             echo("Add job for size: " @ %job.size);
             %this.AddJob(%job);
            }
          else
            {
             echo( "$LavaBlobPoints[" @ %rad @ "] already exists");

             // count the actual number of blobs
             $Dig_TotalBlobs=0;
             for ( %a=0; %a < 50; %a++)
               {
                if ( isObject($LavaBlobPoints[%a]))
                  {
                   $Dig_TotalBlobs++;
                  }
               }
             echo("Dig Total blobs set to " @ $Dig_TotalBlobs);
             if ( $Dig_TotalBlobs > 22)
               {
                MessageAll('', "<color:00FF00>/buyTeleport and /upgradejob is now available");
                return;
               }

            }
         }
       %rad = %a+%size;
      }
    else
      echo("Job queue size: " @ %this.entries.getCount() @ " try again in 1min");

    // check again in 1min
    schedule(1000*60, 0, "PrecalculateLava", %this, %rad);
   }

function BlobUseReport()
   {
    AddLog(0, "Blob use", "start of report", 1);
    for ( %a=0; %a < 50; %a++)
      {
       %obj = $LavaBlobPoints[%a];
       if ( isObject(%obj ) )
         {
          if (%obj.uses > 0)
            {
             AddLog(0, "blob use", "Size: " @ %a @ " used: " @ %obj.uses , 1);
            }
         }
      }
   }
