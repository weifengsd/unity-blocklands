using UnityEngine;

namespace Blocklands
{
    public enum Block : byte { Air, Grass, Dirt, Stone, Wood, Leaves, Sand, Planks, Brick, Glass, Water, Bedrock, Workbench, Stick, WoodPickaxe, StonePickaxe, IronOre, Iron, IronPickaxe, WoodAxe, StoneAxe, IronAxe, Torch, StoneBricks, DarkStone, ShadowCrystal, CrystalLamp, MossBrick, DarkBrick, QuartzTile, Bookshelf, WoodTile, WoodShovel, StoneShovel, IronShovel, CrystalPickaxe, Obsidian, DreamCrystal, Wheat, Apple, Bread, Beef, Mutton, Bamboo, BambooPlanks, Lava, WoodSword, StoneSword, IronSword, Bucket, LavaBucket, Pork, Table, Chair, Bed, Sofa, Cabinet, Cookie, Coin, IronDoor, Window, Gauze, Curtain, Television, CrystalHammer, CastleKey, PrisonBars, RedCarpet }

    public static class Blocks
    {
        public static readonly string[] Names = { "空气", "草方块", "泥土", "石头", "原木", "树叶", "沙子", "木板", "砖块", "水晶块", "水", "基岩", "工作台", "木棍", "木镐", "石镐", "铁矿石", "铁锭", "铁镐", "木斧", "石斧", "铁斧", "手提灯", "石砖", "梦影岩", "紫水晶", "水晶灯", "苔藓砖", "梦影砖", "水晶地砖", "书架", "木纹砖", "木铲", "石铲", "铁铲", "水晶镐", "黑紫晶块", "梦影绿晶", "小麦", "苹果", "面包", "牛肉", "羊肉", "竹子", "竹板", "岩浆", "木剑", "石剑", "铁剑", "铁桶", "岩浆桶", "猪肉", "餐桌", "椅子", "小床", "沙发", "柜子", "饼干", "钱币（1元）" };
        public const Block PinkCrystal=(Block)68,DenseRock=(Block)69,StarRock=(Block)70,IronDrill=(Block)71,StarDrill=(Block)72,BedrockShard=(Block)73;
        public const Block CrystalRod=(Block)74,CombinedStarPick=(Block)75,Snow=(Block)76;
        public const Block CrystalOre=(Block)77,CrystalGem=(Block)78,NetherOre=(Block)79,NetherIngot=(Block)80,CrystalHelmet=(Block)81,CrystalChest=(Block)82,CrystalLegs=(Block)83,CrystalBoots=(Block)84,CrystalScepter=(Block)85,NetherHelmet=(Block)86,NetherChest=(Block)87,NetherLegs=(Block)88,NetherBoots=(Block)89,NetherSword=(Block)90,PaperSheet=(Block)91,Pencil=(Block)92,InvitationCard=(Block)93;
        public const Block SurveillanceCamera=(Block)94;
        public const int Count=95;
        public static readonly int[] BagItems={1,2,3,4,5,6,7,8,9,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67};
        static Blocks(){System.Array.Resize(ref Names,Count);Names[9]="透明水晶块";Names[59]="铁门";Names[60]="窗户";Names[61]="纱布";Names[62]="窗帘";Names[63]="电视";Names[64]="水晶锤";Names[65]="城堡钥匙";Names[66]="铁栅栏";Names[67]="红地毯";Names[68]="粉色水晶";Names[69]="深铁岩";Names[70]="星晶岩";Names[71]="铁钻";Names[72]="星晶镐";Names[73]="基岩碎片";Names[74]="水晶杆";Names[75]="结合星镐";Names[76]="积雪";string[] extra={"水晶矿石","水晶块","下界合金矿","下界合金锭","水晶帽","水晶衣服","水晶裤子","水晶鞋","水晶权杖","合金帽","合金衣服","合金裤子","合金鞋","合金剑","绘画纸","彩色铅笔","邀请函","可安装监控摄像头"};for(int i=0;i<extra.Length;i++)Names[77+i]=extra[i];int n=BagItems.Length;System.Array.Resize(ref BagItems,n+24);System.Array.Copy(BagItems,0,BagItems,1,n);BagItems[0]=94;for(int i=0;i<23;i++)BagItems[n+1+i]=68+i;}
        static readonly Color[] Colors = {
            Color.clear, new Color(.38f,.66f,.27f), new Color(.51f,.34f,.21f), new Color(.54f,.59f,.62f),
            new Color(.43f,.28f,.15f), new Color(.24f,.51f,.25f), new Color(.85f,.77f,.51f),
            new Color(.68f,.46f,.25f), new Color(.67f,.33f,.25f), new Color(.42f,.79f,.81f),
            new Color(.15f,.57f,.71f), new Color(.2f,.23f,.27f), new Color(.64f,.4f,.2f),
            new Color(.64f,.44f,.23f), new Color(.77f,.58f,.32f), new Color(.6f,.66f,.69f),
            new Color(.62f,.48f,.37f),new Color(.76f,.79f,.81f),new Color(.8f,.84f,.88f),
            new Color(.69f,.47f,.25f),new Color(.53f,.61f,.64f),new Color(.75f,.82f,.88f),new Color(1,.78f,.32f),
            new Color(.58f,.62f,.66f),new Color(.24f,.19f,.34f),new Color(.66f,.38f,.95f),new Color(.5f,.98f,.9f),
            new Color(.37f,.52f,.33f),new Color(.4f,.28f,.55f),new Color(.78f,.92f,.91f),new Color(.56f,.31f,.2f),
            new Color(.8f,.61f,.38f),new Color(.68f,.49f,.27f),new Color(.6f,.64f,.68f),new Color(.8f,.86f,.9f),new Color(.41f,.95f,.9f),
            new Color(.19f,.1f,.28f),new Color(.18f,.96f,.48f),new Color(.83f,.69f,.25f),new Color(.88f,.22f,.16f),new Color(.78f,.51f,.21f),new Color(.62f,.24f,.2f),new Color(.72f,.36f,.25f),new Color(.35f,.66f,.23f),new Color(.74f,.73f,.34f),new Color(1,.3f,.04f),new Color(.63f,.44f,.23f),new Color(.58f,.64f,.68f),new Color(.8f,.86f,.94f),new Color(.7f,.75f,.8f),new Color(1,.38f,.08f),new Color(.89f,.48f,.5f),new Color(.65f,.4f,.22f),new Color(.72f,.5f,.29f),new Color(.85f,.35f,.46f),new Color(.27f,.55f,.7f),new Color(.47f,.3f,.18f),new Color(.8f,.55f,.25f),new Color(1,.75f,.15f)
        };
        public static bool Solid(Block b) => b != Block.Air && b != Block.Water;
        public static bool Buildable(Block b) => b==CrystalGem||b==CrystalOre||b==NetherOre||b==Snow||(b>=PinkCrystal&&b<=StarRock)||b==BedrockShard||(b >= Block.Grass && b <= Block.Glass)||b==Block.Workbench||b==Block.IronOre||(b>=Block.StoneBricks&&b<=Block.WoodTile)||b==Block.Obsidian||b==Block.DreamCrystal||b==Block.Wheat||b==Block.Bamboo||b==Block.BambooPlanks||b==Block.PrisonBars||b==Block.RedCarpet||Furniture(b);
        public static bool Fixture(Block b)=>b==Block.IronDoor||b==Block.Window||b==Block.Curtain||b==Block.Television;
        public static bool Furniture(Block b)=>b>=Block.Table&&b<=Block.Cabinet||Fixture(b)||b==Block.RedCarpet||b==Block.Gauze;
        public static bool Selectable(int id)=>id>=1&&id<=9||id>=12&&id<Count&&id!=(int)Block.Lava;
        public static bool Tool(Block b)=>b==CombinedStarPick||b==IronDrill||b==StarDrill||b==Block.WoodPickaxe||b==Block.StonePickaxe||b==Block.IronPickaxe||b==Block.CrystalPickaxe||b==Block.CrystalHammer||(b>=Block.WoodAxe&&b<=Block.IronAxe)||(b>=Block.WoodShovel&&b<=Block.IronShovel)||(b>=Block.WoodSword&&b<=Block.IronSword);
        public static float FoodHeal(Block b)=>b==Block.Cookie?1:b==Block.Apple?2:b==Block.Bread?3:b==Block.Beef||b==Block.Mutton||b==Block.Pork?4:0;
        public static float AttackPower(Block b)=>b==NetherSword?16:b==CrystalScepter?12:b==Block.CrystalHammer?12:b==Block.IronSword?10:b==Block.StoneSword?7:b==Block.WoodSword?5:Tool(b)?3:1;
        public static float DigDelay(Block tool,Block target)
        {
            if(tool==Block.Air)return .45f;
            if(tool==CrystalScepter)return .07f;
            if(tool==NetherSword)return .08f;
            if(tool==Block.IronSword)return .13f;
            if(tool==Block.StoneSword)return .17f;
            if(tool==Block.WoodSword)return .20f;
            if(tool==CombinedStarPick)return .025f;
            if(tool==IronDrill||tool==StarDrill)return .04f;
            if(tool==Block.CrystalHammer)return .035f;
            if(tool==Block.CrystalPickaxe)return .055f;
            if(tool==Block.IronPickaxe)return .065f;
            if(tool==Block.StonePickaxe)return .08f;
            if(tool==Block.WoodPickaxe)return .13f;
            if(tool>=Block.WoodAxe&&tool<=Block.IronAxe&&(target==Block.Wood||target==Block.Planks||target==Block.Leaves||target==Block.Bookshelf))return .12f-((int)tool-(int)Block.WoodAxe)*.03f;
            if(tool>=Block.WoodShovel&&tool<=Block.IronShovel&&(target==Block.Dirt||target==Block.Sand||target==Block.Grass||target==Snow))return .12f-((int)tool-(int)Block.WoodShovel)*.03f;
            return .22f;
        }
        public static Color ColorOf(Block b, int face = 2)
        {
            if((int)b>=77)return b==CrystalOre?new Color(.48f,.53f,.59f):b==NetherOre?new Color(.28f,.21f,.3f):b==PaperSheet||b==InvitationCard?new Color(1,.94f,.8f):b==Pencil?new Color(.95f,.67f,.21f):b>=NetherIngot&&b<=NetherSword&&b!=CrystalScepter&&b!=CrystalHelmet&&b!=CrystalChest&&b!=CrystalLegs&&b!=CrystalBoots?new Color(.3f,.25f,.39f):new Color(.29f,.81f,.96f);
            if(b==Snow)return face==2?new Color(.97f,.98f,1):new Color(.80f,.87f,.94f);
            if(b==PinkCrystal)return new Color(.94f,.58f,.76f);
            if(b==DenseRock)return new Color(.22f,.31f,.42f);
            if(b==StarRock)return new Color(.38f,.22f,.63f);
            if(b==IronDrill)return new Color(.62f,.78f,.85f);
            if(b==StarDrill)return new Color(.76f,.52f,.96f);
            if(b==BedrockShard)return new Color(.28f,.30f,.38f);
            Color c = b==Block.Coin?new Color(.20f,.78f,.38f):b==Block.CrystalHammer?new Color(.39f,.92f,.95f):b==Block.CastleKey?new Color(.85f,.75f,.24f):b==Block.PrisonBars?new Color(.42f,.48f,.55f):b==Block.RedCarpet?new Color(.72f,.06f,.09f):(int)b>=59?b==Block.Gauze?new Color(.95f,.94f,.85f):b==Block.Curtain?new Color(.82f,.42f,.5f):b==Block.Television?new Color(.12f,.16f,.2f):new Color(.65f,.78f,.82f):Colors[(int)b];
            if (b == Block.Grass && face != 2) c = face == 3 ? Colors[2] : new Color(.43f,.45f,.23f);
            if (b == Block.Wood && (face == 2 || face == 3)) c *= 1.25f;
            return c;
        }
    }
}
