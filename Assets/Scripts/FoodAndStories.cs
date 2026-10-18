using System.Collections.Generic;
using UnityEngine;
namespace Blocklands {
 public class StoryCafe:MonoBehaviour {
  Sandbox game;RoyalArt art;Font font;public bool Story;public Vector3 Counter=>transform.position;public int Step {get;private set;}float readyAt;
  string title,food;int payment;
  static readonly string[] CastlePages={
   "很久以前，城堡里有一位爱画地图的公主。她总想知道墙外的森林是什么颜色。",
   "一天，小禾把林地小屋做的点心送来。公主邀请大家进城，第一次打开了大门。",
   "后来，卫兵建起观景电梯。小朋友在塔顶数星星，大人们在故事廊里留下旅行日记。",
   "今天换你来续写这一页：旅途回来，吃点心，读故事，再带朋友去看新风景。"
  };
  public void Initialize(Sandbox g,Vector3 spot,string name,string meal,int coins,bool story=false){game=g;transform.position=spot;Story=story;title=name;food=meal;payment=coins;font=Resources.Load<Font>("NotoSansCJKsc-Regular");art=gameObject.AddComponent<RoyalArt>();Color cream=new Color(.94f,.85f,.7f),pink=new Color(.9f,.55f,.72f);art.Piece(story?"城堡故事长廊":"小吃料理台",new Vector3(0,.58f,0),new Vector3(2.6f,1.16f,.9f),cream,PrimitiveType.Cube,true);
   if(story){art.Piece("故事廊绒毯",new Vector3(0,.035f,1.13f),new Vector3(3.35f,.07f,2.1f),new Color(.62f,.23f,.43f));art.Piece("故事廊金边",new Vector3(0,.08f,1.13f),new Vector3(3.42f,.035f,2.16f),new Color(.92f,.72f,.32f));for(int i=0;i<4;i++){float x=(i-1.5f)*.56f;art.Piece("王国故事画框",new Vector3(x,1.7f,.15f),new Vector3(.49f,.65f,.08f),new Color(.87f,.68f,.35f));art.Piece("故事小画像",new Vector3(x,1.7f,.1f),new Vector3(.38f,.5f,.04f),Color.HSVToRGB(i*.22f,.35f,.9f));art.Piece("画中人物",new Vector3(x,1.69f,.065f),new Vector3(.13f,.3f,.05f),new Color(.94f,.66f,.48f),PrimitiveType.Capsule);art.Piece("画中太阳",new Vector3(x-.1f,1.84f,.05f),Vector3.one*.1f,new Color(1,.85f,.4f),PrimitiveType.Sphere);}foreach(float x in new[]{-1.44f,1.44f}){art.Piece("故事廊金柱",new Vector3(x,1.14f,.15f),new Vector3(.1f,1.8f,.11f),new Color(.85f,.7f,.4f));art.Piece("水晶壁灯",new Vector3(x,2.12f,.2f),Vector3.one*.22f,new Color(.98f,.88f,.67f),PrimitiveType.Sphere);}}
   else{var cook=RoyalArt.Girl(transform,false,false);cook.transform.localPosition=new Vector3(0,0,.95f);cook.gameObject.name="当地小厨师";Color coral=new Color(.96f,.66f,.6f);foreach(float x in new[]{-1.35f,1.35f})art.Piece("料理台圆角支柱",new Vector3(x,1.67f,.25f),new Vector3(.09f,2.9f,.09f),cream,PrimitiveType.Cylinder);art.Piece("粉色遮阳棚",new Vector3(0,3.1f,.25f),new Vector3(3,.16f,1.55f),coral);for(int i=0;i<7;i++)art.Piece("遮阳棚波浪花边",new Vector3((i-3)*.43f,2.96f,1.02f),new Vector3(.42f,.2f,.18f),i%2==0?Color.white:coral,PrimitiveType.Sphere);art.Piece("料理锅",new Vector3(0,1.32f,0),new Vector3(.95f,.32f,.75f),new Color(.36f,.48f,.56f),PrimitiveType.Cylinder);art.Piece("锅中食物",new Vector3(0,1.49f,0),new Vector3(.77f,.08f,.6f),pink,PrimitiveType.Cylinder);for(int i=0;i<3;i++){art.Piece("彩色食材",new Vector3((i-1)*.4f,1.34f,-.3f),Vector3.one*.23f,Color.HSVToRGB(i*.22f,.65f,1),PrimitiveType.Sphere);art.Piece("点心展示盘",new Vector3((i-1)*.67f,1.23f,.36f),new Vector3(.43f,.07f,.3f),Color.white,PrimitiveType.Cylinder);art.Piece("烤好的面包",new Vector3((i-1)*.67f,1.33f,.36f),new Vector3(.26f,.12f,.2f),new Color(.85f,.56f,.28f),PrimitiveType.Sphere);}for(int i=0;i<3;i++)art.Piece("料理蒸汽",new Vector3((i-1)*.18f,1.78f+i*.14f,.02f),Vector3.one*(.08f+i*.025f),new Color(.9f,.94f,1),PrimitiveType.Sphere);}
   var text=new GameObject("料理或故事名字");text.transform.SetParent(transform,false);text.transform.localPosition=new Vector3(0,2.55f,-.1f);var t=text.AddComponent<TextMesh>();t.font=font;text.GetComponent<Renderer>().sharedMaterial=font.material;t.text=title+"\nG互动";t.characterSize=.09f;t.fontSize=32;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;
  }
  public bool Interact(){if(game.Paused||Vector3.Distance(game.Player.transform.position,transform.position)>3.5f)return false;
   if(Story){game.Notice=CastlePages[Step];Step++;if(Step==CastlePages.Length){Step=0;game.EarnActivityCoins(payment,title);game.Notice="四页王国故事读完了，获得"+payment+"元。";}return true;}
   if(Step==0){Step=1;readyAt=Time.time+1;game.Notice=title+"：食材放进锅里，等一会儿按G搅拌。";return true;}
   if(Time.time<readyAt){game.Notice="闻到香味了，再稍等片刻。";return true;}
   if(Step==1){Step=2;readyAt=Time.time+1;game.Notice="已经搅匀啦！等面包烤好，按G享用。";return true;}
   Step=0;int before=game.Inventory[(int)Block.Bread];game.Inventory[(int)Block.Bread]=Mathf.Min(9999,before+1);int amount=game.EarnActivityCoins(payment,title);if(amount==0)game.Inventory[(int)Block.Bread]=before;else{game.HealRoyal(1);game.Notice="吃到了"+food+"，获得"+amount+"元和一个面包，恢复一颗心。";}return true;
  }
  void OnGUI(){if(game==null||game.Paused||Vector3.Distance(game.Player.transform.position,transform.position)>8)return;var style=new GUIStyle(GUI.skin.label){font=font,fontSize=16};style.normal.textColor=Color.white;GUI.Box(new Rect(23,508,500,55),"");GUI.Label(new Rect(35,515,475,40),title+" · "+(Story?"G翻故事页；读完4页得"+payment+"元":"G做"+food+"；完成一次得"+payment+"元"),style);}
 }
 public partial class Sandbox {
  public readonly List<StoryCafe> Cafes=new List<StoryCafe>();
  void BuildStoryCafes(){Cafes.Clear();void Add(Vector3 pos,string name,string meal,int amount,bool story=false){var obj=new GameObject(name);obj.transform.SetParent(villagePeople.transform);var cafe=obj.AddComponent<StoryCafe>();cafe.Initialize(this,pos,name,meal,amount,story);Cafes.Add(cafe);}
   Vector3 GroundAt(int x,int z){for(int y=45;y>=0;y--)if(Blocks.Solid(World.Get(new Vector3Int(x,y,z))))return new Vector3(x+.5f,y+1,z+.5f);return new Vector3(x+.5f,15,z+.5f);}
   if(World.Region==0)Add(new Vector3(36.5f,20,39.5f),"森林小屋料理台","野莓面包",16);
   if(World.Region==1)Add(GroundAt(31,39),"风铃村麦田厨房","蜂蜜面包",16);
   if(World.Region==2)Add(GroundAt(42,39),"水晶谷热茶摊","晶谷点心",18);
   if(World.Region==3)Add(GroundAt(43,38),"梦影地星果摊","星果小餐",18);
   if(World.Region==4)Add(GroundAt(46,38),"竹林米饭摊","竹香米饭",16);
   if(World.Region==6){Add(new Vector3(26,16,39),"皇宫餐厅","皇家草莓点心",24);Add(new Vector3(34,34,39),"城堡四层故事廊","城堡故事",26,true);}
   Add(new Vector3(30.5f,-48.5f,26.5f),"熔岩洞热汤摊","香暖汤",20);
  }
  public bool TryStoryCafeG(){StoryCafe best=null;float distance=3.5f;foreach(var c in Cafes){if(c==null)continue;float d=Vector3.Distance(Player.transform.position,c.Counter);if(d<distance){distance=d;best=c;}}return best!=null&&best.Interact();}
 }
}
