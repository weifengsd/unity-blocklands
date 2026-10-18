using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands
{
    public partial class Sandbox
    {
        public static Rect CraftingCellRect(float w,float h,int i)=>new Rect((w-944)/2+28+(i%3)*86,(h-610)/2+144+(i/3)*86,80,80);
        // Shared by the real IMGUI MouseDown path and the isolated pointer checks.
        public bool HandleCraftingPointer(Vector2 point,int button,float w,float h)
        {
            if(!CraftingOpen||!Paused||CompanionMenuOpen||LifeWorkshopOpen||PartyBagOpen||Health<=0||VictoryOpen||button<0||button>1)return false;
            for(int i=0;i<9;i++)if(CraftingCellRect(w,h,i).Contains(point)){craftingCursor=i;if(button==1)Crafting.SetCell(i,Block.Air,Inventory);else PlaceIngredient(i);return true;}return false;
        }
    }
    public partial class SmokeTest
    {
        IEnumerator CraftingUiChecks(Sandbox g,string folder)
        {
            var rows=new List<string>();void Check(string key,bool ok){rows.Add(key+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"crafting-ui-checks.txt"),rows);}
            g.Respawn();g.Inventory[7]=8;g.SelectItem(7);
            Check("original_first_and_fourth_cells_overlap_old_sidebar",Sandbox.CraftingCellRect(1000,650,0).Overlaps(new Rect(22,160,176,50))&&Sandbox.CraftingCellRect(1000,650,3).Overlaps(new Rect(22,223,176,62)));
            g.OpenCrafting();Check("E_page_is_only_crafting",g.CraftingOpen&&g.Paused&&!g.LifeWorkshopOpen&&!g.CompanionMenuOpen);
            Check("first_cell_pointer_places_only_ingredient",g.HandleCraftingPointer(Sandbox.CraftingCellRect(1000,650,0).center,0,1000,650)&&g.Crafting.Grid[0]==Block.Planks&&g.CraftingOpen&&!g.CompanionMenuOpen&&!g.LifeWorkshopOpen);
            Check("fourth_cell_pointer_does_not_open_invitation",g.HandleCraftingPointer(Sandbox.CraftingCellRect(1000,650,3).center,0,1000,650)&&g.Crafting.Grid[3]==Block.Planks&&g.CraftingOpen&&!g.CompanionMenuOpen&&!g.LifeWorkshopOpen);
            Check("one_and_four_form_correct_stick_recipe",g.Crafting.Match()!=null&&g.Crafting.Match().Output==Block.Stick);
            int sticks=g.Inventory[13];Check("collect_consumes_only_reserved_two_planks",g.CollectCraft()&&g.Inventory[7]==6&&g.Inventory[13]==sticks+4);
            g.HandleCraftingPointer(Sandbox.CraftingCellRect(1440,900,0).center,0,1440,900);g.HandleCraftingPointer(Sandbox.CraftingCellRect(1440,900,0).center,1,1440,900);Check("right_click_removes_without_spending_material",g.Crafting.Grid[0]==Block.Air&&g.Inventory[7]==6);
            Check("outside_grid_is_not_handled_as_ingredient",!g.HandleCraftingPointer(new Vector2(980,5),0,1000,650));
            g.ToggleCrafting();Check("second_E_closes_and_resumes",!g.CraftingOpen&&!g.Paused);
            Check("companion_page_still_opens_explicitly",g.OpenCompanionMenu()&&g.CompanionMenuOpen&&!g.CraftingOpen);Check("companion_page_blocks_hidden_crafting_pointer",!g.HandleCraftingPointer(Sandbox.CraftingCellRect(1000,650,0).center,0,1000,650));g.ToggleCrafting();Check("E_switches_companion_page_to_crafting",g.CraftingOpen&&!g.CompanionMenuOpen&&!g.LifeWorkshopOpen&&g.Paused);
            g.OpenLifeWorkshop();Check("workshop_explicit_open_closes_crafting",g.LifeWorkshopOpen&&!g.CraftingOpen);g.ToggleCrafting();Check("E_switches_workshop_to_crafting",g.CraftingOpen&&!g.LifeWorkshopOpen&&!g.CompanionMenuOpen);
            g.ShowRecipe(0);Check("recipe_selection_does_not_open_other_pages",g.CraftingOpen&&!g.LifeWorkshopOpen&&!g.CompanionMenuOpen);
            g.Crafting.SetCell(0,Block.Planks,g.Inventory);g.Save();g.Load();g.SetPaused(false);Check("reserved_material_survives_save_and_load",g.Inventory[7]==6);
            Check("party_supplies_submenu_remains_separate",g.OpenPartyBag()&&g.PartyBagOpen&&!g.CraftingOpen);g.ClosePartyBag();Check("party_submenu_close_resumes",!g.Paused&&!g.PartyBagOpen);
            yield return null;g.SetPaused(true);
        }
    }
}
