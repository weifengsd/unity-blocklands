using UnityEngine;
namespace Blocklands {
 public partial class VoxelWorld {
  void GenerateDeepLayer(){
   if(Region==10){for(int x=0;x<Width;x++)for(int z=0;z<Depth;z++){for(int y=0;y<10;y++)Put(x,y,z,Block.Stone);Put(x,10,z,Block.Grass);if(x%9==0&&z%11==0)Put(x,11,z,Block.CrystalLamp);}Put(27,11,13,Block.Workbench);return;}
   Block rock=Region==7?Blocks.DenseRock:Blocks.StarRock;
   for(int x=0;x<Width;x++)for(int z=0;z<Depth;z++){
    for(int y=0;y<=10;y++)Put(x,y,z,y==0?rock:y==10&&Region==9&&x>8&&x<25&&z>20&&z<48?Block.Water:rock);
    if(Region!=9)for(int y=32;y<=35;y++)Put(x,y,z,rock);
    if(x<2||z<2||x>61||z>61)for(int y=11;y<=31;y++)Put(x,y,z,rock);
    if(x%9==0&&z%11==0){Put(x,11,z,Block.CrystalLamp);Put(x,12,z,Region==7?Block.IronOre:Blocks.PinkCrystal);}
   }
   Put(27,11,13,Block.Workbench);
  }
  void BuildCastleIsland(){
   // Twenty museum floors; windows and curtains use the same fixtures as village houses.
   for(int floor=0;floor<20;floor++){int y=15+floor*3;
    for(int x=2;x<=12;x++)for(int z=26;z<=47;z++){Put(x,y,z,Block.QuartzTile);bool edge=x==2||x==12||z==26||z==47;bool gate=floor==0&&z==26&&x>=5&&x<=9;if(edge){Put(x,y+1,z,gate?Block.Air:Blocks.PinkCrystal);Put(x,y+2,z,gate?Block.Air:Blocks.PinkCrystal);}}
    foreach(int wx in new[]{4,7,10}){Put(wx,y+1,47,Block.Window);Put(wx,y+2,47,Block.Air);Put(wx,y+1,46,Block.Curtain);}
    if(floor>0)for(int x=9;x<=11;x++)for(int z=28;z<=30;z++)Put(x,y,z,Block.Air);
    for(int z=32;z<=44;z+=6){Put(5,y+1,z,Block.Table);Put(5,y+2,z,(floor+z)%3==0?Block.DreamCrystal:(floor+z)%3==1?Block.Obsidian:Block.CrystalLamp);Put(9,y+1,z,Block.Table);Put(9,y+2,z,(floor+z)%2==0?Block.ShadowCrystal:Blocks.PinkCrystal);}
   }
   for(int x=2;x<=12;x++)for(int z=26;z<=47;z++)Put(x,75,z,Blocks.PinkCrystal);
   for(int x=18;x<=47;x++)for(int z=3;z<=11;z++)if(z==7||x==31||x==32)Put(x,14,z,Block.QuartzTile);
   for(int x=53;x<=59;x++)for(int z=25;z<=31;z++)Put(x,14,z,(x==53||x==59||z==25||z==31)?Blocks.PinkCrystal:Block.Water);
   foreach(int x in new[]{4,10,19,44,55,59})foreach(int z in new[]{5,10,53,58})if(!(x<13&&z>24)&&!((x==19||x==44)&&z<=10)){Tree(x,z,14);if(x%2==0)Put(Mathf.Min(62,x+2),15,z,Block.Bed);}
   SmallIslandBuilding(49,2,14,15,"hospital");SmallIslandBuilding(51,43,11,10,"police");SmallIslandBuilding(3,3,10,9,"shop");
   for(int x=56;x<=60;x++)for(int z=48;z<=52;z++)if(x==56||x==60||z==48||z==52){Put(x,16,z,x==58&&z==48?Block.PrisonBars:Block.StoneBricks);Put(x,17,z,x==58&&z==48?Block.PrisonBars:Block.StoneBricks);}Put(58,16,51,Block.Bed);
  }
  void SmallIslandBuilding(int sx,int sz,int w,int d,string kind){int height=kind=="hospital"?8:4;for(int x=sx;x<sx+w;x++)for(int z=sz;z<sz+d;z++){Put(x,15,z,Block.QuartzTile);bool edge=x==sx||x==sx+w-1||z==sz||z==sz+d-1;bool door=z==sz&&(x==sx+w/2||x==sx+w/2+1);if(edge)for(int y=16;y<15+height;y++)Put(x,y,z,door&&y<=18?Block.Air:Blocks.PinkCrystal);Put(x,15+height,z,Blocks.PinkCrystal);if(kind=="hospital"&&x>=sx+3&&x<=sx+w-4&&z==sz+d/2&&x%3==0)Put(x,16,z,Block.Bed);if(kind=="shop"&&x==sx+w/2&&z==sz+d/2)Put(x,16,z,Block.Table);} }
 }
 public partial class Sandbox {
  float depthWork;Vector3Int depthCell;int depthRegion=-1;
  public int DeepestVisit {get;private set;}=5;
  public bool MineDepth(Vector3Int cell,float work){
   int region=World.Region;Block need=region==7?Blocks.IronDrill:region==8?Blocks.StarDrill:Block.CrystalHammer;
   if(region==10)need=Blocks.CombinedStarPick;
   if(region==9)need=Blocks.CombinedStarPick;
   if((Selected!=(int)need&&Selected!=(int)Blocks.CombinedStarPick)||Inventory[Selected]<1){depthWork=0;Notice="这一层需要"+Blocks.Names[(int)need]+"；E打开合成图鉴查看配方。";return false;}
   if(depthCell!=cell||depthRegion!=region){depthWork=0;depthCell=cell;depthRegion=region;}
   depthWork+=work;Notice="开凿 "+Mathf.Min(100,Mathf.RoundToInt(depthWork/3*100))+"% · 持续左键";
   if(depthWork<3)return false;
   if(!World.OpenDepthHole(cell))return false;depthWork=0;
   Inventory[(int)Blocks.BedrockShard]=Mathf.Min(9999,Inventory[(int)Blocks.BedrockShard]+1);Notice="洞口打通了！走进洞口继续向下。获得基岩碎片。";return true;
  }
  public bool EnterLowerLayer(){if(World.Region==10){Player.Teleport(World.SafeSpawn());return false;}int next=World.Region==5?7:World.Region==7?8:World.Region==8?9:World.Region==9?10:5;DeepestVisit=Mathf.Max(DeepestVisit,next);return TravelTo(next);}
 }
}
