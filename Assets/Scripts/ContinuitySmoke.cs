using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands {public partial class SmokeTest {
 IEnumerator ContinuityChecks(Sandbox g,string folder){
  var list=new List<string>();void Check(string key,bool ok){list.Add(key+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"continuity-checks.txt"),list);}
  g.Respawn();g.RoyalIdentity=true;g.PrincessReturnVersion=0;g.TravelTo(6);yield return null;
  Check("explicit_request_reopens_once",!g.RoyalIdentity&&g.PrincessReturnVersion==Sandbox.RequestedPrincessReturn);
  g.RoyalIdentity=true;g.Save();g.TravelTo(0);g.TravelTo(6);yield return null;g.Load();yield return null;
  Check("saved_retirement_survives_reload",g.RoyalIdentity&&FindObjectsByType<CrownChallenge>().Length==0);
  g.HandlePortraitKey(KeyCode.C);yield return null;Check("c_opens_crowned_portrait",g.InspectingHero&&g.PortraitHasCrown);
  ScenePicture(g,Path.Combine(folder,"portrait.png"));g.HandlePortraitKey(KeyCode.C);Check("c_closes_while_paused",!g.InspectingHero&&!g.Paused);
  g.HandlePortraitKey(KeyCode.F6);g.HandlePortraitKey(KeyCode.Escape);Check("f6_and_escape",!g.InspectingHero&&!g.Paused);
  Check("sun_shadow_body_exists",g.HasBodyShadow);g.EquipFlowerWand();Check("h_flower_wand",g.FlowerWandActive);g.SelectItem((int)Block.Stone);Check("inventory_restores_tool",!g.FlowerWandActive);
  var castle=FindAnyObjectByType<CastleSystem>();g.Player.Teleport(new Vector3(31.5f,16,10));yield return null;Check("royal_gate_opens",castle.PalaceGate!=null&&castle.PalaceGate.Open);castle.CallRoyalRide();Check("roofed_royal_cabin",GameObject.Find("斜屋顶")!=null);g.Player.Teleport(new Vector3(36,18,4),-38,15);yield return null;ScenePicture(g,Path.Combine(folder,"cabin.png"));g.Player.Teleport(new Vector3(31.5f,CastleSystem.FloorY(9),33));Check("royal_sits_on_tenth_floor_throne",castle.UseThrone()&&g.RoyalThroneSeated);Check("throne_exit",castle.UseThrone()&&!g.RoyalThroneSeated);castle.CrownedPrincess();yield return null;Check("royal_never_jailed",!castle.Jailed);
  yield return new WaitForSecondsRealtime(48);Check("no_retired_princess_procession",!castle.ProcessionActive&&FindObjectsByType<CrownChallenge>().Length==0);
  g.TravelTo(1);g.HouseBase.Add(new SavedBlock{x=28,y=15,z=50,id=0});g.CompanionUnlocked=true;g.CompanionEnabled=false;g.CompanionRevision=0;g.RestoreCompanionForHouse();yield return null;Check("legacy_house_restores_child",g.CompanionEnabled&&g.CompanionUnlocked);g.Save();g.Load();yield return null;Check("child_persists",g.CompanionEnabled);g.ToggleCompanion();g.Save();g.Load();Check("deliberate_p_toggle_persists",!g.CompanionEnabled);
  var c=new Crafting();var bag=new int[Blocks.Count];Block[] pattern={Block.WoodPickaxe,Block.StonePickaxe,Block.IronPickaxe,Block.CrystalPickaxe,Blocks.StarDrill,Block.CrystalHammer,Blocks.IronDrill,Blocks.CrystalRod};for(int i=0;i<pattern.Length;i++){bag[(int)pattern[i]]=1;c.SetCell(i,pattern[i],bag);}Check("combined_recipe",c.Match()?.Output==Blocks.CombinedStarPick);
  var save=WorldSave.Capture(g.World,g.Player,g.Inventory,g.Selected);save.inventory=new int[74];WorldSave.Validate(save);Check("old_inventory_migration",save.inventory.Length==Blocks.Count);
  g.TravelTo(5);Check("lava_depth_is_in_one_world",g.World.Region==1&&g.Player.transform.position.y<-32);g.Inventory[(int)Blocks.CombinedStarPick]=1;g.SelectItem((int)Blocks.CombinedStarPick);var p=new Vector3Int(30,0,30);Check("combined_opens_connected_bedrock",g.MineDepth(p,3.1f)&&g.World.Get(p)==Block.Air);g.Player.Teleport(new Vector3(31,-136,22));g.UpdateConnectedDepth();g.Save();g.Load();Check("connected_depth_saved",g.World.Region==1&&g.Player.transform.position.y<-120&&g.World.Get(p)==Block.Air);g.Player.Teleport(new Vector3(31,-177,30));yield return null;g.Player.View.transform.position=g.Player.transform.position+new Vector3(0,7,-4);g.Player.View.transform.LookAt(g.Player.transform.position+new Vector3(1,0,1));ScenePicture(g,Path.Combine(folder,"sun-shadow.png"));
 }
}}
