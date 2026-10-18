using UnityEngine;

namespace Blocklands
{
    public sealed class ExcursionScenery : MonoBehaviour
    {
        RoyalArt art;readonly System.Collections.Generic.List<Transform> turning=new System.Collections.Generic.List<Transform>();
        public int LandmarkCount {get;private set;}
        public void Initialize(int region)
        {
            art=gameObject.AddComponent<RoyalArt>();
            Color gold=new Color(1,.78f,.36f),pink=new Color(.94f,.54f,.77f),ice=new Color(.94f,.98f,1),wood=new Color(.53f,.32f,.18f);
            if(region==11)
            {
                foreach(float x in new[]{27f,37f}){art.Piece("彩虹游乐场入口门柱",new Vector3(x,16.8f,18),new Vector3(.4f,3.6f,.4f),pink);art.Piece("门柱星灯",new Vector3(x,18.7f,18),Vector3.one*.72f,gold,PrimitiveType.Sphere);}
                art.Piece("彩虹入口拱门",new Vector3(32,18.65f,18),new Vector3(10,.3f,.55f),gold);LandmarkCount++;
                foreach(float x in new[]{11f,54f}){art.Piece("彩色小卖亭",new Vector3(x,16.05f,34),new Vector3(4,2.1f,3.2f),x<30?pink:new Color(.57f,.73f,.95f));art.Taper("波纹帐篷顶",new Vector3(x,17.75f,34),new Vector3(5.1f,1.1f,4.2f),gold,.42f);for(int i=0;i<5;i++)art.Piece("亭子彩色气球",new Vector3(x+(i-2)*.68f,19.1f,32.6f),Vector3.one*.42f,Color.HSVToRGB(i*.19f,.62f,1),PrimitiveType.Sphere);LandmarkCount++;}
            }
            if(region==12)
            {
                for(int i=0;i<4;i++){float x=i%2==0?9:55,z=24+i*9;var trunk=art.Piece("海湾棕榈树",new Vector3(x,18,z),new Vector3(.34f,5.8f,.34f),wood,PrimitiveType.Cylinder);trunk.localRotation=Quaternion.Euler(0,0,i%2==0?-8:8);for(int f=0;f<6;f++){float a=f*Mathf.PI/3;var leaf=art.Piece("棕榈扇叶",new Vector3(x+Mathf.Sin(a)*1.2f,21,z+Mathf.Cos(a)*1.2f),new Vector3(.6f,.16f,2.4f),new Color(.26f,.62f,.35f));leaf.localRotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,17);}LandmarkCount++;}
                art.Piece("海边木栈道",new Vector3(32,15.08f,43),new Vector3(16,.2f,4),wood);art.Piece("沙滩瞭望棚",new Vector3(32,17.1f,55),new Vector3(4.7f,3.9f,3.5f),new Color(.72f,.56f,.37f));art.Taper("瞭望棚茅草屋顶",new Vector3(32,19.3f,55),new Vector3(5.6f,.9f,4.5f),new Color(.83f,.73f,.5f),.65f);LandmarkCount++;
            }
            if(region==13)
            {
                foreach(float x in new[]{10f,54f}){art.Piece("追风风车塔",new Vector3(x,18.3f,39),new Vector3(.65f,6.6f,.65f),new Color(.75f,.82f,.91f),PrimitiveType.Cylinder);var rotor=new GameObject("旋转的风车叶片").transform;rotor.SetParent(transform,false);rotor.localPosition=new Vector3(x,21.55f,38.55f);for(int b=0;b<4;b++){var blade=art.Piece("云谷风车叶",new Vector3(x,21.55f,38.55f),new Vector3(.28f,3.2f,.14f),b%2==0?pink:ice);blade.SetParent(rotor,true);blade.localRotation=Quaternion.Euler(0,0,b*90);}turning.Add(rotor);LandmarkCount++;}
                for(int i=0;i<5;i++){float x=12+i*10,z=i%2==0?50:54;art.Piece("漂浮云岛",new Vector3(x,20+i%2*1.2f,z),new Vector3(4.5f,1.4f,3),ice,PrimitiveType.Sphere);art.Piece("云岛风铃",new Vector3(x,18.8f+i%2*1.2f,z),new Vector3(.18f,.8f,.18f),gold);}LandmarkCount++;
            }
            if(region==14)
            {
                foreach(float x in new[]{9f,22f,43f,55f}){float z=x<30?45:53;art.Piece("荧光蘑菇茎",new Vector3(x,15.9f,z),new Vector3(.42f,1.8f,.42f),ice,PrimitiveType.Cylinder);art.Taper("发光蘑菇伞",new Vector3(x,17.1f,z),new Vector3(2.2f,.65f,2.2f),x<30?pink:new Color(.63f,.72f,1),.25f);LandmarkCount++;}
                art.Piece("听雨长椅",new Vector3(31,15.4f,54),new Vector3(5,.75f,1.3f),wood);foreach(float x in new[]{-2f,2f})art.Piece("长椅灯笼",new Vector3(31+x,16.7f,54),new Vector3(.4f,.6f,.4f),gold);LandmarkCount++;
            }
            if(region==15)
            {
                foreach(float x in new[]{7f,17f,47f,57f})foreach(float z in new[]{13f,52f}){art.Piece("雪松树干",new Vector3(x,17,z),new Vector3(.36f,4,.36f),wood,PrimitiveType.Cylinder);art.Taper("覆雪松冠",new Vector3(x,19.4f,z),new Vector3(2.6f,4.2f,2.6f),new Color(.34f,.55f,.55f),.1f);art.Taper("树梢积雪",new Vector3(x,21,z),new Vector3(1.9f,1.4f,1.9f),ice,.12f);LandmarkCount++;}
                art.Piece("山谷热可可小木屋",new Vector3(45,16.6f,52),new Vector3(4.6f,3.2f,4),wood);art.Taper("木屋厚雪屋顶",new Vector3(45,18.5f,52),new Vector3(5.4f,1,4.8f),ice,.3f);LandmarkCount++;
            }
        }
        void Update(){for(int i=0;i<turning.Count;i++)if(turning[i]!=null)turning[i].localRotation=Quaternion.Euler(0,0,Time.time*(i%2==0?35:-31));}
    }

    public partial class Sandbox
    {
        public ExcursionScenery ExcursionScenery {get;private set;}
        void BuildExcursionScenery()
        {
            ExcursionScenery=null;if(World.Region<11||World.Region>15)return;
            var root=new GameObject(VoxelWorld.RegionNames[World.Region]+"远景设施");root.transform.SetParent(villagePeople.transform);
            ExcursionScenery=root.AddComponent<ExcursionScenery>();ExcursionScenery.Initialize(World.Region);
        }
    }
}
