using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace Blocklands
{
    public enum SecretFixtureKind { Hatch, Safe, Cabinet, Swing, Monitor }
    public class SecretRoomFixture:MonoBehaviour
    {
        public OwnedHome Home;public SecretFixtureKind Kind;public Transform MovingPart;
        public bool Open {get;private set;}public Mesh OwnedMesh;public Material OwnedMaterial;
        public string Caption=>Kind==SecretFixtureKind.Monitor?"实时监控 · H查看浪和入口":Kind==SecretFixtureKind.Swing?"密室秋千 · G坐上 / 下来":Kind==SecretFixtureKind.Hatch?"木板伪装门 · 轻按H开关":"密室"+(Kind==SecretFixtureKind.Safe?"保险箱":"储物柜")+" · 轻按H";
        public void SetOpen(bool value){Open=value;if(MovingPart!=null)MovingPart.localRotation=Kind==SecretFixtureKind.Hatch?Quaternion.Euler(0,0,value?100:0):Quaternion.Euler(0,value?-105:0,0);Physics.SyncTransforms();}
        void OnDestroy(){if(OwnedMesh!=null)Destroy(OwnedMesh);if(OwnedMaterial!=null)Destroy(OwnedMaterial);}
    }
    public partial class Sandbox
    {
        Transform secretRoomRoot,secretSwingPivot;SecretRoomFixture secretFocus,secretSwing;
        string secretPin="",secretPinAgain="",secretAmount="1",secretStatus="";bool secretAuthenticated;
        Vector2 secretScroll;Vector3 secretSwingEntry;bool secretSwingFlying;float secretSwingAge,visitorClock;
        public bool SecretRoomMenuOpen=>secretFocus!=null;
        public bool SecretSwingActive=>secretSwing!=null;
        public Vector3 SecretSwingEntry=>secretSwingEntry;
        public bool SecretSwingEntryFlying=>secretSwingFlying;
        public string VisitorStatus {get;private set;}="";
        SecretRoomVisitor secretVisitor;
        public static bool ValidRoomPin(string pin){if(pin==null||pin.Length<4||pin.Length>12)return false;foreach(char c in pin)if(c<'0'||c>'9')return false;return true;}
        static string RoomHash(string salt,string pin){using(var sha=SHA256.Create())return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(salt+"|"+pin)));}
        public static bool MatchesRoomPin(string salt,string hash,string pin){if(!ValidRoomPin(pin)||string.IsNullOrEmpty(salt)||string.IsNullOrEmpty(hash))return false;var actual=RoomHash(salt,pin);if(actual.Length!=hash.Length)return false;int diff=0;for(int i=0;i<actual.Length;i++)diff|=actual[i]^hash[i];return diff==0;}
        public static void ValidateSecretStorage(OwnedHome h)
        {
            if(h.transparentHouseVersion<0||h.transparentHouseVersion>1||h.transparentHouseVersion==1&&(h.realm||!h.legacy||h.region!=1||h.x!=28||h.y!=14||h.z!=50))throw new System.IO.InvalidDataException("透明旧屋位置不合法");
            if(h.secretRoomVersion<0||h.secretRoomVersion>1||h.safeMoney<0||h.safeMoney>1000000||h.deepBaseVersion<0||h.deepBaseVersion>1||h.deepPrizeMask<0||h.deepPrizeMask>511||h.deepBaseVersion==1&&h.secretRoomVersion!=1)throw new System.IO.InvalidDataException("密室数据不合法");
            if(h.partyHallVersion<0||h.partyHallVersion>1||h.partyHallVersion==1&&(h.realm||h.partyX<0||h.partyZ<0||h.partyW<16||h.partyW>18||h.partyD<16||h.partyD>18||h.partyX+h.partyW>VoxelWorld.Width||h.partyZ+h.partyD>VoxelWorld.Depth||h.partyDir<0||h.partyDir>3))throw new System.IO.InvalidDataException("派对厅位置不合法");
            if(h.partyRouteVersion<0||h.partyRouteVersion>1)throw new System.IO.InvalidDataException("迎宾通道数据不合法");
            if(h.doorGuests==null)h.doorGuests=new List<string>();if(h.doorGuests.Count>100)throw new System.IO.InvalidDataException("门权限数据过多");var guests=new HashSet<string>();foreach(var key in h.doorGuests)if(string.IsNullOrEmpty(key)||key.Length>100||!guests.Add(key)||!(key.StartsWith("adult:")||key.StartsWith("family:")||key.StartsWith("community:")))throw new System.IO.InvalidDataException("门权限数据不合法");
            foreach(var pair in new[]{new[]{h.doorSalt,h.doorHash},new[]{h.safeSalt,h.safeHash}}){bool empty=string.IsNullOrEmpty(pair[0])&&string.IsNullOrEmpty(pair[1]);if(!empty&&(pair[0]==null||pair[0].Length!=24||pair[1]==null||pair[1].Length!=44))throw new System.IO.InvalidDataException("游戏门锁数据不合法");}
            if(h.storedItems==null)h.storedItems=new int[Blocks.Count];else if(h.storedItems.Length==94)System.Array.Resize(ref h.storedItems,Blocks.Count);if(h.storedItems.Length!=Blocks.Count)throw new System.IO.InvalidDataException("密室柜子数据不合法");
            for(int i=0;i<h.storedItems.Length;i++)if(h.storedItems[i]<0||h.storedItems[i]>9999||h.storedItems[i]>0&&!CabinetItemAllowed(i))throw new System.IO.InvalidDataException("密室柜子物品不合法");
        }
        static bool CabinetItemAllowed(int id)=>id>0&&(id<91||id==94)&&id!=(int)Block.Coin;
        public SecretRoomFixture RoomFixture(OwnedHome home,SecretFixtureKind kind){if(secretRoomRoot==null)return null;foreach(var f in secretRoomRoot.GetComponentsInChildren<SecretRoomFixture>())if(f.Home==home&&f.Kind==kind)return f;return null;}
        public void BuildSecretRooms()
        {
            ResetDeepBase();StopSecretSwing();DisposeSecretMonitor();if(secretRoomRoot!=null){secretRoomRoot.gameObject.SetActive(false);Destroy(secretRoomRoot.gameObject);}secretRoomRoot=new GameObject("地下密室设施").transform;secretRoomRoot.SetParent(villagePeople.transform,false);roomBeacon=null;
            foreach(var h in OwnedHomes)if(!h.realm&&h.region==World.Region&&h.secretRoomVersion==1)
            {
                ValidateSecretStorage(h);var room=new GameObject("宽敞地下密室").transform;room.SetParent(secretRoomRoot,false);room.position=new Vector3(h.x,h.y-5,h.z);var a=room.gameObject.AddComponent<RoyalArt>();
                var pink=new Color(.9f,.67f,.75f);var oak=new Color(.6f,.39f,.22f);var cream=new Color(.98f,.87f,.65f);
                if(h.deepBaseVersion==0)for(int step=0;step<7;step++)a.Piece("半格密室阶梯",new Vector3(3,6.25f-step,2.25f+step),new Vector3(2,.5f,.5f),Blocks.ColorOf(Block.StoneBricks),PrimitiveType.Cube,true);
                a.Piece("中央柔软地毯",new Vector3(6,.02f,4.6f),new Vector3(5,.035f,4.4f),pink);
                a.Piece("阅读长桌",new Vector3(8.8f,.75f,2),new Vector3(1.6f,.15f,3),oak,PrimitiveType.Cube,true);
                foreach(float z in new[]{1f,3f})a.Piece("桌脚",new Vector3(8.8f,.35f,z),new Vector3(.2f,.7f,.2f),oak);
                for(int i=0;i<7;i++)a.Piece("彩色藏书",new Vector3(8.8f,.97f,1.1f+i*.28f),new Vector3(.45f,.3f,.18f),Color.HSVToRGB(i*.14f,.35f,.9f));
                a.Piece("粉色小沙发",new Vector3(5,.45f,1.2f),new Vector3(2.5f,.65f,.9f),pink,PrimitiveType.Cube,true);a.Piece("沙发靠背",new Vector3(5,.97f,1.5f),new Vector3(2.5f,.65f,.2f),pink,PrimitiveType.Cube,true);
                foreach(float x in new[]{.3f,9.6f})for(int z=2;z<=8;z+=3){a.Piece("暖色壁灯",new Vector3(x,3.1f,z),new Vector3(.15f,.4f,.4f),cream);var light=new GameObject("密室暖灯").AddComponent<Light>();light.transform.SetParent(room,false);light.transform.localPosition=new Vector3(x<1?1:9,3.1f,z);light.type=LightType.Point;light.range=8;light.intensity=.85f;light.color=cream;}
                var safeRoot=new GameObject("有密码的保险箱").transform;safeRoot.SetParent(room,false);safeRoot.localPosition=new Vector3(8.6f,0,5.3f);var sa=safeRoot.gameObject.AddComponent<RoyalArt>();
                var sf=safeRoot.gameObject.AddComponent<SecretRoomFixture>();sf.Home=h;sf.Kind=SecretFixtureKind.Safe;
                sa.Piece("钢制箱体",new Vector3(0,.9f,.15f),new Vector3(1.3f,1.8f,1.15f),new Color(.3f,.4f,.46f),PrimitiveType.Cube,true);
                sf.MovingPart=new GameObject("保险箱门铰链").transform;sf.MovingPart.SetParent(safeRoot,false);sf.MovingPart.localPosition=new Vector3(-.67f,0,-.46f);var face=sf.MovingPart.gameObject.AddComponent<RoyalArt>();face.Piece("厚钢门",new Vector3(.67f,.9f,0),new Vector3(1.3f,1.75f,.12f),new Color(.56f,.67f,.69f),PrimitiveType.Cube,true);face.Piece("密码键盘",new Vector3(.8f,1.13f,-.09f),new Vector3(.28f,.4f,.05f),new Color(.1f,.2f,.24f));for(int n=0;n<9;n++)face.Piece("数字按键",new Vector3(.72f+(n%3)*.07f,1.24f-(n/3)*.09f,-.125f),new Vector3(.035f,.04f,.02f),cream);face.Piece("转动把手",new Vector3(.28f,.85f,-.14f),new Vector3(.35f,.35f,.08f),cream,PrimitiveType.Cylinder);
                var cabinet=new GameObject("真正能储物的柜子").transform;cabinet.SetParent(room,false);cabinet.localPosition=new Vector3(8.8f,0,8);var ca=cabinet.gameObject.AddComponent<RoyalArt>();var cf=cabinet.gameObject.AddComponent<SecretRoomFixture>();cf.Home=h;cf.Kind=SecretFixtureKind.Cabinet;ca.Piece("储物柜",new Vector3(0,1,.1f),new Vector3(1.3f,2,1),oak,PrimitiveType.Cube,true);for(int i=0;i<3;i++){ca.Piece("储物抽屉",new Vector3(0,.36f+i*.62f,-.43f),new Vector3(1.2f,.55f,.06f),pink);ca.Piece("抽屉把手",new Vector3(0,.36f+i*.62f,-.51f),new Vector3(.35f,.07f,.08f),cream);}
                var swingRoot=new GameObject("地下休闲秋千").transform;swingRoot.SetParent(room,false);swingRoot.localPosition=new Vector3(5.8f,0,8);var sw=swingRoot.gameObject.AddComponent<SecretRoomFixture>();sw.Home=h;sw.Kind=SecretFixtureKind.Swing;var wa=swingRoot.gameObject.AddComponent<RoyalArt>();foreach(float x in new[]{-1.15f,1.15f})wa.Piece("秋千支柱",new Vector3(x,1.9f,0),new Vector3(.18f,3.8f,.2f),oak,PrimitiveType.Cube,true);wa.Piece("秋千横梁",new Vector3(0,3.8f,0),new Vector3(2.8f,.2f,.24f),oak);
                sw.MovingPart=new GameObject("秋千摆轴").transform;sw.MovingPart.SetParent(swingRoot,false);sw.MovingPart.localPosition=Vector3.up*3.7f;var ropes=sw.MovingPart.gameObject.AddComponent<RoyalArt>();foreach(float x in new[]{-.65f,.65f})ropes.Piece("真实秋千绳",new Vector3(x,-1.5f,0),new Vector3(.045f,3,.045f),cream);ropes.Piece("秋千座板",new Vector3(0,-3,0),new Vector3(1.6f,.16f,.7f),pink,PrimitiveType.Cube,true);
                var hatch=new GameObject("与地板同纹理的木板门").transform;hatch.SetParent(secretRoomRoot,false);hatch.position=new Vector3(h.x+2,h.y+1.94f,h.z+4);var hf=hatch.gameObject.AddComponent<SecretRoomFixture>();hf.Home=h;hf.Kind=SecretFixtureKind.Hatch;hf.MovingPart=hatch;
                for(int tile=0;tile<4;tile++){var wood=GameObject.CreatePrimitive(PrimitiveType.Cube);wood.name="木板伪装入口";wood.transform.SetParent(hatch,false);wood.transform.localPosition=new Vector3(.5f+tile%2,0,-.5f-tile/2);wood.transform.localScale=new Vector3(1,.12f,1);if(hf.OwnedMesh==null){hf.OwnedMesh=Instantiate(wood.GetComponent<MeshFilter>().sharedMesh);Color color=Blocks.ColorOf(Block.Planks);color.a=(int)Block.Planks/128f;var colors=new Color[hf.OwnedMesh.vertexCount];for(int i=0;i<colors.Length;i++)colors[i]=color;hf.OwnedMesh.colors=colors;hf.OwnedMaterial=new Material(Resources.Load<Shader>("BlockSurface"));}wood.GetComponent<MeshFilter>().sharedMesh=hf.OwnedMesh;wood.GetComponent<Renderer>().sharedMaterial=hf.OwnedMaterial;}
                PlaceSign(room,"H保险箱与柜子 · G秋千",new Vector3(6,3.2f,9.6f));
                BuildSecretMonitor(room,h);
                BuildDeepRooms(h);
            }
        }
        SecretRoomFixture AimSecret(){if(RayFromView(out var hit,6))return hit.collider.GetComponentInParent<SecretRoomFixture>();return null;}
        public bool TrySecretRoomH(){if(Paused||!ShortH)return false;var f=AimSecret();if(f==null||f.Kind==SecretFixtureKind.Swing)return false;return OpenSecretFixture(f);}
        public bool OpenSecretFixture(SecretRoomFixture f)
        {
            if(f==null||f.Home.secretRoomVersion!=1||!OwnedHomes.Contains(f.Home)||f.Home.region!=World.Region)return false;
            if(f.Kind==SecretFixtureKind.Monitor)return OpenSecretMonitor(f.Home);
            if(f.Kind==SecretFixtureKind.Hatch&&f.Open){var closed=new Bounds(new Vector3(f.Home.x+3,f.Home.y+1.94f,f.Home.z+3),new Vector3(2,.12f,2));if(closed.Intersects(Player.Body.bounds)){Notice="先离开木板门的位置，再关门，避免夹住自己。";return true;}f.SetOpen(false);Notice="木板门已关闭并上锁。";return true;}
            secretFocus=f;secretAuthenticated=f.Kind==SecretFixtureKind.Cabinet;secretPin=secretPinAgain="";secretAmount="1";secretStatus="";secretScroll=Vector2.zero;SetPaused(true);return true;
        }
        public bool UnlockSecret(string pin,string confirmation="")
        {
            if(secretFocus==null||secretFocus.Kind==SecretFixtureKind.Cabinet)return false;var h=secretFocus.Home;bool door=secretFocus.Kind==SecretFixtureKind.Hatch;string salt=door?h.doorSalt:h.safeSalt,hash=door?h.doorHash:h.safeHash;
            if(string.IsNullOrEmpty(hash))
            {
                if(!ValidRoomPin(pin)||pin!=confirmation){secretStatus="请输入两次相同的4–12位数字密码。";return false;}
                byte[] bytes=new byte[16];using(var random=RandomNumberGenerator.Create())random.GetBytes(bytes);salt=Convert.ToBase64String(bytes);hash=RoomHash(salt,pin);if(door){h.doorSalt=salt;h.doorHash=hash;}else{h.safeSalt=salt;h.safeHash=hash;}
                if(!SaveSecretTransaction()){if(door){h.doorSalt=h.doorHash=null;}else{h.safeSalt=h.safeHash=null;}return false;}
            }
            else if(!MatchesRoomPin(salt,hash,pin)){secretStatus="密码不对，锁没有打开。";return false;}
            secretPin=secretPinAgain="";secretAuthenticated=true;secretFocus.SetOpen(true);if(door){chairAuthorized.Add(h);CloseSecretMenu(false);Notice=h.deepBaseVersion==1?"游戏门密码正确。靠近木板按3召唤升降椅，木板地面不会留下洞。":"木板门已打开。沿楼梯下去，轻按H可关门。";}else secretStatus="保险箱已打开；关闭面板后自动上锁。";return true;
        }
        bool SaveSecretTransaction(){if(saveBlocked){secretStatus="存档读取失败，暂不能存取；请先检查原存档，避免覆盖进度。";return false;}try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);return true;}catch(Exception){secretStatus="保存失败，交易没有完成。请检查存档目录。";return false;}}
        public bool TransferSafeMoney(int amount,bool deposit)
        {
            if(secretFocus==null||!secretAuthenticated||secretFocus.Kind!=SecretFixtureKind.Safe||amount<=0)return false;var h=secretFocus.Home;int coins=Inventory[(int)Block.Coin],saved=h.safeMoney;
            if(deposit&&(coins<amount||amount>1000000-saved)||!deposit&&(saved<amount||amount>9999-coins)){secretStatus="金额不足或空间不足，没有移动钱。";return false;}
            Inventory[(int)Block.Coin]+=deposit?-amount:amount;h.safeMoney+=deposit?amount:-amount;if(!SaveSecretTransaction()){Inventory[(int)Block.Coin]=coins;h.safeMoney=saved;return false;}secretStatus=(deposit?"存入":"取出")+amount+"元。";return true;
        }
        public bool TransferCabinetItem(int id,int amount,bool deposit)
        {
            if(secretFocus==null||secretFocus.Kind!=SecretFixtureKind.Cabinet||!secretAuthenticated||!CabinetItemAllowed(id)||amount<=0)return false;var h=secretFocus.Home;int carried=Inventory[id],stored=h.storedItems[id];
            if(deposit&&(carried<amount||amount>9999-stored)||!deposit&&(stored<amount||amount>9999-carried)){secretStatus="数量或空间不足，没有移动物品。";return false;}
            Inventory[id]+=deposit?-amount:amount;h.storedItems[id]+=deposit?amount:-amount;var armor=(int[])ArmorSlots.Clone();if(Inventory[id]==0)for(int i=0;i<4;i++)if(ArmorSlots[i]==id)ArmorSlots[i]=0;
            if(!SaveSecretTransaction()){Inventory[id]=carried;h.storedItems[id]=stored;Array.Copy(armor,ArmorSlots,4);return false;}armorAppearanceRevision++;secretStatus=(deposit?"存入":"取回")+amount+"个"+Blocks.Names[id]+"。";return true;
        }
        public void CloseSecretMenu(bool lockSafe=true){if(lockSafe&&secretFocus!=null&&secretFocus.Kind==SecretFixtureKind.Safe)secretFocus.SetOpen(false);secretFocus=null;secretPin=secretPinAgain="";secretAuthenticated=false;SetPaused(false);}
        void DrawSecretMenu(float w,float height)
        {
            float x=w/2-390,y=height/2-270;Panel(new Rect(x,y,780,540),Paper);Label(new Rect(x+24,y+20,650,40),secretFocus.Kind==SecretFixtureKind.Hatch?"密室 · 木板伪装门":secretFocus.Kind==SecretFixtureKind.Safe?"密室 · 我的保险箱":"密室 · 我的储物柜",27,Ink);
            if(!secretAuthenticated){var h=secretFocus.Home;bool setup=string.IsNullOrEmpty(secretFocus.Kind==SecretFixtureKind.Hatch?h.doorHash:h.safeHash);Label(new Rect(x+24,y+85,720,60),setup?"第一次使用：自己设置4–12位数字密码。门和保险箱可以用不同密码。\n只用于这个游戏；请不要使用真实账户密码。":"请输入你设置的游戏密码。密码正确才能打开。",18,Ink);Label(new Rect(x+24,y+165,700,30),setup?"设置密码":"密码",18,Ink);secretPin=GUI.PasswordField(new Rect(x+24,y+205,700,42),secretPin,'●',12);if(setup){Label(new Rect(x+24,y+262,700,30),"再输入一次",18,Ink);secretPinAgain=GUI.PasswordField(new Rect(x+24,y+301,700,42),secretPinAgain,'●',12);}if(GUI.Button(new Rect(x+24,y+368,700,44),setup?"设置密码并打开":"解锁"))UnlockSecret(secretPin,secretPinAgain);}
            else if(secretFocus.Kind==SecretFixtureKind.Safe){Label(new Rect(x+24,y+85,700,40),"身上："+Inventory[(int)Block.Coin]+" 元     保险箱："+secretFocus.Home.safeMoney+" 元",24,Ink);Label(new Rect(x+24,y+155,700,30),"输入要存入或取出的金额（正整数）",17,Muted);secretAmount=GUI.TextField(new Rect(x+24,y+200,700,45),secretAmount,7);if(GUI.Button(new Rect(x+24,y+270,337,48),"存入保险箱")){if(int.TryParse(secretAmount,out var n))TransferSafeMoney(n,true);else secretStatus="请输入正确金额。";}if(GUI.Button(new Rect(x+387,y+270,337,48),"取回身上")){if(int.TryParse(secretAmount,out var n))TransferSafeMoney(n,false);else secretStatus="请输入正确金额。";}}
            else{Label(new Rect(x+24,y+70,710,45),"每次移动1件；钱存保险箱，邀请函保留在快捷栏。",17,Muted);secretScroll=GUI.BeginScrollView(new Rect(x+24,y+117,730,300),secretScroll,new Rect(0,0,705,CountCabinetRows()*48));int row=0;for(int id=1;id<Blocks.Count;id++)if(CabinetItemAllowed(id)&&(Inventory[id]>0||secretFocus.Home.storedItems[id]>0)){Label(new Rect(0,row*48,395,43),Blocks.Names[id]+" · 身上"+Inventory[id]+" / 柜里"+secretFocus.Home.storedItems[id],17,Ink);bool prior=GUI.enabled;GUI.enabled=Inventory[id]>0;if(GUI.Button(new Rect(406,row*48,120,38),"存入1件"))TransferCabinetItem(id,1,true);GUI.enabled=secretFocus.Home.storedItems[id]>0;if(GUI.Button(new Rect(540,row*48,130,38),"取出1件"))TransferCabinetItem(id,1,false);GUI.enabled=prior;row++;}GUI.EndScrollView();}
            Label(new Rect(x+24,y+434,720,36),secretStatus,16,Muted);if(GUI.Button(new Rect(x+24,y+484,730,36),"关闭 / Esc"))CloseSecretMenu();
        }
        int CountCabinetRows(){int n=0;for(int id=1;id<Blocks.Count;id++)if(CabinetItemAllowed(id)&&(Inventory[id]>0||secretFocus.Home.storedItems[id]>0))n++;return Mathf.Max(1,n);}
        public bool TrySecretSwingG(){if(SecretSwingActive){StopSecretSwing();return true;}var f=AimSecret();return f!=null&&f.Kind==SecretFixtureKind.Swing&&StartSecretSwing(f);}
        public bool StartSecretSwing(SecretRoomFixture f){if(f==null||f.Kind!=SecretFixtureKind.Swing||Paused||CompanionReturnBlocked)return false;secretSwingEntry=Player.transform.position;secretSwingFlying=Player.Flying;secretSwing=f;secretSwingPivot=f.MovingPart;secretSwingAge=0;Player.SetFlying(false);Player.BeginCoasterSeat();Player.HomeRest(true);Notice="坐上密室秋千，荡20秒；按G可以随时下来。";return true;}
        public void StopSecretSwing(){if(secretSwing==null)return;secretSwingPivot.localRotation=Quaternion.identity;Player.MoveCoasterSeat(secretSwingEntry);Player.ReleaseCoasterSeat();Player.HomeRest(false);Player.SetFlying(secretSwingFlying);secretSwing=null;secretSwingPivot=null;Notice="已从秋千安全下来。";}
        public void TickSecretRooms(float dt){if(Paused)return;if(SecretSwingActive){secretSwingAge+=dt;secretSwingPivot.localRotation=Quaternion.Euler(Mathf.Sin(secretSwingAge*1.8f)*18,0,0);Player.MoveCoasterSeat(secretSwingPivot.TransformPoint(new Vector3(0,-2.83f,0)));if(secretSwingAge>=20)StopSecretSwing();}visitorClock+=dt;if(secretVisitor!=null)secretVisitor.Advance(dt);else if(visitorClock>80){visitorClock=0;var h=NearestOwnedHome();if(h!=null&&h.secretRoomVersion==1&&Player.transform.position.y<h.y)StartRoomVisitor(h);}TickTsunami(dt);if(secretMonitorCamera!=null)RenderSecretMonitor();}
        public bool StartRoomVisitor(OwnedHome h){if(h==null||RoomFixture(h,SecretFixtureKind.Hatch)==null||secretVisitor!=null)return false;var person=MakeCommunityPerson(secretRoomRoot,"试锁后离开的村民",new Vector3(h.x+4.5f,h.y+2.02f,h.z+1.5f),new Color(.38f,.45f,.57f),"访客");person.GetComponent<CommunityCitizen>().enabled=false;secretVisitor=person.gameObject.AddComponent<SecretRoomVisitor>();secretVisitor.Game=this;secretVisitor.Home=h;if(h.deepBaseVersion==1&&h.doorGuests.Count>0)secretVisitor.ResidentKey=h.doorGuests[0];VisitorStatus="有个村民悄悄走向木板入口……";return true;}
        public bool SecretRoomSheltered(){foreach(var h in OwnedHomes)if(h.region==World.Region&&!h.realm&&h.secretRoomVersion==1){var p=Player.transform.position;var f=RoomFixture(h,SecretFixtureKind.Hatch);if(p.x>h.x&&p.x<h.x+10&&p.z>h.z&&p.z<h.z+10&&p.y>=(h.deepBaseVersion==1?DeepFloorY(h,9):h.y-5)&&p.y<h.y&&f!=null&&!f.Open)return true;}return false;}
        void ResetSecretRooms(){ResetDeepBase();StopSecretSwing();if(SecretRoomMenuOpen)CloseSecretMenu();DisposeSecretMonitor();StopTsunami();ResetFloodCare();secretVisitor=null;VisitorStatus="";visitorClock=0;locatedRoom=null;}
        public void RoomVisitorMessage(string text,bool finished){VisitorStatus=text;Notice=text;if(finished)secretVisitor=null;}
    }
    public class SecretRoomVisitor:MonoBehaviour
    {
        public Sandbox Game;public OwnedHome Home;public string ResidentKey="adult:0";float age;int phase,stair,stairMotion;bool frontDoorOpened;public bool AttemptedLock {get;private set;}public bool CouldNotFindDoor {get;private set;}public bool EnteredDeepRoom {get;private set;}
        Vector3 StairPoint(int index)=>new Vector3(Home.x+3,Home.y+1.53f-index*.5f,Home.z+2.42f+index*.5f);
        Vector3 StairExit=>new Vector3(Home.x+4.7f,Home.y-4.97f,Home.z+8.86f);
        public void Advance(float dt){if(Game==null||Game.Paused)return;age+=dt;var hatch=Game.RoomFixture(Home,SecretFixtureKind.Hatch);if(hatch==null){Destroy(gameObject);return;}if(Home.deepBaseVersion==1){AdvanceDeepVisitor(hatch,dt);return;}
            if(phase==4&&!frontDoorOpened){frontDoorOpened=true;foreach(var door in Game.World.GetComponentsInChildren<HomeFixture>())if(door.Kind==Block.IronDoor&&door.Cell==new Vector3Int(Home.x+4,Home.y+2,Home.z)&&!door.State.Open)door.Toggle();Physics.SyncTransforms();}
            Vector3 destination=phase==4?new Vector3(Home.x+4.5f,Home.y+2.02f,Home.z-.6f):phase==1||phase==3?(stair==14?StairExit:StairPoint(stair)):phase==2?new Vector3(Home.x+7.3f,Home.y-4.97f,Home.z+4.3f):hatch.Open?new Vector3(Home.x+3,Home.y+2.02f,Home.z+3):new Vector3(Home.x+4.7f,Home.y+2.02f,Home.z+(age<3?2:3.5f));
            // Walk past the stair edge before descending; rise before stepping onto
            // a higher landing. The capsule never cuts diagonally through a tread.
            if((phase==1||phase==3)&&stair<14&&stairMotion==0){if(phase==1)destination.y=transform.position.y;else destination=new Vector3(transform.position.x,destination.y,transform.position.z);}
            NpcNavigation.Ensure(transform).MoveTo(destination,1.6f,Mathf.Min(dt,.1f));
            if(phase==0&&age>=(hatch.Open?3:6)&&Vector3.Distance(transform.position,destination)<.6f){if(hatch.Open){phase=1;stair=stairMotion=0;age=0;Game.RoomVisitorMessage("木板门没关，村民看见楼梯，来试开保险箱。",false);}else{CouldNotFindDoor=true;phase=4;age=0;Game.RoomVisitorMessage("村民没认出木板暗门，找了一会儿就离开了。钱和物品没有丢。",false);}}
            else if(phase==1&&Vector3.Distance(transform.position,destination)<.08f){if(stair==14){phase=2;age=0;}else if(stairMotion==0)stairMotion=1;else{stair++;stairMotion=0;}}
            else if(phase==2&&age>=3&&Vector3.Distance(transform.position,destination)<.5f){AttemptedLock=true;phase=3;stair=14;stairMotion=0;age=0;Game.RoomVisitorMessage("村民打不开保险箱，只能沿楼梯离开。",false);}
            else if(phase==3&&Vector3.Distance(transform.position,destination)<.08f){if(stair==14){stair=13;stairMotion=0;}else if(stairMotion==0)stairMotion=1;else{stair--;stairMotion=0;if(stair< -1){phase=4;age=0;}}}
            if(phase==4&&age>=8){Game.RoomVisitorMessage("试锁的村民已经离开；保险箱和柜子里的物品都还在。",true);gameObject.SetActive(false);Destroy(gameObject);}
        }
        void AdvanceDeepVisitor(SecretRoomFixture hatch,float dt){bool knows=Game.ResidentKnowsDoor(Home,ResidentKey);if(!knows){CouldNotFindDoor=true;if(age>3)Game.RoomVisitorMessage("你没有告诉这个村民游戏门密码；他无法进入，即使看见木板也不会猜中。",false);if(age>8){Game.RoomVisitorMessage("不知道密码的村民离开了，基地安全。",true);gameObject.SetActive(false);Destroy(gameObject);}return;}if(phase==0){if(age<3)return;phase=1;age=0;hatch.SetOpen(true);EnteredDeepRoom=true;Game.RoomVisitorMessage("你主动告诉过这位村民游戏门密码，他通过升降椅进入第一层；保险箱仍然上锁。",false);}if(phase==1){var nav=GetComponent<NpcNavigation>();if(nav!=null)nav.enabled=false;transform.position=Vector3.Lerp(Sandbox.DeepCorner(Home,0),Sandbox.DeepLanding(Home,1),Mathf.Clamp01(age/3));if(age>=3){phase=2;age=0;}}else if(phase==2&&age>=4){AttemptedLock=true;phase=3;age=0;Game.RoomVisitorMessage("村民看看房间，打不开保险箱，随后离开。",false);}else if(phase==3){transform.position=Vector3.Lerp(Sandbox.DeepLanding(Home,1),Sandbox.DeepCorner(Home,0),Mathf.Clamp01(age/3));if(age>=3){hatch.SetOpen(false);Game.RoomVisitorMessage("知道游戏门密码的村民已经离开；没有拿走物品。",true);gameObject.SetActive(false);Destroy(gameObject);}}}
    }
}
