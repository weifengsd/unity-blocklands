using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    [Serializable] public class VillagePlacedBlock {public int x,y,z,id,previous;}
    public partial class Sandbox
    {
        public readonly Dictionary<Vector3Int,VillagePlacedBlock> VillagePlacements=new Dictionary<Vector3Int,VillagePlacedBlock>();
        public readonly int[] VillageRefunds=new int[Blocks.Count];float nextRefundClaim;
        public void RecordVillagePlacement(Vector3Int cell,Block block,Block previous){if(World.Region!=1||(block!=Block.Stone&&block!=Block.Dirt))return;VillagePlacements[cell]=new VillagePlacedBlock{x=cell.x,y=cell.y,z=cell.z,id=(int)block,previous=(int)previous};}
        public void ForgetVillagePlacement(Vector3Int cell){if(World.Region==1)VillagePlacements.Remove(cell);}
        public List<VillagePlacedBlock> CaptureVillagePlacements(){var list=new List<VillagePlacedBlock>();foreach(var p in VillagePlacements.Values)list.Add(new VillagePlacedBlock{x=p.x,y=p.y,z=p.z,id=p.id,previous=p.previous});return list;}
        public void RestoreVillageReturns(SavedWorld save){VillagePlacements.Clear();if(save.villagePlacements!=null)foreach(var p in save.villagePlacements)VillagePlacements[new Vector3Int(p.x,p.y,p.z)]=p;Array.Clear(VillageRefunds,0,VillageRefunds.Length);if(save.villageRefunds!=null&&save.villageRefunds.Length==Blocks.Count)Array.Copy(save.villageRefunds,VillageRefunds,Blocks.Count);}
        public static int PrepareVillageReturn(SavedWorld save)
        {
            if(save.villageRefunds==null||save.villageRefunds.Length==0)save.villageRefunds=new int[Blocks.Count];else if(save.villageRefunds.Length==94)Array.Resize(ref save.villageRefunds,Blocks.Count);if(save.villagePlacements==null)save.villagePlacements=new List<VillagePlacedBlock>();
            List<SavedBlock> edits=save.region==1?save.edits:null;if(edits==null&&save.regions!=null)foreach(var r in save.regions)if(r.id==1){edits=r.edits;break;}
            var records=new Dictionary<Vector3Int,VillagePlacedBlock>();foreach(var p in save.villagePlacements)records[new Vector3Int(p.x,p.y,p.z)]=p;int returned=0;
            if(edits!=null)foreach(var e in edits)if(e.id==(int)Block.Stone||e.id==(int)Block.Dirt){var cell=new Vector3Int(e.x,e.y,e.z);int item=e.id;int old=records.TryGetValue(cell,out var p)&&p.id==item?p.previous:(int)Block.Air;e.id=old;save.villageRefunds[item]++;returned++;}
            if(edits!=null)edits.RemoveAll(e=>(VoxelWorld.VillageRenewable(new Vector3Int(e.x,e.y,e.z))||e.y==14&&e.id==0)&&!PrivateVillageCell(new Vector3Int(e.x,e.y,e.z),save.ownedHomes,save.houseBase!=null&&save.houseBase.Count>0));
            save.villagePlacements.Clear();for(int id=2;id<=3;id++){int count=Mathf.Min(save.villageRefunds[id],9999-save.inventory[id]);save.inventory[id]+=count;save.villageRefunds[id]-=count;}return returned;
        }
        static bool PrivateVillageCell(Vector3Int p,List<OwnedHome> homes,bool legacy){if(legacy&&p.x>=28&&p.x<38&&p.z>=50&&p.z<60&&p.y>=8&&p.y<24)return true;if(homes!=null)foreach(var h in homes)if(h.region==1&&!h.realm&&h.basement&&p.x>=h.x-1&&p.x<=h.x+10&&p.z>=h.z-1&&p.z<=h.z+10&&p.y>=h.y-(h.deepBaseVersion==1?70:6)&&p.y<=h.y+4)return true;return false;}
        bool PreserveMyVillageCell(Vector3Int p)=>PrivateVillageCell(p,OwnedHomes,HouseBase.Count>0)||PrivatePartyHall(p,OwnedHomes);
        static bool PrivatePartyHall(Vector3Int p,List<OwnedHome> homes){if(homes!=null)foreach(var h in homes)if(h.region==1&&h.partyHallVersion==1&&p.x>=h.partyX&&p.x<h.partyX+h.partyW&&p.z>=h.partyZ&&p.z<h.partyZ+h.partyD&&p.y>=h.y+1&&p.y<=h.y+8)return true;return false;}
        public static void ValidateVillageReturns(SavedWorld save)
        {
            if(save.villageRefunds==null||save.villageRefunds.Length==0)save.villageRefunds=new int[Blocks.Count];else if(save.villageRefunds.Length==94)Array.Resize(ref save.villageRefunds,Blocks.Count);if(save.villageRefunds.Length!=Blocks.Count)throw new System.IO.InvalidDataException("村庄回收袋格式不合法");for(int id=0;id<Blocks.Count;id++)if(save.villageRefunds[id]<0||save.villageRefunds[id]>2000000||save.villageRefunds[id]>0&&id!=2&&id!=3)throw new System.IO.InvalidDataException("村庄回收数量不合法");
            if(save.villagePlacements!=null){var seen=new HashSet<Vector3Int>();foreach(var p in save.villagePlacements){if(p==null||!VoxelWorld.InBounds(new Vector3Int(p.x,p.y,p.z))||!seen.Add(new Vector3Int(p.x,p.y,p.z))||(p.id!=2&&p.id!=3)||(p.previous!=0&&p.previous!=10))throw new System.IO.InvalidDataException("村庄临时方块记录不合法");}}
        }
        void TickVillageRefundClaims(){if(saveBlocked||Time.unscaledTime<nextRefundClaim)return;int stone=Inventory[3],dirt=Inventory[2],oldStone=VillageRefunds[3],oldDirt=VillageRefunds[2];bool changed=false;for(int id=2;id<=3;id++){int n=Mathf.Min(VillageRefunds[id],9999-Inventory[id]);if(n>0){Inventory[id]+=n;VillageRefunds[id]-=n;changed=true;}}if(!changed)return;nextRefundClaim=Time.unscaledTime+5;try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);Notice="回收袋里的石头和泥土已补回背包。";}catch(Exception){Inventory[3]=stone;Inventory[2]=dirt;VillageRefunds[3]=oldStone;VillageRefunds[2]=oldDirt;Notice="回收物品尚未保存，仍保留在回收袋。";}}
    }
}
