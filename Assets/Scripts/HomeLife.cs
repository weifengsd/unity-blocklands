using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public partial class Sandbox
    {
        public readonly List<SavedBlock> HouseBase=new List<SavedBlock>();
        public bool HomeSeated {get;private set;}public bool HoldingBook=>heldBook!=null;
        sealed class HomeBook {public readonly string Title,Category;public readonly string[] Pages;public HomeBook(string title,string category,params string[] pages){Title=title;Category=category;Pages=pages;}}
        static readonly HomeBook[] Library={
            new HomeBook("《风铃村的一天》","村庄故事","清晨，风铃村的窗帘被风轻轻吹开。\n小禾说：今天要给羊添草。","午后，麦田像金色的海。\n村民们把收好的小麦送到工作台旁。","傍晚，大家把铁门关好，屋里的电视亮起来。"),
            new HomeBook("《水晶谷采集手册》","探索指南","透明水晶在水晶谷的岩壁和小矿脉里闪光。","采集后可在工作台查看窗户配方：正中一块透明水晶。","别忘了带食物。走得太远时，先找安全的位置休息。","回村后，拉开窗帘，让新窗户迎着阳光。"),
            new HomeBook("《小熊点心食谱》","食谱","第一步：收集小麦和苹果。","第二步：把小麦、苹果、小麦横着摆在工作台上。","第三步：做好的小熊饼干可以补充体力。","如果朋友来作客，可以把点心放在餐桌上分享。","页边小字：吃东西仍然使用 R 键。"),
            new HomeBook("《云端电梯旅行记》","旅行故事","我和家人走进玻璃电梯。","按钮有底层、1到7层，还有顶层。","村民去他们的楼层，玩家可以继续去自己的楼层。","电梯经过没被选择的楼层时不会停。","顶层的风很大，扶好栏杆再看远处的村庄。","回程时，我们把看到的风景写进这本书。"),
            new HomeBook("《窗边的小猫》","图画故事","一只灰白小猫在窗台上打了个哈欠。","它看见窗帘关着，就轻轻推了推。","阳光从窗缝照进来，小猫的尾巴像一根问号。","它听见电视里有人说：你好，今天过得怎么样？","小猫没有回答，只把鼻子贴在玻璃上看云。","傍晚，它跳到沙发旁，陪主人读完最后一页。","故事结束：明天也许会有新的冒险。"),
            new HomeBook("《幻方世界建筑图册》","建造指南","一座小屋先需要平整的地基。","门要留出通道，不能把人关在里面。","窗户可以朝向风景最好的一面。","三幅窗帘让房间既明亮又有隐私。","床和沙发朝向电视，休息时更舒服。","书架旁放一张椅子，就有了安静的阅读角。","电视桌要靠墙，中央通道留给家人走动。","完成后，记得按 F5 保存世界。"),
            new HomeBook("《竹林里的风》","自然观察","竹叶在风里发出沙沙声。","一根竹子并不孤单，整片竹林会一起摇摆。","竹子上下摆放能做木棍，九宫格装满能做竹板。","在地下熔岩区要更小心，那里没有柔软的草地。","观察完别急着离开，看看水面有没有倒影。"),
            new HomeBook("《小小广播站》","短篇广播","欢迎收听幻方世界夜间广播。","主持人：今晚的天空有几颗星？\n小朋友：我数到八颗，又发现第九颗！","主持人：明天想去哪儿？\n小朋友：去水晶谷，再回家看动画！","本期广播结束，请翻回首页或换一本书。"),
            new HomeBook("《窗台便签》","一页短笺","给回家的你：铁门已经关好，热茶在桌上。"),
            new HomeBook("《两颗星的约定》","双页小故事","第一颗星说：我们一起照亮回家的路。","第二颗星说：等你翻到这一页，就知道我还在这里。")
        };
        Vector3 seatExit;int demolitionTaps,bookCursor,bookPage;float lastDemolition;HomeFixture homeTV;HomeBook heldBook;
        bool OwnHouseCell(Vector3Int p)=>World.Region==1&&HouseBase.Count>0&&p.x>=28&&p.x<=37&&p.z>=50&&p.z<=59&&p.y>=15&&p.y<=23;
        void ResetHomePose(){HomeSeated=false;heldBook=null;bookPage=0;homeTV=null;demolitionTaps=0;if(Player!=null)Player.HomeRest(false);}
        HomeFixture AimFixture(){return RayFromView(out var h,6)?h.collider.GetComponentInParent<HomeFixture>():null;}
        HomeFixture NearbyTV(){HomeFixture best=null;float d=12;foreach(var f in World.GetComponentsInChildren<HomeFixture>())if(f.Kind==Block.Television){float n=Vector3.Distance(f.transform.position,Player.transform.position);if(n<d){best=f;d=n;}}return best;}
        public bool UseSeat(Vector3Int cell)
        {
            if(HomeSeated){HomeSeated=false;Player.HomeRest(false);Player.Teleport(seatExit);Notice="已起身。";return true;}
            Block b=World.Get(cell);if(b!=Block.Bed&&b!=Block.Sofa)return false;
            seatExit=Player.transform.position;homeTV=NearbyTV();Vector3 pos=(Vector3)cell+new Vector3(.5f,b==Block.Bed?.55f:.6f,.5f);Vector3 d=homeTV!=null?homeTV.transform.position-pos:Vector3.back;
            Player.Teleport(pos,Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,0);Player.HomeRest(true,b==Block.Bed);HomeSeated=true;Notice=(b==Block.Bed?"已上床休息":"已坐在沙发上")+" · H起身，J开关电视，↑↓换台";return true;
        }
        public bool ToggleHomeTV(){homeTV=NearbyTV();if(homeTV==null){Notice="附近没有电视。";return false;}homeTV.Toggle();Notice=homeTV.State.On?"电视已开启 · 坐下后↑↓换台":"电视已关闭";return true;}
        public bool ToggleBook(Vector3Int cell)
        {
            if(HoldingBook){heldBook=null;bookPage=0;Notice="书已放回书柜。";return true;}
            if(World.Get(cell)!=Block.Bookshelf)return false;
            int step=Random.Range(1,Library.Length);bookCursor=(bookCursor+step)%Library.Length;heldBook=Library[bookCursor];bookPage=0;
            Notice="从书柜拿出"+heldBook.Title+" · ←→翻页，K放回";return true;
        }
        public int HeldBookPages=>heldBook==null?0:heldBook.Pages.Length;public int HeldBookPage=>heldBook==null?0:bookPage+1;public string HeldBookTitle=>heldBook?.Title??"";
        public bool TurnBook(int delta){if(!HoldingBook)return false;bookPage=(bookPage+delta%heldBook.Pages.Length+heldBook.Pages.Length)%heldBook.Pages.Length;return true;}
        public static bool LibraryUsesOneToEightPages(){var found=new bool[9];foreach(var book in Library)found[book.Pages.Length]=true;for(int i=1;i<=8;i++)if(!found[i])return false;return true;}
        bool HomeKeys()
        {
            if(Paused)return false;
            if(HoldingBook){if(Input.GetKeyDown(KeyCode.K)){ToggleBook(default);return true;}if(Input.GetKeyDown(KeyCode.LeftArrow))TurnBook(-1);if(Input.GetKeyDown(KeyCode.RightArrow))TurnBook(1);return true;}
            if(ShortH){
                if(HomeSeated){UseSeat(default);return true;}var f=AimFixture();if(f!=null){f.Toggle();Notice=Blocks.Names[(int)f.Kind]+(f.Kind==Block.Television?(f.State.On?"已打开":"已关闭"):(f.State.Open?"已打开":"已关闭"));return true;}
                if(hasTarget&&UseSeat(target))return true;
            }
            if(Input.GetKeyDown(KeyCode.K)&&(HoldingBook||hasTarget)){ToggleBook(target);return true;}
            if(Input.GetKeyDown(KeyCode.J)){ToggleHomeTV();return true;}
            if(HomeSeated){if(homeTV!=null&&homeTV.State.On){if(Input.GetKeyDown(KeyCode.UpArrow))homeTV.Channel(-1);if(Input.GetKeyDown(KeyCode.DownArrow))homeTV.Channel(1);}return true;}
            return false;
        }
        public bool DemolishOwnHouse()
        {
            if(World.Region!=1||HouseBase.Count==0){Notice="这里只能整屋拆除自己用V建造的房屋。";return false;}
            var old=new Dictionary<Vector3Int,Block>();var edits=new Dictionary<Vector3Int,Block>(World.Edits);var baseline=new List<SavedBlock>(HouseBase);
            foreach(var e in baseline){var p=new Vector3Int(e.x,e.y,e.z);old[p]=World.Get(p);World.Edit(p,(Block)e.id,false);World.Edits.Remove(p);World.FixtureStates.Remove(p);}HouseBase.Clear();ResetHomePose();World.RebuildAll();
            try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(System.Exception e){foreach(var p in old)World.Edit(p.Key,p.Value,false);World.Edits.Clear();foreach(var p in edits)World.Edits[p.Key]=p.Value;HouseBase.AddRange(baseline);World.RebuildAll();Notice="保存失败，拆除已撤销："+e.Message;return false;}
            Notice="自建房已拆除，原地面已恢复；按V可重新建房。材料不返还。";return true;
        }
        void MigrateOwnHouse()
        {
            if(World.Region!=1||HouseBase.Count>0)return;
            if(!World.Edits.TryGetValue(new Vector3Int(28,15,50),out var floor)||floor!=Block.Planks||World.Get(new Vector3Int(28,21,50))!=Block.Brick)return;
            foreach(var p in World.PlanHouse(28,50).Keys)HouseBase.Add(new SavedBlock{x=p.x,y=p.y,z=p.z,id=(int)(p.y==14?(p.x<=33?Block.WoodTile:Block.Grass):Block.Air)});CompanionUnlocked=CompanionEnabled=true;
        }
        void DrawHomeBook(float w,float h)
        {
            if(!HoldingBook||Paused)return;Panel(new Rect(w/2-260,h/2-145,520,290),new Color(.97f,.92f,.77f));Label(new Rect(w/2-235,h/2-125,470,35),heldBook.Title+" · "+heldBook.Category,22,Ink);
            Label(new Rect(w/2-235,h/2-83,470,178),heldBook.Pages[bookPage],18,Ink);
            Label(new Rect(w/2-235,h/2+107,470,25),"第 "+(bookPage+1)+" / "+heldBook.Pages.Length+" 页   ← → 翻页   K 放回书柜",15,Muted);
        }
    }
}
