// Loging.cs
// Logging functions for unlimited mining
//

// add an ore log entry
function GameConnection::AddOreLog(%this, %ore, %money, %radFlag)
{
  if ( %money == 0)
    return;

  %this.stats.mineMoney.add(%money );
  if ( %radFlag)
    %key = "radioactive " @ %ore.name @ " | " @ %money;
  else
    %key = %ore.name @ " | " @ %money;

  if (%this.Ores[%key] > 0)
    %this.Ores[%key]++;
  else
  {
    %this.Ores[%key] = 1;
    %this.OreRecord = SetField( %this.OreRecord, getFieldCount(%this.OreRecord), %key);
  }
}

// write the accumulated ore logs out as log entries
function GameConnection::WriteOreLogs(%this)
{
  if ( getFieldCount(%this.OreRecord) == 0)
    return;

  for ( %a=1; %a < GetFieldCount(%this.OreRecord); %a++)
  {
    %log = getField(%this.OreRecord, %a);
    AddLog(%this, "mined", %log @ " | " @ %this.Ores[%log], 0);
    %this.Ores[%log]=0;
  }
  %this.OreRecord=0;
}


// Add a log message
function addLog(%client, %operation, %data, %echo)
{
  if ( isObject(%client) )
  {
    %string = getDateTime() @" | " @ %client.getPlayerName() @ " | " @ %operation @ " | " @ %data;
  }
  else
  {
    %string = getDateTime() @ " | None | " @ %operation @ " | " @ %data;
  }
  %entry = new ScriptObject()
  {
    line = %string;
  };
  if ( !isObject(GameLog) )
  {
    new SimSet(GameLog) {};
    }
    GameLog.add( %entry);

    if ( GameLog.getCount() > 100)
    {
      FlushLog();
    }
    if ( %echo==1)
    {
      echo(%string);
    }
  }

  // write the contents of GameLog to a file
  function FlushLog()
  {
    if ( isObject(GameLog))
    {
      echo("flush GameLog " @ GameLog.getCount());
      %file=new FileObject();
      %file.openForAppend("config/server/mining/MiningLog" @ stripChars(getWord(getDateTime(), 0),"/") @ ".txt");

      for ( %x=0; %x < GameLog.getCount(); %x++)
      {
        %file.writeLine( GameLog.getObject(%x).line );
      }
      %file.close();
      %file.delete();
      GameLog.clear();
    }
    else
      new SimSet(GameLog) {};
    }
