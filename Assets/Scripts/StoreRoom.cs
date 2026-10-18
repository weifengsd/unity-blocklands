using UnityEngine;
namespace Blocklands
{
    public partial class SkyHouse
    {
        Material shopWood,shopWall,priceMaterial;Material[] stockColors;Font priceFont;
        public Vector3 StoreOrigin(int f)=>new Vector3(column.x+side*3.5f,FloorHeight(f),column.z+2.3f);
        public bool AtShop(int f,Vector3 p){if(Vector3.Distance(p,Kiosk(f))<=5)return true;Vector3 q=p-StoreOrigin(f);return Mathf.Abs(q.x)<4.3f&&q.z>0&&q.z<8.1f&&q.y>-.2f&&q.y<4.5f;}
        void BuildStoreRoom(int f)
        {
            if(shopWood==null){shopWood=ShopMat(new Color(.49f,.31f,.18f));shopWall=ShopMat(new Color(.84f,.82f,.71f));stockColors=new Material[6];for(int i=0;i<6;i++)stockColors[i]=ShopMat(Color.HSVToRGB(i/6f,.48f,.84f));}
            var room=new GameObject("商店房间 "+(f+1)).transform;room.SetParent(transform);room.position=StoreOrigin(f);
            Box(room,"大商店地板",new Vector3(0,-.15f,4),new Vector3(8.4f,.3f,8),shopWood,true);
            Box(room,"入口连接台",new Vector3(0,-.15f,-.35f),new Vector3(3,.3f,.9f),shopWood,true);
            foreach(float x in new[]{-4.2f,4.2f})Box(room,"商店侧墙",new Vector3(x,2.4f,4),new Vector3(.16f,4.8f,8),shopWall,true);
            Box(room,"后墙",new Vector3(0,2.4f,8),new Vector3(8.4f,4.8f,.16f),shopWall,true);
            foreach(float x in new[]{-2.85f,2.85f})Box(room,"门边墙",new Vector3(x,2.4f,0),new Vector3(2.7f,4.8f,.16f),shopWall,true);
            Box(room,"门楣",new Vector3(0,4.1f,0),new Vector3(3,1.4f,.16f),shopWood,true);Box(room,"屋顶",new Vector3(0,4.85f,4),new Vector3(8.6f,.18f,8.3f),shopWood,true);
            Box(room,"更大的柜台",new Vector3(0,.6f,6.7f),new Vector3(4.8f,1.2f,.9f),shopWood,true);
            Box(room,"店员身体",new Vector3(0,1.1f,7.5f),new Vector3(.6f,.85f,.36f),stockColors[f%stockColors.Length]);Box(room,"店员头",new Vector3(0,1.85f,7.5f),Vector3.one*.55f,shopWall);Box(room,"店员头发",new Vector3(0,2.12f,7.5f),new Vector3(.59f,.13f,.59f),shopWood);foreach(float x in new[]{-.14f,.14f}){Box(room,"店员眼睛",new Vector3(x,1.9f,7.215f),Vector3.one*.065f,shopWood);Box(room,"店员腿",new Vector3(x,.4f,7.5f),new Vector3(.24f,.8f,.28f),shopWood);}foreach(float x in new[]{-.43f,.43f})Box(room,"店员手臂",new Vector3(x,1.05f,7.5f),new Vector3(.18f,.8f,.24f),shopWall);
            Control(room,new Vector3(0,1.35f,6.25f),f,true);
            for(int row=0;row<3;row++)for(int sideIndex=0;sideIndex<2;sideIndex++){
                int rackId=row*2+sideIndex;var rack=new GameObject("货架 "+rackId).transform;rack.SetParent(room,false);rack.localPosition=new Vector3(sideIndex==0?-2.6f:2.6f,0,1.6f+row*1.55f);
                foreach(float x in new[]{-.76f,.76f})Box(rack,"架柱",new Vector3(x,1.3f,0),new Vector3(.1f,2.6f,.7f),shopWood);
                for(int tier=0;tier<3;tier++)Box(rack,"层板",new Vector3(0,.45f+tier*.9f,0),new Vector3(1.6f,.1f,.8f),shopWood);
                var collider=rack.gameObject.AddComponent<BoxCollider>();collider.center=Vector3.up*1.3f;collider.size=new Vector3(1.6f,2.6f,.8f);var c=rack.gameObject.AddComponent<VillageControl>();c.Lift=this;c.Floor=f;c.Shop=true;
                for(int j=0;j<4;j++){int index=rackId*4+j;var offer=Sandbox.Offers[f][index];Vector3 p=new Vector3((j%2-.5f)*.72f,.76f+j/2*.9f,0);Box(rack,Blocks.Names[(int)offer.Item],p,Vector3.one*.4f,stockColors[index%6]);Sign(rack,new Vector3(p.x,p.y-.22f,-.44f),offer.Price+"元",.07f);}
            }
            Sign(room,new Vector3(0,3.45f,-.12f),(f+1)+"层 · 村庄百货",.15f);
            var lamp=new GameObject("商店灯");lamp.transform.SetParent(room,false);lamp.transform.localPosition=new Vector3(0,3.8f,3.8f);var light=lamp.AddComponent<Light>();light.type=LightType.Point;light.range=10;light.intensity=1.5f;light.color=new Color(1,.86f,.65f);
            StaticBatchingUtility.Combine(room.gameObject);
        }
        Material ShopMat(Color c){var m=new Material(Resources.Load<Shader>("ActorSurface"));m.color=c;materials.Add(m);return m;}
        void UpdatePriceFont(Font changed){if(changed==priceFont&&priceMaterial!=null)priceMaterial.mainTexture=priceFont.material.mainTexture;}
        void Sign(Transform parent,Vector3 p,string words,float size,float yaw=0){if(priceMaterial==null){priceFont=Resources.Load<Font>("NotoSansCJKsc-Regular");priceFont.RequestCharactersInTexture("0123456789元层 · 村庄百货",32);priceMaterial=new Material(Resources.Load<Shader>("WorldText"));priceMaterial.mainTexture=priceFont.material.mainTexture;materials.Add(priceMaterial);Font.textureRebuilt+=UpdatePriceFont;}var obj=new GameObject("标价牌");obj.transform.SetParent(parent,false);obj.transform.localPosition=p;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);var t=obj.AddComponent<TextMesh>();t.font=priceFont;t.fontSize=32;t.text=words;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.95f,.8f,.35f);obj.GetComponent<Renderer>().sharedMaterial=priceMaterial;}
    }
}
