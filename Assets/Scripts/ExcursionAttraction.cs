using System;
using UnityEngine;

namespace Blocklands
{
    public enum ExcursionKind { Rainbow, Beach, Kite, Fireflies, Sled }

    public sealed class ExcursionAttraction : MonoBehaviour
    {
        static readonly string[] Titles={"彩虹跳台","潮汐寻宝","追风飞行","萤火追逐","雪橇冲刺"};
        static readonly int[] Rewards={42,54,64,48,72};
        static readonly KeyCode[] RainbowBeats={KeyCode.LeftArrow,KeyCode.Space,KeyCode.RightArrow,KeyCode.Space,KeyCode.LeftArrow,KeyCode.RightArrow,KeyCode.Space,KeyCode.RightArrow};
        static readonly int[] BeachPearls={2,0,1,2,1,0};
        static readonly int[] FireflyColors={0,2,1,3,0,1,2};
        static readonly int[] SnowHazards={1,2,0,1,2,0,1,0};
        static readonly float[] KiteHeights={.52f,.65f,.43f,.7f,.5f,.61f,.39f,.66f};
        Sandbox game;RoyalArt art;Transform spinner,boat,kite,sled;Transform[] lights;
        public ExcursionKind Kind {get;private set;}
        public bool Active {get;private set;}
        public int Progress {get;private set;}
        public int Mistakes {get;private set;}
        public int CompletedRounds {get;private set;}
        public int Lane {get;private set;}
        public float Elapsed {get;private set;}
        public float Altitude {get;private set;}
        float nextEvent;
        int beatIndex;
        Vector3 rideStart;
        float rideYaw,ridePitch;
        bool rideWasFlying;
        string localHint="靠近按 G 开始";
        public string Title=>Titles[(int)Kind];
        public int Reward=>Rewards[(int)Kind];
        public string Prompt=>Kind==ExcursionKind.Rainbow?"看屏幕提示的下一拍，只按←／→／空格；连对8拍获胜，错3次重来。":Kind==ExcursionKind.Beach?"屏幕告诉你珍珠在左／中／右；方向键换位置，到同一边按空格，捡满6颗。":Kind==ExcursionKind.Kite?"按住空格升高、松开缓降；每3秒云环经过一次，接近目标高度穿过4环。":Kind==ExcursionKind.Fireflies?"屏幕显示下一只颜色：红按1、绿按2、蓝按3、金按4；找齐7只。":"每2秒滑过一处冰晶；看提示在哪道，按←／→躲到别道，绕过8处。";

        public void Initialize(Sandbox owner,ExcursionKind kind,Vector3 at)
        {
            game=owner;Kind=kind;transform.position=at;art=gameObject.AddComponent<RoyalArt>();
            Color pink=new Color(.96f,.6f,.8f),gold=new Color(1,.82f,.42f),blue=new Color(.45f,.76f,.94f),mint=new Color(.55f,.89f,.75f);
            art.Piece("景点彩色底座",new Vector3(0,.12f,0),new Vector3(4.8f,.22f,4.8f),kind==ExcursionKind.Beach?new Color(.9f,.77f,.53f):kind==ExcursionKind.Fireflies?new Color(.35f,.57f,.41f):new Color(.78f,.82f,.94f),PrimitiveType.Cylinder);
            if(kind==ExcursionKind.Rainbow)
            {
                spinner=new GameObject("彩虹旋转舞台").transform;spinner.SetParent(transform,false);spinner.localPosition=new Vector3(0,1.05f,0);
                art.Piece("虹光圆盘",new Vector3(0,1.05f,0),new Vector3(3.8f,.22f,3.8f),gold,PrimitiveType.Cylinder).SetParent(spinner,true);
                for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;var light=art.Piece("彩虹踏点",new Vector3(Mathf.Sin(angle)*1.53f,1.25f,Mathf.Cos(angle)*1.53f),Vector3.one*.4f,Color.HSVToRGB(i/8f,.72f,1),PrimitiveType.Sphere);light.SetParent(spinner,true);}
                foreach(float x in new[]{-2.4f,2.4f}){art.Piece("游乐场灯柱",new Vector3(x,1.2f,-1.3f),new Vector3(.18f,2.4f,.18f),pink);art.Piece("星星顶灯",new Vector3(x,2.5f,-1.3f),Vector3.one*.55f,gold,PrimitiveType.Sphere);}
            }
            else if(kind==ExcursionKind.Beach)
            {
                boat=art.Piece("寻宝小船",new Vector3(0,.7f,1.15f),new Vector3(2.3f,.42f,1.25f),new Color(.54f,.32f,.2f));
                for(int i=0;i<3;i++){float x=(i-1)*1.1f;art.Piece("珍珠展示贝壳",new Vector3(x,.44f,-1.1f),new Vector3(.75f,.22f,.52f),pink,PrimitiveType.Sphere);art.Piece("圆润珍珠",new Vector3(x,.63f,-1.1f),Vector3.one*.25f,new Color(.94f,.99f,1),PrimitiveType.Sphere);}
                art.Piece("沙滩遮阳伞杆",new Vector3(2.05f,1.5f,.4f),new Vector3(.11f,2.8f,.11f),gold);art.Taper("海风遮阳伞",new Vector3(2.05f,3,.4f),new Vector3(2,.45f,2),blue,.08f);
            }
            else if(kind==ExcursionKind.Kite)
            {
                kite=art.Piece("粉蓝风筝",new Vector3(0,3.25f,.8f),new Vector3(1.3f,1.3f,.13f),pink);
                kite.localRotation=Quaternion.Euler(0,0,45);art.Piece("风筝丝线",new Vector3(0,2.25f,.5f),new Vector3(.04f,2.2f,.05f),gold);
                foreach(float x in new[]{-2f,2f}){art.Piece("云朵",new Vector3(x,1.1f,1.5f),new Vector3(1.35f,.65f,.85f),new Color(.94f,.98f,1),PrimitiveType.Sphere);art.Piece("云环",new Vector3(x,2.05f,1.5f),new Vector3(.8f,.16f,.8f),blue,PrimitiveType.Cylinder);}
            }
            else if(kind==ExcursionKind.Fireflies)
            {
                lights=new Transform[6];for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;float x=Mathf.Sin(angle)*1.65f,z=Mathf.Cos(angle)*1.65f;art.Piece("萤火树干",new Vector3(x,1.1f,z),new Vector3(.15f,2.2f,.15f),new Color(.38f,.27f,.19f));art.Piece("圆叶树冠",new Vector3(x,2.3f,z),new Vector3(.85f,.7f,.85f),mint,PrimitiveType.Sphere);lights[i]=art.Piece("漂浮萤火虫",new Vector3(x,1.7f,z-.38f),Vector3.one*.23f,Color.HSVToRGB((i%4)*.24f,.57f,1),PrimitiveType.Sphere);}
            }
            else
            {
                sled=art.Piece("红色小雪橇",new Vector3(0,.5f,0),new Vector3(1.4f,.35f,2.3f),new Color(.91f,.38f,.48f));
                for(int i=-1;i<=1;i++){art.Piece("雪道彩旗",new Vector3(i*1.55f,1.4f,1.2f),new Vector3(.09f,2.8f,.09f),blue);art.Piece("彩旗旗面",new Vector3(i*1.55f+.25f,2.55f,1.2f),new Vector3(.55f,.28f,.06f),i==0?gold:pink);}
                foreach(float x in new[]{-.6f,.6f})art.Piece("雪橇滑轨",new Vector3(x,.24f,0),new Vector3(.13f,.12f,2.55f),new Color(.63f,.85f,.98f));
            }
            var guide=RoyalArt.Girl(transform,true,false);guide.transform.localPosition=new Vector3(-2.7f,0,-1.5f);guide.gameObject.name=Title+"向导";
            var sign=new GameObject(Title+"指示牌");sign.transform.SetParent(transform,false);sign.transform.localPosition=new Vector3(0,3.8f,-1.5f);var text=sign.AddComponent<TextMesh>();text.font=Resources.Load<Font>("NotoSansCJKsc-Regular");text.GetComponent<Renderer>().sharedMaterial=text.font.material;text.text=Title+"\nG 开始";text.fontSize=32;text.characterSize=.12f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
        }

        void Update()
        {
            if(game==null||game.Paused)return;
            float t=Time.time;
            if(spinner!=null)spinner.localRotation=Quaternion.Euler(0,t*22,0);
            if(boat!=null)boat.localPosition=new Vector3(Active?(Lane-1)*1.1f:0,.7f+Mathf.Sin(t*2)*.08f,1.15f);
            if(kite!=null)kite.localPosition=new Vector3(Mathf.Sin(t*.9f)*.35f,3.25f+(Active?Altitude*2:0)+Mathf.Sin(t*1.7f)*.38f,.8f);
            if(sled!=null){sled.localPosition=new Vector3(Active?(Lane-1)*1.25f:0,.5f,Active?Mathf.Min(Elapsed*.3f,4f):0);sled.localRotation=Quaternion.Euler(0,Mathf.Sin(t*2)*5,0);}
            if(lights!=null)for(int i=0;i<lights.Length;i++){var p=lights[i].localPosition;p.y=1.7f+Mathf.Sin(t*2+i)*.25f;lights[i].localPosition=p;}
        }

        public bool TryStart()
        {
            if(Active||game==null||game.Paused||Vector3.Distance(game.Player.transform.position,transform.position)>4.5f)return false;
            rideStart=game.Player.transform.position;rideYaw=game.Player.transform.eulerAngles.y;ridePitch=game.Player.Pitch;rideWasFlying=game.Player.Flying;
            Active=true;Elapsed=0;Progress=0;Mistakes=0;Lane=1;Altitude=.45f;beatIndex=0;nextEvent=Kind==ExcursionKind.Sled?2f:3f;localHint="准备好，看上方的下一步提示。";
            UpdateRideCamera();
            game.Notice=Title+"开始！"+Prompt+"；Esc 可退出。";return true;
        }
        void UpdateRideCamera()
        {
            if(!Active)return;
            Vector3 at=transform.position;
            if(Kind==ExcursionKind.Beach)game.Player.Teleport(new Vector3(at.x+(Lane-1)*1.15f,15.15f,at.z-1.5f+Mathf.Min(Elapsed*.12f,3.6f)),0,0);
            else if(Kind==ExcursionKind.Kite)game.Player.Teleport(new Vector3(at.x,15.15f+Altitude*5.5f,at.z-1.5f),0,0);
            else if(Kind==ExcursionKind.Sled)game.Player.Teleport(new Vector3(at.x+(Lane-1)*1.25f,15.15f,at.z-3f+Mathf.Min(Elapsed*.8f,13.5f)),0,0);
        }
        void ReturnFromRide(){if(Kind!=ExcursionKind.Beach&&Kind!=ExcursionKind.Kite&&Kind!=ExcursionKind.Sled)return;game.Player.Teleport(rideStart,rideYaw,ridePitch);game.Player.SetFlying(rideWasFlying);}
        public void Cancel(){if(!Active)return;Active=false;ReturnFromRide();game.Notice=Title+"已退出，随时靠近按 G 再玩。";}
        void Win(){Active=false;ReturnFromRide();CompletedRounds++;int coins=game.EarnActivityCoins(Reward,Title);game.Notice=Title+"完成！得到"+coins+"元；再玩一次也能继续领奖。";}
        void Lose(string reason){Active=false;ReturnFromRide();game.Notice=Title+"："+reason+"。靠近按 G 再试一次。";}

        public void Press(KeyCode key)
        {
            if(!Active)return;
            if(Kind==ExcursionKind.Rainbow)
            {
                if(key!=KeyCode.LeftArrow&&key!=KeyCode.RightArrow&&key!=KeyCode.Space)return;
                if(key==RainbowBeats[beatIndex]){beatIndex++;Progress=beatIndex;localHint="彩虹踏点 " +Progress+" / 8";if(Progress>=RainbowBeats.Length)Win();}
                else{Mistakes++;localHint="踏错了一个节拍，再看提示！";if(Mistakes>=3)Lose("彩虹节奏断开了");}
            }
            else if(Kind==ExcursionKind.Beach)
            {
                if(key==KeyCode.LeftArrow)Lane=Mathf.Max(0,Lane-1);
                else if(key==KeyCode.RightArrow)Lane=Mathf.Min(2,Lane+1);
                else if(key==KeyCode.Space){if(Lane==BeachPearls[Progress]){Progress++;localHint="捡到了闪亮珍珠！";if(Progress>=BeachPearls.Length)Win();}else{Mistakes++;localHint="珍珠在另一边，先换位置！";}}
                UpdateRideCamera();
            }
            else if(Kind==ExcursionKind.Fireflies)
            {
                int choice=key==KeyCode.Alpha1?0:key==KeyCode.Alpha2?1:key==KeyCode.Alpha3?2:key==KeyCode.Alpha4?3:-1;
                if(choice<0)return;if(choice==FireflyColors[Progress]){Progress++;localHint="萤火虫在手心闪了一下！";if(Progress>=FireflyColors.Length)Win();}
                else{Mistakes++;localHint="它飞走了，再看颜色。";if(Mistakes>=4)Lose("萤火虫们藏回树林");}
            }
            else if(Kind==ExcursionKind.Sled)
            {
                if(key==KeyCode.LeftArrow)Lane=Mathf.Max(0,Lane-1);else if(key==KeyCode.RightArrow)Lane=Mathf.Min(2,Lane+1);
                UpdateRideCamera();
            }
        }
        public void Tick(float dt,bool spaceHeld)
        {
            if(!Active||game.Paused)return;Elapsed+=Mathf.Max(0,dt);
            if(Elapsed>42){Lose("这次时间到了");return;}
            if(Kind==ExcursionKind.Kite)
            {
                Altitude=Mathf.Clamp01(Altitude+(spaceHeld?.36f:-.24f)*dt);
                while(Active&&Elapsed>=nextEvent){float target=KiteHeights[Mathf.Min(Progress+Mistakes,KiteHeights.Length-1)];if(Mathf.Abs(Altitude-target)<.28f){Progress++;localHint="顺利穿过云环！";if(Progress>=4){Win();return;}}else{Mistakes++;localHint="离云环差一点，调节风筝高度。";if(Mistakes>=4){Lose("风筝错过了云环");return;}}nextEvent+=3f;}
            }
            else if(Kind==ExcursionKind.Sled)
            {
                while(Active&&Elapsed>=nextEvent){int obstacle=SnowHazards[Mathf.Min(Progress+Mistakes,SnowHazards.Length-1)];if(Lane==obstacle){Mistakes++;localHint="雪橇擦到了冰晶！";if(Mistakes>=3){Lose("雪橇撞上了冰晶");return;}}else{Progress++;localHint="安全绕过冰晶！";}if(Progress+Mistakes>=SnowHazards.Length){Win();return;}nextEvent+=2f;}
            }
            UpdateRideCamera();
        }
        public string TargetHint
        {
            get
            {
                if(Kind==ExcursionKind.Rainbow){var key=RainbowBeats[Mathf.Min(beatIndex,RainbowBeats.Length-1)];return "下一拍："+(key==KeyCode.Space?"空格":key==KeyCode.LeftArrow?"←":"→");}
                if(Kind==ExcursionKind.Beach)return "珍珠在"+LaneName(BeachPearls[Mathf.Min(Progress,BeachPearls.Length-1)])+"边 · 你在"+LaneName(Lane)+"边";
                if(Kind==ExcursionKind.Kite)return "当前高度 "+Mathf.RoundToInt(Altitude*100)+"% · 云环目标 "+Mathf.RoundToInt(KiteHeights[Mathf.Min(Progress+Mistakes,KiteHeights.Length-1)]*100)+"% · "+Mathf.CeilToInt(Mathf.Max(0,nextEvent-Elapsed))+"秒后经过";
                if(Kind==ExcursionKind.Fireflies)return "下一只萤火颜色："+new[]{"红 1","绿 2","蓝 3","金 4"}[FireflyColors[Mathf.Min(Progress,FireflyColors.Length-1)]];
                return "冰晶在"+LaneName(SnowHazards[Mathf.Min(Progress+Mistakes,SnowHazards.Length-1)])+"道 · 你在"+LaneName(Lane)+"道 · "+Mathf.CeilToInt(Mathf.Max(0,nextEvent-Elapsed))+"秒后碰到";
            }
        }
        static string LaneName(int n)=>n==0?"左":n==1?"中":"右";
        void OnGUI()
        {
            if(game==null||game.Paused||!Active&&Vector3.Distance(game.Player.transform.position,transform.position)>7)return;
            float scale=Mathf.Min(Screen.width/1000f,Screen.height/650f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
            var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("NotoSansCJKsc-Regular"),fontSize=16,wordWrap=true,alignment=TextAnchor.MiddleCenter};style.normal.textColor=Color.white;
            float w=Screen.width/scale;
            if(!Active){Sandbox.Panel(new Rect(w/2-275,Screen.height/scale-198,550,86),new Color(.12f,.2f,.3f,.9f));GUI.Label(new Rect(w/2-260,Screen.height/scale-191,520,70),Title+" · 靠近按G开始 · 完成得"+Reward+"元\n"+Prompt,style);return;}
            Sandbox.Panel(new Rect(w/2-330,14,660,164),new Color(.09f,.16f,.27f,.94f));
            GUI.Label(new Rect(w/2-315,21,630,28),Title+"   ·   "+Reward+"元   ·   进度 "+Progress+"   ·   失误 "+Mistakes,style);
            GUI.Label(new Rect(w/2-315,50,630,49),Prompt,style);
            GUI.Label(new Rect(w/2-315,105,630,26),TargetHint,style);
            GUI.Label(new Rect(w/2-315,135,630,35),localHint+" · 剩余"+Mathf.CeilToInt(Mathf.Max(0,42-Elapsed))+"秒 · Esc退出",style);
        }
    }

    public partial class Sandbox
    {
        public ExcursionAttraction Excursion {get;private set;}
        public bool ExcursionActive=>Excursion!=null&&Excursion.Active;
        void BuildExcursionAttraction()
        {
            if(World.Region<11||World.Region>15){Excursion=null;return;}
            var o=new GameObject(VoxelWorld.RegionNames[World.Region]+"的游乐项目");o.transform.SetParent(villagePeople.transform);
            Excursion=o.AddComponent<ExcursionAttraction>();Excursion.Initialize(this,(ExcursionKind)(World.Region-11),new Vector3(32.5f,15,33.5f));
        }
        void ResetExcursionAttraction(){if(Excursion!=null)Excursion.Cancel();Excursion=null;}
        public bool TryExcursionG()=>Excursion!=null&&Excursion.TryStart();
        void TickExcursionAttraction()
        {
            if(Excursion==null||!Excursion.Active)return;
            if(!IsSmoke){if(Input.GetKeyDown(KeyCode.Escape)){Excursion.Cancel();return;}foreach(var key in new[]{KeyCode.LeftArrow,KeyCode.RightArrow,KeyCode.Space,KeyCode.Alpha1,KeyCode.Alpha2,KeyCode.Alpha3,KeyCode.Alpha4})if(Input.GetKeyDown(key))Excursion.Press(key);}
            Excursion.Tick(Time.deltaTime,!IsSmoke&&Input.GetKey(KeyCode.Space));
        }
    }
}
