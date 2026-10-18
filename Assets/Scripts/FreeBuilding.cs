using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public Dictionary<Vector3Int,Block> HousePlanAt(int x,int y,int z)
    {
        var plan=new Dictionary<Vector3Int,Block>();foreach(var e in World.PlanHouse(x,z))plan[e.Key+Vector3Int.up*(y-14)]=e.Value;for(int step=0;step<4;step++)for(int dx=4;dx<=5;dx++)plan[new Vector3Int(x+dx,y-2+step,z-4+step)]=Block.StoneBricks;return plan;
    }
    public bool BuildHouseHere()
    {
        Vector3 centre=Player.transform.position+Player.transform.forward*11;int x=Mathf.FloorToInt(centre.x)-5,z=Mathf.FloorToInt(centre.z)-5;
        if(InVoidRealms)return BuildRealmHouseHere(x,z);
        int y=0;for(int a=x;a<x+10;a++)for(int b=z;b<z+10;b++){int height=-1;for(int h=Mathf.Min(65,Mathf.FloorToInt(Player.transform.position.y)+4);h>=0;h--)if(Blocks.Solid(World.Get(new Vector3Int(a,h,b)))){height=h;break;}y=Mathf.Max(y,height);}
        return BuildHouseAt(x,y,z);
    }
    public bool BuildHouseAt(int x,int y,int z)
    {
        if(y<3||y>65||x<1||z<5||x+10>=VoxelWorld.Width||z+10>=VoxelWorld.Depth||InVoidRealms){Notice="请选择世界边界内、可采掘陆地上的空地。";return false;}
        var plan=HousePlanAt(x,y,z);var cost=new Dictionary<Block,int>();var old=new Dictionary<Vector3Int,Block>();
        foreach(var e in plan){if(World.Region==1&&VoxelWorld.VillageRenewable(e.Key)||World.Region==6&&VoxelWorld.CastleRenewable(e.Key)){Notice="这里是公共建筑，请在旁边选一块空地。";return false;}if(World.Get(e.Key)!=Block.Air&&e.Key.y>y){Notice="上方有树木或建筑，先清理空地再建房。";return false;}if(World.Edits.ContainsKey(e.Key)){Notice="这里已有你的方块，请在旁边建新房。";return false;}old[e.Key]=World.Get(e.Key);if(e.Value!=Block.Air){cost.TryGetValue(e.Value,out int n);cost[e.Value]=n+1;}}
        var bounds=new Bounds(new Vector3(x+5,y+5,z+3),new Vector3(10,10,14));if(bounds.Intersects(Player.Body.bounds)){Notice="请站到预览地块外再建房。";return false;}
        foreach(var c in cost)if(Inventory[(int)c.Key]<c.Value){Notice="材料不足："+Blocks.Names[(int)c.Key]+"需要"+c.Value+"。E背包查看九宫格配方。";return false;}
        foreach(var hit in Physics.OverlapBox(bounds.center,bounds.extents,Quaternion.identity,~(1<<2),QueryTriggerInteraction.Ignore))if(hit.GetComponentInParent<VoxelWorld>()==null){Notice="地块里有居民或设施，请换一块空地。";return false;}
        var priorEdits=new Dictionary<Vector3Int,Block>(World.Edits);foreach(var e in plan)World.Edit(e.Key,e.Value,false);foreach(var c in cost)Inventory[(int)c.Key]-=c.Value;var home=new OwnedHome{region=World.Region,x=x,y=y,z=z};OwnedHomes.Add(home);World.RebuildAll();
        try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception err){OwnedHomes.Remove(home);foreach(var e in old)World.Edit(e.Key,e.Value,false);World.Edits.Clear();foreach(var e in priorEdits)World.Edits[e.Key]=e.Value;foreach(var c in cost)Inventory[(int)c.Key]+=c.Value;World.RebuildAll();Notice="建造未保存，已恢复材料："+err.Message;return false;}
        CompanionUnlocked=true;CompanionEnabled=true;RestoreCompanionForHouse();Notice="新家建好了！可以继续挖房屋下面的地面，也可以在工坊修地下密室。";Save();return true;
    }
    public OwnedHome NearestOwnedHome()
    {
        RegisterLegacyVillageHome();
        OwnedHome best=null;float distance=24;foreach(var h in OwnedHomes)if(h.realm&&InVoidRealms||!h.realm&&!InVoidRealms&&h.region==World.Region){float d=Vector2.Distance(new Vector2(Player.transform.position.x,Player.transform.position.z),new Vector2(h.x+5,h.z+5));if(d<distance){distance=d;best=h;}}return best;
    }
    public bool AddHomeBasement(OwnedHome chosen=null,bool complimentary=false)
    {
        if(saveBlocked){Notice="存档读取失败，暂不能扩建；请先检查原存档，避免覆盖进度。";return false;}
        var h=chosen??NearestOwnedHome();if(h==null||!OwnedHomes.Contains(h)||h.region!=World.Region){Notice="这里尚未登记房屋。可点“改造当前木屋”或“免费创建完整基地”。";return false;}if(h.realm){Notice="浮岛与星球的小屋可以住和布置；地下采掘密室要在上方方块陆地建。";return false;}if(h.secretRoomVersion>=1){BuildSecretRooms();LocateSecretRoom(h);Notice="10×10密室已建好，入口在屋内前左侧。可点“带我到密室入口”；保险箱、柜子、秋千和监控都在里面。";return false;}if(h.y<7||h.x<2||h.z<2||h.x+11>=VoxelWorld.Width||h.z+11>=VoxelWorld.Depth){Notice="扩建需要足够深的地基和房屋四周一格空地。";return false;}
        int cost=complimentary||h.legacy?0:h.basement?48:144;if(Inventory[(int)Block.StoneBricks]<cost){Notice="宽敞密室需要"+cost+"石砖；E打开配方本合成。";return false;}
        var before=new Dictionary<Vector3Int,Block>();var changes=new Dictionary<Vector3Int,Block>();
        for(int x=-1;x<=10;x++)for(int z=-1;z<=10;z++)for(int y=-6;y<0;y++){bool wall=x==-1||x==10||z==-1||z==10||y==-6;var p=new Vector3Int(h.x+x,h.y+y,h.z+z);changes[p]=wall?Block.StoneBricks:Block.Air;}
        for(int step=0;step<7;step++)for(int x=2;x<=3;x++){int floor=h.y-step;changes[new Vector3Int(h.x+x,floor,h.z+2+step)]=Block.StoneBricks;for(int y=1;y<=4;y++)changes[new Vector3Int(h.x+x,floor+y,h.z+2+step)]=Block.Air;}
        var recovered=new int[Blocks.Count];foreach(var e in changes){if(World.Region==1&&VoxelWorld.VillageRenewable(e.Key)||World.Region==6&&VoxelWorld.CastleRenewable(e.Key)){Notice="扩建范围碰到了公共建筑，请另选你自己的房屋。";return false;}if(World.Edits.TryGetValue(e.Key,out var edited)&&edited!=Block.Air&&edited!=Block.StoneBricks&&e.Key.y<h.y){if(complimentary&&e.Value==Block.Air&&CabinetItemAllowed((int)edited))recovered[(int)edited]++;else{Notice="扩建边界里有你放置的物品，请先收好，避免覆盖。";return false;}}if(new Bounds((Vector3)e.Key+Vector3.one*.5f,Vector3.one).Intersects(Player.Body.bounds)&&e.Value!=Block.Air){Notice="先离开楼梯入口，再修密室。";return false;}before[e.Key]=World.Get(e.Key);}
        var prior=new Dictionary<Vector3Int,Block>(World.Edits);bool wasBasement=h.basement;var priorStored=h.storedItems==null?null:(int[])h.storedItems.Clone();foreach(var other in OwnedHomes)if(other!=h&&!other.realm&&other.region==h.region&&other.basement&&new Bounds(new Vector3(other.x+5,other.y-3,other.z+5),new Vector3(12,6,12)).Intersects(new Bounds(new Vector3(h.x+5,h.y-3,h.z+5),new Vector3(12,6,12)))){Notice="扩建会碰到另一座自建房的地下室，请先换一座有空地的房屋。";return false;}if(h.storedItems==null)h.storedItems=new int[Blocks.Count];int recoveredCount=0;for(int id=1;id<Blocks.Count;id++){if(recovered[id]>9999-h.storedItems[id]){Notice="改造要回收的物品超过柜子容量，请先取走一部分。";h.storedItems=priorStored;return false;}recoveredCount+=recovered[id];}foreach(var e in changes)World.Edit(e.Key,e.Value,false);Inventory[(int)Block.StoneBricks]-=cost;h.basement=true;h.secretRoomVersion=1;for(int id=1;id<Blocks.Count;id++)h.storedItems[id]+=recovered[id];World.RebuildAll();
        try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception){foreach(var e in before)World.Edit(e.Key,e.Value,false);World.Edits.Clear();foreach(var e in prior)World.Edits[e.Key]=e.Value;Inventory[(int)Block.StoneBricks]+=cost;h.basement=wasBasement;h.secretRoomVersion=0;h.storedItems=priorStored;World.RebuildAll();Notice="密室未保存，地形和材料已恢复。";return false;}
        BuildSecretRooms();BuildTransparentLegacyHomes();LocateSecretRoom(h);Notice="10×10地下密室建好了：入口在屋内前左侧。H设密码；里面有保险箱、柜子、秋千和监控。"+(recoveredCount>0?"施工移开的"+recoveredCount+"件旧摆放已收入储物柜。":"");return true;
    }
    public bool BuildRealmHouseHere(int x,int z)
    {
        float floor=InHeaven?HeavenFloor+1:InPlanetLife?PlanetFloor+1:HellFloor+1;float radius=InHeaven?120:InPlanetLife?PlanetLandDiameter/2:80;
        if(Vector2.Distance(new Vector2(x+5,z+5),new Vector2(32,32))>radius-12){Notice="请在岛屿或星球的陆地范围内选址。";return false;}
        var bounds=new Bounds(new Vector3(x+5,floor+2.4f,z+5),new Vector3(12,4.6f,12));if(bounds.Intersects(Player.Body.bounds)){Notice="先站到房屋地块外，再确认建造。";return false;}
        foreach(var c in Physics.OverlapBox(bounds.center,bounds.extents,Quaternion.identity,~(1<<2),QueryTriggerInteraction.Ignore))if(c.bounds.max.y>floor+.3f){Notice="这里已有居民或房屋，请选一块空地。";return false;}
        if(Inventory[(int)Block.Planks]<100||Inventory[(int)Block.StoneBricks]<24||Inventory[(int)Block.Glass]<6){Notice="小屋需要100木板、24石砖、6透明水晶，先去背包合成和收集。";return false;}
        var h=new OwnedHome{realm=true,region=World.Region,x=x,y=Mathf.RoundToInt(floor),z=z};OwnedHomes.Add(h);Inventory[(int)Block.Planks]-=100;Inventory[(int)Block.StoneBricks]-=24;Inventory[(int)Block.Glass]-=6;
        try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception err){OwnedHomes.Remove(h);Inventory[(int)Block.Planks]+=100;Inventory[(int)Block.StoneBricks]+=24;Inventory[(int)Block.Glass]+=6;Notice="建造未保存，材料已退回："+err.Message;return false;}
        BuildRealmHome(villagePeople.transform,new Vector3(x+5,floor,z+5),"我建造的奇境小屋",new Color(.92f,.88f,.76f),new Color(.72f,.61f,.84f));Notice="你的小屋建好了，G开门、吃点心、看电视、坐下或躺下。换地区后仍保留。";return true;
    }
    void BuildSavedRealmHomes(Transform root){foreach(var h in OwnedHomes)if(h.realm)BuildRealmHome(root,new Vector3(h.x+5,h.y,h.z+5),"我建造的奇境小屋",new Color(.92f,.88f,.76f),new Color(.72f,.61f,.84f));}
}}
