//owners.cs
// Events to change brick ownership

// set the owner to a given bl_id
registerOutputEvent("fxDTSBrick", "SetOwnerID", "String 7 100", 1);
registerOutputEvent("fxDTSBrick", "SetOwnerIDChain", "String 7 100", 1);

// change the owner to whoever sent the event
registerOutputEvent("fxDTSBrick", "ChangeOwner", "", 1);
registerOutputEvent("fxDTSBrick", "ChangeOwnerChain", "", 1);

function fxDTSBrick::setOwnerID(%this, %bl_id, %obj)
  {
   echo("set owner ID");
   echo("%this:  " @ %this);
   echo("%bl_id: " @ %bl_id);
   echo("%obj  : " @ %obj);

   %brickgroup = "BrickGroup_" @ %bl_id;
   if ( !isObject(%brickgroup) )
     {
      %brickgroup=new SimSet("BrickGroup_" @ %bl_id)
        {
         client=0;
         bl_id=%bl_id;
         name="\c1BL_ID: " @ %bl_id @ "\c1\c0";
        };
      echo("made new brickgroup: " @ %brickgroup.getID() );
     }
   echo("brickgroup: " @ %brickgroup.getID() );
   %obj.brickgroup.remove(%this);
   %brickgroup.add(%this);
   %this.stackBL_ID = -1;
   %this.client=%brickgroup.client;
  }

function fxDTSBrick::ChangeOwner(%this, %obj, %arga)
  {
   //%brickGroup = %this.getGroup();
   //%brickgroup.remove(%this);
   //%obj.brickgroup.add(%this);
   //%this.stackBL_ID = -1;
   //%this.client=%obj;
   %this.setColor(4);
  }

// set the owner for this brick and all others connected to it
function fxDTSBrick::ChangeOwnerChain(%this, %obj, %arga)
   {
    %this.Changeowner(%obj, %arga);

    %brick = %this;
    %startPos = %brick.getPosition();
    %stackBrick[%brick] = 1;
    %stackList[0] = %brick;
    %stackHead = 1;
    for  (%stackTail = 0; %stackTail != %stackHead; %stackTail++)
      {
       %currBrick = %stackList[%stackTail];

       if (%stackTail)
         {
          for (%i = 0; %i < %currBrick.getNumDownBricks(); %i++)
           {
            %newBrick = %currBrick.getDownBrick(%i);
            if (!%stackBrick[%newBrick])
              {
               %stackBrick[%newBrick] = 1;
               %stackList[%stackHead] = %newBrick;
               %stackHead++;
               %newBrick.ChangeOwner(%obj, %arga);
              }
           }
         }

       for (%i = 0; %i < %currBrick.getNumUpBricks(); %i++)
        {
         %newBrick = %currBrick.getUpBrick(%i);
         if (!%stackBrick[%newBrick])
           {
            %stackBrick[%newBrick] = 1;
            %stackList[%stackHead] = %newBrick;
            %stackHead++;
            %newBrick.ChangeOwner(%obj, %arga);
           }
        }
      }
   }
