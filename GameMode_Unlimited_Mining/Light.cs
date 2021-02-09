//deactivatePackage(LightEvents);

registerOutputEvent("Player", "LightOn", "", 1);
registerOutputEvent("Player", "LightOff", "", 1);
registerOutputEvent("Player", "LockLight", "", 1);
registerOutputEvent("Player", "UnLockLight", "", 1);
registerOutputEvent("Player", "SetLight", "bool", 1);
registerOutputEvent("Player", "SetLightEnabled", "bool", 1);

// turn light on if its currently off
function Player::LightOn(%this, %client)
{
  if ( !isObject(%this.light))
  {
    serverCmdLight(%this.client,1);
  }
}

// turn light off if its currently on
function Player::LightOff(%this, %client)
{
  if ( isObject(%this.light))
  {
    serverCmdLight(%this.client, 1);
  }
}

// set the light on or off
// %state == 1. turn light on
// %state == 0. turn light off
function Player::SetLight(%this, %state, %client)
{
  if ( %state == 1)
  {
    %this.LightOn(%state, %client);
  }
  if ( %state == 0)
  {
    %this.lightOff(%state, %client);
  }
}

// lock the light so players cannot change it
function Player::LockLight(%this, %client)
{
  echo("Light locked");
  %this.LockLight=1;
}

// unlock the light so players turn on/off like normal
function Player::UnLockLight(%this, %client)
{
  echo("Light unlocked");
  %this.LockLight=0;
}

function Player::SetLightEnabled(%this, %lock, %client)
{
  if ( %state == 1)
  {
    %this.LockLight(%client);
  }
  if ( %state == 0)
  {
    %this.UnLockLight(%client);
  }

}

package LightEvents
{
  function serverCmdLight(%client, %override)
  {
    //echo("serverCmdLight " @ %override);
    if ( %override == 1)
    {
      parent::serverCmdLight(%client);
      return;
    }

    if ( %client.player.lockLight == 1)
    {
      return;
    }
    parent::serverCmdLight(%client);
  }

};

activatePackage(LightEvents);
