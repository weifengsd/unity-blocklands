using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    public class WorldLoadingScreen:MonoBehaviour
    {
        public static WorldLoadingScreen Active {get;private set;}
        public float Progress {get;private set;}
        public string Phase {get;private set;}="正在打开奇境";
        public bool Ready {get;private set;}
        public bool Failed {get;private set;}
        public bool HasKeyArt=>art!=null;
        public readonly List<float> ProgressHistory=new List<float>();
        public readonly List<string> PhaseHistory=new List<string>();
        Texture2D art,shade,bar,footer;Font font;float began,fadeBegan=-1;string failure="";
        readonly string[] tips={"把家，藏进地底九层","开着房车，驶向云端","一封邀请函，一整晚的派对","穿过传送门，遇见梦影龙"};
        readonly string[] details={"升降椅、保险箱、实时监控……\n把地下基地布置成自己的秘密天地。","八个座位，带上朋友一起出发。\n房车能飞行，也能停下来休息。","布置舞台、音响和彩灯，\n请朋友到自己的大房间做客。","收集梦影绿晶，搭好传送门。\n带上工具与伙伴，再踏上冒险。"};
        public static WorldLoadingScreen Create(){var screen=new GameObject("幻方世界 · 奇境加载画面").AddComponent<WorldLoadingScreen>();return screen;}
        void Awake()
        {
            Active=this;began=Time.realtimeSinceStartup;art=Resources.Load<Texture2D>("LoadingWorldKeyArt");font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            var clear=new GameObject("加载背景相机").AddComponent<Camera>();clear.transform.SetParent(transform,false);clear.depth=-10000;clear.cullingMask=0;clear.clearFlags=CameraClearFlags.SolidColor;clear.backgroundColor=new Color(.018f,.027f,.065f);
            shade=new Texture2D(128,1,TextureFormat.RGBA32,false);bar=new Texture2D(128,1,TextureFormat.RGBA32,false);
            footer=new Texture2D(1,128,TextureFormat.RGBA32,false);
            for(int x=0;x<128;x++){float t=x/127f;shade.SetPixel(x,0,new Color(.012f,.022f,.065f,Mathf.Lerp(.82f,0,t)));bar.SetPixel(x,0,Color.Lerp(new Color(.23f,.91f,1),new Color(.99f,.43f,.83f),t));}
            for(int y=0;y<128;y++)footer.SetPixel(0,y,new Color(.015f,.024f,.06f,.97f*(1-y/127f)));
            shade.Apply();bar.Apply();footer.Apply();shade.wrapMode=bar.wrapMode=footer.wrapMode=TextureWrapMode.Clamp;Report(0,Phase);
        }
        public void Report(float value,string phase){if(Failed||Ready)return;Progress=Mathf.Max(Progress,Mathf.Clamp01(value));Phase=phase;ProgressHistory.Add(Progress);if(PhaseHistory.Count==0||PhaseHistory[PhaseHistory.Count-1]!=phase)PhaseHistory.Add(phase);}
        public void Complete(){Report(1,"世界已就绪");Ready=true;}
        public void FadeOut(){fadeBegan=Time.realtimeSinceStartup;}
        public void Fail(string message){Failed=true;failure=message;Phase="加载暂未完成";fadeBegan=-1;}
        public static Rect ProgressTrack(float w,float h)=>new Rect(50,h-66,w-100,7);
        public static Rect TitleArea(float w)=>new Rect(46,109,Mathf.Min(720,w-92),118);
        void Box(Rect r,Color c){var old=GUI.color;GUI.color*=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Text(Rect r,string text,int size,Color c,bool bold=false,TextAnchor align=TextAnchor.UpperLeft){GUI.Label(r,text,new GUIStyle{font=font,fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,alignment=align,wordWrap=true,normal={textColor=c},padding=new RectOffset(0,0,0,0),margin=new RectOffset(0,0,0,0)});}
        void OnGUI()
        {
            var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;GUI.depth=-30000;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);if(scale<=0)return;float w=Screen.width/scale,h=Screen.height/scale;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            float age=Time.realtimeSinceStartup-began,opacity=fadeBegan<0?1:1-Mathf.Clamp01((Time.realtimeSinceStartup-fadeBegan)/.45f);GUI.color=new Color(1,1,1,opacity);
            Box(new Rect(0,0,w,h),new Color(.018f,.027f,.065f));
            if(art!=null)GUI.DrawTexture(new Rect(-3+Mathf.Sin(age*.1f)*3,-2,w+6,h+4),art,ScaleMode.ScaleAndCrop);
            GUI.DrawTexture(new Rect(0,0,w*.63f,h),shade,ScaleMode.StretchToFill);
            // Readable footer, with no flashes or rapid camera motion.
            GUI.DrawTexture(new Rect(0,h-230,w,230),footer,ScaleMode.StretchToFill);
            for(int i=0;i<22;i++){float x=(i*173.3f+41)%w,y=(i*93.7f+Mathf.Sin(age*.2f+i)*9)%(h-190);float alpha=.14f+.1f*Mathf.Sin(age*.65f+i);Box(new Rect(x,y,i%3==0?3:2,i%3==0?3:2),new Color(.58f,.9f,1,alpha));}
            var cyan=new Color(.4f,.93f,1);var cream=new Color(.98f,.95f,.88f);
            Box(new Rect(50,61,25,3),cyan);Text(new Rect(88,51,580,24),"自由建造  /  奇境冒险  /  和朋友一起",14,cream);
            Rect title=TitleArea(w);Text(new Rect(title.x+2,title.y+4,title.width,title.height),"幻方世界",86,new Color(.02f,.04f,.13f,.8f),true);Text(title,"幻方世界",86,cream,true);
            Text(new Rect(51,229,560,42),"把想象，搭成世界。",26,new Color(.85f,.94f,1));
            Box(new Rect(52,288,46,3),cyan);Box(new Rect(98,288,46,3),new Color(1,.52f,.82f));
            string[] tags={"水晶城堡","飞行房车","地下密室","好友派对"};for(int i=0;i<tags.Length;i++){float x=51+i*108;Box(new Rect(x,317,98,29),new Color(.05f,.12f,.23f,.65f));Text(new Rect(x,321,98,23),tags[i],14,new Color(.83f,.93f,1),false,TextAnchor.MiddleCenter);}
            int tip=Mathf.FloorToInt(age/6)%tips.Length;float ty=Mathf.Max(392,h-275);Box(new Rect(51,ty,3,94),new Color(.44f,.86f,.99f,.75f));Text(new Rect(68,ty-4,460,25),"探索灵感  "+(tip+1).ToString("00")+" / 04",12,cyan);Text(new Rect(68,ty+23,490,39),tips[tip],25,cream,true);Text(new Rect(68,ty+62,480,57),details[tip],15,new Color(.77f,.84f,.92f));
            Text(new Rect(50,h-138,w-200,32),Failed?"世界暂时无法打开":Ready?"准备就绪，冒险即将开始":Phase,20,cream,true);
            Text(new Rect(w-210,h-153,160,56),Failed?"!":Mathf.FloorToInt(Progress*100).ToString("00")+"%",42,cyan,true,TextAnchor.MiddleRight);
            var track=ProgressTrack(w,h);Box(track,new Color(.31f,.41f,.57f,.45f));if(Progress>0)GUI.DrawTexture(new Rect(track.x,track.y,track.width*Progress,track.height),bar,ScaleMode.StretchToFill);
            if(!Ready&&!Failed&&Progress>0){float x=track.x+track.width*Progress;Box(new Rect(x-2,track.y-4,4,15),new Color(.77f,1,1,.8f));}
            Text(new Rect(50,h-43,w-100,23),"幻方世界 2.27    ·    概念主视觉，实际画面以游戏内为准",12,new Color(.63f,.72f,.83f));
            if(Failed){Box(new Rect(46,h-220,w-92,62),new Color(.22f,.045f,.09f,.96f));Text(new Rect(61,h-210,w-122,50),failure+"\n请关闭游戏后重试；未完成初始化的世界不会自动保存。",16,cream);}
            GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
        }
        void OnDestroy(){if(Active==this)Active=null;Destroy(shade);Destroy(bar);Destroy(footer);}
    }
}
