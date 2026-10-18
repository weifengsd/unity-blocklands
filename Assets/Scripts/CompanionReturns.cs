using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    // Home flags are committed at departure. Reloading mid-trip safely places the guest
    // at home instead of losing them or creating a second follower.
    public partial class Sandbox
    {
        class HomeRide
        {
            public int Id;public bool Child;public Transform Car,Passenger,Parent;
            public Vector3 Start,End,Home,Scale;public Quaternion Rotation;public float Age;
            public RealmResidentLife Life;public NeighbourSpeech Speech;public Transform CastleActor;
            public PlanetCitizenAge Citizen;public readonly Dictionary<Transform,Quaternion> LegPoses=new Dictionary<Transform,Quaternion>();
        }
        readonly List<HomeRide> homeRides=new List<HomeRide>();
        bool companionMenuOpen;Vector2 companionMenuScroll;int chatChild=-1;string childChatDraft="",childChatReply="";
        public bool CompanionMenuOpen=>companionMenuOpen;
        public int HomeRideCount=>homeRides.Count;
        public string ChildChatReply=>childChatReply;
        public bool CompanionReturnBlocked=>CarRiding||PartyRideActive||DeepInputCaptured||SecretSwingActive||CoasterRiding||CoasterSlipActive||FestivalLocked||PalacePlayLocked||ExcursionActive||SideRideActive||RoyalTransportActive||VoidRideActive;

        public bool OpenCompanionMenu()
        {
            if(CompanionReturnBlocked||PhoneOpen||InspectingHero||StickerOpen){Notice="先结束当前游玩或关闭对话，再打开伙伴菜单。";return false;}
            inventoryOpen=false;lifeWorkshopOpen=false;activeStroke=null;partyBag=false;travelOpen=false;companionMenuOpen=true;chatChild=-1;companionMenuScroll=Vector2.zero;SetPaused(true);return true;
        }
        public void CloseCompanionMenu(){companionMenuOpen=false;chatChild=-1;SetPaused(false);}
        public bool ChatWithChild(int id,string words)
        {
            if(!InvitedChildStyles.Contains(id)&&!CastleChildAtHome(id))return false;
            string s=(words??"").Trim();if(s.Length==0){childChatReply="你想和我说什么呢？";return false;}
            chatChild=id;
            childChatReply=s.Contains("回城堡")||s.Contains("回水晶城堡")?"好呀！点下面的“坐车回水晶城堡”，我就回游乐区玩。":
                s.Contains("球")?"我喜欢传球！我们到水晶城堡的球场，按G一起玩吧。":
                s.Contains("秋千")?"我喜欢秋千，也喜欢滑滑梯。回城堡以后还能再见面！":
                s.Contains("再见")?"再见！下次到水晶城堡，还可以邀请我一起玩。":
                s.Contains("名字")?"你可以叫我城堡小伙伴"+(id+1)+"，认一认我的裙子和蝴蝶结吧！":
                s.Contains("你好")||s.Contains("开心")?"你好！今天和大家一起玩，我很开心。":"我听见你说：“"+s+"”。我们可以聊传球、秋千，或者约好下次一起玩。";
            Notice="城堡小伙伴"+(id+1)+"："+childChatReply;return true;
        }
        bool CastleChildAtHome(int id)
        {
            if(World.Region!=6||InVoidRealms)return false;
            foreach(var style in FindObjectsByType<CastleGirlStyle>())if(style.StyleId==id&&style.GetComponent<InvitedChildFollower>()==null&&style.GetComponentInParent<PalaceLife>()!=null)return true;
            return false;
        }
        bool ReturningChild(int id){foreach(var r in homeRides)if(r.Child&&r.Id==id)return true;return false;}
        public Vector3 PlanetCitizenHome(int id)
        {
            if(id==32)return new Vector3(93,PlanetFloor+1,78);
            if(id==33)return new Vector3(97,PlanetFloor+1,78);
            if(id==34)return new Vector3(93,PlanetFloor+1,108);
            return new Vector3(-26+id%6*22,PlanetFloor+1,2+id/6*30);
        }
        public bool ReturnChildByCar(int id)
        {
            if(CompanionReturnBlocked||!InvitedChildStyles.Contains(id)||!invitedModels.TryGetValue(id,out var art)||art==null)return false;
            var r=new HomeRide{Id=id,Child=true,Passenger=art.transform,Scale=art.transform.localScale,Rotation=art.transform.rotation};
            r.Home=CastleBallPlay.Court+new Vector3((id%3-1)*2.6f,0,-4-id/3*1.6f);
            if(World.Region==6&&!InVoidRealms)foreach(var style in FindObjectsByType<CastleGirlStyle>(FindObjectsInactive.Include))if(style.StyleId==id&&style.GetComponent<InvitedChildFollower>()==null&&style.GetComponentInParent<PalaceLife>()!=null){r.CastleActor=style.transform;r.Home=style.transform.position;break;}
            r.Start=HomeCarDock(art.transform.position+Player.transform.forward*2.2f);
            r.End=World.Region==6&&!InVoidRealms?HomeCarDock(r.Home):r.Start+new Vector3(18,24,22);
            InvitedChildStyles.Remove(id);invitedModels.Remove(id);StartHomeRide(r);Save();Notice="城堡小伙伴"+(id+1)+"坐上粉色小汽车，返回水晶城堡游乐区。小禾继续陪你。";return true;
        }
        public bool ReturnResidentByCar(int id)
        {
            if(CompanionReturnBlocked||!PlanetHeavenVisitors.Contains(id))return false;
            PlanetCitizenAge citizen=null;foreach(var c in FindObjectsByType<PlanetCitizenAge>())if(c.CitizenId==id){citizen=c;break;}
            if(citizen==null)return false;
            var transfer=citizen.GetComponent<HeavenVisitTransfer>();if(transfer!=null){transfer.enabled=false;transfer.Cancel();}
            var r=new HomeRide{Id=id,Child=false,Passenger=citizen.transform,Parent=citizen.transform.parent,Scale=citizen.transform.localScale,Rotation=citizen.transform.rotation,Life=citizen.GetComponent<RealmResidentLife>(),Speech=citizen.GetComponent<NeighbourSpeech>(),Citizen=citizen};
            r.Home=PlanetCitizenHome(id);r.Start=HomeCarDock(citizen.transform.position+Vector3.forward*2.2f);r.End=HomeCarDock(r.Home);
            PlanetHeavenVisitors.Remove(id);StartHomeRide(r);Save();Notice="星球居民"+(id+1)+"坐上云朵汽车，回下面星球自己的家。你不会被带走，也不会自动飞。";return true;
        }
        public int ReturnAllChildren(){int count=0;foreach(int id in new List<int>(InvitedChildStyles))if(ReturnChildByCar(id))count++;return count;}
        public int ReturnAllResidents(){int count=0;foreach(int id in new List<int>(PlanetHeavenVisitors))if(ReturnResidentByCar(id))count++;return count;}
        Vector3 HomeCarDock(Vector3 wanted)
        {
            if(Physics.Raycast(wanted+Vector3.up*2.5f,Vector3.down,out var hit,5,~(1<<2),QueryTriggerInteraction.Ignore))wanted.y=Mathf.Max(wanted.y,hit.point.y+.18f);
            return wanted;
        }
        void StartHomeRide(HomeRide r)
        {
            if(r.Speech!=null&&stickerPerson==r.Speech)CloseInvitationSticker();
            if(r.Life!=null)r.Life.enabled=false;if(r.Speech!=null){r.Speech.StopForTrip();r.Speech.enabled=false;}
            if(r.Citizen!=null)r.Citizen.enabled=false;
            r.Car=MakeHomeCar(r.Start,!r.Child);r.Car.name=r.Child?"送小朋友回水晶城堡的粉色小汽车":"居民返回星球的云朵汽车";
            var art=r.Car.GetComponent<RoyalArt>();art.Piece("柔软的乘客座椅",new Vector3(0,.55f,-.25f),new Vector3(1.1f,.25f,.85f),new Color(1,.77f,.86f));
            art.Piece("乘客靠背",new Vector3(0,.9f,-.7f),new Vector3(1.1f,.7f,.15f),new Color(.89f,.47f,.72f));
            r.Passenger.SetParent(r.Car,true);r.Passenger.localPosition=new Vector3(0,.65f,-.2f);r.Passenger.localRotation=Quaternion.identity;r.Passenger.localScale=r.Scale;
            foreach(var t in r.Passenger.GetComponentsInChildren<Transform>())if(t.name=="手臂")t.gameObject.SetActive(true);
            foreach(Transform t in r.Passenger)if(t.name=="小腿"||t.name=="left leg"||t.name=="right leg"){r.LegPoses[t]=t.localRotation;t.localRotation=Quaternion.Euler(-65,0,0);}
            homeRides.Add(r);UpdatePartyHandPose();
        }
        Transform MakeHomeCar(Vector3 position,bool cloud)
        {
            var root=new GameObject("伙伴送客车").transform;root.SetParent(transform,false);root.position=position;var a=root.gameObject.AddComponent<RoyalArt>();
            Color pink=new Color(.98f,.48f,.72f),white=new Color(.94f,.96f,1),blue=new Color(.45f,.7f,.91f),dark=new Color(.17f,.18f,.26f);
            a.Piece("车底盘",new Vector3(0,.46f,0),new Vector3(1.8f,.28f,2.8f),cloud?blue:pink);
            a.Piece("前引擎盖",new Vector3(0,.77f,1),new Vector3(1.8f,.42f,.9f),cloud?blue:pink);
            a.Piece("车尾",new Vector3(0,.8f,-1.15f),new Vector3(1.8f,.42f,.5f),cloud?blue:pink);
            foreach(float side in new[]{-1f,1f})
            {
                a.Piece("低矮车门",new Vector3(side*.84f,.77f,-.2f),new Vector3(.14f,.4f,1.75f),cloud?blue:pink);
                a.Piece("后视镜",new Vector3(side*1.05f,1.03f,.55f),new Vector3(.3f,.2f,.18f),white);
                foreach(float z in new[]{-.88f,.92f}){var wheel=a.Piece("圆形车轮",new Vector3(side*.91f,.35f,z),new Vector3(.62f,.12f,.62f),dark,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(0,0,90);var hub=a.Piece("银色轮毂",new Vector3(side*1.045f,.35f,z),new Vector3(.27f,.13f,.27f),white,PrimitiveType.Cylinder);hub.localRotation=Quaternion.Euler(0,0,90);}
                a.Piece("前车灯",new Vector3(side*.6f,.8f,1.48f),new Vector3(.32f,.23f,.07f),new Color(1,.94f,.62f));
                a.Piece("尾灯",new Vector3(side*.6f,.82f,-1.42f),new Vector3(.25f,.16f,.05f),new Color(.95f,.23f,.35f));
            }
            a.Piece("前挡风框下缘",new Vector3(0,1.07f,.58f),new Vector3(1.6f,.07f,.08f),white);
            a.Piece("方向盘",new Vector3(.38f,1.15f,.3f),new Vector3(.27f,.05f,.27f),dark,PrimitiveType.Cylinder);
            if(cloud){foreach(float side in new[]{-1f,1f})a.Piece("云朵飞行翼",new Vector3(side*1.25f,.55f,-.2f),new Vector3(1,.25f,.9f),white,PrimitiveType.Sphere);}
            return root;
        }
        void TickHomeRides()
        {
            for(int i=homeRides.Count-1;i>=0;i--)
            {
                var r=homeRides[i];r.Age+=Time.unscaledDeltaTime;
                if(r.Child&&r.CastleActor!=null)r.End=HomeCarDock(r.CastleActor.position);
                float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((r.Age-1)/4));
                if(r.Car!=null){r.Car.position=Vector3.Lerp(r.Start,r.End,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*3;Vector3 d=r.End-r.Start;d.y=0;if(d.sqrMagnitude>.01f)r.Car.rotation=Quaternion.LookRotation(d);}
                if(r.Age>=6||r.Car==null||r.Passenger==null){FinishHomeRide(r);homeRides.RemoveAt(i);}
            }
        }
        void FinishHomeRide(HomeRide r)
        {
            if(r.Child)
            {
                // Restore the original playground actor (not another follower clone).
                foreach(var style in FindObjectsByType<CastleGirlStyle>(FindObjectsInactive.Include))
                    if(style.StyleId==r.Id&&style.GetComponent<InvitedChildFollower>()==null&&style.GetComponentInParent<PalaceLife>()!=null)
                    {style.gameObject.SetActive(true);var speech=style.GetComponent<NeighbourSpeech>();if(speech!=null)speech.ResetAfterTrip();}
                if(r.Passenger!=null)Destroy(r.Passenger.gameObject);
            }
            else if(r.Passenger!=null)
            {
                r.Passenger.SetParent(r.Parent!=null?r.Parent:transform,true);r.Passenger.localScale=r.Scale;r.Passenger.position=r.Home;r.Passenger.rotation=r.Rotation;
                foreach(var pose in r.LegPoses)if(pose.Key!=null)pose.Key.localRotation=pose.Value;if(r.Citizen!=null)r.Citizen.enabled=true;
                if(r.Life!=null){r.Life.Initialize(this,"散步");r.Life.enabled=true;}
                if(r.Speech!=null){r.Speech.enabled=true;r.Speech.ResetAfterTrip();}
            }
            if(r.Car!=null)Destroy(r.Car.gameObject);
        }
        void CompleteHomeRides(){foreach(var r in homeRides)FinishHomeRide(r);homeRides.Clear();}
        void ResetCompanionReturns(){CompleteHomeRides();companionMenuOpen=false;chatChild=-1;}
        void DrawCompanionMenu(float w,float h)
        {
            float x=w/2-375,y=h/2-265;Panel(new Rect(x,y,750,530),new Color(1,.9f,.65f));
            Label(new Rect(x+22,y+16,640,32),"伙伴 · 聊天和坐车送回",24,new Color(.29f,.18f,.08f));
            Label(new Rect(x+22,y+52,706,28),"X只送小禾回小屋；下面的按钮只管小朋友和星球居民。",16,new Color(.35f,.22f,.13f));
            if(chatChild>=0)
            {
                Label(new Rect(x+22,y+100,706,35),"和城堡小伙伴"+(chatChild+1)+"聊天",21,new Color(.29f,.18f,.08f));
                var style=new GUIStyle(GUI.skin.label){fontSize=20,wordWrap=true};style.normal.textColor=new Color(.29f,.18f,.08f);
                GUI.Label(new Rect(x+22,y+150,706,120),childChatReply,style);
                GUI.SetNextControlName("ChildChat");childChatDraft=GUI.TextField(new Rect(x+22,y+290,540,48),childChatDraft,90);
                if(GUI.Button(new Rect(x+575,y+290,153,48),"发送")){ChatWithChild(chatChild,childChatDraft);childChatDraft="";}
                if(InvitedChildStyles.Contains(chatChild)&&GUI.Button(new Rect(x+22,y+365,330,45),"坐车回水晶城堡")){ReturnChildByCar(chatChild);CloseCompanionMenu();return;}
                if(GUI.Button(new Rect(x+380,y+365,348,45),"返回伙伴名单"))chatChild=-1;
                Label(new Rect(x+22,y+420,706,48),"可以聊：你好、传球、秋千、回城堡。这里是本地游戏对话，不联网。",16,new Color(.35f,.22f,.13f));
            }
            else
            {
                var ids=new List<int>(InvitedChildStyles);if(World.Region==6&&!InVoidRealms)foreach(var c in FindObjectsByType<CastleGirlStyle>())if(c.GetComponentInParent<PalaceLife>()!=null&&!ids.Contains(c.StyleId)&&!ReturningChild(c.StyleId))ids.Add(c.StyleId);
                ids.Sort();var visitors=new List<int>(PlanetHeavenVisitors);visitors.Sort();float height=Mathf.Max(275,(ids.Count+visitors.Count)*65+110);
                companionMenuScroll=GUI.BeginScrollView(new Rect(x+18,y+94,714,318),companionMenuScroll,new Rect(0,0,690,height));float row=4;
                foreach(int id in ids){Label(new Rect(12,row+10,245,35),"城堡小伙伴"+(id+1)+(InvitedChildStyles.Contains(id)?" · 同行":" · 城堡玩耍"),18,new Color(.29f,.18f,.08f));if(GUI.Button(new Rect(276,row+4,110,44),"聊天")){chatChild=id;childChatDraft="";ChatWithChild(id,"你好");}bool enabled=GUI.enabled;GUI.enabled=InvitedChildStyles.Contains(id)&&!CompanionReturnBlocked;if(GUI.Button(new Rect(400,row+4,275,44),"坐车回水晶城堡")){ReturnChildByCar(id);CloseCompanionMenu();GUI.enabled=enabled;GUI.EndScrollView();return;}GUI.enabled=enabled;row+=65;}
                foreach(int id in visitors){Label(new Rect(12,row+10,310,35),"星球居民"+(id+1)+" · 天堂做客",18,new Color(.29f,.18f,.08f));if(GUI.Button(new Rect(400,row+4,275,44),"坐车回下面的星球")){ReturnResidentByCar(id);CloseCompanionMenu();GUI.EndScrollView();return;}row+=65;}
                if(ids.Count+visitors.Count==0)Label(new Rect(12,20,655,95),"目前没有需要送回的伙伴。\n先邀请小朋友，或把星球居民送去天堂。",18,new Color(.29f,.18f,.08f));GUI.EndScrollView();
                bool before=GUI.enabled;GUI.enabled=InvitedChildCount>0;if(GUI.Button(new Rect(x+22,y+425,340,42),"全部小朋友坐车回城堡")){ReturnAllChildren();CloseCompanionMenu();GUI.enabled=before;return;}GUI.enabled=PlanetHeavenVisitors.Count>0;if(GUI.Button(new Rect(x+388,y+425,340,42),"全部居民坐车回星球")){ReturnAllResidents();CloseCompanionMenu();GUI.enabled=before;return;}GUI.enabled=before;
            }
            if(GUI.Button(new Rect(x+235,y+480,280,35),"关闭 · 继续游戏  [Esc]"))CloseCompanionMenu();
        }
    }
}
