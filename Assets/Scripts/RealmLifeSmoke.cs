using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands { public partial class SmokeTest
{
    IEnumerator RealmLifeChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string name,bool ok){rows.Add(name+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"realm-life-checks.txt"),rows);}
        g.Respawn();g.RoyalIdentity=true;
        foreach(int region in new[]{0,1,2,4,6,11,12,13,14,15})
        {
            Check("travel_to_"+region,g.TravelTo(region));g.SetDaytime(false);
            g.Player.Teleport(new Vector3(32,Sandbox.HeavenFloor+2,22));yield return null;yield return null;
            Check("heaven_daylight_region_"+region,g.InHeaven&&g.ActiveSkyColor.r>.6f&&RenderSettings.ambientSkyColor.r>.5f);
            Check("manual_F_allowed_region_"+region,g.Player.CanUseFlightF);
            g.InspectHero();yield return null;Check("halo_and_wings_region_"+region,g.PortraitHasHalo&&g.PortraitHasWings);g.InspectHero();
        }
        g.Player.Teleport(new Vector3(32,-300,22));g.UpdateConnectedDepth();yield return new WaitForSecondsRealtime(2);
        Check("nothing_forces_flight_in_long_void",!g.Player.Flying&&!g.InVoidRealms);
        Check("realm_starts_hundreds_below_bedrock",VoxelWorld.MinY-Sandbox.RealmEntryY>=250&&VoxelWorld.MinY-Sandbox.HeavenFloor>=350);
        Check("planet_twice_realm_size",Sandbox.PlanetDiameter>=Sandbox.RealmDiameter*2&&Sandbox.PlanetLandDiameter>=Sandbox.RealmDiameter*2);
        g.Player.Teleport(new Vector3(32,Sandbox.HeavenFloor+6,22));yield return new WaitForSecondsRealtime(1);
        Check("heaven_has_actual_ground",g.Player.Body.isGrounded&&!g.Player.Flying);
        g.Player.Teleport(new Vector3(31,Sandbox.HeavenFloor+1.1f,31),0,-4);yield return null;
        ScenePicture(g,Path.Combine(folder,"heaven-neighbourhood.png"));
        RealmUsePoint door=null,food=null,bed=null,pool=null;
        foreach(var point in FindObjectsByType<RealmUsePoint>())
        {
            if(Mathf.Abs(point.transform.position.y-Sandbox.HeavenFloor)>10)continue;
            if(point.Kind==RealmUseKind.Door&&door==null)door=point;
            if(point.Kind==RealmUseKind.Food&&food==null)food=point;
            if(point.Kind==RealmUseKind.Bed&&bed==null)bed=point;
            if(point.Kind==RealmUseKind.Pool)pool=point;
        }
        Check("heaven_usable_home_contents",door!=null&&food!=null&&bed!=null&&pool!=null);
        g.Player.Teleport(door.Target.position+Vector3.back*2.5f);g.Player.View.transform.LookAt(door.transform.position);Physics.SyncTransforms();
        Check("real_house_door_opens_with_G",g.TryVoidRealmG()&&door.Active&&Quaternion.Angle(door.Target.localRotation,Quaternion.identity)>70);
        Transform home=door.Target.parent;g.Player.Teleport(home.TransformPoint(new Vector3(0,.5f,2)));yield return new WaitForSecondsRealtime(.5f);
        bool clear=true;foreach(var c in Physics.OverlapCapsule(g.Player.transform.position+Vector3.up*.35f,g.Player.transform.position+Vector3.up*1.6f,.2f,~(1<<2),QueryTriggerInteraction.Ignore))if(c!=g.Player.Body)clear=false;
        Check("house_interior_is_hollow_and_floored",g.Player.Body.isGrounded&&clear);
        g.Player.Teleport(home.TransformPoint(new Vector3(0,.5f,-3)),0,12);ScenePicture(g,Path.Combine(folder,"inside-heaven-home.png"));
        g.Player.Teleport(new Vector3(32,Sandbox.HeavenFloor+1.1f,22));int eaten=g.RealmMealsEaten;g.UseRealmPoint(food);
        Check("food_has_eating_action",g.RealmMealActive);
        yield return new WaitForSecondsRealtime(.6f);ScenePicture(g,Path.Combine(folder,"eating-food.png"));
        float deadline=Time.unscaledTime+8;while(g.RealmMealActive&&Time.unscaledTime<deadline)yield return null;
        Check("food_is_eaten_not_just_stored",g.RealmMealsEaten==eaten+1&&!g.RealmMealActive);
        g.UseRealmPoint(pool);Check("hot_spring_has_bath_action",g.RealmBathActive&&GameObject.Find("可泡的温泉水")!=null);
        yield return new WaitForSecondsRealtime(.4f);g.Player.View.transform.localRotation=Quaternion.Euler(22,0,0);ScenePicture(g,Path.Combine(folder,"actual-hot-spring.png"));
        deadline=Time.unscaledTime+15;while(g.RealmBathActive&&Time.unscaledTime<deadline)yield return null;
        Check("hot_spring_exits_without_flight",!g.RealmBathActive&&!g.Player.Flying&&g.Player.transform.position.z>17);
        g.UseRealmPoint(bed);Check("bed_can_be_used",g.VoidRideActive);g.TryVoidRealmG();Check("bed_can_be_left",!g.VoidRideActive);
        g.Player.Teleport(new Vector3(32,Sandbox.PlanetFloor+5,12));yield return new WaitForSecondsRealtime(1);
        Check("inhabited_planet_is_landable",g.InPlanetLife&&g.Player.Body.isGrounded&&!g.Player.Flying);
        g.Player.Teleport(new Vector3(100,Sandbox.PlanetFloor+3,100));yield return new WaitForSecondsRealtime(1);
        Check("planet_land_extends_beyond_old_64_square",g.Player.Body.isGrounded&&g.Player.transform.position.x>64);
        int people=0,planetHomes=0;RealmUsePoint craft=null,harvest=null;
        foreach(var resident in FindObjectsByType<RealmResidentLife>())if(resident.name.StartsWith("星球居民"))people++;
        foreach(var point in FindObjectsByType<RealmUsePoint>())if(Mathf.Abs(point.transform.position.y-Sandbox.PlanetFloor)<10){if(point.Kind==RealmUseKind.Door)planetHomes++;if(point.Kind==RealmUseKind.Craft)craft=point;if(point.Kind==RealmUseKind.Harvest)harvest=point;}
        Check("planet_has_people_homes_and_farms",people>=20&&planetHomes>=9&&craft!=null&&harvest!=null);
        int bread=g.Inventory[(int)Block.Bread];g.UseRealmPoint(harvest);Check("planet_farm_works",g.Inventory[(int)Block.Bread]==bread+1);
        g.UseRealmPoint(craft);Check("planet_nine_grid_crafting_works",g.Paused);g.SetPaused(false);
        g.Player.Teleport(new Vector3(31,Sandbox.PlanetFloor+2,5),0,-3);yield return null;ScenePicture(g,Path.Combine(folder,"inhabited-planet.png"));
        g.Player.Teleport(new Vector3(330,Sandbox.PlanetFloor+90,330));g.Player.SetFlying(true);g.Player.View.transform.LookAt(new Vector3(32,Sandbox.PlanetFloor-130,32));ScenePicture(g,Path.Combine(folder,"large-planet-overview.png"));g.Player.Teleport(new Vector3(31,Sandbox.PlanetFloor+2,5));
        g.Save();g.Load();yield return null;Check("planet_position_roundtrips",g.InPlanetLife&&g.Player.transform.position.y<Sandbox.PlanetFloor+5);
        g.Player.Teleport(new Vector3(92,Sandbox.HellFloor+6,32));yield return new WaitForSecondsRealtime(1);
        Check("hell_catches_planet_shaft_without_flight",g.InHell&&g.Player.Body.isGrounded&&!g.Player.Flying);
        var ghost=FindAnyObjectByType<RealmGhostLife>();Vector3 ghostBefore=ghost.transform.position;yield return new WaitForSecondsRealtime(1.1f);
        Check("hell_ghosts_actually_move",Vector3.Distance(ghostBefore,ghost.transform.position)>.1f);
        g.Inventory[(int)Block.Coin]=0;int lanterns=0;foreach(var point in FindObjectsByType<RealmUsePoint>())if(point.Kind==RealmUseKind.Lantern){g.UseRealmPoint(point);lanterns++;}
        Check("hell_has_playable_lantern_task",lanterns==3&&g.RealmLanternsLit==3&&g.Inventory[(int)Block.Coin]==30);
        g.Player.Teleport(new Vector3(46,Sandbox.HellFloor+1.1f,39),0,-6);yield return null;ScenePicture(g,Path.Combine(folder,"haunted-hell.png"));
        g.Save();g.Load();yield return null;Check("deep_hell_save_is_valid",g.InHell&&g.Player.transform.position.y<Sandbox.HellFloor+5);
        g.Player.Teleport(new Vector3(48,Sandbox.HeavenFloor-4,32));yield return null;
        deadline=Time.unscaledTime+22;while(g.Player.transform.position.y>Sandbox.PlanetFloor+4&&Time.unscaledTime<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.25f);Check("actual_heaven_drop_lands_on_inhabited_planet",g.InPlanetLife&&g.Player.Body.isGrounded&&!g.Player.Flying);
        g.Player.Teleport(new Vector3(92,Sandbox.PlanetFloor-4,32));yield return null;
        deadline=Time.unscaledTime+30;while(g.Player.transform.position.y>Sandbox.HellFloor+4&&Time.unscaledTime<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.25f);Check("actual_planet_shaft_lands_in_hell",g.InHell&&g.Player.Body.isGrounded&&!g.Player.Flying);
        g.Player.SetFlying(true);Check("manual_flight_still_available",g.Player.Flying);
        g.TravelTo(0);yield return null;Check("return_restores_original_surface_state",!g.InVoidRealms&&g.Player.View.farClipPlane==150);
    }
}}
