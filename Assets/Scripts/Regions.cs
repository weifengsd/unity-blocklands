using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public partial class VoxelWorld
    {
        public static readonly string[] RegionNames={"林地小屋","风铃村庄","水晶矿谷","梦影地","熔岩竹谷","地下熔岩","粉晶城堡神殿","深铁岩层","星晶岩层","地底星海","地心花园","彩虹游乐场","海风沙滩","云端风筝谷","萤火秘林","雪晶山谷","下界合金矿境"};
        public static readonly string[] RegionDescriptions={"小屋与好朋友小禾，G聊天。","村民、农田、兔子与可选铁傀儡。","水晶矿石、梦影绿晶与铁矿。","每次打败后，下一次进门又有龙。","竹林与岩浆，采集黑紫晶块造门。","继续向下开凿可以进入深铁岩层。","粉晶王宫、庭院与50元门票博物馆。","底层深铁岩需要铁钻。","底层星晶岩需要星晶钻。","结合星镐可以继续打开底层。","结合星镐发现的地下花园。","彩虹跳台：按提示踏出光之节奏。","潮汐寻宝：换位置追逐海边珍珠。","追风飞行：操控风筝越过云环。","萤火追逐：在林间找准发光方向。","雪橇冲刺：变换雪道躲开冰晶。","黑岩、岩浆与合金矿；采矿后合成装备。"};
        public int Region {get;private set;}
        public readonly Dictionary<int,List<SavedBlock>> RegionEdits=new Dictionary<int,List<SavedBlock>>();
        public System.Func<Vector3Int,bool> PreserveVillageCell;
        public Color SkyColor=>Region==5?new Color(.12f,.04f,.035f):Region==3?new Color(.24f,.19f,.34f):Region==14?new Color(.14f,.21f,.32f):Region==15?new Color(.73f,.85f,.96f):Region==11?new Color(.76f,.64f,.89f):Region==12?new Color(.63f,.82f,.91f):Region==13?new Color(.65f,.81f,.97f):Region==2?new Color(.55f,.7f,.79f):new Color(.64f,.79f,.88f);
        public List<SavedBlock> CopyEdits()
        {
            var result=new List<SavedBlock>();foreach(var e in Edits)result.Add(new SavedBlock{x=e.Key.x,y=e.Key.y,z=e.Key.z,id=(int)e.Value});return result;
        }
        public void ApplyEdits(List<SavedBlock> edits){foreach(var b in edits){if(b.y==0&&b.id==0){Put(b.x,0,b.z,Block.Air);Edits[new Vector3Int(b.x,0,b.z)]=Block.Air;}else Edit(new Vector3Int(b.x,b.y,b.z),(Block)b.id,false);}}
        public void SwitchRegion(int target)
        {
            if(target<0||target>=RegionNames.Length)throw new System.ArgumentOutOfRangeException(nameof(target));
            RegionEdits[Region]=CopyEdits();Generate(target);
            if(RegionEdits.TryGetValue(target,out var edits)){if(target==0)edits.RemoveAll(e=>CabinRenewable(new Vector3Int(e.x,e.y,e.z)));if(target==1)edits.RemoveAll(e=>(VillageRenewable(new Vector3Int(e.x,e.y,e.z))||e.y==14&&e.id==(int)Block.Air)&&!(PreserveVillageCell?.Invoke(new Vector3Int(e.x,e.y,e.z))??false));if(target==6)edits.RemoveAll(e=>CastleRenewable(new Vector3Int(e.x,e.y,e.z)));ApplyEdits(edits);}
            RebuildAll();
        }
        public static bool VillageRenewable(Vector3Int p)
        {
            if(p.x>=16&&p.x<25&&p.z>=35&&p.z<43&&p.y>=14&&p.y<=16)return true;
            foreach(var plot in VillagePlots)if(p.x>=plot.x&&p.x<plot.x+10&&p.z>=plot.y&&p.z<plot.y+10&&p.y>=14&&p.y<=25)return true;
            return false;
        }
        public static bool MuseumRenewable(Vector3Int p)=>p.x>=2&&p.x<=12&&p.z>=26&&p.z<=47&&p.y>=15;
        public static bool CabinRenewable(Vector3Int p)=>p.y>=19&&p.y<=30&&p.x>=32&&p.x<=41&&p.z>=34&&p.z<=43||p.y==19&&p.x>=36&&p.x<=37&&p.z>=30&&p.z<=34;
        public static bool CastleRenewable(Vector3Int p)=>MuseumRenewable(p)||p.x>=14&&p.x<=49&&p.z>=14&&p.z<=49&&p.y>=15||p.x>=49&&p.x<=62&&p.z>=2&&p.z<=53&&p.y>=15;
        public Vector3 SafeSpawn()
        {
            Vector3 p=Spawn;
            for(int i=0;i<Height;i++){if(!Blocks.Solid(Get(Vector3Int.FloorToInt(p)))&&!Blocks.Solid(Get(Vector3Int.FloorToInt(p+Vector3.up))))return p;p.y++;}
            return new Vector3(p.x,Height+2,p.z);
        }
        int RegionHeight(int x,int z)
        {
            if(Region==5)return 10;
            if(Region>=11)return 14;
            if(Region>=7)return 10;
            if(Region==6)return 14;
            if(Region==4)return 16;
            if(Region==1)return 14;
            if(x>=26&&x<=35&&z>=8&&z<=22)return 17;
            if(Region==2)
            {
                float d=Vector2.Distance(new Vector2(x,z),new Vector2(32,35));
                return Mathf.Clamp(8+Mathf.FloorToInt(d*.47f)+Mathf.FloorToInt(Mathf.PerlinNoise(x*.13f,z*.13f)*3),8,30);
            }
            return 13+Mathf.FloorToInt(Mathf.PerlinNoise((x+80)*.07f,(z+140)*.07f)*8);
        }
        void GenerateRegion()
        {
            if(Region>=7&&Region<=10){GenerateDeepLayer();return;}
            if(Region==5){UndergroundLava();return;}
            for(int x=0;x<Width;x++)for(int z=0;z<Depth;z++)
            {
                int h=RegionHeight(x,z);
                for(int y=0;y<=h;y++)Put(x,y,z,y==0?Block.Bedrock:Region==3?Block.DarkStone:Region==2?Block.Stone:y==h?Block.Grass:y<h-3?Block.Stone:Block.Dirt);
            }
            if(Region==16)GenerateAlloyRealm();else if(Region>=11)GenerateExcursionTerrain();else if(Region==1)Village();else if(Region==2)CrystalValley();else if(Region==3)ShadowLand();else if(Region==6)CastleShrine();else LavaBamboo();
            Put(27,RegionHeight(27,13)+1,13,Block.Workbench);
        }
        void UndergroundLava()
        {
            for(int x=0;x<Width;x++)for(int z=0;z<Depth;z++)
            {
                bool path=(x>=28&&x<=33)||(z>=21&&z<=25)||(z>=41&&z<=45);
                bool island=Vector2.Distance(new Vector2(x,z),new Vector2(18,24))<6||Vector2.Distance(new Vector2(x,z),new Vector2(45,43))<6;
                bool wall=x<3||z<3||x>60||z>60;
                for(int y=0;y<=10;y++)Put(x,y,z,y==0?Block.Bedrock:y==10&&!path&&!island&&!wall?Block.Lava:Block.DarkStone);
                int ceiling=35+Mathf.FloorToInt(Mathf.PerlinNoise(x*.13f,z*.13f)*4);
                for(int y=ceiling;y<Height;y++)Put(x,y,z,Block.DarkStone);
                if(wall)for(int y=11;y<ceiling;y++)Put(x,y,z,Block.Obsidian);
                if(x%13==0&&z%11==0)for(int y=ceiling-4;y<ceiling;y++)Put(x,y,z,Block.DarkBrick);
            }
            Put(27,11,13,Block.Workbench);
        }
        void Village()
        {
            for(int z=5;z<59;z++)for(int x=28;x<=33;x++)Put(x,14,z,Block.WoodTile);
            for(int x=5;x<59;x++)for(int z=29;z<=32;z++)Put(x,14,z,Block.WoodTile);
            foreach(var p in VillagePlots)House(p.x,p.y);
            for(int x=57;x<92;x++)for(int z=29;z<=32;z++)Put(x,14,z,Block.WoodTile);
            for(int z=5;z<92;z++)for(int x=71;x<=73;x++)Put(x,14,z,Block.WoodTile);
            for(int x=28;x<89;x++)for(int z=70;z<=72;z++)Put(x,14,z,Block.WoodTile);
            for(int x=56;x<70;x++)for(int z=55;z<66;z++){Put(x,14,z,x%4==0?Block.Water:Block.Dirt);if(x%4!=0&&z%2==0)Put(x,15,z,Block.Wheat);}
            // Central well and planted garden beds.
            for(int x=35;x<=39;x++)for(int z=34;z<=38;z++)Put(x,15,z,x==35||x==39||z==34||z==38?Block.StoneBricks:Block.Water);
            foreach(int x in new[]{35,39})foreach(int z in new[]{34,38})for(int y=16;y<=19;y++)Put(x,y,z,Block.Wood);
            for(int x=34;x<=40;x++)for(int z=33;z<=39;z++)Put(x,20,z,Block.Planks);
            for(int x=16;x<25;x++)for(int z=35;z<43;z++){Put(x,14,z,Block.Dirt);if(x%3==0)Put(x,14,z,Block.Water);else if(z%2==0)Put(x,15,z,Block.Wheat);}
            foreach(int x in new[]{4,23,56})foreach(int z in new[]{5,50,57})Tree(x,z,14);
            for(int x=35;x<=38;x++)for(int z=6;z<=9;z++)Put(x,15,z,Block.Gauze);
        }
        void CastleShrine()
        {
            const int left=14,right=49,front=14,back=49;
            for(int floor=0;floor<10;floor++)
            {
                int y=15+floor*6;
                for(int x=left;x<=right;x++)for(int z=front;z<=back;z++)
                {
                    Put(x,y,z,Blocks.PinkCrystal);
                    bool edge=x==left||x==right||z==front||z==back;
                    bool gate=floor==0&&z==front&&x>=23&&x<=40;
                    if(edge)for(int dy=1;dy<6;dy++)Put(x,y+dy,z,gate&&dy<=3?Block.Air:Blocks.PinkCrystal);
                    if(x>=30&&x<=33&&z>=front+1&&z<=back-1)Put(x,y+1,z,Block.RedCarpet);
                }
                foreach(int wx in new[]{20,26,38,44}){Put(wx,y+1,back,Block.Window);Put(wx,y+2,back,Block.Air);Put(wx,y+1,back-1,Block.Curtain);}
                foreach(int wx in new[]{20,26,38,44}){Put(wx,y+2,front,Block.Window);Put(wx,y+3,front,Block.Air);Put(wx,y+2,front+1,Block.Curtain);}
                // Rooms ring the carpet hall. The north-east square stays clear as the lift shaft.
                for(int z=23;z<=43;z++)if(z%8>1)for(int dy=1;dy<=4;dy++)Put(23,y+dy,z,Blocks.PinkCrystal);
                for(int x=17;x<=45;x++)if(x%8>1&&(x<30||x>33))for(int dy=1;dy<=4;dy++)Put(x,y+dy,34,Blocks.PinkCrystal);
                if(floor>0)for(int x=42;x<=45;x++)for(int z=17;z<=20;z++)Put(x,y,z,Block.Air);
                Put(20,y+1,19,Block.CrystalLamp);Put(38,y+1,40,Block.CrystalLamp);
            }
            for(int x=14;x<=49;x++)for(int z=14;z<=49;z++)Put(x,75,z,Blocks.PinkCrystal);
            Put(38,70,34,Block.CrystalLamp);Put(30,70,38,Block.Table);Put(32,70,38,Block.Chair);Put(35,70,41,Block.Bed);Put(35,70,38,Block.Table);Put(35,71,38,Block.Television);
            BuildCastleIsland();
        }
        void House(int x,int z)
        {
            for(int dx=0;dx<10;dx++)for(int dz=0;dz<10;dz++)
            {
                Put(x+dx,14,z+dz,Block.StoneBricks);Put(x+dx,15,z+dz,Block.Planks);
                for(int y=16;y<21;y++)
                {
                    bool wall=dx==0||dx==9||dz==0||dz==9;
                    bool door=dz==0&&(dx==4||dx==5)&&y<19;
                    bool window=dz==9&&(dx==2||dx==4||dx==6)&&y>=17&&y<=18;
                    Put(x+dx,y,z+dz,wall&&!door?(window?Block.Air:dx%9==0&&dz%9==0?Block.Wood:Block.Planks):Block.Air);
                }
                for(int roof=21;roof<=21+Mathf.Min(dx,9-dx)/2;roof++)Put(x+dx,roof,z+dz,Block.Brick);
            }
            Put(x+2,16,z+6,Block.Workbench);Put(x+7,16,z+7,Block.Bookshelf);Put(x+7,17,z+7,Block.Bookshelf);
            Put(x+4,16,z,Block.IronDoor);
            foreach(int dx in new[]{2,4,6}){Put(x+dx,17,z+9,Block.Window);Put(x+dx,17,z+8,Block.Curtain);}
            // Keep the central route from residents' rooms to the front door clear.
            Put(x+7,16,z+6,Block.Cabinet);Put(x+7,16,z+3,Block.Table);Put(x+6,16,z+4,Block.Chair);Put(x+3,16,z+5,Block.Bed);Put(x+4,16,z+7,Block.Sofa);Put(x+7,16,z+2,Block.Table);Put(x+7,17,z+2,Block.Television);
        }
        void Tree(int x,int z,int h)
        {
            for(int y=1;y<6;y++)Put(x,h+y,z,Block.Wood);
            for(int a=-2;a<=2;a++)for(int b=-2;b<=2;b++)for(int y=4;y<=6;y++)if(Mathf.Abs(a)+Mathf.Abs(b)<4)Put(x+a,h+y,z+b,Block.Leaves);
        }
        public static readonly Vector2Int[] VillagePlots={new Vector2Int(9,8),new Vector2Int(9,25),new Vector2Int(9,43),new Vector2Int(42,8),new Vector2Int(42,25),new Vector2Int(42,43),new Vector2Int(62,8),new Vector2Int(78,8),new Vector2Int(62,25),new Vector2Int(78,25),new Vector2Int(62,43),new Vector2Int(78,43),new Vector2Int(9,78),new Vector2Int(42,78),new Vector2Int(62,78),new Vector2Int(78,78)};
        void GenerateAlloyRealm()
        {
            for(int x=2;x<Width-2;x++)for(int z=2;z<Depth-2;z++){Put(x,14,z,Block.DarkStone);if(x>38&&z>28&&x%13<5&&z%13<5)Put(x,14,z,Block.Lava);if(x%11==0&&z%9==0){Put(x,15,z,Blocks.NetherOre);Put(x,16,z,Blocks.NetherOre);}}
            for(int z=5;z<88;z++)for(int x=28;x<=33;x++)Put(x,14,z,Block.DarkBrick);
            for(int i=0;i<12;i++)for(int y=15;y<21+i%4;y++)Put(46+i%4*10,y,10+i/4*22,Block.Obsidian);
        }
        void CrystalValley()
        {
            // A descending stepped path reaches the basin without flight.
            for(int z=22;z<=35;z++)for(int x=29;x<=32;x++)
            {
                int floor=17-(z-22)/2;
                for(int y=1;y<Height;y++)Put(x,y,z,y<=floor?Block.StoneBricks:Block.Air);
            }
            var random=new System.Random(842);
            for(int i=0;i<85;i++)
            {
                int x=random.Next(5,59),z=random.Next(24,59),h=RegionHeight(x,z);
                if(x>=28&&x<=34&&z<=36)continue;
                Block ore=i%3==0?Block.IronOre:Block.Glass;
                for(int y=0;y<random.Next(2,5);y++)Put(x,h+y,z,ore);
                Put(x+1,h,z,ore);Put(x,h-1,z,ore);
            }
            // Fixed veins right beside the entry path make the objective discoverable.
            for(int y=15;y<=20;y++){Put(25,y,23,Block.Glass);Put(35,y,23,Block.IronOre);}
            for(int x=21;x<=23;x++)for(int y=17;y<=19;y++)Put(x,y,24,Block.DreamCrystal);
            for(int x=40;x<=42;x++)for(int y=16;y<=20;y++)Put(x,y,35,Block.DreamCrystal);
            for(int x=43;x<=49;x+=2)for(int y=16;y<=20;y++){Put(x,y,41,Blocks.CrystalOre);Put(x+1,y,41,Block.Stone);}
        }
        void LavaBamboo()
        {
            for(int x=4;x<60;x++)for(int z=4;z<60;z++)
            {
                if(x>39&&z>18&&z<48)Put(x,16,z,Block.Lava);
                if(x<24&&x%3==1&&z%4==1)for(int y=17;y<23+(x+z)%4;y++)Put(x,y,z,Block.Bamboo);
            }
            for(int x=35;x<=38;x++)for(int z=19;z<=25;z++)Put(x,17,z,Block.Obsidian);
            for(int x=27;x<=32;x++)for(int z=23;z<=27;z++)Put(x,16,z,Block.StoneBricks);
        }
        void ShadowLand()
        {
            var random=new System.Random(942);
            for(int i=0;i<55;i++)
            {
                int x=random.Next(5,59),z=random.Next(24,59),h=RegionHeight(x,z);
                for(int y=0;y<random.Next(2,6);y++)Put(x,h+y,z,Block.ShadowCrystal);
                Put(x+1,h,z,Block.ShadowCrystal);
            }
            // The dream dragon descends over these ancient terraces at runtime.
            for(int x=23;x<=40;x++)for(int z=36;z<=49;z++)for(int y=1;y<=22;y++)Put(x,y,z,Block.DarkBrick);
            foreach(int x in new[]{24,39})foreach(int z in new[]{37,48})for(int y=23;y<=31;y++)Put(x,y,z,Block.DarkStone);
            for(int x=24;x<=39;x++){Put(x,32,37,Block.DarkBrick);Put(x,32,48,Block.DarkBrick);}
            for(int x=29;x<=34;x++)for(int y=23;y<=25;y++)Put(x,y,43,Block.ShadowCrystal);
            for(int z=23;z<36;z++)for(int x=29;x<=32;x++)
            {
                int floor=17+(z-23)/2;for(int y=1;y<Height;y++)Put(x,y,z,y<=floor?Block.DarkBrick:Block.Air);
            }
            for(int y=18;y<=21;y++)Put(25,y,22,Block.ShadowCrystal);
        }
    }
}
