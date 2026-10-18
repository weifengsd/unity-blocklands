using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public static Vector3 SmallHousePartyDoor(OwnedHome h)=>new Vector3(h.x+5.3f,h.y+2.05f,h.z+1.1f);
    public static Vector3 SmallHousePartyLanding(OwnedHome h)=>new Vector3(h.x+5.3f,h.y+2.06f,h.z-3);
    public static bool InPartyHall(OwnedHome h,Vector3 p)=>p.x>h.partyX+1&&p.x<h.partyX+h.partyW-1&&p.z>h.partyZ+1&&p.z<h.partyZ+h.partyD-1&&Mathf.Abs(p.y-(h.y+2))<2;
    static bool OldHousePartyWalkway(OwnedHome h,Vector3Int p)=>p.y>=h.y+2&&p.y<=h.y+4&&p.x>=h.x+1&&p.x<=h.x+8&&p.z>=h.z+1&&p.z<=h.z+8&&((p.x>=h.x+4&&p.x<=h.x+6)||(h.partyDir>=2&&p.z>=h.z+4&&p.z<=h.z+6));
    bool PartyPropBlocksWalkway(OwnedHome h,HomePartyProp p){var size=PartyPropSize(p.kind);var bounds=new Bounds(p.position+Vector3.up*size.y/2,size);for(int x=h.x+1;x<=h.x+8;x++)for(int z=h.z+1;z<=h.z+8;z++)if(OldHousePartyWalkway(h,new Vector3Int(x,h.y+2,z))&&bounds.Intersects(new Bounds(new Vector3(x+.5f,h.y+3.5f,z+.5f),new Vector3(1,3,1))))return true;return false;}
    public bool PrepareSmallHousePartyRoute(OwnedHome h)
    {
        if(h==null||h.region!=World.Region||saveBlocked)return false;if(h.partyRouteVersion==1)return true;ValidateSecretStorage(h);var priorEdits=new Dictionary<Vector3Int,Block>(World.Edits);var priorStored=(int[])h.storedItems.Clone();var stock=(int[])PartyStock.Clone();var props=new List<HomePartyProp>(HomePartyProps);var cells=new List<Vector3Int>();int recovered=0,returned=0;
        for(int x=h.x+1;x<=h.x+8;x++)for(int z=h.z+1;z<=h.z+8;z++)for(int y=h.y+2;y<=h.y+4;y++){var p=new Vector3Int(x,y,z);if(!OldHousePartyWalkway(h,p))continue;Block old=World.Get(p);if(old==Block.Air)continue;if(!CabinetItemAllowed((int)old)||h.storedItems[(int)old]>=9999){h.storedItems=priorStored;Notice="迎宾通道中有不能收回的物品，暂不能开派对。";return false;}h.storedItems[(int)old]++;recovered++;cells.Add(p);}
        if(World.Get(new Vector3Int(h.x+4,h.y+2,h.z))!=Block.IronDoor)for(int x=h.x+4;x<=h.x+5;x++)for(int y=h.y+2;y<=h.y+4;y++){var p=new Vector3Int(x,y,h.z);Block old=World.Get(p);if(old==Block.Air)continue;if(!CabinetItemAllowed((int)old)||h.storedItems[(int)old]>=9999){h.storedItems=priorStored;return false;}h.storedItems[(int)old]++;recovered++;cells.Add(p);}
        foreach(var p in props)if(p.home==HomeKey(h)&&PartyPropBlocksWalkway(h,p)){if(PartyStock[p.kind]>=9999){h.storedItems=priorStored;Array.Copy(stock,PartyStock,stock.Length);HomePartyProps.Clear();HomePartyProps.AddRange(props);return false;}PartyStock[p.kind]++;HomePartyProps.Remove(p);returned++;}
        foreach(var p in cells)World.Edit(p,Block.Air,false);h.partyRouteVersion=1;if(!SavePartyState()){h.partyRouteVersion=0;h.storedItems=priorStored;Array.Copy(stock,PartyStock,stock.Length);HomePartyProps.Clear();HomePartyProps.AddRange(props);int region=World.Region;World.Generate(region);World.ApplyEdits(ToSavedEdits(priorEdits));World.Edits.Clear();foreach(var e in priorEdits)World.Edits[e.Key]=e.Value;World.RebuildAll();return false;}World.RebuildAll();Notice="迎宾通道已腾开："+recovered+"块旧家具/方块收入柜子，"+returned+"件派对用品回到书包。";return true;
    }
    void OpenSmallHouseForGuests(OwnedHome h){foreach(var f in World.GetComponentsInChildren<HomeFixture>())if(f.Kind==Block.IronDoor&&f.Cell==new Vector3Int(h.x+4,h.y+2,h.z)&&!f.State.Open)f.Toggle();}
    List<Vector3> BuildSmallHouseGuestRoute(OwnedHome h,Vector3 a,Vector3 b,Vector3 seat)
    {
        float y=h.y+2.05f;var route=new List<Vector3>{SmallHousePartyDoor(h),new Vector3(h.x+5.5f,y,h.z+4.5f)};
        if(h.partyDir==0)route.Add(new Vector3(h.x+5.5f,y,h.z+10.5f));else if(h.partyDir==1)route.Add(new Vector3(h.x+5.5f,y,h.z-1.5f));else if(h.partyDir==2)route.Add(new Vector3(h.x+10.5f,y,h.z+5.5f));else route.Add(new Vector3(h.x-1.5f,y,h.z+5.5f));route.Add(a);route.Add(b);route.Add(seat);return route;
    }
}}
