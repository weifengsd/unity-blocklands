using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    [Serializable] public class SavedBlock { public int x,y,z,id; }
    [Serializable] public class SavedRegion { public int id;public List<SavedBlock> edits=new List<SavedBlock>(); }
    [Serializable] public class SavedWorld
    {
        public bool emptyHands;public List<MiningPickup> miningPickups=new List<MiningPickup>();public OwnedVehicle vehicle;
        public string preciseHealth;public List<NpcCombatState> npcCombat=new List<NpcCombatState>();
        public int version=1,seed=VoxelWorld.Seed,region;
        public Vector3 position;
        public float yaw,pitch;
        public bool flying;
        public bool companion,companionUnlocked,royal;public int deepest=5;
        public int princessReturnVersion,companionRevision;
        public int continuousVersion;
        public int palaceSavings;
        public bool friendFollowing,friendWaiting,friendGiftWaiting;
        public List<FallenResident> fallenResidents=new List<FallenResident>();
        public List<int> lostSkyResidents=new List<int>();
        public List<int> invitedChildren=new List<int>(),planetHeavenVisitors=new List<int>();
        public int survivalVersion;
        public float health=10,dragonHealth=100;
        public bool gateUnlocked;
        public List<int> deadAnimals=new List<int>();
        public List<SavedBlock> stolen=new List<SavedBlock>();
        public int selected;
        public List<SavedBlock> houseBase=new List<SavedBlock>();
        public int lifestyleVersion;public List<OwnedHome> ownedHomes=new List<OwnedHome>();public List<DrawingStroke> drawing=new List<DrawingStroke>();public int[] armorSlots=new int[4];public bool villageGuardian;public string invitationTitle,invitationDetails;public int invitationGuests=3,invitationsSent;
        public int invitationVersion;public List<InvitationDesign> invitationDesigns=new List<InvitationDesign>();public List<int> madeInvitations=new List<int>();public int[] quickItems;
        public int[] inventory;
        public List<VillagePlacedBlock> villagePlacements=new List<VillagePlacedBlock>();public int[] villageRefunds;
        public int cameraKitVersion;public List<InstalledCamera> installedCameras=new List<InstalledCamera>();
        public int partyKitVersion;public int[] partyStock;public List<HomePartyGuest> partyGuests=new List<HomePartyGuest>();public List<HomePartyProp> partyProps=new List<HomePartyProp>();public string partyHome;public bool partyRunning,partyNight;
        public List<SavedBlock> edits=new List<SavedBlock>();
        public List<SavedRegion> regions=new List<SavedRegion>();
    }
    public static partial class WorldSave
    {
        public static string PathName
        {
            get
            {
                var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-blocklands-smoke");
                // Keep existing macOS worlds when the visible game title changes.
                string stableFolder=(Application.platform==RuntimePlatform.OSXPlayer||Application.platform==RuntimePlatform.OSXEditor)
                    ?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support","Creatives","Blocklands")
                    :Application.persistentDataPath;
#if BLOCKLANDS_SAFE_PREVIEW
                string originalSave=Path.Combine(stableFolder,"blocklands-world.json");stableFolder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support","Creatives","BlocklandsLifestylePreview");
                if(index<0&&!File.Exists(Path.Combine(stableFolder,"blocklands-world.json"))&&File.Exists(originalSave)&&new FileInfo(originalSave).Length<=96*1024*1024){Directory.CreateDirectory(stableFolder);File.Copy(originalSave,Path.Combine(stableFolder,"blocklands-world.json"),false);}
#endif
                string folder=index>=0&&index+1<args.Length?args[index+1]:stableFolder;
                return Path.Combine(folder,"blocklands-world.json");
            }
        }
        public static SavedWorld Capture(VoxelWorld world,FirstPerson player,int[] inventory,int selected)
        {
            var save=new SavedWorld{region=world.Region,position=player.Game.SafePlayerSavePosition,yaw=player.transform.eulerAngles.y,pitch=player.Pitch,flying=player.Game.SafePlayerSaveFlying,selected=selected,inventory=(int[])inventory.Clone()};
            save.survivalVersion=1;save.health=player.Game.Health;save.dragonHealth=player.Game.DragonHealth;save.gateUnlocked=player.Game.GateUnlocked;save.deadAnimals=new List<int>(player.Game.DeadAnimals);
            save.emptyHands=player.Game.EmptyHands;save.miningPickups=new List<MiningPickup>(player.Game.MiningPickups);save.vehicle=player.Game.VehicleData;
            save.preciseHealth=player.Game.PreciseHealth.ToString(System.Globalization.CultureInfo.InvariantCulture);save.npcCombat=new List<NpcCombatState>(player.Game.NpcCombatStates);
            save.stolen=new List<SavedBlock>(player.Game.Theft);
            save.houseBase=new List<SavedBlock>(player.Game.HouseBase);
            save.lifestyleVersion=1;save.ownedHomes=new List<OwnedHome>(player.Game.OwnedHomes);save.drawing=new List<DrawingStroke>(player.Game.Drawing);save.armorSlots=(int[])player.Game.ArmorSlots.Clone();save.villageGuardian=player.Game.VillageGuardianEnabled;save.invitationTitle=player.Game.InvitationTitle;save.invitationDetails=player.Game.InvitationDetails;save.invitationGuests=player.Game.InvitationGuests;save.invitationsSent=player.Game.InvitationsSent;
            save.villagePlacements=player.Game.CaptureVillagePlacements();save.villageRefunds=(int[])player.Game.VillageRefunds.Clone();
            save.cameraKitVersion=player.Game.CameraKitVersion;save.installedCameras=new List<InstalledCamera>(player.Game.InstalledCameras);
            save.partyKitVersion=player.Game.PartyKitVersion;save.partyStock=(int[])player.Game.PartyStock.Clone();save.partyGuests=new List<HomePartyGuest>(player.Game.HomePartyGuests);save.partyProps=new List<HomePartyProp>(player.Game.HomePartyProps);save.partyHome=player.Game.PartyHomeKey;save.partyRunning=player.Game.HomePartyRunning;save.partyNight=player.Game.IsNight;
            save.invitationVersion=2;save.invitationDesigns=new List<InvitationDesign>(player.Game.InvitationDesigns);save.madeInvitations=new List<int>(player.Game.MadeInvitations);save.quickItems=(int[])player.Game.QuickItems.Clone();
            save.companion=player.Game.CompanionEnabled;save.companionUnlocked=player.Game.CompanionUnlocked;save.royal=player.Game.RoyalIdentity;save.deepest=player.Game.DeepestVisit;
            save.princessReturnVersion=player.Game.PrincessReturnVersion;save.companionRevision=player.Game.CompanionRevision;
            save.continuousVersion=1;
            save.palaceSavings=player.Game.PalaceSavings;
            save.friendFollowing=player.Game.ForestFriendFollowing;save.friendWaiting=player.Game.ForestFriendWaiting||player.Game.ForestFriendArriving;save.friendGiftWaiting=player.Game.FriendGiftWaiting||player.Game.FriendGiftComing;
            player.Game.RememberFallenResidents();save.fallenResidents=new List<FallenResident>(player.Game.FallenResidents);
            save.lostSkyResidents=new List<int>(player.Game.LostSkyResidents);
            save.invitedChildren=new List<int>(player.Game.InvitedChildStyles);save.planetHeavenVisitors=new List<int>(player.Game.PlanetHeavenVisitors);
            foreach(var edit in world.Edits)save.edits.Add(new SavedBlock{x=edit.Key.x,y=edit.Key.y,z=edit.Key.z,id=(int)edit.Value});
            foreach(var r in world.RegionEdits)if(r.Key!=world.Region)save.regions.Add(new SavedRegion{id=r.Key,edits=r.Value});
            return save;
        }
        public static void Validate(SavedWorld save)
        {
            // Inventory-only extension: migrate the immediately preceding release too.
            if(save!=null&&save.inventory!=null&&(save.inventory.Length==59||save.inventory.Length==64||save.inventory.Length==68||save.inventory.Length==74||save.inventory.Length==76||save.inventory.Length==77||save.inventory.Length==94))Array.Resize(ref save.inventory,Blocks.Count);
            if(save==null||save.version!=1||save.seed!=VoxelWorld.Seed||save.region<0||save.region>=VoxelWorld.RegionNames.Length||save.inventory==null||(save.inventory.Length!=12&&save.inventory.Length!=16&&save.inventory.Length!=36&&save.inventory.Length!=49&&save.inventory.Length!=51&&save.inventory.Length!=52&&save.inventory.Length!=58&&save.inventory.Length!=59&&save.inventory.Length!=64&&save.inventory.Length!=Blocks.Count)||save.edits==null)
                throw new InvalidDataException("存档格式或版本不匹配");
            if(save.houseBase!=null){ValidateEdits(save.houseBase);foreach(var b in save.houseBase)if(b.x<28||b.x>37||b.z<50||b.z>59||b.y<14||b.y>23)throw new InvalidDataException("自建房范围不合法");}
            if(save.ownedHomes!=null){if(save.ownedHomes.Count>100)throw new InvalidDataException("房屋太多");foreach(var h in save.ownedHomes)if(h==null||h.region<0||h.region>=VoxelWorld.RegionNames.Length||(!h.realm&&(h.x<0||h.x+10>VoxelWorld.Width||h.z<0||h.z+10>VoxelWorld.Depth||h.y<0||h.y+10>=VoxelWorld.Height))||(h.realm&&(h.x<-220||h.x>256||h.z<-220||h.z>256||(h.y!=-649&&h.y!=-869&&h.y!=-1329))))throw new InvalidDataException("房屋位置不合法");}
            if(save.ownedHomes!=null)foreach(var home in save.ownedHomes)Sandbox.ValidateSecretStorage(home);
            Sandbox.ValidateVillageReturns(save);
            Sandbox.ValidateInstalledCameras(save);
            Sandbox.ValidateHomeParty(save);
            if(save.armorSlots!=null){if(save.armorSlots.Length!=4)throw new InvalidDataException("装备数据不合法");for(int i=0;i<4;i++)if(save.armorSlots[i]!=0&&save.armorSlots[i]!=81+i&&save.armorSlots[i]!=86+i)throw new InvalidDataException("装备槽不匹配");}
            if(save.quickItems!=null&&save.quickItems.Length>0){if(save.quickItems.Length!=9)throw new InvalidDataException("快捷栏不合法");foreach(int id in save.quickItems)if(!Blocks.Selectable(id)||id==91||id==92)throw new InvalidDataException("快捷栏物品不合法");}
            if(save.invitationVersion>=2){var ids=new HashSet<int>();if(save.invitationDesigns==null||save.invitationDesigns.Count>128||save.madeInvitations==null||save.madeInvitations.Count>9999||save.inventory[93]!=save.madeInvitations.Count)throw new InvalidDataException("邀请函数量不合法");foreach(var d in save.invitationDesigns){if(d==null||d.id<1||!ids.Add(d.id)||d.drawing==null||d.drawing.Count>1000||d.guests<1||d.guests>8)throw new InvalidDataException("邀请函画稿不合法");foreach(var stroke in d.drawing)if(stroke==null||stroke.points==null||stroke.points.Count>3000)throw new InvalidDataException("邀请函笔画不合法");}foreach(int id in save.madeInvitations)if(!ids.Contains(id))throw new InvalidDataException("邀请函缺少画稿");}
            if(save.drawing!=null){if(save.drawing.Count>1000)throw new InvalidDataException("画纸笔画太多");foreach(var stroke in save.drawing){if(stroke==null||stroke.points==null||stroke.points.Count>3000)throw new InvalidDataException("画纸数据不合法");foreach(var p in stroke.points)if(!Finite(p.x)||!Finite(p.y)||p.x<0||p.x>1||p.y<0||p.y>1)throw new InvalidDataException("画纸坐标不合法");}}
            if(save.survivalVersion<0||save.survivalVersion>1||save.survivalVersion==1&&(!Finite(save.health)||save.health<0||save.health>10||!Finite(save.dragonHealth)||save.dragonHealth<0||save.dragonHealth>100))throw new InvalidDataException("生命数据不合法");
            if(save.deadAnimals!=null){var ids=new HashSet<int>();foreach(int id in save.deadAnimals)if(id<0||id>35||!ids.Add(id))throw new InvalidDataException("动物数据不合法");}
            if(!string.IsNullOrEmpty(save.preciseHealth)){if(save.preciseHealth.Length>40||!decimal.TryParse(save.preciseHealth,System.Globalization.NumberStyles.AllowDecimalPoint,System.Globalization.CultureInfo.InvariantCulture,out decimal exact)||exact<0||exact>10||Math.Abs((double)exact-save.health)>.0001)throw new InvalidDataException("精确生命不合法");}
            if(save.npcCombat==null)save.npcCombat=new List<NpcCombatState>();if(save.npcCombat.Count>512)throw new InvalidDataException("倒地记录过多");var combatKeys=new HashSet<string>();foreach(var n in save.npcCombat)if(n==null||n.region<0||n.region>=VoxelWorld.RegionNames.Length||string.IsNullOrEmpty(n.key)||n.key.Length>200||n.hits<0||n.hits>2||!combatKeys.Add(n.region+":"+n.key))throw new InvalidDataException("NPC战斗记录不合法");
            if(!Finite(save.position.x)||!Finite(save.position.y)||!Finite(save.position.z)||!Finite(save.yaw)||!Finite(save.pitch)||Mathf.Abs(save.pitch)>85||!Blocks.Selectable(save.selected))
                throw new InvalidDataException("存档位置不合法");
            foreach(int count in save.inventory)if(count<0||count>9999)throw new InvalidDataException("物品数量不合法");
            if(save.miningPickups==null)save.miningPickups=new List<MiningPickup>();if(save.miningPickups.Count>512)throw new InvalidDataException("掉落物过多");foreach(var d in save.miningPickups)if(d==null||d.region<0||d.region>=VoxelWorld.RegionNames.Length||d.id<1||d.id>=Blocks.Count||d.count<1||d.count>9999||!Finite(d.position.x)||!Finite(d.position.y)||!Finite(d.position.z))throw new InvalidDataException("掉落物不合法");ValidateVehicle(save.vehicle);
            ValidateEdits(save.edits);
            if(save.palaceSavings<0||save.palaceSavings>1000000)throw new InvalidDataException("金库金额不合法");
            if(save.fallenResidents!=null){var residentIds=new HashSet<int>();foreach(var r in save.fallenResidents)if(r==null||r.id<0||r.id>5||!residentIds.Add(r.id)||!Finite(r.position.x)||!Finite(r.position.y)||!Finite(r.position.z)||r.position.y>0||r.position.y<-1000)throw new InvalidDataException("地下居民数据不合法");}
            if(save.lostSkyResidents!=null){var ids=new HashSet<int>();foreach(int id in save.lostSkyResidents)if(id<0||id>=24||!ids.Add(id))throw new InvalidDataException("底部会合者数据不合法");}
            if(save.invitedChildren!=null){var ids=new HashSet<int>();foreach(int id in save.invitedChildren)if(id<0||id>6||!ids.Add(id))throw new InvalidDataException("受邀伙伴数据不合法");}
            if(save.planetHeavenVisitors!=null){var ids=new HashSet<int>();foreach(int id in save.planetHeavenVisitors)if(id<0||id>34||id>21&&id<32||!ids.Add(id))throw new InvalidDataException("天堂访客数据不合法");}
            if(save.stolen!=null){ValidateEdits(save.stolen);foreach(var s in save.stolen)if(!Sandbox.VillageOwned(new Vector3Int(s.x,s.y,s.z))||!Blocks.Buildable((Block)s.id))throw new InvalidDataException("归还记录不合法");}
            var regions=new HashSet<int>{save.region};
            if(save.regions!=null)foreach(var r in save.regions)
            {
                if(r==null||r.id<0||r.id>=VoxelWorld.RegionNames.Length||!regions.Add(r.id))throw new InvalidDataException("地区数据重复或不合法");
                ValidateEdits(r.edits);
            }
        }
        static void ValidateEdits(List<SavedBlock> edits)
        {
            if(edits==null||edits.Count>VoxelWorld.Width*VoxelWorld.VerticalSize*VoxelWorld.Depth)throw new InvalidDataException("地区方块数据过大");
            var seen=new HashSet<Vector3Int>();
            foreach(var b in edits)
            {
                if(b==null)throw new InvalidDataException("空方块数据");
                var p=new Vector3Int(b.x,b.y,b.z);
                if(!VoxelWorld.InBounds(p)||(p.y==0&&b.id!=0)||b.id<0||b.id>=Blocks.Count||!(b.id==0||b.id==10||b.id==(int)Block.Lava||Blocks.Buildable((Block)b.id))||!seen.Add(p))throw new InvalidDataException("方块数据不合法");
            }
        }
        static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
        public static void Write(SavedWorld save,string path)
        {
            Validate(save);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path+".tmp",JsonUtility.ToJson(save));
            if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");
            else File.Move(path+".tmp",path);
        }
        public static SavedWorld Read(string path)
        {
            if(new FileInfo(path).Length>96*1024*1024)throw new InvalidDataException("存档过大");
            var save=JsonUtility.FromJson<SavedWorld>(File.ReadAllText(path));Validate(save);
            if(save.inventory.Length<Blocks.Count)Array.Resize(ref save.inventory,Blocks.Count);
            return save;
        }
    }
}
