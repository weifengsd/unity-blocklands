using UnityEngine;

namespace Blocklands
{
    public partial class VoxelWorld
    {
        // Five standalone, editable destinations. The entrance-to-attraction route is kept open.
        void GenerateExcursionTerrain()
        {
            for (int z=12;z<=34;z++) for (int x=29;x<=34;x++)
                Put(x,14,z,Region==12?Block.Sand:Region==14?Block.WoodTile:Block.QuartzTile);
            for (int x=25;x<=39;x++) for (int z=27;z<=39;z++)
                Put(x,14,z,Region==12?Block.Sand:Region==14?Block.WoodTile:Region==15?Block.QuartzTile:Block.StoneBricks);

            if (Region==11) // Rainbow fairground: tiled plaza, bright pylons and five arches.
            {
                for (int x=17;x<=47;x++) for (int z=24;z<=49;z++)
                    Put(x,14,z,(x+z)%5==0?Blocks.PinkCrystal:(x+z)%3==0?Block.Glass:Block.QuartzTile);
                for(int x=18;x<=46;x+=7)for(int y=15;y<=19;y++){Put(x,y,23,Block.CrystalLamp);Put(x,y,49,Block.CrystalLamp);}
                for(int x=20;x<=44;x+=6){Put(x,19,24,Blocks.PinkCrystal);Put(x,19,48,Blocks.PinkCrystal);}
            }
            else if (Region==12) // The tide has a real shallow water strip behind the beach.
            {
                for(int x=4;x<60;x++)for(int z=29;z<45;z++)Put(x,14,z,Block.Sand);
                for(int x=4;x<60;x++)for(int z=45;z<60;z++){Put(x,14,z,Block.Sand);Put(x,15,z,Block.Water);}
                for(int x=12;x<53;x+=10){Put(x,15,40,Block.Wood);Put(x,16,40,Block.CrystalLamp);}
                for(int z=30;z<42;z+=3){Put(20,15,z,Block.Bamboo);Put(45,15,z,Block.Bamboo);}
            }
            else if (Region==13) // A cloud terrace with a clearly walkable flight runway.
            {
                for(int x=14;x<=50;x++)for(int z=24;z<=47;z++)
                    Put(x,14,z,(x+z)%7==0?Block.Glass:Block.QuartzTile);
                foreach(int x in new[]{17,47})foreach(int z in new[]{24,46})
                    for(int y=15;y<=20;y++)Put(x,y,z,Block.Glass);
                for(int z=24;z<=46;z+=5){Put(20,15,z,Block.CrystalLamp);Put(44,15,z,Block.CrystalLamp);}
            }
            else if (Region==14) // Lantern-lit woodland and a safe central firefly clearing.
            {
                foreach(int x in new[]{8,17,47,55})foreach(int z in new[]{9,24,44,55})Tree(x,z,14);
                for(int x=12;x<=52;x+=8)for(int z=24;z<=52;z+=9){Put(x,15,z,Block.Wood);Put(x,16,z,Block.CrystalLamp);}
                for(int x=25;x<=39;x++)for(int z=27;z<=39;z++)Put(x,14,z,(x+z)%4==0?Block.MossBrick:Block.WoodTile);
            }
            else if (Region==15) // Snow-bright ridge; raised banks never block the main track.
            {
                for(int x=4;x<60;x++)for(int z=4;z<60;z++)Put(x,14,z,Blocks.Snow);
                for(int z=24;z<55;z++)for(int x=10;x<=13;x++)Put(x,15+(z-24)/12,z,Blocks.Snow);
                for(int z=24;z<55;z++)for(int x=50;x<=53;x++)Put(x,15+(z-24)/12,z,Blocks.Snow);
                for(int x=6;x<58;x+=9)for(int z=9;z<56;z+=11)if(x<25||x>40||z>45){Put(x,15,z,Blocks.Snow);Put(x+1,15,z,Blocks.Snow);Put(x,16,z,Blocks.Snow);}
                for(int z=23;z<=53;z+=6){Put(8,15,z,Block.CrystalLamp);Put(56,15,z,Block.CrystalLamp);}
                // A walkable snowy ridge rises behind the cocoa shelter, not beneath the play areas.
                for(int x=12;x<=52;x++)for(int z=57;z<64;z++)
                {
                    int height=Mathf.Max(1,11-Mathf.Abs(x-32)/3-Mathf.Abs(z-60));
                    for(int y=15;y<=14+height;y++)Put(x,y,z,Blocks.Snow);
                }
            }
            Put(27,15,13,Block.Workbench);
        }
    }
}
