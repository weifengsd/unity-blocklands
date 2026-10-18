using System.Collections.Generic;using UnityEngine;
namespace Blocklands {
 public partial class CastleSystem {
  public RoyalTelevision ThroneTV {get;private set;}public RoyalTelevision RoofTV {get;private set;}
  public Vector3 RoyalBookcase=>new Vector3(28.2f,76,25.2f);
  RoyalTelevision AddRoyalTV(Transform parent,Vector3 position){var tv=new GameObject("皇家240频道电视");tv.transform.SetParent(parent);tv.transform.position=position;var model=tv.AddComponent<RoyalTelevision>();model.Initialize(Game);return model;}
  void BuildThroneTV(){ThroneTV=AddRoyalTV(transform,new Vector3(31.5f,70,29));}
  void BuildRoofEntertainment(){RoofTV=AddRoyalTV(playerSun,new Vector3(31,76,19.5f));var shelf=new GameObject("遮阳伞下书架").transform;shelf.SetParent(playerSun);shelf.position=RoyalBookcase;var a=shelf.gameObject.AddComponent<RoyalArt>();Color wood=new Color(.92f,.65f,.74f),gold=new Color(.9f,.72f,.35f);foreach(float x in new[]{-.8f,.8f})a.Piece("书柜侧板",new Vector3(x,1,0),new Vector3(.1f,2,.5f),wood);for(int row=0;row<4;row++){a.Piece("书架层板",new Vector3(0,row*.6f+.1f,0),new Vector3(1.7f,.08f,.55f),gold);if(row==3)break;for(int i=0;i<7;i++){var b=a.Piece("彩色书脊",new Vector3(-.63f+i*.21f,.38f+row*.6f,.08f),new Vector3(.16f,.43f,.28f),Color.HSVToRGB((row*7+i)*.047f,.45f,.92f));a.Piece("书脊金线",b.localPosition+new Vector3(0,.11f,.15f),new Vector3(.12f,.025f,.01f),gold);}}}
  public RoyalTelevision NearbyRoyalTV(){RoyalTelevision best=null;float d=8;foreach(var t in new[]{RoofTV,ThroneTV})if(t!=null){float n=Vector3.Distance(t.transform.position,Game.Player.transform.position);if(n<d){best=t;d=n;}}return best;}
 }
 public partial class Sandbox {
  readonly List<int> royalBooks=new List<int>();int lastRoyalBook=-1;bool readingRoyalBook;
  public bool RoyalLeisureKey(KeyCode key){if(Paused||World.Region!=6||castle==null||RoyalMenuOpen)return false;int stage=castle.PlayerSunStage;
   if(key==KeyCode.Alpha2&&stage==0&&RoyalIdentity){castle.TogglePlayerSun();return true;}
   if(key==KeyCode.Z&&(stage==2||stage==3)){castle.ToggleSunPose();return true;}
   if(key==KeyCode.Alpha9&&stage==3){castle.DrinkSunJuice();return true;}
   if(key==KeyCode.Alpha5&&(stage==2||stage==3)){CloseRoyalBook();castle.ReturnSunAnytime();return true;}
   if(stage==1||stage==4)return false;
   if(key==KeyCode.K&&(readingRoyalBook||(stage==2||stage==3)&&Vector3.Distance(Player.transform.position,castle.RoyalBookcase)<6)){TakeRoyalBook();return true;}
   if(readingRoyalBook&&(key==KeyCode.LeftArrow||key==KeyCode.RightArrow)){TurnBook(key==KeyCode.LeftArrow?-1:1);return true;}
   var tv=castle.NearbyRoyalTV();if(tv==null)return false;if(key==KeyCode.G){tv.Toggle();Notice=tv.On?tv.Programme+" · 上下键换台":"皇家电视已关闭";return true;}
   if((stage==3||RoyalThroneSeated||HomeSeated)&&tv.On&&(key==KeyCode.UpArrow||key==KeyCode.DownArrow)){tv.Change(key==KeyCode.UpArrow?-1:1);Notice=tv.Programme;return true;}return false;
  }
  void TakeRoyalBook(){if(readingRoyalBook){CloseRoyalBook();Notice="书已放回阳台书架，K再拿一本。";return;}if(royalBooks.Count==0){for(int i=0;i<Library.Length;i++)royalBooks.Add(i);for(int i=royalBooks.Count-1;i>0;i--){int j=Random.Range(0,i+1),v=royalBooks[i];royalBooks[i]=royalBooks[j];royalBooks[j]=v;}if(royalBooks[0]==lastRoyalBook){int t=royalBooks[0];royalBooks[0]=royalBooks[1];royalBooks[1]=t;}}lastRoyalBook=royalBooks[0];royalBooks.RemoveAt(0);heldBook=Library[lastRoyalBook];bookPage=0;readingRoyalBook=true;Notice="拿到"+heldBook.Title+"；本轮未读完前不会重复。←→翻页，K放回。";}
  void CloseRoyalBook(){if(!readingRoyalBook)return;readingRoyalBook=false;heldBook=null;bookPage=0;}
 }
}
