using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands
{
    public partial class SmokeTest
    {
        static void ScenePicture(Sandbox g,string path){var old=g.Player.View.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1440,900,24);var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);g.Player.View.targetTexture=rt;g.Player.View.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());g.Player.View.targetTexture=old;RenderTexture.active=active;rt.Release();Object.Destroy(rt);Object.Destroy(tex);}
        IEnumerator VillageLifeChecks(Sandbox g,string folder)
        {
            var checks=new List<string>();void Check(string k,bool v){checks.Add(k+"="+v);adventurePass&=v;File.WriteAllLines(Path.Combine(folder,"life-checks.txt"),checks);}
            g.Respawn();g.TravelTo(1);yield return null;
            var one=new Vector3Int(9,17,9);var two=new Vector3Int(9,18,9);Block b=g.World.Get(one);int count=g.Inventory[(int)b];g.MineBlock(one);g.MineBlock(two);Check("theft_recorded",g.Theft.Count==2);
            g.Player.Teleport(new Vector3(9.5f,17,9.5f));Check("return_cannot_trap_player_in_block",!g.ReturnStolen()&&g.Theft.Count==2);g.Player.Teleport(g.World.SafeSpawn());
            g.Save();g.Load();g.SetPaused(false);Check("theft_persists_across_load",g.Theft.Count==2&&g.VillageAngryUntil>Time.time);
            Check("return_one_keeps_remaining_claim",g.ReturnStolen()&&g.Theft.Count==1&&g.VillageAngryUntil>Time.time);
            Check("return_all_restores_and_forgives",g.ReturnStolen()&&g.Theft.Count==0&&g.VillageAngryUntil<=Time.time&&g.Inventory[(int)b]==count&&g.World.Get(one)==b);
            Check("returned_item_remains_owned",g.MineBlock(one)&&g.Theft.Count==1);g.ReturnStolen();yield return null;
            var person=FindObjectsByType<Villager>()[0];Vector3 start=person.transform.position;for(int i=0;i<9;i++)person.RegisterHit();Check("nine_hits_do_not_relocate",person.RepelHits==9&&person.transform.position==start);
            person.RegisterHit();float far=0;foreach(int x in new[]{9,42})foreach(int z in new[]{8,25,43})far=Mathf.Max(far,(new Vector3(x+4.5f,15,z-1)-g.Player.transform.position).sqrMagnitude);
            Check("tenth_hit_sends_to_farthest_house",person.RepelHits==0&&Mathf.Abs((person.transform.position-g.Player.transform.position).sqrMagnitude-far)<.1f&&person.RetreatUntil>Time.time);
            Check("muttering_clip_available",Resources.Load<AudioClip>("VillagerMutter")!=null);
            g.TravelTo(0);g.TravelTo(1);yield return null;SkyHouse lift=FindObjectsByType<SkyHouse>()[0];
            Vector3 basePos=lift.CarCenter;g.Player.Teleport(basePos+Vector3.forward*5);g.Player.SetFlying(true);Check("call_public_lift",lift.RequestFloor(4));
            float until=Time.unscaledTime+30;while(Time.unscaledTime<until&&(lift.Floor!=4||!lift.DoorOpen))yield return null;Check("public_lift_reaches_top_and_opens",lift.Floor==4&&lift.DoorOpen&&g.Player.transform.position.y<20);
            lift.RequestFloor(-1);yield return new WaitForSecondsRealtime(2);Check("calling_does_not_teleport_lift",lift.CarY>16&&lift.CarY<66);
            until=Time.unscaledTime+30;while(Time.unscaledTime<until&&(lift.Floor!=-1||!lift.DoorOpen))yield return null;Check("lift_returns_to_caller",lift.Floor==-1&&lift.DoorOpen);
            g.Player.Teleport(lift.CarCenter);yield return null;Check("player_enters_actual_car",lift.PlayerInside);lift.RequestFloor(2);bool closed=false;
            until=Time.unscaledTime+25;while(Time.unscaledTime<until&&(lift.Floor!=2||!lift.DoorOpen)){closed|=!lift.DoorOpen;yield return null;}
            Check("doors_close_and_passenger_rides",closed&&lift.Floor==2&&lift.DoorOpen&&lift.PlayerInside&&g.Player.transform.position.y>46&&g.Player.transform.position.y<49);
            g.Player.Teleport(lift.Kiosk(2)+Vector3.back*1.3f-Vector3.up*.8f);g.Player.SetFlying(true);g.Inventory[(int)Block.Coin]=20;int beds=g.Inventory[(int)Block.Bed];Check("shop_exact_price_and_item",g.BuyAt(lift,2,0)&&g.Inventory[(int)Block.Coin]==15&&g.Inventory[(int)Block.Bed]==beds+1);
            g.Inventory[(int)Block.Coin]=0;Check("insufficient_funds_no_charge",!g.BuyAt(lift,2,0)&&g.Inventory[(int)Block.Bed]==beds+1);
            Vector3 toward=lift.CarCenter-g.Player.View.transform.position;g.Player.Teleport(g.Player.transform.position,Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg,0);g.Player.SetFlying(true);yield return null;ScenePicture(g,Path.Combine(folder,"25-glass-lift.png"));
            var site=new Vector3Int(4,15,28);Check("purchased_furniture_can_be_placed",g.TryPlace(site,Block.Bed)&&g.World.Get(site)==Block.Bed&&g.Inventory[(int)Block.Bed]==beds);
            g.Save();g.Load();g.SetPaused(false);Check("furniture_survives_save",g.World.Get(site)==Block.Bed);
            g.Player.Teleport(new Vector3(14,17,8),0,12);g.Player.SetFlying(true);yield return null;ScenePicture(g,Path.Combine(folder,"26-furnished-house.png"));
            g.TravelTo(0);g.Save();
        }
    }
}
