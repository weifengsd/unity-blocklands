using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator MarketChecks(Sandbox g,string folder)
        {
            var checks=new List<string>();void Check(string k,bool v){checks.Add(k+"="+v);adventurePass&=v;File.WriteAllLines(Path.Combine(folder,"market-checks.txt"),checks);}
            g.Respawn();g.TravelTo(1);yield return null;bool catalog=true;foreach(var row in Sandbox.Offers){var ids=new HashSet<Block>();foreach(var o in row)catalog&=o.Price>0&&o.Item!=Block.Coin&&ids.Add(o.Item);catalog&=row.Length==24;}Check("24_distinct_priced_items_each_floor",catalog);
            int money=g.Inventory[(int)Block.Coin];g.Inventory[(int)Block.Planks]=2;g.Inventory[(int)Block.Stick]=2;g.Crafting.Clear();g.Crafting.SetCell(0,Block.Planks,g.Inventory);g.Crafting.SetCell(1,Block.Planks,g.Inventory);Check("two_planks_make_five_yuan",g.CollectCraft()&&g.Inventory[(int)Block.Coin]==money+5&&g.Inventory[(int)Block.Planks]==0&&g.Inventory[(int)Block.Stick]==2);
            var lift=FindObjectsByType<SkyHouse>()[0];g.Player.Teleport(lift.Kiosk(0)-Vector3.up*.7f+Vector3.back*1.2f);g.Player.SetFlying(true);g.Inventory[(int)Block.Coin]=10;int crystal=g.Inventory[(int)Block.ShadowCrystal];Check("shopping_uses_coin_not_crystal",g.BuyAt(lift,0,0)&&g.Inventory[(int)Block.Coin]==8&&g.Inventory[(int)Block.ShadowCrystal]==crystal);
            g.Player.Teleport(lift.StoreOrigin(0)+new Vector3(0,.2f,4.8f));g.Player.SetFlying(true);Check("shopping_inside_large_room",g.BuyAt(lift,0,1));yield return null;g.Player.Teleport(lift.StoreOrigin(0)+new Vector3(0,.2f,.15f),0,3);g.Player.SetFlying(true);yield return null;ScenePicture(g,Path.Combine(folder,"27-large-market.png"));
            bool rooms=true;foreach(var l in FindObjectsByType<SkyHouse>()){int roomCount=0;foreach(Transform child in l.transform)if(child.name.StartsWith("商店房间"))roomCount++;rooms&=roomCount==7;}Check("seven_shop_rooms_per_elevator",rooms);
            g.TravelTo(0);g.TravelTo(1);yield return null;var family=FindObjectsByType<SkyResidentInfo>();bool childFound=false,elderFound=false;foreach(var person in family){if((person.Child&&childFound)||(!person.Child&&elderFound))continue;yield return new WaitForSecondsRealtime(.15f);var body=person.GetComponent<CharacterController>();body.enabled=false;person.transform.position=new Vector3(30,15,20);body.enabled=true;g.Player.Teleport(new Vector3(30,15,16),0,10);g.Player.SetFlying(true);Physics.SyncTransforms();Check(person.Child?"child_actual_ray_hit":"elder_actual_ray_hit",g.StrikeActor()&&person.Hits==1);for(int n=0;n<8;n++)person.RegisterHit();Check(person.Child?"child_nine_hits":"elder_nine_hits",person.Hits==9&&!person.Owner.ResidentResting(person.Index));person.RegisterHit();Check(person.Child?"child_ten_hits_retreat":"elder_ten_hits_retreat",person.Owner.ResidentResting(person.Index));if(person.Child)childFound=true;else elderFound=true;if(childFound&&elderFound)break;}
            Check("family_count_preserved",FindObjectsByType<SkyResidentInfo>().Length==24);yield return new WaitForSecondsRealtime(10.5f);bool awake=true;foreach(var p in family)awake&=!p.Owner.ResidentResting(p.Index);Check("family_returns_after_rest",awake);
            g.Respawn();g.TravelTo(1);g.Player.Teleport(new Vector3(28,16,43),0,12);g.Player.SetFlying(true);yield return null;var plot=Sandbox.NeighborPlot;var plan=g.World.PlanHouse(plot.x,plot.y);var cost=g.HouseCost(plot.x,plot.y);int countBefore=g.Inventory[(int)Block.Planks];foreach(var c in cost)g.Inventory[(int)c.Key]=0;Check("house_missing_materials_refused",!g.BuildNeighborHouse(plot.x,plot.y));foreach(var c in cost)g.Inventory[(int)c.Key]=c.Value;
            Check("same_house_built_with_exact_materials",g.BuildNeighborHouse(plot.x,plot.y));bool empty=true;foreach(var c in cost)empty&=g.Inventory[(int)c.Key]==0;Check("house_materials_deducted_once",empty);
            bool matches=true;foreach(var e in plan)matches&=g.World.Get(e.Key)==e.Value;Check("house_matches_village_blueprint",matches);Check("house_cannot_overwrite_existing",!g.BuildNeighborHouse(plot.x,plot.y));
            g.TravelTo(0);g.TravelTo(1);matches=true;foreach(var e in plan)matches&=g.World.Get(e.Key)==e.Value;Check("new_house_survives_village_refresh",matches);g.Save();g.Load();matches=true;foreach(var e in plan)matches&=g.World.Get(e.Key)==e.Value;Check("new_house_survives_reload",matches);
            g.Player.Teleport(new Vector3(33,24,40),0,24);g.Player.SetFlying(true);g.SetPaused(false);yield return null;ScenePicture(g,Path.Combine(folder,"28-neighbor-house.png"));g.TravelTo(0);yield return new WaitForSecondsRealtime(2.2f);g.Damage(10,"测试");g.OpenHouseMenu();Check("builder_cannot_block_death_screen",!g.HouseMenuOpen);g.Respawn();g.Save();
        }
    }
}
