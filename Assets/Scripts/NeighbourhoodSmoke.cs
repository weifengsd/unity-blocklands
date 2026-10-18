using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands { public partial class SmokeTest
{
    IEnumerator NeighbourhoodChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string label,bool ok){rows.Add(label+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"neighbourhood-checks.txt"),rows);}
        g.Respawn();g.RoyalIdentity=false;g.TravelTo(6);yield return null;
        Check("Chinese_voice_assets_bundled",Resources.Load<AudioClip>("CastleChildEnvy")!=null&&Resources.Load<AudioClip>("PlanetResidentWish")!=null&&Resources.Load<AudioClip>("PlanetElderWish")!=null);
        NeighbourSpeech child=null;int castleSpeakers=0;
        foreach(var speaker in FindObjectsByType<NeighbourSpeech>())if(speaker.Kind==NeighbourSpeechKind.CastleEnvy){castleSpeakers++;if(child==null)child=speaker;}
        Check("all_castle_playmates_can_speak",castleSpeakers==7&&child!=null&&child.VoiceAvailable);
        g.Player.Teleport(child.transform.position+Vector3.back);yield return new WaitForSecondsRealtime(1);
        Check("ordinary_visitors_are_not_called_princess",child.PlayCount==0);
        g.RoyalIdentity=true;float deadline=Time.unscaledTime+12;while(child.PlayCount==0&&Time.unscaledTime<deadline)yield return null;
        Check("approaching_as_princess_triggers_voice",child.PlayCount==1&&child.Line.Contains("羡慕这位公主"));
        Check("castle_voice_has_subtitle",child.GetComponentInChildren<TextMesh>().text.Contains("水晶城堡"));
        yield return new WaitForSecondsRealtime(1.5f);Check("castle_line_does_not_restart_every_frame",child.PlayCount==1);
        g.Player.Teleport(child.transform.position+new Vector3(0,1,-3));g.Player.View.transform.LookAt(child.transform.position+Vector3.up);ScenePicture(g,Path.Combine(folder,"castle-child-talking.png"));
        g.TravelTo(0);g.Player.Teleport(new Vector3(32,Sandbox.PlanetFloor+2,2));yield return null;
        int children=0,elders=0,adults=0;bool blockHeads=true;
        foreach(var age in FindObjectsByType<PlanetCitizenAge>()){if(age.Age==PlanetAge.Child)children++;else if(age.Age==PlanetAge.Elder)elders++;else adults++;var head=age.transform.Find("head");blockHeads&=head!=null&&head.GetComponent<MeshFilter>().sharedMesh.name.Contains("Cube");}
        Check("planet_uses_village_style_block_people",blockHeads&&children>=5&&elders>=5&&adults>=12);
        NeighbourSpeech resident=null;foreach(var speaker in FindObjectsByType<NeighbourSpeech>())if(speaker.Kind==NeighbourSpeechKind.PlanetWish&&speaker.name=="星球居民 1")resident=speaker;
        g.Player.Teleport(resident.transform.position+Vector3.back);deadline=Time.unscaledTime+16;while(resident.PlayCount==0&&Time.unscaledTime<deadline)yield return null;
        Check("planet_wish_is_spoken_on_approach",resident.PlayCount>0&&resident.VoiceAvailable&&resident.Line.Contains("神圣不可侵犯"));
        int first=resident.PlayCount;deadline=Time.unscaledTime+18;while(resident.PlayCount==first&&Time.unscaledTime<deadline){if(g.StickerOpen)g.SubmitInvitationSticker("不进天堂");yield return null;}
        Check("nearby_planet_resident_murmurs_again",resident.PlayCount>first);
        g.Player.Teleport(resident.transform.position+new Vector3(0,1,-3));g.Player.View.transform.LookAt(resident.transform.position+Vector3.up);ScenePicture(g,Path.Combine(folder,"planet-block-people.png"));
        RealmUsePoint hospital=null,police=null;var lost=new List<RealmUsePoint>();var rides=new List<RealmUsePoint>();
        foreach(var point in FindObjectsByType<RealmUsePoint>()){if(point.Kind==RealmUseKind.Hospital)hospital=point;if(point.Kind==RealmUseKind.Police)police=point;if(point.Kind==RealmUseKind.LostProperty)lost.Add(point);if(point.Kind==RealmUseKind.PlayRide)rides.Add(point);}
        Check("planet_has_hospital_police_and_three_rides",hospital!=null&&police!=null&&lost.Count==3&&rides.Count==3&&GameObject.Find("星球医生")!=null&&GameObject.Find("星球护士")!=null&&GameObject.Find("星球警官")!=null);
        g.Damage(2,"测试受伤");g.UseRealmPoint(hospital);Check("doctor_can_heal_player",g.Health==10);
        RealmUsePoint hospitalDoor=null;foreach(var p in FindObjectsByType<RealmUsePoint>())if(p.Kind==RealmUseKind.Door&&p.Target.parent.name=="星球医院")hospitalDoor=p;
        g.UseRealmPoint(hospitalDoor);Check("hospital_door_can_open",hospitalDoor!=null&&hospitalDoor.Active);
        g.Player.Teleport(new Vector3(95,Sandbox.PlanetFloor+1.5f,76.6f),0,8);yield return new WaitForSecondsRealtime(.3f);ScenePicture(g,Path.Combine(folder,"planet-hospital.png"));
        g.UseRealmPoint(police);int money=g.Inventory[(int)Block.Coin];foreach(var point in lost)g.UseRealmPoint(point);g.UseRealmPoint(police);
        Check("police_lost_property_task_awards_20",g.PlanetLostProperty==3&&g.Inventory[(int)Block.Coin]==money+20);g.UseRealmPoint(police);Check("police_task_does_not_duplicate_reward",g.Inventory[(int)Block.Coin]==money+20);
        foreach(var ride in rides)
        {
            g.Player.Teleport(ride.transform.position+Vector3.back*2);g.Player.View.transform.LookAt(ride.transform.position);Physics.SyncTransforms();money=g.Inventory[(int)Block.Coin];
            Check("G_starts_planet_ride_"+ride.Variant,g.TryVoidRealmG()&&g.PlanetRideActive);
            deadline=Time.unscaledTime+16;while(g.PlanetRideActive&&Time.unscaledTime<deadline)yield return null;
            Check("planet_ride_"+ride.Variant+"_finishes_and_rewards",!g.PlanetRideActive&&!g.Player.Flying&&g.Inventory[(int)Block.Coin]==money+18);
        }
        g.Player.Teleport(Sandbox.PlanetPark+new Vector3(0,3,-13),0,9);ScenePicture(g,Path.Combine(folder,"planet-playground.png"));
        RealmUsePoint bench=null;foreach(var point in FindObjectsByType<RealmUsePoint>())if(point.Caption.Contains("长椅"))bench=point;
        g.UseRealmPoint(bench);Check("park_bench_can_be_used",bench!=null&&g.VoidRideActive);g.TryVoidRealmG();Check("park_bench_can_be_left",!g.VoidRideActive);
        g.Save();int preserved=g.Inventory[(int)Block.Coin];g.Load();yield return null;Check("planet_rewards_and_location_survive_reload",g.InPlanetLife&&g.Inventory[(int)Block.Coin]==preserved);
        g.SetPaused(true);var studio=new GameObject("星球居民年龄展示").transform;Vector3 center=new Vector3(1000,1000,1000);var ages=new HashSet<PlanetAge>();
        foreach(var age in FindObjectsByType<PlanetCitizenAge>()){if(!ages.Add(age.Age))continue;var copy=Instantiate(age.gameObject,studio);foreach(var behaviour in copy.GetComponentsInChildren<Behaviour>())behaviour.enabled=false;copy.transform.position=center+Vector3.right*((int)age.Age-1)*1.1f;copy.transform.rotation=Quaternion.identity;foreach(var t in copy.GetComponentsInChildren<Transform>())t.gameObject.layer=2;if(ages.Count==3)break;}
        int mask=g.Player.View.cullingMask;g.Player.HomeRest(true);g.Player.View.cullingMask=1<<2;g.Player.View.transform.position=center+new Vector3(0,1,3.5f);g.Player.View.transform.LookAt(center+Vector3.up);ScenePicture(g,Path.Combine(folder,"children-adults-elders.png"));Destroy(studio.gameObject);g.Player.View.cullingMask=mask;g.Player.HomeRest(false);g.Player.View.transform.localPosition=Vector3.up*1.65f;g.Player.View.transform.localRotation=Quaternion.identity;g.SetPaused(false);
    }
}}
