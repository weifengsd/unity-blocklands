using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public class InvitedChildFollower:MonoBehaviour {public int StyleId;}
    public class HeavenVisitTransfer:MonoBehaviour
    {
        Sandbox game;Vector3 start,end;float elapsed;RealmResidentLife life;Transform cloud;
        public void Begin(Sandbox g,Vector3 target){game=g;start=transform.position;end=target;life=GetComponent<RealmResidentLife>();if(life!=null)life.enabled=false;cloud=new GameObject("去天堂的云光通道").transform;cloud.SetParent(transform,false);var art=cloud.gameObject.AddComponent<RoyalArt>();art.Piece("托起居民的云",new Vector3(0,-.1f,0),new Vector3(1.5f,.25f,1.5f),new Color(.87f,.94f,1),PrimitiveType.Sphere);}
        public void Cancel(){if(cloud!=null)Destroy(cloud.gameObject);Destroy(this);}
        void Update(){if(game==null||game.Paused)return;elapsed+=Time.unscaledDeltaTime;transform.position=Vector3.Lerp(start,end,Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/3)));if(elapsed>=3){if(life!=null){life.Initialize(game,"散步");life.enabled=true;}if(cloud!=null)Destroy(cloud.gameObject);Destroy(this);}}
    }
    public partial class Sandbox
    {
        NeighbourSpeech stickerPerson;float stickerShowAt;bool stickerVisible;string stickerDraft="",stickerFeedback="";
        public bool StickerOpen=>stickerVisible&&stickerPerson!=null;
        public string StickerText=>stickerDraft;
        public readonly List<int> InvitedChildStyles=new List<int>();
        public readonly HashSet<int> PlanetHeavenVisitors=new HashSet<int>();
        readonly Dictionary<int,RoyalArt> invitedModels=new Dictionary<int,RoyalArt>();
        public int InvitedChildCount=>InvitedChildStyles.Count;
        float partyPlayUntil;

        public void QueueInvitationSticker(NeighbourSpeech person,float delay)
        {
            if(person==null||person.InvitationResolvedThisVisit||stickerPerson!=null)return;
            stickerPerson=person;stickerShowAt=Time.unscaledTime+delay;stickerVisible=false;stickerDraft="";stickerFeedback="";
        }
        void TickInvitationSticker()
        {
            if(stickerPerson==null){stickerVisible=false;return;}
            if(!stickerVisible&&Vector3.Distance(Player.transform.position,stickerPerson.transform.position)>8){stickerPerson=null;return;}
        }
        public bool OpenStickerFor(NeighbourSpeech person){if(person==null)return false;stickerPerson=person;stickerVisible=true;stickerDraft="";stickerFeedback="";return true;}
        public bool HasInvitationRequest=>stickerPerson!=null&&!stickerVisible&&Time.unscaledTime>=stickerShowAt;
        public bool OpenInvitationRequest(){if(stickerPerson==null)return false;var p=stickerPerson;lifeWorkshopOpen=false;SetPaused(false);return OpenStickerFor(p);}
        public bool SubmitInvitationSticker(string words)
        {
            if(!StickerOpen)return false;string answer=(words??"").Trim().Replace(" ","").Replace("。","").Replace("！","");
            bool no=answer=="不可以"||answer=="不可以进去"||answer=="不进城堡"||answer=="暂时不可以"||answer=="不要"||answer=="不能"||answer=="不进天堂"||answer=="不去天堂"||answer=="不去"||answer=="先不去";
            bool yes=!no&&(answer=="可以"||answer=="可以进去"||answer=="可以进城堡"||answer=="一起进去"||answer=="一起进城堡"||answer=="进天堂"||answer=="可以去天堂"||answer=="要进天堂"||answer=="一起去天堂"||answer=="好"||answer=="要");
            if(!yes&&!no){stickerFeedback="请输入“可以”或“不可以”；星球居民也可输入“进天堂”或“不进天堂”。";return false;}
            var person=stickerPerson;person.InvitationResolvedThisVisit=true;
            if(yes&&person.Kind==NeighbourSpeechKind.CastleEnvy)
            {
                var style=person.GetComponent<CastleGirlStyle>();if(style==null||!RoyalIdentity){stickerFeedback="成为公主后才能邀请小朋友一起进城堡。";return false;}
                if(!InvitedChildStyles.Contains(style.StyleId))InvitedChildStyles.Add(style.StyleId);
                person.gameObject.SetActive(false);SyncInvitedChildren();Notice="小朋友加入了牵手队伍。小禾也可以继续同行，大家一起进城堡、一起玩。";
            }
            else if(yes)
            {
                var age=person.GetComponent<PlanetCitizenAge>();if(age!=null){PlanetHeavenVisitors.Add(age.CitizenId);var transfer=person.GetComponent<HeavenVisitTransfer>()??person.gameObject.AddComponent<HeavenVisitTransfer>();transfer.Begin(this,HeavenVisitorPosition(age.CitizenId));}
                Notice="居民乘云光通道去天堂玩了。你不会被自动带飞。";
            }
            else Notice=person.Kind==NeighbourSpeechKind.CastleEnvy?"暂不邀请，小朋友继续留在公园玩耍，不会受到伤害。":"居民留在星球生活，暂时不去天堂。";
            stickerPerson=null;stickerVisible=false;stickerDraft="";Save();return true;
        }
        public void CloseInvitationSticker(){if(stickerPerson!=null)stickerPerson.InvitationResolvedThisVisit=true;stickerPerson=null;stickerVisible=false;stickerDraft="";}
        void StickerKeys(){if(Input.GetKeyDown(KeyCode.Escape))CloseInvitationSticker();else if((Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))&&string.IsNullOrEmpty(Input.compositionString))SubmitInvitationSticker(stickerDraft);}
        void DrawInvitationSticker(float w,float h)
        {
            float x=w/2-380,y=h-225;Panel(new Rect(x-3,y-3,766,154),new Color(.42f,.29f,.07f));Panel(new Rect(x,y,760,148),new Color(1,.81f,.19f,1));
            Label(new Rect(x+18,y+9,724,38),stickerPerson.Line,18,new Color(.28f,.18f,.05f));
            var inputStyle=new GUIStyle(GUI.skin.textField){font=font,fontSize=22,padding=new RectOffset(12,12,8,8)};
            inputStyle.normal.background=Texture2D.whiteTexture;inputStyle.focused.background=Texture2D.whiteTexture;inputStyle.hover.background=Texture2D.whiteTexture;
            inputStyle.normal.textColor=inputStyle.focused.textColor=new Color(.2f,.13f,.03f);
            Color background=GUI.backgroundColor;GUI.backgroundColor=new Color(1,.94f,.5f);GUI.SetNextControlName("InvitationAnswer");stickerDraft=GUI.TextField(new Rect(x+18,y+55,532,48),stickerDraft,100,inputStyle);GUI.backgroundColor=background;
            if(stickerDraft.Length==0)Label(new Rect(x+32,y+64,500,28),"在这里打字：可以 / 不可以",19,new Color(.55f,.4f,.12f));
            if(GUI.Button(new Rect(x+565,y+55,82,48),"发送"))SubmitInvitationSticker(stickerDraft);
            if(GUI.Button(new Rect(x+661,y+55,82,48),"关闭"))CloseInvitationSticker();
            Label(new Rect(x+18,y+113,724,28),stickerFeedback.Length>0?stickerFeedback:"Enter发送 · Esc关闭 · 星球居民可填“进天堂 / 不进天堂”",15,new Color(.32f,.22f,.06f));
            GUI.FocusControl("InvitationAnswer");
        }

        public Vector3 HeavenVisitorPosition(int id)=>new Vector3(24+(id%5)*2,HeavenFloor+1.1f,43+(id/5)%3*2);
        void SyncInvitedChildren(bool relocate=false)
        {
            foreach(int id in InvitedChildStyles)
            {
                if(!invitedModels.TryGetValue(id,out var art)||art==null){art=CastleGirlStyle.Make(transform,id);art.name="受邀城堡伙伴 "+id;art.gameObject.AddComponent<InvitedChildFollower>().StyleId=id;art.transform.position=FindFollowerSpawn(id);NpcNavigation.Ensure(art.transform);invitedModels[id]=art;}else if(relocate)art.transform.position=FindFollowerSpawn(id);
                art.gameObject.SetActive(true);
            }
            foreach(var style in FindObjectsByType<CastleGirlStyle>(FindObjectsInactive.Include))if(style.GetComponent<InvitedChildFollower>()==null&&style.GetComponentInParent<PalaceLife>()!=null)style.gameObject.SetActive(!InvitedChildStyles.Contains(style.StyleId)&&!ReturningChild(style.StyleId));
            var remove=new List<int>();foreach(var pair in invitedModels)if(!InvitedChildStyles.Contains(pair.Key)){if(pair.Value!=null)Destroy(pair.Value.gameObject);remove.Add(pair.Key);}foreach(int id in remove)invitedModels.Remove(id);
        }
        void RestoreInvitationParty(SavedWorld save)
        {
            InvitedChildStyles.Clear();if(save.invitedChildren!=null)InvitedChildStyles.AddRange(save.invitedChildren);PlanetHeavenVisitors.Clear();if(save.planetHeavenVisitors!=null)foreach(int id in save.planetHeavenVisitors)PlanetHeavenVisitors.Add(id);CloseInvitationSticker();SyncInvitedChildren();
        }
        public List<Transform> PartyMembers()
        {
            var members=new List<Transform>();if(ForestFriendFollowing&&forestFriend!=null)members.Add(forestFriend);
            foreach(int id in InvitedChildStyles)if(invitedModels.TryGetValue(id,out var a)&&a!=null)members.Add(a.transform);return members;
        }
        public void StartPartyBallPlay(){if(InvitedChildCount>0||ForestFriendFollowing)partyPlayUntil=Time.unscaledTime+10;}
        public bool PartyPlayingBall=>World.Region==6&&Time.unscaledTime<partyPlayUntil;
        void TickInvitedChildren()
        {
            int n=0;foreach(int id in InvitedChildStyles)
            {
                if(!invitedModels.TryGetValue(id,out var art)||art==null)continue;
                Vector3 goal=PartyFollowOrigin+Player.transform.right*(.72f+n*.67f)+Player.transform.forward*1.05f;
                if(PartyPlayingBall){float angle=(n+1)*Mathf.PI/2;goal=CastleBallPlay.Court+new Vector3(Mathf.Sin(angle)*2.5f,0,Mathf.Cos(angle)*2.5f);}
                if(CoasterRiding){var coaster=ActiveCoaster;coaster.PartySeat(n+(ForestFriendFollowing?1:0),out goal,out var rotation);art.transform.rotation=rotation;}
                else art.transform.rotation=Quaternion.Slerp(art.transform.rotation,Player.transform.rotation,Time.deltaTime*8);
                if(CoasterRiding)art.transform.position=goal;else NpcNavigation.Ensure(art.transform).MoveTo(goal,9,Time.deltaTime);n++;
            }
        }
    }
}
