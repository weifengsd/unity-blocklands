using System;

namespace Blocklands
{
    public class Recipe
    {
        public readonly string Name,Hint;
        public readonly int Width,Height,Count;
        public readonly Block[] Pattern;
        public readonly Block Output;
        public Recipe(string name,string hint,int width,int height,Block[] pattern,Block output,int count)
        {Name=name;Hint=hint;Width=width;Height=height;Pattern=pattern;Output=output;Count=count;}
    }
    public class Crafting
    {
        public readonly Block[] Grid=new Block[9];
        public static readonly Recipe[] Recipes={
            new Recipe("木板","任意一格放 1 个原木",1,1,new[]{Block.Wood},Block.Planks,4),
            new Recipe("木棍","2 个木板上下相邻，不能横放",1,2,new[]{Block.Planks,Block.Planks},Block.Stick,4),
            new Recipe("工作台","4 个木板摆成 2 × 2 正方形",2,2,new[]{Block.Planks,Block.Planks,Block.Planks,Block.Planks},Block.Workbench,1),
            new Recipe("木镐","上排 3 木板，中下两格放木棍",3,3,new[]{Block.Planks,Block.Planks,Block.Planks,Block.Air,Block.Stick,Block.Air,Block.Air,Block.Stick,Block.Air},Block.WoodPickaxe,1),
            new Recipe("石镐","上排 3 石头，中下两格放木棍",3,3,new[]{Block.Stone,Block.Stone,Block.Stone,Block.Air,Block.Stick,Block.Air,Block.Air,Block.Stick,Block.Air},Block.StonePickaxe,1),
            new Recipe("砖块","幻方世界配方：4 石头摆成正方形",2,2,new[]{Block.Stone,Block.Stone,Block.Stone,Block.Stone},Block.Brick,4),
            new Recipe("铁锭","幻方世界配方：1 铁矿石提炼 1 铁锭",1,1,new[]{Block.IronOre},Block.Iron,1),
            Pick(Block.Iron,Block.IronPickaxe),Pick(Block.Glass,Block.CrystalPickaxe),
            Axe(Block.Planks,Block.WoodAxe),Axe(Block.Stone,Block.StoneAxe),Axe(Block.Iron,Block.IronAxe),
            Shovel(Block.Planks,Block.WoodShovel),Shovel(Block.Stone,Block.StoneShovel),Shovel(Block.Iron,Block.IronShovel),
            new Recipe("手提灯","上格水晶、下格木棍；手持照明",1,2,new[]{Block.Glass,Block.Stick},Block.Torch,1),
            new Recipe("石砖","一横排放 3 个石头",3,1,new[]{Block.Stone,Block.Stone,Block.Stone},Block.StoneBricks,3),
            new Recipe("透明水晶块","幻方世界配方：4 沙子摆成正方形；用于透明窗户",2,2,new[]{Block.Sand,Block.Sand,Block.Sand,Block.Sand},Block.Glass,4),
            new Recipe("梦影砖","4 梦影岩摆成正方形",2,2,new[]{Block.DarkStone,Block.DarkStone,Block.DarkStone,Block.DarkStone},Block.DarkBrick,4),
            new Recipe("苔藓砖","上格树叶、下格石砖",1,2,new[]{Block.Leaves,Block.StoneBricks},Block.MossBrick,2),
            new Recipe("水晶地砖","4 水晶块摆成正方形",2,2,new[]{Block.Glass,Block.Glass,Block.Glass,Block.Glass},Block.QuartzTile,4),
            new Recipe("木纹砖","3 木板摆成一横排",3,1,new[]{Block.Planks,Block.Planks,Block.Planks},Block.WoodTile,3),
            new Recipe("书架","上排木板，下排木棍（装饰）",3,2,new[]{Block.Planks,Block.Planks,Block.Planks,Block.Stick,Block.Stick,Block.Stick},Block.Bookshelf,1),
            new Recipe("水晶灯","中心紫水晶，四周普通水晶",3,3,new[]{Block.Air,Block.Glass,Block.Air,Block.Glass,Block.ShadowCrystal,Block.Glass,Block.Air,Block.Glass,Block.Air},Block.CrystalLamp,1),
            new Recipe("面包","3 小麦横排；吃掉恢复 3 心",3,1,new[]{Block.Wheat,Block.Wheat,Block.Wheat},Block.Bread,1),
            new Recipe("苹果","自定义：3 树叶横排；恢复 2 心",3,1,new[]{Block.Leaves,Block.Leaves,Block.Leaves},Block.Apple,1),
            new Recipe("竹制木棍","2 竹子上下摆放",1,2,new[]{Block.Bamboo,Block.Bamboo},Block.Stick,4),
            new Recipe("竹板","9 竹子填满九宫格",3,3,new[]{Block.Bamboo,Block.Bamboo,Block.Bamboo,Block.Bamboo,Block.Bamboo,Block.Bamboo,Block.Bamboo,Block.Bamboo,Block.Bamboo},Block.BambooPlanks,4),
            Sword(Block.Planks,Block.WoodSword),Sword(Block.Stone,Block.StoneSword),Sword(Block.Iron,Block.IronSword),
            new Recipe("铁桶","3铁锭摆成V形；手持左键收岩浆",3,2,new[]{Block.Iron,Block.Air,Block.Iron,Block.Air,Block.Iron,Block.Air},Block.Bucket,1),
            new Recipe("餐桌","上排木板，底排两端木棍",3,2,new[]{Block.Planks,Block.Planks,Block.Planks,Block.Stick,Block.Air,Block.Stick},Block.Table,1),
            new Recipe("椅子","木板做靠背与坐面，下排木棍",2,3,new[]{Block.Planks,Block.Air,Block.Planks,Block.Planks,Block.Stick,Block.Stick},Block.Chair,1),
            new Recipe("小床","上排树叶，下排木板",3,2,new[]{Block.Leaves,Block.Leaves,Block.Leaves,Block.Planks,Block.Planks,Block.Planks},Block.Bed,1),
            new Recipe("沙发","上排两端树叶，下排木板",3,2,new[]{Block.Leaves,Block.Air,Block.Leaves,Block.Planks,Block.Planks,Block.Planks},Block.Sofa,1),
            new Recipe("柜子","两列：木板、水晶、木板",2,3,new[]{Block.Planks,Block.Planks,Block.Glass,Block.Glass,Block.Planks,Block.Planks},Block.Cabinet,1),
            new Recipe("饼干","一横排：小麦、苹果、小麦",3,1,new[]{Block.Wheat,Block.Apple,Block.Wheat},Block.Cookie,3),
            new Recipe("果香小饼干","一横排：苹果、小麦、苹果；可做 4 块饼干",3,1,new[]{Block.Apple,Block.Wheat,Block.Apple},Block.Cookie,4),
            new Recipe("竹香小床","上排 3 树叶，下排 3 竹板",3,2,new[]{Block.Leaves,Block.Leaves,Block.Leaves,Block.BambooPlanks,Block.BambooPlanks,Block.BambooPlanks},Block.Bed,1),
            new Recipe("粉晶夜灯","粉色水晶在上、木棍在下",1,2,new[]{Blocks.PinkCrystal,Block.Stick},Block.CrystalLamp,1),
            new Recipe("钱币","2个木板横着并排 → 5元",2,1,new[]{Block.Planks,Block.Planks},Block.Coin,5),
            new Recipe("铁门","只在中间一列放3石头：第2、5、8格",3,3,new[]{Block.Air,Block.Stone,Block.Air,Block.Air,Block.Stone,Block.Air,Block.Air,Block.Stone,Block.Air},Block.IronDoor,1),
            new Recipe("窗户","只在正中第5格放透明水晶；水晶谷采集，或4沙子合成",3,3,new[]{Block.Air,Block.Air,Block.Air,Block.Air,Block.Glass,Block.Air,Block.Air,Block.Air,Block.Air},Block.Window,1),
            new Recipe("窗帘","3纱布横排；村庄入口东侧 x35–38/z6–9 白色采集点",3,1,new[]{Block.Gauze,Block.Gauze,Block.Gauze},Block.Curtain,1),
            new Recipe("电视","上排3铁锭，中排水晶/水晶灯/水晶，下排3木板",3,3,new[]{Block.Iron,Block.Iron,Block.Iron,Block.Glass,Block.CrystalLamp,Block.Glass,Block.Planks,Block.Planks,Block.Planks},Block.Television,1),
            new Recipe("城堡钥匙","中间一列：铁锭、透明水晶、铁锭；其余格留空",3,3,new[]{Block.Air,Block.Iron,Block.Air,Block.Air,Block.Glass,Block.Air,Block.Air,Block.Iron,Block.Air},Block.CastleKey,1),
            new Recipe("基岩灯","基岩碎片在上，透明水晶在下",1,2,new[]{Blocks.BedrockShard,Block.Glass},Block.CrystalLamp,2),
            new Recipe("粉色水晶","透明水晶在上，基岩碎片在下",1,2,new[]{Block.Glass,Blocks.BedrockShard},Blocks.PinkCrystal,4),
            new Recipe("星晶镐","上排3星晶岩，中下两格放木棍",3,3,new[]{Blocks.StarRock,Blocks.StarRock,Blocks.StarRock,Block.Air,Block.Stick,Block.Air,Block.Air,Block.Stick,Block.Air},Blocks.StarDrill,1),
            new Recipe("水晶杆","竖放：透明水晶、透明水晶、木棍",1,3,new[]{Block.Glass,Block.Glass,Block.Stick},Blocks.CrystalRod,1),
            new Recipe("结合星镐","上排木镐/石镐/铁镐；中排水晶镐/星晶镐/水晶锤；下排铁钻/水晶杆/空",3,3,new[]{Block.WoodPickaxe,Block.StonePickaxe,Block.IronPickaxe,Block.CrystalPickaxe,Blocks.StarDrill,Block.CrystalHammer,Blocks.IronDrill,Blocks.CrystalRod,Block.Air},Blocks.CombinedStarPick,1),
            new Recipe("铁钻","上排3铁锭；中间铁镐；下中水晶杆。铁矿在水晶矿谷采集。",3,3,new[]{Block.Iron,Block.Iron,Block.Iron,Block.Air,Block.IronPickaxe,Block.Air,Block.Air,Blocks.CrystalRod,Block.Air},Blocks.IronDrill,1),
            new Recipe("水晶锤","上排3透明水晶；中间铁锭；下中木棍。",3,3,new[]{Block.Glass,Block.Glass,Block.Glass,Block.Air,Block.Iron,Block.Air,Block.Air,Block.Stick,Block.Air},Block.CrystalHammer,1),
            new Recipe("水晶块","T选水晶矿谷采矿；任意一格矿石→2水晶块。可建造、做装备。",1,1,new[]{Blocks.CrystalOre},Blocks.CrystalGem,2),
            new Recipe("下界合金锭","2合金矿与2铁锭交错摆成方形",2,2,new[]{Blocks.NetherOre,Block.Iron,Block.Iron,Blocks.NetherOre},Blocks.NetherIngot,2),
            Armor(Blocks.CrystalGem,Blocks.CrystalHelmet,0),Armor(Blocks.CrystalGem,Blocks.CrystalChest,1),Armor(Blocks.CrystalGem,Blocks.CrystalLegs,2),Armor(Blocks.CrystalGem,Blocks.CrystalBoots,3),
            Armor(Blocks.NetherIngot,Blocks.NetherHelmet,0),Armor(Blocks.NetherIngot,Blocks.NetherChest,1),Armor(Blocks.NetherIngot,Blocks.NetherLegs,2),Armor(Blocks.NetherIngot,Blocks.NetherBoots,3),
            new Recipe("水晶权杖","竖放水晶块、水晶杆、木棍",1,3,new[]{Blocks.CrystalGem,Blocks.CrystalRod,Block.Stick},Blocks.CrystalScepter,1),Sword(Blocks.NetherIngot,Blocks.NetherSword),
            new Recipe("监控摄像头","横排：铁锭、透明水晶、水晶灯",3,1,new[]{Block.Iron,Block.Glass,Block.CrystalLamp},Blocks.SurveillanceCamera,1),
        };
        static Recipe Armor(Block m,Block output,int slot)
        {
            Block a=Block.Air;Block[] p=slot==0?new[]{m,m,m,m,a,m,a,a,a}:slot==1?new[]{m,a,m,m,m,m,m,m,m}:slot==2?new[]{m,m,m,m,a,m,m,a,m}:new[]{m,a,m,m,a,m,a,a,a};
            return new Recipe(Blocks.Names[(int)output],slot==0?"上排3材料，第二排两端材料":slot==1?"除上排中间，其他8格材料":slot==2?"上排3材料，中下两排两端材料":"左、右两列各放2材料",3,3,p,output,1);
        }
        static Recipe Sword(Block m,Block o)=>new Recipe(Blocks.Names[(int)o],"竖排：2 "+Blocks.Names[(int)m]+"，底格木棍",1,3,new[]{m,m,Block.Stick},o,1);
        static Recipe Pick(Block material,Block output)=>new Recipe(Blocks.Names[(int)output],"上排 3 "+Blocks.Names[(int)material]+"，中下放木棍",3,3,new[]{material,material,material,Block.Air,Block.Stick,Block.Air,Block.Air,Block.Stick,Block.Air},output,1);
        static Recipe Axe(Block material,Block output)=>new Recipe(Blocks.Names[(int)output],"左上 3 材料呈拐角；右列 2 木棍",2,3,new[]{material,material,material,Block.Stick,Block.Air,Block.Stick},output,1);
        static Recipe Shovel(Block material,Block output)=>new Recipe(Blocks.Names[(int)output],"竖排："+Blocks.Names[(int)material]+"、木棍、木棍",1,3,new[]{material,Block.Stick,Block.Stick},output,1);
        // Grid cells reserve items from the bag. Only collecting the result spends them.
        // Closing/saving therefore cannot drop or duplicate ingredients.
        public int Reserved(Block item){int n=0;foreach(var cell in Grid)if(cell==item)n++;return n;}
        public int Available(Block item,int[] bag)=>bag[(int)item]-Reserved(item);
        public bool SetCell(int cell,Block item,int[] bag)
        {
            if(cell<0||cell>=9||item!=Block.Air&&!Blocks.Selectable((int)item))return false;
            if(Grid[cell]==item)return true;
            if(item!=Block.Air&&Available(item,bag)<=0)return false;
            Grid[cell]=item;return true;
        }
        public void Clear()=>Array.Clear(Grid,0,Grid.Length);
        public Recipe Match()
        {
            foreach(var r in Recipes)for(int oy=0;oy<=3-r.Height;oy++)for(int ox=0;ox<=3-r.Width;ox++)
            {
                bool match=true;
                for(int y=0;y<3;y++)for(int x=0;x<3;x++)
                {
                    bool inside=x>=ox&&x<ox+r.Width&&y>=oy&&y<oy+r.Height;
                    Block expected=inside?r.Pattern[(y-oy)*r.Width+x-ox]:Block.Air;
                    if(Grid[y*3+x]!=expected)match=false;
                }
                if(match)return r;
            }
            return null;
        }
        public bool CanCollect(int[] bag)
        {
            var r=Match();if(r==null)return false;
            for(int i=1;i<bag.Length;i++)if(bag[i]<Reserved((Block)i))return false;
            return bag[(int)r.Output]-Reserved(r.Output)<=9999-r.Count;
        }
        public bool Collect(int[] bag,out Recipe result)
        {
            result=Match();if(result==null||!CanCollect(bag))return false;
            foreach(var b in Grid)if(b!=Block.Air)bag[(int)b]--;
            bag[(int)result.Output]+=result.Count;Clear();return true;
        }
    }
}
