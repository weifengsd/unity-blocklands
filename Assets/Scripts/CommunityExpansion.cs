using UnityEngine;
namespace Blocklands
{
    public class CommunityCitizen:MonoBehaviour
    {
        public Sandbox Game;public string Role;public Vector3 Home,Farm;public string Activity {get;private set;}="散步";float cycle;
        public void Initialize(Sandbox g,string role,Vector3 home){Game=g;Role=role;Home=home;Farm=home;NpcNavigation.Ensure(transform);}
        void Update()
        {
            if(Game==null||Game.Paused||FloodResident.IsDown(transform))return;cycle+=Time.deltaTime;float t=Mathf.Repeat(cycle,24);Vector3 goal=Home+new Vector3(Mathf.Sin(cycle*.15f)*5,0,Mathf.Cos(cycle*.15f)*4);
            if(Role=="农民"){Activity=t<7?"去麦田":t<14?"浇水收割":"带面包回家";goal=t<14?Farm:Home;}
            else Activity=Role=="医生"?"巡诊":Role=="警员"?"巡逻":"散步聊天";
            NpcNavigation.Ensure(transform).MoveTo(goal,1.5f,Time.deltaTime);Vector3 d=goal-transform.position;d.y=0;if(d.sqrMagnitude>.3f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(d),Time.deltaTime*3);
            foreach(Transform p in transform)if(p.name=="手臂")p.localRotation=Quaternion.Euler(Role=="农民"&&t>=7&&t<14?-40+Mathf.Sin(cycle*5)*18:Mathf.Sin(cycle*2)*12,0,0);
            var can=transform.Find("浇水壶");if(can!=null)can.gameObject.SetActive(Role=="农民"&&t>=7&&t<14);
        }
    }
    public class MeadowRabbit:MonoBehaviour
    {
        public int MagicHits {get;private set;}public void MagicStartle(){MagicHits++;home=transform.position+(transform.position-Game.Player.transform.position).normalized*4;age=0;if(figure!=null)figure.localPosition=Vector3.up*.5f;}
        public Sandbox Game;Vector3 home;Transform figure;float age;
        public void Initialize(Sandbox g){Game=g;home=transform.position;figure=transform.Find("兔子模型");}
        void Update(){if(Game==null||Game.Paused)return;age+=Time.deltaTime;Vector3 goal=home+new Vector3(Mathf.Sin(age*.6f)*3,0,Mathf.Cos(age*.43f)*3);if(Vector3.Distance(Game.Player.transform.position,transform.position)<2.5f)goal=transform.position+(transform.position-Game.Player.transform.position).normalized*3;NpcNavigation.Ensure(transform).MoveTo(goal,2.8f,Time.deltaTime);if(figure!=null)figure.localPosition=Vector3.up*Mathf.Max(0,Mathf.Sin(age*5))*.22f;}
    }
    public class VillageGuardian:MonoBehaviour
    {
        public float CrystalStunnedUntil {get;private set;}public void StunByCrystal(){CrystalStunnedUntil=Time.time+2;}
        public Sandbox Game;Vector3 home;float age;
        public void Initialize(Sandbox g){Game=g;home=transform.position;NpcNavigation.Ensure(transform);var c=GetComponent<CapsuleCollider>();if(c!=null){c.height=3.6f;c.radius=.7f;c.center=Vector3.up*1.8f;}}
        void Update(){if(Game==null||Game.Paused||FloodResident.IsDown(transform)||Time.time<CrystalStunnedUntil)return;age+=Time.deltaTime;bool alert=Time.time<Game.VillageAngryUntil;Vector3 target=alert?Game.Player.transform.position:home+new Vector3(Mathf.Sin(age*.18f)*7,0,Mathf.Cos(age*.18f)*7);NpcNavigation.Ensure(transform).MoveTo(target,alert?3.6f:1.4f,Time.deltaTime);var d=target-transform.position;d.y=0;if(d.sqrMagnitude>.1f)transform.rotation=Quaternion.LookRotation(d);if(alert&&Game.CanReachPlayer(transform.position+Vector3.up,1.8f))Game.Damage(.5f,"铁傀儡保护村民，挡住了你");}
    }
    public class CommunityActivity:MonoBehaviour
    {
        public Sandbox Game;public string Kind;public int Count;float again;
        public bool Interact(){if(Game==null||Game.Paused||Time.time<again)return false;again=Time.time+2;
            if(Kind=="采摘果园"){Game.Inventory[(int)Block.Apple]=Mathf.Min(9999,Game.Inventory[(int)Block.Apple]+1);Game.Notice="摘下一颗苹果，R可以吃；果树需要20秒长出新果。";again=Time.time+20;return true;}
            if(Kind=="星星寻宝"){gameObject.SetActive(false);Game.HeavenStarsFound++;Game.Notice="找到星星 "+Game.HeavenStarsFound+" / 8";if(Game.HeavenStarsFound==8)Game.EarnActivityCoins(32,"天堂星星寻宝");return true;}
            if(Kind=="花坛拼图"){Count++;foreach(var r in GetComponentsInChildren<Renderer>())r.sharedMaterial.color=Color.HSVToRGB(Count*.22f%1,.35f,.9f);if(Count%3==0)Game.EarnActivityCoins(20,"花坛拼图");else Game.Notice="花坛拼图 "+(Count%3)+" / 3，继续按G拼好三种颜色。";return true;}
            Game.Inventory[(int)Block.Bread]=Mathf.Min(9999,Game.Inventory[(int)Block.Bread]+1);Game.Notice="农民把收获的小麦烤成了面包，送给了你。";again=Time.time+20;return true;}
    }
    public partial class Sandbox
    {
        Transform guardian;float doctorRemaining;public int HeavenStarsFound {get;internal set;}
        RoyalArt MakeCommunityPerson(Transform parent,string name,Vector3 position,Color shirt,string role)
        {
            var root=new GameObject(name).transform;root.SetParent(parent,false);root.position=position;var a=root.gameObject.AddComponent<RoyalArt>();Color skin=new Color(.76f,.58f,.44f),dark=new Color(.18f,.14f,.16f);
            a.Piece("长外衣",new Vector3(0,.9f,0),new Vector3(.63f,1.05f,.4f),shirt);a.Piece("腰带",new Vector3(0,.72f,0),new Vector3(.67f,.07f,.43f),Color.Lerp(shirt,Color.black,.35f));a.Piece("方块头",new Vector3(0,1.72f,0),new Vector3(.56f,.6f,.54f),skin);a.Piece("头发",new Vector3(0,2.01f,-.02f),new Vector3(.61f,.16f,.58f),dark);
            foreach(float side in new[]{-1f,1f}){a.Piece("眼白",new Vector3(side*.14f,1.77f,.28f),new Vector3(.13f,.08f,.025f),Color.white);a.Piece("眼珠",new Vector3(side*.14f,1.77f,.298f),new Vector3(.055f,.065f,.02f),new Color(.19f,.34f,.26f));a.Piece("眉毛",new Vector3(side*.14f,1.85f,.29f),new Vector3(.14f,.025f,.02f),dark);a.Piece("手臂",new Vector3(side*.43f,1.05f,.03f),new Vector3(.2f,.65f,.22f),shirt);a.Piece("手",new Vector3(side*.43f,.7f,.03f),new Vector3(.2f,.2f,.22f),skin);a.Piece("鞋",new Vector3(side*.18f,.17f,.04f),new Vector3(.26f,.32f,.35f),dark);}
            a.Piece("鼻子",new Vector3(0,1.65f,.35f),new Vector3(.13f,.23f,.16f),skin);a.Piece("嘴巴",new Vector3(0,1.49f,.28f),new Vector3(.15f,.035f,.025f),dark);
            if(role=="农民"){a.Piece("草帽",new Vector3(0,2.05f,0),new Vector3(.85f,.12f,.75f),new Color(.83f,.68f,.37f));a.Piece("浇水壶",new Vector3(.44f,.72f,.32f),new Vector3(.28f,.26f,.32f),new Color(.26f,.55f,.46f));}
            if(role=="医生"){a.Piece("医生胸牌",new Vector3(.15f,1.3f,.215f),new Vector3(.13f,.2f,.02f),new Color(.75f,.2f,.27f));}
            if(role=="警员"){a.Piece("警帽",new Vector3(0,2.06f,0),new Vector3(.65f,.13f,.6f),new Color(.1f,.22f,.37f));a.Piece("徽章",new Vector3(.14f,1.3f,.22f),new Vector3(.11f,.15f,.02f),new Color(1,.8f,.3f));}
            root.gameObject.AddComponent<CommunityCitizen>().Initialize(this,role,position);return a;
        }
        Vector3 SurfacePoint(int x,int z){for(int y=65;y>=0;y--)if(Blocks.Solid(World.Get(new Vector3Int(x,y,z))))return new Vector3(x+.5f,y+1.03f,z+.5f);return new Vector3(x,15,z);}
        void BuildCommunityExpansion()
        {
            var root=new GameObject("生活扩建 · 村庄与天堂").transform;root.SetParent(villagePeople.transform,false);var art=root.gameObject.AddComponent<RoyalArt>();HeavenStarsFound=0;guardian=null;doctorRemaining=0;
            if(World.Region==1)
            {
                for(int i=6;i<VoxelWorld.VillagePlots.Length;i++){var p=VoxelWorld.VillagePlots[i];var farmer=MakeCommunityPerson(root,"村庄农民 "+(i+1),new Vector3(p.x+4.5f,15.03f,p.y-1.4f),Color.HSVToRGB(i*.13f%1,.4f,.73f),"农民");farmer.GetComponent<CommunityCitizen>().Farm=new Vector3(58+i%3*4,15.03f,57+i%3*2);}
                for(int i=0;i<12;i++)BuildRabbit(root,new Vector3(57+i%4*3,15.03f,49+i/4*3),i);
                for(int i=0;i<24;i++){Vector3 p=new Vector3(5+i%8*11,15,58+i/8*4);art.Piece("草丛",p+Vector3.up*.16f,new Vector3(.15f,.4f,.16f),new Color(.32f,.58f,.26f));art.Piece("野花",p+new Vector3(.15f,.38f,0),new Vector3(.2f,.17f,.2f),Color.HSVToRGB(i*.13f%1,.5f,.96f),PrimitiveType.Sphere);}
                var harvest=art.Piece("居民收获餐桌",new Vector3(60,15.6f,68),new Vector3(3,.6f,1.3f),new Color(.68f,.43f,.27f),PrimitiveType.Cube,true);harvest.gameObject.AddComponent<CommunityActivity>().Game=this;
            }
            if(World.Region!=3&&World.Region!=4&&World.Region!=5&&World.Region<11||World.Region==12||World.Region==16)
            {
                Vector3 point=World.Region==1?new Vector3(55,15.03f,70):World.Region==16?new Vector3(30,15.03f,58):SurfacePoint(53,53);
                MakeCommunityPerson(root,"巡诊医生",point,Color.white,"医生");MakeCommunityPerson(root,"公共道路巡逻警员",point+Vector3.right*4,new Color(.22f,.37f,.58f),"警员");
                art.Piece("巡诊遮阳棚",point+Vector3.up*3.2f,new Vector3(5,.18f,4),new Color(.79f,.93f,.9f));foreach(float side in new[]{-2f,2f})art.Piece("遮阳棚支柱",point+new Vector3(side,1.6f,1),new Vector3(.14f,3.2f,.14f),new Color(.5f,.6f,.57f));PlaceSign(root,"巡诊医生与巡逻警员 · Esc生活工坊聊天",point+new Vector3(0,2.4f,-1.5f));
            }
            foreach(var age in FindObjectsByType<PlanetCitizenAge>())if(!PlanetHeavenVisitors.Contains(age.CitizenId)&&age.Age==PlanetAge.Adult&&age.CitizenId<22&&age.CitizenId%3==2)age.GetComponent<RealmResidentLife>().AssignFarm(new Vector3(92+age.CitizenId%4*3,PlanetFloor+1.03f,68+age.CitizenId%2*3));
            if(World.Region==2)for(int x=43;x<=49;x+=2)for(int y=16;y<=20;y++){var rootVein=new GameObject("石头里的亮蓝水晶").transform;rootVein.SetParent(root,false);rootVein.position=new Vector3(x+.5f,y+.5f,41.5f);var gem=rootVein.gameObject.AddComponent<RoyalArt>();foreach(float side in new[]{-.24f,0,.24f}){var piece=gem.Piece("矿石蓝晶",new Vector3(side,side*.5f,-.51f),new Vector3(.13f,.22f,.08f),new Color(.15f,.7f,.99f));piece.localRotation=Quaternion.Euler(0,0,20);gem.Piece("晶面亮点",new Vector3(side+.025f,side*.5f+.05f,-.56f),Vector3.one*.035f,new Color(.86f,.98f,1));}var detail=rootVein.gameObject.AddComponent<CrystalVeinDetail>();detail.Game=this;detail.Cell=new Vector3Int(x,y,41);}
            BuildHeavenPlayGarden(root,art);BuildSavedRealmHomes(root);RefreshVillageGuardian();
            foreach(var speech in FindObjectsByType<NeighbourSpeech>())NpcNavigation.Ensure(speech.transform);
            foreach(var person in FindObjectsByType<RoyalArt>())if(person.name.Contains("店员")||person.name.Contains("服务员")||person.name.Contains("医生")||person.name.Contains("警官"))NpcNavigation.Ensure(person.transform);
        }
        void BuildRabbit(Transform parent,Vector3 at,int i)
        {
            var root=new GameObject("草地兔子 "+i).transform;root.SetParent(parent,false);root.position=at;var body=root.gameObject.AddComponent<BoxCollider>();body.center=Vector3.up*.35f;body.size=new Vector3(.45f,.6f,.6f);var f=new GameObject("兔子模型").transform;f.SetParent(root,false);var a=f.gameObject.AddComponent<RoyalArt>();Color c=i%2==0?new Color(.87f,.84f,.76f):new Color(.62f,.53f,.44f);a.Piece("兔子身体",Vector3.up*.32f,new Vector3(.45f,.42f,.65f),c);a.Piece("兔子头",new Vector3(0,.5f,.3f),Vector3.one*.36f,c);foreach(float x in new[]{-.11f,.11f}){a.Piece("长耳朵",new Vector3(x,.86f,.25f),new Vector3(.12f,.48f,.12f),c);a.Piece("眼睛",new Vector3(x,.55f,.495f),new Vector3(.035f,.05f,.02f),Color.black);}a.Piece("尾巴",new Vector3(0,.4f,-.37f),Vector3.one*.2f,Color.white);root.gameObject.AddComponent<MeadowRabbit>().Initialize(this);
        }
        public void RefreshVillageGuardian()
        {
            if(guardian!=null)Destroy(guardian.gameObject);guardian=null;if(World.Region!=1||!VillageGuardianEnabled)return;var r=new GameObject("可選的村庄铁傀儡").transform;r.SetParent(villagePeople.transform,false);r.position=new Vector3(31,15.03f,62);var a=r.gameObject.AddComponent<RoyalArt>();Color iron=new Color(.77f,.8f,.78f),vine=new Color(.26f,.44f,.28f);a.Piece("宽铁胸甲",new Vector3(0,1.8f,0),new Vector3(1.35f,1.5f,.7f),iron);a.Piece("方铁头",new Vector3(0,2.95f,0),new Vector3(.75f,.75f,.75f),iron);foreach(float side in new[]{-1f,1f}){a.Piece("铁胳膊",new Vector3(side*.9f,1.65f,0),new Vector3(.4f,1.7f,.43f),iron);a.Piece("铁腿",new Vector3(side*.35f,.55f,0),new Vector3(.45f,1.1f,.48f),iron);a.Piece("眼睛",new Vector3(side*.19f,3.02f,.38f),new Vector3(.11f,.08f,.025f),new Color(.63f,.15f,.18f));}a.Piece("绿藤",new Vector3(.35f,1.95f,.37f),new Vector3(.15f,1.25f,.04f),vine);r.gameObject.AddComponent<VillageGuardian>().Initialize(this);guardian=r;ApplyNpcCombatView(r);
        }
        public bool AskCommunityDoctor()
        {
            foreach(var person in FindObjectsByType<NpcNavigation>())if((person.name.Contains("医生")||person.name.Contains("护士"))&&Vector3.Distance(person.transform.position,Player.transform.position)<6){doctorRemaining=6;lifeWorkshopOpen=false;SetPaused(false);Notice="医生开始检查，六秒后恢复生命。";return true;}Notice="先走到医生身边，再请求治疗。";return false;
        }
        void TickLifestyle(){TickArmorAppearance();if(doctorRemaining>0){doctorRemaining=Mathf.Max(0,doctorRemaining-Time.unscaledDeltaTime);if(doctorRemaining==0){Health=10;Notice="医生检查完成，生命已恢复。";}}}
        public bool TryCommunityActivityG(){if(Paused)return false;CommunityActivity best=null;float d=3;foreach(var a in FindObjectsByType<CommunityActivity>()){float n=Vector3.Distance(a.transform.position,Player.transform.position+Vector3.up);if(n<d){best=a;d=n;}}return best!=null&&best.Interact();}
        void BuildHeavenPlayGarden(Transform root,RoyalArt a)
        {
            float floor=HeavenFloor+1;Color pink=new Color(.95f,.63f,.77f),mint=new Color(.48f,.76f,.68f),gold=new Color(.95f,.81f,.49f);
            foreach(var p in new[]{new Vector3(-45,floor,-15),new Vector3(109,floor,70)})BuildRealmHome(root,p,"天堂花园家",new Color(.93f,.91f,1),pink);
            a.Piece("天堂花园步道",new Vector3(32,floor+.03f,85),new Vector3(155,.05f,2.5f),new Color(.93f,.87f,.74f));
            for(int i=0;i<36;i++){Vector3 p=new Vector3(-44+i%18*9,floor,81+i/18*8);a.Piece("花园花茎",p+Vector3.up*.22f,new Vector3(.07f,.4f,.07f),mint);for(int petal=0;petal<5;petal++){float t=petal*Mathf.PI*2/5;a.Piece("彩色花瓣",p+new Vector3(Mathf.Cos(t)*.16f,.43f,Mathf.Sin(t)*.16f),Vector3.one*.18f,Color.HSVToRGB(i*.12f%1,.35f,.96f),PrimitiveType.Sphere);}}
            Vector3 gazebo=new Vector3(6,floor,67);foreach(float side in new[]{-3f,3f})foreach(float z in new[]{-2f,2f})a.Piece("花园凉亭支柱",gazebo+new Vector3(side,1.7f,z),new Vector3(.22f,3.4f,.22f),Color.white);a.Taper("花园凉亭花瓣屋顶",gazebo+Vector3.up*3.7f,new Vector3(8,.7f,6),pink,.45f);
            for(int i=0;i<3;i++){Vector3 p=new Vector3(-28+i*24,floor,105);a.Piece("野餐长桌",p+Vector3.up*.85f,new Vector3(4,.2f,1.8f),gold,PrimitiveType.Cube,true);foreach(float z in new[]{-1.4f,1.4f}){var bench=a.Piece("野餐长椅",p+new Vector3(0,.4f,z),new Vector3(4,.25f,.7f),pink,PrimitiveType.Cube,true);AddRealmPoint(bench,RealmUseKind.Bed,"G坐野餐长椅 / 起身");}var food=a.Piece("野餐草莓点心",p+new Vector3(0,1.12f,0),Vector3.one*.3f,pink,PrimitiveType.Sphere,true);AddRealmPoint(food,RealmUseKind.Food,"G 拿起来吃：草莓点心");}
            for(int i=0;i<6;i++){Vector3 p=new Vector3(-35+i*24,floor,91);var tree=a.Piece("果树树干",p+Vector3.up*1.3f,new Vector3(.45f,2.6f,.45f),new Color(.53f,.36f,.25f));a.Piece("果树叶冠",p+Vector3.up*3,new Vector3(3,2.3f,3),mint);var fruit=a.Piece("G采摘果园",p+new Vector3(0,1.5f,-1),Vector3.one*.4f,pink,PrimitiveType.Sphere,true);var use=fruit.gameObject.AddComponent<CommunityActivity>();use.Game=this;use.Kind="采摘果园";}
            for(int i=0;i<8;i++){var star=a.Piece("天堂可寻找的星星 "+i,new Vector3(-36+i%4*8,floor+.7f,72+i/4*6),Vector3.one*.5f,gold,PrimitiveType.Sphere,true);var use=star.gameObject.AddComponent<CommunityActivity>();use.Game=this;use.Kind="星星寻宝";}
            var bed=a.Piece("G花坛拼图",new Vector3(108,floor+.4f,85),new Vector3(3,.7f,3),mint,PrimitiveType.Cube,true);var puzzle=bed.gameObject.AddComponent<CommunityActivity>();puzzle.Game=this;puzzle.Kind="花坛拼图";
            for(int type=4;type<=6;type++)
            {
                Vector3 at=new Vector3(50+(type-4)*23,floor,84);var ride=new GameObject(type==4?"云河小船":type==5?"星星蹦床":"长云滑梯").transform;ride.SetParent(root,false);ride.position=at;var art=ride.gameObject.AddComponent<RoyalArt>();
                if(type==4){art.Piece("云河水面",new Vector3(0,.12f,0),new Vector3(13,.15f,8),new Color(.42f,.77f,.88f));art.Piece("云河小船船底",new Vector3(0,.65f,0),new Vector3(2,.3f,3),gold);foreach(float x in new[]{-1f,1f})art.Piece("船舷",new Vector3(x,.9f,0),new Vector3(.16f,.5f,3),pink);}
                if(type==5){var disc=art.Piece("蹦床",new Vector3(0,.5f,0),new Vector3(5,.3f,5),new Color(.48f,.52f,.76f),PrimitiveType.Cylinder,true);CastleBallPlay.UseFlatDiscCollider(disc);foreach(float x in new[]{-2.2f,2.2f})art.Piece("蹦床扶手",new Vector3(x,1.2f,0),new Vector3(.12f,2.4f,.12f),gold);}
                if(type==6){var slope=art.Piece("长长云滑梯",new Vector3(0,1.5f,-1),new Vector3(2,.2f,7),mint);slope.localRotation=Quaternion.Euler(-28,0,0);for(int i=0;i<7;i++)art.Piece("滑梯楼梯",new Vector3(0,i*.35f,2+i*.25f),new Vector3(2,.3f,.5f),pink);}
                var button=art.Piece("G开始游玩",new Vector3(3.7f,1,-1.5f),Vector3.one*.5f,pink,PrimitiveType.Cube,true);var point=AddRealmPoint(button,RealmUseKind.PlayRide,"G "+ride.name,ride);point.Variant=type;PlaceSign(root,ride.name+" · G开始 / 提前结束",at+new Vector3(0,2,-5));
            }
            PlaceSign(root,"天堂大花园：果园、寻宝、蹦床、云河小船、长滑梯",new Vector3(32,floor+2,81));
        }
    }
}
