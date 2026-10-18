using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public partial class Sandbox
    {
        readonly List<SavedBlock> theft=new List<SavedBlock>();public List<SavedBlock> Theft=>theft;
        public void AlertVillage(){if(World.Region==1)VillageAngryUntil=Time.time+90;}
        public void Mutter(){var clip=Resources.Load<AudioClip>("VillagerMutter");if(clip!=null)audioSource.PlayOneShot(clip,.8f);Notice="村民退到最远那户，低声嘀咕：我怎么样才能抓到你啊？";}
        public bool ReturnStolen()
        {
            if(World.Region!=1||theft.Count==0){Notice="没有可归还的刚偷物品。";return false;}
            var e=theft[theft.Count-1];var p=new Vector3Int(e.x,e.y,e.z);if(Inventory[e.id]<1){Notice="归还需要1个"+Blocks.Names[e.id]+"；请先补齐。";return false;}
            if(World.Get(p)!=Block.Air){Notice="物品原来的位置被占用，先清空那里再归还。";return false;}
            if(new Bounds((Vector3)p+Vector3.one*.5f,Vector3.one).Intersects(Player.Body.bounds)){Notice="先离开物品原来的位置，再按Y归还，避免卡进方块。";return false;}
            float alarm=VillageAngryUntil;if(!World.Edit(p,(Block)e.id))return false;World.Edits.Remove(p);Inventory[e.id]--;theft.RemoveAt(theft.Count-1);if(theft.Count==0)VillageAngryUntil=0;
            try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception err){World.Edit(p,Block.Air);Inventory[e.id]++;theft.Add(e);VillageAngryUntil=alarm;Notice="保存失败，已撤销归还："+err.Message;return false;}
            Notice=theft.Count==0?"物品已归还，村民原谅你，追赶结束。":"已归还一件，再按Y归还剩余"+theft.Count+"件。";return true;
        }
        void LifeKeys(){if(Input.GetKeyDown(KeyCode.Y))ReturnStolen();if(Input.GetKeyDown(KeyCode.V))OpenHouseMenu();}
    }
}
