using System;
using UnityEngine;

namespace Blocklands
{
    public partial class Sandbox
    {
        GameObject villagePeople;
        bool excursionTab=true;
        public void OpenTravel(){if(CastleJailed){Notice="监狱铁栅栏还没打开，不能直接旅行离开。用钥匙、锤子或等待卫兵放人。";return;}inventoryOpen=false;Crafting.Clear();travelOpen=true;Notice="选择目的地即可出发；你的材料和建造都会保存。";SetPaused(true);}
        public bool TravelTo(int region,bool viaPortal=false)
        {
            ExitCar(false);
            StopPartyRide();
            ResetDeepBase();
            if(SecretSwingActive)StopSecretSwing();
            CloseSecretMonitor();
            if(region<0||region>=VoxelWorld.RegionNames.Length)return false;
            if(region==5||region>=7&&region<=10)return VisitConnectedDepth(region);
            if(Health<=0)return false;
            if(region==3&&(!viaPortal||!GateUnlocked)){Notice="梦影地只能通过熔岩竹谷的传送门进入：14黑紫晶块＋6梦影绿晶。";return false;}
            if(saveBlocked){Notice="请先检查原存档，暂不切换地区以免丢失建造。";return false;}
            // Save the departure world first. An I/O error must never discard its edits.
            var departure=WorldSave.Capture(World,Player,Inventory,Selected);int refunded=region==1?PrepareVillageReturn(departure):0;
            try{WorldSave.Write(departure,WorldSave.PathName);}
            catch(Exception e){Notice="出发前保存失败："+e.Message;return false;}
            if(region==1){World.RegionEdits.Clear();if(departure.regions!=null)foreach(var r in departure.regions)World.RegionEdits[r.id]=r.edits;if(World.Region==1)World.ApplyEdits(departure.edits);Array.Copy(departure.inventory,Inventory,Inventory.Length);RestoreVillageReturns(departure);}
            World.SwitchRegion(region);theft.Clear();if(region==1){NpcCombatStates.RemoveAll(n=>n.region==1);DeadAnimals.Clear();VillageAngryUntil=0;FallenResidents.Clear();LostSkyResidents.Clear();}if(region==3&&DragonHealth<=0)DragonHealth=100;Player.Teleport(World.SafeSpawn(),0,4);RefreshRegion();
            SetPaused(false);Save();Notice="已到达"+VoxelWorld.RegionNames[region]+(refunded>0?"。临时石头、泥土已回收"+refunded+"块；背包满的部分留在回收袋。":"。E 合成，T 可随时返程。");return true;
        }
        void RefreshRegion()
        {
            ResetHomePartyModels();
            World.PreserveVillageCell=PreserveMyVillageCell;
            ResetSecretRooms();
            companionWorld=-1;
            ResetCompanionReturns();
            ResetPhone();
            ResetExcursionAttraction();
            ResetExcursionSides();
            CloseRoyalBook();ResetHomePose();ClearRoyalThrone();MigrateOwnHouse();
            RegisterLegacyVillageHome();
            RestoreCompanionForHouse();ApplyRequestedPrincessReturn();
            if(villagePeople!=null)Destroy(villagePeople);
            villagePeople=new GameObject("Village residents");
            if(World.Region==1)for(int i=0;i<6;i++)
            {
                var p=new GameObject("Villager "+(i+1));p.transform.SetParent(villagePeople.transform);
                p.transform.position=new Vector3(i%2==0?26:35,15,18+(i/2)*15);
                p.AddComponent<Villager>().Initialize(i,this);
            }
            if(World.Region==1)villagePeople.AddComponent<VillageTower>().Initialize(this);
            if(World.Region==1){int house=0;foreach(int x in new[]{9,42})foreach(int z in new[]{8,25,43}){var lift=new GameObject("House "+(++house)+" sky elevator");lift.transform.SetParent(villagePeople.transform);lift.AddComponent<SkyHouse>().Initialize(this,new Vector3(x,16,z));}}
            RefreshForestFriendPresence();
            foreach(var light in FindObjectsByType<Light>())if(light.type==LightType.Directional){light.intensity=World.Region==3?.5f:.85f;light.color=World.Region==3?new Color(.72f,.65f,1):new Color(1,.95f,.86f);}
            RenderSettings.fogColor=ActiveSkyColor;Player.View.backgroundColor=ActiveSkyColor;
            RefreshAdventure();
            RefreshActivities();
            BuildConnectedScenery();
            RestoreFallenResidents();
            BuildFestival();
            BuildPalaceLeisure();BuildRegionalEncounters(villagePeople.transform);
            BuildStoryCafes();
            BuildExcursionAttraction();
            BuildExcursionSides();
            BuildExcursionScenery();
            BuildExcursionWeather();
            CloseInvitationSticker();SyncInvitedChildren(true);
            BuildCommunityExpansion();
            BuildSecretRooms();
            BuildFloodResidents();
            EnsureCameraKit();BuildInstalledCameras();
            BuildHomeParty();SetDaytime(!IsNight);
            BuildMobility();
            RestoreNpcCombatViews();BuildTransparentLegacyHomes();
        }
        void DrawTravel(float w,float h)
        {
            float x=(w-900)/2,y=(h-594)/2;
            Panel(new Rect(x,y,900,594),Paper);Panel(new Rect(x,y,900,6),Gold);
            Label(new Rect(x+30,y+23,820,40),"去远方  /  旅行地图",29,Ink);
            if(FlatButton(new Rect(x+548,y+28,148,34),"经典地区",false))excursionTab=false;
            if(FlatButton(new Rect(x+710,y+28,166,34),"新增游玩景点",false))excursionTab=true;
            if(excursionTab)
            {
                Label(new Rect(x+30,y+72,820,30),"六个地方：游玩景点与下界合金矿境。按 1–6 或点卡片出发。",16,Muted);
                for(int n=0;n<6;n++)
                {
                    int id=11+n;Rect card=new Rect(x+20+(n%3)*286,y+112+(n/3)*150,274,138);
                    Panel(card,n==0?new Color(.93f,.82f,.94f):n==1?new Color(.84f,.93f,.96f):n==2?new Color(.84f,.88f,.98f):n==3?new Color(.83f,.92f,.83f):new Color(.89f,.94f,1));
                    Panel(new Rect(card.x,card.y,7,138),n==0?new Color(.76f,.29f,.66f):n==1?new Color(.18f,.59f,.75f):n==2?new Color(.31f,.52f,.87f):n==3?new Color(.25f,.58f,.34f):new Color(.43f,.63f,.82f));
                    Label(new Rect(card.x+14,card.y+8,246,27),(n+1)+" · "+VoxelWorld.RegionNames[id]+(World.Region==id?" 〔当前〕":""),18,Ink);
                    Label(new Rect(card.x+14,card.y+40,246,45),VoxelWorld.RegionDescriptions[id],14,Muted);
                    if(FlatButton(new Rect(card.x+14,card.y+96,246,30),World.Region==id?"回到景点入口":"去玩这个景点",false)){TravelTo(id);return;}
                }
                var message=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};message.normal.textColor=Muted;
                GUI.Label(new Rect(x+30,y+445,825,90),"彩虹、海浪、云风、萤火与雪道各有不同的挑战。靠近景点中心按 G 开始；每次完整玩完都有金币。",message);
                GUI.Label(new Rect(x+30,y+542,825,34),Notice,message);
                return;
            }
            Label(new Rect(x+30,y+72,820,30),"按 1 / 2 / 3 / 5 / 6 / 7 旅行。梦影地仍须从实物门进入。",16,Muted);
            for(int i=0;i<6;i++)
            {
                Rect card=new Rect(x+20+(i%3)*286,y+112+(i/3)*150,274,138);
                Panel(card,new Color(.82f,.84f,.76f));
                Color color=i==0?new Color(.3f,.55f,.28f):i==1?new Color(.76f,.51f,.3f):i==2?new Color(.23f,.7f,.75f):new Color(.48f,.29f,.65f);
                Panel(new Rect(card.x,card.y,7,114),color);
                Label(new Rect(card.x+14,card.y+9,246,27),(i+1)+" · "+VoxelWorld.RegionNames[i]+(World.Region==i?" 〔当前〕":""),18,Ink);
                Label(new Rect(card.x+14,card.y+39,246,42),VoxelWorld.RegionDescriptions[i],13,Muted);
                if(FlatButton(new Rect(card.x+14,card.y+94,246,29),i==3?"必须从传送门进入":i==1?"进入 / 刷新房屋、猪羊、麦田":World.Region==i?"回到此地区入口":"出发探索",false)){TravelTo(i);return;}
            }
            // The seventh card is centred on the final row so the castle is visually distinct.
            int c=6;Rect castle=new Rect(x+313,y+412,274,114);Panel(castle,new Color(.82f,.84f,.76f));Panel(new Rect(castle.x,castle.y,7,114),new Color(.76f,.54f,.18f));Label(new Rect(castle.x+14,castle.y+9,246,27),"7 · "+VoxelWorld.RegionNames[c]+(World.Region==c?" 〔当前〕":""),18,Ink);Label(new Rect(castle.x+14,castle.y+39,246,30),VoxelWorld.RegionDescriptions[c],13,Muted);if(FlatButton(new Rect(castle.x+14,castle.y+76,246,29),World.Region==c?"回到此地区入口":"出发探索",false)){TravelTo(c);return;}
            var tip=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};tip.normal.textColor=Muted;GUI.Label(new Rect(x+30,y+530,746,28),Notice,tip);
            for(int d=7;d<=10;d++){bool enabled=GUI.enabled;GUI.enabled=d<=DeepestVisit;if(FlatButton(new Rect(x+20+(d-7)*215,y+556,205,26),(d==9?"0":(d+1).ToString())+" · "+VoxelWorld.RegionNames[d]+(d>DeepestVisit?"（待下挖发现）":""),false)){TravelTo(d);return;}GUI.enabled=enabled;}
        }
    }
    public class Villager:MonoBehaviour
    {
        Vector3 origin;int index;Transform leftLeg,rightLeg;Material skin,cloth,dark,metal;Sandbox game;CharacterController mover;Transform knife;
        public int ResidentId=>index;
        public void RestoreUnderground(Vector3 p){mover.enabled=false;transform.position=p;origin=p;mover.enabled=true;}
        public bool IsFriend {get;private set;} public bool OnTower {get;private set;} public bool TowerReady {get;private set;}
        BoxCollider towerCollider;float chatAt;
        public int RepelHits {get;private set;}public float RetreatUntil {get;private set;}
        public void CrystalRecoil(){RetreatUntil=Mathf.Max(RetreatUntil,Time.time+.6f);Vector3 d=transform.position-game.Player.transform.position;d.y=0;if(mover.enabled&&d.sqrMagnitude>.01f)mover.Move(d.normalized*.8f);}
        public void RegisterHit(){if(IsFriend||FloodResident.IsDown(transform)||game.Paused||Time.time<RetreatUntil)return;game.AlertVillage();RepelHits++;game.Notice="村民退让："+RepelHits+" / 10";if(RepelHits<10)return;Vector3 far=origin;float best=-1;foreach(int x in new[]{9,42})foreach(int z in new[]{8,25,43}){Vector3 p=new Vector3(x+4.5f,15,z-1);float d=(p-game.Player.transform.position).sqrMagnitude;if(d>best){best=d;far=p;}}mover.enabled=false;towerCollider.enabled=false;OnTower=TowerReady=false;var tower=transform.parent.GetComponent<VillageTower>();if(tower!=null)tower.BreakFormation();transform.position=far;origin=far;mover.enabled=true;RepelHits=0;RetreatUntil=Time.time+10;game.Mutter();}
        public void Initialize(int i,Sandbox owner,bool friend=false)
        {
            game=owner;index=i;origin=transform.position;IsFriend=friend;
            mover=gameObject.AddComponent<CharacterController>();mover.height=2.1f;mover.center=Vector3.up*1.05f;mover.radius=.3f;mover.stepOffset=1.05f;
            skin=Mat(new Color(.78f,.57f,.4f));cloth=Mat(Color.HSVToRGB(.08f+i*.13f,.45f,.65f));dark=Mat(new Color(.17f,.13f,.13f));
            Cube("body",new Vector3(0,1.02f,0),new Vector3(.68f,1.05f,.4f),cloth);
            Cube("head",new Vector3(0,1.75f,0),new Vector3(.55f,.55f,.5f),skin);
            Cube("hair",new Vector3(0,2.02f,-.025f),new Vector3(.59f,.14f,.54f),dark);
            var white=Mat(new Color(.94f,.95f,.88f));foreach(float x in new[]{-.14f,.14f}){Cube("eye white",new Vector3(x,1.83f,.267f),new Vector3(.13f,.085f,.025f),white);Cube("eye",new Vector3(x,1.83f,.283f),new Vector3(.06f,.07f,.025f),dark);Cube("eyebrow",new Vector3(x,1.92f,.272f),new Vector3(.14f,.025f,.025f),dark);}
            Cube("nose",new Vector3(0,1.69f,.33f),new Vector3(.13f,.23f,.18f),skin);
            Cube("smile",new Vector3(0,1.6f,.258f),new Vector3(.17f,.035f,.02f),dark);
            foreach(float x in new[]{-.44f,.44f})Cube("arm",new Vector3(x,1.02f,0),new Vector3(.19f,.73f,.24f),skin);
            leftLeg=Cube("left leg",new Vector3(-.18f,.37f,0),new Vector3(.24f,.72f,.28f),dark);
            rightLeg=Cube("right leg",new Vector3(.18f,.37f,0),new Vector3(.24f,.72f,.28f),dark);
            metal=Mat(new Color(.64f,.7f,.73f));knife=Cube("villager blade",new Vector3(.44f,.72f,.38f),new Vector3(.12f,.07f,.65f),metal);knife.gameObject.SetActive(false);
            towerCollider=gameObject.AddComponent<BoxCollider>();towerCollider.center=Vector3.up*1.05f;towerCollider.size=new Vector3(.6f,2.1f,.5f);towerCollider.enabled=false;
        }
        Material Mat(Color c){var m=new Material(Resources.Load<Shader>("Selection"));m.color=c;return m;}
        Transform Cube(string title,Vector3 p,Vector3 s,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=title;g.transform.SetParent(transform,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;Destroy(g.GetComponent<Collider>());return g.transform;}
        void Update()
        {
            if(game==null||game.Paused)return;
            if(FloodResident.IsDown(transform))return;
            if(DronePursuit.Active(transform))return;
            if(transform.position.y<VoxelWorld.MinY-5)return;
            if(Time.time<RetreatUntil){knife.gameObject.SetActive(false);mover.SimpleMove(Vector3.zero);return;}
            if(IsFriend&&Vector3.Distance(transform.position,game.Player.transform.position)<4&&Input.GetKeyDown(KeyCode.G)){game.Notice=Time.time<chatAt?"小禾：我就在小屋等你，慢慢探索，不着急。":"小禾：欢迎回来！记得带面包和铁桶，旅途愉快！";chatAt=Time.time+12;}
            bool angry=!IsFriend&&Time.time<game.VillageAngryUntil;knife.gameObject.SetActive(angry);
            var tower=transform.parent.GetComponent<VillageTower>();
            if(tower!=null&&ClimbTower(tower,angry))return;
            float t=Time.time*.4f+index;Vector3 target=angry?game.Player.transform.position:origin+new Vector3(Mathf.Sin(t)*1.4f,0,Mathf.Cos(t)*1.4f);
            if(angry)foreach(int x in new[]{9,42})foreach(int z in new[]{8,25,43})
                if(target.x>x&&target.x<x+10&&target.z>z&&target.z<z+10&&Vector3.Distance(transform.position,new Vector3(x+4.5f,transform.position.y,z-.6f))>2.5f&&!(transform.position.x>x&&transform.position.x<x+10&&transform.position.z>z&&transform.position.z<z+10))target=new Vector3(x+4.5f,15,z-.6f);
            Vector3 direction=target-transform.position;direction.y=0;if(direction.magnitude>.2f){mover.SimpleMove(direction.normalized*(angry?3.8f:1.1f));transform.rotation=Quaternion.LookRotation(direction);}else mover.SimpleMove(Vector3.zero);
            leftLeg.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*(angry?9:4)+index)*20,0,0);rightLeg.localRotation=Quaternion.Euler(-Mathf.Sin(Time.time*(angry?9:4)+index)*20,0,0);
            if(angry&&game.CanReachPlayer(transform.position+Vector3.up,1.7f))game.Damage(.5f,"被村民追上",Vector3.Distance(transform.position,game.Player.transform.position));
        }
        bool ClimbTower(VillageTower tower,bool angry)
        {
            if(!tower.Active){
                TowerReady=false;if(!OnTower)return false;
                Vector3 down=tower.Anchor+new Vector3(Mathf.Sin(index*1.3f),0,Mathf.Cos(index*1.3f))*2.6f;transform.position=Vector3.MoveTowards(transform.position,down,Time.deltaTime*4);
                if(Vector3.Distance(transform.position,down)<.03f){OnTower=false;towerCollider.enabled=false;mover.enabled=true;}return true;
            }
            Vector3 goal=tower.Anchor+Vector3.up*(index*2.12f);
            float flat=Vector2.Distance(new Vector2(transform.position.x,transform.position.z),new Vector2(goal.x,goal.z));
            bool supported=index==0||tower.Members[index-1].TowerReady;
            if(index>0&&!supported&&!OnTower){Vector3 wait=tower.Anchor+new Vector3(Mathf.Sin(index*1.3f),0,Mathf.Cos(index*1.3f))*3;Vector3 d=wait-transform.position;d.y=0;mover.SimpleMove(d.magnitude>.2f?d.normalized*3:Vector3.zero);TowerReady=false;return true;}
            if(index>0&&supported&&flat<2.3f&&!OnTower){OnTower=true;mover.enabled=false;towerCollider.enabled=true;}
            if(OnTower){transform.position=Vector3.MoveTowards(transform.position,goal,Time.deltaTime*3);TowerReady=Vector3.Distance(transform.position,goal)<.08f;}
            else {Vector3 d=tower.Anchor-transform.position;d.y=0;mover.SimpleMove(d.magnitude>.25f?d.normalized*3.8f:Vector3.zero);TowerReady=index==0&&flat<.55f;}
            Vector3 look=game.Player.transform.position-transform.position;look.y=0;if(look.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(look);
            if(angry&&game.CanReachPlayer(transform.position+Vector3.up,1.8f))game.Damage(.5f,"被搭人梯的村民追上",Vector3.Distance(transform.position,game.Player.transform.position));
            return true;
        }
        void OnDestroy(){Destroy(skin);Destroy(cloth);Destroy(dark);Destroy(metal);}
    }
}
