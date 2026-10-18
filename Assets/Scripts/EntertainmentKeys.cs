using UnityEngine;
namespace Blocklands {
 public partial class Sandbox {
  public static string EntertainmentKeyLabel=>"G";
  public bool TryEntertainmentG(){
   if(Paused||PhoneOpen||RoyalMenuOpen||InspectingHero||Health<=0)return false;
   if(PalaceConsoleOpen){PalaceLeisure.CloseConsole();return true;}
   if(PalacePlayLocked)return PalaceLeisure.Interact();
   if(FestivalLocked)return FestivalG();
   if(!FestivalInputAvailable)return false;
   Vector3 p=Player.transform.position;float nearest=float.PositiveInfinity;System.Func<bool> action=null;
   void Offer(float distance,float range,System.Func<bool> use){if(distance<=range&&distance<nearest){nearest=distance;action=use;}}
   if(PalaceLeisure!=null){foreach(var point in new[]{PalaceLife.CarouselPoint,PalaceLife.SwingPoint,PalaceLife.SlidePoint,PalaceLife.BankPoint,PalaceLife.PatrolPoint,CastleBallPlay.Court})Offer(Vector3.Distance(p,point),3,PalaceLeisure.Interact);if(PalaceLeisure.PatrolActive)Offer(Vector3.Distance(p,PalaceLeisure.ThiefPosition),2.8f,PalaceLeisure.Interact);}
   if(Encounters!=null)foreach(var station in Encounters.Stations)foreach(var point in station.Nodes)Offer(Vector3.Distance(p,point),2.7f,TryRegionalEncounter);
   foreach(var station in FestivalStations){if(station==null)continue;Offer(Vector3.Distance(p,station.Centre),station.Running&&station.Kind==FestivalKind.Dig?4:5,station.Interact);if(station.Running&&station.Kind==FestivalKind.Dig&&station.Progress<3)Offer(Vector3.Distance(p,station.Target(station.Progress)),3,station.Interact);}
   if(Activity!=null){Offer(Vector3.Distance(p,Activity.StartPoint),4,Activity.Interact);if(Activity.Running&&World.Region==3)for(int i=0;i<Activity.Count;i++)Offer(Vector3.Distance(p+Vector3.up,Activity.Marker(i)),2.5f,Activity.Interact);}
   return action!=null&&action();
  }
 }
}
