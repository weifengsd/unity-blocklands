using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public partial class Sandbox
    {
        bool houseMenu;Vector2 houseScroll;public static readonly Vector2Int NeighborPlot=new Vector2Int(28,50);
        public Dictionary<Block,int> HouseCost(int x,int z){var cost=new Dictionary<Block,int>();foreach(var e in World.PlanHouse(x,z))if(e.Value!=Block.Air){if(!cost.ContainsKey(e.Value))cost[e.Value]=0;cost[e.Value]++;}return cost;}
        public bool BuildNeighborHouse(int x,int z,bool complimentary=false)
        {
            if(World.Region!=1){Notice="先T→2到村庄，再用V搭建同款村屋。";return false;}
            var plan=World.PlanHouse(x,z);var cost=HouseCost(x,z);foreach(var e in plan){if(!VoxelWorld.InBounds(e.Key)||VoxelWorld.VillageRenewable(e.Key)){Notice="请选村民原房屋与麦田外、边界内的空地。";return false;}
                Block old=World.Get(e.Key);if(e.Key.y==14){if(!Blocks.Solid(old)||World.Edits.ContainsKey(e.Key)){Notice="地基需平整，不能覆盖已有建造。";return false;}}
                else if(old!=Block.Air){Notice="预定空地有方块或已有房屋，请先清理。";return false;}}
            var bounds=new Bounds(new Vector3(x+5,20,z+5),new Vector3(10,10,10));if(bounds.Intersects(Player.Body.bounds)){Notice="你站在房屋范围内，请先走到旁边再确认。";return false;}
            foreach(var hit in Physics.OverlapBox(bounds.center,bounds.extents,Quaternion.identity,~0,QueryTriggerInteraction.Ignore))if(hit.GetComponentInParent<VoxelWorld>()==null){Notice="这里有角色或其他设施，暂时不能建房。";return false;}
            if(!complimentary)foreach(var c in cost)if(Inventory[(int)c.Key]<c.Value){Notice="材料不足："+Blocks.Names[(int)c.Key]+"需要"+c.Value+"，现有"+Inventory[(int)c.Key];return false;}
            var before=new Dictionary<Vector3Int,Block>();foreach(var e in plan)before[e.Key]=World.Get(e.Key);var edits=new Dictionary<Vector3Int,Block>(World.Edits);
            var previousBase=new List<SavedBlock>(HouseBase);HouseBase.Clear();foreach(var e in before)HouseBase.Add(new SavedBlock{x=e.Key.x,y=e.Key.y,z=e.Key.z,id=(int)e.Value});
            foreach(var e in plan)World.Edit(e.Key,e.Value,false);if(!complimentary)foreach(var c in cost)Inventory[(int)c.Key]-=c.Value;World.RebuildAll();
            bool oldUnlocked=CompanionUnlocked,oldEnabled=CompanionEnabled;int oldRevision=CompanionRevision;CompanionUnlocked=CompanionEnabled=true;CompanionRevision=1;
            try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception err){CompanionUnlocked=oldUnlocked;CompanionEnabled=oldEnabled;CompanionRevision=oldRevision;HouseBase.Clear();HouseBase.AddRange(previousBase);foreach(var e in before)World.Edit(e.Key,e.Value,false);World.Edits.Clear();foreach(var e in edits)World.Edits[e.Key]=e.Value;if(!complimentary)foreach(var c in cost)Inventory[(int)c.Key]+=c.Value;World.RebuildAll();Notice="保存失败，建造已撤销："+err.Message;return false;}
            CompanionUnlocked=CompanionEnabled=true;Notice="同款村屋建好了！孩子伙伴已经出现，按P可以让她回家或再次召唤。";return true;
        }
        public bool HouseMenuOpen=>houseMenu;
        public void OpenHouseMenu(){if(Health<=0||VictoryOpen)return;if(houseMenu){SetPaused(false);return;}villageMenu=null;inventoryOpen=travelOpen=false;houseMenu=true;houseScroll=Vector2.zero;SetPaused(true);}
        void DrawHouseMenu(float w,float h)
        {
            float x=w/2-330,y=h/2-270;Panel(new Rect(x,y,660,540),Paper);Label(new Rect(x+24,y+20,612,40),"V 建造 · 同款村屋",28,Ink);
            Label(new Rect(x+24,y+67,612,45),"推荐空地：村庄南侧 x28–37 / z50–59\n10×10村屋，含门窗、屋顶与家具；先走出范围再确认。",15,Muted);
            var cost=HouseCost(NeighborPlot.x,NeighborPlot.y);houseScroll=GUI.BeginScrollView(new Rect(x+24,y+123,612,200),houseScroll,new Rect(0,0,586,cost.Count*29));int row=0;foreach(var c in cost){Label(new Rect(5,row++*29,580,27),Blocks.Names[(int)c.Key]+"：需要 "+c.Value+" / 现有 "+Inventory[(int)c.Key],16,Inventory[(int)c.Key]>=c.Value?Ink:new Color(.75f,.2f,.13f));}GUI.EndScrollView();
            if(LiftMenuButton(new Rect(x+24,y+337,612,40),"确认建造同款村屋（先备齐上方材料）",false)){if(BuildNeighborHouse(NeighborPlot.x,NeighborPlot.y)){SetPaused(false);return;}}
            if(LiftMenuButton(new Rect(x+24,y+385,300,36),"在前方空地建小屋",false)){if(BuildHouseHere()){SetPaused(false);return;}}
            if(LiftMenuButton(new Rect(x+24,y+438,612,35),"免费创建完整秘密基地（房屋＋密室）",false)){CreateCompleteSecretBase();return;}
            if(LiftMenuButton(new Rect(x+337,y+385,299,36),"关闭 · V / Esc",false))SetPaused(false);
            var s=new GUIStyle(GUI.skin.label){fontSize=15,wordWrap=true};s.normal.textColor=Ink;GUI.Label(new Rect(x+24,y+483,612,47),Notice,s);
        }
    }
}
