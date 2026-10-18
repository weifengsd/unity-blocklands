using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public bool TryPartyHouseB(){if(Paused||CompanionReturnBlocked||PhoneOpen||StickerOpen||!RayFromView(out var hit,8))return false;var p=hit.point;foreach(var h in OwnedHomes)if(!h.realm&&h.region==World.Region&&p.x>=h.x-.1f&&p.x<=h.x+10.1f&&p.z>=h.z-.1f&&p.z<=h.z+10.1f&&p.y>=h.y&&p.y<=h.y+11)return ExpandPartyHouse(h);return false;}
    public bool ExpandPartyHouse(OwnedHome home=null)
    {
        var h=home??FindSecretHome();if(h==null||h.realm||h.region!=World.Region||saveBlocked||CompanionReturnBlocked){Notice="请先看着自己的地面木屋，按B扩建。";return false;}if(h.partyHallVersion==1){partyHome=HomeKey(h);Notice="这座家已经扩成派对厅；E打开派对用品，N天黑让宾客来。";return true;}
        ValidateSecretStorage(h);Dictionary<Vector3Int,Block> chosen=null;int hx=0,hz=0,hw=0,hd=0,direction=0;
        for(int dir=0;dir<4;dir++){int x=dir<2?h.x-4:dir==2?h.x+10:h.x-16,z=dir==0?h.z+10:dir==1?h.z-16:h.z-4,w=dir<2?18:16,d=dir<2?16:18;if(x<4||z<4||x+w>VoxelWorld.Width-4||z+d>VoxelWorld.Depth-4)continue;var plan=PartyHallPlan(h,x,z,w,d,dir);bool clear=true;foreach(var e in plan){if(World.Region==1&&VoxelWorld.VillageRenewable(e.Key)||World.Region==6&&VoxelWorld.CastleRenewable(e.Key)){clear=false;break;}bool original=e.Key.x>=h.x&&e.Key.x<h.x+10&&e.Key.z>=h.z&&e.Key.z<h.z+10;if(!original&&World.Edits.TryGetValue(e.Key,out var old)&&old!=Block.Air){clear=false;break;}if(!original&&e.Key.y>h.y+1&&World.Get(e.Key)!=Block.Air){clear=false;break;}}foreach(var other in OwnedHomes)if(other!=h&&!other.realm&&other.region==h.region&&other.x<x+w&&other.x+10>x&&other.z<z+d&&other.z+10>z)clear=false;if(!clear)continue;chosen=plan;hx=x;hz=z;hw=w;hd=d;direction=dir;break;}
        if(chosen==null){Notice="四周没有足够的安全空地。先清理自家周围的树木或方块，再按B；不会覆盖公共房屋。";return false;}var before=new Dictionary<Vector3Int,Block>(World.Edits);var stored=(int[])h.storedItems.Clone();string priorHome=partyHome;foreach(var e in chosen){Block old=World.Get(e.Key);if(old!=e.Value&&old!=Block.Air&&e.Key.x>=h.x&&e.Key.x<h.x+10&&e.Key.z>=h.z&&e.Key.z<h.z+10&&CabinetItemAllowed((int)old)){if(h.storedItems[(int)old]>=9999){h.storedItems=stored;Notice="柜子放不下连接通道收回的旧家具。";return false;}h.storedItems[(int)old]++;}}
        foreach(var e in chosen)World.Edit(e.Key,e.Value,false);h.partyHallVersion=1;h.partyX=hx;h.partyZ=hz;h.partyW=hw;h.partyD=hd;h.partyDir=direction;partyHome=HomeKey(h);
        if(!SavePartyState()){h.partyHallVersion=0;h.storedItems=stored;partyHome=priorHome;int region=World.Region;World.Generate(region);World.ApplyEdits(ToSavedEdits(before));World.Edits.Clear();foreach(var e in before)World.Edits[e.Key]=e.Value;World.RebuildAll();return false;}World.RebuildAll();BuildHomeParty();Notice="你的家变大了！新增"+hw+"×"+hd+"派对厅，连接处收回的家具存入柜子。原密室与保险箱不动。";return true;
    }
    Dictionary<Vector3Int,Block> PartyHallPlan(OwnedHome h,int x,int z,int w,int d,int direction)
    {
        var plan=new Dictionary<Vector3Int,Block>();for(int dx=0;dx<w;dx++)for(int dz=0;dz<d;dz++)for(int y=h.y+1;y<=h.y+8;y++){bool wall=dx==0||dz==0||dx==w-1||dz==d-1;bool doorway=direction==0?dz==d-1&&Mathf.Abs(dx-w/2)<=1:direction==1?dz==0&&Mathf.Abs(dx-w/2)<=1:direction==2?dx==w-1&&Mathf.Abs(dz-d/2)<=1:dx==0&&Mathf.Abs(dz-d/2)<=1;Block b=y==h.y+1?Block.Planks:y==h.y+8?Block.Brick:wall&&!(doorway&&y<=h.y+4)?y>=h.y+4&&y<=h.y+5&&(dx%4==0||dz%4==0)?Block.Glass:Block.Planks:Block.Air;plan[new Vector3Int(x+dx,y,z+dz)]=b;}
        for(int side=-1;side<=1;side++)for(int y=h.y+2;y<=h.y+4;y++){if(direction<2){int zz=direction==0?h.z+9:h.z;for(int k=-1;k<=1;k++)plan[new Vector3Int(h.x+5+side,y,zz+k)]=Block.Air;}else{int xx=direction==2?h.x+9:h.x;for(int k=-1;k<=1;k++)plan[new Vector3Int(xx+k,y,h.z+5+side)]=Block.Air;}}
        return plan;
    }
    public static Vector3 PartyOutward(OwnedHome h)=>h.partyDir==0?Vector3.forward:h.partyDir==1?Vector3.back:h.partyDir==2?Vector3.right:Vector3.left;
    public static Vector3 PartyDoor(OwnedHome h)=>h.partyDir==0?new Vector3(h.partyX+h.partyW/2f,h.y+2.05f,h.partyZ+h.partyD-1.3f):h.partyDir==1?new Vector3(h.partyX+h.partyW/2f,h.y+2.05f,h.partyZ+1.3f):h.partyDir==2?new Vector3(h.partyX+h.partyW-1.3f,h.y+2.05f,h.partyZ+h.partyD/2f):new Vector3(h.partyX+1.3f,h.y+2.05f,h.partyZ+h.partyD/2f);
}}
