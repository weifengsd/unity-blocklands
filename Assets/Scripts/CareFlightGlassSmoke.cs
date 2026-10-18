using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands {public partial class SmokeTest
{
    IEnumerator CareFlightGlassChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string k,bool ok){rows.Add(k+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"care-flight-glass-checks.txt"),rows);}
        g.Respawn();g.Player.Teleport(new Vector3(75,19,68));Check("summon_car",g.SummonCar());g.Player.Teleport(g.CarPosition+Vector3.right*3);Check("enter_car",g.EnterCar());Check("camper_flight",g.SetCamper(true)&&g.ToggleCarFlight());
        for(int i=0;i<240;i++)g.LiftCar(1,.1f);Check("flies_above_old_200m_limit",g.CarPosition.y>250);
        Vector3 parked=g.CarPosition;g.ExitCar();g.Player.Teleport(parked+Vector3.right*3);Check("reenter_parked_high_camper",g.EnterCar());float y=g.CarPosition.y;g.LiftCar(-1,.1f);Check("reentered_camper_can_descend",g.CarPosition.y<y);
        g.Player.CoasterLook(0,-60);Vector3 start=g.CarPosition;for(int i=0;i<15;i++)g.DriveCar(Vector2.up,.1f);Check("look_down_and_forward_dives",g.CarPosition.y<start.y-3&&g.CarPosition.z>start.z);
        Check("zero_starts_controlled_landing_not_stranding",g.ToggleCarFlight()&&g.CarFlying&&g.CarLanding);y=g.CarPosition.y;g.LiftCar(-1,.1f);Check("landing_keeps_descent_enabled",g.CarPosition.y<y);Check("zero_cancels_landing",g.ToggleCarFlight()&&g.CarFlying&&!g.CarLanding);
        g.Save();g.Load();g.SetPaused(false);yield return null;g.Player.Teleport(g.CarPosition+Vector3.right*3);Check("saved_airborne_camper_can_be_reentered",g.EnterCar());y=g.CarPosition.y;g.LiftCar(-1,.1f);Check("saved_airborne_camper_can_descend",g.CarPosition.y<y);g.ExitCar();
        g.TravelTo(1);yield return null;Check("create_base",g.CreateCompleteSecretBase());var h=g.FindSecretHome();Check("confirmed_old_house_is_transparent",h!=null&&h.legacy&&h.transparentHouseVersion==1);
        if(h==null)yield break;h.safeMoney=321;h.storedItems[(int)Block.Apple]=9;int planks=h.storedItems[(int)Block.Planks];Check("old_wall_voxels_removed",g.World.Get(new Vector3Int(28,18,53))==Block.Air&&g.World.Get(new Vector3Int(32,22,55))==Block.Air);
        Check("wood_and_roof_recovered",planks>0&&h.storedItems[(int)Block.Brick]>0);g.BuildTransparentLegacyHomes();Check("conversion_is_idempotent",h.storedItems[(int)Block.Planks]==planks);
        bool glass=false;foreach(var r in FindObjectsByType<Renderer>())if(r.name=="可透视玻璃侧墙")glass|=r.sharedMaterial.shader.name=="Blocklands/GlassPane"&&r.sharedMaterial.color.a<.3f;Check("glass_material_is_genuinely_transparent",glass);
        Check("basement_and_valuables_preserved",h.secretRoomVersion==1&&h.safeMoney==321&&h.storedItems[(int)Block.Apple]==9&&g.RoomFixture(h,SecretFixtureKind.Safe)!=null&&g.RoomFixture(h,SecretFixtureKind.Monitor)!=null);g.Save();g.Load();g.SetPaused(false);yield return null;h=g.FindSecretHome();Check("glass_conversion_survives_reload_without_double_refund",h.transparentHouseVersion==1&&h.storedItems[(int)Block.Planks]==planks&&h.safeMoney==321);
        g.Player.Teleport(new Vector3(33,18,44));g.Player.SetFlying(true);g.Player.View.transform.LookAt(new Vector3(33,18,55));ScenePicture(g,Path.Combine(folder,"transparent-house.png"));
        g.Player.Teleport(new Vector3(75,19,68));g.SummonCar();g.Player.Teleport(g.CarPosition+Vector3.right*3);g.EnterCar();g.ToggleCarFlight();g.LiftCar(1,.1f);
        g.ApplyFloodImpact(96,110);int down=g.FloodDownCount;Check("whole_village_needs_rescue",down>=30);g.Theft.Add(new SavedBlock{x=9,y=16,z=8,id=(int)Block.Planks});
        float began=Time.realtimeSinceStartup;while(g.FloodDownCount>0&&Time.realtimeSinceStartup-began<120)yield return null;
        rows.Add("rescue_real_seconds="+(Time.realtimeSinceStartup-began).ToString("F2"));rows.Add("patients="+down);File.WriteAllLines(Path.Combine(folder,"care-flight-glass-checks.txt"),rows);
        Check("all_patients_recovered_within_120_real_seconds",g.FloodDownCount==0);Check("medical_team_has_parallel_nurses",FindObjectsByType<FloodMedic>().Length==Sandbox.MedicalTeamSize);Check("rescue_keeps_running_while_player_is_in_car",g.CarRiding&&g.FloodRescuedCount>=down);Check("unreturned_theft_restarts_chase_after_revival",g.VillageAngryUntil>Time.time);
        g.ExitCar();g.Player.Teleport(new Vector3(75,40,68));g.Player.SetFlying(true);g.Inventory[90]=1;g.SelectItem(90);Villager target=null;foreach(var v in FindObjectsByType<Villager>())if(!v.IsFriend&&v.ResidentId==0)target=v;
        Check("alloy_sword_can_down_recovered_resident",target!=null&&g.HitWithAlloy(target.transform)&&SwordDownedNpc.IsDown(target.transform));began=Time.realtimeSinceStartup;while(target!=null&&FloodResident.IsDown(target.transform)&&Time.realtimeSinceStartup-began<30)yield return null;
        Check("nurse_recovers_sword_down_without_region_reentry",target!=null&&!FloodResident.IsDown(target.transform)&&target.GetComponent<Collider>().enabled);Check("revival_clears_persistent_combat_down_state",target!=null&&!g.NpcCombatStates.Exists(s=>s.region==1&&s.key==g.CombatNpcKey(target.transform)&&s.down));
        Check("same_resident_can_be_downed_again",g.HitWithAlloy(target.transform));began=Time.realtimeSinceStartup;while(FloodResident.IsDown(target.transform)&&Time.realtimeSinceStartup-began<30)yield return null;Check("nurse_recovers_same_resident_second_time",!FloodResident.IsDown(target.transform));g.Save();g.SetPaused(true);
    }
}}
