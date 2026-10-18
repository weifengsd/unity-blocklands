using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public partial class Sandbox {
 public readonly List<FestivalStation> FestivalStations=new List<FestivalStation>();
 public bool FestivalLocked{get{foreach(var s in FestivalStations)if(s!=null&&s.LocksPlayer)return true;return false;}}
 public bool FestivalInputAvailable=>!Paused&&!RoyalMenuOpen&&!PhoneOpen&&!PalacePlayLocked&&!PalaceConsoleOpen&&!InspectingHero&&!HomeSeated&&!HoldingBook&&!RoyalThroneSeated&&!RoyalTransportActive&&Health>0&&!VictoryOpen;
 void BuildFestival(){CancelFestivalForNavigation();FestivalStations.Clear();void Add(FestivalKind kind,string title,Vector3 position){var o=new GameObject(title);o.transform.SetParent(villagePeople.transform);var s=o.AddComponent<FestivalStation>();FestivalStations.Add(s);s.Initialize(this,kind,title,position);}
  Vector3 Ground(int x,int z){for(int y=55;y>=0;y--)if(Blocks.Solid(World.Get(new Vector3Int(x,y,z))))return new Vector3(x+.5f,y+1,z+.5f);return new Vector3(x,15,z);}
  if(World.Region==0)Add(FestivalKind.Coaster,"森林云端过山车",new Vector3(32,19,28));
  if(World.Region==1)Add(FestivalKind.Memory,"风铃村灯笼记忆赛",new Vector3(31,15,40));
  if(World.Region==2)Add(FestivalKind.Dance,"水晶谷音乐石",Ground(30,14));
  if(World.Region==3)Add(FestivalKind.Flight,"梦影裂隙飞行环",Ground(42,17));
  if(World.Region==4)Add(FestivalKind.Footprints,"竹林神秘大脚印",Ground(20,20));
  if(World.Region==6)Add(FestivalKind.Memory,"博物馆星座解码",new Vector3(7,16,39));
  Add(FestivalKind.Dance,"熔岩猪灵羊灵舞会",new Vector3(31,-48.7f,30));
  Add(FestivalKind.Dig,"深铁矿脉考古",new Vector3(30,-75,12));
  Add(FestivalKind.Memory,"星晶回声石",new Vector3(31,-109,30));
  Add(FestivalKind.Sailing,"星海浮标航行赛",new Vector3(34,-135.3f,23));
  Add(FestivalKind.Footprints,"地心花园发光足迹",new Vector3(30,-178,30));
 }
 public bool FestivalG(){if(!FestivalInputAvailable)return false;foreach(var s in FestivalStations)if(s!=null&&s.Running&&s.Interact())return true;FestivalStation best=null;float distance=5;foreach(var s in FestivalStations){if(s==null||DepthBoatRiding&&s.Kind!=FestivalKind.Sailing)continue;float d=Vector3.Distance(s.Centre,Player.transform.position);if(d<=distance){distance=d;best=s;}}return best!=null&&best.Interact();}
}}
