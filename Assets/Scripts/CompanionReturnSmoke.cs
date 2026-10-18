using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands {public partial class SmokeTest
{
    IEnumerator CompanionReturnChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string k,bool ok){rows.Add(k+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"companion-return-checks.txt"),rows);}
        g.Respawn();g.InviteForestFriend();g.Player.Teleport(g.ForestFriendPosition+Vector3.right);g.TryMeetFriendOrGift();g.RoyalIdentity=true;g.TravelTo(6);yield return null;
        var kids=new List<NeighbourSpeech>();foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.CastleEnvy)kids.Add(n);
        int first=kids[0].GetComponent<CastleGirlStyle>().StyleId,second=kids[1].GetComponent<CastleGirlStyle>().StyleId;
        g.OpenStickerFor(kids[0]);g.SubmitInvitationSticker("可以");g.OpenStickerFor(kids[1]);g.SubmitInvitationSticker("可以");yield return null;
        Check("two_invited_children_keep_forest_friend",g.InvitedChildCount==2&&g.ForestFriendFollowing);
        Check("companion_menu_pauses_safely",g.OpenCompanionMenu()&&g.CompanionMenuOpen&&g.Paused);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"companion-menu.png"));yield return new WaitForSecondsRealtime(.3f);
        Check("typed_chat_has_topic_reply",g.ChatWithChild(first,"我们一起玩球")&&g.ChildChatReply.Contains("传球"));
        Check("return_one_child_by_car",g.ReturnChildByCar(first)&&g.InvitedChildCount==1&&g.HomeRideCount==1&&g.ForestFriendFollowing);
        Check("same_child_cannot_be_sent_twice",!g.ReturnChildByCar(first)&&g.HomeRideCount==1);
        bool boarded=false;foreach(var marker in FindObjectsByType<InvitedChildFollower>())if(marker.StyleId==first)boarded=marker.transform.parent!=null&&marker.transform.parent.name.Contains("粉色小汽车");Check("child_visibly_rides_in_car",boarded);
        g.CloseCompanionMenu();Check("closing_menu_resumes_game",!g.CompanionMenuOpen&&!g.Paused);
        yield return new WaitForSecondsRealtime(.8f);var car=GameObject.Find("送小朋友回水晶城堡的粉色小汽车");if(car!=null)HomeCarPicture(g,car.transform,Path.Combine(folder,"child-riding-home.png"));
        yield return new WaitForSecondsRealtime(5.5f);Check("child_arrives_and_car_clears",g.HomeRideCount==0&&g.InvitedChildCount==1);
        NeighbourSpeech homeChild=null;foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.CastleEnvy&&n.GetComponent<CastleGirlStyle>().StyleId==first)homeChild=n;
        Check("original_child_is_back_in_castle",homeChild!=null&&homeChild.gameObject.activeInHierarchy);
        Check("can_chat_after_child_goes_home",g.ChatWithChild(first,"你好")&&g.ChildChatReply.Contains("你好"));
        Check("X_only_sends_small_he_not_children",g.SendForestFriendHome()&&g.InvitedChildStyles.Contains(second));yield return new WaitForSecondsRealtime(4.5f);
        g.InviteForestFriend();yield return new WaitForSecondsRealtime(8.2f);g.Player.Teleport(g.ForestFriendPosition+Vector3.right);g.TryMeetFriendOrGift();g.CloseInvitationSticker();
        g.OpenStickerFor(homeChild);Check("child_can_be_invited_again",g.SubmitInvitationSticker("可以")&&g.InvitedChildCount==2);
        Check("all_children_can_depart_without_small_he",g.ReturnAllChildren()==2&&g.InvitedChildCount==0&&g.ForestFriendFollowing);
        g.Save();g.Load();yield return null;Check("reload_mid_trip_returns_children_no_duplicate",g.HomeRideCount==0&&g.InvitedChildCount==0&&g.ForestFriendFollowing);
        int originals=0;foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.CastleEnvy)originals++;Check("all_seven_castle_children_restored",originals==7);
        NeighbourSpeech again=null;foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.CastleEnvy){again=n;break;}int awayId=again.GetComponent<CastleGirlStyle>().StyleId;g.OpenStickerFor(again);g.SubmitInvitationSticker("可以");g.TravelTo(0);yield return null;
        FestivalStation coaster=null;foreach(var s in g.FestivalStations)if(s.Kind==FestivalKind.Coaster)coaster=s;g.Player.Teleport(coaster.Centre);coaster.StartGame();coaster.SetCoasterGrip(true);
        Check("return_is_blocked_while_riding",g.CoasterRiding&&!g.ReturnChildByCar(awayId)&&g.InvitedChildCount==1);
        Check("companion_menu_does_not_interrupt_ride",!g.OpenCompanionMenu()&&!g.CompanionMenuOpen);g.LeaveCoasterWithJ();g.CloseInvitationSticker();
        int world=g.World.Region;Vector3 location=g.Player.transform.position;
        Check("child_can_return_from_other_region",g.ReturnChildByCar(awayId)&&g.World.Region==world&&Vector3.Distance(location,g.Player.transform.position)<.01f&&g.ForestFriendFollowing);
        g.TravelTo(6);yield return null;Check("travel_during_return_finishes_safely",g.HomeRideCount==0&&g.InvitedChildCount==0&&g.ForestFriendFollowing);
        originals=0;foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.CastleEnvy)originals++;Check("travel_does_not_duplicate_castle_children",originals==7);
        g.Player.Teleport(new Vector3(32,Sandbox.PlanetFloor+2,2));yield return null;
        NeighbourSpeech r0=null,r1=null;foreach(var n in FindObjectsByType<NeighbourSpeech>()){var c=n.GetComponent<PlanetCitizenAge>();if(c!=null&&c.CitizenId==0)r0=n;if(c!=null&&c.CitizenId==1)r1=n;}
        g.OpenStickerFor(r0);g.SubmitInvitationSticker("进天堂");g.OpenStickerFor(r1);g.SubmitInvitationSticker("进天堂");yield return new WaitForSecondsRealtime(3.3f);
        Check("two_heaven_visitors_ready",g.PlanetHeavenVisitors.Count==2);
        Vector3 player=g.Player.transform.position;bool flying=g.Player.Flying;
        Check("resident_departure_removes_only_selected_flag",g.ReturnResidentByCar(0)&&!g.PlanetHeavenVisitors.Contains(0)&&g.PlanetHeavenVisitors.Contains(1));
        Check("resident_uses_visible_car",r0.transform.parent!=null&&r0.transform.parent.name.Contains("云朵汽车"));
        HomeCarPicture(g,r0.transform.parent,Path.Combine(folder,"resident-riding-to-planet.png"));
        Check("resident_return_does_not_teleport_or_fly_player",g.Player.Flying==flying&&Vector3.Distance(player,g.Player.transform.position)<.01f);
        Check("cannot_repeat_resident_return",!g.ReturnResidentByCar(0));
        yield return new WaitForSecondsRealtime(6.3f);Check("resident_returns_to_own_planet_neighbourhood",r0.transform.position.y<Sandbox.PlanetFloor+3&&Vector3.Distance(r0.transform.position,g.PlanetCitizenHome(0))<3&&r0.GetComponent<RealmResidentLife>().enabled);
        g.CloseInvitationSticker();g.OpenStickerFor(r0);Check("returned_resident_can_visit_heaven_again",g.SubmitInvitationSticker("进天堂"));
        Check("return_cancels_pending_upward_cloud_trip",g.ReturnResidentByCar(0)&&!g.PlanetHeavenVisitors.Contains(0));
        Check("return_all_remaining_residents",g.ReturnAllResidents()==1&&g.PlanetHeavenVisitors.Count==0&&g.ForestFriendFollowing);
        g.Save();g.Load();yield return null;int citizens=0;bool allHome=true;foreach(var c in FindObjectsByType<PlanetCitizenAge>()){citizens++;allHome&=c.transform.position.y<Sandbox.PlanetFloor+4;}Check("resident_returns_survive_reload_without_loss",citizens==25&&allHome&&g.PlanetHeavenVisitors.Count==0&&g.HomeRideCount==0);
        g.TravelTo(0);yield return null;Check("friend_X_still_operates_after_returns",g.SendForestFriendHome()&&g.InvitedChildCount==0&&g.PlanetHeavenVisitors.Count==0);
    }
    void HomeCarPicture(Sandbox g,Transform car,string path)
    {
        var eye=g.Player.View.transform;Vector3 pos=eye.position;Quaternion rot=eye.rotation;int mask=g.Player.View.cullingMask;g.Player.HomeRest(true);
        var layers=new Dictionary<Transform,int>();foreach(var t in car.GetComponentsInChildren<Transform>()){layers[t]=t.gameObject.layer;t.gameObject.layer=2;}
        g.Player.View.cullingMask=1<<2;eye.position=car.position+car.rotation*new Vector3(4.7f,2.8f,4.8f);eye.LookAt(car.position+Vector3.up*1.1f);ScenePicture(g,path);
        foreach(var pair in layers)pair.Key.gameObject.layer=pair.Value;g.Player.View.cullingMask=mask;
        g.Player.HomeRest(false);eye.position=pos;eye.rotation=rot;
    }
}}
