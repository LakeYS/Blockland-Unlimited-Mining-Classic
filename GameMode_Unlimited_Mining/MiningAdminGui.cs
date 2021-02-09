function MiningAdminGui::onWake()
{
  Dig_PlayerList.clear();
  commandToServer('GetMiningPlayerList');
  $Dig_PlayerCount=0;
}

function clientCmdReceiveMiningPlayer(%playerData)
{
  Dig_PlayerList.addRow( $Dig_PlayerCount, %playerData, $Dig_PlayerCount);
  $Dig_PlayerCount++;
}

function Dig_PlayerList::onSelect(%this, %id, %text)
{
  // populate the editor fields
  PlayerName.setText( getField(%text, 0) );
  PlayerPick.setText( getField(%text, 1) );
  PlayerMoney.setText( getField(%text, 2) );
  PlayerHeat.setText( getField(%text, 3) );
  PlayerRad.setText( getField(%text, 4) );
  PlayerRank.setText( getField(%text, 5) );
}

// send updates for all the selected stats
function UpdateAllStats()
{
  %text = PlayerName.getValue();
  %player = getField(%text, 0);
  commandToServer('SetDig_Value', %player, "pick", PlayerPick.getValue() );
  commandToServer('SetDig_Value', %player, "money", PlayerLevel.getValue() );
  commandToServer('SetDig_Value', %player, "heatsuit", PlayerHeat.getValue() );
  commandToServer('SetDig_Value', %player, "radsuit", PlayerRad.getValue() );
  commandToServer('SetDig_Value', %player, "Rank", PlayerRank.getValue() );
  commandToServer('SetDig_Value', %player, "Bomb", PlayerBomb.getValue() );
}

package Dig_MiningAdminButton
{
  function adminGui::onWake(%this)
  {
    Parent::onWake(%this);

    if (isObject(Dig_MiningAdminGuiButton))
    {
      return;
    }

    %btn = new GuiBitmapButtonCtrl(Dig_MiningAdminGuiButton)
    {
      profile = BlockButtonProfile;
      horizSizing = "left";
      vertSizing = "bottom";
      position = "214 214";
      extent = "98 38";
      command = "canvas.pushDialog(MiningAdminGui);";
      text = "Mining Admin";
      bitmap = "base/client/ui/button1";
      mcolor = "100 100 255 255";
    };
    adminGui.getObject(0).add(%btn);
  }
};
