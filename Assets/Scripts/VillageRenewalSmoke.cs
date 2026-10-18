using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator VillageRenewalChecks(Sandbox g,string folder)
        {
            var checks=new List<string>();void Check(string key,bool value){checks.Add(key+"="+value);adventurePass&=value;File.WriteAllLines(Path.Combine(folder,"village-checks.txt"),checks);}
            g.TravelTo(1);yield return null;
            bool perHouse=true;for(int h=0;h<6;h++){int p=0,s=0;foreach(var a in FindObjectsByType<FarmAnimal>())if(a.Household==h){if(a.IsPig)p++;else s++;}perHouse&=p==3&&s==3;}Check("three_pigs_three_sheep_each_house",perHouse);
            int children=0,elders=0;foreach(var p in FindObjectsByType<SkyResidentInfo>()){if(p.Child)children++;else elders++;}Check("twelve_children_twelve_elders_and_six_adults",children==12&&elders==12&&FindObjectsByType<Villager>().Length==6);
            var wall=new Vector3Int(9,17,9);var wheat=new Vector3Int(17,15,36);var outside=new Vector3Int(30,39,30);Block original=g.World.Get(wall);
            g.MineBlock(wall);g.MineBlock(wheat);g.World.Edit(outside,Block.QuartzTile);int pork=g.Inventory[(int)Block.Pork];
            foreach(var a in FindObjectsByType<FarmAnimal>())if(a.IsPig){a.Hit(9);a.Hit(9);break;}
            Check("pig_drops_pork_once",g.Inventory[(int)Block.Pork]==pork+2);
            g.Save();g.Load();g.SetPaused(false);yield return null;
            Check("load_same_visit_keeps_harvested_state",g.World.Get(wall)==Block.Air&&g.World.Get(wheat)==Block.Air&&FindObjectsByType<FarmAnimal>().Length==35);
            g.TravelTo(0);g.TravelTo(1);yield return null;
            Check("reentry_repairs_house_and_wheat",g.World.Get(wall)==original&&g.World.Get(wheat)==Block.Wheat);
            Check("reentry_refreshes_livestock",FindObjectsByType<FarmAnimal>().Length==36&&g.DeadAnimals.Count==0);
            Check("outside_build_and_loot_preserved",g.World.Get(outside)==Block.QuartzTile&&g.Inventory[(int)Block.Pork]==pork+2);
            int lifts=0,floors=0;bool fourEach=true;foreach(var lift in FindObjectsByType<SkyHouse>()){lifts++;fourEach&=lift.GetComponentsInChildren<SkyResidentInfo>().Length==4;foreach(Transform t in lift.transform)if(t.name.StartsWith("Sky floor"))floors++;}Check("six_elevators_48_platforms",lifts==6&&floors==48&&fourEach);
            g.Player.Teleport(new Vector3(30,24,2),0,20);g.Player.SetFlying(true);g.SelectItem(1);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"22-renewed-village.png"));yield return new WaitForSecondsRealtime(1);
            g.Respawn();g.TravelTo(1);yield return null;
            SkyHouse subject=null;var houses=FindObjectsByType<SkyHouse>();foreach(var lift in houses)if(Vector2.Distance(new Vector2(lift.Anchor.x,lift.Anchor.z),new Vector2(26.5f,13))<.3f)subject=lift;
            g.Player.Teleport(new Vector3(30.5f,68,62));g.Player.SetFlying(true);g.MineBlock(new Vector3Int(16,17,15));
            float until=Time.unscaledTime+45;bool allReady=false;while(Time.unscaledTime<until){allReady=true;foreach(var lift in houses)allReady&=lift.Floor==4&&lift.BoardedCount==4;if(allReady)break;yield return null;}
            Check("all_six_households_reach_sky",allReady);foreach(var lift in houses)if(lift!=subject)lift.enabled=false;
            if(!allReady)foreach(var lift in houses)Debug.Log("BOARDING_AUDIT "+lift.Anchor+" "+lift.BoardingDebug);
            g.Player.Teleport(new Vector3(30.5f,68,13));g.Player.SetFlying(true);
            until=Time.unscaledTime+4;while(Time.unscaledTime<until&&subject.Phase!="瞄准")yield return null;
            Check("family_boards_and_reaches_top_floor",subject.BoardedCount==4&&subject.Floor==4);
            Debug.Log("SKY_AUDIT phase="+subject.Phase+" floor="+subject.Floor+" player="+g.Player.transform.position+" hp="+g.Health+" paused="+g.Paused+" anchor="+subject.Anchor);
            Check("catch_is_telegraphed",subject.Phase=="瞄准");float hp=g.Health;
            until=Time.unscaledTime+4;while(Time.unscaledTime<until&&g.Health==hp)yield return null;Check("stationary_player_loses_half_heart",g.Health==hp-.5f);
            subject.enabled=false;Vector3 eye=new Vector3(34,70,20),toward=(subject.Anchor+subject.ReachTip)*.5f-eye;g.Player.Teleport(eye,Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg,-Mathf.Atan2(toward.y,new Vector2(toward.x,toward.z).magnitude)*Mathf.Rad2Deg);g.Player.SetFlying(true);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"24-reaching-human-chain.png"));yield return new WaitForSecondsRealtime(1);g.Player.Teleport(new Vector3(30.5f,68,13));g.Player.SetFlying(true);subject.enabled=true;
            until=Time.unscaledTime+7;while(Time.unscaledTime<until&&subject.Phase!="瞄准")yield return null;
            hp=g.Health;g.Player.Teleport(new Vector3(30.5f,68,16));g.Player.SetFlying(true);yield return new WaitForSecondsRealtime(3.2f);Check("sideways_dodge_avoids_locked_aim",g.Health==hp);
            g.Player.Teleport(new Vector3(32,70,22),-35,24);g.Player.SetFlying(true);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"23-sky-elevator-chain.png"));yield return new WaitForSecondsRealtime(1);
            g.SetPaused(true);float y=subject.transform.GetChild(0).position.y;hp=g.Health;yield return new WaitForSecondsRealtime(.5f);Check("pause_freezes_damage",g.Health==hp);
            g.TravelTo(0);g.Save();
        }
    }
}
