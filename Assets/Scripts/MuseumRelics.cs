using UnityEngine;
namespace Blocklands {
 public class MuseumRelic:MonoBehaviour {
  public Vector3Int Cell;Sandbox game;public string Title;
  static readonly string[] Titles={"青花瓷瓶","古陶罐","金色面具","青铜鼎","仪仗古剑","石刻日晷","航海罗盘","螺壳化石","小龙骨架","远古石碑","王冠复制品","银色酒壶","彩绘陶盘","玉石手镯","古代算盘","乐器编钟","水晶模型","珊瑚化石","花纹铜镜","古船模型","黄金权杖","历史地图卷轴","星空观测仪","陶瓷茶壶"};
  public static bool IsAnchor(Vector3Int p)=>p.y>=17&&p.y<=74&&(p.y-17)%3==0&&(p.x==5||p.x==9)&&p.z>=32&&p.z<=44&&(p.z-32)%6==0;
  public void Initialize(Sandbox g,Vector3Int cell,int index){game=g;Cell=cell;Title=Titles[index%Titles.Length];transform.position=cell+new Vector3(.5f,-.26f,.5f);var a=gameObject.AddComponent<RoyalArt>();Color color=Color.HSVToRGB((index*.137f)%1,.42f,.82f),gold=new Color(.91f,.7f,.24f),white=new Color(.89f,.87f,.76f);int[] kinds={0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,7,12,17,18,19,6,11};switch(kinds[index%24]){
   case 0:a.Taper("瓶身",new Vector3(0,.28f,0),new Vector3(.48f,.24f,.48f),color,.5f);a.Piece("瓶口",new Vector3(0,.57f,0),new Vector3(.2f,.08f,.2f),white,PrimitiveType.Cylinder);break;
   case 1:a.Piece("陶器",new Vector3(0,.3f,0),new Vector3(.58f,.6f,.58f),color,PrimitiveType.Sphere);a.Piece("器盖",new Vector3(0,.64f,0),new Vector3(.35f,.07f,.35f),gold,PrimitiveType.Cylinder);break;
   case 2:a.Piece("面具",new Vector3(0,.4f,0),new Vector3(.55f,.7f,.14f),gold,PrimitiveType.Sphere);foreach(float x in new[]{-.12f,.12f})a.Piece("面具孔",new Vector3(x,.45f,-.075f),new Vector3(.08f,.05f,.04f),Color.black);break;
   case 3:a.Piece("鼎身",new Vector3(0,.36f,0),new Vector3(.5f,.3f,.4f),color);foreach(float x in new[]{-.17f,.17f})foreach(float z in new[]{-.15f,.15f})a.Piece("足",new Vector3(x,.15f,z),new Vector3(.07f,.3f,.07f),gold);break;
   case 4:a.Piece("剑刃",new Vector3(0,.48f,0),new Vector3(.09f,.6f,.05f),white);a.Piece("剑柄",new Vector3(0,.12f,0),new Vector3(.08f,.2f,.06f),gold);a.Piece("剑格",new Vector3(0,.25f,0),new Vector3(.3f,.06f,.08f),gold);break;
   case 5:a.Piece("刻盘",new Vector3(0,.12f,0),new Vector3(.72f,.1f,.72f),color,PrimitiveType.Cylinder);a.Piece("指针",new Vector3(0,.32f,0),new Vector3(.055f,.36f,.055f),gold);break;
   case 6:a.Piece("罗盘",new Vector3(0,.18f,0),new Vector3(.6f,.15f,.6f),gold,PrimitiveType.Cylinder);a.Piece("指南针",new Vector3(0,.34f,0),new Vector3(.07f,.025f,.5f),color);break;
   case 8:for(int j=0;j<5;j++){a.Piece("脊骨",new Vector3(0,.35f,(j-2)*.13f),Vector3.one*.1f,white);a.Piece("肋骨",new Vector3(0,.25f,(j-2)*.13f),new Vector3(.3f,.2f,.035f),white);}a.Piece("头骨",new Vector3(0,.46f,.32f),new Vector3(.19f,.2f,.22f),white);break;
   case 9:a.Piece("石碑",new Vector3(0,.4f,0),new Vector3(.48f,.8f,.15f),color);for(int j=0;j<4;j++)a.Piece("铭文",new Vector3(0,.2f+j*.12f,-.08f),new Vector3(.3f,.03f,.01f),gold);break;
   case 10:a.Piece("王冠圈",new Vector3(0,.17f,0),new Vector3(.5f,.16f,.5f),gold,PrimitiveType.Cylinder);for(int j=0;j<6;j++){float t=j*Mathf.PI/3;a.Piece("冠尖",new Vector3(Mathf.Cos(t)*.2f,.4f,Mathf.Sin(t)*.2f),new Vector3(.07f,.35f,.07f),gold);}break;
   case 11:a.Piece("壶身",new Vector3(0,.28f,0),new Vector3(.45f,.48f,.4f),color,PrimitiveType.Sphere);a.Piece("壶嘴",new Vector3(.27f,.35f,0),new Vector3(.26f,.12f,.12f),gold);a.Piece("壶盖",new Vector3(0,.55f,0),new Vector3(.3f,.06f,.3f),gold,PrimitiveType.Cylinder);break;
   case 12:a.Piece("盘",new Vector3(0,.08f,0),new Vector3(.65f,.04f,.65f),white,PrimitiveType.Cylinder);a.Piece("彩纹",new Vector3(0,.13f,0),new Vector3(.42f,.02f,.42f),color,PrimitiveType.Cylinder);break;
   case 13:for(int j=0;j<12;j++){float t=j*Mathf.PI/6;a.Piece("玉镯",new Vector3(Mathf.Cos(t)*.24f,.12f,Mathf.Sin(t)*.24f),Vector3.one*.12f,new Color(.2f,.75f,.52f),PrimitiveType.Sphere);}break;
   case 14:a.Piece("算盘框",new Vector3(0,.15f,0),new Vector3(.7f,.08f,.55f),gold);for(int j=0;j<4;j++)for(int k=0;k<3;k++)a.Piece("算珠",new Vector3((j-1.5f)*.13f,.25f,(k-1)*.12f),new Vector3(.1f,.07f,.08f),color,PrimitiveType.Sphere);break;
   case 15:a.Piece("钟架",new Vector3(0,.68f,0),new Vector3(.75f,.08f,.1f),gold);for(int j=0;j<3;j++)a.Taper("编钟",new Vector3((j-1)*.23f,.39f,0),new Vector3(.19f,.22f,.19f),color,.55f);break;
   case 16:a.Taper("晶体",new Vector3(0,.37f,0),new Vector3(.42f,.37f,.42f),color,.06f);break;
   case 17:a.Taper("船身",new Vector3(0,.15f,0),new Vector3(.7f,.1f,.3f),color,.6f);a.Piece("桅杆",new Vector3(0,.46f,0),new Vector3(.035f,.6f,.035f),gold);a.Piece("帆",new Vector3(.1f,.53f,0),new Vector3(.3f,.35f,.035f),white);break;
   case 18:a.Piece("权杖",new Vector3(0,.36f,0),new Vector3(.055f,.72f,.055f),gold);a.Piece("宝珠",new Vector3(0,.76f,0),Vector3.one*.18f,color,PrimitiveType.Sphere);break;
   case 19:a.Piece("古地图",new Vector3(0,.08f,0),new Vector3(.55f,.025f,.7f),white);foreach(float x in new[]{-.3f,.3f}){var roll=a.Piece("卷轴",new Vector3(x,.13f,0),new Vector3(.1f,.35f,.1f),gold,PrimitiveType.Cylinder);roll.localRotation=Quaternion.Euler(90,0,0);}break;
   default:for(int j=0;j<7;j++){float t=j*.8f;a.Piece("化石骨节",new Vector3(Mathf.Cos(t)*.18f,.12f+j*.07f,Mathf.Sin(t)*.18f),Vector3.one*(.25f-j*.02f),white,PrimitiveType.Sphere);}break;
  }var c=gameObject.AddComponent<BoxCollider>();c.center=new Vector3(0,.35f,0);c.size=new Vector3(.8f,.85f,.8f);}
  void Update(){if(game.World.Region!=6||game.World.Get(Cell)==Block.Air)Destroy(gameObject);}
 }
 public partial class CastleSystem {
  void BuildRelics(){int i=0;for(int f=0;f<20;f++)foreach(int x in new[]{5,9})foreach(int z in new[]{32,38,44}){var p=new Vector3Int(x,17+3*f,z);if(Game.World.Get(p)==Block.Air)continue;var o=new GameObject("展品").AddComponent<MuseumRelic>();o.transform.SetParent(transform);o.Initialize(Game,p,i++);}}
 }
}
