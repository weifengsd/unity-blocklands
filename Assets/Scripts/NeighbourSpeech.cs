using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public enum NeighbourSpeechKind { CastleEnvy, PlanetWish }
    public enum PlanetAge { Child, Adult, Elder }
    public class PlanetCitizenAge : MonoBehaviour
    {
        public PlanetAge Age;public Sandbox Game;public int CitizenId;Transform left,right;
        void Start(){left=transform.Find("left leg");right=transform.Find("right leg");}
        void Update(){if(Game==null||Game.Paused)return;float step=Mathf.Sin(Time.time*3+transform.position.x)* (Age==PlanetAge.Elder?9:18);if(left!=null)left.localRotation=Quaternion.Euler(step,0,0);if(right!=null)right.localRotation=Quaternion.Euler(-step,0,0);}
    }
    public class NeighbourSpeech : MonoBehaviour
    {
        public Sandbox Game;public NeighbourSpeechKind Kind;public bool Elder;public bool InvitationResolvedThisVisit;public int PlayCount {get;private set;}
        AudioSource voice;AudioClip clip;TextMesh bubble;float nextAt,bubbleUntil;bool wasNear,saidThisVisit,voicePaused;
        public string Line=>Kind==NeighbourSpeechKind.CastleEnvy?"哎呀，好羡慕这位公主，我也想进水晶城堡。":"我想去天堂玩一会儿，但是那里可是神圣不可侵犯的。";
        public bool Ready=>isActiveAndEnabled&&Eligible&&Near&&!Game.StickerOpen&&Time.unscaledTime>=nextAt&&(Kind==NeighbourSpeechKind.PlanetWish||!saidThisVisit);
        public bool Speaking=>isActiveAndEnabled&&voice!=null&&(voice.isPlaying||voicePaused);
        public bool Near=>Game!=null&&Vector3.Distance(Game.Player.transform.position,transform.position)<4.5f;
        public bool Eligible=>Game!=null&&!Game.Paused&&!Game.PhoneOpen&&!Game.InspectingHero&&(Kind==NeighbourSpeechKind.CastleEnvy?Game.World.Region==6&&!Game.InVoidRealms&&Game.RoyalIdentity:Game.InPlanetLife);
        public bool VoiceAvailable=>clip!=null;
        public void Initialize(Sandbox game,NeighbourSpeechKind kind,bool elder=false)
        {
            Game=game;Kind=kind;Elder=elder;clip=Resources.Load<AudioClip>(kind==NeighbourSpeechKind.CastleEnvy?"CastleChildEnvy":elder?"PlanetElderWish":"PlanetResidentWish");
            voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.volume=.45f;voice.spatialBlend=.65f;voice.minDistance=1.5f;voice.maxDistance=12;
            var label=new GameObject("靠近时的字幕").transform;label.SetParent(transform,false);label.localPosition=Vector3.up*(kind==NeighbourSpeechKind.CastleEnvy?3:2.45f);
            bubble=label.gameObject.AddComponent<TextMesh>();bubble.font=Resources.Load<Font>("NotoSansCJKsc-Regular");bubble.GetComponent<Renderer>().sharedMaterial=bubble.font.material;bubble.fontSize=36;bubble.characterSize=.028f;bubble.anchor=TextAnchor.MiddleCenter;bubble.color=new Color(1,.95f,.7f);bubble.text="";
            game.RegisterNeighbour(this);
        }
        public float Speak()
        {
            if(!Ready)return 0;PlayCount++;saidThisVisit=true;float duration=clip!=null?clip.length:5;
            if(clip!=null)voice.PlayOneShot(clip);bubble.text=Kind==NeighbourSpeechKind.CastleEnvy?"哎呀，好羡慕这位公主！\n我也想进水晶城堡。":"我想去天堂玩一会儿……\n但那里可是神圣不可侵犯的。";
            bubbleUntil=Time.unscaledTime+duration+1;nextAt=Time.unscaledTime+duration+6;Game.Notice=name+"："+Line;Game.QueueInvitationSticker(this,duration);return duration;
        }
        public void StopForTrip(){if(voice!=null)voice.Stop();voicePaused=false;if(bubble!=null)bubble.text="";}
        public void ResetAfterTrip(){StopForTrip();InvitationResolvedThisVisit=false;saidThisVisit=false;wasNear=false;nextAt=Time.unscaledTime+8;}
        void Update()
        {
            if(Game==null)return;bool near=Near;if(!near&&wasNear){saidThisVisit=false;InvitationResolvedThisVisit=false;nextAt=Time.unscaledTime+1;}wasNear=near;
            if(bubble!=null){if(Time.unscaledTime>bubbleUntil||!Eligible)bubble.text="";Vector3 eye=Game.Player.View.transform.position-bubble.transform.position;if(eye.sqrMagnitude>.01f)bubble.transform.rotation=Quaternion.LookRotation(-eye);}
            if((Game.Paused||Game.PhoneOpen)&&voice.isPlaying){voice.Pause();voicePaused=true;}else if(!Game.Paused&&!Game.PhoneOpen&&voicePaused){voice.UnPause();voicePaused=false;}
        }
    }
    public partial class Sandbox
    {
        readonly List<NeighbourSpeech> neighbours=new List<NeighbourSpeech>();float nextNeighbourSpeech;
        public void RegisterNeighbour(NeighbourSpeech person){neighbours.Add(person);}
        void TickNeighbourSpeech()
        {
            if(Paused||PhoneOpen||InspectingHero||StickerOpen||Time.unscaledTime<nextNeighbourSpeech)return;
            foreach(var person in neighbours)if(person!=null&&person.Speaking)return;
            NeighbourSpeech nearest=null;float distance=10;
            foreach(var person in neighbours){if(person==null||!person.Ready)continue;float d=Vector3.Distance(Player.transform.position,person.transform.position);if(d<distance){nearest=person;distance=d;}}
            if(nearest!=null)nextNeighbourSpeech=Time.unscaledTime+nearest.Speak()+1;
            neighbours.RemoveAll(p=>p==null);
        }
        RoyalArt BuildPlanetVillager(Transform root,string name,Vector3 position,int index,PlanetAge age,bool speaking=true)
        {
            var person=new GameObject(name).transform;person.SetParent(root,false);person.localPosition=PlanetHeavenVisitors.Contains(index)?HeavenVisitorPosition(index):position;if(age==PlanetAge.Child)person.localScale=Vector3.one*.72f;
            var a=person.gameObject.AddComponent<RoyalArt>();Color skin=new Color(.78f,.57f,.4f),dark=new Color(.17f,.13f,.13f),cloth=Color.HSVToRGB((.08f+index*.13f)%1,.45f,.65f),hair=age==PlanetAge.Elder?new Color(.83f,.83f,.8f):dark;
            a.Piece("body",new Vector3(0,1.05f,0),new Vector3(.65f,.85f,.36f),cloth);
            a.Piece("head",new Vector3(0,1.75f,0),new Vector3(.55f,.55f,.5f),skin);
            a.Piece("hair",new Vector3(0,2.02f,-.025f),new Vector3(.59f,.14f,.54f),hair);
            foreach(float x in new[]{-.14f,.14f})a.Piece("eye",new Vector3(x,1.83f,.258f),new Vector3(.065f,.065f,.025f),dark);
            a.Piece("nose",new Vector3(0,1.7f,.28f),new Vector3(.1f,.12f,.09f),skin);a.Piece("smile",new Vector3(0,1.6f,.258f),new Vector3(.17f,.035f,.02f),dark);
            foreach(float x in new[]{-.44f,.44f})a.Piece("手臂",new Vector3(x,1.02f,0),new Vector3(.19f,.73f,.24f),skin);
            a.Piece("left leg",new Vector3(-.18f,.37f,0),new Vector3(.24f,.72f,.28f),dark);a.Piece("right leg",new Vector3(.18f,.37f,0),new Vector3(.24f,.72f,.28f),dark);
            if(age==PlanetAge.Elder){foreach(float x in new[]{-.14f,.14f})a.Piece("老人眼镜",new Vector3(x,1.83f,.283f),new Vector3(.16f,.035f,.025f),Color.gray);a.Piece("老人手杖",new Vector3(.52f,.62f,.15f),new Vector3(.08f,1.2f,.08f),new Color(.48f,.32f,.19f));}
            var ages=person.gameObject.AddComponent<PlanetCitizenAge>();ages.Game=this;ages.Age=age;ages.CitizenId=index;
            person.gameObject.AddComponent<RealmResidentLife>().Initialize(this,"散步");
            if(speaking)person.gameObject.AddComponent<NeighbourSpeech>().Initialize(this,NeighbourSpeechKind.PlanetWish,age==PlanetAge.Elder);
            return a;
        }
    }
}
