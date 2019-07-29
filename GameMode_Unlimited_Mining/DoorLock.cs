registerOutputEvent("fxDTSBrick", "keyPressed", "String 20 20", 1);
registerOutputEvent("fxDTSBrick", "setSecurityCode", "String 200 72", 1);
registerOutputEvent("fxDTSBrick", "checkSecurityCode", "", 1);

// key pressed event -- normally sent from a print brick
function fxDTSBrick::keyPressed(%this, %arga, %argb, %argc, %argd, %arge)
  {
   echo("Key pressed event: " @ %this @ " arga: " @ %arga @" argb: " @ %argb @" argc: " @ %argc @" argd: " @ %argd );
   echo("this class: " @ %this.getClassName() );
   echo("this db name: " @ %this.getDataBlock().getName() );

   %this.securityCode = %this.securityCode @ %arga;
   echo("sec code: " @ %this.securityCode);

   if ( %this.clearSchedule > 0)
     {
      cancel(%this.clearSchedule);
     }
   schedule(3000, %this, %this, "clearSecurityCode");
  }

// Check to see if the entered code matches, and send a true/false event accordingly
function fxDTSBrick::checkSecurityCode(%this, %arga, %client, %argc, %argd, %arge)
  {
   echo("check security code event: " @ %this @ " arga: " @ %arga @" argb: " @ %argb @" argc: " @ %argc @" argd: " @ %argd );
   echo("this class: " @ %this.getClassName() );
   echo("this db name: " @ %this.getDataBlock().getName() );

   if ( %this.clearSchedule > 0)
     {
      cancel(%this.clearSchedule);
     }

   if ( %this.securityCode $= %arga)
     {
      %this.processInputEvent("onVariableTrue", %client);
     }
   else
     {
      %this.processInvputEvent("onVariableFalse", %client);
     }
   %this.clearSecurityCode();
  }

// clear whatever code has been entered for this brick
function fxDTSBrick::clearSecurityCode(%this)
  {
   echo("Clear security code for " @ %this );
   %this.securityCode = "";
  }
