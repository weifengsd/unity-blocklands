using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands {public partial class SmokeTest
{
    IEnumerator CrystalMaterialChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string key,bool ok){rows.Add(key+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"crystal-material-checks.txt"),rows);}
        g.Respawn();g.TravelTo(2);yield return null;
        Check("material_and_ore_names_are_crystal",Blocks.Names[77]=="水晶矿石"&&Blocks.Names[78]=="水晶块"&&Blocks.Names[9]=="透明水晶块");
        Check("old_item_ids_and_inventory_slots_unchanged",(int)Blocks.CrystalOre==77&&(int)Blocks.CrystalGem==78&&g.Inventory.Length==95);
        var ore=new Vector3Int(43,18,41);Check("crystal_ore_is_mineable_in_crystal_valley",g.World.Get(ore)==Blocks.CrystalOre&&g.World.Get(ore+Vector3Int.right)==Block.Stone);
        int before=g.Inventory[77];Check("mining_adds_one_real_ore",g.MineBlock(ore)&&g.Inventory[77]==before+1&&g.World.Get(ore)==Block.Air);
        var c=g.Crafting;c.Clear();c.SetCell(4,Blocks.CrystalOre,g.Inventory);var recipe=c.Match();Check("recipe_explains_mining_and_two_crystal_blocks",recipe!=null&&recipe.Name=="水晶块"&&recipe.Output==Blocks.CrystalGem&&recipe.Count==2&&recipe.Hint.Contains("水晶矿谷"));
        before=g.Inventory[78];Check("refining_spends_one_ore_and_adds_two_blocks",c.Collect(g.Inventory,out _)&&g.Inventory[78]==before+2&&g.Inventory[77]==0);
        g.Player.Teleport(new Vector3(40,23,37));g.Player.SetFlying(true);g.SelectItem(78);g.Player.RefreshHeldItem();Check("crystal_block_is_visible_in_hand",g.Player.VisibleHeldItemId==78);
        var cell=new Vector3Int(43,22,41);before=g.Inventory[78];Check("crystal_block_can_really_be_placed",g.TryPlace(cell,Blocks.CrystalGem)&&g.World.Get(cell)==Blocks.CrystalGem&&g.Inventory[78]==before-1);Check("placed_block_can_be_mined_back",g.MineBlock(cell)&&g.Inventory[78]==before&&g.World.Get(cell)==Block.Air);
        bool armor=true;foreach(var r in Crafting.Recipes)if((int)r.Output>=81&&(int)r.Output<=85)foreach(var b in r.Pattern)if(b!=Block.Air&&b!=Block.Stick&&b!=Blocks.CrystalRod)armor&=b==Blocks.CrystalGem;Check("crystal_equipment_uses_crystal_blocks",armor);
        g.Inventory[78]=25;g.Inventory[74]=1;g.Inventory[13]=1;before=g.Inventory[81];Check("whole_set_consumes_twenty_five_crystal_blocks",g.CraftArmorSet(false)&&g.Inventory[78]==0&&g.Inventory[81]==before+1&&g.Inventory[85]>0);
        g.Inventory[78]=17;g.Inventory[9]=11;g.Save();g.Load();g.SetPaused(false);yield return null;Check("old_stock_and_transparent_blocks_survive_load",g.Inventory[78]==17&&g.Inventory[9]==11&&Blocks.Names[78]=="水晶块");
        g.SetPaused(true);
    }
}}
