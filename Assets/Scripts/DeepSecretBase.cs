using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    public class DeepRoomObject:MonoBehaviour {public OwnedHome Home;public int Floor;public string Activity;public Transform Animated;public Material ScreenMaterial;void OnDestroy(){if(ScreenMaterial!=null)Destroy(ScreenMaterial);}}
    public partial class Sandbox
    {
        public static readonly string[] DeepRoomNames={"海啸监控室","无人机观察室","温暖洗漱房","地下花园","甜点厨房","故事图书室","彩灯游戏房","奖品收藏室","星光休息房"};
        OwnedHome chairHome,activityHome;Transform secretChair;bool chairMenu,chairTravelling,chairArrived;int chairDestination;float chairAge,chairDuration;Vector3 chairFrom,chairTo,chairEntry;bool chairEntryFlying;
        float deepActivityAge;int deepActivityFloor;string deepActivity="";Transform deepAnimated;
        public bool SecretChairActive=>secretChair!=null;
        public bool SecretChairMenuOpen=>chairMenu;
        public bool DeepActivityActive=>!string.IsNullOrEmpty(deepActivity);
        public bool DeepInputCaptured=>SecretChairActive||DeepActivityActive||DronePanelOpen;
        public Vector3 DeepSafePosition=>SecretChairActive?chairEntry:DronePanelOpen?dronePlayerEntry:activityEntry;
        Vector3 activityEntry;bool activityFlying;
        readonly HashSet<OwnedHome> chairAuthorized=new HashSet<OwnedHome>();
        public static float DeepFloorY(OwnedHome h,int floor)=>h.y-5-(floor-1)*8;
        public static Vector3 DeepCorner(OwnedHome h,int floor)=>floor==0?new Vector3(h.x+3,h.y+2.04f,h.z+3):new Vector3(h.x+1.7f,DeepFloorY(h,floor)+.04f,h.z+8.2f);
        public static Vector3 DeepLanding(OwnedHome h,int floor)=>DeepCorner(h,floor)+(floor==0?Vector3.right*1.7f:Vector3.right*1.5f+Vector3.up*.04f);
        public int CurrentDeepFloor(OwnedHome h){var p=Player.transform.position;if(h==null||h.region!=World.Region||p.x<h.x-1||p.x>h.x+11||p.z<h.z-1||p.z>h.z+11)return -1;if(Mathf.Abs(p.y-(h.y+2))<2)return 0;for(int f=1;f<=9;f++)if(p.y>=DeepFloorY(h,f)-.3f&&p.y<DeepFloorY(h,f)+5)return f;return -1;}
        public bool InDeepBase {get {foreach(var h in OwnedHomes)if(h.deepBaseVersion==1&&CurrentDeepFloor(h)>0)return true;return false;}}
        public bool UpgradeNineLayerBase(OwnedHome home=null)
        {
            var h=home??FindSecretHome();if(h==null||h.realm||h.region!=World.Region||h.secretRoomVersion!=1||saveBlocked||DeepInputCaptured||CompanionReturnBlocked){Notice="先建好自己的第一层密室，并结束当前游玩，再扩建九层。";return false;}
            if(h.deepBaseVersion==1){Notice="九层已经建好；地面木板旁或各层左后角按3叫升降椅。";return true;}
            var edits=new Dictionary<Vector3Int,Block>(World.Edits);var stored=(int[])h.storedItems.Clone();var changes=new Dictionary<Vector3Int,Block>();var recovered=new int[Blocks.Count];
            for(int f=2;f<=9;f++){int floor=Mathf.RoundToInt(DeepFloorY(h,f));for(int x=-1;x<=10;x++)for(int z=-1;z<=10;z++)for(int y=floor-1;y<=floor+5;y++){bool shell=x==-1||x==10||z==-1||z==10||y==floor-1||y==floor+5;var p=new Vector3Int(h.x+x,y,h.z+z);if(!VoxelWorld.InBounds(p)){Notice="这里不够深，不能建九层。";return false;}changes[p]=y==0?Block.Air:shell?Block.StoneBricks:Block.Air;}}
            // Restore the upper floor and remove the former broad stair trench.
            for(int x=2;x<=3;x++)for(int z=2;z<=8;z++){changes[new Vector3Int(h.x+x,h.y+1,h.z+z)]=Block.Planks;changes[new Vector3Int(h.x+x,h.y,h.z+z)]=Block.StoneBricks;for(int y=h.y-5;y<h.y;y++)changes[new Vector3Int(h.x+x,y,h.z+z)]=Block.Air;}
            foreach(var other in OwnedHomes)if(other!=h&&!other.realm&&other.region==h.region&&other.basement&&Mathf.Abs(other.x-h.x)<12&&Mathf.Abs(other.z-h.z)<12){Notice="地下扩建会碰到另一座房屋，请换空地。";return false;}
            foreach(var e in changes){if(e.Key.y<h.y-6&&(World.Region==1&&VoxelWorld.VillageRenewable(e.Key)||World.Region==6&&VoxelWorld.CastleRenewable(e.Key))){Notice="地下扩建会碰到公共房屋或楼层，不能覆盖。请另选自己的空地。";return false;}if(e.Key.y<h.y-6&&World.Edits.TryGetValue(e.Key,out var old)&&old!=Block.Air&&old!=e.Value){if(!CabinetItemAllowed((int)old)){Notice="下面已有需要保留的特殊物品，暂不施工。";return false;}recovered[(int)old]++;}}
            for(int i=0;i<recovered.Length;i++)if(recovered[i]>9999-h.storedItems[i]){Notice="柜子放不下施工收回的物品。";return false;}
            foreach(var e in changes){if(e.Key.y==0&&e.Value==Block.Air)World.OpenDepthHole(e.Key,false);else World.Edit(e.Key,e.Value,false);}for(int i=0;i<recovered.Length;i++)h.storedItems[i]+=recovered[i];h.deepBaseVersion=1;
            try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(Exception){h.deepBaseVersion=0;h.storedItems=stored;int region=World.Region;World.Generate(region);World.ApplyEdits(ToSavedEdits(edits));World.Edits.Clear();foreach(var e in edits)World.Edits[e.Key]=e.Value;World.RebuildAll();Notice="扩建保存失败，原地形和柜子已恢复。";return false;}
            World.RebuildAll();BuildSecretRooms();if(LifeWorkshopOpen)CloseLifeWorkshop();Notice="九层秘密基地建好了！地面木板已封好；左后角按3叫椅子，第二层按4开无人机屏。";return true;
        }
        static List<SavedBlock> ToSavedEdits(Dictionary<Vector3Int,Block> edits){var list=new List<SavedBlock>();foreach(var e in edits)list.Add(new SavedBlock{x=e.Key.x,y=e.Key.y,z=e.Key.z,id=(int)e.Value});return list;}
        void BuildDeepRooms(OwnedHome h)
        {
            if(h.deepBaseVersion!=1)return;
            for(int f=1;f<=9;f++)
            {
                var root=new GameObject("第"+f+"层 · "+DeepRoomNames[f-1]).transform;root.SetParent(secretRoomRoot,false);root.position=new Vector3(h.x,DeepFloorY(h,f),h.z);var a=root.gameObject.AddComponent<RoyalArt>();Color tint=Color.HSVToRGB((f*.085f+.83f)%1,.25f,.82f),cream=new Color(.96f,.87f,.7f),wood=new Color(.55f,.36f,.27f);
                if(f>1){a.Piece("实心地下地板",new Vector3(5,-.08f,5),new Vector3(10,.24f,10),tint,PrimitiveType.Cube,true);a.Piece("封闭顶板",new Vector3(5,4.92f,5),new Vector3(10,.2f,10),tint,PrimitiveType.Cube,true);foreach(float x in new[]{-.03f,10.03f})a.Piece("左/右墙",new Vector3(x,2.4f,5),new Vector3(.25f,5,10.3f),tint,PrimitiveType.Cube,true);foreach(float z in new[]{-.03f,10.03f})a.Piece("前/后墙",new Vector3(5,2.4f,z),new Vector3(10.3f,5,.25f),tint,PrimitiveType.Cube,true);a.Piece("层内软地毯",new Vector3(5,.065f,5),new Vector3(5.5f,.035f,5.5f),new Color(.82f,.56f,.72f));for(int k=0;k<4;k++){var light=new GameObject("柔光顶灯").AddComponent<Light>();light.transform.SetParent(root,false);light.transform.localPosition=new Vector3(2+k%2*6,3.7f,2+k/2*6);light.range=10;light.intensity=.8f;light.color=cream;a.Piece("顶灯",light.transform.localPosition+Vector3.up*.45f,new Vector3(.65f,.12f,.65f),cream);}}
                PlaceSign(root,"第"+f+"层 · "+DeepRoomNames[f-1],new Vector3(5,4.2f,9.7f));PlaceSign(root,"3 召唤升降椅",new Vector3(1.7f,1.8f,8.8f));a.Piece("螺旋座椅召唤垫",new Vector3(1.7f,.04f,8.2f),new Vector3(1.7f,.08f,1.7f),new Color(.56f,.74f,.86f),PrimitiveType.Cylinder);
                if(f==1)continue;
                BuildDeepNeighbourhood(root,h,f,a);
                if(f==2){var monitor=MakeDeepObject(root,h,f,"drone",new Vector3(5,1.6f,1.4f),"无人机电视 · 按4打开",new Vector3(3.6f,2.1f,.3f),new Color(.1f,.2f,.25f));EnsureVillageDrone(h);var screen=a.Piece("第二层无人机实时画面",new Vector3(5,2.65f,1.57f),new Vector3(3.4f,1.91f,1),Color.white,PrimitiveType.Quad);screen.localRotation=Quaternion.Euler(0,180,0);monitor.ScreenMaterial=new Material(Resources.Load<Shader>("CCTVScreen"));monitor.ScreenMaterial.mainTexture=droneTexture;screen.GetComponent<Renderer>().sharedMaterial=monitor.ScreenMaterial;RenderDrone(true);a.Piece("无人机操作桌",new Vector3(5,.75f,2.4f),new Vector3(3,.2f,1.4f),wood,PrimitiveType.Cube,true);a.Piece("备用无人机展台",new Vector3(8,1,5),new Vector3(1.8f,.2f,1.8f),cream);MakeDroneModel(root,new Vector3(8,1.4f,5),false);MakeDeepObject(root,h,f,"photo",new Vector3(8,0,7.5f),"观察照片册 · G翻页",new Vector3(1.8f,.8f,1),wood);}
                if(f==3){MakeDeepObject(root,h,f,"wash",new Vector3(7,0,2),"洗手洗脸 · G",new Vector3(2,.9f,1.3f),cream);a.Piece("水池盆",new Vector3(7,1.03f,2),new Vector3(1.5f,.18f,.8f),new Color(.52f,.83f,.91f));a.Piece("水龙头",new Vector3(7,1.4f,1.7f),new Vector3(.12f,.7f,.15f),cream);MakeDeepObject(root,h,f,"bath",new Vector3(6,0,6),"泡泡浴 · G",new Vector3(3,.8f,2),new Color(.9f,.72f,.78f));for(int i=0;i<15;i++)a.Piece("泡泡",new Vector3(5+(i%5)*.4f,.85f,5.5f+i/5*.4f),Vector3.one*.25f,Color.white,PrimitiveType.Sphere);}
                if(f==4){for(int i=0;i<8;i++){a.Piece("花盆",new Vector3(3+i%4*1.4f,.3f,2+i/4*4),new Vector3(.85f,.6f,.85f),wood);a.Piece("花叶",new Vector3(3+i%4*1.4f,1,2+i/4*4),new Vector3(.65f,.75f,.65f),new Color(.3f,.65f,.42f),PrimitiveType.Sphere);a.Piece("花朵",new Vector3(3+i%4*1.4f,1.45f,2+i/4*4),Vector3.one*.4f,Color.HSVToRGB(i*.13f,.5f,.95f),PrimitiveType.Sphere);}MakeDeepObject(root,h,f,"garden",new Vector3(7.8f,0,8),"给花浇水 · G · 收获奖品",new Vector3(1.5f,.8f,1),cream);}
                if(f==5){a.Piece("厨房长柜",new Vector3(7,1,1.3f),new Vector3(4.5f,2,.8f),wood,PrimitiveType.Cube,true);for(int i=0;i<4;i++)a.Piece("柜门",new Vector3(5.4f+i,1,1.76f),new Vector3(.9f,1.7f,.05f),tint);MakeDeepObject(root,h,f,"cook",new Vector3(7,0,4),"烤面包与点心 · G",new Vector3(2,.9f,1.3f),cream);MakeDeepObject(root,h,f,"eat",new Vector3(5,0,7),"吃点心喝果汁 · G",new Vector3(2.8f,.8f,1.7f),wood);a.Piece("草莓果汁",new Vector3(5.8f,1.25f,7),new Vector3(.3f,.55f,.3f),new Color(1,.54f,.7f),PrimitiveType.Cylinder);a.Piece("面包",new Vector3(4.6f,1.05f,7),new Vector3(.7f,.3f,.5f),cream,PrimitiveType.Sphere);}
                if(f==6){for(int k=0;k<3;k++){a.Piece("大书架",new Vector3(7+k,2,1),new Vector3(.9f,4,.6f),wood,PrimitiveType.Cube,true);for(int i=0;i<12;i++)a.Piece("不同彩书",new Vector3(6.7f+k+i%4*.17f,.7f+i/4*1.1f,1.4f),new Vector3(.12f,.7f,.15f),Color.HSVToRGB((i*.1f+k*.2f)%1,.4f,.8f));}MakeDeepObject(root,h,f,"read",new Vector3(5,0,5),"故事阅读桌 · G翻下一页",new Vector3(2.4f,.85f,1.6f),wood);}
                if(f==7){MakeDeepObject(root,h,f,"game",new Vector3(5,0,4),"彩灯反应游戏 · G",new Vector3(3,.8f,2),new Color(.26f,.32f,.55f));for(int i=0;i<9;i++)a.Piece("彩灯",new Vector3(4+i%3,1.05f,3.3f+i/3*.7f),Vector3.one*.3f,Color.HSVToRGB(i*.11f,.6f,1),PrimitiveType.Sphere);MakeDeepObject(root,h,f,"music",new Vector3(8,0,7),"敲音符 · G",new Vector3(1.2f,1.4f,1),tint);}
                if(f==8){MakeDeepObject(root,h,f,"prize",new Vector3(5,0,4),"领取探索奖品 · G",new Vector3(2,1.2f,1.5f),new Color(.92f,.64f,.35f));for(int i=0;i<8;i++){a.Piece("奖品台",new Vector3(3+i%4*1.5f,.5f,7+i/4),new Vector3(.6f,1,.6f),wood);a.Piece("闪亮奖品",new Vector3(3+i%4*1.5f,1.3f,7+i/4),new Vector3(.4f,.5f,.4f),Color.HSVToRGB(i*.12f,.45f,1),PrimitiveType.Sphere);}MakeDeepObject(root,h,f,"collection",new Vector3(8,0,2),"查看收藏 · G",new Vector3(1.5f,1,.6f),cream);}
                if(f==9){MakeDeepObject(root,h,f,"sleep",new Vector3(6,0,5),"星光小床 · G睡20秒",new Vector3(3.5f,.55f,2),new Color(.84f,.57f,.73f));a.Piece("床头板",new Vector3(7.7f,.8f,5),new Vector3(.15f,1.6f,2.1f),wood);a.Piece("厚软床垫",new Vector3(6,.61f,5),new Vector3(3.3f,.14f,1.9f),cream);a.Piece("星光小被子",new Vector3(5.4f,.71f,5),new Vector3(2,.11f,1.85f),new Color(.73f,.66f,.91f));a.Piece("软枕头",new Vector3(7.1f,.79f,5),new Vector3(1,.25f,1.5f),cream,PrimitiveType.Sphere);for(int i=0;i<30;i++)a.Piece("天花板星星",new Vector3(.5f+i%6*1.7f,4.78f,.7f+i/6*1.7f),Vector3.one*.08f,cream,PrimitiveType.Sphere);MakeDeepObject(root,h,f,"stars",new Vector3(8,0,8),"星光许愿 · G",new Vector3(1,1.1f,1),new Color(.65f,.75f,1));}
            }
        }
        DeepRoomObject MakeDeepObject(Transform room,OwnedHome h,int floor,string activity,Vector3 pos,string name,Vector3 size,Color color){var o=new GameObject(name).transform;o.SetParent(room,false);o.localPosition=pos;var a=o.gameObject.AddComponent<RoyalArt>();a.Piece(name,new Vector3(0,size.y/2,0),size,color,PrimitiveType.Cube,true);var f=o.gameObject.AddComponent<DeepRoomObject>();f.Home=h;f.Floor=floor;f.Activity=activity;PlaceSign(room,name,new Vector3(pos.x,Mathf.Min(4.25f,pos.y+size.y+.7f),pos.z));return f;}
        public bool TrySecretChair3()
        {
            if(chairMenu){CloseSecretChair();return true;}if(chairTravelling){Notice="椅子正在螺旋升降，抵达后按3下椅。";return true;}if(chairArrived){CloseSecretChair();return true;}if(Paused||DeepActivityActive||DronePanelOpen||CompanionReturnBlocked||PhoneOpen||StickerOpen)return false;
            var h=FindSecretHome();if(h==null||h.secretRoomVersion!=1)return false;int f=CurrentDeepFloor(h);if(f<0||Vector3.Distance(Player.transform.position,DeepCorner(h,f))>3.4f)return false;
            if(h.deepBaseVersion!=1&&!UpgradeNineLayerBase(h))return true;
            if(f==0&&!chairAuthorized.Contains(h)){var hatch=RoomFixture(h,SecretFixtureKind.Hatch);if(hatch!=null)OpenSecretFixture(hatch);Notice="先输入游戏门密码，验证后再按3叫椅子。";return true;}
            chairHome=h;chairEntry=Player.transform.position;chairEntryFlying=Player.Flying;secretChair=new GameObject("可消失的螺旋升降椅").transform;secretChair.SetParent(secretRoomRoot,false);secretChair.position=DeepCorner(h,f);var art=secretChair.gameObject.AddComponent<RoyalArt>();art.Piece("粉蓝软座",Vector3.up*.65f,new Vector3(1.1f,.3f,1.1f),new Color(.5f,.75f,.92f));art.Piece("椅背",new Vector3(0,1.2f,-.45f),new Vector3(1.1f,1.2f,.2f),new Color(.8f,.59f,.79f));foreach(float x in new[]{-.6f,.6f})art.Piece("扶手",new Vector3(x,1,.05f),new Vector3(.14f,.16f,.9f),new Color(.94f,.84f,.65f));chairMenu=true;SetPaused(false);return true;
        }
        public bool ChooseSecretChairFloor(int destination)
        {
            if(!chairMenu||chairHome==null||destination<0||destination>9||CurrentDeepFloor(chairHome)==destination)return false;chairDestination=destination;chairFrom=secretChair.position;chairTo=DeepCorner(chairHome,destination);chairAge=0;chairDuration=Mathf.Max(2.5f,Mathf.Abs(chairTo.y-chairFrom.y)/5);chairMenu=false;chairTravelling=true;var hatch=RoomFixture(chairHome,SecretFixtureKind.Hatch);if(hatch!=null)hatch.SetOpen(false);Player.SetFlying(false);Player.BeginCoasterSeat();Player.HomeRest(true);Player.MoveCoasterSeat(chairFrom+Vector3.up*.8f);Notice="已经坐稳，木板门自动关好；螺旋升降中，到站按3下椅。";return true;
        }
        public void CloseSecretChair(){if(secretChair==null){chairMenu=chairTravelling=chairArrived=false;return;}Vector3 exit=chairArrived?DeepLanding(chairHome,chairDestination):chairEntry;if(chairTravelling)exit=chairEntry;Player.MoveCoasterSeat(exit);Player.ReleaseCoasterSeat();Player.HomeRest(false);Player.SetFlying(!chairArrived&&chairEntryFlying);Destroy(secretChair.gameObject);secretChair=null;chairHome=null;chairMenu=chairTravelling=chairArrived=false;Notice="已下椅，升降椅消失。";}
        public void TickDeepBase(float dt)
        {
            if(Paused)return;if(chairTravelling){chairAge+=dt;float t=Mathf.Clamp01(chairAge/chairDuration),s=Mathf.SmoothStep(0,1,t),angle=t*Mathf.PI*2*2;Vector3 p=Vector3.Lerp(chairFrom,chairTo,s)+new Vector3(Mathf.Sin(angle),0,1-Mathf.Cos(angle))*(.25f*Mathf.Sin(t*Mathf.PI));secretChair.position=p;secretChair.rotation=Quaternion.Euler(0,t*360*2,0);Player.MoveCoasterSeat(p+Vector3.up*.8f);if(t>=1){chairTravelling=false;chairArrived=true;chairEntry=DeepLanding(chairHome,chairDestination);Notice=(chairDestination==0?"到地面小屋了":"到第"+chairDestination+"层了")+"。按3站起来，椅子会消失。";}}
            if(DeepActivityActive){deepActivityAge+=dt;if(deepActivity=="slide"&&deepAnimated!=null){float t=Mathf.Clamp01(deepActivityAge/4);Player.MoveCoasterSeat(deepAnimated.position+new Vector3(0,2*(1-t)+.15f,-1.2f+t*2.4f));}if(deepActivity=="bounce"&&deepAnimated!=null)Player.MoveCoasterSeat(deepAnimated.position+Vector3.up*(.35f+Mathf.Abs(Mathf.Sin(deepActivityAge*2.6f))*1.5f));if(deepActivityAge>=DeepActivityDuration)FinishDeepActivity();}
            TickDrone(dt);
        }
        float DeepActivityDuration=>deepActivity=="slide"?4:deepActivity=="sleep"?20:deepActivity=="game"?12:10;
        public bool TryDeepActivityG(){if(DeepActivityActive){CancelDeepActivity();return true;}if(Paused||DeepInputCaptured||CompanionReturnBlocked)return false;if(!RayFromView(out var hit,5))return false;var item=hit.collider.GetComponentInParent<DeepRoomObject>();return item!=null&&StartDeepActivity(item);}
        int readPage;string deepReadText="";
        public bool StartDeepActivity(DeepRoomObject item)
        {
            if(item==null||item.Home.deepBaseVersion!=1||DeepInputCaptured||Paused)return false;if(item.Activity=="drone")return OpenDronePanel(item.Home);
            if(item.Activity=="read"||item.Activity=="photo"||item.Activity=="collection"||item.Activity=="computer"){string[] pages={"小树苗在地下花园发芽了。柔光灯给了它温暖的春天。","无人机看见北岸的浪头，提醒大家关好门。","村民把面包送给朋友，大家一起照顾小花。","九层里有安静的床、闪亮的奖品，也有快乐的游戏。"};deepReadText=pages[readPage++%pages.Length];Notice=(item.Activity=="computer"?"电脑故事小游戏：":"")+deepReadText;return true;}
            if(item.Activity=="prize"){return AwardDeepPrize(item.Home,8,68);}
            if(item.Activity=="stars"){Notice="你许下了一个温柔的愿望，天花板星光亮起。";return AwardDeepPrize(item.Home,9,38);}
            if(item.Activity=="music"){audioSource.PlayOneShot(placeSound);Notice="叮！你敲响了清亮的音符。";return true;}
            activityHome=item.Home;deepActivityFloor=item.Floor;deepActivity=item.Activity;deepAnimated=item.transform;deepActivityAge=0;deepGameScore=0;deepGameTarget=0;activityEntry=Player.transform.position;activityFlying=Player.Flying;Player.SetFlying(false);Player.BeginCoasterSeat();Player.HomeRest(true,item.Activity=="sleep");if(item.Activity=="sleep")Player.MoveCoasterSeat(item.transform.position+new Vector3(0,.62f,0));Notice="正在"+DeepActivityLabel+"；G可提前起来。";return true;
        }
        string DeepActivityLabel=>deepActivity=="slide"?"坐地下小滑梯":deepActivity=="bounce"?"玩弹跳床":deepActivity=="sleep"?"星光床上休息":deepActivity=="wash"?"洗手洗脸":deepActivity=="bath"?"泡泡洗澡":deepActivity=="garden"?"浇花":deepActivity=="cook"?"烘烤面包":deepActivity=="eat"?"吃点心喝果汁":"玩彩灯反应游戏";
        bool AwardDeepPrize(OwnedHome h,int floor,int coins){int bit=1<<(floor-1);if((h.deepPrizeMask&bit)!=0){Notice="这一层的探索奖品已经领过，仍可继续玩。";return true;}if(Inventory[(int)Block.Coin]>9999-coins){Notice="钱包满了，先把钱存进保险箱。";return false;}h.deepPrizeMask|=bit;Inventory[(int)Block.Coin]+=coins;try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(Exception){h.deepPrizeMask&=~bit;Inventory[(int)Block.Coin]-=coins;Notice="奖品没能保存，暂未领取。";return false;}Notice="获得第"+floor+"层探索奖品："+coins+"元。";return true;}
        int deepGameScore,deepGameTarget;public int DeepGameScore=>deepGameScore;
        public bool TapDeepLight(int index){if(deepActivity!="game"||index!=deepGameTarget)return false;deepGameScore++;deepGameTarget=(deepGameTarget+2)%3;audioSource.PlayOneShot(placeSound);return true;}
        void FinishDeepActivity(){var h=activityHome;int f=deepActivityFloor;string kind=deepActivity;int score=deepGameScore;CancelDeepActivity();if(kind=="sleep"||kind=="eat")Health=Mathf.Min(10,Health+1);if(kind=="cook"&&Inventory[(int)Block.Bread]<9999)Inventory[(int)Block.Bread]++;if(kind=="game"&&score<3){Notice="彩灯游戏结束，点中3次才能领奖；G可以重玩。";return;}AwardDeepPrize(h,f,20+f*3);Save();}
        public void CancelDeepActivity(){if(!DeepActivityActive)return;Player.MoveCoasterSeat(activityEntry);Player.ReleaseCoasterSeat();Player.HomeRest(false);Player.SetFlying(activityFlying);deepActivity="";deepAnimated=null;activityHome=null;Notice="已回到房间，可以继续探索。";}
        void ResetDeepBase(){DisposeDrone();CloseSecretChair();CancelDeepActivity();chairAuthorized.Clear();}
        void DrawDeepBase(float w,float h)
        {
            if(chairMenu){float x=w/2-300,y=h/2-225;Panel(new Rect(x,y,600,450),Paper);Label(new Rect(x+20,y+15,560,35),"螺旋升降椅 · 选层后自动坐上",24,Ink);for(int f=0;f<=9;f++){int col=f%2,row=f/2;bool enabled=GUI.enabled;GUI.enabled=CurrentDeepFloor(chairHome)!=f;if(GUI.Button(new Rect(x+20+col*286,y+68+row*63,274,54),f==0?"地面小屋":f+" · "+DeepRoomNames[f-1]))ChooseSecretChairFloor(f);GUI.enabled=enabled;}if(GUI.Button(new Rect(x+20,y+395,560,35),"取消 / 3"))CloseSecretChair();return;}
            if(SecretChairActive)Label(new Rect(w/2-330,h-100,660,50),chairArrived?"到站了 · 按3站起来，座椅消失":"螺旋升降中 · 穿过封闭木板 · 请坐稳",22,Color.white,TextAnchor.MiddleCenter);
            if(DeepActivityActive){Panel(new Rect(w/2-285,h-155,570,100),new Color(.11f,.15f,.23f,.93f));Label(new Rect(w/2-260,h-146,520,35),DeepActivityLabel+" · "+Mathf.CeilToInt(DeepActivityDuration-deepActivityAge)+"秒 · G起来",21,Color.white,TextAnchor.MiddleCenter);Panel(new Rect(w/2-240,h-90,480*Mathf.Clamp01(deepActivityAge/DeepActivityDuration),8),new Color(.95f,.69f,.8f));if(deepActivity=="game"){Label(new Rect(w/2-240,h/2-120,480,35),"点亮灯的按钮 · 已点中"+deepGameScore+"次",23,Color.white,TextAnchor.MiddleCenter);for(int i=0;i<3;i++)if(GUI.Button(new Rect(w/2-240+i*165,h/2-60,150,90),i==deepGameTarget?"★ 亮灯":"暗灯"))TapDeepLight(i);}}
            if(!DeepInputCaptured&&!Paused){var home=FindSecretHome();int floor=CurrentDeepFloor(home);if(home!=null&&home.deepBaseVersion==1&&floor>=0)Label(new Rect(w/2-340,h-220,680,40),(floor==0?"地面小屋":floor+"层 · "+DeepRoomNames[floor-1])+" · 左后角3升降椅"+(floor==2?" · 4无人机电视":" · 看着设施G游玩"),17,Color.white,TextAnchor.MiddleCenter);}
        }
    }
}
