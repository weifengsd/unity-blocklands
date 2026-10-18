using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public partial class CastleSystem:MonoBehaviour
    {
        public const int Floors=10;
        public Sandbox Game {get;private set;}
        public bool ElevatorMenu {get;private set;}
        public bool Jailed {get;private set;}
        public float JailRemaining=>Jailed?Mathf.Max(0,jailUntil-Time.time):0;
        public float VisitorUntil {get;private set;}
        float breachAt=-1,jailUntil;
        readonly List<Transform> guards=new List<Transform>();readonly List<Vector3> guardHomes=new List<Vector3>();readonly List<Material> materials=new List<Material>();
        static readonly Vector3 Entrance=new Vector3(31.5f,16,15.5f),StairBase=new Vector3(19.5f,16,19.5f),LiftBase=new Vector3(43.5f,16,18.5f),Cell=new Vector3(58.5f,16.2f,50.5f);
        static Vector3Int[] GateCells=>new[]{new Vector3Int(58,16,48),new Vector3Int(58,17,48)};
        Material Mat(Color c){var m=new Material(Resources.Load<Shader>("Selection"));m.color=c;materials.Add(m);return m;}
        Transform Cube(string name,Transform parent,Vector3 position,Vector3 size,Material mat)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent,false);o.transform.position=position;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;Destroy(o.GetComponent<Collider>());return o.transform;
        }
        public static float FloorY(int floor)=>16+Mathf.Clamp(floor,0,Floors-1)*6;
        public int FloorOf(Vector3 p)=>Mathf.Clamp(Mathf.RoundToInt((p.y-16)/6),0,Floors-1);
        public void Initialize(Sandbox game)
        {
            Game=game;var guardBlue=Mat(new Color(.17f,.28f,.58f));var skin=Mat(new Color(.78f,.57f,.4f));var hair=Mat(new Color(.12f,.1f,.12f));var gold=Mat(new Color(.95f,.72f,.18f));var crimson=Mat(new Color(.68f,.06f,.11f));var ivory=Mat(new Color(.87f,.84f,.71f));
            for(int i=0;i<3;i++){
                var root=new GameObject("城堡卫兵 "+(i+1)).transform;root.SetParent(transform);root.position=new Vector3(27+i*4,16,19+i%2*2);guards.Add(root);guardHomes.Add(root.position);
                Cube("uniform",root,root.position+Vector3.up*1.05f,new Vector3(.72f,.9f,.42f),guardBlue);Cube("head",root,root.position+Vector3.up*1.78f,new Vector3(.58f,.58f,.52f),skin);Cube("helmet",root,root.position+Vector3.up*2.08f,new Vector3(.65f,.16f,.57f),hair);Cube("helmet crest",root,root.position+Vector3.up*2.25f,new Vector3(.18f,.28f,.34f),crimson);Cube("left eye",root,root.position+new Vector3(-.13f,1.8f,.27f),new Vector3(.07f,.07f,.03f),hair);Cube("right eye",root,root.position+new Vector3(.13f,1.8f,.27f),new Vector3(.07f,.07f,.03f),hair);Cube("spear",root,root.position+new Vector3(.43f,1.05f,.12f),new Vector3(.08f,2,.08f),Mat(new Color(.72f,.72f,.77f)));
            }
            var princess=new GameObject("公主").transform;princess.SetParent(transform);princess.position=new Vector3(31.5f,FloorY(9),34.5f);princess.rotation=Quaternion.Euler(0,180,0);var pink=Mat(new Color(.96f,.42f,.7f));
            Cube("princess gown",princess,princess.position+Vector3.up*.78f,new Vector3(1.28f,1.55f,.72f),pink);Cube("princess waist ribbon",princess,princess.position+new Vector3(0,1.15f,.38f),new Vector3(1.05f,.14f,.08f),gold);Cube("princess head",princess,princess.position+Vector3.up*1.9f,new Vector3(.62f,.62f,.55f),skin);Cube("princess hair",princess,princess.position+new Vector3(0,2.1f,-.05f),new Vector3(.68f,.2f,.62f),hair);Cube("crown",princess,princess.position+Vector3.up*2.27f,new Vector3(.72f,.18f,.62f),gold);Cube("crown jewel",princess,princess.position+new Vector3(0,2.39f,.3f),new Vector3(.13f,.13f,.08f),Mat(new Color(.57f,.21f,.9f)));foreach(float x in new[]{-.14f,.14f})Cube("princess eye",princess,princess.position+new Vector3(x,1.94f,.29f),new Vector3(.07f,.08f,.03f),hair);Cube("princess smile",princess,princess.position+new Vector3(0,1.74f,.3f),new Vector3(.18f,.035f,.03f),crimson);
            for(int f=0;f<Floors;f++){
                float y=FloorY(f);var stair=new GameObject("楼梯标记 "+(f+1)).transform;stair.SetParent(transform);Cube("stair glow",stair,new Vector3(StairBase.x,y+.5f,StairBase.z),new Vector3(1.1f,1,.35f),gold);Cube("stair rail",stair,new Vector3(StairBase.x-.7f,y+1.1f,StairBase.z),new Vector3(.08f,2,.9f),gold);
                var lift=new GameObject("城堡电梯门 "+(f+1)).transform;lift.SetParent(transform);Cube("lift frame",lift,new Vector3(LiftBase.x,y+1.25f,LiftBase.z-.1f),new Vector3(2.7f,2.6f,.25f),gold);Cube("lift glass",lift,new Vector3(LiftBase.x,y+1.25f,LiftBase.z-.25f),new Vector3(2.28f,2.18f,.08f),Mat(new Color(.35f,.75f,.9f,.35f)));
                foreach(var p in new[]{new Vector3(17,y+1.2f,17),new Vector3(46,y+1.2f,17),new Vector3(17,y+1.2f,46),new Vector3(46,y+1.2f,46)})Cube("gold column",transform,p,new Vector3(.58f,2.4f,.58f),gold);
                Cube("royal banner",transform,new Vector3(24,y+1.3f,34.8f),new Vector3(2.2f,1.5f,.08f),crimson);Cube("royal banner",transform,new Vector3(39,y+1.3f,34.8f),new Vector3(2.2f,1.5f,.08f),crimson);
                Cube("room table",transform,new Vector3(27,y+.5f,26),new Vector3(2.3f,.45f,1.2f),ivory);Cube("room pedestal",transform,new Vector3(37,y+.55f,26),new Vector3(.75f,1.05f,.75f),gold);
                var light=new GameObject("castle chandelier "+(f+1)).AddComponent<Light>();light.transform.SetParent(transform);light.transform.position=new Vector3(31.5f,y+2.35f,30);light.type=LightType.Point;light.color=new Color(1,.74f,.34f);light.range=15;light.intensity=1.45f;
            }
            foreach(Transform t in transform)if(t.name.StartsWith("城堡电梯门")){t.gameObject.SetActive(false);Destroy(t.gameObject);}
            RoyalDecoration();
        }
        bool HasVisitor=>Game.RoyalIdentity||Time.time<VisitorUntil;
        bool InsidePalace(Vector3 p)=>p.x>14&&p.x<50&&p.z>14.8f&&p.z<50&&p.y>=15&&p.y<76;
        bool GateOpen()=>Game.World.Get(GateCells[0])==Block.Air&&Game.World.Get(GateCells[1])==Block.Air;
        void SetGate(bool open){foreach(var c in GateCells)Game.World.Edit(c,open?Block.Air:Block.PrisonBars,false);Game.World.RebuildAll();}
        public bool RequestVisit()
        {
            if(Vector3.Distance(Game.Player.transform.position,Entrance)>8){Game.Notice="先到城堡正门卫兵前，再按 G 请求通行。";return false;}
            VisitorUntil=Time.time+90;breachAt=-1;for(int i=0;i<guards.Count;i++)guards[i].position=guardHomes[i];Game.Notice="卫兵：访客许可已登记，90秒内可进入城堡。U上楼、I下楼、O使用电梯。";return true;
        }
        public bool NearLift=>Vector2.Distance(new Vector2(Game.Player.transform.position.x,Game.Player.transform.position.z),new Vector2(LiftBase.x,LiftBase.z))<4;
        public bool NearStairs=>Vector2.Distance(new Vector2(Game.Player.transform.position.x,Game.Player.transform.position.z),new Vector2(StairBase.x,StairBase.z))<4;
        public void ToggleElevator(){if(!NearLift){Game.Notice="先走到每层东北角的玻璃电梯门旁，再按 O。";return;}if(!royalLift.Inside){royalLift.Request(FloorOf(Game.Player.transform.position));Game.Notice="电梯正在到本层，等门打开后走进去，再按O选层。";return;}ElevatorMenu=!ElevatorMenu;Game.Notice=ElevatorMenu?"电梯：1—9层，0去10层。":"电梯面板关闭。";}
        public bool SelectElevatorFloor(int floor)
        {
            if(!ElevatorMenu||floor<0||floor>=Floors||!royalLift.Inside)return false;ElevatorMenu=false;royalLift.Request(floor);Game.Notice="玻璃门关闭后，电梯升降前往第"+(floor+1)+"层。";return true;
        }
        public bool StepFloor(int delta)
        {
            if(!NearStairs){Game.Notice="先靠近每层西北角的金色楼梯标记，再按 U 上楼或 I 下楼。";return false;}
            int from=FloorOf(Game.Player.transform.position),to=Mathf.Clamp(from+delta,0,Floors-1);if(to==from){Game.Notice=to==0?"这里已经是一层。":"这里已经是十层。";return false;}
            Game.Player.Teleport(new Vector3(StairBase.x,FloorY(to),StairBase.z+1.2f),0,0);Game.Notice="通过楼梯来到第"+(to+1)+"层。";return true;
        }
        public bool TryKeyGate(Vector3Int target)
        {
            if(Game.World.Get(target)!=Block.PrisonBars)return false;
            if(Game.Inventory[(int)Block.CastleKey]<1){Game.Notice="铁栅栏需要城堡钥匙；也可以手持水晶锤左键挖开。";return true;}
            Game.Inventory[(int)Block.CastleKey]--;SetGate(true);Jailed=false;jailUntil=0;VisitorUntil=Time.time+60;breachAt=-1;Game.Notice="城堡钥匙打开了监狱铁栅栏；卫兵给你60秒离开或继续参观。";return true;
        }
        void Jail()
        {
            if(Jailed||Game.RoyalIdentity)return;Jailed=true;ElevatorMenu=false;SetGate(false);Game.Player.Teleport(Cell,180,0);jailUntil=Time.time+30;Game.Notice="卫兵抓住了你：关进监狱30秒。城堡钥匙按H可开门，水晶锤可挖栅栏。";
        }
        void Release(){if(!Jailed)return;Jailed=false;SetGate(true);VisitorUntil=Time.time+60;breachAt=-1;Game.Notice="30秒监禁结束，卫兵打开铁栅栏，并给你60秒离开或继续参观。";}
        void Update()
        {
            if(Game==null||Game.Paused)return;
            if(Game.CarRiding){breachAt=-1;return;}
            MuseumUpdate();
            RoyalCareUpdate();TickPlayerSun();
            WelcomeAtGate();
            if(Game.RoyalIdentity&&Jailed)CrownedPrincess();
            if(Jailed){if(GateOpen()){Jailed=false;VisitorUntil=Time.time+60;breachAt=-1;Game.Notice="铁栅栏已被打开；你有60秒离开或继续参观，不会立刻再次被抓。";}else if(Time.time>=jailUntil)Release();return;}
            bool breach=InsidePalace(Game.Player.transform.position)&&!HasVisitor;
            if(!breach){breachAt=-1;return;}
            if(breachAt<0){breachAt=Time.time;Game.Notice="卫兵：请先在正门按 G 请求访客通行！";}
            foreach(var guard in guards){Vector3 p=Game.Player.transform.position;Vector3 next=Vector3.MoveTowards(guard.position,new Vector3(p.x,16,p.z),Time.deltaTime*4);guard.position=next;Vector3 look=p-guard.position;look.y=0;if(look.sqrMagnitude>.01f)guard.rotation=Quaternion.LookRotation(look);}
            if(Time.time-breachAt>=4)Jail();
        }
        void OnDestroy(){if(drinkCup!=null)Destroy(drinkCup.gameObject);if(sunStage!=0&&Game!=null&&Game.Player!=null)Game.Player.HomeRest(false);foreach(var m in materials)if(m!=null)Destroy(m);}
    }
    public partial class Sandbox
    {
        CastleSystem castle;
        public bool RoyalMenuOpen=>castle!=null&&castle.AnyLiftMenu;
        public bool CastleJailed=>castle!=null&&castle.Jailed;
        public bool IsNight {get;private set;}
        public Color ActiveSkyColor=>InHeaven?new Color(.75f,.85f,.94f):InHell?new Color(.22f,.045f,.06f):InPlanetLife?new Color(.56f,.76f,.89f):InVoidRealms?new Color(.09f,.13f,.27f):Player!=null&&Player.transform.position.y<-5?new Color(.025f,.035f,.09f):IsNight?new Color(.025f,.045f,.12f):World.SkyColor;
        public void AttachCastle(CastleSystem value)=>castle=value;
        public void SetDaytime(bool day)
        {
            IsNight=!day;foreach(var light in FindObjectsByType<Light>())if(light.type==LightType.Directional){light.intensity=day?.85f:.18f;light.color=day?new Color(1,.95f,.86f):new Color(.45f,.53f,1);}
            RenderSettings.ambientSkyColor=day?new Color(.38f,.43f,.46f):new Color(.04f,.05f,.13f);RenderSettings.ambientEquatorColor=day?new Color(.29f,.32f,.28f):new Color(.03f,.035f,.08f);RenderSettings.ambientGroundColor=day?new Color(.16f,.17f,.14f):new Color(.012f,.015f,.03f);RenderSettings.fogColor=ActiveSkyColor;Player.View.backgroundColor=ActiveSkyColor;Notice=day?"白天已开启。":"黑夜已降临。";
            if(day)StopHomeParty();else StartHomeParty();
        }
        bool CastleKeys()
        {
            if(Input.GetKeyDown(KeyCode.M)){SetDaytime(true);return true;}if(Input.GetKeyDown(KeyCode.N)){SetDaytime(false);return true;}
            if(World.Region!=6||castle==null)return false;
            if(castle.MuseumMenuKeys())return true;
            if(Input.GetKeyDown(KeyCode.L))return castle.BuyMuseumTicket();
            if(castle.Jailed){if(ShortH&&hasTarget)return castle.TryKeyGate(target);return false;}
            if(castle.ElevatorMenu){if(Input.GetKeyDown(KeyCode.Escape)){castle.ToggleElevator();return true;}for(int i=1;i<=9;i++)if(Input.GetKeyDown(KeyCode.Alpha1+i-1))return castle.SelectElevatorFloor(i-1);if(Input.GetKeyDown(KeyCode.Alpha0))return castle.SelectElevatorFloor(9);return true;}
            if(Input.GetKeyDown(KeyCode.G))return castle.RequestVisit();
            if(Input.GetKeyDown(KeyCode.R)&&castle.UseThrone())return true;
            if(ShortH&&castle.TryStartDuel())return true;
            if(ShortH&&hasTarget&&castle.TouchArtifact(target))return true;
            if(ShortH&&castle.ToggleGate())return true;
            if(Input.GetKeyDown(KeyCode.O)){if(castle.MuseumLiftKey())return true;castle.ToggleElevator();return true;}
            if(Input.GetKeyDown(KeyCode.U)){if(castle.MuseumStep(1))return true;return castle.StepFloor(1);}
            if(Input.GetKeyDown(KeyCode.I)){if(castle.MuseumStep(-1))return true;return castle.StepFloor(-1);}
            if(ShortH&&hasTarget)return castle.TryKeyGate(target);
            return false;
        }
        void DrawCastleUI(float w,float h)
        {
            if(World.Region!=6||castle==null)return;Label(new Rect(w-310,130,285,25),IsNight?"黑夜 · M 切白天":"白天 · N 切黑夜",15,Color.white,TextAnchor.MiddleRight);
            Label(new Rect(w-310,154,285,25),"城堡：G访客 · U/I楼梯 · O电梯",15,Color.white,TextAnchor.MiddleRight);
            if(!InVoidRealms)Label(new Rect(w-390,178,365,25),RoyalIdentity?"1 重新挑战公主（恢复普通身份）":"J登记参观 · 十层瞄准公主按H挑战",15,new Color(1,.85f,.65f),TextAnchor.MiddleRight);
            if(castle.ElevatorMenu){Panel(new Rect(w/2-230,h/2-120,460,240),new Color(.05f,.1f,.17f,.94f));Label(new Rect(w/2-205,h/2-98,410,30),"城堡电梯 · 选择目标楼层",23,new Color(.94f,.82f,.39f),TextAnchor.MiddleCenter);Label(new Rect(w/2-190,h/2-50,380,72),"按 1—9 前往第1—9层\n按 0 前往第10层\nEsc 关闭面板",19,Color.white,TextAnchor.MiddleCenter);}
            castle.DrawMuseumLift(w,h);castle.DrawPlayerSun(w,h);
            if(castle.Jailed)Label(new Rect(w/2-260,h-178,520,30),"监狱中：城堡钥匙 + H 开门，或水晶锤左键挖铁栅栏；否则等待释放。",16,new Color(1,.72f,.38f),TextAnchor.MiddleCenter);
        }
    }
}
