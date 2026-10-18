using UnityEngine;
using System.Collections.Generic;
namespace Blocklands {
 public class RoyalArt:MonoBehaviour {
  readonly List<Material> mats=new List<Material>();
  readonly List<Mesh> meshes=new List<Mesh>();
  public Transform Taper(string name,Vector3 p,Vector3 size,Color color,float top){var t=Piece(name,p,size,color,PrimitiveType.Cylinder);var m=t.GetComponent<MeshFilter>().mesh;var v=m.vertices;for(int i=0;i<v.Length;i++){float f=Mathf.Lerp(1,top,(v[i].y+1)*.5f);v[i].x*=f;v[i].z*=f;}m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();meshes.Add(m);return t;}
  public Transform Arm;
  public Transform Piece(string name,Vector3 p,Vector3 size,Color color,PrimitiveType shape=PrimitiveType.Cube,bool solid=false){var g=GameObject.CreatePrimitive(shape);g.name=name;g.transform.SetParent(transform,false);g.transform.localPosition=p;g.transform.localScale=size;var m=new Material(Resources.Load<Shader>("ActorSurface"));m.color=color;mats.Add(m);g.GetComponent<Renderer>().sharedMaterial=m;if(!solid){g.GetComponent<Collider>().enabled=false;Destroy(g.GetComponent<Collider>());}return g.transform;}
  public static RoyalArt Girl(Transform parent,bool child,bool crown){var o=new GameObject(child?"同行伙伴":"女主角");o.transform.SetParent(parent,false);var a=o.AddComponent<RoyalArt>();Color skin=new Color(.94f,.69f,.53f),hair=new Color(.22f,.10f,.14f),pink=new Color(.94f,.42f,.66f),gold=new Color(1,.77f,.2f);
   a.Taper("裙摆",new Vector3(0,.59f,0),new Vector3(.85f,.3f,.65f),pink,.52f);a.Piece("上衣",new Vector3(0,1.05f,0),new Vector3(.48f,.5f,.32f),pink);a.Piece("腰带",new Vector3(0,.86f,0),new Vector3(.53f,.08f,.36f),gold);
   a.Piece("脸",new Vector3(0,1.57f,0),new Vector3(.47f,.55f,.43f),skin,PrimitiveType.Sphere);a.Piece("头发",new Vector3(0,1.78f,-.055f),new Vector3(.5f,.23f,.45f),hair,PrimitiveType.Sphere);a.Piece("马尾",new Vector3(0,1.4f,-.28f),new Vector3(.23f,.35f,.24f),hair,PrimitiveType.Capsule);
   foreach(float x in new[]{-.1f,.1f}){a.Piece("眼睛",new Vector3(x,1.61f,.207f),new Vector3(.065f,.08f,.035f),hair,PrimitiveType.Sphere);a.Piece("鞋",new Vector3(x,.12f,.06f),new Vector3(.17f,.15f,.3f),pink);a.Piece("小腿",new Vector3(x,.3f,0),new Vector3(.11f,.35f,.13f),skin);}
   a.Piece("鼻子",new Vector3(0,1.53f,.24f),new Vector3(.055f,.065f,.06f),skin,PrimitiveType.Sphere);a.Piece("微笑",new Vector3(0,1.43f,.2f),new Vector3(.1f,.025f,.03f),new Color(.62f,.18f,.25f));
   foreach(float x in new[]{-.31f,.31f}){var arm=a.Piece("手臂",new Vector3(x,.99f,0),new Vector3(.14f,.25f,.15f),skin,PrimitiveType.Capsule);if(x>0)a.Arm=arm;}a.Piece("书包",new Vector3(0,.98f,-.27f),new Vector3(.4f,.42f,.21f),new Color(.85f,.35f,.65f));
   if(crown){a.Piece("皇冠",new Vector3(0,1.92f,0),new Vector3(.46f,.12f,.4f),gold);foreach(float x in new[]{-.18f,0,.18f})a.Piece("冠尖",new Vector3(x,2.02f,.11f),new Vector3(.07f,.14f,.1f),gold);}
   if(child)o.transform.localScale=Vector3.one*.75f;return a;
  }
  public static RoyalArt ForestFriend(Transform parent)
  {
   var a=Girl(parent,false,false);a.name="小禾 · 蝴蝶结与简洁连衣裙";
   foreach(var r in a.GetComponentsInChildren<Renderer>())
    if(r.name=="裙摆"||r.name=="上衣"||r.name=="鞋")r.sharedMaterial.color=new Color(.68f,.65f,.89f);
   a.Piece("小禾的蝴蝶结左环",new Vector3(-.18f,1.88f,-.02f),new Vector3(.22f,.14f,.08f),new Color(.96f,.52f,.67f),PrimitiveType.Sphere);
   a.Piece("小禾的蝴蝶结右环",new Vector3(.18f,1.88f,-.02f),new Vector3(.22f,.14f,.08f),new Color(.96f,.52f,.67f),PrimitiveType.Sphere);
   a.Piece("小禾的蝴蝶结中心",new Vector3(0,1.88f,-.01f),Vector3.one*.11f,new Color(.98f,.77f,.85f),PrimitiveType.Sphere);
   return a;
  }
  void OnDestroy(){foreach(var m in mats)if(m!=null)Destroy(m);foreach(var m in meshes)if(m!=null)Destroy(m);}
 }
 public partial class Sandbox {
  public bool CompanionEnabled {get;internal set;}public bool CompanionUnlocked {get;internal set;}public bool RoyalIdentity {get;internal set;}
  RoyalArt companionModel,portrait,portraitFriend;bool inspecting;float portraitAngle,strikeUntil;Vector3 portraitCamera;Quaternion portraitRotation;bool priorPause;int portraitMask;Color portraitBackground;static readonly Vector3 Studio=new Vector3(1000,1000,1000);
  public bool InspectingHero=>inspecting;public bool PortraitHasCrown=>portrait!=null&&portrait.transform.Find("皇冠")!=null;
  public bool PortraitHasHalo=>portrait!=null&&portrait.transform.Find("皇冠上的光环")!=null;
  public bool PortraitHasWings=>portrait!=null&&portrait.transform.Find("天使翅膀")!=null;
  public bool PortraitHasHorns=>portrait!=null&&portrait.transform.Find("红色龙角")!=null;
  public bool PortraitShowsFriendWithoutCrown=>portraitFriend!=null&&portraitFriend.transform.Find("小禾的蝴蝶结中心")!=null&&portraitFriend.transform.Find("皇冠")==null;
  public bool ToggleCompanion(){if(!CompanionUnlocked){RestoreCompanionForHouse();if(CompanionUnlocked){Save();Notice="孩子伙伴在你右前方出现了，P可收回。";return true;}}if(!CompanionUnlocked){Notice="先在风铃村庄建好自己的房子，再按P邀请伙伴。";return false;}CompanionEnabled=!CompanionEnabled;CompanionRevision=1;Save();Notice=CompanionEnabled?"孩子伙伴在你右前方出现了！P收回。":"孩子伙伴回家休息了，P可以召回。";return true;}
  public void CompanionStrike(){strikeUntil=Time.time+.55f;}
  public void CompanionMine(Vector3Int cell,Block block){if(!CompanionEnabled)return;CompanionStrike();if(!(block==Block.Stone||block==Block.Wood||block==Block.Dirt||block==Block.Glass||block==Block.IronOre||block==Blocks.DenseRock||block==Blocks.StarRock))return;foreach(var d in new[]{Vector3Int.right,Vector3Int.left,Vector3Int.forward,Vector3Int.back}){var p=cell+d;if(p.y<=0||World.Region==1&&VillageOwned(p)||World.Get(p)!=block||Inventory[(int)block]>=9999)continue;if(World.Edit(p,Block.Air))Inventory[(int)block]++;break;}}
  public void InspectHero(){if(!inspecting){priorPause=Paused;portraitCamera=Player.View.transform.localPosition;portraitRotation=Player.View.transform.localRotation;portraitMask=Player.View.cullingMask;portraitBackground=Player.View.backgroundColor;portrait=RoyalArt.Girl(transform,false,RoyalIdentity);portrait.transform.position=Studio+(ForestFriendFollowing?Vector3.left*.55f:Vector3.zero);if(InHeaven)AddHaloAndWings(portrait,true);if(InHell)AddHornsAndTrident(portrait);if(ForestFriendFollowing){portraitFriend=RoyalArt.ForestFriend(transform);portraitFriend.transform.position=Studio+Vector3.right*.55f;var skin=new Color(.94f,.69f,.53f);portrait.Piece("牵手的右前臂",new Vector3(.45f,1.02f,.09f),new Vector3(.32f,.13f,.15f),skin,PrimitiveType.Capsule);portraitFriend.Piece("牵手的左前臂",new Vector3(-.45f,1.02f,.09f),new Vector3(.32f,.13f,.15f),skin,PrimitiveType.Capsule);portrait.Piece("贴合的手掌",new Vector3(.56f,1.02f,.09f),Vector3.one*.16f,skin,PrimitiveType.Sphere);portraitFriend.Piece("贴合的手掌",new Vector3(-.56f,1.02f,.09f),Vector3.one*.16f,skin,PrimitiveType.Sphere);foreach(var t in portraitFriend.GetComponentsInChildren<Transform>())t.gameObject.layer=2;}BuildInvitedPortraits();foreach(var t in portrait.GetComponentsInChildren<Transform>())t.gameObject.layer=2;Player.View.cullingMask=1<<2;Player.View.backgroundColor=new Color(.22f,.12f,.2f);portraitAngle=0;inspecting=true;SetPaused(true);Player.HomeRest(true);}else{inspecting=false;DisposeInvitedPortraits();if(portrait!=null)Destroy(portrait.gameObject);if(portraitFriend!=null)Destroy(portraitFriend.gameObject);portraitFriend=null;Player.HomeRest(false);Player.View.cullingMask=portraitMask;Player.View.backgroundColor=portraitBackground;Player.View.transform.localPosition=portraitCamera;Player.View.transform.localRotation=portraitRotation;SetPaused(priorPause);}}
  bool RoyalKeys(){foreach(var key in new[]{KeyCode.F6,KeyCode.C,KeyCode.Escape})if(Input.GetKeyDown(key)&&HandlePortraitKey(key))return true;if(inspecting)return true;if(!Paused&&Input.GetMouseButtonDown(0)&&RayFromView(out var h,5)){var b=h.collider.GetComponent<RoyalFloorButton>();if(b!=null&&b.Lift.Inside){b.Lift.Request(b.Floor);Notice="前往第"+(b.Floor+1)+"层";return true;}}if(!Paused&&Input.GetKeyDown(KeyCode.P)){ToggleCompanion();return true;}if(!Paused&&Input.GetKeyDown(KeyCode.Q)&&castle!=null&&RoyalIdentity){castle.CallRoyalRide();return true;}return false;}
  void LateUpdate(){if(Player==null)return;
   UpdateBodyShadow();
   TickPartyHands();
   if(inspecting){portraitAngle+=Input.GetAxisRaw("Mouse X")*2;var off=Quaternion.Euler(0,portraitAngle,0)*new Vector3(0,1.1f,PartyPortraitDistance);Player.View.transform.position=PartyPortraitFocus+off;Player.View.transform.LookAt(PartyPortraitFocus+Vector3.up*1.05f);}
   if(!CompanionEnabled){if(companionModel!=null){Destroy(companionModel.gameObject);companionModel=null;}return;}
   if(companionModel==null){companionModel=RoyalArt.Girl(transform,true,false);companionModel.transform.position=FindFollowerSpawn(2);NpcNavigation.Ensure(companionModel.transform);companionWorld=World.Region;}
   if(companionWorld!=World.Region){companionWorld=World.Region;companionModel.transform.position=FindFollowerSpawn(2);}
   if(Paused)return;Vector3 target=Player.transform.position+Player.transform.forward*1.8f+Player.transform.right*1.1f;NpcNavigation.Ensure(companionModel.transform).MoveTo(target,9,Time.deltaTime);companionModel.transform.rotation=Quaternion.Slerp(companionModel.transform.rotation,Player.transform.rotation,Time.deltaTime*8);if(companionModel.Arm!=null)companionModel.Arm.localRotation=Quaternion.Euler(Time.time<strikeUntil?-70:Mathf.Sin(Time.time*7)*10,0,0);
  }
 }
}
