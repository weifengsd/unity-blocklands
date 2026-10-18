using UnityEngine;
using UnityEngine.Rendering;
namespace Blocklands {
 public partial class Sandbox {
  // Bump only after an explicit user request for a new princess challenge.
  public const int RequestedPrincessReturn=20260920;
  public int PrincessReturnVersion {get;internal set;}=RequestedPrincessReturn;
  public int CompanionRevision {get;internal set;}=1;
  RoyalArt shadowBody;bool shadowCrown;
  public bool HasBodyShadow=>shadowBody!=null;
  public bool HandlePortraitKey(KeyCode key){if(key==KeyCode.F6||key==KeyCode.C&&(inspecting||!Paused)){InspectHero();return true;}if(inspecting&&key==KeyCode.Escape){InspectHero();return true;}return false;}
  void ApplyRequestedPrincessReturn(){if(World.Region==6&&PrincessReturnVersion<RequestedPrincessReturn){RoyalIdentity=false;PrincessReturnVersion=RequestedPrincessReturn;Player.Teleport(new Vector3(31.5f,16,10),0,0);Notice="这次重新开放公主挑战。门口按J登记；再次获胜后，她将永久退场。";}}
  public void RestoreCompanionForHouse(){bool house=HouseBase.Count>0;if(!house&&World.Region==1){int n=0;foreach(var e in World.Edits)if(!VoxelWorld.VillageRenewable(e.Key)&&e.Key.y>=15&&(e.Value==Block.Planks||e.Value==Block.Wood||e.Value==Block.StoneBricks))n++;house=n>=16;}if(house&&(!CompanionUnlocked||CompanionRevision<1)){CompanionUnlocked=CompanionEnabled=true;CompanionRevision=1;}}
  void UpdateBodyShadow(){if(shadowBody==null||shadowCrown!=RoyalIdentity){if(shadowBody!=null)Destroy(shadowBody.gameObject);shadowBody=RoyalArt.Girl(Player.transform,false,RoyalIdentity);shadowBody.gameObject.name="主角日光投影身体";shadowCrown=RoyalIdentity;foreach(var r in shadowBody.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.ShadowsOnly;}}
 }
 public partial class CastleSystem {
  bool greeted;
  public void SuppressFormerPrincess(){if(!Game.RoyalIdentity)return;if(procession!=null){procession.gameObject.SetActive(false);Destroy(procession.gameObject);procession=null;}if(sunScene!=null){sunScene.gameObject.SetActive(false);Destroy(sunScene.gameObject);sunScene=null;}foreach(var p in GetComponentsInChildren<CrownChallenge>(true))p.gameObject.SetActive(false);}
  void WelcomeAtGate(){Vector3 p=Game.Player.transform.position;bool near=Vector3.Distance(p,new Vector3(31.5f,16,10))<10;if(!near){greeted=false;return;}if(Game.RoyalIdentity){if(!greeted)palaceGate.Set(true);for(int i=0;i<guards.Count;i++){var goal=new Vector3(i==0?22:i==1?41:31.5f,16,i==2?16:12);guards[i].position=Vector3.MoveTowards(guards[i].position,goal,Time.deltaTime*3);Vector3 d=p-guards[i].position;d.y=0;if(d.sqrMagnitude>.01f)guards[i].rotation=Quaternion.LookRotation(d);}}if(!greeted){greeted=true;Game.Notice=Game.RoyalIdentity?"卫兵：欢迎公主！已为您打开大门。Q可召唤皇家轿屋。":"卫兵在门口值守，请按J登记后进入。";}}
  void FurnishCarriage(Transform c){var a=c.GetComponent<RoyalArt>();var old=c.Find("顶棚");if(old!=null)old.gameObject.SetActive(false);Color pink=new Color(.92f,.56f,.76f),gold=new Color(1,.8f,.35f);foreach(float side in new[]{-1f,1f}){var roof=a.Piece("斜屋顶",new Vector3(side*.85f,3.45f,0),new Vector3(1.95f,.16f,2.5f),pink);roof.localRotation=Quaternion.Euler(0,0,-side*26);a.Piece("侧墙",new Vector3(side*1.35f,1.25f,0),new Vector3(.12f,.7f,2.1f),pink);foreach(float z in new[]{-.95f,.95f})a.Piece("窗框",new Vector3(side*1.35f,2.1f,z),new Vector3(.12f,1.7f,.12f),gold);a.Piece("窗上框",new Vector3(side*1.35f,2.9f,0),new Vector3(.12f,.12f,2.1f),gold);}a.Piece("后墙",new Vector3(0,1.5f,1),new Vector3(2.7f,1.2f,.12f),pink);a.Piece("软座",new Vector3(0,1.05f,.3f),new Vector3(2,.25f,1.1f),new Color(.96f,.75f,.86f));}
 }
}
