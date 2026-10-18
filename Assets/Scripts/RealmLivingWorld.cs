using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public enum RealmUseKind { Door, Food, Bed, Book, Television, Pool, Harvest, Craft, Lantern, Hospital, Police, LostProperty, PlayRide }
    public class RealmUsePoint : MonoBehaviour
    {
        public Sandbox Game;public RealmUseKind Kind;public string Caption;public Transform Target,Decoration;public bool Active;public int Variant;
        public void Use(){if(Game!=null)Game.UseRealmPoint(this);}
    }
    public class RealmMeshLifetime : MonoBehaviour
    {
        public Mesh Mesh;public Material Material;public Material[] Materials;
        void OnDestroy(){if(Mesh!=null)Destroy(Mesh);if(Material!=null)Destroy(Material);if(Materials!=null)foreach(var m in Materials)Destroy(m);}
    }
    public class RealmSteamMotion : MonoBehaviour
    {
        public Sandbox Game;Vector3 origin,scale;Material material;float offset;
        void Start(){origin=transform.localPosition;scale=transform.localScale;offset=origin.x*.31f;material=GetComponent<Renderer>().sharedMaterial;}
        void Update(){if(Game==null||Game.Paused)return;float t=Mathf.Repeat(Time.time+offset,3);transform.localPosition=origin+Vector3.up*t*.35f;transform.localScale=scale*(.5f+t*.35f);material.color=new Color(.92f,.96f,1,.14f*(1-t/3));}
    }
    public class RealmResidentLife : MonoBehaviour
    {
        public Sandbox Game;public string Job;Vector3 home,farm;Transform[] arms;float phase;Transform wateringCan;
        public void AssignFarm(Vector3 target){Job="种田";farm=target;if(wateringCan==null){var art=GetComponent<RoyalArt>();if(art!=null)wateringCan=art.Piece("农民浇水壶",new Vector3(.5f,.72f,.3f),new Vector3(.28f,.23f,.3f),new Color(.33f,.57f,.62f));}}
        public void Initialize(Sandbox game,string job){Game=game;Job=job;home=transform.position;phase=home.x*.31f;if(!name.Contains("电视"))NpcNavigation.Ensure(transform);var list=new List<Transform>();foreach(var t in GetComponentsInChildren<Transform>())if(t.name=="手臂")list.Add(t);arms=list.ToArray();}
        void Update(){if(Game==null||Game.Paused)return;float t=Time.time+phase;
            if(Job=="散步")NpcNavigation.Ensure(transform).MoveTo(home+new Vector3(Mathf.Sin(t*.3f)*2,0,Mathf.Cos(t*.3f)*1.5f),1.2f,Time.deltaTime);
            if(Job=="种田"){float work=Mathf.Repeat(t,24);NpcNavigation.Ensure(transform).MoveTo(work<14?farm:home,1.5f,Time.deltaTime);if(wateringCan!=null)wateringCan.gameObject.SetActive(work>=7&&work<14);}
            Vector3 look=Game.Player.transform.position-transform.position;look.y=0;if(look.sqrMagnitude<70&&look.sqrMagnitude>.1f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look),Time.deltaTime*2);
            for(int i=0;i<arms.Length;i++)arms[i].localRotation=Quaternion.Euler(Job=="种田"?-40+Mathf.Sin(t*4)*18:Job=="唱歌"?-35+Mathf.Sin(t*3)*10:Mathf.Sin(t*2+i)*18,0,0);
        }
    }
    public class RealmGhostLife : MonoBehaviour
    {
        public Sandbox Game;Vector3 home;float phase;
        public void Initialize(Sandbox game){Game=game;home=transform.position;phase=home.x*.3f;}
        void Update(){if(Game==null||Game.Paused)return;float t=Time.time+phase;transform.position=home+new Vector3(Mathf.Sin(t*.5f)*3,Mathf.Sin(t*1.4f)*.5f,Mathf.Cos(t*.5f)*2);transform.Rotate(0,Time.deltaTime*12,0);}
    }
    public class RealmDragonLife : MonoBehaviour
    {
        public Sandbox Game;DragonSculpt sculpt;float nextBreath;AudioSource voice;AudioClip growl;
        void Start(){sculpt=GetComponent<DragonSculpt>();voice=gameObject.AddComponent<AudioSource>();voice.volume=.1f;voice.spatialBlend=1;voice.minDistance=2;voice.maxDistance=35;var samples=new float[26460];var rng=new System.Random(13);for(int i=0;i<samples.Length;i++){float t=i/22050f,fade=Mathf.Sin(Mathf.PI*i/samples.Length);samples[i]=(Mathf.Sin(t*2*Mathf.PI*47)*.5f+(float)(rng.NextDouble()-.5)*.12f)*fade;}growl=AudioClip.Create("恶龙低沉咆哮",samples.Length,1,22050,false);growl.SetData(samples,0);}
        void Update(){if(Game==null||Game.Paused||sculpt==null)return;sculpt.LeftWing.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.7f)*8);sculpt.RightWing.localRotation=Quaternion.Euler(0,0,-Mathf.Sin(Time.time*1.7f)*8);
            if(!Game.InHell||Time.time<nextBreath)return;nextBreath=Time.time+4;
            if(Vector3.Distance(Game.Player.transform.position,transform.position)<13){voice.PlayOneShot(growl);Game.Damage(.5f,"恶龙的火息扫过地面，快躲到岩柱后面");}}
        void OnDestroy(){if(growl!=null)Destroy(growl);}
    }

    public partial class Sandbox
    {
        readonly List<RealmUsePoint> realmPoints=new List<RealmUsePoint>();
        Transform realmFood;float realmMealRemaining,realmBathRemaining;bool realmResting;Vector3 realmRestExit;int realmBookPage;
        public int RealmMealsEaten {get;private set;}
        public int RealmLanternsLit {get;private set;}
        public bool RealmLifeLocked=>realmMealRemaining>0||realmBathRemaining>0||realmResting||PlanetRideActive;
        public bool RealmMealActive=>realmMealRemaining>0;
        public bool RealmBathActive=>realmBathRemaining>0;

        void BuildRealmGround(Transform root,string name,float y,float radius,Color color,Rect opening)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();const float step=4;
            for(float x=32-radius;x<32+radius;x+=step)for(float z=32-radius;z<32+radius;z+=step)
            {
                Vector2 mid=new Vector2(x+step/2,z+step/2);if((mid-new Vector2(32,32)).sqrMagnitude>radius*radius||opening.Contains(mid))continue;
                int n=vertices.Count;vertices.Add(new Vector3(x,y,z));vertices.Add(new Vector3(x,y,z+step));vertices.Add(new Vector3(x+step,y,z+step));vertices.Add(new Vector3(x+step,y,z));
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var obj=new GameObject(name);obj.transform.SetParent(root,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            var material=new Material(Resources.Load<Shader>("ActorSurface")){color=color};obj.AddComponent<MeshRenderer>().sharedMaterial=material;obj.AddComponent<MeshCollider>().sharedMesh=mesh;
            var lifetime=obj.AddComponent<RealmMeshLifetime>();lifetime.Mesh=mesh;lifetime.Material=material;
        }

        RealmUsePoint AddRealmPoint(Transform part,RealmUseKind kind,string caption,Transform target=null)
        {
            var point=part.gameObject.AddComponent<RealmUsePoint>();point.Game=this;point.Kind=kind;point.Caption=caption;point.Target=target??part;
            if(part.GetComponent<Collider>()==null){var collider=part.gameObject.AddComponent<BoxCollider>();collider.isTrigger=true;}
            realmPoints.Add(point);return point;
        }

        void AddRealmFood(RoyalArt art,Vector3 position,string caption,Color color)
        {
            var food=new GameObject(caption).transform;food.SetParent(art.transform,false);food.localPosition=position;
            BuildRealmFoodModel(food.gameObject.AddComponent<RoyalArt>(),caption);
            var collider=food.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(.65f,.6f,.6f);collider.isTrigger=true;
            AddRealmPoint(food,RealmUseKind.Food,"G 拿起来吃："+caption);
        }

        void BuildRealmFoodModel(RoyalArt art,string food)
        {
            if(food.Contains("水果")){art.Piece("苹果",Vector3.zero,Vector3.one*.45f,new Color(.89f,.25f,.3f),PrimitiveType.Sphere);art.Piece("苹果柄",new Vector3(0,.28f,0),new Vector3(.045f,.14f,.045f),new Color(.41f,.26f,.12f));art.Piece("苹果叶",new Vector3(.09f,.3f,0),new Vector3(.2f,.05f,.1f),new Color(.33f,.69f,.35f),PrimitiveType.Sphere);}
            else if(food.Contains("饼干")){art.Piece("小饼干",Vector3.zero,new Vector3(.5f,.065f,.5f),new Color(.84f,.58f,.27f),PrimitiveType.Cylinder);for(int i=0;i<6;i++)art.Piece("巧克力粒",new Vector3(Mathf.Cos(i)*.16f,.07f,Mathf.Sin(i)*.16f),Vector3.one*.035f,new Color(.27f,.13f,.07f),PrimitiveType.Sphere);}
            else{var bread=art.Piece("烤面包",Vector3.zero,new Vector3(.23f,.32f,.23f),new Color(.86f,.62f,.31f),PrimitiveType.Capsule);bread.localRotation=Quaternion.Euler(0,0,90);foreach(float x in new[]{-.16f,0,.16f})art.Piece("面包烘烤纹路",new Vector3(x,.14f,0),new Vector3(.028f,.012f,.22f),new Color(.57f,.35f,.15f));}
        }

        void BuildRealmHome(Transform parent,Vector3 position,string title,Color wall,Color roof)
        {
            var home=new GameObject(title).transform;home.SetParent(parent,false);home.localPosition=position;var a=home.gameObject.AddComponent<RoyalArt>();
            var wood=new Color(.75f,.55f,.42f);var cream=new Color(.99f,.94f,.83f);
            a.Piece("房屋地板",new Vector3(0,.1f,0),new Vector3(10,.2f,10),wood,PrimitiveType.Cube,true);
            foreach(float x in new[]{-5f,5f})a.Piece("侧墙",new Vector3(x,2,0),new Vector3(.25f,4,10),wall,PrimitiveType.Cube,true);
            a.Piece("后墙",new Vector3(0,2,5),new Vector3(10,4,.25f),wall,PrimitiveType.Cube,true);
            foreach(float x in new[]{-3f,3f})a.Piece("大门两侧墙",new Vector3(x,2,-5),new Vector3(4,4,.25f),wall,PrimitiveType.Cube,true);
            a.Piece("大门上方墙",new Vector3(0,3.6f,-5),new Vector3(2,1,.25f),wall,PrimitiveType.Cube,true);
            foreach(float x in new[]{-2.6f,2.6f}){var top=a.Piece("斜屋顶",new Vector3(x,4.65f,0),new Vector3(5.8f,.3f,11),roof,PrimitiveType.Cube,true);top.localRotation=Quaternion.Euler(0,0,x<0?20:-20);}
            foreach(float z in new[]{-5f,5f})BuildRealmGable(home,z,wall);
            var hinge=new GameObject("可开关房门").transform;hinge.SetParent(home,false);hinge.localPosition=new Vector3(-1,0,-5.15f);
            var doorArt=hinge.gameObject.AddComponent<RoyalArt>();var door=doorArt.Piece("门扇",new Vector3(1,1.45f,0),new Vector3(2,2.9f,.18f),roof,PrimitiveType.Cube,true);
            doorArt.Piece("门把手",new Vector3(1.7f,1.4f,-.12f),Vector3.one*.13f,new Color(1,.8f,.28f),PrimitiveType.Sphere);
            AddRealmPoint(door,RealmUseKind.Door,"G 开门 / 关门",hinge);
            foreach(float x in new[]{-3f,3f}){a.Piece("窗户",new Vector3(x,2.4f,-5.16f),new Vector3(1.25f,1,.08f),new Color(.64f,.86f,.96f));a.Piece("窗帘",new Vector3(x,2.4f,-4.78f),new Vector3(1.5f,1.1f,.1f),cream);}
            a.Piece("床架",new Vector3(-2.8f,.4f,2),new Vector3(2.1f,.65f,3.5f),wood,PrimitiveType.Cube,true);
            var bed=a.Piece("柔软被子",new Vector3(-2.8f,.82f,1.8f),new Vector3(2.05f,.23f,2.6f),roof,PrimitiveType.Cube,true);AddRealmPoint(bed,RealmUseKind.Bed,"G 上床休息 / 起身");
            a.Piece("小枕头",new Vector3(-2.8f,.91f,3.35f),new Vector3(1.8f,.25f,.6f),cream,PrimitiveType.Sphere);
            a.Piece("沙发",new Vector3(2.7f,.7f,1),new Vector3(3,1.2f,1.3f),roof,PrimitiveType.Cube,true);
            a.Piece("沙发靠背",new Vector3(2.7f,1.4f,1.6f),new Vector3(3,.8f,.22f),roof);
            a.Piece("餐桌",new Vector3(.2f,1.05f,-1.2f),new Vector3(2.7f,.22f,1.8f),wood,PrimitiveType.Cube,true);
            foreach(float x in new[]{-.75f,1.1f})a.Piece("桌脚",new Vector3(x,.52f,-1.2f),new Vector3(.16f,1,.16f),wood);
            AddRealmFood(a,new Vector3(.2f,1.35f,-1.2f),title+"的面包",new Color(.87f,.65f,.35f));
            var tv=a.Piece("电视屏幕",new Vector3(2.5f,2.05f,4.7f),new Vector3(2.2f,1.2f,.1f),new Color(.14f,.17f,.25f),PrimitiveType.Cube,true);var television=AddRealmPoint(tv,RealmUseKind.Television,"G 打开 / 关闭电视");
            var cartoon=RoyalArt.Girl(home,true,false);cartoon.name="电视里的动画人物";cartoon.transform.localPosition=new Vector3(2.5f,1.6f,4.51f);cartoon.transform.localScale=Vector3.one*.35f;cartoon.transform.localRotation=Quaternion.Euler(0,180,0);cartoon.gameObject.AddComponent<RealmResidentLife>().Initialize(this,"唱歌");cartoon.gameObject.SetActive(false);television.Decoration=cartoon.transform;
            var shelf=a.Piece("书架",new Vector3(-4.5f,1.4f,-.8f),new Vector3(.5f,2.5f,2.5f),wood,PrimitiveType.Cube,true);AddRealmPoint(shelf,RealmUseKind.Book,"G 读一本故事书");
            for(int i=0;i<8;i++)a.Piece("彩色书",new Vector3(-4.15f,.7f+i%2*.9f,-1.7f+i/2*.5f),new Vector3(.23f,.6f,.32f),Color.HSVToRGB(i*.13f,.4f,.9f));
            a.Piece("灶台",new Vector3(3.7f,1,-3.6f),new Vector3(1.7f,1.8f,1.5f),cream,PrimitiveType.Cube,true);
            a.Piece("茶壶",new Vector3(3.7f,2.05f,-3.6f),Vector3.one*.5f,roof,PrimitiveType.Sphere);
            PlaceSign(home,title+" · G 开门",new Vector3(0,3.3f,-5.35f));
        }

        void BuildRealmGable(Transform home,float z,Color color)
        {
            var mesh=new Mesh{name="封闭屋顶下的三角墙"};mesh.vertices=new[]{new Vector3(-5,4,z),new Vector3(5,4,z),new Vector3(0,5.7f,z),new Vector3(-5,4,z),new Vector3(5,4,z),new Vector3(0,5.7f,z)};mesh.triangles=new[]{0,1,2,5,4,3};mesh.RecalculateNormals();mesh.RecalculateBounds();
            var wall=new GameObject("封闭三角屋墙");wall.transform.SetParent(home,false);wall.AddComponent<MeshFilter>().sharedMesh=mesh;var material=new Material(Resources.Load<Shader>("ActorSurface")){color=color};wall.AddComponent<MeshRenderer>().sharedMaterial=material;wall.AddComponent<MeshCollider>().sharedMesh=mesh;var lifetime=wall.AddComponent<RealmMeshLifetime>();lifetime.Mesh=mesh;lifetime.Material=material;
        }

        void BuildInhabitedPlanet(Transform root,RoyalArt art)
        {
            BuildPlanetShell(root);
            BuildRealmGround(root,"星球可生活的大陆",PlanetFloor+1,PlanetLandDiameter/2,new Color(.45f,.67f,.4f),new Rect(84,24,16,16));
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4;art.Piece("星球山脉",new Vector3(32+Mathf.Cos(angle)*130,PlanetFloor-4,32+Mathf.Sin(angle)*130),new Vector3(25,18,22),new Color(.4f,.58f,.37f),PrimitiveType.Sphere);
            }
            for(int i=0;i<9;i++)BuildRealmHome(root,new Vector3(-32+i%3*36,PlanetFloor+1,-20+i/3*38),"星球居民家 "+(i+1),new Color(.87f,.82f,.7f),new Color(.68f,.42f,.35f));
            for(int i=0;i<22;i++)
            {
                BuildPlanetVillager(root,"星球居民 "+(i+1),new Vector3(-26+i%6*22,PlanetFloor+1,2+i/6*30),i,i%5==0?PlanetAge.Child:i%5==1?PlanetAge.Elder:PlanetAge.Adult);
            }
            for(int i=0;i<55;i++)
            {
                float angle=i*2.39996f,radius=80+i%8*7;Vector3 p=new Vector3(32+Mathf.Cos(angle)*radius,PlanetFloor+1,32+Mathf.Sin(angle)*radius);
                if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(95,80))<13||Vector2.Distance(new Vector2(p.x,p.z),new Vector2(95,110))<13)continue;
                art.Piece("星球树林的树干",p+Vector3.up*2,new Vector3(.8f,4,.8f),new Color(.49f,.31f,.18f),PrimitiveType.Cube,true);
                art.Piece("星球树林的树冠",p+Vector3.up*5,new Vector3(5,5,5),new Color(.32f,.59f,.31f),PrimitiveType.Sphere);
            }
            for(int i=0;i<8;i++){var crop=art.Piece("星球小麦农田",new Vector3(92+i%4*3,PlanetFloor+1.5f,70+i/4*3),new Vector3(1.7f,1,1.7f),new Color(.83f,.73f,.34f),PrimitiveType.Cube,true);AddRealmPoint(crop,RealmUseKind.Harvest,"G 收获小麦，得到面包");}
            var work=art.Piece("星球工作台",new Vector3(31,PlanetFloor+1.7f,20),new Vector3(1.6f,1.3f,1.6f),new Color(.58f,.36f,.2f),PrimitiveType.Cube,true);AddRealmPoint(work,RealmUseKind.Craft,"G 使用九宫格工作台");
            art.Piece("星球蓝色湖泊",new Vector3(-58,PlanetFloor+1.08f,55),new Vector3(32,.08f,25),new Color(.28f,.64f,.77f),PrimitiveType.Cylinder);
            foreach(float x in new[]{-32f,4f,40f})art.Piece("居民区小路",new Vector3(x,PlanetFloor+1.02f,24),new Vector3(2.2f,.025f,105),new Color(.72f,.66f,.48f));
            foreach(float z in new[]{-20f,18f,56f})art.Piece("居民区横向小路",new Vector3(2,PlanetFloor+1.025f,z-7),new Vector3(110,.025f,2.2f),new Color(.72f,.66f,.48f));
            PlaceSign(root,"星球居民区 · 东边井口通往地狱",new Vector3(53,PlanetFloor+2.3f,12));
            foreach(float z in new[]{24f,40f})art.Piece("星球深井护栏",new Vector3(92,PlanetFloor+2,z),new Vector3(16,1,.2f),new Color(.72f,.5f,.3f));
            PlaceSign(root,"下方是地狱 · 自己走入井口才会坠落",new Vector3(92,PlanetFloor+4,20));
            BuildPlanetFacilities(root,art);
        }

        void BuildRealmSpa(Transform root,RoyalArt art)
        {
            float y=HeavenFloor+1;var stone=new Color(.85f,.82f,.87f);
            foreach(float x in new[]{9.8f,18.2f})art.Piece("温泉池边",new Vector3(x,y+.45f,13),new Vector3(.55f,.8f,8),stone,PrimitiveType.Cube,true);
            foreach(float z in new[]{9f,17f})art.Piece("温泉池边",new Vector3(14,y+.45f,z),new Vector3(8.5f,.8f,.55f),stone,PrimitiveType.Cube,true);
            var water=art.Piece("可泡的温泉水",new Vector3(14,y+.35f,13),new Vector3(7.8f,.08f,7.5f),new Color(.37f,.73f,.83f),PrimitiveType.Cube,true);
            water.GetComponent<Renderer>().sharedMaterial.shader=Resources.Load<Shader>("GlassPane");water.GetComponent<Renderer>().sharedMaterial.color=new Color(.28f,.68f,.79f,.7f);
            AddRealmPoint(water,RealmUseKind.Pool,"G 泡温泉，八秒后出池");
            for(int i=0;i<9;i++){var steam=art.Piece("温泉热气",new Vector3(11+i%3*3,y+.55f,10+i/3*3),Vector3.one*(.35f+i%3*.12f),new Color(.9f,.93f,.97f),PrimitiveType.Sphere);steam.GetComponent<Renderer>().sharedMaterial.shader=Resources.Load<Shader>("GlassPane");steam.gameObject.AddComponent<RealmSteamMotion>().Game=this;}
        }

        void BuildPlanetShell(Transform root)
        {
            float radius=PlanetDiameter/2,capRadius=PlanetLandDiameter/2;
            float capHeight=Mathf.Sqrt(radius*radius-capRadius*capRadius),centerY=PlanetFloor+1-capHeight;
            float start=Mathf.Acos(capHeight/radius);const int rings=40,segments=72;
            var vertices=new List<Vector3>();var ocean=new List<int>();var land=new List<int>();
            for(int i=0;i<=rings;i++){float theta=Mathf.Lerp(start,Mathf.PI,i/(float)rings);for(int j=0;j<=segments;j++){float phi=j*Mathf.PI*2/segments;vertices.Add(new Vector3(32+Mathf.Sin(theta)*Mathf.Cos(phi)*radius,centerY+Mathf.Cos(theta)*radius,32+Mathf.Sin(theta)*Mathf.Sin(phi)*radius));}}
            for(int i=0;i<rings;i++)for(int j=0;j<segments;j++){int a=i*(segments+1)+j,b=a+segments+1;var list=i<30&&Mathf.PerlinNoise(j*.12f,i*.15f)>.57f?land:ocean;list.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            var mesh=new Mesh{name="与大陆边缘接合的球壳"};mesh.SetVertices(vertices);mesh.subMeshCount=2;mesh.SetTriangles(ocean,0);mesh.SetTriangles(land,1);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var planet=new GameObject("直径384的巨大圆形星球");planet.transform.SetParent(root,false);planet.AddComponent<MeshFilter>().sharedMesh=mesh;
            var blue=new Material(Resources.Load<Shader>("ActorSurface")){color=new Color(.18f,.43f,.64f)};var green=new Material(Resources.Load<Shader>("ActorSurface")){color=new Color(.37f,.58f,.35f)};
            planet.AddComponent<MeshRenderer>().sharedMaterials=new[]{blue,green};var lifetime=planet.AddComponent<RealmMeshLifetime>();lifetime.Mesh=mesh;lifetime.Materials=new[]{blue,green};
        }

        void BuildRealmLifeExpansion(Transform root,RoyalArt art)
        {
            realmPoints.RemoveAll(p=>p==null);RealmLanternsLit=0;
            float h=HeavenFloor+1;
            foreach(var p in new[]{new Vector3(-18,h,35),new Vector3(72,h,55),new Vector3(75,h,-8),new Vector3(-15,h,-8)})BuildRealmHome(root,p,"云上温暖小屋",new Color(.94f,.92f,1),new Color(.86f,.65f,.8f));
            art.Piece("天堂花园小路",new Vector3(32,h+.025f,35),new Vector3(105,.04f,2.3f),new Color(.88f,.8f,.69f));
            art.Piece("天堂通往温泉的小路",new Vector3(14,h+.03f,30),new Vector3(2.4f,.04f,43),new Color(.88f,.8f,.69f));
            for(int i=0;i<10;i++){Vector3 p=new Vector3(-12+i%5*20,h,2+i/5*66);art.Piece("天堂休息长椅",p+Vector3.up*.5f,new Vector3(3.2f,.25f,1.2f),new Color(.9f,.66f,.78f),PrimitiveType.Cube,true);art.Piece("长椅靠背",p+new Vector3(0,1,.6f),new Vector3(3.2f,.7f,.17f),new Color(.9f,.66f,.78f));}
            foreach(var a in root.GetComponentsInChildren<RoyalArt>())if(a.name.Contains("天使")||a.name.Contains("天堂的"))a.gameObject.AddComponent<RealmResidentLife>().Initialize(this,a.name.Contains("唱歌")?"唱歌":"散步");
            for(int i=0;i<4;i++){var mother=RealmPerson(root,"照顾花园的天使家人 "+i,new Vector3(-6+i*16,h,67),Color.HSVToRGB(.1f+i*.18f,.28f,.95f),i%3==0,i%2==0);AddHaloAndWings(mother,false);mother.gameObject.AddComponent<RealmResidentLife>().Initialize(this,"唱歌");}
            for(int i=0;i<3;i++){var lantern=art.Piece("地狱可净化的灵魂灯",new Vector3(-8+i*24,HellFloor+2.1f,-6),new Vector3(.6f,1.2f,.6f),new Color(.64f,.14f,.24f),PrimitiveType.Cube,true);AddRealmPoint(lantern,RealmUseKind.Lantern,"G 净化这盏灵魂灯");}
            foreach(var ghost in root.GetComponentsInChildren<Transform>())if(ghost.name=="徘徊的幽灵")ghost.gameObject.AddComponent<RealmGhostLife>().Initialize(this);
            var dragon=root.Find("低吼的赤色恶龙");if(dragon!=null)dragon.gameObject.AddComponent<RealmDragonLife>().Game=this;
        }

        public void StartRealmMeal(string food)
        {
            if(realmMealRemaining>0)return;realmMealRemaining=3;Player.HomeRest(true);
            if(realmFood!=null)Destroy(realmFood.gameObject);
            realmFood=new GameObject("亲手拿到嘴边的"+food).transform;realmFood.SetParent(Player.View.transform,false);
            var content=new GameObject("食物").transform;content.SetParent(realmFood,false);BuildRealmFoodModel(content.gameObject.AddComponent<RoyalArt>(),food);realmFood.localScale=Vector3.one*.4f;
            var handArt=realmFood.gameObject.AddComponent<RoyalArt>();handArt.Piece("握住食物的小手",new Vector3(.12f,-.13f,0),new Vector3(.32f,.25f,.3f),new Color(.94f,.69f,.53f),PrimitiveType.Sphere);
            foreach(var t in realmFood.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
            Notice="正在拿起并吃掉"+food+"，不是只放到背包里。";
        }
        public void StartRealmBath(){if(realmBathRemaining>0)return;realmBathRemaining=8;Player.Teleport(new Vector3(14,HeavenFloor+1.45f,13));Player.HomeRest(true);Notice="已经坐进有水和热气的温泉，泡八秒钟。";}

        public void UseRealmPoint(RealmUsePoint point)
        {
            if(Paused||!InVoidRealms||point==null)return;
            switch(point.Kind)
            {
                case RealmUseKind.Door:point.Active=!point.Active;point.Target.localRotation=Quaternion.Euler(0,point.Active?-100:0,0);Notice=point.Active?"门打开了，可以走进屋内。":"门已关闭。";break;
                case RealmUseKind.Food:StartRealmMeal(point.Caption.Replace("G 拿起来吃：",""));break;
                case RealmUseKind.Pool:StartRealmBath();break;
                case RealmUseKind.Bed:realmResting=!realmResting;if(realmResting){realmRestExit=Player.transform.position;Player.Teleport(point.transform.position+Vector3.up*.3f);bool bench=point.Caption.Contains("长椅");Player.HomeRest(true,!bench);Notice=bench?"坐在长椅上休息；再按G起身。":"躺在床上休息；再按G起身。";}else{Player.HomeRest(false);Player.Teleport(realmRestExit);}break;
                case RealmUseKind.Book:var stories=new[]{"天使帮迷路的孩子找到家。","居民一起种麦子，再把面包送给邻居。","小朋友和爸爸妈妈搭好了云朵秋千。","勇敢的人点亮三盏灯，幽灵就退到了远处。","树林里的小鸟提醒大家保护湖泊。","屋内的书架藏着星球旅行者的日记。"};Notice="故事书 "+(realmBookPage%stories.Length+1)+"："+stories[realmBookPage++%stories.Length];break;
                case RealmUseKind.Television:point.Active=!point.Active;point.GetComponent<Renderer>().sharedMaterial.color=point.Active?new Color(.35f,.69f,.85f):new Color(.14f,.17f,.25f);if(point.Decoration!=null)point.Decoration.gameObject.SetActive(point.Active);Notice=point.Active?"电视打开了，屏幕中的人物正在跳舞。":"电视关闭了。";break;
                case RealmUseKind.Harvest:Inventory[(int)Block.Bread]=Mathf.Min(9999,Inventory[(int)Block.Bread]+1);Notice="收获了小麦，居民帮你烤好一块面包。";break;
                case RealmUseKind.Craft:OpenCrafting();break;
                case RealmUseKind.Lantern:if(!point.Active){point.Active=true;RealmLanternsLit++;point.GetComponent<Renderer>().sharedMaterial.color=new Color(.37f,.81f,.9f);if(RealmLanternsLit==3)EarnActivityCoins(30,"净化地狱三盏灵魂灯");}Notice="已净化灵魂灯 "+RealmLanternsLit+" / 3，三盏全部点亮会得到30元。";break;
                case RealmUseKind.Hospital:Health=10;Notice="星球医生帮你检查和治疗，生命已恢复满格。";break;
                case RealmUseKind.Police:UsePlanetPolice();break;
                case RealmUseKind.LostProperty:if(!point.Active){point.Active=true;PlanetLostProperty++;point.Target.gameObject.SetActive(false);}Notice="找到失物 "+PlanetLostProperty+" / 3，带回警察局按G归还。";break;
                case RealmUseKind.PlayRide:StartPlanetRide(point.Variant,point.Target);break;
            }
        }

        bool TryRealmLifeG()
        {
            if(PlanetRideActive){StopPlanetRide(false);return true;}
            if(realmResting){realmResting=false;Player.HomeRest(false);Player.Teleport(realmRestExit);return true;}
            if(realmMealRemaining>0||realmBathRemaining>0)return true;
            if(RayFromView(out var hit,3.8f,QueryTriggerInteraction.Collide)){var point=hit.collider.GetComponentInParent<RealmUsePoint>();if(point!=null){UseRealmPoint(point);return true;}}
            RealmUsePoint nearest=null;float distance=2.4f;
            foreach(var point in realmPoints){if(point==null)continue;float d=Vector3.Distance(Player.transform.position+Vector3.up,point.transform.position);if(d<distance){nearest=point;distance=d;}}
            if(nearest==null)return false;UseRealmPoint(nearest);return true;
        }

        string RealmInteractionCaption()
        {
            if(PlanetRideActive)return "G 提前起身，结束星球游乐项目";
            if(realmResting)return "G 起身离开床";
            if(RealmMealActive)return "正在把食物拿到嘴边吃，稍等三秒";
            if(RealmBathActive)return "正在泡温泉，还剩 "+Mathf.CeilToInt(realmBathRemaining)+" 秒";
            if(RayFromView(out var hit,4,QueryTriggerInteraction.Collide)){var point=hit.collider.GetComponentInParent<RealmUsePoint>();if(point!=null)return point.Caption;}
            return "靠近门、食物、床、书架、工作台或温泉，按 G 使用";
        }

        void TickRealmLife()
        {
            TickPlanetRide();
            bool lit=InHeaven||InPlanetLife;
            RenderSettings.ambientSkyColor=lit?new Color(.58f,.65f,.73f):InHell?new Color(.2f,.08f,.12f):IsNight?new Color(.04f,.05f,.13f):new Color(.38f,.43f,.46f);
            RenderSettings.ambientEquatorColor=lit?new Color(.48f,.51f,.45f):InHell?new Color(.18f,.065f,.06f):IsNight?new Color(.03f,.035f,.08f):new Color(.29f,.32f,.28f);
            RenderSettings.ambientGroundColor=lit?new Color(.32f,.34f,.27f):new Color(.12f,.08f,.1f);
            Player.View.farClipPlane=InVoidRealms?650:150;
            if(!InVoidRealms){realmResting=false;realmMealRemaining=realmBathRemaining=0;if(realmFood!=null){Destroy(realmFood.gameObject);realmFood=null;}return;}
            if(realmMealRemaining>0){realmMealRemaining=Mathf.Max(0,realmMealRemaining-Time.unscaledDeltaTime);if(realmFood!=null){float t=3-realmMealRemaining;realmFood.localPosition=Vector3.Lerp(new Vector3(.3f,-.3f,.55f),new Vector3(0,-.06f,.23f),Mathf.Clamp01(t));realmFood.Find("食物").localScale=Vector3.one*Mathf.Max(.15f,realmMealRemaining/3);}if(realmMealRemaining==0){RealmMealsEaten++;Health=Mathf.Min(10,Health+2);if(realmFood!=null)Destroy(realmFood.gameObject);realmFood=null;Player.HomeRest(false);Notice="食物吃完了，恢复两颗心。";}}
            if(realmBathRemaining>0){realmBathRemaining=Mathf.Max(0,realmBathRemaining-Time.unscaledDeltaTime);if(realmBathRemaining==0){Health=Mathf.Min(10,Health+2);Player.HomeRest(false);Player.Teleport(new Vector3(14,HeavenFloor+1.1f,19));Notice="泡完温泉，走到池边了。";}}
        }
    }
}
