using UnityEngine;
namespace Blocklands {
 public class RoyalFloorButton:MonoBehaviour {public RoyalLift Lift;public int Floor;}
 public class RoyalLift:MonoBehaviour {
  Sandbox game;Transform car,left,right;float open=1,dwell;int target;public int Floor{get;private set;}
  public bool Moving=>Mathf.Abs(car.position.y-CastleSystem.FloorY(target))>.02f;
  public bool DoorsOpen=>open>.98f;
  public bool Inside{get{Vector3 d=game.Player.transform.position-car.position;return Mathf.Abs(d.x)<1.36f&&Mathf.Abs(d.z)<1.36f&&d.y>-.25f&&d.y<2.5f;}}
  public Vector3 Position=>car.position;
  Material glass;
  public void Initialize(Sandbox g){game=g;car=new GameObject("水晶观景轿厢").transform;car.SetParent(transform);car.position=new Vector3(43.5f,16,18.5f);var art=car.gameObject.AddComponent<RoyalArt>();Color pink=new Color(.86f,.49f,.65f),gold=new Color(.94f,.76f,.34f);art.Piece("轿厢地板",new Vector3(0,-.12f,0),new Vector3(3,.24f,3),pink,PrimitiveType.Cube,true);art.Piece("轿厢顶",new Vector3(0,3.4f,0),new Vector3(3,.15f,3),gold);
   glass=new Material(Resources.Load<Shader>("GlassPane"));glass.color=new Color(.6f,.85f,1,.16f);
   Transform Glass(Vector3 p,Vector3 s){var t=art.Piece("透明玻璃",p,s,Color.white,PrimitiveType.Cube,true);t.GetComponent<Renderer>().sharedMaterial=glass;return t;}
   Glass(new Vector3(-1.5f,1.65f,0),new Vector3(.06f,3.3f,3));Glass(new Vector3(1.5f,1.65f,0),new Vector3(.06f,3.3f,3));Glass(new Vector3(0,1.65f,1.5f),new Vector3(3,3.3f,.06f));left=Glass(new Vector3(-1.5f,1.65f,-1.5f),new Vector3(1.5f,3.3f,.06f));right=Glass(new Vector3(1.5f,1.65f,-1.5f),new Vector3(1.5f,3.3f,.06f));
   for(int f=0;f<10;f++){Vector3 p=new Vector3((f%2-.5f)*.65f,2.25f-(f/2)*.28f,1.36f);var b=art.Piece("楼层按钮 "+(f+1),p,new Vector3(.5f,.3f,.12f),pink,PrimitiveType.Cube,true);var control=b.gameObject.AddComponent<RoyalFloorButton>();control.Lift=this;control.Floor=f;var label=new GameObject("数字");label.transform.SetParent(car,false);label.transform.localPosition=p+Vector3.back*.08f;label.transform.localRotation=Quaternion.identity;var t=label.AddComponent<TextMesh>();t.text=(f+1).ToString();t.characterSize=.07f;t.fontSize=32;t.anchor=TextAnchor.MiddleCenter;t.color=Color.black;}
   for(int f=0;f<10;f++){var panel=new GameObject("电梯站 "+(f+1)).transform;panel.SetParent(transform);panel.position=new Vector3(43.5f,CastleSystem.FloorY(f),16.5f);var a=panel.gameObject.AddComponent<RoyalArt>();foreach(float x in new[]{-1.75f,1.75f})a.Piece("站台边框",new Vector3(x,1.7f,0),new Vector3(.12f,3.4f,.12f),gold);a.Piece("站牌",new Vector3(2,1.6f,0),new Vector3(.55f,.55f,.14f),pink);}
  }
  public void Request(int f){target=Mathf.Clamp(f,0,9);dwell=Time.time+.4f;}
  void Update(){if(game==null||game.Paused)return;float want=Moving&&Time.time>=dwell?0:1;open=Mathf.MoveTowards(open,want,Time.deltaTime*2);left.localPosition=new Vector3(-.75f-open*1.5f,1.65f,-1.5f);right.localPosition=new Vector3(.75f+open*1.5f,1.65f,-1.5f);if(!Moving||open>.01f)return;bool inside=Inside;Vector3 before=car.position;car.position=Vector3.MoveTowards(car.position,new Vector3(43.5f,CastleSystem.FloorY(target),18.5f),Time.deltaTime*8);if(inside)game.Player.Carry(car.position-before);if(!Moving){Floor=target;dwell=Time.time+1;}}
  void OnDestroy(){if(glass!=null)Destroy(glass);}
 }
 public partial class CastleSystem {
  RoyalLift royalLift;public RoyalLift Lift=>royalLift;
  bool museumTicket;public bool MuseumTicket=>museumTicket;
  public bool BuyMuseumTicket(){var p=Game.Player.transform.position;if(Vector3.Distance(p,new Vector3(7,16,24))>5){Game.Notice="到西侧博物馆门口购票，票价50元。";return false;}if(Game.RoyalIdentity){museumTicket=true;museumGate.Set(true);Game.Notice="欢迎公主，免费参观，大门已打开。";return true;}if(museumTicket){Game.Notice="本次参观门票已有效，无需重复购买。";return true;}if(Game.Inventory[(int)Block.Coin]<50){Game.Notice="博物馆门票50元，余额不足。";return false;}Game.Inventory[(int)Block.Coin]-=50;museumTicket=true;if(museumGate!=null)museumGate.Set(true);Game.Save();Game.Notice="门票购买成功，粉晶大门已打开。";return true;}
  void MuseumUpdate(){Vector3 p=Game.Player.transform.position;if(!Game.RoyalIdentity&&!museumTicket&&Time.time>=MuseumAlarmUntil&&p.y>15&&p.x>2&&p.x<13&&p.z>27&&p.z<48){Game.Player.Teleport(new Vector3(7,16,24));Game.Notice="先在门口按L购买50元门票。";}MuseumExpansionUpdate();}
  void RoyalDecoration(){royalLift=new GameObject("粉晶城堡观景电梯").AddComponent<RoyalLift>();royalLift.transform.SetParent(transform);royalLift.Initialize(Game);var throneRoot=new GameObject("粉晶王座").transform;throneRoot.SetParent(transform);throneRoot.position=new Vector3(31.5f,FloorY(9),35);var a=throneRoot.gameObject.AddComponent<RoyalArt>();Color gold=new Color(1,.77f,.3f),pink=new Color(.88f,.34f,.58f);a.Piece("王座高靠背",new Vector3(0,1.8f,.7f),new Vector3(2.4f,3.6f,.28f),gold);a.Piece("天鹅绒靠垫",new Vector3(0,1.8f,.5f),new Vector3(1.9f,2.9f,.16f),pink);a.Piece("坐垫",new Vector3(0,.45f,0),new Vector3(2,.35f,1.6f),pink);foreach(float x in new[]{-1.1f,1.1f})a.Piece("王座扶手",new Vector3(x,.8f,0),new Vector3(.18f,1.6f,1.8f),gold);
   foreach(int x in new[]{14,49})foreach(int z in new[]{14,49}){var tower=new GameObject("粉晶尖塔").transform;tower.SetParent(transform);tower.position=new Vector3(x,15,z);var t=tower.gameObject.AddComponent<RoyalArt>();t.Piece("水晶塔身",new Vector3(0,31,0),new Vector3(4,31,4),new Color(.94f,.67f,.82f),PrimitiveType.Cylinder);t.Taper("塔尖",new Vector3(0,65,0),new Vector3(5,4,5),pink,.02f);t.Piece("塔冠",new Vector3(0,69,0),Vector3.one*.55f,gold,PrimitiveType.Sphere);}
   var museum=new GameObject("博物馆售票处").transform;museum.SetParent(transform);museum.position=new Vector3(7,16,24);var desk=museum.gameObject.AddComponent<RoyalArt>();desk.Piece("售票柜台",new Vector3(2,.6f,0),new Vector3(1.6f,1.2f,.8f),pink);RoyalArt.Girl(museum,false,false).transform.localPosition=new Vector3(2,0,.8f);
   PalaceSign(new Vector3(7,20,25.8f),"粉晶博物馆\n普通访客50元 · 公主免费\n门口按L入馆",180,.1f);
   PalaceSign(new Vector3(31.5f,20,13.6f),"粉晶城堡\nJ申请参观 · H开门",180,.13f);
   PalaceSign(new Vector3(7,19,32),"梦影绿晶展\n来自水晶矿谷的珍藏",180,.07f);
   PalaceSign(new Vector3(7,19,38),"地心岩石展\n向下探索不同岩层",180,.07f);
   BuildMuseumSystems();BuildRelics();nextSun=Time.time+48;
   var princess=transform.Find("公主");if(princess!=null){foreach(Transform t in princess){t.gameObject.SetActive(false);Destroy(t.gameObject);}var figure=RoyalArt.Girl(princess,false,true);figure.transform.localPosition=Vector3.up*.4f;princess.gameObject.AddComponent<CrownChallenge>().Initialize(Game);}BuildRoyalFacilities();
  }
  void PalaceSign(Vector3 p,string words,float yaw,float size){var o=new GameObject(words);o.transform.SetParent(transform);o.transform.position=p;o.transform.rotation=Quaternion.identity;var t=o.AddComponent<TextMesh>();t.font=Resources.Load<Font>("NotoSansCJKsc-Regular");t.fontSize=40;t.characterSize=size;t.text=words;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.38f,.12f,.28f);o.GetComponent<Renderer>().sharedMaterial=t.font.material;}
 }
}
