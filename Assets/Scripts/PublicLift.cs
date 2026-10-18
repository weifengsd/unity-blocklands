using UnityEngine;
namespace Blocklands
{
    public class VillageControl:MonoBehaviour {public SkyHouse Lift;public int Floor;public bool Shop,Cabin;public Renderer Face;public string Caption=>Shop?"楼层小卖部 · 右键购物":SkyHouse.FloorLabel(Floor)+(Cabin?" · 左键选层":" · 左键呼梯");}
    public partial class SkyHouse
    {
        Transform doorL,doorR;float doorAmount=1;float shoppingAt;int shopFloor;
        bool groundHunt;
        Vector3 RiderWaypoint(int route)=>route==0?home+new Vector3(4.5f,-1,-1):route==1?new Vector3(column.x+side*3.2f,15,home.z-1):route==2?new Vector3(column.x+side*3.2f,15,column.z):column;
        public string BoardingDebug {get {string s="";foreach(var r in riders)s+="["+r.Route+","+r.Boarded+","+r.Root.position+"]";return s;}}
        bool HuntOnGround(bool alarm,Vector3 player){if(alarm&&player.y<23&&floor==-1&&BoardedCount==4){SetDoors(true);if(!DoorOpen)return true;groundHunt=true;Phase="按底层按钮后，出梯追赶";foreach(var r in riders){if(r.Resting)continue;r.Body.enabled=true;Vector3 p=r.Root.position-column;Vector3 target=Mathf.Abs(p.x)<2&&Mathf.Abs(p.z)<2?RiderWaypoint(2):player;Vector3 d=target-r.Root.position;d.y=0;r.Body.SimpleMove(d.normalized*2.8f);if(d.sqrMagnitude>.01f)r.Root.rotation=Quaternion.LookRotation(d);if(game.CanReachPlayer(r.Root.position+Vector3.up,1.35f))game.Damage(.5f,"被村民家人追上");}return true;}if(groundHunt){foreach(var r in riders){if(r.Resting)continue;r.Boarded=false;r.Route=2;}groundHunt=false;}return false;}
        public bool DoorOpen=>doorAmount>.98f;public float CarY=>car.position.y;
        public Vector3 CarCenter=>car.position+Vector3.up*.2f;
        bool BringDownForBoarding(){if(BoardedCount==4||(floor==-1&&Mathf.Abs(CarY-FloorHeight(-1))<.02f))return false;Phase="家人在底层按按钮呼梯";MoveNpcLift(-1);return true;}
        public bool PlayerInside {get{Vector3 p=game.Player.transform.position-car.position;return Mathf.Abs(p.x)<1.35f&&Mathf.Abs(p.z)<1.35f&&p.y>-.2f&&p.y<2.7f;}}
        public Vector3 Kiosk(int f)=>new Vector3(column.x+side*4.7f,FloorHeight(f)+1,column.z+1.3f);
        void Control(Transform parent,Vector3 p,int f,bool shop,bool cabin=false){var b=Box(parent,"Interactive button",p,new Vector3(.65f,.48f,.3f),Mat(shop?new Color(.36f,.8f,.4f):new Color(.95f,.7f,.2f)),true);var c=b.gameObject.AddComponent<VillageControl>();c.Lift=this;c.Floor=f;c.Shop=shop;c.Cabin=cabin;c.Face=b.GetComponent<Renderer>();if(!shop)floorButtons.Add(c);}
        void BuildPublicLift()
        {
            var glass=new Material(Resources.Load<Shader>("GlassPane"));glass.color=new Color(.45f,.85f,1,.17f);materials.Add(glass);
            Box(car,"Glass back",new Vector3(-side*1.5f,1.55f,0),new Vector3(.07f,3,3),glass,true);
            Box(car,"Glass roof",new Vector3(0,3.08f,0),new Vector3(3,.07f,3),glass,true);
            foreach(float z in new[]{-1.5f,1.5f})Box(car,"Glass side",new Vector3(0,1.55f,z),new Vector3(3,3,.07f),glass,true);
            doorL=Box(car,"Sliding glass door L",new Vector3(side*1.5f,1.55f,-2),new Vector3(.07f,3,1.5f),glass,true);doorR=Box(car,"Sliding glass door R",new Vector3(side*1.5f,1.55f,2),new Vector3(.07f,3,1.5f),glass,true);
            Box(car,"Number panel",new Vector3(-side*1.38f,1.4f,0),new Vector3(.08f,2.2f,2.8f),ShopMat(new Color(.17f,.23f,.28f)));
            int[] buttons={0,1,2,3,4,5,6,-1,RoofFloor};for(int i=0;i<buttons.Length;i++){int f=buttons[i];Vector3 p=new Vector3(-side*1.23f,2-i/3*.6f,(i%3-1)*.85f);var b=Box(car,"Floor "+FloorLabel(f),p,new Vector3(.16f,.42f,.65f),ShopMat(new Color(.6f,.64f,.7f)),true);var c=b.gameObject.AddComponent<VillageControl>();c.Lift=this;c.Floor=f;c.Cabin=true;c.Face=b.GetComponent<Renderer>();floorButtons.Add(c);Sign(car,p+Vector3.right*side*.09f,FloorLabel(f),.08f,-side*90);car.GetChild(car.childCount-1).GetComponent<TextMesh>().color=new Color(.08f,.1f,.13f);}
            for(int f=-1;f<=RoofFloor;f++){
                Control(transform,new Vector3(column.x+side*2.25f,FloorHeight(f)+1,column.z-1.7f),f,false);
                if(f<0||f==RoofFloor)continue;
                Vector3 shop=Kiosk(f);Box(transform,"Shop counter",shop-Vector3.up*.55f,new Vector3(1.7f,.8f,.75f),deck,true);Control(transform,shop+Vector3.up*.05f,f,true);
                Box(transform,"Shopkeeper body",shop+new Vector3(0,.2f,.8f),new Vector3(.5f,.8f,.35f),iron);Box(transform,"Shopkeeper head",shop+new Vector3(0,.8f,.8f),Vector3.one*.45f,skin);
                foreach(float x in new[]{-.1f,.1f})Box(transform,"Shopkeeper eyes",shop+new Vector3(x,.86f,.56f),Vector3.one*.05f,hair);
                for(int i=0;i<3;i++)Box(transform,"Shelf stock",shop+new Vector3((i-1)*.5f,.1f,.25f),Vector3.one*.23f,Mat(Color.HSVToRGB((f*.14f+i*.2f)%1,.5f,.85f)));
                BuildStoreRoom(f);
            }
            shoppingAt=Time.time+25;shopFloor=Mathf.RoundToInt(home.z)%ShopFloors;
        }
        void SetDoors(bool open){doorAmount=Mathf.MoveTowards(doorAmount,open?1:0,Time.deltaTime*2);doorL.localPosition=new Vector3(side*1.5f,1.55f,-.75f-doorAmount*1.5f);doorR.localPosition=new Vector3(side*1.5f,1.55f,.75f+doorAmount*1.5f);}
    }
}
