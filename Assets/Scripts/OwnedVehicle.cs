using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    [Serializable] public class VehiclePassenger {public string key,name;public bool child;}
    [Serializable] public class OwnedVehicle {public int region;public Vector3 position;public float yaw;public bool camper,curtains,flying;public int[] cargo=new int[Blocks.Count];public List<VehiclePassenger> passengers=new List<VehiclePassenger>();}
    public partial class Sandbox
    {
        public OwnedVehicle VehicleData {get;private set;}Transform vehicleRoot;public bool CarRiding {get;private set;}Vector3 carEntry;float carSpeed,carCareTime;string carCare="";Transform carRequest;bool carCargoOpen;int cargoPage;
        public Vector3 CarPosition=>vehicleRoot==null?Vector3.zero:vehicleRoot.position;public Vector3 CarSafePosition=>CarRiding&&vehicleRoot!=null?FindCarExit():carEntry;public bool CarCargoOpen=>carCargoOpen;
        public int CarSeatCount=>8;public int CarPassengerCount=>VehicleData==null?0:VehicleData.passengers.Count;
        public static bool CarPassengerEligible(Transform p){if(FloodResident.IsDown(p))return false;var v=p.GetComponent<Villager>();return v!=null&&!v.IsFriend||p.GetComponent<SkyResidentInfo>()!=null||p.GetComponent<CommunityCitizen>()!=null;}
        public bool SummonCar()
        {
            if(Paused||InVoidRealms||CompanionReturnBlocked||saveBlocked){Notice="请先回到地面空地，再召唤汽车。";return false;}
            if(VehicleData!=null&&VehicleData.region==World.Region&&Vector3.Distance(Player.transform.position,VehicleData.position)<12){Notice="汽车已经在本地区，走近按O上车。";return true;}
            if(VehicleData!=null&&VehicleData.passengers.Count>0){Notice="先回到汽车所在地区让乘客下车，不能带人跨地区瞬移汽车。";return false;}
            Vector3 site=Vector3.zero;bool found=false;
            for(int a=0;a<8;a++){var p=Player.transform.position+Quaternion.Euler(0,a*45,0)*Player.transform.forward*7;if(p.x<3||p.x>VoxelWorld.Width-3||p.z<5||p.z>VoxelWorld.Depth-5)continue;if(!Physics.Raycast(p+Vector3.up*3,Vector3.down,out var ground,9,~(1<<2),QueryTriggerInteraction.Ignore))continue;p.y=ground.point.y+.08f;if(Physics.CheckBox(p+Vector3.up*1.6f,new Vector3(1.75f,1.3f,4),Quaternion.identity,~(1<<2),QueryTriggerInteraction.Ignore))continue;site=p;found=true;break;}
            if(!found){Notice="这儿放不下八座汽车；找一块更宽、更平的空地。";return false;}
            var old=VehicleData;VehicleData=old??new OwnedVehicle();VehicleData.region=World.Region;VehicleData.position=site;VehicleData.yaw=0;BuildVehicle();Save();Notice="八座汽车已到空地；走到车旁再按O上车。";return true;
        }
        public bool EnterCar(){if(Paused||vehicleRoot==null||Vector3.Distance(Player.transform.position,CarPosition)>7||CompanionReturnBlocked||PhoneOpen)return false;carEntry=Player.transform.position;CarRiding=true;carSpeed=0;carCare="";RestoreAirborneControls();Player.SetFlying(false);Player.BeginCoasterSeat();Player.HomeRest(true);PutHeldItemAway();SeatDriver();Notice="坐进驾驶座：W/S或↑↓前后，左右键转向；O同意上客或下车，7变房车，数字0飞行。";return true;}
        void SeatDriver(){if(!CarRiding||vehicleRoot==null)return;Player.MoveCoasterSeat(vehicleRoot.TransformPoint(new Vector3(-.7f,.65f,2.6f)));Player.View.transform.localPosition=Vector3.up*1.55f;}
        public void ExitCar(bool announce=true){if(!CarRiding)return;CarRiding=false;carLanding=false;carCare="";carCargoOpen=false;carSpeed=0;Vector3 exit=vehicleRoot==null?carEntry:FindCarExit();Player.MoveCoasterSeat(exit);Player.ReleaseCoasterSeat();Player.HomeRest(false);Player.SetFlying(false);carEntry=exit;if(announce)Notice="已经下车，方向键现在只移动你自己。";}
        Vector3 FindCarExit(){foreach(float side in new[]{-1f,1f}){Vector3 p=vehicleRoot.TransformPoint(new Vector3(side*2.6f,.1f,1));if(Physics.Raycast(p+Vector3.up*2,Vector3.down,out var hit,5,~(1<<2),QueryTriggerInteraction.Ignore)){p.y=hit.point.y+.05f;if(!Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.35f,~(1<<2),QueryTriggerInteraction.Ignore))return p;}}return carEntry;}
        public bool TryCarO()
        {
            if(Paused||PhoneOpen||StickerOpen||RoyalMenuOpen)return false;
            if(CarRiding){FindCarRequest();if(carRequest!=null)return AdmitCarPassenger(carRequest);ExitCar();return true;}
            if(vehicleRoot!=null&&Vector3.Distance(Player.transform.position,CarPosition)<7)return EnterCar();
            if(World.Region==6&&Player.transform.position.x>16&&Player.transform.position.x<47&&Player.transform.position.z>20&&Player.transform.position.z<48)return false;
            SummonCar();return true;
        }
        void FindCarRequest(){carRequest=null;if(vehicleRoot==null||carCare!="")return;float best=6;foreach(var nav in FindObjectsByType<NpcNavigation>())if(nav.gameObject.activeInHierarchy&&CarPassengerEligible(nav.transform)){float d=Vector3.Distance(nav.transform.position,CarPosition);string key=PartyPersonKey(nav.transform);if(d<best&&!VehicleData.passengers.Exists(p=>p.key==key)){best=d;carRequest=nav.transform;}}}
        public bool AdmitCarPassenger(Transform person){if(!CarRiding||person==null||!CarPassengerEligible(person)||Vector3.Distance(person.position,CarPosition)>6||VehicleData.passengers.Count>=7)return false;string key=PartyPersonKey(person);if(VehicleData.passengers.Exists(p=>p.key==key))return false;VehicleData.passengers.Add(new VehiclePassenger{key=key,name=person.name,child=person.GetComponent<SkyResidentInfo>()?.Child??false});person.gameObject.SetActive(false);BuildVehicle();Save();Notice="你按O同意了上车；乘客 "+CarPassengerCount+" / 7，驾驶位另外1个。";return true;}
        public bool LetPassengersOut(){if(vehicleRoot==null||Mathf.Abs(carSpeed)>.1f)return false;foreach(var p in VehicleData.passengers)foreach(var nav in FindObjectsByType<NpcNavigation>(FindObjectsInactive.Include))if(PartyPersonKey(nav.transform)==p.key){nav.gameObject.SetActive(true);nav.transform.position=FindCarExit()+Vector3.back*(VehicleData.passengers.IndexOf(p)*.8f);break;}VehicleData.passengers.Clear();BuildVehicle();Save();return true;}
        public bool SetCamper(bool value){if(!CarRiding)return false;carSpeed=0;VehicleData.camper=value;BuildVehicle();Save();Notice=value?"房车展开了；H拉窗帘，右侧按钮使用床和洗漱设施。":"房车收起，回到八座汽车。";return true;}
        public bool ToggleCarCurtains(){if(!CarRiding||!VehicleData.camper)return false;VehicleData.curtains=!VehicleData.curtains;BuildVehicle();Save();Notice=VehicleData.curtains?"窗帘拉上了，可以安心睡觉。":"窗帘打开了。";return true;}
        public bool BeginCarCare(string kind){if(!CarRiding||!VehicleData.camper||Mathf.Abs(carSpeed)>.1f)return false;if(kind=="睡觉"&&!VehicleData.curtains){Notice="先按H拉上窗帘再睡。";return false;}if(kind!="睡觉"&&kind!="洗漱"&&kind!="洗澡"&&kind!="马桶")return false;carCare=kind;carCareTime=kind=="睡觉"?30:kind=="洗澡"?10:5;Vector3 p=kind=="睡觉"?new Vector3(-.9f,.8f,-2.5f):kind=="洗澡"?new Vector3(.7f,.2f,-3.1f):new Vector3(1.05f,.5f,-1);Player.MoveCoasterSeat(vehicleRoot.TransformPoint(p));Player.View.transform.localRotation=Quaternion.Euler(35,180,0);Notice="正在"+kind+"，完成后回到驾驶座。";return true;}
        public void DriveCar(Vector2 input,float dt)
        {
            PhaseDrive(input,dt);
        }
        void TickVehicle(float dt){if(Paused||!CarRiding)return;if(carCare!=""){carCareTime-=dt;if(carCareTime<=0){carCare="";SeatDriver();Notice="休息/洗漱完成，可以继续驾驶。";}return;}if(carLanding)LiftCar(-1,dt);FindCarRequest();if(carRequest!=null)Notice=carRequest.name+"：我也想进车里。你按O才会同意。";}
        void CarKeys(){if(Input.GetKeyDown(KeyCode.E)){OpenCrafting();return;}if(Input.GetKeyDown(KeyCode.Escape)){SetPaused(true);return;}if(Input.GetKeyDown(KeyCode.O)){TryCarO();return;}if(Input.GetKeyDown(KeyCode.Alpha0)){ToggleCarFlight();return;}if(CarFlying)LiftCar((Input.GetKey(KeyCode.Space)?1:0)-(Input.GetKey(KeyCode.LeftShift)?1:0),Time.deltaTime);if(Input.GetKeyDown(KeyCode.Alpha7)){SetCamper(!VehicleData.camper);return;}if(Input.GetKeyDown(KeyCode.H)){ToggleCarCurtains();return;}DriveCar(new Vector2((Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S)?1:0)),Time.deltaTime);}
        void RestoreMobility(SavedWorld s){ExitCar(false);VehicleData=s.vehicle;EmptyHands=s.emptyHands;MiningPickups.Clear();if(s.miningPickups!=null)MiningPickups.AddRange(s.miningPickups);}
        void BuildMobility(){BuildVehicle();BuildMiningPickups();}
    }
    public static partial class WorldSave
    {
        static void ValidateVehicle(OwnedVehicle v){if(v==null)return;if(v.region<0||v.region>=VoxelWorld.RegionNames.Length||!Finite(v.position.x)||!Finite(v.position.y)||!Finite(v.position.z)||!Finite(v.yaw)||v.cargo==null||v.cargo.Length!=Blocks.Count||v.passengers==null||v.passengers.Count>7)throw new System.IO.InvalidDataException("汽车数据不合法");foreach(int n in v.cargo)if(n<0||n>9999)throw new System.IO.InvalidDataException("行李数量不合法");var keys=new HashSet<string>();foreach(var p in v.passengers)if(p==null||string.IsNullOrEmpty(p.key)||p.key.Length>200||p.name==null||p.name.Length>150||!keys.Add(p.key))throw new System.IO.InvalidDataException("乘客数据不合法");}
    }
}
