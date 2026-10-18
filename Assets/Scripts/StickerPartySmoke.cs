using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands {public partial class SmokeTest
{
 IEnumerator StickerPartyChecks(Sandbox g,string folder)
 {
  var rows=new List<string>();void Check(string k,bool ok){rows.Add(k+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"sticker-party-checks.txt"),rows);}
  g.Respawn();g.InviteForestFriend();g.Player.Teleport(g.ForestFriendPosition+Vector3.right);Check("small_he_friend_still_joins",g.TryMeetFriendOrGift()&&g.ForestFriendFollowing);
  g.RoyalIdentity=true;g.TravelTo(6);yield return null;
  var children=new List<NeighbourSpeech>();foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.CastleEnvy)children.Add(n);
  g.Player.Teleport(children[0].transform.position+Vector3.back);float popupDeadline=Time.unscaledTime+14;while(!g.HasInvitationRequest&&Time.unscaledTime<popupDeadline)yield return null;Check("spoken_request_waits_for_player_permission",g.HasInvitationRequest&&!g.StickerOpen);Check("player_explicitly_opens_sticker",g.OpenInvitationRequest()&&g.StickerOpen);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"yellow-answer-bar.png"));yield return new WaitForSecondsRealtime(.6f);g.CloseInvitationSticker();
  float hp=g.Health;g.OpenStickerFor(children[0]);Check("sticker_opens",g.StickerOpen);Check("unknown_text_is_not_a_command",!g.SubmitInvitationSticker("你好")&&g.StickerOpen);
  Check("a_question_is_not_misread_as_permission",!g.SubmitInvitationSticker("可以吗")&&g.StickerOpen);
  Check("no_invitation_is_safe",g.SubmitInvitationSticker("不可以")&&g.InvitedChildCount==0&&children[0].gameObject.activeSelf&&g.Health==hp);
  g.OpenStickerFor(children[1]);Check("yes_adds_child_without_replacing_friend",g.SubmitInvitationSticker("可以")&&g.InvitedChildCount==1&&g.ForestFriendFollowing);
  g.OpenStickerFor(children[2]);g.SubmitInvitationSticker("一起进去");yield return new WaitForSecondsRealtime(.3f);
  Check("multiple_children_and_friend_form_party",g.InvitedChildCount==2&&g.PartyMembers().Count==3&&g.ForestFriendFollowing);
  Check("hands_are_meshes_not_tether_lines",g.PartyHoldingHands&&FindObjectsByType<JoinedHandsRig>().Length>=3);
  bool noLines=true;foreach(var rig in FindObjectsByType<JoinedHandsRig>())noLines&=rig.GetComponentsInChildren<LineRenderer>().Length==0;Check("no_guardrail_line_in_hand_models",noLines);
  g.InspectHero();yield return null;ScenePicture(g,Path.Combine(folder,"party-holding-hands.png"));g.InspectHero();
  g.Save();g.Load();yield return null;Check("party_survives_reload_with_small_he",g.InvitedChildCount==2&&g.ForestFriendFollowing&&g.PartyMembers().Count==3);
  g.Player.Teleport(CastleBallPlay.Court);Check("party_can_play_ball_together",g.TryEntertainmentG()&&g.PartyPlayingBall);g.CloseInvitationSticker();
  g.TravelTo(0);yield return null;FestivalStation ride=null;foreach(var s in g.FestivalStations)if(s.Kind==FestivalKind.Coaster)ride=s;
  g.Player.Teleport(ride.Centre);ride.StartGame();ride.SetCoasterGrip(true);ride.Tick(5);yield return null;
  Check("companions_board_without_dismissing_small_he",g.CoasterRiding&&g.ForestFriendFollowing&&g.PartyMembers().Count==3);
  g.UpdatePartyHandPose();ride.HeroGripTargets(out var left,out var right);var own=GameObject.Find("主角真正伸出的双手");Check("player_palms_touch_yellow_bar",own!=null&&Vector3.Distance(own.transform.Find("左手握姿").position,left)<.04f&&Vector3.Distance(own.transform.Find("右手握姿").position,right)<.04f);
  g.Player.View.transform.LookAt((left+right)*.5f);ScenePicture(g,Path.Combine(folder,"gripping-yellow-bar.png"));
  var members=g.PartyMembers();g.Player.View.transform.LookAt(members[1].position+Vector3.up);ScenePicture(g,Path.Combine(folder,"companions-on-coaster.png"));
  for(int i=0;i<40000&&ride.Running;i++)ride.Tick(1f/60);
  Check("holding_space_completes_exactly_two_laps",ride.Won&&ride.CoasterCompletedLaps==2&&!g.CoasterRiding&&g.ForestFriendFollowing&&g.InvitedChildCount==2);
  yield return new WaitForSeconds(2.3f);g.Player.Teleport(ride.Centre);ride.StartGame();ride.SetCoasterGrip(false);hp=g.Health;ride.Tick(3);
  Check("letting_go_causes_only_non_graphic_minor_slip",!ride.Running&&g.CoasterSlipActive&&g.Health==hp-.5f&&g.ForestFriendFollowing&&g.InvitedChildCount==2);
  yield return new WaitForSecondsRealtime(1.8f);g.Player.Teleport(ride.Centre);ride.StartGame();ride.SetCoasterGrip(true);Check("J_still_exits_early",g.LeaveCoasterWithJ()&&!ride.Running&&g.Player.Body.enabled);
  g.Player.Teleport(new Vector3(32,Sandbox.PlanetFloor+2,2));yield return null;NeighbourSpeech resident=null,stay=null;
  foreach(var n in FindObjectsByType<NeighbourSpeech>())if(n.Kind==NeighbourSpeechKind.PlanetWish){var age=n.GetComponent<PlanetCitizenAge>();if(age.CitizenId==0)resident=n;if(age.CitizenId==1)stay=n;}
  g.OpenStickerFor(stay);Check("planet_no_keeps_resident_here",g.SubmitInvitationSticker("不进天堂")&&!g.PlanetHeavenVisitors.Contains(1));
  g.OpenStickerFor(resident);Check("planet_yes_grants_own_trip_not_player_flight",g.SubmitInvitationSticker("进天堂")&&g.PlanetHeavenVisitors.Contains(0)&&!g.Player.Flying);
  yield return new WaitForSecondsRealtime(3.3f);Check("resident_arrives_in_heaven",resident.transform.position.y>Sandbox.HeavenFloor&&resident.transform.position.y<Sandbox.HeavenFloor+4&&!g.Player.Flying);
  g.Save();g.Load();yield return null;bool restored=false;foreach(var age in FindObjectsByType<PlanetCitizenAge>())if(age.CitizenId==0)restored=age.transform.position.y>Sandbox.HeavenFloor;
  Check("heaven_visit_and_party_both_persist",restored&&g.PlanetHeavenVisitors.Contains(0)&&g.InvitedChildCount==2&&g.ForestFriendFollowing);
 }
}}
