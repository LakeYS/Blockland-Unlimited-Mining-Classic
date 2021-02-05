// Ores.cs
// starting Mining Ores
$Dig_Radioactivedepth=8000;
exec("./lava.cs");

if ( !isObject(OreData) )
  {
   new SimSet(OreData)
      {
       new ScriptObject(MineDirt) { name="Dirt";            value=0;     color=54; minPercent=0;    MaxPercent=100;  depth=0;     };
       new ScriptObject(MineOre) { name="Einsteinium";      value=5;     color=4;  minPercent=87;   maxpercent=89;   depth=-15;   veinLength=8; };
       new ScriptObject(MineOre) { name="Black Metal";      value=50;    color=52; minPercent=89;   maxPercent=90;   depth=-200;  veinLength=8; };
       new ScriptObject(MineOre) { name="Altiar 4";         value=120;   color=2;  minPercent=90;   maxPercent=91;   depth=-400;  veinLength=8; };
       new ScriptObject(MineOre) { name="Graphite";         value=225;   color=50; minPercent=91;   maxPercent=92;   depth=-600;  veinLength=8; };
       new ScriptObject(MineOre) { name="Titanium Bonds";   value=300;   color=29; minPercent=92;   maxPercent=92.7; depth=-800;  veinLength=7; };
       new ScriptObject(MineOre) { name="Molecular Bonds";  value=500;   color=31; minPercent=93;   maxPercent=93.6; depth=-1000;  veinLength=7; };
       new ScriptObject(MineOre) { name="Tacheyon Weavings";value=700;   color=40; minPercent=94;   maxPercent=94.5; depth=-1200;  veinLength=7; };
       new ScriptObject(MineOre) { name="Niobium Alloy";    value=1000;  color=6;  minPercent=95;   maxPercent=95.4; depth=-1400;  veinLength=6; };
       new ScriptObject(MineOre) { name="Mercury Nitox";    value=3000;  color=27; minPercent=96;   maxPercent=96.3; depth=-1700;  veinLength=6; minPick=25;};
       new ScriptObject(MineOre) { name="Tacheyon Nitox";   value=5000;  color=6;  minPercent=97;   maxPercent=97.2; depth=-2100;  veinLength=5; minPick=50;};
       new ScriptObject(MineOre) { name="Diamond Lattice";  value=7000;  color=17; minPercent=98;   maxPercent=98.1; depth=-3000; veinLength=5; minPick=75;};
       new ScriptObject(MineOre) { name="Klaatu";           value=10000; color=53; minPercent=99.9; maxPercent=100;  depth=-4800; colorfx=1; veinLength=5;minPick=100; };
       new ScriptObject(MineOre) { name="Starillite";       value=31415; color=45; minPercent=99.5; maxPercent=100;  depth=-25000; colorfx=1; veinLength=18;minPick=25000; };
       new ScriptObject(MineOre) { name="Gold";             value=30;    color=3;  minPercent=83;   maxPercent=84;   depth=0;   veinLength=10; };
       new ScriptObject(MineOre) { name="Silver";           value=15;    color=30; minPercent=84;   maxPercent=85;   depth=0;   veinLength=11; };
       new ScriptObject(MineOre) { name="Diamond";          value=100;   color=17; minPercent=85;   maxPercent=86;   depth=0;   veinLength=5;  };
       new ScriptObject(MineOre) { name="Platinum";         value=50;    color=41; minPercent=86;   maxPercent=87;   depth=0;   veinLength=10; };
       new ScriptObject(MineOre) { name="ATM Machine";        value=1000000; color=6; minPercent=0.1;  maxPercent=0.2;  depth=12000;veinLength=2; NoBomb=1;minPick=1000; };
       new ScriptObject(MineOre) { name="Epicness";        value=1000000; color=7; minPercent=0.1;  maxPercent=0.2;  depth=-12000;veinLength=2; NoBomb=1;minPick=1000; };
       new ScriptObject(MineOre) { name="Starillite";       value=31415; color=45; minPercent=99.5; maxPercent=100;  depth=-25000; colorfx=1; veinLength=18;minPick=25000; };
      };
  }

function LayerInfo(%layer)
   {
    echo("Layer: " @ %layer.name @" sDepth: " @ %layer.sDepth @ " eDepth: " @ %layer.eDepth @ " color: " @ %layer.colorStart @ " hp: " @ %layer.health @ " R: " @ %layer.minRank);
   }

// add sub-layers to %layer, working downwards
function MakeLayers(%layer, %layerColor, %offset, %dir, %color)
   {
    %layer.layers = new SimSet();
    if ( %dir==1)
      {
       %top = %layer.sDepth;
      }
    if ( %dir==-1)
      {
       %top = %layer.eDepth;
      }
    %health=%layer.health;
    for ( %b=0; %b < getFieldCount(DirtLayers.layertypes); %b++)
      {
       %layerType = getField(DirtLayers.layerTypes, %b);
       %layerName = %layerType SPC %layer.name;

       %dirt = new ScriptObject(DirtLayer)
         {
          name = %layerName;
          sDepth=%top;
          eDepth = %top - (%offset * %dir);
          health=%health;
          colorStart=%layerColor;
          colorSize=2;
          minRank = %layer.minRank;
         };
       %top = %top - (%offset * %dir);

       if ( %dir== -1)
         {
          // swap start/end depth so layer picking will work right
          %tmp = %dirt.sDepth;
          %dirt.sDepth=%dirt.eDepth;
          %dirt.eDepth = %tmp;
         }

       %layer.layers.add(%dirt);
       LayerInfo(%dirt);

       if ( %color == 1)
          %layerColor = getNextLayerColor(%layerColor);
       else
         {
          %layerColor+=2;
          if ( %layerColor > 62)
            {
             %layerColor = 0;
            }
         }

       %health *= 2;
      }
    %layer.health=%health;
    if ( %dir==1)
      {
       %layer.eDepth = %top;
      }
    if ( %dir==-1)
      {
       %layer.sDepth = %top;
      }

    LayerInfo(%layer);
    DirtData.add(%layer);
    return %layerColor;
   }

// setup the depths and layer sizes for the dirt layers
// start with a dirt layer at elevation 0
// go "up" and create each upper layer in increments of 1500
// then go down and create each lower layer in increments of 1500
function DirtLayers::Setup(%this)
   {
    if ( isObject(DirtData) )
      DirtData.delete();
    new SimSet(DirtData) {};

    %middleLayer = new ScriptObject(DirtLayer)
      {
       name = %this.MiddleLayer;
       sDepth = 750;
       health=5;
      };

    // Middle Layer
    %layerColor = MakeLayers(%middleLayer, 54, 1500, 1);

    // Upper Layers
    %depth = %middleLayer.sDepth;
    %health = 20;
    %layerColor = 27;
    for ( %a=0; %a < getFieldCount(%this.ToplayerNames); %a++)
      {
       %layer = getField(%this.TopLayerNames, %a);
       %dirtLayer = new ScriptObject(DirtLayer)
         {
          name = %layer;
          eDepth = %depth;
          health = %health;
         };
       %layerColor = MakeLayers(%dirtLayer, %layerColor, 2000, -1, 1);
       %depth = %dirtLayer.sDepth;
       %health = %dirtLayer.health;
      }
    $Dig_Data_MaxHeight = %depth;


    // Lower Layers
    %layerColor = 45;
    %depth = %middleLayer.eDepth;
    %health = %middleLayer.health*2;
    for ( %a=0; %a < getFieldCount(%this.BottomLayerNames); %a++)
      {
       %layer = getField(%this.BottomLayerNames, %a);
       %dirtLayer = new ScriptObject(DirtLayer)
         {
          name = %layer;
          sDepth = %depth;
          health = %health;
         };
       %layerColor = MakeLayers(%dirtLayer, %layerColor, 2000, 1);
       %depth = %dirtLayer.eDepth;
       %health = %dirtLayer.health;

       if ( %a == 2)
         {
          $Dig_Radioactivedepth = %dirtLayer.eDepth;
         }
      }

    // extra layers
    %layerColor = 0;
    %lastDepth = %dirtLayer.eDepth;
    %a = 0;
    while ( %lastDepth + $Brick_Z > 0)
      {
       echo("lastDepth: " @ %lastDepth @ " L + Z: " @ %lastDepth + $Brick_Z);
       if ( %a > 0)
         %layer = "Mantle " @ %a;
       else
         %layer = "Mantle";

       %dirtLayer = new ScriptObject(DirtLayer)
         {
          name = %layer;
          sDepth = %depth;
          health = %health;
          minRank = %a*3;
         };
       %layerColor = MakeLayers(%dirtLayer, %layerColor, 2000, 1, 1);
       %depth = %dirtLayer.eDepth;
       %health = %dirtLayer.health;
       %lastDepth = %dirtLayer.eDepth;

       %a++;
      }

    $Dig_Data_MaxDepth = %depth;
    %this.LastLayer = %a;
    %this.lastDepth = %depth;
    %this.lastHealth = %health;
    %this.lastColor = %layerColor;

    // Shift everything according to $Brick_Z
    for ( %a=0; %a < DirtData.getCount(); %a++)
      {
       %layer = DirtData.getObject(%a);
       %layer.eDepth += $Brick_Z;
       %layer.sDepth += $Brick_Z;
       for ( %b=0; %b < %layer.layers.getCount(); %b++)
         {
          %dLayer = %layer.layers.getObject(%b);
          %dlayer.eDepth += $Brick_Z;
          %dlayer.sDepth += $Brick_Z;
         }
      }
    $Dig_Data_MaxHeight += $Brick_Z;
    $Dig_lavaDiv = mabs($Dig_Data_MaxDepth / 50);
   }

// Add Another layer to the bottom
function DirtLayers::addDirtLayer(%this, %client)
   {

    %layer = "Mantle " @ %this.lastLayer;
    %depth = %this.lastDepth;
    %health = %this.lastHealth;
    %layerColor = %this.lastColor;

    %dirtLayer = new ScriptObject(DirtLayer)
      {
       name = %layer;
       sDepth = %depth;
       health = %health;
       minRank = %layerColor;
      };
    %layerColor = MakeLayers(%dirtLayer, %layerColor, 2000, 1, 1);

    $Dig_Data_MaxDepth = %depth;
    %this.LastLayer++;
    %this.lastDepth = %dirtLayer.eDepth;
    %this.lastHealth = %dirtLayer.health;
    %this.lastColor = %layerColor;
    AddLog(%client, "Add dirt layer " @ %a+1);
   }

// return the next (non-transparent) layer color starting at %colorID
function getNextLayerColor(%colorID)
   {
    %id = %colorID + 1;
    if ( %colorID > 44)
       return 0;
    else
      %id = %colorID + 1;

    %data = getColorIDTable(%id);
    if ( getWord(%data, 3) $= "1.000000")
      {
       return %id;
      }
    return getNextLayerColor(%id + 1);
   }

// return true if this ore should be placed
function MineOre::CheckPlace(%this, %pos, %random)
   {
    if ( %this.disabled==1)
      return;

    if ( (%random <= %this.maxPercent) && (%random >= %this.minPercent) )
      {
       if ( %this.depth == 0)
         {
          return true;
         }

       %z = getWord(%pos, 2) - $Brick_Z;
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


// place ore at %position
function MineOre::placeAt(%this, %pos, %random)
   {
    %health=%this.value * 4;
    %brick = PlaceBrick(%pos, %health, %this.color, %this.colorFx, %this.ShapeFx, %this);
    CheckRadioActive(%brick);

    // create an ore vein
    %rnd = getRandom(1,6);
    switch ( %rnd)
		  {
       case 1 :
         %dir = "1 0 0";
       case 2 :
         %dir = "-1 0 0";
       case 3 :
         %dir = "0 1 0";
       case 4 :
         %dir = "0 -1 0";
       case 5 :
         %dir = "0 0 1";
       case 6 :
         %dir = "0 0 -1";
		  }
    %length = getRandom(1, %this.veinLength);
    %currPos = %pos;
    for (%a=0; %a<= %length; %a++)
      {
       %currPos = VectorAdd(%currPos, %dir);
       if ( $Dig_PlacedDirt[%currPos] $="" )
         {
          $Dig_placedDirt[%currPos] = %this.getName() TAB %this SPC %brick.radioactive SPC %brick.multiplier;
         }
      }
    return %brick;
   }

// check to see if %brick can be made radioactive
function CheckRadioActive(%brick)
   {
    if ( getrandom(1,2) == 2)
      {
       %depth = %brick.type.depth;
       // depth of 0 means the brick type can appear anywhere

       if ( %depth == 0)
         {
          %depth = $Dig_Radioactivedepth;
         }
       else
         %depth = %brick.type.depth + $Dig_Radioactivedepth;

       if ( getWord(%brick.position, 2) < %depth )
         {
          %brick.multiplier=getRandom(1,20);
          %brick.health*=%brick.multiplier*2;
          %brick.radioactive=true;
         }
      }
   }
function GenerateOre(%dirtRecord, %pos)
   {
    %data   = getField(%dirtRecord, 1);
    %this   = getWord(%data, 0);

    %radioactive=getword(%data, 1);
    %multiplier=getWord(%data,2);
    %brick = %this.PlaceContinue(%pos, %radioactive, %multiplier);

    return %brick;
   }

// based on whats in %dirtRecord - make a new ore block
function MineOre::placeContinue(%this, %pos, %rad, %radMult)
   {
    %brick  = PlaceBrick(%pos, %this.value * 4, %this.color, %this.colorFx, %this.ShapeFx, %this);

    %brick.radioactive= %rad;
    %brick.multiplier = %radmult;

    if ( %brick.radioactive)
      {
       %brick.health=%this.value * 4 * %brick.multiplier;
      }

    return %brick;
   }

// place a dirt block at %pos
function MineDirt::PlaceAt(%this, %pos, %random)
   {
    %z= getword(%pos, 2);
		for ( %a=0; %a < DirtData.getCount(); %a++)
		  {
			 %dirt = DirtData.getObject(%a);

       if ( %dirt.canPlace(%z) )
         {
          return %dirt.placeAt(%pos, %random, %z);
         }
      }
    %layer = DirtData.getObject(DirtData.getCount() -1);
    %dirt = %layer.layers.getObject( %layer.layers.getCount()-1);
    return %dirt.placeAt(%pos, %random, %z);
   }

// return true if %z fits in this dirt layer
function DirtLayer::canPlace(%this, %z)
   {
    //echo("sDepth: " @ %this.sDepth @ " z: " @ %z @ " eDepth: " @ %this.eDepth);
    return %z <= %this.sDepth && %z >= %this.eDepth;
   }

// place a dirt block in this layer
function DirtLayer::PlaceAt(%this, %pos, %random, %z)
   {
    if ( isObject(%this.layers) )
      {
       for ( %a=0; %a < %this.layers.getCount(); %a++)
         {
          %layer = %this.layers.getObject(%a);
          if ( %layer.canPlace(%z) )
            {
             return %layer.placeAt(%pos, %random, %z);
            }
         }
      }

    %color = %this.colorStart;
    if ( %this.colorSize > 1)
      {
       // adjust color depending on how far we are into the current layer of dirt
       %diff = %this.Sdepth - %this.Edepth;
       %offset = mAbs(%z - %this.sDepth);
       %change1 = ((%offset / %diff) * (%this.colorSize - 1) ) + 0.5;
       %change = mFloor(%change1);
       if ( %change > %this.colorSize)
         {
          %change=%this.colorSize;
         }

      }
    %color += %change;
    %brick = Placebrick(%pos, %this.health, %color, 0, 0, %this );
    %brick.DirtType=%this;
    CheckRadioActive(%brick);
    return %brick;
   }

// return ore name to display for the client
function DirtLayer::getMineName(%this, %brick)
   {
    if ( %brick.radioactive )
      {
       return "<color:FFFF00>Radioactive <color:FFFFFF>" @ %brick.dirtType.name;
      }
    else
      return "<color:FFFFFF>" @ %brick.dirtType.name;
   }

function MineOre::getMineName(%this, %brick)
   {
    if ( %brick.radioactive )
      {
       return "<color:FFFF00>Radioactive <color:11FFAA>" @ %this.name;
      }
    else
      return "<color:11FFAA>" @ %this.name;
   }

// Mine dirt block
function DirtLayer::mined(%this, %client, %brick, %mode)
   {
    if ( %this.minRank > 0)
      {
       if (%client.getRank() < %this.minRank)
         {
          MessageClient(%client, '', "<color:FF0000>you need rank " @ %this.minRank@ " to mine that");
          return;
         }
      }
    %health=$modeHealth[%mode];
    if ( %health > 0)
      {
       %brick.health -= 1* (%client.getPick() * (%client.stats.rank+1));
      }
    %brick.health*= %health;

    // if the brick is dead and we are checking for discoveries... do so
    if (%brick.health < 1)
      {
       if ( %mode $= "drill" || %mode $= "bomb" )
         return;

       if ( %brick.radioactive)
         %client.AddDirt( mFloor(%brick.multiplier * $modeMoney[%mode]) );
       else
         %client.AddDirt(1);

       if ( $ModeDiscover[%mode] ==1)
         {
          Dig_CheckDiscoverOre(%client);
         }
      }
   }

// Mine an ore - this one gives $$
function MineOre::mined(%this, %client, %brick, %mode)
   {
    %health=$modeHealth[%mode];
    %money =$modeMoney[%mode];
    if ( %health > 0)
      {
       if ( %brick.radioactive)
         {
          %minPick = %this.minPick * mFloor(%brick.multiplier / 2);
         }
       else
         %minPick = %this.minPick;

       if ( %minPick > %client.getPick() )
         {
          MessageClient(%client, '', "<color:FF0000>you need a pick of " @ %minPick @ " to mine that");
          return;
         }
       %brick.health -= 1*%client.getPick();
      }
    %brick.health= mFloor(%brick.health * %health);

    // if the brick is dead, update the client's money
    if (%brick.health < 1)
      {
       // some ores give no money when bombed
       if ( %this.NoBomb && %mode $= "bomb")
         {
          return;
         }
       if ( %brick.radioactive)
         {
          MineRadioactive(%this, %client, %brick, %mode);
         }
       else
         {
          if ( isObject(%client))
            {
             if ( %client.getClassName() $= "GameConnection")
               {
                %client.AddOreLog(%this, mFloor(%this.value * %money), false );
               }
             else
               {
                error("err in MineOre::mined(" @ %client.getClassName() );
               }

            }
         }
      }
   }

// client minded some radioactive ore
function MineRadioactive(%this, %client, %brick, %mode)
   {
    %health=$modeHealth[%mode];
    %money =$modeMoney[%mode];
    if ( isObject(%client))
      {
       %cash = %brick.multiplier * mfloor(%money * %this.value);
       %client.AddOreLog(%this, %cash, true);
      }

    %damage = 0;
    if ( %mode !$= "bomb")
       %damage = mFloor(mabs(%brick.multiplier * %money));

    if ( %health > 0)
      {
       %msg = "You have been irradiated";

       // burn rad suit as needed
       %rs = %client.getRS();
       if ( %rs.greaterThan(0) )
         {
          %rs = %client.AddRS(-%damage);

          if ( %rs.lessthan(0) )
            {
             %damage-=mabs(%client.getRS().display() );
             %client.stats.radsuit.zero();
             %msg = "<color:5555FF>Your radsuit was destroyed by the radioactive ore -- you need to buy a new one";
            }
          else
            {
            %msg = "<color:5555FF>Your radsuit absorbed <color:0000FF>" @ %damage @ " Rads.<color:5555FF>You have " @ %rs.display() @ " Left";
            %damage=0;
           }
         }
       %client.player.radDamage+=%damage;
       %hp = 100;
       %rPercent = %client.player.radDamage / %hp;
       if ( %client.player.radDamage >= %hp)
         {
          switch ( getRandom(1,3) )
            {
             case 1:
                messageAll('', "<color:0000FF>" @ %client.getplayerName() @ " <color:00FF00>Died of radiation poisoning");
                %client.player.kill();
                %client.player.radDamage=0;
                %client.getRS().zero();
                return;
             case 2 :
                %msg = "You Have been mutated";
                %x=getRandom() + 0.5;
                %y=getRandom() + 0.5;
                %z=getRandom() + 0.5;
                %client.player.setScale(%x SPC %y SPC %z);
                %client.player.radDamage=0;
                AddLog(%client, "mutate", "scale: " @ %x SPC %y SPC %z);
             case 3 :
                %msg = "You Escaped dieing from radiation poisoning            for now";
            }
         }
       else
         {
          if ( %rPercent > 0.25)
            {
            %msg = "You are getting radation poisoning";
            }
          if ( %rPercent > 0.4)
            {
             %msg = "You have Radation poisoning";
            }
          if ( %rPercent > 0.6)
            {
             %msg = "You have Radation Sickness";
            }
          if ( %rPercent > 0.8 )
            {
             %msg = "You have Severe radation sickness.<color:FFFFFF> Try /cureRad to get better";
            }
         }
       messageClient(%client, '', "<color:0000FF>" @ %msg);
      }
   }

// return a copy of this ore
function MineOre::copy(%this)
   {
    %new = new ScriptObject(MineOre)
       {
        name  = %this.name;
        value = %this.value;
        color = %this.color;
        minPercent=%this.minPercent;
        maxPercent=%this.maxPercent;
        depth = %this.depth;
       };
    return %new;
   }

// place a brick at the given spot and stats
function PlaceBrick(%pos, %health, %color, %colorFx, %shapeFX, %type)
   {
    %client = $Dig_host;
    //%pos1 = VectorAdd(%pos, $BrickOffset);
    %brick = new fxDTSBrick()
      {
       client = %client;
       datablock = "brick4xCubeData";
       position = %pos;
       rotation = "0 0 0 0";
       colorID = %color;
       scale = "1 1 1";
       angleID = "0";
       colorfxID = %colorfx;
       shapefxID = %shapeFx;
       isPlanted = 1;
       stackBL_ID = $Dig_Host.bl_id;
       type = %type;
       health = %health;
       isMineable = 1;
      };
    %r = %brick.plant();
    //echo("plant result: " @ %r @ " PlaceBrick(\"" @ %pos @ "\" , " @ %health @ " , " @ %color @ " , " @ %colorFx @ " , " @ %shapeFx @ " , " @ %type @ ");");
    %brick.setTrusted(1);

    $Dig_host.brickgroup.add(%brick);
    $Dig_placedDirt[%pos] = %brick;
    return %brick;
   }

// cure radation sickness
function serverCmdCureRad(%client)
   {
    if ( %client.player.radDamage < 1)
      {
       Dig_DisplayError(%client, "You dont have any radation damage");
       return;
      }
    messageClient(%client, '', "<color:FFFF00>You have " @ %client.player.RadDamage @ " Radiation Damage");
    messageClient(%client, '', "<color:FFFF00>It will cost " @ %client.player.RadDamage @ " pick levels -OR- $" @ %client.player.RadDamage*500 @ " To cure");
    messageClient(%client, '', "<color:FFFF00>say: /curepick or /curemoney to cure yourself");
   }

// cure radiation sickness by paying with pick levels
function serverCmdCurePick(%client)
   {
    if ( %client.player.radDamage < 1)
      {
       Dig_DisplayError(%client, "You dont have any radation damage");
       return;
      }
    if ( %client.getPick().lessThan(mFloor(%client.player.RadDamage / 10) ) )
      {
       Dig_Displayerror(%client, "You dont have enough pick levels to pay for treatment");
       return;
      }

    messageClient(%client, '', "<color:FFFF00>You spent " @ %client.player.RadDamage @ " Pick levels to cure yourself");
    %client.addPick( -mFloor(%client.player.RadDamage/10) );
    %client.player.RadDamage=0;
    %client.sendMiningStatus();
   }

// cure radation sickness by paying with money
function serverCmdCureMoney(%client)
   {
    if ( %client.player.radDamage < 1)
      {
       Dig_DisplayError(%client, "You dont have any radation damage");
       return;
      }
    messageClient(%client, '', "<color:FFFF00>You spent $" @ %client.player.RadDamage*500 @ " to cure yourself");
    %client.AddMoney( -(%client.player.RadDamage * 500) );
    %client.player.RadDamage=0;
    %client.sendMiningStatus();
   }

// create a randomly located 'blob' of each ore
function makeOreBlobs()
   {
    for ( %a=1; %a < OreData.getCount(); %a++)
      {
       %ore = OreData.getObject(%a);
       %x=getRandom(-400,400);
       %y=getRandom(-400,400);
       %z=getRandom($Dig_Data_MaxDepth/2, $Dig_Data_MaxHeight/2);
       //if ( %ore.depth > 0 && %z < %ore.depth )
       //  {
       //   %z=getRandom(%ore.depth, $Dig_Data_Maxheight);
       //  }
       //if ( %ore.depth < 0 && %z > %ore.depth)
       //  {
       //   %z=getRandom(%ore.depth, $Dig_Data_MaxDepth);
       //  }

       %x = mFloor(%x / 2) * 2; //ensure x is an even number
       %y = mFloor(%y / 2) * 2; //ensure y is an even number
       %z = mFloor(%z / 2) * 2; //ensure z is an even number

       if ( %z < $Dig_Data_MaxDepth)
         {
          %z = $Dig_Data_maxDepth + 20;
         }
       if ( %z > $Dig_Data_MaxHeight)
         {
          %z = $Dig_Data_maxHeight - 20;
         }
       %x++; %y++;
       schedule(%a*1000, MissionGroup, "MakeOreBlob", %ore, %x SPC %y SPC %z);
      }
   }

function MakeOreBlobSize(%ore, %pos, %size)
   {
    echo("make ore blob: " @ %ore.name @ " size: " @ %size @ " at " @ %pos);
    %health=%ore.value * 4;

    for ( %x= getWord(%pos, 0)-%size; %x < getWord(%pos, 0)+%size; %x+=2)
      {
       for ( %y=getWord(%pos, 1)-%size; %y < getWord(%pos, 1)+%size; %y+=2)
         {
          for ( %z=getWord(%pos,2)-%size; %z <getWord(%pos, 2)+%size; %z+=2)
            {
             %pos1 = %x SPC %y SPC %z;
             if ( VectorLen( VectorSub(%pos1, %pos)) <= %size)
               {
                $Dig_placedDirt[%pos1] = "MineOre" TAB %ore @ " 0 0";
               }
            }
         }
      }
   }

// make an ore blob using %ore, centered at %pos
function MakeOreBlob(%ore, %pos)
   {
    MakeOreBlobSize(%ore, %pos, 10);
   }

function MakeRadOreBlob(%ore, %pos, %level)
   {
    echo("make Rad ore blob: " @ %ore.name @ " at " @ %pos);
    %health=%ore.value * 4;

    %blobSize = 10;
    for ( %x= getWord(%pos, 0)-%blobSize; %x < getWord(%pos, 0)+%blobSize; %x+=2)
      {
       for ( %y=getWord(%pos, 1)-%blobSize; %y < getWord(%pos, 1)+%blobSize; %y+=2)
         {
          for ( %z=getWord(%pos,2)-%blobSize; %z <getWord(%pos, 2)+%blobSize; %z+=2)
            {
             %pos1 = %x SPC %y SPC %z;
             if ( VectorLen( VectorSub(%pos1, %pos)) <= %blobsize)
               {
                $Dig_placedDirt[%pos1] = "MineOre" TAB %ore @ " 1 " @ %level;
               }
            }
         }
      }
   }
function DirtReport()
   {
    for ( %a=0; %a < DirtData.getCount(); %a++)
      {
       %layer = DirtData.getObject(%a);
       LayerInfo(%layer);
       for ( %b=0; %b < %layer.layers.getCount(); %b++)
         {
          %subLayer=%layer.layers.getObject(%b);
          LayerInfo(%subLayer);
         }
      }
   }

if ( !isObject(DirtData) )
  {
   $Dirt = new ScriptObject(DirtLayers)
      {
       TopLayerNames = "Soil" TAB "Ground" TAB "Top";
       MiddleLayer = "Dirt";
       BottomLayerNames = "Rock" TAB "Bedrock" TAB "Metal Rock" TAB "Core";
       layerTypes = "" TAB "Dense" TAB "Hard" TAB "Seriously Hard";
      };
   $Dirt.setup();
  }
