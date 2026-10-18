using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    [Serializable] public class MiningPickup {public int region,id,count;public Vector3 position;}
    public class MiningPickupView:MonoBehaviour {public Sandbox Game;public MiningPickup Data;float age;void Update(){if(Game==null||Game.Paused)return;age+=Time.deltaTime;transform.position=Data.position+Vector3.up*(.12f+Mathf.Max(0,.7f-age*1.4f)+Mathf.Abs(Mathf.Sin(age*2))*.08f);transform.Rotate(0,30*Time.deltaTime,0);Game.CollectMiningPickup(Data);}}
    public partial class Sandbox
    {
        public readonly List<MiningPickup> MiningPickups=new List<MiningPickup>();Transform pickupRoot;
        public bool MineWithHeldTool(Vector3Int cell)
        {
            if(Paused||MiningPickups.Count>=480||!CanMineSelected){Notice="食物不能挖矿；请换工具、武器或方块。";return false;}
            int region=World.Region;int[] before=(int[])Inventory.Clone();if(!MineBlock(cell))return false;
            int extra=EmptyHands?0:Selected==75?8:Selected==35||Selected==64||Selected==85?2:0;
            foreach(var delta in new[]{Vector3Int.right,Vector3Int.left,Vector3Int.up,Vector3Int.down,new Vector3Int(1,1,0),new Vector3Int(-1,1,0),new Vector3Int(1,-1,0),new Vector3Int(-1,-1,0)})
            {if(extra<=0||World.Region!=region)break;Block b=World.Get(cell+delta);if(b==Block.Stone||b==Block.Dirt||b==Block.Sand||b==Block.Wood||b==Block.Planks||b==Block.Grass||b==Block.StoneBricks)if(MineBlock(cell+delta))extra--;}
            for(int i=1;i<Inventory.Length;i++){int count=Inventory[i]-before[i];if(count<=0)continue;Inventory[i]=before[i];MiningPickups.Add(new MiningPickup{region=region,id=i,count=count,position=(Vector3)cell+new Vector3(.5f,.3f,.5f)});}
            BuildMiningPickups();Notice="碎块落在地上了；走到1.8米内自动拾取。";return true;
        }
        public bool CollectMiningPickup(MiningPickup data)
        {
            if(data==null||Paused||data.region!=World.Region||!MiningPickups.Contains(data)||Vector3.Distance(Player.transform.position+Vector3.up*.6f,data.position)>1.8f)return false;
            int take=Mathf.Min(data.count,9999-Inventory[data.id]);if(take<=0)return false;Inventory[data.id]+=take;data.count-=take;if(data.count==0){MiningPickups.Remove(data);if(pickupRoot!=null)foreach(var v in pickupRoot.GetComponentsInChildren<MiningPickupView>())if(v.Data==data){v.gameObject.SetActive(false);Destroy(v.gameObject);}}
            Notice="捡到"+Blocks.Names[data.id]+" × "+take+"。";return true;
        }
        void BuildMiningPickups(){if(pickupRoot!=null){pickupRoot.gameObject.SetActive(false);Destroy(pickupRoot.gameObject);}pickupRoot=new GameObject("可拾取方块碎块").transform;foreach(var d in MiningPickups)if(d.region==World.Region){var t=new GameObject(Blocks.Names[d.id]+"碎块×"+d.count).transform;t.SetParent(pickupRoot,false);t.position=d.position;var art=t.gameObject.AddComponent<RoyalArt>();for(int j=0;j<3;j++){var b=art.Piece("落下的碎块",new Vector3((j-1)*.17f,j%2*.12f,0),Vector3.one*.19f,Blocks.ColorOf((Block)d.id));b.localRotation=Quaternion.Euler(j*25,30,10);}var v=t.gameObject.AddComponent<MiningPickupView>();v.Game=this;v.Data=d;}}
    }
}
