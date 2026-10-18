using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator HomeChecks(Sandbox g,string folder)
        {
            var checks=new List<string>();void Check(string k,bool v){checks.Add(k+"="+v);adventurePass&=v;File.WriteAllLines(Path.Combine(folder,"home-checks.txt"),checks);}
            g.Respawn();g.TravelTo(1);yield return null;
            int[] bag=new int[Blocks.Count];for(int i=0;i<bag.Length;i++)bag[i]=100;var craft=new Crafting();
            foreach(int i in new[]{1,4,7})craft.SetCell(i,Block.Stone,bag);Check("door_exact_middle_column",craft.Match()?.Output==Block.IronDoor);craft.Collect(bag,out _);Check("door_cost_three_stones",bag[(int)Block.Stone]==97&&bag[(int)Block.IronDoor]==101);
            foreach(int i in new[]{0,3,6})craft.SetCell(i,Block.Stone,bag);Check("door_wrong_column_rejected",craft.Match()==null);craft.Clear();craft.SetCell(4,Block.Glass,bag);Check("window_exact_center",craft.Match()?.Output==Block.Window);craft.Collect(bag,out _);Check("window_cost_one_crystal",bag[(int)Block.Glass]==99);craft.SetCell(0,Block.Glass,bag);Check("window_corner_rejected",craft.Match()==null);craft.Clear();foreach(int i in new[]{0,1,2})craft.SetCell(i,Block.Gauze,bag);Check("curtain_recipe",craft.Match()?.Output==Block.Curtain);
            int doors=0;foreach(int x in new[]{9,42})foreach(int z in new[]{8,25,43})if(g.World.Get(new Vector3Int(x+4,16,z))==Block.IronDoor)doors++;Check("six_village_iron_doors",doors==6);
            var cost=g.HouseCost(28,50);Check("house_one_door_three_windows",cost[Block.IronDoor]==1&&cost[Block.Window]==3&&cost[Block.Curtain]==3&&cost[Block.Television]==1);
            int before=g.Inventory[(int)Block.Gauze];Check("gauze_harvest_site",g.MineBlock(new Vector3Int(35,15,6))&&g.Inventory[(int)Block.Gauze]==before+1&&g.Theft.Count==0);
            HomeFixture Find(Vector3Int p){foreach(var f in g.World.GetComponentsInChildren<HomeFixture>())if(f.Cell==p)return f;return null;}
            var door=Find(new Vector3Int(13,16,8));if(door.State.Open)door.Toggle();Physics.SyncTransforms();var origin=new Vector3(14,17.4f,6);Check("closed_door_has_collision",Physics.Raycast(origin,Vector3.forward,out var hit,3)&&hit.collider.GetComponentInParent<HomeFixture>()==door);door.Toggle();Physics.SyncTransforms();Check("open_door_clears_passage",!Physics.Raycast(origin,Vector3.forward,3));door.Toggle();
            var window=Find(new Vector3Int(11,17,17));window.Toggle();Check("window_opens",window.State.Open);window.Toggle();Check("window_closes",!window.State.Open);
            var curtain=Find(new Vector3Int(11,17,16));curtain.Toggle();Check("curtain_opens",curtain.State.Open);curtain.Toggle();Check("curtain_closes",!curtain.State.Open);
            g.Player.Teleport(new Vector3(12.5f,16.3f,11.5f));Check("tv_power_on",g.ToggleHomeTV());var tv=Find(new Vector3Int(16,17,10));Check("tv_on_state",tv.State.On);tv.Channel(1);Check("tv_animation_channel",tv.State.Channel==1);tv.Channel(-1);Check("tv_previous_channel",tv.State.Channel==0);var channels=new HashSet<string>();for(int i=0;i<tv.ChannelCount;i++){channels.Add(tv.ChannelTitle+tv.ChannelDialogue);tv.Channel(1);}Check("five_distinct_dialogue_channels",channels.Count==5&&tv.State.Channel==0);
            var stand=g.Player.transform.position;Check("bed_enter",g.UseSeat(new Vector3Int(12,16,13))&&g.HomeSeated);yield return new WaitForSecondsRealtime(.4f);ScenePicture(g,Path.Combine(folder,"32-bed-tv.png"));Check("bed_exit_same_action",g.UseSeat(default)&&!g.HomeSeated&&Vector3.Distance(g.Player.transform.position,stand)<.1f);
            Check("sofa_enter",g.UseSeat(new Vector3Int(13,16,15))&&g.HomeSeated);Check("sofa_exit",g.UseSeat(default)&&!g.HomeSeated);g.ToggleHomeTV();Check("tv_power_off",!tv.State.On);
            Check("book_take",g.ToggleBook(new Vector3Int(16,16,15))&&g.HoldingBook&&g.HeldBookPages>=1&&g.HeldBookPages<=8);string firstBook=g.HeldBookTitle;int firstPages=g.HeldBookPages;int firstPage=g.HeldBookPage;g.TurnBook(1);Check("book_pages_turn",g.HeldBookPages==1||g.HeldBookPage!=firstPage);Check("book_return_same_action",g.ToggleBook(default)&&!g.HoldingBook);Check("book_variety_no_immediate_repeat",g.ToggleBook(new Vector3Int(16,16,15))&&g.HeldBookTitle!=firstBook);Check("library_has_every_length_one_to_eight",Sandbox.LibraryUsesOneToEightPages()&&firstPages>=1&&firstPages<=8&&g.HeldBookPages>=1&&g.HeldBookPages<=8);g.ToggleBook(default);
            g.Player.Teleport(new Vector3(30,16,45));Check("cannot_demolish_village_house",!g.DemolishOwnHouse());foreach(var c in cost)g.Inventory[(int)c.Key]=c.Value+5;
            Check("build_with_new_materials",g.BuildNeighborHouse(28,50));Check("new_materials_deducted",g.Inventory[(int)Block.IronDoor]==5&&g.Inventory[(int)Block.Window]==5&&g.Inventory[(int)Block.Television]==5);g.Save();g.Load();Check("own_house_saved",g.HouseBase.Count>0&&g.World.Get(new Vector3Int(32,16,50))==Block.IronDoor);
            g.TravelTo(0);g.TravelTo(1);yield return null;Check("own_house_survives_revisit",g.World.Get(new Vector3Int(32,16,50))==Block.IronDoor&&g.HouseBase.Count>0);
            g.World.RebuildAll();g.World.RebuildAll();var fixtureCells=new HashSet<Vector3Int>();bool unique=true;foreach(var f in g.World.GetComponentsInChildren<HomeFixture>())unique&=fixtureCells.Add(f.Cell);Check("same_frame_rebuild_has_no_duplicate_fixtures",unique);yield return null;
            g.Player.Teleport(new Vector3(33,18,40),0,-5);g.Player.SetFlying(true);yield return null;ScenePicture(g,Path.Combine(folder,"33-new-house.png"));
            g.Player.Teleport(new Vector3(31.5f,16.3f,54));g.ToggleHomeTV();g.UseSeat(new Vector3Int(31,16,55));yield return new WaitForSecondsRealtime(.3f);ScenePicture(g,Path.Combine(folder,"34-own-bed-tv.png"));g.UseSeat(default);
            foreach(int x in new[]{30,32,34}){Find(new Vector3Int(x,17,58)).Toggle();Find(new Vector3Int(x,17,59)).Toggle();}g.Player.Teleport(new Vector3(32.5f,16.3f,55),0,0);Physics.SyncTransforms();Check("open_curtain_and_window_clear_actual_ray",!Physics.Raycast(new Vector3(32.5f,18,57),Vector3.forward,3,~0,QueryTriggerInteraction.Collide));yield return null;ScenePicture(g,Path.Combine(folder,"35-open-windows.png"));g.Player.Teleport(new Vector3(33,16,45));
            Check("demolish_owned_house",g.DemolishOwnHouse());Check("ground_restored_and_others_kept",g.World.Get(new Vector3Int(28,14,50))==Block.WoodTile&&g.World.Get(new Vector3Int(32,16,50))==Block.Air&&g.World.Get(new Vector3Int(13,16,8))==Block.IronDoor);
            foreach(var c in cost)g.Inventory[(int)c.Key]=c.Value+5;Check("rebuild_same_plot",g.BuildNeighborHouse(28,50));g.Save();var save=WorldSave.Read(WorldSave.PathName);save.inventory=new int[59];WorldSave.Validate(save);Check("legacy_59_inventory_migrates",save.inventory.Length==Blocks.Count);
        }
        IEnumerator HomeAlarmChecks(Sandbox g,string folder)
        {
            g.Respawn();g.TravelTo(1);yield return null;SkyHouse lift=null;foreach(var l in FindObjectsByType<SkyHouse>())if(Mathf.Abs(l.Anchor.z-13)<.1f&&l.Anchor.x<30)lift=l;
            g.Player.Teleport(new Vector3(30.5f,94,13));g.Player.SetFlying(true);g.MineBlock(new Vector3Int(16,17,15));float until=Time.unscaledTime+40;while(Time.unscaledTime<until&&lift.Floor!=SkyHouse.RoofFloor)yield return null;yield return new WaitForSecondsRealtime(.6f);
            bool pass=lift.Floor==SkyHouse.RoofFloor&&!lift.DoorOpen&&lift.BoardedCount==4;adventurePass&=pass;File.WriteAllLines(Path.Combine(folder,"home-alarm-checks.txt"),new[]{"residents_exit_furnished_house_and_board="+pass,lift.Diagnostics()});
        }
    }
}
