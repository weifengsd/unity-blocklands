using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator ExpansionChecks(Sandbox g,string folder)
        {
            var checks=new List<string>();
            void Check(string label,bool value){checks.Add(label+"="+value);adventurePass&=value;File.WriteAllLines(Path.Combine(folder,"expansion-checks.txt"),checks);}
            g.TravelTo(0);yield return null;Check("one_princess_friend_at_home",g.ForestFriendPosition!=Vector3.zero&&FindObjectsByType<Villager>().Length==0);
            yield return new WaitForSecondsRealtime(1);Vector3 f=g.ForestFriendPosition;g.SelectItem(1);g.Player.Teleport(f+new Vector3(0,0,5),180,0);g.Player.SetFlying(true);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"16-home-friend.png"));yield return new WaitForSecondsRealtime(1);
            g.TravelTo(4);g.Player.Teleport(new Vector3(30,17,21));int gems=g.Inventory[(int)Block.ShadowCrystal];
            Check("next_battle_revives_dragon",g.EnterPortal()&&g.DragonHealth==100);yield return null;
            Check("new_dragon_model",FindAnyObjectByType<DragonSculpt>()!=null);
            var portrait=FindAnyObjectByType<DreamDragon>();portrait.enabled=false;portrait.transform.SetPositionAndRotation(new Vector3(30,24,30),Quaternion.identity);Vector3 camera=new Vector3(42,29,44),look=new Vector3(30,26.5f,30)-camera;g.Player.Teleport(camera,Mathf.Atan2(look.x,look.z)*Mathf.Rad2Deg,-Mathf.Atan2(look.y,new Vector2(look.x,look.z).magnitude)*Mathf.Rad2Deg);g.Player.SetFlying(true);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"21-dragon-portrait.png"));yield return new WaitForSecondsRealtime(1);portrait.enabled=true;
            g.HitDragon(30);g.TravelTo(4);g.Player.Teleport(new Vector3(30,17,21));g.EnterPortal();Check("unfinished_battle_keeps_hp",g.DragonHealth==70);
            g.HitDragon(70);Check("new_round_awards_once",g.VictoryOpen&&g.Inventory[(int)Block.ShadowCrystal]==gems+20);g.Load();Check("loading_win_does_not_duplicate",g.DragonHealth==0&&g.Inventory[(int)Block.ShadowCrystal]==gems+20);
            g.TravelTo(5);yield return null;
            bool grass=false;int lava=0;for(int x=0;x<64;x++)for(int z=0;z<64;z++)for(int y=0;y<48;y++){Block b=g.World.Get(new Vector3Int(x,y,z));grass|=b==Block.Grass;if(b==Block.Lava)lava++;}
            Check("underground_no_grass_and_large_lava",!grass&&lava>1500);
            Check("cave_has_roof",Blocks.Solid(g.World.Get(new Vector3Int(30,42,20))));
            Check("six_pig_sheep_spirits",FindObjectsByType<CavernSpirit>().Length==6);
            g.Inventory[(int)Block.Iron]=3;g.Crafting.Clear();g.Crafting.SetCell(0,Block.Iron,g.Inventory);g.Crafting.SetCell(2,Block.Iron,g.Inventory);g.Crafting.SetCell(4,Block.Iron,g.Inventory);
            Check("bucket_v_recipe",g.CollectCraft()&&g.Inventory[(int)Block.Bucket]==1&&g.Inventory[(int)Block.Iron]==0);
            var source=new Vector3Int(26,10,18);g.SelectItem((int)Block.Stone);Check("bare_hand_cannot_collect_lava",g.World.Get(source)==Block.Lava&&!g.MineBlock(source));
            g.SelectItem((int)Block.Bucket);Check("bucket_collects_lava",g.MineBlock(source)&&g.World.Get(source)==Block.Air&&g.Inventory[(int)Block.Bucket]==0&&g.Inventory[(int)Block.LavaBucket]==1);
            Check("pour_lava_returns_bucket",g.TryPlace(source,Block.LavaBucket)&&g.World.Get(source)==Block.Lava&&g.Inventory[(int)Block.Bucket]==1&&g.Inventory[(int)Block.LavaBucket]==0);
            g.Save();g.Load();g.SetPaused(false);Check("lava_and_bucket_save",g.World.Region==5&&g.World.Get(source)==Block.Lava&&g.Inventory[(int)Block.Bucket]==1);
            g.Player.Teleport(new Vector3(30,18,13),0,12);g.Player.SetFlying(true);yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"17-underground.png"));yield return new WaitForSecondsRealtime(1);
            g.Player.Teleport(new Vector3(22,14,19),-30,12);g.Player.SetFlying(true);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"18-pig-spirit.png"));yield return new WaitForSecondsRealtime(1);
            g.TravelTo(1);g.Player.Teleport(new Vector3(30.5f,25.8f,18.5f));g.Player.SetFlying(true);g.MineBlock(new Vector3Int(16,17,15));
            var tower=FindAnyObjectByType<VillageTower>();yield return new WaitForSecondsRealtime(21);
            int ready=0;foreach(var v in tower.Members)if(v.TowerReady)ready++;
            Check("villagers_build_six_tier_tower",tower.Active&&ready==6);
            Check("tower_can_reach_low_flying_player",g.Health<10);
            g.Player.Teleport(new Vector3(30.5f,26.5f,11.5f),0,18);g.Player.SetFlying(true);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"19-villager-tower.png"));yield return new WaitForSecondsRealtime(1);
            yield return new WaitForSecondsRealtime(5);bool descended=true;foreach(var v in tower.Members)descended&=!v.OnTower;
            Check("tower_descends_when_player_moves_away",!tower.Active&&descended);
            g.TravelTo(0);yield return null;Check("friend_returns_without_duplicates",FindObjectsByType<Villager>().Length==0&&g.ForestFriendPosition!=Vector3.zero);
            g.OpenTravel();yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"20-six-regions.png"));yield return new WaitForSecondsRealtime(1);g.Save();
        }
    }
}
