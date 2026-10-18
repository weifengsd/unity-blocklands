using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blocklands
{
    public partial class Sandbox : MonoBehaviour
    {
        public VoxelWorld World {get;private set;}
        public FirstPerson Player {get;private set;}
        public bool Paused {get;private set;}=true;
        public bool IsSmoke {get;private set;}
        public readonly int[] Inventory=new int[Blocks.Count];
        public readonly Crafting Crafting=new Crafting();
        public int Selected {get;private set;}=1;
        public string Notice {get;internal set;}="欢迎来到幻方世界！点击开始探索。";
        public string TargetName {get;private set;}="";
        bool inventoryOpen,started,saveBlocked,travelOpen;
        float nextDig,nextSave;
        Vector3Int target,adjacent;
        bool hasTarget;
        LineRenderer selection;
        AudioSource audioSource;
        AudioClip digSound,placeSound;
        Font font;
        readonly bool traceInput=Array.IndexOf(Environment.GetCommandLineArgs(),"-blocklands-input-log")>=0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(FindAnyObjectByType<Sandbox>()==null)new GameObject("Blocklands").AddComponent<Sandbox>();
        }
        void Awake()
        {
            IsSmoke=Array.IndexOf(Environment.GetCommandLineArgs(),"-blocklands-smoke")>=0;
            font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            loadingScreen=WorldLoadingScreen.Create();SetPaused(true);
        }
        static AudioClip MakeSound(float pitch)
        {
            int length=4410;var data=new float[length];var random=new System.Random(1);
            for(int i=0;i<length;i++)data[i]=(Mathf.Sin(i*pitch/44100*Mathf.PI*2)*.5f+(float)(random.NextDouble()-.5)*.5f)*(1-i/(float)length);
            var clip=AudioClip.Create("Block click",length,1,44100,false);clip.SetData(data,0);return clip;
        }
        void Update()
        {
            if(!GameReady)return;
            TickVehicle(Time.deltaTime);
            if(!Paused)TickFloodCare(Time.deltaTime);
            if(CarCargoOpen){if(!IsSmoke&&Input.GetKeyDown(KeyCode.Escape))CloseCarCargo();return;}
            if(CarRiding&&!Paused){if(!IsSmoke)CarKeys();return;}
            if(!Paused){TickLifestyle();TickHomeRides();TickCoasterSlip();TickForestFriend();TickInvitedChildren();TickVoidRealm();TickNeighbourSpeech();TickInvitationSticker();TickDeepBase(Time.deltaTime);TickSecretRooms(Time.deltaTime);TickVillageRefundClaims();}
            if(SecretRoomMenuOpen){if(!IsSmoke&&Input.GetKeyDown(KeyCode.Escape))CloseSecretMenu();return;}
            if(!Paused)TickHomeParty(Time.deltaTime);
            if(!Paused)TickPartyRide(Time.deltaTime);
            if(PartyRideActive){if(!IsSmoke){if(Input.GetKeyDown(KeyCode.G))StopPartyRide();if(Input.GetKeyDown(KeyCode.Escape))SetPaused(!Paused);}return;}
            if(PartyBagOpen){if(!IsSmoke&&(Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.Escape)))ClosePartyBag();return;}
            if(DronePanelOpen){if(!IsSmoke)DroneKeys();return;}
            if(SecretChairActive){if(!IsSmoke){if(Input.GetKeyDown(KeyCode.Alpha3))TrySecretChair3();if(Input.GetKeyDown(KeyCode.Escape))SetPaused(!Paused);}return;}
            if(DeepActivityActive){if(!IsSmoke){if(Input.GetKeyDown(KeyCode.G))CancelDeepActivity();if(Input.GetKeyDown(KeyCode.Escape))SetPaused(!Paused);}return;}
            if(SecretMonitorOpen){if(Health<=0)CloseSecretMonitor();else{if(!IsSmoke){if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyUp(KeyCode.H))CloseSecretMonitor();for(int i=0;i<3;i++)if(Input.GetKeyDown(KeyCode.Alpha1+i))SetSecretMonitorView(i);if(Input.GetKeyDown(KeyCode.RightArrow))SetSecretMonitorView((monitorView+1)%MonitorViewCount);if(Input.GetKeyDown(KeyCode.LeftArrow))SetSecretMonitorView((monitorView+MonitorViewCount-1)%MonitorViewCount);}return;}}
            if(!IsSmoke&&FestivalInputAvailable&&!StickerOpen&&Input.GetKeyDown(KeyCode.BackQuote)){StartTsunami();return;}
            if(!IsSmoke&&(SecretSwingActive||FestivalInputAvailable&&!StickerOpen)&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TrySecretSwingG())return;
            if(SecretSwingActive){if(!IsSmoke&&Input.GetKeyDown(KeyCode.Escape))SetPaused(!Paused);return;}
            if(LifeWorkshopOpen){if(!IsSmoke&&Input.GetKeyDown(KeyCode.E))OpenCrafting();else if(!IsSmoke&&Input.GetKeyDown(KeyCode.Escape))CloseLifeWorkshop();return;}
            if(CompanionMenuOpen){if(!IsSmoke&&Input.GetKeyDown(KeyCode.E))OpenCrafting();else if(!IsSmoke&&Input.GetKeyDown(KeyCode.Escape))CloseCompanionMenu();return;}
            if(StickerOpen){if(!IsSmoke)StickerKeys();return;}
            if(!IsSmoke&&!Paused&&!PhoneOpen&&Input.GetKeyDown(KeyCode.B)&&TryPartyHouseB())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.O)&&TryCarO())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryPortableRideG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryHomePartyG())return;
            if(!IsSmoke&&!Paused&&!PhoneOpen&&Input.GetKeyDown(KeyCode.Alpha3)&&TrySecretChair3())return;
            if(!IsSmoke&&!Paused&&!PhoneOpen&&Input.GetKeyDown(KeyCode.Alpha4)&&OpenDronePanel())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryDeepActivityG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.Return)&&TryPersonEnter())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.J)&&LeaveCoasterWithJ())return;
            if(!Paused&&ExcursionActive){TickExcursionAttraction();return;}
            if(!Paused&&SideRideActive){TickExcursionSideRide();return;}
            if(HandlePhoneInput())return;
            if(!IsSmoke&&!Paused&&!CoasterRiding&&Input.GetKeyDown(KeyCode.X)&&SendForestFriendHome())return;
            if(!IsSmoke&&!Paused&&PalaceConsoleOpen){if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.G))PalaceLeisure.CloseConsole();return;}
            if(!IsSmoke&&Input.GetKeyDown(KeyCode.Alpha1)&&HandleCastleReplayKey(KeyCode.Alpha1))return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryMeetFriendOrGift())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryCommunityActivityG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryVoidRealmG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryStoryCafeG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryExcursionG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryExcursionSideG())return;
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.G)&&TryEntertainmentG())return;
            if(!IsSmoke&&!Paused&&PalacePlayLocked){if(Input.GetKeyDown(KeyCode.Escape))SetPaused(true);return;}
            if(!IsSmoke&&!Paused&&FestivalLocked){if(Input.GetKeyDown(KeyCode.Escape))SetPaused(true);return;}
            if(!IsSmoke&&!Paused&&Input.GetKeyDown(KeyCode.J)&&RequestVisitWithJ())return;
            if(!IsSmoke)SampleH(Input.GetKeyDown(KeyCode.H),Input.GetKey(KeyCode.H),Input.GetKeyUp(KeyCode.H),Time.unscaledTime,!Paused&&!RoyalMenuOpen&&!inspecting);
            if(!IsSmoke&&!Paused&&TrySecretRoomH())return;
            if(!IsSmoke&&ShortH&&SeaBoat!=null&&SeaBoat.Interact())return;
            if(!IsSmoke&&!Paused&&ShortRoyalTV())return;
            if(!IsSmoke&&!Paused&&Input.GetMouseButtonDown(0)&&TryMuseumButton())return;
            if(!IsSmoke&&!Paused&&castle!=null)foreach(var key in new[]{KeyCode.Alpha2,KeyCode.Z,KeyCode.Alpha9,KeyCode.Alpha5,KeyCode.G,KeyCode.K,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.LeftArrow,KeyCode.RightArrow})if(Input.GetKeyDown(key)&&RoyalLeisureKey(key))return;
            if(!IsSmoke&&!Paused&&castle!=null&&castle.PlayerSunLocked){if(Input.GetKeyDown(KeyCode.Escape))SetPaused(true);return;}
            if(!IsSmoke&&RoyalKeys())return;
            if(!IsSmoke&&ForestShopKey())return;
            if(!IsSmoke&&!Paused&&castle!=null&&castle.AnyLiftMenu){CastleKeys();return;}
            if(IsSmoke){SurvivalUpdate();if(!Paused)Target();return;}
            LifeKeys();
            if(houseMenu&&Paused){if(Input.GetKeyDown(KeyCode.Escape))SetPaused(false);return;}
            if(forestShopOpen&&Paused){if(Input.GetKeyDown(KeyCode.Escape)){forestShopOpen=false;SetPaused(false);}return;}
            if(villageMenu!=null&&Paused){if(Input.GetKeyDown(KeyCode.Escape))SetPaused(false);return;}
            if(Health<=0||VictoryOpen){if(Input.GetKeyDown(KeyCode.Return)){if(VictoryOpen){VictoryOpen=false;TravelTo(0);}else Respawn();}return;}
            if(Input.GetKeyDown(KeyCode.Return)&&Paused&&!inventoryOpen&&!travelOpen){SetPaused(false);return;}
            if(Input.GetKeyDown(KeyCode.Escape)){inventoryOpen=false;travelOpen=false;SetPaused(!Paused);}
            if(Input.GetKeyDown(KeyCode.T))OpenTravel();
            if(Paused&&travelOpen){int count=excursionTab?6:10;for(int i=0;i<count;i++)if(Input.GetKeyDown(i==9?KeyCode.Alpha0:KeyCode.Alpha1+i))TravelTo(excursionTab?11+i:i);return;}
            if(Input.GetKeyDown(KeyCode.E))ToggleCrafting();
            if(Input.GetKeyDown(KeyCode.F5))Save();
            if(Input.GetKeyDown(KeyCode.F9)){inventoryOpen=false;SetPaused(true);Load();}
            if(Paused&&inventoryOpen)
            {
                for(int i=1;i<=9;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i-1)))SelectItem(QuickItems[i-1]);
                CraftingKeys();
            }
            SurvivalUpdate();
            if(Paused)return;
            Target();
            if(HomeKeys())return;
            if(CastleKeys())return;
            for(int i=1;i<=9;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i-1)))SelectItem(QuickItems[i-1]);
            float scroll=Input.mouseScrollDelta.y;if(scroll!=0){int slot=System.Array.IndexOf(QuickItems,Selected);SelectItem(QuickItems[(Mathf.Max(0,slot)+(scroll>0?8:1))%9]);}
            if(Input.GetMouseButtonDown(0)&&TryDirectFloorButton())return;
            if(Input.GetMouseButtonDown(0)&&RayFromView(out var partyHit,6)){var partyProp=partyHit.collider.GetComponentInParent<HomePartyPropView>();if(partyProp!=null){RemovePartyProp(partyProp.Data.id);return;}}
            if(partyHeld>=0&&Input.GetMouseButtonDown(1)){PartyPlaceFromView();return;}
            if(Input.GetMouseButtonDown(0)&&RayFromView(out var cameraHit,6)){var camera=cameraHit.collider.GetComponentInParent<CameraFixture>();if(camera!=null){RemoveInstalledCamera(camera.Data.id);return;}}
            if(Selected==94&&Input.GetMouseButtonDown(1)){InstallCameraFromView();return;}
            if(Input.GetMouseButtonDown(1)&&TryVillageControl())return;
            if(FlowerWandActive&&Input.GetMouseButton(0)){FireFlower();return;}
            if(!EmptyHands&&Selected==85&&Input.GetMouseButton(0)&&(!Input.GetKey(KeyCode.LeftShift)||!hasTarget)){FireCrystalMagic();return;}
            if(!EmptyHands&&Selected==90&&Input.GetMouseButton(0)&&(!Input.GetKey(KeyCode.LeftShift)||!hasTarget)){if(Input.GetMouseButtonDown(0))AlloySlash();return;}if(Input.GetMouseButton(0)&&StrikeActor())return;
            if(hasTarget&&Input.GetMouseButton(0)&&Time.time>=nextDig)
            {
                Block b=World.Get(target);nextDig=Time.time+Blocks.DigDelay(!EmptyHands&&Inventory[Selected]>0?(Block)Selected:Block.Air,b);
                if(Blocks.Buildable(b) && Inventory[(int)b]>=9999)Notice="这种方块的背包已满。";
                else if(MineWithHeldTool(target))Target();
            }
            if(hasTarget&&Input.GetMouseButtonDown(1))
            {
                if(World.Get(target)==Block.Workbench)OpenCrafting();else Place();
            }
            if(Time.time>=nextSave){Save();nextSave=Time.time+30;}
        }
        void Target()
        {
            hasTarget=RayFromView(out RaycastHit hit,6);
            if(hasTarget)
            {
                var deepObject=hit.collider.GetComponentInParent<DeepRoomObject>();if(deepObject!=null){hasTarget=false;selection.enabled=false;TargetName=deepObject.name;return;}
                var partyProp=hit.collider.GetComponentInParent<HomePartyPropView>();if(partyProp!=null){hasTarget=false;selection.enabled=false;TargetName=PartyItemNames[partyProp.Data.kind]+" · G使用 / 左键收回";return;}
                var partyGuest=hit.collider.GetComponentInParent<HomePartyGuestView>();if(partyGuest!=null){hasTarget=false;selection.enabled=false;TargetName=partyGuest.Data.name+" · G聊天 / 唱歌 / 跳舞";return;}
                var lens=hit.collider.GetComponentInParent<CameraFixture>();if(lens!=null){hasTarget=false;selection.enabled=false;TargetName="摄像头"+lens.Data.id+" · 密室屏幕查看 · 左键拆回背包";return;}
                var flooded=hit.collider.GetComponentInParent<FloodResident>();if(flooded!=null&&flooded.Downed){hasTarget=false;selection.enabled=false;TargetName=flooded.Assigned?"医护救援中 · "+Mathf.RoundToInt(flooded.CareProgress*100)+"%":"倒地村民 · 等待医护，或重进村庄恢复";return;}
                var secret=hit.collider.GetComponentInParent<SecretRoomFixture>();if(secret!=null){hasTarget=false;selection.enabled=false;TargetName=secret.Caption;return;}
                var relic=hit.collider.GetComponentInParent<MuseumRelic>();if(relic!=null){target=relic.Cell;adjacent=target+Vector3Int.up;selection.enabled=false;TargetName=relic.Title+" · H触碰会触发警报";return;}
                var royalButton=hit.collider.GetComponent<RoyalFloorButton>();if(royalButton!=null){hasTarget=false;selection.enabled=false;TargetName="第"+(royalButton.Floor+1)+"层 · 左键选择";return;}
                var fixture=hit.collider.GetComponentInParent<HomeFixture>();if(fixture!=null){target=fixture.Cell;adjacent=target+Vector3Int.up;selection.enabled=false;TargetName=Blocks.Names[(int)fixture.Kind]+" · H开关";return;}
                var control=hit.collider.GetComponent<VillageControl>();if(control!=null){hasTarget=false;selection.enabled=false;TargetName=control.Caption;return;}
                var actor=hit.collider.GetComponentInParent<CombatActor>();
                if(actor!=null){hasTarget=false;selection.enabled=false;TargetName=actor.Caption;return;}
                var person=hit.collider.GetComponentInParent<Villager>();
                if(person!=null){hasTarget=false;selection.enabled=false;TargetName=person.IsFriend?"小禾 · 靠近按 G 聊天":"村民";return;}
                var family=hit.collider.GetComponentInParent<SkyResidentInfo>();if(family!=null){hasTarget=false;selection.enabled=false;TargetName=family.Role;return;}
                target=Vector3Int.FloorToInt(hit.point-hit.normal*.01f);adjacent=Vector3Int.FloorToInt(hit.point+hit.normal*.01f);
                hasTarget=VoxelWorld.InBounds(target)&&World.Get(target)!=Block.Air;
            }
            selection.enabled=hasTarget;TargetName=hasTarget?Blocks.Names[(int)World.Get(target)]+(World.Get(target)==Block.Workbench?" · 右键合成":World.Get(target)==Block.Bed||World.Get(target)==Block.Sofa?" · H休息/起身":World.Get(target)==Block.Bookshelf?" · K拿书/放回":""):"";
            if(!hasTarget)return;
            Vector3[] points={new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,1,0),new Vector3(0,1,0),new Vector3(0,0,0),new Vector3(0,0,1),new Vector3(1,0,1),new Vector3(1,0,0),new Vector3(1,0,1),new Vector3(1,1,1),new Vector3(1,1,0),new Vector3(1,1,1),new Vector3(0,1,1),new Vector3(0,1,0),new Vector3(0,1,1),new Vector3(0,0,1)};
            for(int i=0;i<points.Length;i++)points[i]=(Vector3)target+Vector3.one*.5f+(points[i]-Vector3.one*.5f)*1.004f;
            selection.SetPositions(points);
        }
        public bool TryPlace(Vector3Int cell,Block block)
        {
            if(block==Block.LavaBucket){
                if(Inventory[(int)block]<=0||Inventory[(int)Block.Bucket]>=9999||!VoxelWorld.InBounds(cell)||World.Get(cell)!=Block.Air||new Bounds((Vector3)cell+Vector3.one*.5f,Vector3.one).Intersects(Player.Body.bounds))return false;
                if(!World.Edit(cell,Block.Lava))return false;Inventory[(int)block]--;Inventory[(int)Block.Bucket]++;return true;
            }
            if(!Blocks.Buildable(block)||Inventory[(int)block]<=0||!VoxelWorld.InBounds(cell)||Blocks.Solid(World.Get(cell)))return false;
            if(block==Block.IronDoor||block==Block.Window){int width=block==Block.IronDoor?2:1,height=block==Block.IronDoor?3:2;for(int x=0;x<width;x++)for(int y=0;y<height;y++){var p=cell+new Vector3Int(x,y,0);if(!VoxelWorld.InBounds(p)||World.Get(p)!=Block.Air||new Bounds((Vector3)p+Vector3.one*.5f,Vector3.one).Intersects(Player.Body.bounds))return false;}}
            var bounds=new Bounds((Vector3)cell+Vector3.one*.5f,Vector3.one*.99f);
            if(bounds.Intersects(Player.Body.bounds))return false;
            Block previous=World.Get(cell);if(!World.Edit(cell,block))return false;
            RecordVillagePlacement(cell,block,previous);
            Inventory[(int)block]--;return true;
        }
        void Place()
        {
            if(TryPlace(adjacent,(Block)Selected)){audioSource.PlayOneShot(placeSound);Notice="放置了 "+Blocks.Names[Selected];Target();}
            else Notice="这里无法放置：请留出身体空间，并检查库存与边界。";
        }
        public bool CollectCraft()
        {
            if(!Crafting.Collect(Inventory,out Recipe result)){Notice="还不能合成：请检查摆放、材料和背包容量。";return false;}
            Notice="合成成功："+result.Name+" × "+result.Count+"，已放入背包。";return true;
        }
        public bool CraftingOpen=>inventoryOpen;
        public void ToggleCrafting(){if(inventoryOpen)SetPaused(false);else OpenCrafting();}
        public void OpenCrafting(){PutAwayFlowerWand();companionMenuOpen=false;chatChild=-1;lifeWorkshopOpen=false;activeStroke=null;partyBag=false;travelOpen=false;forestShopOpen=false;houseMenu=false;villageMenu=null;inventoryOpen=true;Notice="按右侧配方摆放材料；领取结果时才消耗。";SetPaused(true);}
        public void SelectItem(int id){if(Blocks.Selectable(id)&&id!=91&&id!=92){EmptyHands=false;PutAwayPartyItem();PutAwayFlowerWand();Selected=id;if(System.Array.IndexOf(QuickItems,id)<0)QuickItems[id==93||System.Array.IndexOf(QuickItems,93)<0?8:7]=id;}}
        public void SetPaused(bool value)
        {
            Paused=value;Time.timeScale=value?0:1;Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=value;
            nextDig=Time.time+.25f;
            if(Player!=null)Player.ResetJumpTap();
            if(!value){inventoryOpen=false;travelOpen=false;villageMenu=null;houseMenu=false;Crafting.Clear();}
            if(!value)started=true;
        }
        public void Save()
        {
            if(!GameReady)return;
            if(saveBlocked){Notice="原存档读取失败，已停止自动覆盖。请备份并检查存档文件。";return;}
            try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);Notice="世界与背包已保存。";}
            catch(Exception e){Notice="保存失败："+e.Message;Debug.LogWarning(Notice);}
        }
        public void Load()
        {
            if(!File.Exists(WorldSave.PathName)){Notice="还没有存档，先探索一下吧。";return;}
            try
            {
                var save=WorldSave.Read(WorldSave.PathName);if(save.region==1){int returned=PrepareVillageReturn(save);if(returned>0)WorldSave.Write(save,WorldSave.PathName);}ResetCompanionReturns();bool migratedStar=ConnectedSaveUpgrade.Apply(save);World.RegionEdits.Clear();
                if(save.regions!=null)foreach(var r in save.regions)World.RegionEdits[r.id]=r.edits;
                FallenResidents.Clear();if(save.fallenResidents!=null)FallenResidents.AddRange(save.fallenResidents);
                LostSkyResidents.Clear();if(save.lostSkyResidents!=null)foreach(int id in save.lostSkyResidents)LostSkyResidents.Add(id);
                InvitedChildStyles.Clear();if(save.invitedChildren!=null)InvitedChildStyles.AddRange(save.invitedChildren);PlanetHeavenVisitors.Clear();if(save.planetHeavenVisitors!=null)foreach(int id in save.planetHeavenVisitors)PlanetHeavenVisitors.Add(id);
                PalaceSavings=save.palaceSavings;
                World.Generate(save.region);
                World.ApplyEdits(save.edits);PrincessReturnVersion=save.princessReturnVersion;CompanionRevision=save.companionRevision;CompanionUnlocked=save.companionUnlocked||save.companion||save.houseBase!=null&&save.houseBase.Count>0;CompanionEnabled=save.companion||save.houseBase!=null&&save.houseBase.Count>0&&!save.companionUnlocked;RoyalIdentity=save.royal;DeepestVisit=save.deepest;
                HouseBase.Clear();if(save.houseBase!=null)HouseBase.AddRange(save.houseBase);World.RebuildAll();Array.Copy(save.inventory,Inventory,Inventory.Length);Selected=save.selected;Crafting.Clear();RestoreSurvival(save);
                Vector3 p=save.position;
                if(World.Region==3&&!GateUnlocked){World.SwitchRegion(0);p=World.SafeSpawn();}
                bool valid=Mathf.Abs(p.x)<10000&&Mathf.Abs(p.z)<10000&&p.y>=HellFloor-150&&p.y<=FirstPerson.FlightCeiling;
                if(!valid||Blocks.Solid(World.Get(Vector3Int.FloorToInt(p+Vector3.up*.1f)))||Blocks.Solid(World.Get(Vector3Int.FloorToInt(p+Vector3.up*1.7f))))p=World.Spawn;
                RestoreLifestyle(save);RestoreMobility(save);RestoreNpcCombat(save);RestoreVillageReturns(save);RestoreCameraKit(save);RestoreHomeParty(save);Player.Teleport(p,save.yaw,save.pitch);Player.SetFlying(save.flying);RefreshRegion();if(migratedStar&&SeaBoat!=null)SeaBoat.Board();RestoreForestFriend(save);RestoreInvitationParty(save);saveBlocked=false;Notice="已经恢复上次的世界和背包。";
            }
            catch(Exception e){saveBlocked=true;Notice="存档读取失败，原文件未改动："+e.Message;Debug.LogWarning(Notice);}
        }
        void OnApplicationFocus(bool focus){if(!IsSmoke&&!focus&&Player!=null&&!(PhoneOpen&&VoiceListening))SetPaused(true);}
        void OnApplicationQuit(){if(GameReady&&Player!=null)Save();}
        void OnDestroy(){Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;if(selection!=null)Destroy(selection.sharedMaterial);Destroy(digSound);Destroy(placeSound);}

        void OnGUI()
        {
            if(!GameReady||Player==null||WorldLoadingScreen.Active!=null)return;
            if(CarCargoOpen){float carScale=Mathf.Min(Screen.width/1000f,Screen.height/650f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*carScale);GUI.skin.font=font;DrawCarCargo(Screen.width/carScale,Screen.height/carScale);return;}
            if(IsSmoke&&(Event.current.isMouse||Event.current.isKey||Event.current.type==EventType.ScrollWheel))Event.current.Use();
            if(traceInput&&(Event.current.type==EventType.MouseDown||Event.current.type==EventType.MouseUp))Debug.Log("UI_INPUT "+Event.current.type+" pos="+Event.current.mousePosition+" screen="+Screen.width+"x"+Screen.height+" paused="+Paused+" lock="+Cursor.lockState);
            float scale=Mathf.Min(Screen.width/1000f,Screen.height/650f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
            float w=Screen.width/scale,h=Screen.height/scale;
            GUI.skin.font=font;GUI.skin.label.fontSize=16;GUI.skin.button.fontSize=16;GUI.skin.box.fontSize=16;
            if(SecretRoomMenuOpen){DrawSecretMenu(w,h);return;}
            if(PartyBagOpen){DrawPartyBag(w,h);return;}
            if(DronePanelOpen){DrawDronePanel(w,h);return;}
            if(SecretChairMenuOpen){DrawDeepBase(w,h);return;}
            if(SecretMonitorOpen){DrawSecretMonitor(w,h);return;}
            if(LifeWorkshopOpen){DrawLifeWorkshop(w,h);return;}
            if(CompanionMenuOpen){DrawCompanionMenu(w,h);return;}
            if(StickerOpen){DrawInvitationSticker(w,h);return;}
            if(PhoneOpen&&!Paused){DrawPhone(w,h);return;}
            if(!Paused&&ExcursionActive)return;
            if(!Paused&&SideRideActive)return;
            if(PalaceConsoleOpen&&!Paused){PalaceLeisure.Draw(w,h);return;}
            if(inspecting){Label(new Rect(w/2-300,h-80,600,50),InvitedChildCount>0?"多人牵手外观 · 移动鼠标环顾 · C返回":"女主角外观 · 移动鼠标转动视角 · C返回",20,Color.white,TextAnchor.MiddleCenter);return;}
            if(Paused&&inventoryOpen&&Health>0&&!VictoryOpen){DrawCrafting(w,h);return;}
            DrawHomeBook(w,h);
            DrawSecretStatus(w,h);
            DrawRoomLocation(w,h);
            DrawDeepBase(w,h);
            Panel(new Rect(22,20,370,120),new Color(.08f,.13f,.13f,.87f));
            Label(new Rect(40,28,335,40),"幻方世界 · 2.27",22,new Color(.97f,.88f,.63f));
            GUI.Label(new Rect(40,66,335,32),"探索 · 采集 · 建造  |  E 背包 / 合成");
            GUI.Label(new Rect(40,99,335,32),(InHeaven?"天空天堂":InPlanetLife?"星球生活大陆":InHell?"地狱":VoxelWorld.RegionNames[World.Region])+" · T 旅行 · "+Mathf.FloorToInt(Player.transform.position.x)+" / "+Mathf.FloorToInt(Player.transform.position.y)+" / "+Mathf.FloorToInt(Player.transform.position.z));
            Panel(new Rect(w-340,20,318,102),new Color(.08f,.13f,.13f,.87f));
            Label(new Rect(w-322,31,290,28),Player.Flying?"飞行中 · 空格上升 / Shift下降":InVoidRealms?"步行中 · 自己按 F 或双击空格才飞":World.Region==4?"双击空格起飞 · 门旁 F 激活":"步行中 · 双击空格 / F 起飞",16,new Color(.88f,.92f,.73f));
            Label(new Rect(w-322,65,290,32),partyHeld>=0?"手持："+PartyHeldName+" × "+PartyStock[partyHeld]:(EmptyHands?"空手":"手持："+Blocks.Names[Selected])+"  × "+Inventory[Selected],16,Color.white);
            Label(new Rect(w/2-15,h/2-20,30,40),"+",28,Color.white);
            Label(new Rect(w/2-100,h/2+22,200,28),TargetName,15,Color.white,TextAnchor.MiddleCenter);
            if(!Paused&&!StickerOpen&&!PhoneOpen){var person=NearbyPerson();if(person!=null&&Vector3.Distance(person.position,Player.transform.position)<4)Label(new Rect(w/2-290,h-178,580,26),Selected==93?"看着收信人 · Enter亲手递出邀请函":"附近有人 · Enter主动打开黄色对话条",16,new Color(1,.91f,.62f),TextAnchor.MiddleCenter);}
            if(InVoidRealms&&!Paused)Label(new Rect(w/2-290,h/2+52,580,36),RealmInteractionCaption(),17,new Color(1,.94f,.68f),TextAnchor.MiddleCenter);
            float left=(w-9*74)/2;
            for(int i=1;i<=9;i++)
            {
                int item=QuickItems[i-1];
                Rect slot=new Rect(left+(i-1)*74,h-95,69,73);
                Panel(slot,item==Selected?new Color(.81f,.69f,.39f,.95f):new Color(.08f,.13f,.13f,.85f));
                ItemIcon(new Rect(slot.x+22,slot.y+7,28,24),(Block)item);
                Label(new Rect(slot.x+4,slot.y+35,61,19),Blocks.Names[item],12,Color.white,TextAnchor.MiddleCenter);
                Label(new Rect(slot.x+4,slot.y+53,61,19),i+" · "+Inventory[item],12,Color.white,TextAnchor.MiddleCenter);
            }
            Label(new Rect(22,h-135,w-44,30),Notice,16,Color.white,TextAnchor.MiddleCenter);
            SurvivalHUD(w,h);
            DrawCastleUI(w,h);
            DrawForestFriend(w,h);
            if(PalaceLeisure!=null)PalaceLeisure.Draw(w,h);
            DrawPhoneHint(w,h);
            if(!Paused)DrawVehicleHud(w,h);
            if(Health<=0||VictoryOpen){DrawAdventureEnd(w,h);return;}
            if(!Paused)return;
            Panel(new Rect(0,0,w,h),new Color(.03f,.08f,.1f,.72f));
            if(houseMenu){DrawHouseMenu(w,h);return;}
            if(forestShopOpen){DrawForestShop(w,h);return;}
            if(villageMenu!=null){DrawVillageMenu(w,h);return;}
            if(travelOpen){DrawTravel(w,h);return;}
            if(!CompanionReturnBlocked&&GUI.Button(new Rect(22,160,176,50),"伙伴\n聊天 / 坐车送回")){OpenCompanionMenu();return;}
            if(!CompanionReturnBlocked&&GUI.Button(new Rect(22,223,176,62),"生活工坊\n纸笔 / 装备 / 建房")){OpenLifeWorkshop();return;}
            Rect box=new Rect(w/2-290,h/2-245,580,490);Panel(box,new Color(.08f,.15f,.17f,.98f));
            float x=box.x+30,y=box.y+24;
            if(appIcon!=null)GUI.DrawTexture(new Rect(x,y,46,46),appIcon,ScaleMode.ScaleToFit);
            Label(new Rect(x+58,y,462,42),started?"歇一会儿，再去远方":"幻方世界 · 梦影冒险",27,new Color(.96f,.86f,.61f));y+=58;
            {
                GUI.Label(new Rect(x,y,520,138),"从林地小屋旁出发 · T旅行 / E合成 / R吃食物\nW/S前后、左右方向键横移 / A随时拿手机\n鼠标环顾 / 空格跳跃或双击飞行；Shift降\n左键采集或攻击 / 右键放置 / 1–9选材\n集14黑紫晶块+6梦影绿晶，去熔岩竹谷造门");y+=142;
                if(GUI.Button(new Rect(x,y,520,47),started?"继续探索":"开始探索"))SetPaused(false);y+=62;
                if(GUI.Button(new Rect(x,y,252,42),"保存世界  [F5]"))Save();
                if(GUI.Button(new Rect(x+267,y,252,42),"读取存档  [F9]"))Load();y+=57;
                if(GUI.Button(new Rect(x,y,520,42),"回到当前地区入口（不删除建造）")){Player.Teleport(World.SafeSpawn(),World.Region==0?25:0);Notice="已回到当前地区入口。";SetPaused(false);}y+=53;
                if(GUI.Button(new Rect(x,y,520,37),"去远方 · 旅行地图  [T]"))OpenTravel();
            }
        }
        public static void Panel(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        public static void Label(Rect r,string text,int size,Color color,TextAnchor align=TextAnchor.UpperLeft){var style=new GUIStyle(GUI.skin.label){fontSize=size,alignment=align,padding=new RectOffset(0,0,0,0),clipping=TextClipping.Overflow};style.normal.textColor=color;GUI.Label(r,text,style);}
    }
}
