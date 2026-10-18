using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands {
 public partial class SmokeTest {
  // Exercised through -blocklands-smoke's isolated save directory only.
  IEnumerator ActivityRegressionChecks(Sandbox g,string folder){
   var rows=new List<string>();
   void Check(string name,bool passed){rows.Add(name+"="+passed);adventurePass&=passed;File.WriteAllLines(Path.Combine(folder,"activity-regression-checks.txt"),rows);}
   FestivalStation Find(FestivalKind kind,bool surface){foreach(var s in g.FestivalStations)if(s!=null&&s.Kind==kind&&(s.Centre.y>=0)==surface)return s;return null;}
   int GuardianCount(FestivalStation station){int count=0;foreach(Transform child in station.transform)if(child.name=="足迹守护者笑脸")count++;return count;}

   g.Respawn();g.TravelTo(1);yield return null;
   var memory=Find(FestivalKind.Memory,true);
   Check("village_memory_exists",memory!=null);if(memory==null)yield break;
   g.Inventory[(int)Block.Coin]=100;g.Inventory[(int)Block.Cookie]=20;
   g.Player.Teleport(memory.Centre,0,10);
   g.SetPaused(true);memory.StartGame();Check("paused_activity_cannot_start",!memory.Running&&!g.FestivalG());g.SetPaused(false);
   Check("G_starts_village_memory",g.FestivalG()&&memory.Running&&g.FestivalLocked);
   memory.Direction(memory.ExpectedDirection);Check("memory_preview_cannot_be_skipped",memory.Progress==0);
   memory.Tick(1);memory.StartGame();Check("duplicate_start_does_not_restart_round",memory.Elapsed>=1&&memory.Progress==0);
   memory.Tick(4.1f);
   memory.Direction((memory.ExpectedDirection+1)%4);Check("wrong_memory_answer_does_not_advance",memory.Progress==0);
   var firstSequence=new List<int>();firstSequence.Add(memory.ExpectedDirection);memory.Direction(memory.ExpectedDirection);
   int beforePause=memory.Progress;float pausedClock=memory.Elapsed;
   g.SetPaused(true);memory.Direction(memory.ExpectedDirection);memory.Tick(20);
   Check("pause_blocks_direction_and_clock",memory.Progress==beforePause&&memory.Elapsed==pausedClock);g.SetPaused(false);
   // Distinct taps 50ms apart used to be silently discarded by the 350ms cooldown.
   for(int i=0;i<4;i++){memory.Tick(.05f);firstSequence.Add(memory.ExpectedDirection);memory.Direction(memory.ExpectedDirection);}
   Check("rapid_correct_memory_final_tap_finishes",memory.Won&&!memory.Running&&memory.Progress==5);
   Check("memory_win_releases_player",!g.FestivalLocked&&g.Player.Body.enabled&&Mathf.Abs(g.Player.View.transform.localPosition.y-1.65f)<.001f);
   Check("first_win_rewards_35",g.Inventory[(int)Block.Coin]==135&&g.Inventory[(int)Block.Cookie]==23);
   ScenePicture(g,Path.Combine(folder,"memory-completed.png"));

   Check("G_replays_completed_memory",g.FestivalG()&&memory.Running&&!memory.Won&&memory.Progress==0);
   memory.Tick(5.1f);bool different=false;
   for(int i=0;i<5;i++){different|=memory.ExpectedDirection!=firstSequence[i];memory.Direction(memory.ExpectedDirection);memory.Tick(.05f);}
   Check("memory_replay_uses_new_sequence",different&&memory.Won);
   Check("replay_earns_another_35",g.Inventory[(int)Block.Coin]==170&&g.Inventory[(int)Block.Cookie]==23);
   Check("replay_notice_is_honest",g.Notice.Contains("35")&&!g.Notice.Contains("首胜："));
   memory.StartGame();Check("G_cancels_memory",g.FestivalG()&&!memory.Running&&!g.FestivalLocked);

   g.TravelTo(2);yield return null;
   var dance=Find(FestivalKind.Dance,true);Check("surface_dance_exists",dance!=null);if(dance==null)yield break;
   g.Player.Teleport(dance.Centre);Check("G_starts_dance",g.FestivalG()&&dance.Running);
   dance.Direction(dance.ExpectedDirection);dance.Direction((dance.ExpectedDirection+1)%4);
   Check("wrong_dance_tap_preserves_completed_steps",dance.Progress==1);
   for(int i=0;i<7;i++){dance.Tick(.05f);dance.Direction(dance.ExpectedDirection);}
   Check("rapid_correct_dance_final_tap_finishes",dance.Won&&dance.Progress==8&&!g.FestivalLocked);
   ScenePicture(g,Path.Combine(folder,"dance-completed.png"));

   g.Respawn();yield return null;
   var dig=Find(FestivalKind.Dig,false);var coaster=Find(FestivalKind.Coaster,true);
   Check("dig_and_coaster_exist",dig!=null&&coaster!=null);if(dig==null||coaster==null)yield break;
   g.Player.Teleport(dig.Centre);Check("G_starts_dig",g.FestivalG()&&dig.Running);
   g.Player.Teleport(coaster.Centre);
   Check("remote_dig_does_not_steal_other_station_G",g.FestivalG()&&coaster.Running&&!dig.Running);
   coaster.Cancel();
   g.Player.Teleport(dig.Centre);dig.StartGame();Check("G_at_dig_board_cancels",g.FestivalG()&&!dig.Running);
   var digTarget=dig.transform.Find("探索目标 1");Check("dig_target_exists",digTarget!=null);if(digTarget==null)yield break;Vector3 initialScale=digTarget.localScale;dig.StartGame();
   for(int i=0;i<3;i++){g.Player.Teleport(dig.Target(i));dig.Interact();}
   Check("dig_all_targets_complete",dig.Won&&dig.Progress==3);
   g.Player.Teleport(dig.Centre);dig.StartGame();
   // BeginPolish pulses the current target, so compare the reset after cancelling.
   dig.Cancel();Check("dig_replay_does_not_accumulate_shrink",Vector3.Distance(digTarget.localScale,initialScale)<.001f);

   var sailing=Find(FestivalKind.Sailing,false);Check("sailing_exists",sailing!=null);if(sailing==null)yield break;
   g.SeaBoat.Board();float seatedHeight=g.Player.View.transform.localPosition.y;sailing.StartGame();
   for(int i=0;i<6;i++){g.Player.Carry(sailing.Target(i)-g.Player.transform.position);sailing.Tick(.01f);}
   Check("sailing_completes_without_altering_boat_seat",sailing.Won&&g.DepthBoatRiding&&Mathf.Abs(g.Player.View.transform.localPosition.y-seatedHeight)<.001f);
   g.SeaBoat.JumpOff();

   g.TravelTo(4);yield return null;
   var trail=Find(FestivalKind.Footprints,true);Check("footprint_trail_exists",trail!=null);if(trail==null)yield break;
   for(int round=0;round<2;round++){
    g.Player.Teleport(trail.Centre);trail.StartGame();
    for(int i=0;i<6;i++){g.Player.Teleport(trail.Target(i));trail.Tick(.01f);}
    Check("footprint_round_"+(round+1)+"_completes",trail.Won);
   }
   Check("footprint_replay_does_not_duplicate_guardian",GuardianCount(trail)==1);
   g.Player.Teleport(trail.Centre);trail.StartGame();
   for(int i=0;i<5;i++){g.Player.Teleport(trail.Target(i));trail.Tick(.01f);}
   trail.Tick(89.95f-trail.Elapsed);g.Player.Teleport(trail.Target(5));trail.Tick(.1f);
   Check("deadline_cannot_award_win_then_show_timeout",!trail.Won&&!trail.Running&&trail.Progress==5&&g.Notice.Contains("时间到了"));

   g.RoyalIdentity=true;g.TravelTo(6);yield return null;
   var castle=FindAnyObjectByType<CastleSystem>();var museumMemory=Find(FestivalKind.Memory,true);
   Check("castle_menu_test_fixtures_exist",castle!=null&&museumMemory!=null);if(castle==null||museumMemory==null)yield break;
   g.Player.Teleport(castle.Lift.Position);castle.ToggleElevator();
   Check("lift_menu_is_unpaused",g.RoyalMenuOpen&&!g.Paused);
   g.Player.Teleport(museumMemory.Centre);museumMemory.StartGame();
   Check("lift_menu_blocks_activity_G_and_direct_start",!g.FestivalG()&&!museumMemory.Running);
   g.Player.Teleport(castle.Lift.Position);castle.ToggleElevator();
   Check("closing_menu_restores_activity_eligibility",!g.RoyalMenuOpen&&g.FestivalInputAvailable);
  }
 }
}
