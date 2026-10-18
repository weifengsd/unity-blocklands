using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public class SkyResidentInfo:MonoBehaviour {public bool Child;public SkyHouse Owner;public int Index,Hits;public string Role=>(Child?"小孩":"老人")+" · 退让 "+Hits+"/10";public void RegisterHit(){if(!FloodResident.IsDown(transform))Owner.HitResident(Index);}}
    public partial class SkyHouse:MonoBehaviour
    {
        class Rider {public Transform Root,ArmL,ArmR;public CharacterController Body;public int Route;public bool Boarded,OnLanding,Lost;public float EntryUntil,RestUntil;public bool Resting=>Time.time<RestUntil||SwordDownedNpc.IsDown(Root);public SkyResidentInfo Info;}
        Sandbox game;Vector3 home,column;float side,stopUntil,phaseTime;int floor=-1;Transform car;Rider[] riders=new Rider[4];
        readonly List<Material> materials=new List<Material>();LineRenderer aimLine,handLink;Material iron,deck,skin,hair,grey;bool struck;
        public int Floor=>floor;public string Phase {get;private set;}="家中";public Vector3 AimPoint {get;private set;}public Vector3 ReachTip {get;private set;}
        bool downPressed,alarmWasActive;float downPressUntil;
        public Vector3 Anchor=>new Vector3(column.x+side*5.7f,FloorHeight(Mathf.Max(0,floor))+1.15f,column.z);
        public int BoardedCount {get {int n=0;foreach(var r in riders)if(r!=null&&(r.Boarded||r.Resting))n++;return n;}}
        public int LostResidentCount {get {int n=0;foreach(var r in riders)if(r!=null&&r.Lost)n++;return n;}}
        int ResidentCode(int i)=>(home.x<30?0:3)*4+(home.z<15?0:home.z<35?1:2)*4+i;
        void PlaceLostResident(Rider r,int i)
        {
            r.Lost=true;r.RestUntil=float.PositiveInfinity;r.Boarded=false;r.Body.enabled=false;
            int house=(home.x<30?0:3)+(home.z<15?0:home.z<35?1:2);
            r.Root.position=new Vector3(25+house%3*4+i%2*.9f,Sandbox.HeavenFloor+1.1f,21+house/3*10+i/2*1.5f);
            r.Root.rotation=Quaternion.Euler(0,180,0);
        }
        void RescueFallingResidents()
        {
            for(int i=0;i<riders.Length;i++)
            {
                var r=riders[i];if(r==null||r.Lost||r.Root.position.y>=VoxelWorld.MinY-5)continue;
                PlaceLostResident(r,i);game.RememberLostSkyResident(ResidentCode(i));
                // The same landing surface catches the player. Children and elders remain
                // visible here instead of falling forever or silently respawning elsewhere.
                game.Notice="一位"+(r.Info.Child?"小孩":"老人")+"落到了世界底部的安全会合地。沿洞向下可以找到他们；不会强迫你飞。";
            }
        }
        public string Diagnostics(){string s="floor="+floor+" phase="+Phase+" boarded="+BoardedCount;foreach(var r in riders)s+=" | "+r.Root.position+" route="+r.Route;return s;}
        public const int ShopFloors=7, RoofFloor=7;
        public static float FloorHeight(int f)=>f<0?15.4f:f==RoofFloor?92:26+f*10;
        public static string FloorLabel(int f)=>f<0?"底层":f==RoofFloor?"顶层":(f+1)+"层";
        Material Mat(Color c){var m=new Material(Resources.Load<Shader>("Selection"));m.color=c;materials.Add(m);return m;}
        Transform Box(Transform parent,string name,Vector3 p,Vector3 size,Material m,bool solid=false){var a=GameObject.CreatePrimitive(PrimitiveType.Cube);a.name=name;a.transform.SetParent(parent,false);a.transform.localPosition=p;a.transform.localScale=size;a.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Destroy(a.GetComponent<Collider>());return a.transform;}
        public void Initialize(Sandbox g,Vector3 h)
        {
            game=g;home=h;side=h.x<30?1:-1;column=new Vector3(side>0?h.x+11.8f:h.x-1.8f,15.4f,h.z+5);
            iron=Mat(new Color(.3f,.37f,.42f));deck=Mat(new Color(.57f,.71f,.73f));skin=Mat(new Color(.83f,.6f,.43f));hair=Mat(new Color(.16f,.1f,.08f));grey=Mat(new Color(.85f,.86f,.83f));
            foreach(float dx in new[]{-1.6f,1.6f})Box(transform,"Elevator guide rail",new Vector3(column.x+dx,55.2f,column.z+1.6f),new Vector3(.16f,79.6f,.16f),iron);
            for(int f=0;f<=RoofFloor;f++){
                Vector3 c=new Vector3(column.x+side*3.5f,FloorHeight(f),column.z);
                Box(transform,"Sky floor "+(f+1),c,new Vector3(6,.3f,4.3f),deck,true);
                foreach(float z in new[]{-2.1f,2.1f}){if(z<0)Box(transform,"Balcony rail",c+new Vector3(0,1,z),new Vector3(6,.1f,.1f),iron);else foreach(float edge in new[]{-2.15f,2.15f})Box(transform,"Entry railing",c+new Vector3(edge,1,z),new Vector3(1.7f,.1f,.1f),iron);for(int k=-2;k<=2;k+=2)if(z<0||k!=0)Box(transform,"Rail post",c+new Vector3(k,.5f,z),new Vector3(.1f,1,.1f),iron);}
            }
            car=new GameObject("Moving elevator").transform;car.SetParent(transform);car.position=column;Box(car,"Lift floor",Vector3.zero,new Vector3(3.2f,.3f,3.2f),iron,true);
            foreach(float x in new[]{-1.4f,1.4f})Box(car,"Lift frame",new Vector3(x,1.5f,1.4f),new Vector3(.1f,3,.1f),deck);
            for(int i=0;i<4;i++)riders[i]=MakeRider(i);
            for(int i=0;i<4;i++)if(game.LostSkyResidents.Contains(ResidentCode(i)))PlaceLostResident(riders[i],i);
            var line=new GameObject("抓捕预告线");line.transform.SetParent(transform);aimLine=line.AddComponent<LineRenderer>();aimLine.sharedMaterial=Mat(new Color(1,.65f,.15f));aimLine.positionCount=2;aimLine.widthMultiplier=.05f;aimLine.enabled=false;
            var hands=new GameObject("相互拉住的手");hands.transform.SetParent(transform);handLink=hands.AddComponent<LineRenderer>();handLink.sharedMaterial=skin;handLink.positionCount=4;handLink.widthMultiplier=.10f;handLink.enabled=false;
            BuildPublicLift();
        }
        Rider MakeRider(int i)
        {
            bool child=i<2;var r=new Rider();r.Root=new GameObject(child?"家中小孩":"家中老人").transform;r.Root.SetParent(transform);r.Root.position=home+new Vector3(3+i%2*2,0,3+i/2*2);r.Root.localScale=Vector3.one*(child?.72f:1);
            r.Info=r.Root.gameObject.AddComponent<SkyResidentInfo>();r.Info.Child=child;r.Info.Owner=this;r.Info.Index=i;var target=r.Root.gameObject.AddComponent<BoxCollider>();target.center=Vector3.up*1.05f;target.size=new Vector3(.7f,2.1f,.6f);target.isTrigger=true;r.Body=r.Root.gameObject.AddComponent<CharacterController>();r.Body.height=2.1f;r.Body.center=Vector3.up*1.05f;r.Body.radius=.25f;r.Body.stepOffset=child?1.4f:1.05f;
            Material shirt=Mat(Color.HSVToRGB((i*.17f+home.z*.01f)%1,.5f,.78f));
            Box(r.Root,"shirt",Vector3.up*1.05f,new Vector3(.65f,.85f,.36f),shirt);Box(r.Root,"head",Vector3.up*1.75f,new Vector3(.55f,.55f,.5f),skin);Box(r.Root,"hair",new Vector3(0,2.02f,-.02f),new Vector3(.59f,.14f,.54f),child?hair:grey);
            foreach(float x in new[]{-.14f,.14f}){Box(r.Root,"eye",new Vector3(x,1.83f,.258f),new Vector3(.065f,.065f,.025f),hair);if(!child)Box(r.Root,"glasses",new Vector3(x,1.83f,.28f),new Vector3(.13f,.025f,.02f),grey);}
            Box(r.Root,"nose",new Vector3(0,1.7f,.28f),new Vector3(.1f,.12f,.09f),skin);Box(r.Root,"smile",new Vector3(0,1.61f,.26f),new Vector3(.17f,.035f,.02f),hair);
            r.ArmL=Box(r.Root,"left arm",new Vector3(-.44f,1.02f,0),new Vector3(.19f,.73f,.24f),skin);r.ArmR=Box(r.Root,"right arm",new Vector3(.44f,1.02f,0),new Vector3(.19f,.73f,.24f),skin);
            foreach(float x in new[]{-.18f,.18f})Box(r.Root,"leg",new Vector3(x,.37f,0),new Vector3(.24f,.72f,.28f),hair);return r;
        }
        void Update()
        {
            if(game==null||game.Paused)return;bool alarm=Time.time<game.VillageAngryUntil;Vector3 player=game.Player.transform.position;
            bool floodWaiting=false;foreach(var r in riders)if(FloodResident.IsDown(r.Root)){floodWaiting=true;break;}if(floodWaiting){aimLine.enabled=handLink.enabled=false;npcRequests.Clear();npcPending=-2;npcLanding=-2;TickStops();Phase="海啸救援等待 · 玩家电梯仍可使用";return;}
            RescueFallingResidents();WakeResidents();
            UpdateFloorLamps();
            if(alarm&&npcLanding>=0){CivilUpdate();return;}
            if(alarm&&!alarmWasActive)ClearStops();
            if(!alarm&&alarmWasActive){ClearStops();QueueStop(floor,false);shoppingAt=Time.time+60;}alarmWasActive=alarm;
            if(HuntOnGround(alarm,player))return;
            if(!alarm){CivilUpdate();return;}
            if(BringDownForBoarding())return;
            foreach(var r in riders)if(!r.Boarded&&!r.Resting&&alarm){
                if(r.Route==3&&!DoorOpen){r.Body.SimpleMove(Vector3.zero);continue;}
                Vector3 target=RiderWaypoint(r.Route);
                Vector3 d=target-r.Root.position;d.y=0;
                if(d.magnitude<(r.Route==3?2f:.4f)){if(r.Route<3)r.Route++;else {r.Boarded=true;r.Body.enabled=false;r.EntryUntil=Time.time+.6f;stopUntil=Time.time+.7f;}}
                else {r.Body.SimpleMove(d.normalized*3.4f);r.Root.rotation=Quaternion.LookRotation(d);}
            }
            if(BoardedCount<4){Phase="集合乘梯";SetDoors(true);PositionInCar();return;}
            int targetFloor=player.y<23?-1:(player.y>=90?RoofFloor:Mathf.Clamp(Mathf.RoundToInt((player.y-26)/10),0,ShopFloors-1));
            if(targetFloor!=-1)downPressed=false;
            if(targetFloor==-1&&floor>=0&&!downPressed){downPressed=true;downPressUntil=Time.time+.7f;}
            if(Time.time<downPressUntil){Phase="老人按下底层按钮";SetDoors(true);PositionInCar();var elder=riders[3].Resting?riders[2]:riders[3];if(!elder.Resting){elder.Root.position=car.position+new Vector3(-side*.85f,.18f,.3f);elder.ArmL.localRotation=Quaternion.Euler(65,0,0);}return;}
            if(floor!=targetFloor){
                Phase=targetFloor==-1?"按下底层按钮 / 下楼追赶":"直达已选楼层";phaseTime=0;aimLine.enabled=false;MoveNpcLift(targetFloor);return;
            }
            if(floor==RoofFloor){SetDoors(false);PositionInCar();foreach(var r in riders)if(r.Boarded&&!r.Resting){Vector3 look=player-r.Root.position;look.y=0;if(look.sqrMagnitude>.01f)r.Root.rotation=Quaternion.LookRotation(look);}Phase="顶层待命 · 玻璃门关闭";return;}
            SetDoors(true);
            if(floor<0||Time.time<stopUntil){PositionInCar();return;}
            int active=0;foreach(var r in riders)if(!r.Resting)active++;if(active<2){aimLine.enabled=handLink.enabled=false;PositionOnDeck(Anchor);return;}handLink.positionCount=active;
            Vector3 a=Anchor;
            if(phaseTime==0){struck=false;AimPoint=player+Vector3.up*.7f;if(Vector3.Distance(a,AimPoint)>8){Phase="平台等候";PositionOnDeck(a);return;}game.Notice="空中平台正在瞄准你！看到黄线，向左或向右躲开。";}
            phaseTime+=Time.deltaTime;Vector3 diff=AimPoint-a;float length=Mathf.Min(2.5f*(active-1),diff.magnitude);Vector3 dir=diff.sqrMagnitude>.01f?diff.normalized:Vector3.forward;
            float reach=phaseTime<1.5f?0:phaseTime<2.5f?(phaseTime-1.5f)*length:phaseTime<3.3f?length:Mathf.Max(0,1-(phaseTime-3.3f))*length;
            Phase=phaseTime<1.5f?"瞄准":phaseTime<3.3f?"人链伸出":"收回";ReachTip=a+dir*reach;aimLine.enabled=phaseTime<1.5f;aimLine.SetPosition(0,a);aimLine.SetPosition(1,AimPoint);
            int ordinal=0;for(int i=0;i<4;i++){
                var r=riders[i];if(r.Resting)continue;Vector3 target=a+dir*(reach*ordinal/(active-1))-Vector3.up*(r.Root.localScale.x*1.05f);
                if(reach<.1f)target=a+new Vector3(-side*(i%2)*1.1f,-1.05f,i/2*1.1f-.55f);
                r.Root.position=Vector3.MoveTowards(r.Root.position,target,8*Time.deltaTime);Vector3 facing=Vector3.Cross(dir,Vector3.up);if(facing.sqrMagnitude>.01f)r.Root.rotation=Quaternion.LookRotation(facing);r.ArmL.localRotation=Quaternion.Euler(0,0,-90);r.ArmR.localRotation=Quaternion.Euler(0,0,90);
                handLink.SetPosition(ordinal++,r.Root.position+Vector3.up*(r.Root.localScale.x*1.05f));
            }
            handLink.enabled=reach>.3f;
            // Snapshot aim plus actual distance and terrain line of sight: sideways dodging matters.
            if(!struck&&phaseTime>=2.5f&&phaseTime<3.3f&&Vector3.Distance(ReachTip,player+Vector3.up*.7f)<1.05f&&Physics.Linecast(a,player+Vector3.up,out var hit,~0,QueryTriggerInteraction.Ignore)&&hit.collider==game.Player.Body)struck=game.Damage(.5f,"被空中人链抓到");
            if(phaseTime>5.2f)phaseTime=0;
        }
        void PositionInCar(){handLink.enabled=false;int i=0;foreach(var r in riders){if(r.Boarded&&!r.Resting&&!FloodResident.IsDown(r.Root)){Vector3 seat=car.position+new Vector3((i%2-.5f)*1.2f,.18f,(i/2-.5f)*1.2f);r.Root.position=Time.time<r.EntryUntil?Vector3.MoveTowards(r.Root.position,seat,Time.deltaTime*5):seat;r.ArmL.localRotation=r.ArmR.localRotation=Quaternion.identity;}i++;}}
        void PositionOnDeck(Vector3 a){handLink.enabled=false;int i=0;foreach(var r in riders){if(r.Resting){i++;continue;}r.Root.position=Vector3.MoveTowards(r.Root.position,a+new Vector3(-side*(i%2)*1.1f,-1.0f,i/2*1.1f-.55f),6*Time.deltaTime);r.ArmL.localRotation=r.ArmR.localRotation=Quaternion.identity;i++;}}
        void OnDestroy(){Font.textureRebuilt-=UpdatePriceFont;foreach(var m in materials)Destroy(m);}
    }
}
