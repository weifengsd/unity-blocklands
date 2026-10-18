using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator LiftChecks(Sandbox g,string folder)
        {
            var checks=new List<string>();void Check(string k,bool v){checks.Add(k+"="+v);adventurePass&=v;File.WriteAllLines(Path.Combine(folder,"lift-checks.txt"),checks);}
            g.Respawn();g.TravelTo(1);yield return null;SkyHouse lift=null;foreach(var l in FindObjectsByType<SkyHouse>())if(Mathf.Abs(l.Anchor.z-13)<.1f&&l.Anchor.x<30)lift=l;
            var ids=new HashSet<int>();foreach(var c in lift.GetComponentsInChildren<VillageControl>())if(c.Cabin)ids.Add(c.Floor);Check("nine_visible_floor_buttons",ids.Count==9&&ids.Contains(-1)&&ids.Contains(6)&&ids.Contains(7));Check("seven_shop_catalogs",Sandbox.Offers.Length==7);
            g.Player.Teleport(new Vector3(30,15,5));g.Player.SetFlying(true);lift.StopHistory.Clear();lift.RequestFloor(6);bool remainedClosed=true;float until=Time.unscaledTime+20;
            while(Time.unscaledTime<until&&(lift.Floor!=6||!lift.DoorOpen)){if(lift.CarY>16&&lift.CarY<85.9f)remainedClosed&=!lift.DoorOpen;yield return null;}
            Check("skip_unselected_floors_closed",remainedClosed&&lift.StopHistory.Count==1&&lift.StopHistory[0]==6);Check("selected_seventh_opens",lift.Floor==6&&lift.DoorOpen);
            lift.RequestFloor(-1);until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=-1||!lift.DoorOpen))yield return null;Check("explicit_ground_call_opens",lift.Floor==-1&&lift.DoorOpen);
            lift.StopHistory.Clear();until=Time.unscaledTime+45;while(Time.unscaledTime<until&&!lift.Requested(1))yield return null;Check("residents_select_second",lift.Requested(1)&&lift.BoardedCount==4);
            g.Player.Teleport(lift.CarCenter);lift.RequestFloor(6);until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=1||!lift.DoorOpen))yield return null;
            Check("shared_trip_stops_second_first",lift.StopHistory.Count>0&&lift.StopHistory[0]==1&&lift.ResidentsVisiting(1)==4);
            until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=6||!lift.DoorOpen))yield return null;
            Check("player_continues_seventh",lift.Floor==6&&lift.PlayerInside&&g.Player.transform.position.y>86&&lift.ResidentsVisiting(1)==4);Check("no_extra_stops_in_shared_trip",lift.StopHistory.Count==2&&lift.StopHistory[1]==6);
            VillageControl roof=null;foreach(var b in lift.GetComponentsInChildren<VillageControl>())if(b.Cabin&&b.Floor==7)roof=b;
            Vector3 eye=g.Player.View.transform.position,aim=roof.transform.position-eye;g.Player.Teleport(g.Player.transform.position,Mathf.Atan2(aim.x,aim.z)*Mathf.Rad2Deg,-Mathf.Atan2(aim.y,new Vector2(aim.x,aim.z).magnitude)*Mathf.Rad2Deg);Physics.SyncTransforms();bool selected=g.TryDirectFloorButton();if(Physics.Raycast(g.Player.View.transform.position,g.Player.View.transform.forward,out var debugHit,5,~0,QueryTriggerInteraction.Collide))Debug.Log("BUTTON_AUDIT hit="+debugHit.collider.name+" floor="+(debugHit.collider.GetComponent<VillageControl>()?.Floor.ToString()??"none")+" inside="+lift.PlayerInside+" message="+g.Notice+" target="+roof.transform.position+" origin="+g.Player.View.transform.position);Check("aim_and_click_selects_without_menu",selected&&lift.Requested(7)&&!g.Paused);
            until=Time.unscaledTime+10;while(Time.unscaledTime<until&&lift.Floor!=7)yield return null;yield return new WaitForSecondsRealtime(.6f);Check("selected_roof_opens_for_passengers",lift.Floor==7&&Mathf.Abs(lift.CarY-92)<.02f&&lift.DoorOpen);yield return new WaitForSecondsRealtime(2);Check("roof_closes_after_disembarking",!lift.DoorOpen);
            g.SetPaused(true);float before=lift.CarY;yield return new WaitForSecondsRealtime(.5f);Check("pause_holds_queue_and_car",Mathf.Abs(before-lift.CarY)<.001f);g.SetPaused(false);
            VillageControl middle=null;foreach(var b in lift.GetComponentsInChildren<VillageControl>())if(b.Cabin&&b.Floor==4)middle=b;g.Player.Teleport(lift.CarCenter+Vector3.ProjectOnPlane(lift.CarCenter-middle.transform.position,Vector3.up).normalized*.5f);Vector3 toPanel=middle.transform.position-g.Player.View.transform.position;g.Player.Teleport(g.Player.transform.position,Mathf.Atan2(toPanel.x,toPanel.z)*Mathf.Rad2Deg,-Mathf.Atan2(toPanel.y,new Vector2(toPanel.x,toPanel.z).magnitude)*Mathf.Rad2Deg);yield return null;ScenePicture(g,Path.Combine(folder,"29-number-buttons.png"));g.Player.Teleport(new Vector3(33,96,5),-25,15);g.Player.SetFlying(true);yield return null;ScenePicture(g,Path.Combine(folder,"30-seven-floors-roof.png"));
            // Alternate combinations use the same public scheduler, not scripted floor routes.
            g.TravelTo(0);g.TravelTo(1);yield return null;foreach(var l in FindObjectsByType<SkyHouse>())if(Mathf.Abs(l.Anchor.z-13)<.1f&&l.Anchor.x<30)lift=l;
            lift.PlanResidentVisit(4);g.Player.Teleport(new Vector3(30,15,5));g.Player.SetFlying(true);until=Time.unscaledTime+45;while(Time.unscaledTime<until&&!lift.Requested(4))yield return null;
            Check("residents_can_choose_fifth",lift.Requested(4)&&lift.BoardedCount==4);g.Player.Teleport(lift.CarCenter);lift.RequestFloor(2);until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=2||!lift.DoorOpen))yield return null;
            Check("player_third_does_not_unload_fifth_passengers",lift.Floor==2&&lift.BoardedCount==4&&lift.ResidentsVisiting(2)==0);
            g.Player.Teleport(lift.StoreOrigin(2)+new Vector3(0,.2f,1));g.Player.SetFlying(true);
            until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=4||!lift.DoorOpen))yield return null;yield return new WaitForSecondsRealtime(2);
            Debug.Log("FIFTH_THIRD_AUDIT residents="+lift.ResidentsVisiting(4)+" playerY="+g.Player.transform.position.y+" stops="+string.Join(",",lift.StopHistory));Check("npc_fifth_player_third_only_requested_stops",lift.ResidentsVisiting(4)==4&&Mathf.Abs(g.Player.transform.position.y-SkyHouse.FloorHeight(2))<1&&lift.StopHistory.Count==2&&lift.StopHistory[0]==2&&lift.StopHistory[1]==4);
            g.TravelTo(0);g.TravelTo(1);yield return null;foreach(var l in FindObjectsByType<SkyHouse>())if(Mathf.Abs(l.Anchor.z-13)<.1f&&l.Anchor.x<30)lift=l;
            lift.PlanResidentVisit(SkyHouse.RoofFloor);g.Player.Teleport(new Vector3(30,15,5));g.Player.SetFlying(true);until=Time.unscaledTime+55;while(Time.unscaledTime<until&&lift.CarY<80)yield return null;
            g.Player.Teleport(lift.CarCenter);lift.RequestFloor(-1);until=Time.unscaledTime+15;while(Time.unscaledTime<until&&(lift.Floor!=7||!lift.DoorOpen))yield return null;yield return new WaitForSecondsRealtime(2);
            Check("npc_roof_destination_unloads",lift.ResidentsVisiting(7)==4&&lift.BoardedCount==0);g.Player.Teleport(g.Player.transform.position,90,0);yield return null;ScenePicture(g,Path.Combine(folder,"31-roof-passengers.png"));
            until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=-1||!lift.DoorOpen))yield return null;
            Check("player_bottom_npcs_remain_on_roof",lift.Floor==-1&&lift.PlayerInside&&lift.ResidentsVisiting(7)==4);Check("roof_bottom_no_unrequested_stops",lift.StopHistory.Count==2&&lift.StopHistory[0]==7&&lift.StopHistory[1]==-1);
            g.TravelTo(0);g.TravelTo(1);yield return null;foreach(var l in FindObjectsByType<SkyHouse>())if(Mathf.Abs(l.Anchor.z-13)<.1f&&l.Anchor.x<30)lift=l;g.Player.Teleport(new Vector3(30.5f,94,13));g.Player.SetFlying(true);g.MineBlock(new Vector3Int(16,17,15));until=Time.unscaledTime+40;while(Time.unscaledTime<until&&lift.Floor!=SkyHouse.RoofFloor)yield return null;yield return new WaitForSecondsRealtime(.6f);Check("family_waits_at_roof_with_closed_glass",lift.Floor==SkyHouse.RoofFloor&&!lift.DoorOpen&&lift.BoardedCount==4);g.TravelTo(0);g.Save();
        }
        IEnumerator FifthDestinationChecks(Sandbox g,string folder)
        {
            g.Respawn();g.TravelTo(1);yield return null;SkyHouse lift=null;foreach(var l in FindObjectsByType<SkyHouse>())if(Mathf.Abs(l.Anchor.z-13)<.1f&&l.Anchor.x<30)lift=l;
            lift.PlanResidentVisit(4);g.Player.Teleport(new Vector3(30,15,5));g.Player.SetFlying(true);
            float until=Time.unscaledTime+45;while(Time.unscaledTime<until&&!lift.Requested(4))yield return null;
            bool boarded=lift.BoardedCount==4&&lift.Requested(4);g.Player.Teleport(lift.CarCenter);lift.RequestFloor(2);
            until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=2||!lift.DoorOpen))yield return null;
            bool separate=lift.Floor==2&&lift.BoardedCount==4&&lift.ResidentsVisiting(2)==0;
            g.Player.Teleport(lift.StoreOrigin(2)+new Vector3(0,.2f,1));g.Player.SetFlying(true);
            until=Time.unscaledTime+20;while(Time.unscaledTime<until&&(lift.Floor!=4||!lift.DoorOpen))yield return null;yield return new WaitForSecondsRealtime(2);
            bool arrived=lift.ResidentsVisiting(4)==4&&Mathf.Abs(g.Player.transform.position.y-SkyHouse.FloorHeight(2))<1&&lift.StopHistory.Count==2&&lift.StopHistory[0]==2&&lift.StopHistory[1]==4;
            File.WriteAllLines(Path.Combine(folder,"fifth-checks.txt"),new[]{"npc_selects_fifth="+boarded,"player_leaves_at_third_npcs_stay_aboard="+separate,"npcs_fifth_player_third_only_selected_stops="+arrived,"player_y="+g.Player.transform.position.y,"stops="+string.Join(",",lift.StopHistory)});adventurePass&=boarded&&separate&&arrived;
        }
    }
}
