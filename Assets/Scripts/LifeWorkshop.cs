using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    bool lifeWorkshopOpen;int lifeTab;Vector2 lifeScroll;Color pencilColor=new Color(.9f,.25f,.43f);DrawingStroke activeStroke;
    string communityDraft="",communityReply="靠近村民、医生、警员或星球居民，再输入你想说的话。";Transform communityPerson;
    public bool LifeWorkshopOpen=>lifeWorkshopOpen;
    public void OpenLifeWorkshop(){if(CompanionReturnBlocked||StickerOpen||PhoneOpen||InspectingHero)return;companionMenuOpen=false;inventoryOpen=false;partyBag=false;travelOpen=false;lifeWorkshopOpen=true;lifeTab=0;lifeScroll=Vector2.zero;communityPerson=NearestCommunityPerson();SetPaused(true);}
    public void CloseLifeWorkshop(){lifeWorkshopOpen=false;activeStroke=null;Save();SetPaused(false);}
    Transform NearestCommunityPerson(){Transform best=null;float d=7;foreach(var n in FindObjectsByType<NpcNavigation>()){if(n==null||!n.gameObject.activeInHierarchy||n.GetComponent<MeadowRabbit>()!=null)continue;float a=Vector3.Distance(Player.transform.position,n.transform.position);if(a<d){best=n.transform;d=a;}}foreach(var n in FindObjectsByType<Villager>()){float a=Vector3.Distance(Player.transform.position,n.transform.position);if(a<d){best=n.transform;d=a;}}return best;}
    public bool SubmitCommunityChat(string message)
    {
        if(communityPerson==null||string.IsNullOrWhiteSpace(message)){communityReply="请先靠近一个居民，再打开这里聊天。";return false;}string s=message.Trim();if(s.Length>100)s=s.Substring(0,100);string role=communityPerson.name;
        if(DiscloseSecretDoorTo(communityPerson,s)){communityReply=Notice;communityDraft="";return true;}
        if(s.Contains("密码")){communityReply="这里只处理你主动分享的游戏门密码；不复述或保存真实账户密码。";communityDraft="";return true;}
        communityReply=role+"："+(s.Contains("医院")||s.Contains("受伤")||s.Contains("医生")?"有巡诊医生在各地活动。点下方的治疗按钮，请身边的医生检查。":s.Contains("警察")||s.Contains("安全")?"警员会沿公共道路巡逻；不拿走别人的物品，村庄就不会警戒。":s.Contains("种田")||s.Contains("小麦")?"我们会到麦田浇水和收割，再把面包送到餐桌。你也能帮忙，靠近农田按G。":s.Contains("房")||s.Contains("建")?"你可以自己逐块搭房，也能在工坊选择前方空地建小屋。材料齐了就能建，屋下还能挖密室。":s.Contains("水晶")||s.Contains("合金")?"水晶矿谷有石头包着的水晶矿石；下界合金矿境有合金矿。背包配方本能查整套装备。":s.Contains("你好")?"你好！今天也一起把村子照顾好吧。":s.Contains("派对")||s.Contains("邀请")?"真不错！在工坊自己画邀请函，写好主题、人数和活动内容，再发出邀请。":"我听到了你说“"+s+"”。我们可以聊种田、建房、装备、派对和附近的服务。" );communityDraft="";return true;
    }
    public bool SendDrawnInvitation()
    {
        return DeliverInvitationTo(NearbyPerson());
    }
    public void AddDrawingPoint(Vector2 normalized,bool begin){normalized.x=Mathf.Clamp01(normalized.x);normalized.y=Mathf.Clamp01(normalized.y);if(begin||activeStroke==null){if(Drawing.Count>=1000)return;activeStroke=new DrawingStroke{color=pencilColor};Drawing.Add(activeStroke);}if(activeStroke.points.Count<3000)activeStroke.points.Add(normalized);}
    void DrawPaper(Rect area)
    {
        Panel(area,new Color(1,.985f,.94f));foreach(var stroke in Drawing)for(int i=1;i<stroke.points.Count;i++)PaperLine(area.position+Vector2.Scale(stroke.points[i-1],area.size),area.position+Vector2.Scale(stroke.points[i],area.size),stroke.color,3);
        var e=Event.current;if((e.type==EventType.MouseDown||e.type==EventType.MouseDrag)&&e.button==0&&area.Contains(e.mousePosition)){AddDrawingPoint(new Vector2((e.mousePosition.x-area.x)/area.width,(e.mousePosition.y-area.y)/area.height),e.type==EventType.MouseDown);e.Use();}if(e.type==EventType.MouseUp)activeStroke=null;
    }
    void PaperLine(Vector2 a,Vector2 b,Color color,float width){Matrix4x4 before=GUI.matrix;Color prior=GUI.color;Vector2 d=b-a;GUI.matrix=before*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg),Vector3.one);GUI.color=color;GUI.DrawTexture(new Rect(0,-width/2,d.magnitude,width),Texture2D.whiteTexture);GUI.color=prior;GUI.matrix=before;}
    void DrawLifeWorkshop(float w,float h)
    {
        if(lifeTab==3){DrawCompactCitizenChat(w,h);return;}
        float x=w/2-450,y=h/2-295;Panel(new Rect(x,y,900,590),new Color(.94f,.95f,.91f));Panel(new Rect(x,y,900,6),new Color(.36f,.65f,.6f));Label(new Rect(x+24,y+20,760,38),"生活工坊 · 画画、穿戴和建造",26,Ink);
        string[] tabs={"纸笔与邀请函","装备","自由建房","居民聊天"};for(int i=0;i<4;i++)if(GUI.Button(new Rect(x+24+i*213,y+68,204,37),tabs[i])){lifeTab=i;lifeScroll=Vector2.zero;}
        if(lifeTab==0)
        {
            Label(new Rect(x+26,y+115,630,25),"生活画具一直可用 · 长按鼠标画画，画好后制作邀请函",16,Muted);DrawPaper(new Rect(x+26,y+150,565,325));
            for(int i=0;i<24;i++){Color c=i==0?Color.black:i==23?Color.white:Color.HSVToRGB((i-1)/21f,i>14?.35f:.8f,.95f);float px=x+30+(i%12)*46,py=y+483+(i/12)*23;if(GUI.Button(new Rect(px,py,41,20),""))pencilColor=c;Panel(new Rect(px+4,py+3,33,14),c);}
            Label(new Rect(x+615,y+119,260,28),"画好一张，制作一张",21,Ink);InvitationTitle=GUI.TextField(new Rect(x+615,y+160,259,35),InvitationTitle,40);InvitationDetails=GUI.TextArea(new Rect(x+615,y+206,259,105),InvitationDetails,120);Label(new Rect(x+615,y+323,259,30),"参加人数："+InvitationGuests+" 位",18,Ink);
            if(GUI.Button(new Rect(x+615,y+363,120,36),"少一位"))InvitationGuests=Mathf.Max(1,InvitationGuests-1);if(GUI.Button(new Rect(x+752,y+363,122,36),"多一位"))InvitationGuests=Mathf.Min(8,InvitationGuests+1);
            if(GUI.Button(new Rect(x+615,y+407,259,37),"制作一张邀请函"))CreateInvitationCard();if(GUI.Button(new Rect(x+615,y+450,259,37),"拿起邀请函 · 到人面前按Enter递送")){HoldInvitation();return;}if(GUI.Button(new Rect(x+615,y+494,120,31),"撤回一笔")&&Drawing.Count>0)Drawing.RemoveAt(Drawing.Count-1);if(GUI.Button(new Rect(x+752,y+494,122,31),"保存画稿"))Save();
        }
        if(lifeTab==1){Label(new Rect(x+25,y+119,850,28),"水晶、合金锭是材料；帽、衣、裤、鞋才是穿戴的整套装备。",17,Ink);if(GUI.Button(new Rect(x+25,y+158,205,36),"做整套水晶装（含权杖）"))CraftArmorSet(false);if(GUI.Button(new Rect(x+243,y+158,195,36),"穿整套水晶装"))WearArmorSet(false);if(GUI.Button(new Rect(x+454,y+158,205,36),"做整套合金装（含剑）"))CraftArmorSet(true);if(GUI.Button(new Rect(x+673,y+158,200,36),"穿整套合金装"))WearArmorSet(true);lifeScroll=GUI.BeginScrollView(new Rect(x+25,y+207,850,303),lifeScroll,new Rect(0,0,820,600));for(int i=0;i<10;i++){int id=i<5?81+i:86+i-5;Label(new Rect(12,i*54+8,480,40),Blocks.Names[id]+"  × "+Inventory[id],19,Ink);if(GUI.Button(new Rect(480,i*54+3,150,42),id==85||id==90?"拿在手里":"穿上 / 脱下"))WearArmor(id);if(GUI.Button(new Rect(650,i*54+3,155,42),"查看九宫格配方")){for(int r=0;r<Blocklands.Crafting.Recipes.Length;r++)if((int)Blocklands.Crafting.Recipes[r].Output==id){ShowRecipe(r);break;}lifeWorkshopOpen=false;OpenCrafting();GUI.EndScrollView();return;}}Label(new Rect(12,555,800,35),"水晶权杖：拿起后，鼠标左键施放蓝色魔法光弹。",16,Muted);GUI.EndScrollView();}
        if(lifeTab==2){Label(new Rect(x+26,y+135,845,90),"基地建造：左边建完整基地，右边给当前木屋补地下室。\n派对扩建：看着自家按B；E拿用品，N天黑迎宾。\n保留原保险箱、柜子与地下室；未知边界物品不会被清空。",18,Ink);if(GUI.Button(new Rect(x+26,y+260,400,45),"免费创建完整基地：房屋＋密室")){CreateCompleteSecretBase();return;}if(GUI.Button(new Rect(x+454,y+260,420,45),"免费改造当前木屋：10×10密室")){AdaptCurrentShelter();return;}
            var secretHome=FindSecretHome();Label(new Rect(x+26,y+318,820,62),secretHome==null?"还没有自己的房屋：V建旧家，或点左上按钮自由建房。":(secretHome.secretRoomVersion==1?"我的10×10密室已建好：保险箱、柜子、秋千、监控在里面。":"密室尚未补齐：V旧家免费；其他新密室144石砖，旧密室扩大48石砖。")+"\n先点下方“带我到密室入口”，再轻按H开木板伪装门。",17,Muted);
            if(GUI.Button(new Rect(x+26,y+382,400,32),TsunamiActive?"停止海啸演练 · `键":"主动启动海啸演练 · `键")){if(StartTsunami()){CloseLifeWorkshop();return;}}
            if(GUI.Button(new Rect(x+454,y+382,420,32),"模拟村民试锁后离开")){var home=NearestOwnedHome();if(StartRoomVisitor(home)){CloseLifeWorkshop();return;}Notice="先建好附近自建房的密室。";}
            if(GUI.Button(new Rect(x+26,y+422,400,45),VillageGuardianEnabled?"收回村庄铁傀儡":"让村庄铁傀儡出现")){VillageGuardianEnabled=!VillageGuardianEnabled;RefreshVillageGuardian();Save();}
            if(GUI.Button(new Rect(x+454,y+422,420,45),"免费扩建九层基地 · 封好地面")){UpgradeNineLayerBase();return;}
            if(GUI.Button(new Rect(x+26,y+478,400,39),"带我到密室入口 / 显示定位点")){GoToSecretEntrance();return;}
            if(GUI.Button(new Rect(x+454,y+478,420,39),"随时查看我家实时监控")){OpenSecretMonitor();return;}}
        if(lifeTab==3){Label(new Rect(x+26,y+130,845,34),communityPerson==null?"附近没有居民；靠近后再打开聊天。":"正在和“"+communityPerson.name+"”聊天",20,Ink);var style=new GUIStyle(GUI.skin.label){fontSize=19,wordWrap=true};style.normal.textColor=Ink;GUI.Label(new Rect(x+26,y+184,845,135),communityReply,style);communityDraft=GUI.TextField(new Rect(x+26,y+340,680,42),communityDraft,100);if(GUI.Button(new Rect(x+724,y+340,150,42),"发送"))SubmitCommunityChat(communityDraft);
            bool enabled=GUI.enabled;GUI.enabled=HasInvitationRequest;if(GUI.Button(new Rect(x+26,y+415,400,43),"同意打开附近居民的邀请条")){OpenInvitationRequest();GUI.enabled=enabled;return;}GUI.enabled=enabled;if(GUI.Button(new Rect(x+454,y+415,420,43),"请附近医生治疗"))AskCommunityDoctor();}
        Label(new Rect(x+26,y+537,665,45),Notice,14,Muted);if(GUI.Button(new Rect(x+718,y+539,156,34),"关闭 / Esc"))CloseLifeWorkshop();
    }
    void DrawCompactCitizenChat(float w,float h)
    {
        float x=w/2-450,y=h-225;Panel(new Rect(x,y,900,205),new Color(1,.92f,.67f,.96f));Label(new Rect(x+18,y+10,600,28),communityPerson==null?"靠近居民后聊天":"和“"+communityPerson.name+"”聊天",20,Ink);if(GUI.Button(new Rect(x+732,y+9,150,29),"返回生活工坊"))lifeTab=0;
        var style=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true};style.normal.textColor=Ink;GUI.Label(new Rect(x+18,y+47,864,60),communityReply,style);
        communityDraft=GUI.TextField(new Rect(x+18,y+112,690,34),communityDraft,100);if(GUI.Button(new Rect(x+732,y+112,150,34),"发送"))SubmitCommunityChat(communityDraft);
        bool enabled=GUI.enabled;GUI.enabled=HasInvitationRequest;if(GUI.Button(new Rect(x+18,y+159,395,30),"同意打开小朋友 / 居民的邀请条")){OpenInvitationRequest();GUI.enabled=enabled;return;}GUI.enabled=enabled;if(GUI.Button(new Rect(x+433,y+159,235,30),"请身边的医生治疗"))AskCommunityDoctor();if(GUI.Button(new Rect(x+732,y+159,150,30),"关闭 / Esc"))CloseLifeWorkshop();
    }
}}
