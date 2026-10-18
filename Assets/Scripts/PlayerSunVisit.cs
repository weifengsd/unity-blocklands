using UnityEngine;
namespace Blocklands {
 public partial class CastleSystem {
  Transform playerSun,skyCar,drinkCup;Transform[] attendants;Vector3 sunDeparture,flightFrom,flightTo;float flightAt,restAt,sipAt=-100;int sunStage;
  public bool PlayerSunLocked=>sunStage==1||sunStage==3||sunStage==4;
  public int PlayerSunStage=>sunStage;
  public float SunRestRemaining=>sunStage==3?Mathf.Max(0,30-(Time.time-restAt)):30;
  public bool SipVisible=>drinkCup!=null&&drinkCup.gameObject.activeSelf;
  public void ToggleSunPose(){if(sunStage==3){sunStage=2;Game.Player.HomeRest(false);Game.Player.Teleport(new Vector3(31,76.2f,21.5f),180,0);if(drinkCup!=null)drinkCup.gameObject.SetActive(false);Game.Notice="已起身。Z再躺下，5随时返回。";}else if(sunStage==2)TogglePlayerSun();}
  public void ReturnSunAnytime(){if(sunStage!=2&&sunStage!=3)return;Game.Player.HomeRest(false);if(drinkCup!=null)drinkCup.gameObject.SetActive(false);BeginSunFlight(sunDeparture,4);Game.Notice="仆人正在送你回去。";}
  public bool TogglePlayerSun(){
   if(!Game.RoyalIdentity){Game.Notice="成为公主后，2可请仆人接你到屋顶休息。";return false;}
   if(sunStage==0){if(TransportActive||Game.HomeSeated||Game.HoldingBook||Game.RoyalThroneSeated||AnyLiftMenu){Game.Notice="请先起身并结束当前乘坐，再按2。";return false;}sunDeparture=Game.Player.transform.position;BuildPlayerSun();BeginSunFlight(new Vector3(31,76.2f,21),1);Game.Notice="仆人抬着轿屋，送你到屋顶阳台。";return true;}
   if(sunStage==2){if(Vector3.Distance(Game.Player.transform.position,new Vector3(31,76,24))>5){Game.Notice="靠近粉色躺椅，再按Z躺下。";return false;}sunStage=3;restAt=Time.time;Game.Player.Teleport(new Vector3(31,76.3f,24),180,-10);Game.Player.HomeRest(true,true);Game.Notice="已躺下，仆人送来饮料。9喝饮料，随时按5返回，Z起身。";return true;}
   if(sunStage==3){Game.Player.HomeRest(false);BeginSunFlight(sunDeparture,4);Game.Notice="休息结束，仆人送你回楼下。";return true;}return false;
  }
  void BeginSunFlight(Vector3 destination,int stage){sunStage=stage;flightAt=Time.time;flightFrom=Game.Player.transform.position;flightTo=destination;Game.Player.SetFlying(false);skyCar=Carriage("阳光休息接送轿屋",flightFrom-Vector3.up*.8f);}
  void BuildPlayerSun(){if(playerSun!=null)return;playerSun=new GameObject("玩家公主屋顶休息区").transform;playerSun.SetParent(transform);playerSun.position=new Vector3(31,76,24);var a=playerSun.gameObject.AddComponent<RoyalArt>();Color pink=new Color(.98f,.62f,.78f),white=new Color(.97f,.93f,.84f);a.Piece("阳台软毯",new Vector3(0,.03f,0),new Vector3(8,.06f,7),pink);a.Piece("公主躺椅",new Vector3(0,.3f,0),new Vector3(1.4f,.35f,2.8f),pink);var back=a.Piece("躺椅斜靠背",new Vector3(0,.72f,.95f),new Vector3(1.4f,.9f,.16f),white);back.localRotation=Quaternion.Euler(-25,0,0);a.Piece("果汁桌",new Vector3(1.5f,.5f,-.4f),new Vector3(.85f,1,.85f),white);
   attendants=new Transform[2];for(int i=0;i<2;i++){float side=i==0?-1:1;var server=RoyalArt.Girl(playerSun,false,false);attendants[i]=server.transform;server.gameObject.name="阳台服务员";server.transform.localPosition=new Vector3(side*3,0,-2);server.Piece("端饮料托盘",new Vector3(0,1,.4f),new Vector3(.6f,.05f,.4f),white);StrawberryDrink.Create(server.transform,new Vector3(0,1.18f,.4f),.3f);a.Piece("遮阳伞杆",new Vector3(side*2.8f,1.6f,.5f),new Vector3(.08f,3.2f,.08f),white);a.Taper("粉色遮阳伞",new Vector3(side*2.8f,3.2f,.5f),new Vector3(2.5f,.3f,2.5f),pink,.05f);}
   BuildRoofEntertainment();var cupRoot=new GameObject("公主手中饮料").transform;cupRoot.SetParent(Game.Player.View.transform,false);drinkCup=cupRoot;var cup=cupRoot.gameObject.AddComponent<RoyalArt>();StrawberryDrink.Create(cupRoot,Vector3.zero,.22f);cup.Piece("拿杯的小手",new Vector3(.08f,-.04f,0),new Vector3(.12f,.13f,.13f),new Color(.94f,.69f,.53f),PrimitiveType.Sphere);foreach(var t in cupRoot.GetComponentsInChildren<Transform>())t.gameObject.layer=2;cupRoot.gameObject.SetActive(false);
  }
  public bool DrinkSunJuice(){if(sunStage!=3||Time.time-restAt<3){Game.Notice="先躺下，等仆人把饮料送到身边。";return false;}if(Time.time-sipAt<2.5f)return false;sipAt=Time.time;drinkCup.gameObject.SetActive(true);Game.HealRoyal(1);Game.Notice="拿起草莓饮料，喝一口，再放下杯子。";return true;}
  void TickPlayerSun(){if(sunStage==0)return;if(sunStage==1||sunStage==4){float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.time-flightAt)/6));Vector3 destination=Vector3.Lerp(flightFrom,flightTo,t);Game.Player.Teleport(destination,180,0);skyCar.position=destination-Vector3.up*.8f;if(t>=1){Destroy(skyCar.gameObject);skyCar=null;if(sunStage==1){sunStage=2;Game.Notice="抵达屋顶：Z躺上粉色躺椅，之后9喝饮料。";}else {sunStage=0;if(drinkCup!=null)Destroy(drinkCup.gameObject);if(playerSun!=null)Destroy(playerSun.gameObject);Game.Notice="仆人已经送你回来。";}}}
   if(sunStage==3){for(int i=0;i<2;i++)attendants[i].localPosition=Vector3.MoveTowards(attendants[i].localPosition,new Vector3(i==0?-1.2f:1.3f,0,-1.3f),Time.deltaTime*.8f);if(drinkCup!=null&&drinkCup.gameObject.activeSelf){float age=Time.time-sipAt;var strawberry=drinkCup.GetComponentInChildren<StrawberryDrink>();if(strawberry!=null)strawberry.Sip(age/2.5f);float t=Mathf.Sin(Mathf.Clamp01(age/2.5f)*Mathf.PI);drinkCup.localPosition=Vector3.Lerp(new Vector3(.3f,-.4f,.65f),new Vector3(.03f,-.13f,.3f),t);drinkCup.localRotation=Quaternion.Euler(-t*45,0,0);if(age>=2.5f)drinkCup.gameObject.SetActive(false);}}
  }
  public void DrawPlayerSun(float w,float h){if(sunStage==0)return;Sandbox.Panel(new Rect(w/2-265,h-135,530,70),new Color(.21f,.1f,.22f,.95f));Sandbox.Label(new Rect(w/2-255,h-128,510,60),sunStage==1||sunStage==4?"仆人正在接送，请稍候…":sunStage==2?"阳光阳台 · 按Z躺下休息":"自由休息 · Z起身 · 9喝饮料\nG电视 · K书架 · 5随时返回",19,Color.white,TextAnchor.MiddleCenter);}
 }
}
