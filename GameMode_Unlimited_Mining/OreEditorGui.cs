function OreEditorGui::OnWake(%this)
   {
    // load the listbox with existing ore names
    Ore_LoadOres();

    // fix gui loading glitch
    OreColorButton.setText("");
    OreEditorGui.currentOre=0;
   }

function Ore_LoadOres()
   {
    OreNamesList.clear();
    new SimSet(OreDataClient);
    $Dig_OreCount=0;
    commandToServer('GetMiningOresList');
  }

function clientCmdReceiveOreData(%OreRecord)
   {
    %ore = ClientOre::fromRecord(%OreRecord);
    OreNamesList.addRow($Dig_OreCount, %ore.name);
    OreDataClient.add(%ore);
    $Dig_OreCount++;
   }

// return a ClientOre instance from %rec
function ClientOre::fromRecord(%rec)
   {
    %ore = new ScriptObject(ClientOre);
    %ore.ServerID  = getField(%Rec, 0);
    %ore.name =      getField(%Rec, 1);
    %ore.value =     getField(%Rec, 2);
    %ore.color =     getField(%Rec, 3);
    %ore.minPercent= getField(%Rec, 4);
    %ore.maxPercent= getField(%Rec, 5);
    %ore.depth     = getField(%Rec, 6);
    %ore.colorfx   = getField(%Rec, 7);
    %ore.veinLength= getField(%Rec, 8);
    %ore.minPick   = getField(%Rec, 9);
    %ore.bl_id     = getField(%Rec, 10);
    %ore.timeout   = getField(%Rec, 11);
    %ore.disabled  = getField(%Rec, 12);
    return %ore;
   }

// Update the selected ore
function OreNamesList::onSelect(%this, %id, %text)
   {
    OreEditorGui.currentOre=OreDataClient.getObject(%id);
    OreEditorGui.OreID = %id;
    OreEditorGui.OreSelected();
   }

// An ore has been selected from the listbox -- update everything accordingly
function OreEditorGui::OreSelected()
   {
    %ore = OreEditorGui.CurrentOre;
    OreName.setText( %ore.name);

    MinPercent.setText( %ore.minPercent);
    MaxPercent.setText( %ore.maxPercent);
    MinDepth.setText( %ore.depth);

    OreValue.setText( %ore.value);
    OreEditorGui::UpdateOrehealth();

    OreColor.setColor( getColorIDTable( %ore.color ) );
    OreColorID.setText( %ore.color );
    OreFx.setText(%ore.ColorFX);
    VeinLength.setText(%ore.VeinLength);
    MinPick.setText(%ore.minPick);
    OreBL_ID.setText(%ore.bl_id);
    OreTimeout.setText(%ore.timeout);
    OreDisabled.setText(%ore.disabled);

    if ( OreEditorGui.CurrentOre > 1 )
      {
       DeleteOreButton.setVisible(true);
      }
    else
      {
       DeleteOreButton.setVisible(false);
      }
   }

// UPdate ore health numbers
function OreEditorGui::UpdateOreHealth()
   {
    %ore=OreEditorGui.currentOre;
    %ore.value = OreValue.getValue();
    OreHealth.setText("Health: " @ %ore.value * 4);
   }

// something changed in the GUI - update the currently selected ore
function OreEditorGui::UpdateOreData()
   {
   %ore= OreEditorGui.currentOre;
   %ore.minPercent = MinPercent.getValue();
   %ore.maxPercent = MaxPercent.getValue();
   %ore.depth = MinDepth.getValue();
   %ore.colorFx = OreFx.getValue();
   %ore.VeinLength = VeinLength.getValue();
   %ore.MinPick = MinPick.getValue();
   %ore.bl_id = OreBL_ID.getValue();
   %ore.timeout = OreTimeout.getValue();
   %ore.Disabled = OreDisabled.getValue();
   }

// ore name changed.  update the gui accordingly
function OreEditorGui::UpdateOreName()
	{
	 %ore = OreEditorGui.currentOre;
   %ore.name = Orename.getValue();
   OreNamesList.setRowByID(OreNamesList.getSelectedID(), %ore.name);
  }

function OreEditorGui::createColorMenu(%this,%obj) //I wouldn't have to do this if we had access to the client source
   {
    if (isObject(OreEditorGui_ColorMenu))
      {
       OreEditorGui_ColorMenu.delete();
       return;
      }

    if (isObject(Avatar_ColorMenu))
      Avatar_ColorMenu.delete();

    WrenchEventsDlg::createColorMenu(%this.getObject(0),%obj);

    Avatar_ColorMenu.setName("OreEditorGui_ColorMenu");

    OreEditorGui.add(OreEditorGui_ColorMenu);

    %ext = OreEditorGui_ColorMenu.getExtent();
    %w = getWord(%ext,0);
    %h = getWord(%ext,1);

    %objPos = OreColor.getPosition();
    %objX = getWord(%objPos,0);
    %objY = getWord(%objPos,1);

    %objExt = OreColor.getExtent();
    %objW = getWord(%objExt,0);
    %objH = getWord(%objExt,1);

    OreEditorGui_ColorMenu.resize(%objX+%objW,%objY,%w,%h);

    for (%i=0;%i<64;%i++)
      {
       %num = (%i*2)+1;

       if (%num >= OreEditorGui_ColorMenu.getObject(0).getCount())
         break;

       %o = OreEditorGui_ColorMenu.getObject(0).getObject(%num);
       %o.command = "OreEditorGui.pickColor(" @ %i @ ");";
      }
   }

function OreEditorGui::pickColor(%this, %c)
   {
    if (isObject(OreEditorGui_ColorMenu))
      OreEditorGui_ColorMenu.delete();

    OreColor.value = %c;
    OreEditorGui.currentOre.color = %c;

    %this.OreSelected();
   }

// add a blank ore at the end of the list
// Send a request to the server to make a copy of the selected ore
function OreEditorGui::AddNewOre()
   {
    %ore= OreEditorGui.currentOre;
    if ( !isObject(%ore) )
      {
       clientCmdMessageBoxOK("Ore Editor", "Select an ore to copy first");
       return;
      }
    commandToServer('NewOre', %ore.getServerRecord() );

    // server will send back the new ore - nothing else to do here.
   }

// Send the updated ore to the server
function OreEditorGui::SaveOre()
   {
    %ore = OreEditorGui.currentOre;
    commandToServer('UpdateOre', %ore.getServerRecord() );
   }

// return a TDL from this ore to be send to the server
function ClientOre::getServerRecord(%this)
   {
    return %this.ServerID TAB %this.name TAB %this.value TAB %this.color TAB %this.minPercent TAB %this.maxPercent TAB %this.depth TAB %this.colorfx TAB %this.veinLength TAB %this.minPick TAB %this.bl_id TAB %this.timeout TAB %this.disabled;
   }

// delete the selected ore
function OreEditorGui::DeleteOre()
   {
    %ore = OreEditorGui.currentOre;
    commandToServer('DeleteOre', %ore.ServerID );

    OreNamesList.RemoveRowById(OreEditorGui.OreID);
    OreDataClient.remove(%ore);
    $Dig_OreCount--;
   }

// save all the ores to config/server/MiningData.cs
function OreEditorGui::SaveAllOre()
   {
    commandToServer('SaveOres');
   }

// host only
function clientCmdeditOre()
   {
    Canvas.PushDialog(OreEditorGui);
   }
