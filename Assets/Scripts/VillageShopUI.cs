using UnityEngine;
using System.Collections.Generic;
namespace Blocklands
{
    public partial class Sandbox
    {
        VillageControl villageMenu;Vector2 shopScroll;
        public struct Offer{public Block Item;public int Price,Amount;public Offer(Block b,int p,int n=1){Item=b;Price=p;Amount=n;}}
        static readonly Offer[][] Featured={
            new[]{new Offer(Block.Beef,2),new Offer(Block.Mutton,2),new Offer(Block.Pork,2),new Offer(Block.Bread,1),new Offer(Block.Cookie,1,3),new Offer(Block.Apple,1)},
            new[]{new Offer(Block.Table,3),new Offer(Block.Chair,2),new Offer(Block.Cabinet,4),new Offer(Block.Workbench,3)},
            new[]{new Offer(Block.Bed,5),new Offer(Block.Sofa,5),new Offer(Block.Bookshelf,3),new Offer(Block.CrystalLamp,4)},
            new[]{new Offer(Block.IronSword,5),new Offer(Block.IronPickaxe,5),new Offer(Block.Bucket,3),new Offer(Block.Torch,2)},
            new[]{new Offer(Block.Planks,1,16),new Offer(Block.Wood,1,8),new Offer(Block.StoneBricks,1,8),new Offer(Block.Glass,2,8),new Offer(Block.Table,3),new Offer(Block.CrystalHammer,12)},
            new[]{new Offer(Block.CrystalHammer,12),new Offer(Block.CastleKey,6),new Offer(Blocks.IronDrill,20),new Offer(Blocks.StarDrill,35),new Offer(Block.IronSword,5),new Offer(Block.CrystalPickaxe,5)}
        };
        public static readonly Offer[][] Offers=CreateCatalog();
        static Offer[][] CreateCatalog(){var all=new Offer[SkyHouse.ShopFloors][];for(int f=0;f<SkyHouse.ShopFloors;f++){var row=new List<Offer>(Featured[f%Featured.Length]);var seen=new HashSet<Block>();foreach(var o in row)seen.Add(o.Item);for(int k=0;row.Count<24&&k<Blocks.BagItems.Length;k++){Block b=(Block)Blocks.BagItems[(k+f*7)%Blocks.BagItems.Length];if(b==Block.Coin||b==Block.LavaBucket||b==Block.PrisonBars||!seen.Add(b))continue;int price=b==Block.CrystalHammer?12:b==Block.CastleKey?6:Blocks.Furniture(b)?4:Blocks.Tool(b)?5:Blocks.FoodHeal(b)>0?2:1;int qty=b==Block.Planks?16:b==Block.Wood?8:1;row.Add(new Offer(b,price,qty));}all[f]=row.ToArray();}return all;}
        public bool BuyAt(SkyHouse lift,int level,int index)
        {
            if(lift==null||level<0||level>=SkyHouse.ShopFloors||index<0||index>=Offers[level].Length||World.Region!=1||Time.time<VillageAngryUntil||!lift.AtShop(level,Player.transform.position)){Notice="到达对应楼层的小卖部，再购买；追赶时暂停交易。";return false;}
            var o=Offers[level][index];int cash=(int)Block.Coin,id=(int)o.Item;
            if(Inventory[cash]<o.Price||Inventory[id]>9999-o.Amount){Notice="钱币不足，或背包已满；没有扣费。";return false;}
            Inventory[cash]-=o.Price;Inventory[id]+=o.Amount;
            try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception e){Inventory[cash]+=o.Price;Inventory[id]-=o.Amount;Notice="保存失败，已撤销交易："+e.Message;return false;}
            Notice="买到"+Blocks.Names[id]+" × "+o.Amount+"，花费"+o.Price+"元。V选家具，右键摆放。";return true;
        }
        bool TryVillageControl()
        {
            if(!RayFromView(out var hit,5,QueryTriggerInteraction.Ignore))return false;
            var c=hit.collider.GetComponent<VillageControl>();if(c==null||!c.Shop)return false;
            if(Time.time<VillageAngryUntil){Notice="先归还偷走的物品，才能使用公共电梯和商店。";return true;}
            villageMenu=c;shopScroll=Vector2.zero;inventoryOpen=travelOpen=false;SetPaused(true);return true;
        }
        static bool LiftMenuButton(Rect r,string text,bool selected){Panel(r,selected?new Color(1,.82f,.16f):new Color(.84f,.85f,.79f));var s=new GUIStyle(GUI.skin.button){fontSize=15};s.normal.background=s.hover.background=s.active.background=null;s.normal.textColor=s.hover.textColor=s.active.textColor=Ink;return GUI.Button(r,text,s);}
        void DrawVillageMenu(float w,float h)
        {
            float x=w/2-320,y=h/2-240;Panel(new Rect(x,y,640,480),Paper);var c=villageMenu;
            Label(new Rect(x+24,y+20,592,36),c.Shop?"楼层小卖部 · 24种商品 · 钱币购物":"透明电梯 · 先选中，再按一次确认",23,Ink);
            Label(new Rect(x+24,y+65,592,28),"钱包（元）："+Inventory[(int)Block.Coin]+"  | 最近停靠："+SkyHouse.FloorLabel(c.Lift.Floor),16,Muted);
            if(c.Shop){shopScroll=GUI.BeginScrollView(new Rect(x+24,y+105,592,240),shopScroll,new Rect(0,0,566,528));for(int i=0;i<Offers[c.Floor].Length;i++){var o=Offers[c.Floor][i];if(LiftMenuButton(new Rect(i%2*283,i/2*44,275,38),Blocks.Names[(int)o.Item]+" × "+o.Amount+"  —  "+o.Price+" 元",false))BuyAt(c.Lift,c.Floor,i);}GUI.EndScrollView();}
            var style=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};style.normal.textColor=Ink;GUI.Label(new Rect(x+24,y+355,592,62),Notice,style);
            if(LiftMenuButton(new Rect(x+24,y+428,592,32),"关闭 · Esc",false))SetPaused(false);
        }
    }
}
