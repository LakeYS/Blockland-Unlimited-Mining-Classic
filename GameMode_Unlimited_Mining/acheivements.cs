// Acheivements.cs
//
// Achivements for unlimited mining
//
// minebrick @ depth: 1000,2000, etc
// collect dirt: 10k, 20k, 30k, etc
//
function GameConnection::SetupAcheivements(%this)
{
  %this.AcheivementDepth  = -1000;
  %this.AcheivementHeight = 1000;
  %this.AcheivementDirt   = 10000;
  %this.Acheivements=0;
}

function GameConnection::CheckDepth(%this)
{
  if ( !isObject(%this.player) )
  return;

  %depth = getWord(%this.player.getposition(), 2) - $Brick_Z;
  if ( %depth < %this.AcheivementDepth )
  {
    %this.AcheivementMsg("Has reached a depth of " @ %this.AcheivementDepth);
    %this.Acheivements++;
    %this.AcheivementDepth*=2;
  }
  if ( %depth > %this.AcheivementHeight)
  {
    %this.AcheivementMsg("Has reached a height of " @ %this.AcheivementHeight);
    %this.Acheivements++;
    %this.AcheivementHeight*=2;
  }
}

function GameConnection::CheckDirt(%this)
{
  if ( %this.dirt.greaterThan(%this.AcheivementDirt) )
  {
    %this.AcheivementMsg("Has Acquired " @ %this.AcheivementDirt@ " dirt");
    %this.Acheivements++;
    %this.AcheivementDirt*=2;
  }
}

function GameConnection::AcheivementMsg(%this, %msg)
{
  messageAll('', "<color:0000FF>Achievement: <color:FFFF00>"@%this.getPlayername() @ "<color:FFFFFF> " @ %msg);
}

function GameConnection::AcheivementsString(%this)
{
  return "Depth: " @ %this.AcheivementDepth/1000 @ "k Dirt: " @ %this.AcheivementDirt/1000 @ "k";
}

// display info about everyone's acheivements
function ServerCmdAcheivements(%client)
{
  for ( %a=0; %a < ClientGroup.getCount(); %a++)
  {
    %cl = ClientGroup.getObject(%a);
    if ( %cl.Acheivements > 0)
    %pts = %cl.AcheivementsString();
    else
    {
      if ( %client== %cl)
        %pts = "Nothing yet";
      else
        %pts = "None";
    }

    if ( %pts !$= "none")
    {
      messageClient(%client, '', "<color:FFFFFF>" @ %cl.getPlayerName() @ " <color:FFFF00><lmargin:100> " @ %pts@"<lmargin:0>");
    }
  }
}
