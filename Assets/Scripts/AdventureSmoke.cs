using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    public partial class SmokeTest
    {
        bool adventurePass=true;
        IEnumerator AdventureChecks(Sandbox game,string folder)
        {
            var checks=new List<string>();
            void Check(string label,bool value){checks.Add(label+"="+value);adventurePass&=value;File.WriteAllLines(Path.Combine(folder,"adventure-checks.txt"),checks);}
            game.TravelTo(0);game.SetPaused(false);
            Check("starts_with_ten_hearts",game.Health==10);
            yield return new WaitForSecondsRealtime(2.2f);
            Check("half_heart_damage",game.Damage(.5f,"测试")&&game.Health==9.5f);
            Check("same_frame_damage_not_repeated",!game.Damage(.5f,"测试")&&game.Health==9.5f);
            game.Inventory[(int)Block.Apple]=1;game.SelectItem((int)Block.Apple);
            game.OpenCrafting();Check("eat_in_backpack",game.EatSelected()&&game.Health==10&&game.Inventory[(int)Block.Apple]==0);
            game.Inventory[(int)Block.Apple]=1;Check("full_health_preserves_food",!game.EatSelected()&&game.Inventory[(int)Block.Apple]==1);
            Check("paused_damage_blocked",!game.Damage(3,"测试"));
            Check("dreamland_not_on_travel_menu",!game.TravelTo(3)&&game.World.Region==0);
            game.Inventory[(int)Block.Wheat]=3;game.Crafting.Clear();
            for(int i=0;i<3;i++)game.Crafting.SetCell(i,Block.Wheat,game.Inventory);
            int bread=game.Inventory[(int)Block.Bread];Check("bread_recipe_consumes_three_wheat",game.CollectCraft()&&game.Inventory[(int)Block.Wheat]==0&&game.Inventory[(int)Block.Bread]==bread+1);
            game.TravelTo(1);yield return null;
            Check("six_households_36_animals",FindObjectsByType<FarmAnimal>().Length==36);
            game.Player.Teleport(new Vector3(30,20,15),-35,24);game.Player.SetFlying(true);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"11a-farm.png"));yield return new WaitForSecondsRealtime(1);
            int beef=game.Inventory[(int)Block.Pork],mutton=game.Inventory[(int)Block.Mutton];
            foreach(var a in FindObjectsByType<FarmAnimal>()){a.Hit(5);a.Hit(5);}
            Check("animals_drop_meat_once",game.DeadAnimals.Count==36&&game.Inventory[(int)Block.Pork]==beef+36&&game.Inventory[(int)Block.Mutton]==mutton+36);
            Check("theft_alert",game.MineBlock(new Vector3Int(11,16,14))&&game.VillageAngryUntil>Time.time+30);
            // Real NPC/controller movement and collision: stand on the open main street.
            game.Player.Teleport(new Vector3(30,15,18));yield return new WaitForSecondsRealtime(5);
            Check("villagers_physically_chase_and_hit",game.Health<10&&game.Health>=7);
            game.Player.Teleport(new Vector3(30,19,21),180,20);game.Player.SetFlying(true);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"11-village-alert.png"));yield return new WaitForSecondsRealtime(1);
            game.TravelTo(0);game.TravelTo(1);yield return null;
            Check("animals_refresh_after_travel",FindObjectsByType<FarmAnimal>().Length==36);
            game.TravelTo(2);game.Inventory[(int)Block.DreamCrystal]=0;
            bool mined=true;for(int x=21;x<=23;x++)for(int y=18;y<=19;y++)mined&=game.MineBlock(new Vector3Int(x,y,24));
            Check("six_green_crystals_mined",mined&&game.Inventory[(int)Block.DreamCrystal]==6);
            Check("crystals_alone_cannot_unlock_map",!game.TravelTo(3));
            game.TravelTo(4);game.Player.Teleport(new Vector3(30,17,21),0,0);game.Inventory[(int)Block.Obsidian]=0;
            Physics.SyncTransforms();Check("grass_beneath_thin_bamboo_keeps_face",Physics.Raycast(new Vector3(7.1f,17.5f,9.1f),Vector3.down,out var groundHit,1)&&Mathf.Abs(groundHit.point.y-17)<.01f);
            Check("no_partial_portal_when_short",!game.BuildPortal()&&game.World.Get(new Vector3Int(28,17,25))==Block.Air);
            Check("green_crystals_need_frame",!game.EnterPortal()&&game.Inventory[(int)Block.DreamCrystal]==6);
            mined=true;for(int x=35;x<=36;x++)for(int z=19;z<=25;z++)mined&=game.MineBlock(new Vector3Int(x,17,z));
            Check("fourteen_frame_blocks_mined",mined&&game.Inventory[(int)Block.Obsidian]==14);
            game.Player.Teleport(new Vector3(29,25,13),0,25);game.Player.SetFlying(true);
            yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"12-lava-bamboo.png"));yield return new WaitForSecondsRealtime(1);
            game.Player.Teleport(new Vector3(30,17,21),0,0);
            Check("build_frame_spends_exactly_fourteen",game.BuildPortal()&&game.PortalComplete()&&game.Inventory[(int)Block.Obsidian]==0);
            Check("existing_frame_no_double_charge",game.BuildPortal()&&game.Inventory[(int)Block.Obsidian]==0);
            game.Inventory[(int)Block.DreamCrystal]=5;Check("five_green_crystals_rejected",!game.EnterPortal()&&!game.GateUnlocked&&game.Inventory[(int)Block.DreamCrystal]==5);
            game.Inventory[(int)Block.DreamCrystal]=6;game.Player.Teleport(new Vector3(30,17,19),0,-9);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"13-portal.png"));yield return new WaitForSecondsRealtime(1);
            Check("portal_enters_and_consumes_six",game.EnterPortal()&&game.World.Region==3&&game.GateUnlocked&&game.Inventory[(int)Block.DreamCrystal]==0);
            yield return null;
            Check("dragon_has_100_hp",FindAnyObjectByType<DreamDragon>()!=null&&game.DragonHealth==100);
            game.Player.Teleport(new Vector3(30,23,23),0,-8);game.Player.SetFlying(true);
            yield return new WaitForSecondsRealtime(6);
            // Aim the actual first-person ray at the moving dragon; no input injection.
            var boss=FindAnyObjectByType<DreamDragon>();
            Vector3 toBoss=boss.transform.position+Vector3.up-game.Player.View.transform.position;
            game.Player.Teleport(game.Player.transform.position,Mathf.Atan2(toBoss.x,toBoss.z)*Mathf.Rad2Deg,-Mathf.Atan2(toBoss.y,new Vector2(toBoss.x,toBoss.z).magnitude)*Mathf.Rad2Deg);game.Player.SetFlying(true);
            game.Inventory[(int)Block.IronSword]=1;game.SelectItem((int)Block.IronSword);Physics.SyncTransforms();
            Check("actual_ray_hits_dragon_with_sword",game.StrikeActor()&&game.DragonHealth==90);
            Vector3 closeView=game.Player.transform.position;float closeYaw=game.Player.transform.eulerAngles.y,closePitch=game.Player.Pitch;
            Vector3 showcase=boss.transform.position-new Vector3(0,2,15);game.Player.Teleport(showcase,0,-4);game.Player.SetFlying(true);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"14-dragon.png"));yield return new WaitForSecondsRealtime(1);
            game.Player.Teleport(closeView,closeYaw,closePitch);game.Player.SetFlying(true);game.SetPaused(true);
            game.HitDragon(10);Check("paused_boss_damage_blocked",game.DragonHealth==90);
            game.SetPaused(false);bool fired=false;float observeUntil=Time.unscaledTime+7;
            while(Time.unscaledTime<observeUntil){fired|=FindObjectsByType<DragonBreath>().Length>0;yield return null;}
            Check("dragon_ai_fires_projectiles",fired);
            // Test projectile hit accounting on a clear sightline, separately from AI aim/dodging.
            boss.enabled=false;foreach(var old in FindObjectsByType<DragonBreath>())Destroy(old.gameObject);
            game.Player.Teleport(new Vector3(30,35,12));game.Player.SetFlying(true);yield return new WaitForSecondsRealtime(1.3f);
            float hp=game.Health;var shot=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(shot.GetComponent<Collider>());
            shot.transform.position=game.Player.transform.position+new Vector3(0,1,3);shot.AddComponent<DragonBreath>().Initialize(game,Vector3.back);
            yield return new WaitForSecondsRealtime(1);
            Check("dragon_projectile_damages_player_once",game.Health==hp-1);
            game.Save();float savedHp=game.Health;game.Load();
            Debug.Log("SAVE_AUDIT hp="+game.Health+" expected="+savedHp+" dragon="+game.DragonHealth+" dead="+game.DeadAnimals.Count+" gate="+game.GateUnlocked);
            Check("adventure_save_restores",game.GateUnlocked&&game.DragonHealth==90&&game.Health==savedHp&&game.DeadAnimals.Count==0);
            game.SetPaused(false);game.TravelTo(4);game.Player.Teleport(new Vector3(30,17,21));
            Check("portal_frame_persists",game.PortalComplete());
            Check("activated_portal_no_repeat_cost",game.EnterPortal()&&game.Inventory[(int)Block.DreamCrystal]==0);
            Check("unlocked_still_requires_physical_portal",!game.TravelTo(3));
            int prize=game.Inventory[(int)Block.ShadowCrystal];game.HitDragon(90);game.HitDragon(90);
            Check("boss_victory_once",game.DragonHealth==0&&game.VictoryOpen&&game.Paused&&game.Inventory[(int)Block.ShadowCrystal]==prize+20);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"15-victory.png"));yield return new WaitForSecondsRealtime(1);
            game.Load();Check("victory_and_loot_persist",game.DragonHealth==0&&game.Inventory[(int)Block.ShadowCrystal]==prize+20);
            game.TravelTo(0);game.SetPaused(false);yield return new WaitForSecondsRealtime(2.2f);
            Check("zero_health_ends_adventure",game.Damage(10,"测试")&&game.Health==0&&game.Paused);
            int retained=game.Inventory[(int)Block.ShadowCrystal];game.Respawn();Check("respawn_preserves_inventory",game.Health==10&&game.World.Region==0&&game.Inventory[(int)Block.ShadowCrystal]==retained);
            game.Save();
        }
    }
}
