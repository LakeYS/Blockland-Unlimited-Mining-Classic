if (ForceRequiredAddOn("Tool_RPG") == $Error::AddOn_NotFound)
  {
   error("ERROR: GameMode_UnlimitedMining - required add-on Tool_RPGfound");
   error("ERROR: GameMode_UnlimitedMining - Mod will not work right");
   return;
  }

// pickaxe to mine stuff with -- projectile
datablock projectileData(DiggingPickaxeProjectile : hammerProjectile)
  {
   directDamage = 5;
   lifeTime = 100;
   explodeOnDeath = false;
  };

// pickaxe to mine stuff with -- item for the tools inventory
datablock itemData(DiggingPickaxeItem : rpgPickaxeItem)
  {
    uiName = "Mining Pick";
    image = DiggingPickaxeImage;
    colorShiftColor = $Dig_PickaxeColor;
  };

// pickaxe to mine stuff with -- Image for player to hold in their hand
datablock shapeBaseImageData(DiggingPickaxeImage : rpgPickaxeImage)
  {
   item = DiggingPickaxeItem;
   projectile = DiggingPickaxeProjectile;
   colorShiftColor = $Dig_PickaxeColor;
   stateSound[0] = ""; // disable cheesy sword draw sound
  };

// Cement placer - looks & works just like the print gun, except its grey
// Cement placer -- item for the tools inventory
datablock itemData(DiggingCementItem : printGun)
  {
   uiName = "Cement Placer";
   image = DiggingCementImage;
   colorShiftColor = $Dig_CementAxeColor;
  };

// Cement placer -- image for player to hold in their hand
datablock ShapeBaseImageData(DiggingCementImage : printGunImage)
  {
   item = DiggingCementItem;
   colorShiftColor = $Dig_CementAxeColor;
  };

// Blockade placer - looks & works just like the print gun, except its Red
// Blockade placer -- item for the tools inventory
datablock itemData(BlockadeItem : printGun)
  {
   uiName = "Blockade Placer";
   image = BlockadeImage;
   colorShiftColor = "0.900000 0.100000 0.100000 1.000000";
  };

// Inv Cement placer -- image for player to hold in their hand
datablock ShapeBaseImageData(BlockadeImage : printGunImage)
  {
   item = BlockadeItem;
   colorShiftColor = "0.900000 0.100000 0.100000 1.000000";
  };

datablock TriggerData(NoFireZoneTrigger)
  {
   tickPeriodMS = 500;
  };
