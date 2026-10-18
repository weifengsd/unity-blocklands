using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands { public partial class SmokeTest
{
    IEnumerator CastleReplayChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string name,bool ok){rows.Add(name+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"castle-replay-checks.txt"),rows);}
        g.Respawn();g.RoyalIdentity=true;
        Check("1_outside_castle_does_not_reset",!g.HandleCastleReplayKey(KeyCode.Alpha1)&&g.RoyalIdentity);
        g.TravelTo(6);yield return null;var castle=FindAnyObjectByType<CastleSystem>();
        Check("former_princess_absent_before_request",FindObjectsByType<CrownChallenge>().Length==0);
        g.Inventory[(int)Block.Coin]=500;g.Inventory[(int)Block.Bread]=12;g.TransferPalaceMoney(100);
        int wallet=g.Inventory[(int)Block.Coin],savings=g.PalaceSavings;var build=new Vector3Int(60,40,60);g.World.Edit(build,Block.Planks);
        g.Player.Teleport(castle.Lift.Position+Vector3.up*.2f);castle.ToggleElevator();
        Check("elevator_1_is_not_replay",castle.ElevatorMenu&&!g.HandleCastleReplayKey(KeyCode.Alpha1)&&g.RoyalIdentity);
        Check("elevator_still_accepts_first_floor",castle.SelectElevatorFloor(0));
        g.Player.Teleport(new Vector3(31.5f,70,30));
        Check("1_explicitly_reopens_challenge",g.HandleCastleReplayKey(KeyCode.Alpha1)&&!g.RoyalIdentity);
        yield return null;castle=FindAnyObjectByType<CastleSystem>();
        Check("exactly_one_original_princess_returns",FindObjectsByType<CrownChallenge>().Length==1);
        Check("ordinary_player_is_outside_without_permit",g.Player.transform.position.z<14&&!castle.PalaceGate.Open&&castle.VisitorUntil<=Time.time);
        Check("money_inventory_and_building_preserved",g.Inventory[(int)Block.Coin]==wallet&&g.Inventory[(int)Block.Bread]==12&&g.PalaceSavings==savings&&g.World.Get(build)==Block.Planks);
        g.HandlePortraitKey(KeyCode.C);yield return null;Check("C_shows_ordinary_player_without_crown",g.InspectingHero&&!g.PortraitHasCrown);g.HandlePortraitKey(KeyCode.C);
        Check("J_permission_is_required_again",g.RequestVisitWithJ()&&castle.PalaceGate.Open&&castle.VisitorUntil>Time.time);
        var duel=FindAnyObjectByType<CrownChallenge>();g.Player.Teleport(duel.transform.position+Vector3.back*4);
        Check("returned_princess_can_be_challenged",duel.StartDuel());
        yield return new WaitForSecondsRealtime(1.8f);for(int i=0;i<30;i++){duel.Hit(1);yield return new WaitForSecondsRealtime(.17f);}
        yield return null;Check("victory_restores_player_and_removes_original",g.RoyalIdentity&&FindObjectsByType<CrownChallenge>().Length==0);
        float until=Time.unscaledTime+28;while(g.RoyalTransportActive&&Time.unscaledTime<until)yield return null;
        Check("normal_hospital_sequence_still_completes",!g.RoyalTransportActive&&g.Health==10);
        g.HandlePortraitKey(KeyCode.C);yield return null;Check("C_shows_player_crown_after_victory",g.PortraitHasCrown);g.HandlePortraitKey(KeyCode.C);
        g.TravelTo(0);g.TravelTo(6);yield return null;g.Save();g.Load();yield return null;
        Check("former_princess_stays_gone_on_revisit_and_reload",g.RoyalIdentity&&FindObjectsByType<CrownChallenge>().Length==0);
        g.Player.Teleport(new Vector3(31.5f,16,10));Check("1_can_request_another_round",g.HandleCastleReplayKey(KeyCode.Alpha1));yield return null;
        Check("second_explicit_return_is_one_princess",!g.RoyalIdentity&&FindObjectsByType<CrownChallenge>().Length==1);
        var original=FindAnyObjectByType<CrownChallenge>();g.HandleCastleReplayKey(KeyCode.Alpha1);yield return null;
        Check("repeated_1_does_not_duplicate_or_restart_open_round",FindObjectsByType<CrownChallenge>().Length==1&&FindAnyObjectByType<CrownChallenge>()==original);
        g.Save();g.Load();yield return null;Check("ordinary_replay_state_is_saved",!g.RoyalIdentity&&FindObjectsByType<CrownChallenge>().Length==1);
        var colors=new HashSet<Color>();var defaults=new HashSet<CastleGirlMood>();bool crownFree=true,bows=true;int girls=0;
        foreach(var girl in FindObjectsByType<CastleGirlStyle>()){girls++;colors.Add(girl.DressColor);defaults.Add(girl.DefaultMood);crownFree&=girl.transform.Find("皇冠")==null;bows&=girl.transform.Find("蝴蝶结中心")!=null;}
        Check("castle_playmates_have_seven_distinct_styles",girls==7&&colors.Count==7&&defaults.Count==7);
        Check("playmates_use_bows_not_player_crown",crownFree&&bows);
        var ball=FindAnyObjectByType<CastleBallPlay>();var ballObject=GameObject.Find("来回传递的彩色球");Vector3 before=ballObject.transform.position;
        yield return new WaitForSecondsRealtime(.6f);Check("friends_actually_pass_the_ball",Vector3.Distance(before,ballObject.transform.position)>.1f);
        g.Player.Teleport(CastleBallPlay.Court);int money=g.Inventory[(int)Block.Coin];bool passes=true;
        for(int i=0;i<3;i++){bool used=g.TryEntertainmentG();passes&=used;Check("G_ball_press_"+i,used);yield return new WaitForSecondsRealtime(.9f);}
        Check("flat_court_does_not_lift_player",g.Player.transform.position.y<16.1f);
        Check("G_joins_ball_game_and_awards_18",passes&&g.Inventory[(int)Block.Coin]==money+18);
        g.Player.Teleport(new Vector3(31,17,-1),0,10);yield return null;ScenePicture(g,Path.Combine(folder,"castle-friends-playing.png"));
        g.SetPaused(true);var studio=new GameObject("城堡女孩款式对照").transform;Vector3 center=new Vector3(1000,1000,1000);
        for(int i=0;i<7;i++){var a=CastleGirlStyle.Make(studio,i);a.transform.position=center+Vector3.right*(i-3)*1.05f;foreach(var t in a.GetComponentsInChildren<Transform>())t.gameObject.layer=2;}
        int mask=g.Player.View.cullingMask;g.Player.HomeRest(true);g.Player.View.cullingMask=1<<2;g.Player.View.transform.position=center+new Vector3(0,1,6.5f);g.Player.View.transform.LookAt(center+Vector3.up*.8f);
        ScenePicture(g,Path.Combine(folder,"seven-different-girls.png"));Destroy(studio.gameObject);g.Player.View.cullingMask=mask;g.Player.HomeRest(false);g.Player.View.transform.localPosition=Vector3.up*1.65f;g.Player.View.transform.localRotation=Quaternion.identity;g.SetPaused(false);
    }
}}
