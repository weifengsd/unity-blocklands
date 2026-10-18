using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    void BuildDeepNeighbourhood(Transform room,OwnedHome h,int f,RoyalArt art)
    {
        Color pink=new Color(.88f,.61f,.76f),green=new Color(.36f,.67f,.45f),wood=new Color(.55f,.38f,.25f);if(f==4){art.Piece("地下公园小树干",new Vector3(8.7f,1.2f,3.8f),new Vector3(.25f,2.4f,.25f),wood);art.Piece("小树圆树冠",new Vector3(8.7f,2.7f,3.8f),new Vector3(1.7f,1.5f,1.7f),green,PrimitiveType.Sphere);art.Piece("花园水景",new Vector3(5,.17f,4),new Vector3(3.3f,.3f,1.2f),new Color(.31f,.71f,.87f));for(int i=0;i<6;i++)art.Piece("彩色小鱼",new Vector3(3.6f+i*.5f,.36f,4),new Vector3(.2f,.1f,.09f),Color.HSVToRGB(i*.16f,.6f,.9f),PrimitiveType.Sphere);var slide=MakeDeepObject(room,h,f,"slide",new Vector3(1.7f,0,3.2f),"公园小滑梯 · G坐上",new Vector3(1,.2f,2.4f),pink);for(int i=0;i<5;i++)art.Piece("滑梯小台阶",new Vector3(1.7f,.22f+i*.38f,1.6f+i*.2f),new Vector3(1,.18f,.35f),wood);var slope=art.Piece("光滑滑道",new Vector3(1.7f,1,3.2f),new Vector3(.95f,.15f,2.6f),pink);slope.localRotation=Quaternion.Euler(38,0,0);}
        if(f==6){MakeDeepObject(room,h,f,"computer",new Vector3(2.6f,0,2),"阅读室小电脑 · G换故事",new Vector3(1.5f,.8f,1),wood);art.Piece("电脑屏幕",new Vector3(2.6f,1.3f,1.9f),new Vector3(1,.65f,.08f),new Color(.25f,.56f,.73f));for(int i=0;i<5;i++)art.Piece("键盘按键",new Vector3(2.25f+i*.17f,.93f,2.35f),new Vector3(.12f,.03f,.15f),Color.white);}
        if(f==7){MakeDeepObject(room,h,f,"bounce",new Vector3(2.3f,0,3.7f),"室内弹跳床 · G玩10秒",new Vector3(1.8f,.35f,1.8f),new Color(.38f,.59f,.82f));foreach(float x in new[]{1.5f,3.1f})foreach(float z in new[]{2.9f,4.5f})art.Piece("弹跳床护栏",new Vector3(x,1.3f,z),new Vector3(.08f,2.4f,.08f),pink);}
        if(f==9){MakeDeepObject(room,h,f,"eat",new Vector3(2.6f,0,3.5f),"地下小咖啡馆 · G吃点心",new Vector3(2,.8f,1.4f),wood);art.Piece("小咖啡馆遮阳棚",new Vector3(2.6f,2.8f,3.5f),new Vector3(2.5f,.12f,1.7f),pink);for(int i=0;i<3;i++)art.Piece("咖啡馆草莓蛋糕",new Vector3(2+i*.6f,1.05f,3.5f),new Vector3(.4f,.25f,.4f),new Color(.95f,.57f,.68f),PrimitiveType.Sphere);}
    }
}}
