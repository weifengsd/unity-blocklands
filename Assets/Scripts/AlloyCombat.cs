using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    [Serializable] public class NpcCombatState {public int region,hits;public string key;public bool down;}
    public class SwordDownedNpc:MonoBehaviour
    {
        public bool Down;Transform pose;Collider[] bodies;bool[] enabledBefore;
        public static bool IsDown(Transform t){var d=t.GetComponentInParent<SwordDownedNpc>();return d!=null&&d.Down;}
        public void Fall(){if(Down)return;bodies=GetComponents<Collider>();enabledBefore=new bool[bodies.Length];for(int i=0;i<bodies.Length;i++)enabledBefore[i]=bodies[i].enabled;Down=true;if(pose==null){pose=new GameObject("合金剑击倒姿势").transform;pose.SetParent(transform,false);var parts=new List<Transform>();foreach(Transform child in transform)if(child!=pose&&(child.GetComponent<Renderer>()!=null||child.GetComponentInChildren<Renderer>()!=null))parts.Add(child);foreach(var p in parts)p.SetParent(pose,false);}pose.localPosition=Vector3.up*.3f;pose.localRotation=Quaternion.Euler(0,0,85);DisableBodies();}
        public void Recover(){if(!Down)return;Down=false;pose.localPosition=Vector3.zero;pose.localRotation=Quaternion.identity;for(int i=0;i<bodies.Length;i++)if(bodies[i]!=null)bodies[i].enabled=enabledBefore[i];Physics.SyncTransforms();}
        void DisableBodies(){foreach(var c in GetComponents<Collider>())c.enabled=false;}void LateUpdate(){if(Down)DisableBodies();}
    }
    public partial class Sandbox
    {
        public readonly List<NpcCombatState> NpcCombatStates=new List<NpcCombatState>();float nextAlloySlash;
        public string CombatNpcKey(Transform target)=>target.GetComponent<VillageGuardian>()!=null?"golem:"+World.Region:PartyPersonKey(target);
        public void ApplyNpcCombatView(Transform target){string key=CombatNpcKey(target);var s=NpcCombatStates.Find(n=>n.region==World.Region&&n.key==key);if(s!=null&&s.down){(target.GetComponent<SwordDownedNpc>()??target.gameObject.AddComponent<SwordDownedNpc>()).Fall();if(target.GetComponent<VillageGuardian>()==null)RegisterMedicalPatient(target);}}
        void RestoreNpcCombatViews(){foreach(var v in FindObjectsByType<Villager>())if(!v.IsFriend)ApplyNpcCombatView(v.transform);foreach(var v in FindObjectsByType<SkyResidentInfo>())ApplyNpcCombatView(v.transform);foreach(var v in FindObjectsByType<CommunityCitizen>())ApplyNpcCombatView(v.transform);foreach(var v in FindObjectsByType<VillageGuardian>())ApplyNpcCombatView(v.transform);}
        public bool HitWithAlloy(Transform target)
        {
            if(target==null||Paused||EmptyHands||Selected!=90||Inventory[90]<1||FloodResident.IsDown(target))return false;var v=target.GetComponent<Villager>();if(v!=null&&v.IsFriend)return false;bool golem=target.GetComponent<VillageGuardian>()!=null;bool npc=golem||v!=null||target.GetComponent<SkyResidentInfo>()!=null||target.GetComponent<CommunityCitizen>()!=null;if(!npc){var actor=target.GetComponent<CombatActor>();if(actor==null)return false;actor.Hit(24);return true;}
            string key=CombatNpcKey(target);var s=NpcCombatStates.Find(n=>n.region==World.Region&&n.key==key);if(s==null){if(NpcCombatStates.Count>=512)return false;s=new NpcCombatState{region=World.Region,key=key};NpcCombatStates.Add(s);}if(s.down)return false;s.hits++;AlertVillage();if(s.hits>=(golem?2:1)){s.down=true;var tower=target.parent==null?null:target.parent.GetComponent<VillageTower>();if(tower!=null)tower.BreakFormation();ApplyNpcCombatView(target);if(!golem)RegisterMedicalPatient(target);Notice=golem?"铁傀儡被两刀击倒；重进村庄后恢复。":"村民倒地了；护士会前来救援，恢复后可再次行动。";}else Notice="铁傀儡挡住第一刀，还剩一次挥斩。";Save();return true;
        }
        public bool AlloySlash()
        {
            if(Paused||EmptyHands||Selected!=90||Inventory[90]<1||Time.time<nextAlloySlash)return false;nextAlloySlash=Time.time+.7f;Player.StartAlloySwing();var seen=new HashSet<Transform>();bool hit=false;
            foreach(var c in Physics.OverlapSphere(Player.View.transform.position,5,~(1<<2),QueryTriggerInteraction.Ignore)){Transform t=null;var v=c.GetComponentInParent<Villager>();var f=c.GetComponentInParent<SkyResidentInfo>();var g=c.GetComponentInParent<VillageGuardian>();var citizen=c.GetComponentInParent<CommunityCitizen>();var actor=c.GetComponentInParent<CombatActor>();t=v!=null?v.transform:f!=null?f.transform:g!=null?g.transform:citizen!=null?citizen.transform:actor!=null?actor.transform:null;if(t==null||!seen.Add(t))continue;Vector3 aim=t.position+Vector3.up-Player.View.transform.position;if(Vector3.Dot(aim.normalized,Player.View.transform.forward)<.35f)continue;if(Physics.Raycast(Player.View.transform.position,aim.normalized,out var wall,aim.magnitude,~(1<<2),QueryTriggerInteraction.Ignore)&&!wall.collider.transform.IsChildOf(t)&&wall.collider.transform!=t)continue;hit|=HitWithAlloy(t);}
            if(!hit)Notice="合金剑斜向挥过前方；靠近并对准目标再挥。";return true;
        }
        public void EquipSetWeapon(bool alloy){int id=alloy?90:85;if(Inventory[id]>0)SelectItem(id);}
        void RestoreNpcCombat(SavedWorld save){NpcCombatStates.Clear();if(save.npcCombat!=null)NpcCombatStates.AddRange(save.npcCombat);}
    }
    public partial class FirstPerson
    {
        float alloySwingUntil;
        public void StartAlloySwing(){RefreshHeldItem();alloySwingUntil=Time.time+.45f;}
        void AnimateAlloySwing(){if(heldItem==null||heldId!=90)return;float t=1-Mathf.Clamp01((alloySwingUntil-Time.time)/.45f);heldItem.localRotation=Time.time<alloySwingUntil?Quaternion.Euler(-30+60*t,0,-65+125*t):Quaternion.Euler(0,0,-8);}
    }
}
