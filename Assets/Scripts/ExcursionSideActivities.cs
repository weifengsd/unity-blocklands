using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public enum ExcursionSideKind
    {
        RainbowSlide,RainbowCarousel,RainbowTea,BeachSandcastle,BeachSurf,
        CloudSlide,KiteWand,RainHut,ForestLanterns,ForestStory,SnowSlide,Snowman,SnowCocoa
    }

    public sealed class ExcursionSideActivity : MonoBehaviour
    {
        static readonly string[] Titles={"彩虹长滑梯","彩虹旋转木马","彩虹交友茶会","海边沙堡工坊","追浪冲浪板","云朵滑滑梯","星风魔法棒","听雨树屋","萤火点灯小路","月下交友故事","积雪长滑道","雪人伙伴","热可可雪棚"};
        static readonly string[] Hints={
            "G上滑梯，沿彩虹滑道滑到底。","G坐木马，绕游乐场转一圈。","连续G：打招呼、倒果汁、分享点心。",
            "连续G：堆底座、盖塔顶、点缀贝壳。","G踩上冲浪板，沿海浪滑行。","G从云台滑下来。",
            "连续G：收三颗风之星，让魔法棒亮起来。","G躲进树屋雨棚，听雨一会儿。","连续G，沿小路点亮四盏萤火灯。",
            "连续G，和林间新朋友聊三句。","G坐雪橇，从真正的积雪坡滑下。","连续G：滚雪球、叠身体、装眼睛和帽子。","G在雪棚喝杯热可可，暖和一会儿。"
        };
        static readonly int[] Coins={24,30,18,22,32,28,26,18,27,18,32,22,16};
        Sandbox game;RoyalArt art;Transform spinner;readonly List<Transform> growing=new List<Transform>();
        Vector3 entry;float entryYaw,entryPitch,age,duration,readyAt;bool entryFlying,running;int stage;
        public ExcursionSideKind Kind {get;private set;}
        public string Title=>Titles[(int)Kind];
        public string Hint=>Hints[(int)Kind];
        public int Reward=>Coins[(int)Kind];
        public int Finished {get;private set;}
        public bool Running=>running;
        public int Stage=>stage;

        bool Ride=>Kind==ExcursionSideKind.RainbowSlide||Kind==ExcursionSideKind.RainbowCarousel||Kind==ExcursionSideKind.BeachSurf||Kind==ExcursionSideKind.CloudSlide||Kind==ExcursionSideKind.SnowSlide;
        bool TimedRest=>Kind==ExcursionSideKind.RainHut||Kind==ExcursionSideKind.SnowCocoa;
        int Stages=>Kind==ExcursionSideKind.ForestLanterns?4:3;

        public void Initialize(Sandbox owner,ExcursionSideKind kind,Vector3 at)
        {
            game=owner;Kind=kind;transform.position=at;art=gameObject.AddComponent<RoyalArt>();
            Color pink=new Color(.95f,.57f,.76f),gold=new Color(1,.8f,.38f),blue=new Color(.52f,.79f,.96f),wood=new Color(.55f,.34f,.2f),snow=new Color(.96f,.98f,1),green=new Color(.35f,.64f,.4f);
            art.Piece("景点平台",new Vector3(0,.08f,0),new Vector3(4.6f,.16f,4.6f),kind==ExcursionSideKind.SnowSlide||kind==ExcursionSideKind.Snowman||kind==ExcursionSideKind.SnowCocoa?snow:kind==ExcursionSideKind.RainHut||kind==ExcursionSideKind.ForestLanterns||kind==ExcursionSideKind.ForestStory?wood:kind==ExcursionSideKind.BeachSandcastle||kind==ExcursionSideKind.BeachSurf?new Color(.93f,.79f,.56f):pink,PrimitiveType.Cylinder);
            switch(kind)
            {
                case ExcursionSideKind.RainbowSlide:
                case ExcursionSideKind.CloudSlide:
                case ExcursionSideKind.SnowSlide:
                    Color slideColor=kind==ExcursionSideKind.SnowSlide?snow:kind==ExcursionSideKind.CloudSlide?blue:pink;
                    var slope=art.Piece("可以滑下的长滑道",new Vector3(0,1.85f,1.4f),new Vector3(1.85f,.18f,7.8f),slideColor);slope.localRotation=Quaternion.Euler(-22,0,0);
                    foreach(float x in new[]{-1f,1f}){var rail=art.Piece("安全扶手",new Vector3(x,2.15f,1.4f),new Vector3(.13f,.42f,7.7f),kind==ExcursionSideKind.SnowSlide?blue:gold);rail.localRotation=slope.localRotation;}
                    for(int i=0;i<4;i++)art.Piece("上滑梯台阶",new Vector3(-1.45f,.4f+i*.55f,-2.1f+i*.52f),new Vector3(1,.18f,.55f),wood);
                    art.Piece("滑梯顶端观景台",new Vector3(0,4,-2.35f),new Vector3(3.1f,.24f,2.1f),slideColor);
                    foreach(float x in new[]{-1.3f,1.3f})art.Piece("顶端平台支柱",new Vector3(x,1.95f,-2.35f),new Vector3(.18f,3.9f,.18f),kind==ExcursionSideKind.SnowSlide?snow:wood);
                    if(kind==ExcursionSideKind.SnowSlide)for(int i=0;i<8;i++)art.Piece("坡道积雪",new Vector3((i%2==0?-1:1)*1.28f,.25f,2+i*.65f),new Vector3(.65f,.35f,.85f),snow,PrimitiveType.Sphere);
                    break;
                case ExcursionSideKind.RainbowCarousel:
                    spinner=new GameObject("旋转木马").transform;spinner.SetParent(transform,false);spinner.localPosition=new Vector3(0,.44f,0);
                    art.Piece("旋转圆台",new Vector3(0,.44f,0),new Vector3(4.2f,.22f,4.2f),pink,PrimitiveType.Cylinder).SetParent(spinner,true);
                    for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Vector3 p=new Vector3(Mathf.Sin(a)*1.6f,1.2f,Mathf.Cos(a)*1.6f);art.Piece("木马",p,new Vector3(.6f,.52f,1),Color.HSVToRGB(i/6f,.47f,1),PrimitiveType.Sphere).SetParent(spinner,true);art.Piece("木马立柱",new Vector3(p.x,1.9f,p.z),new Vector3(.08f,2.5f,.08f),gold).SetParent(spinner,true);}
                    art.Taper("木马彩顶",new Vector3(0,3,0),new Vector3(4.6f,.7f,4.6f),blue,.13f);
                    break;
                case ExcursionSideKind.RainbowTea:
                case ExcursionSideKind.SnowCocoa:
                case ExcursionSideKind.RainHut:
                    art.Piece("雨棚左柱",new Vector3(-2,1.6f,0),new Vector3(.15f,3.2f,.15f),wood);
                    art.Piece("雨棚右柱",new Vector3(2,1.6f,0),new Vector3(.15f,3.2f,.15f),wood);
                    art.Piece("遮雨大棚顶",new Vector3(0,3.3f,0),new Vector3(4.8f,.26f,4.8f),kind==ExcursionSideKind.SnowCocoa?snow:kind==ExcursionSideKind.RainHut?green:pink);
                    art.Piece("小圆桌",new Vector3(0,.75f,.2f),new Vector3(2.4f,.2f,1.45f),wood,PrimitiveType.Cylinder);
                    foreach(float x in new[]{-.75f,0,.75f})art.Piece(kind==ExcursionSideKind.RainbowTea?"彩虹果汁杯":"热可可杯",new Vector3(x,1,.2f),new Vector3(.3f,.42f,.3f),kind==ExcursionSideKind.SnowCocoa?new Color(.63f,.3f,.18f):pink,PrimitiveType.Cylinder);
                    if(kind==ExcursionSideKind.RainbowTea){var friend=RoyalArt.Girl(transform,true,true);friend.transform.localPosition=new Vector3(1.6f,0,1.3f);friend.gameObject.name="茶会新朋友";for(int i=0;i<3;i++)Grow("分享的小点心",new Vector3((i-1)*.65f,1.2f,.35f),Vector3.one*.23f,gold);}
                    break;
                case ExcursionSideKind.BeachSandcastle:
                    art.Piece("常驻沙堡基座",new Vector3(0,.35f,.3f),new Vector3(1.8f,.48f,1.3f),new Color(.85f,.68f,.44f));
                    foreach(float x in new[]{-1.25f,1.25f})art.Taper("小沙塔",new Vector3(x,.7f,.3f),new Vector3(.58f,1,.58f),new Color(.89f,.73f,.48f),.45f);
                    for(int i=0;i<3;i++)Grow(i==0?"沙堡底座":i==1?"沙堡尖塔":"沙堡贝壳门",new Vector3((i-1)*.75f,.45f+i*.35f,0),new Vector3(i==0?2.2f:.65f,i==0?.65f:1.25f,.7f),i==2?pink:new Color(.9f,.75f,.49f));
                    for(int i=0;i<5;i++)art.Piece("海贝壳",new Vector3((i-2)*.72f,.3f,1.8f),Vector3.one*.3f,Color.HSVToRGB(i*.15f,.4f,1),PrimitiveType.Sphere);
                    break;
                case ExcursionSideKind.BeachSurf:
                    art.Piece("长冲浪板",new Vector3(0,.38f,0),new Vector3(1,.15f,3.9f),pink);for(int i=0;i<4;i++)art.Piece("卷起来的海浪",new Vector3((i-1.5f)*1.25f,.43f,1.6f+i*.5f),new Vector3(1.2f,.68f,.6f),blue,PrimitiveType.Sphere);
                    break;
                case ExcursionSideKind.KiteWand:
                    art.Piece("魔法棒蓝杆",new Vector3(0,1.45f,.4f),new Vector3(.15f,2.45f,.15f),blue);
                    art.Piece("魔法棒爱心头",new Vector3(0,2.8f,.4f),new Vector3(.7f,.55f,.26f),pink,PrimitiveType.Sphere);
                    for(int i=0;i<3;i++)Grow("逐颗点亮的风之星",new Vector3((i-1)*.9f,2.1f,-.3f),Vector3.one*.45f,gold);
                    break;
                case ExcursionSideKind.ForestLanterns:
                    for(int i=0;i<4;i++){art.Piece("萤火灯柱",new Vector3((i-1.5f)*1.1f,1,1.1f),new Vector3(.12f,2,.12f),wood);Grow("依次点亮的灯",new Vector3((i-1.5f)*1.1f,2.1f,1.1f),Vector3.one*.5f,new Color(1,.9f,.48f));}
                    break;
                case ExcursionSideKind.ForestStory:
                    var newFriend=RoyalArt.Girl(transform,true,false);newFriend.transform.localPosition=new Vector3(1.35f,0,.9f);newFriend.gameObject.name="萤火秘林新朋友";
                    art.Piece("故事树桩",new Vector3(-.6f,.5f,.2f),new Vector3(1.4f,1,1.4f),wood,PrimitiveType.Cylinder);
                    foreach(float x in new[]{-1.4f,-.7f,0})art.Piece("故事花朵",new Vector3(x,1.1f,-.3f),Vector3.one*.24f,pink,PrimitiveType.Sphere);
                    break;
                case ExcursionSideKind.Snowman:
                    art.Piece("雪人伙伴的起始小雪球",new Vector3(0,.38f,.2f),new Vector3(.85f,.75f,.85f),snow,PrimitiveType.Sphere);
                    Grow("雪人身体",new Vector3(0,.83f,.2f),Vector3.one*1.4f,snow);
                    Grow("雪人脑袋",new Vector3(0,1.95f,.2f),Vector3.one*.93f,snow);
                    Grow("雪人帽子",new Vector3(0,2.52f,.2f),new Vector3(1,.26f,1),blue);
                    foreach(float x in new[]{-.2f,.2f})art.Piece("雪人眼睛",new Vector3(x,2.03f,.66f),Vector3.one*.1f,new Color(.18f,.2f,.29f),PrimitiveType.Sphere);
                    break;
            }
            if(kind!=ExcursionSideKind.RainbowTea&&kind!=ExcursionSideKind.ForestStory){var guide=RoyalArt.Girl(transform,true,false);guide.transform.localPosition=new Vector3(-2.8f,0,-1.45f);guide.gameObject.name=Title+"小向导";}
            var sign=new GameObject(Title+"玩法牌");sign.transform.SetParent(transform,false);sign.transform.localPosition=new Vector3(0,3.3f,-1.8f);var t=sign.AddComponent<TextMesh>();t.font=Resources.Load<Font>("NotoSansCJKsc-Regular");t.GetComponent<Renderer>().sharedMaterial=t.font.material;t.text=Title+"\nG 体验";t.fontSize=32;t.characterSize=.11f;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;
        }
        void Grow(string title,Vector3 p,Vector3 size,Color color){var t=art.Piece(title,p,size,color,PrimitiveType.Sphere);t.gameObject.SetActive(false);growing.Add(t);}
        public bool TryG()
        {
            if(game==null||game.Paused||Vector3.Distance(game.Player.transform.position,transform.position)>4.5f)return false;
            if(running){game.Notice=Title+"正在进行，先玩完或按Esc退出。";return true;}
            if(Ride||TimedRest){BeginTimed();return true;}
            if(Time.time<readyAt){game.Notice=Title+"：稍等一下，准备好再按G。";return true;}
            stage++;readyAt=Time.time+.65f;
            if(growing.Count>=stage)growing[stage-1].gameObject.SetActive(true);
            game.Notice=Title+"："+(Kind==ExcursionSideKind.RainbowTea?new[]{"先向新朋友打招呼。","给朋友倒杯彩虹果汁。","大家一起分享点心！"}[stage-1]:Kind==ExcursionSideKind.ForestStory?new[]{"朋友说：雨后萤火虫会醒来。","你们一起听树叶沙沙作响。","你和新朋友约好下次再来。"}[stage-1]:Hint+"  第"+stage+"步。");
            if(stage>=Stages)Finish();return true;
        }
        void BeginTimed()
        {
            entry=game.Player.transform.position;entryYaw=game.Player.transform.eulerAngles.y;entryPitch=game.Player.Pitch;entryFlying=game.Player.Flying;
            running=true;age=0;duration=Kind==ExcursionSideKind.RainbowCarousel?12:Kind==ExcursionSideKind.BeachSurf?11:Kind==ExcursionSideKind.SnowSlide?10:Kind==ExcursionSideKind.RainHut?8:Kind==ExcursionSideKind.SnowCocoa?9:8;
            if(TimedRest)game.Player.Teleport(transform.position+Vector3.back*.5f,entryYaw,entryPitch);
            game.Notice=Title+"开始啦！"+Hint+"；要提前退出按Esc。";
        }
        public void Tick(float dt)
        {
            if(!running||game.Paused)return;age+=Mathf.Max(0,dt);
            if(Ride){float u=Mathf.Clamp01(age/duration);Vector3 p=transform.position;
                if(Kind==ExcursionSideKind.RainbowCarousel){float a=u*Mathf.PI*4;p+=new Vector3(Mathf.Sin(a)*1.5f,.55f,Mathf.Cos(a)*1.5f);}
                else if(Kind==ExcursionSideKind.BeachSurf)p+=new Vector3(Mathf.Sin(u*Mathf.PI*3)*1.3f,.18f+Mathf.Sin(u*Mathf.PI*8)*.2f,u*9-3);
                else {float height=Kind==ExcursionSideKind.SnowSlide?3.3f:4.1f;p+=new Vector3(0,height*(1-u)+.15f,u*8-3);}
                game.Player.Teleport(p,entryYaw,Mathf.Lerp(-7,3,u));
            }
            if(age>=duration)Finish();
        }
        public void Cancel(){if(!running)return;running=false;RestorePlayer();game.Notice=Title+"先休息，之后靠近按G可以再玩。";}
        void RestorePlayer(){if(!Ride&&!TimedRest)return;game.Player.Teleport(entry,entryYaw,entryPitch);game.Player.SetFlying(entryFlying);}
        void Finish()
        {
            if(running){running=false;RestorePlayer();}
            int coins=game.EarnActivityCoins(Reward,Title);Finished++;stage=0;readyAt=0;
            if(Kind==ExcursionSideKind.SnowCocoa||Kind==ExcursionSideKind.RainHut)game.HealRoyal(1);
            game.Notice=Title+"完成！获得"+coins+"元。想玩时再按G，下一次完成也有奖励。";
            foreach(var part in growing)part.gameObject.SetActive(false);
        }
        void Update(){if(spinner!=null&&!game.Paused)spinner.localRotation=Quaternion.Euler(0,Time.time*25,0);}
        void OnGUI()
        {
            if(game==null||game.Paused||!running&&Vector3.Distance(game.Player.transform.position,transform.position)>7)return;
            float scale=Mathf.Min(Screen.width/1000f,Screen.height/650f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
            var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("NotoSansCJKsc-Regular"),fontSize=15,wordWrap=true,alignment=TextAnchor.MiddleCenter};style.normal.textColor=Color.white;
            float w=Screen.width/scale,h=Screen.height/scale;
            if(running){Sandbox.Panel(new Rect(w/2-275,18,550,76),new Color(.14f,.2f,.28f,.92f));GUI.Label(new Rect(w/2-263,26,526,61),Title+" · 还剩"+Mathf.CeilToInt(Mathf.Max(0,duration-age))+"秒\n"+Hint+"  ·  Esc退出",style);}
            else{Sandbox.Panel(new Rect(w/2-265,h-171,530,65),new Color(.14f,.2f,.28f,.88f));GUI.Label(new Rect(w/2-253,h-164,506,52),Title+" · G体验 · 完成得"+Reward+"元\n"+Hint,style);}
        }
    }

    public partial class Sandbox
    {
        readonly List<ExcursionSideActivity> excursionSides=new List<ExcursionSideActivity>();
        public IReadOnlyList<ExcursionSideActivity> ExcursionSides=>excursionSides;
        public ExcursionSideActivity ActiveSideRide {get;private set;}
        public bool SideRideActive=>ActiveSideRide!=null&&ActiveSideRide.Running;
        void BuildExcursionSides()
        {
            excursionSides.Clear();ActiveSideRide=null;if(World.Region<11||World.Region>15)return;
            void Add(ExcursionSideKind kind,Vector3 at){var o=new GameObject(kind.ToString());o.transform.SetParent(villagePeople.transform);var side=o.AddComponent<ExcursionSideActivity>();side.Initialize(this,kind,at);excursionSides.Add(side);}
            int r=World.Region;
            if(r==11){Add(ExcursionSideKind.RainbowSlide,new Vector3(18,15,37));Add(ExcursionSideKind.RainbowCarousel,new Vector3(47,15,37));Add(ExcursionSideKind.RainbowTea,new Vector3(32,15,50));}
            if(r==12){Add(ExcursionSideKind.BeachSandcastle,new Vector3(18,15,37));Add(ExcursionSideKind.BeachSurf,new Vector3(47,15,37));}
            if(r==13){Add(ExcursionSideKind.CloudSlide,new Vector3(18,15,37));Add(ExcursionSideKind.KiteWand,new Vector3(47,15,37));}
            if(r==14){Add(ExcursionSideKind.RainHut,new Vector3(18,15,37));Add(ExcursionSideKind.ForestLanterns,new Vector3(47,15,37));Add(ExcursionSideKind.ForestStory,new Vector3(32,15,50));}
            if(r==15){Add(ExcursionSideKind.SnowSlide,new Vector3(18,15,39));Add(ExcursionSideKind.Snowman,new Vector3(47,15,37));Add(ExcursionSideKind.SnowCocoa,new Vector3(32,15,50));}
            var board=new GameObject("景点导览牌");board.transform.SetParent(villagePeople.transform);board.transform.position=new Vector3(31.5f,15,20);
            var art=board.AddComponent<RoyalArt>();Color edge=new Color(.83f,.66f,.39f),paper=new Color(.18f,.3f,.41f);
            foreach(float x in new[]{-2.25f,2.25f})art.Piece("导览牌支柱",new Vector3(x,1.65f,0),new Vector3(.16f,3.3f,.16f),edge);
            art.Piece("地图大面板",new Vector3(0,2.25f,0),new Vector3(4.8f,2.6f,.16f),paper);
            var label=new GameObject("导览文字");label.transform.SetParent(board.transform,false);label.transform.localPosition=new Vector3(0,2.25f,-.11f);
            var mesh=label.AddComponent<TextMesh>();mesh.font=Resources.Load<Font>("NotoSansCJKsc-Regular");mesh.GetComponent<Renderer>().sharedMaterial=mesh.font.material;
            mesh.fontSize=32;mesh.characterSize=.07f;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=Color.white;
            mesh.text=VoxelWorld.RegionNames[r]+"\n← "+excursionSides[0].Title+" · G体验\n中央 "+Excursion.Title+" · G开始\n→ "+excursionSides[1].Title+" · G体验"+(excursionSides.Count>2?"\n前方 "+excursionSides[2].Title+" · G体验":"");
        }
        void ResetExcursionSides(){if(ActiveSideRide!=null)ActiveSideRide.Cancel();ActiveSideRide=null;excursionSides.Clear();}
        public bool TryExcursionSideG()
        {
            ExcursionSideActivity nearest=null;float best=4.5f;
            foreach(var side in excursionSides){if(side==null)continue;float distance=Vector3.Distance(Player.transform.position,side.transform.position);if(distance<best){nearest=side;best=distance;}}
            if(nearest==null||!nearest.TryG())return false;
            ActiveSideRide=nearest.Running?nearest:null;return true;
        }
        void TickExcursionSideRide(){if(ActiveSideRide==null)return;if(!IsSmoke&&Input.GetKeyDown(KeyCode.Escape)){ActiveSideRide.Cancel();ActiveSideRide=null;return;}ActiveSideRide.Tick(Time.deltaTime);if(!ActiveSideRide.Running)ActiveSideRide=null;}
    }
}
