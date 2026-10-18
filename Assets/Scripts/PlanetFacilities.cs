using UnityEngine;

namespace Blocklands { public partial class Sandbox
{
    float planetRideRemaining,planetRideAge;int planetRideType;Transform planetRidePivot;Vector3 planetRideEntry;
    public bool PlanetRideActive=>planetRideRemaining>0;
    public int PlanetLostProperty {get;private set;}bool planetPolicePaid;
    public static readonly Vector3 PlanetPark=new Vector3(72,PlanetFloor+1,51);

    void BuildPlanetFacilities(Transform root,RoyalArt art)
    {
        if(PlanetRideActive)Player.HomeRest(false);planetRideRemaining=0;planetRidePivot=null;PlanetLostProperty=0;planetPolicePaid=false;
        Vector3 hospital=new Vector3(95,PlanetFloor+1,80),police=new Vector3(95,PlanetFloor+1,110);
        BuildRealmHome(root,hospital,"星球医院",new Color(.9f,.95f,.96f),new Color(.79f,.5f,.52f));
        art.Piece("医院红十字竖",hospital+new Vector3(0,5.4f,-5.2f),new Vector3(.35f,1.3f,.13f),new Color(.85f,.2f,.25f));art.Piece("医院红十字横",hospital+new Vector3(0,5.4f,-5.2f),new Vector3(1.3f,.35f,.13f),new Color(.85f,.2f,.25f));
        var doctor=BuildPlanetVillager(root,"星球医生",hospital+new Vector3(-2,0,-2),32,PlanetAge.Adult);doctor.transform.Find("body").GetComponent<Renderer>().sharedMaterial.color=Color.white;
        var nurse=BuildPlanetVillager(root,"星球护士",hospital+new Vector3(2,0,-2),33,PlanetAge.Adult);nurse.transform.Find("body").GetComponent<Renderer>().sharedMaterial.color=new Color(.6f,.86f,.92f);
        var treatment=art.Piece("医院检查台",hospital+new Vector3(0,1.1f,-2.4f),new Vector3(2,.25f,1),new Color(.77f,.85f,.86f),PrimitiveType.Cube,true);AddRealmPoint(treatment,RealmUseKind.Hospital,"G 请医生治疗，恢复全部生命");
        art.Piece("医院心跳显示器",hospital+new Vector3(-3,1.6f,2),new Vector3(1.1f,.7f,.2f),new Color(.15f,.36f,.33f));
        for(int i=0;i<4;i++)art.Piece("医院药瓶",hospital+new Vector3(-3+i*.4f,1.25f,-1),new Vector3(.18f,.35f,.18f),Color.HSVToRGB(i*.2f,.3f,.9f),PrimitiveType.Cylinder);
        BuildRealmHome(root,police,"星球警察局",new Color(.78f,.86f,.94f),new Color(.3f,.48f,.69f));
        var officer=BuildPlanetVillager(root,"星球警官",police+new Vector3(-2,0,-2),34,PlanetAge.Adult);officer.transform.Find("body").GetComponent<Renderer>().sharedMaterial.color=new Color(.13f,.28f,.58f);
        var desk=art.Piece("星球失物招领台",police+new Vector3(0,1.1f,-2.5f),new Vector3(2,.25f,1.1f),new Color(.39f,.5f,.64f),PrimitiveType.Cube,true);AddRealmPoint(desk,RealmUseKind.Police,"G 领取 / 完成失物招领任务");
        art.Piece("警察局电脑",police+new Vector3(0,1.7f,-2.3f),new Vector3(1.1f,.65f,.13f),new Color(.13f,.21f,.34f));
        for(int i=0;i<7;i++)art.Piece("警察局安全隔栏",police+new Vector3(6+i*.45f,1.2f,2),new Vector3(.08f,2.4f,.08f),new Color(.56f,.63f,.67f));
        for(int i=0;i<3;i++)
        {
            Vector3 p=PlanetPark+new Vector3(-8+i*8,.35f,-5+i%2*5);var item=art.Piece("居民遗失的小物件 "+i,p,new Vector3(.45f,.45f,.45f),i==0?Color.yellow:i==1?new Color(.85f,.36f,.53f):new Color(.36f,.65f,.83f),PrimitiveType.Sphere,true);AddRealmPoint(item,RealmUseKind.LostProperty,"G 拾起失物，交给星球警察局");
        }
        BuildPlanetPlayground(root,art);
        PlaceSign(root,"星球医院与警察局 · 屋内按G治疗 / 失物招领",new Vector3(92,PlanetFloor+3,92));
    }

    void BuildPlanetPlayground(Transform root,RoyalArt art)
    {
        var pink=new Color(.89f,.55f,.73f);var mint=new Color(.43f,.74f,.65f);var gold=new Color(.94f,.77f,.4f);
        Vector3 swing=PlanetPark+Vector3.left*8;
        foreach(float x in new[]{-1.8f,1.8f})art.Piece("星球秋千支架",swing+new Vector3(x,1.6f,0),new Vector3(.2f,3.2f,.2f),mint);
        art.Piece("星球秋千横杆",swing+Vector3.up*3.2f,new Vector3(3.8f,.2f,.2f),gold);
        var pivot=new GameObject("星球秋千摆轴").transform;pivot.SetParent(root,false);pivot.position=swing+Vector3.up*3.1f;
        var a=pivot.gameObject.AddComponent<RoyalArt>();foreach(float x in new[]{-.65f,.65f})a.Piece("秋千绳",new Vector3(x,-1.2f,0),new Vector3(.07f,2.4f,.07f),gold);
        a.Piece("星球秋千座椅",new Vector3(0,-2.4f,0),new Vector3(1.5f,.2f,.65f),pink);
        var button=art.Piece("G坐星球秋千",swing+new Vector3(2,1,-1.2f),new Vector3(.5f,.5f,.2f),pink,PrimitiveType.Cube,true);var point=AddRealmPoint(button,RealmUseKind.PlayRide,"G 星球秋千，玩六秒",pivot);point.Variant=1;
        Vector3 carousel=PlanetPark+Vector3.forward*10;
        var baseDisc=art.Piece("星球旋转木马地台",carousel-Vector3.up*.1f,new Vector3(6,.1f,6),mint,PrimitiveType.Cylinder,true);var old=baseDisc.GetComponent<Collider>();old.enabled=false;Destroy(old);baseDisc.gameObject.AddComponent<MeshCollider>().sharedMesh=baseDisc.GetComponent<MeshFilter>().sharedMesh;
        var platform=new GameObject("星球旋转木马转盘").transform;platform.SetParent(root,false);platform.position=carousel;
        var c=platform.gameObject.AddComponent<RoyalArt>();c.Piece("木马中心柱",Vector3.up*1.8f,new Vector3(.3f,3.6f,.3f),gold);c.Taper("木马彩色顶棚",Vector3.up*3.5f,new Vector3(6,.4f,6),pink,.04f);
        for(int i=0;i<4;i++){float t=i*Mathf.PI/2;Vector3 p=new Vector3(Mathf.Sin(t)*2,.8f,Mathf.Cos(t)*2);c.Piece("木马小马身体",p,new Vector3(.7f,.6f,1),i%2==0?Color.white:mint);c.Piece("木马小马头",p+new Vector3(0,.35f,.45f),new Vector3(.4f,.6f,.4f),i%2==0?Color.white:mint);}
        var carouselButton=art.Piece("G坐星球木马",carousel+new Vector3(0,1,-3.3f),new Vector3(.6f,.5f,.2f),pink,PrimitiveType.Cube,true);var carouselPoint=AddRealmPoint(carouselButton,RealmUseKind.PlayRide,"G 星球旋转木马，玩八秒",platform);carouselPoint.Variant=2;
        Vector3 slide=PlanetPark+Vector3.right*8;
        var slope=art.Piece("星球大滑梯",slide+new Vector3(0,1.6f,-.9f),new Vector3(1.8f,.25f,6),mint,PrimitiveType.Cube,true);slope.localRotation=Quaternion.Euler(-26,0,0);
        for(int i=0;i<6;i++)art.Piece("滑梯台阶",slide+new Vector3(0,i*.45f,i*.37f),new Vector3(1.8f,.3f,.5f),pink,PrimitiveType.Cube,true);
        var slideButton=art.Piece("G玩星球滑梯",slide+new Vector3(1.5f,1,-2),new Vector3(.5f,.5f,.2f),pink,PrimitiveType.Cube,true);var slidePoint=AddRealmPoint(slideButton,RealmUseKind.PlayRide,"G 从大滑梯滑下来",slope);slidePoint.Variant=3;
        var bench=art.Piece("星球公园长椅",PlanetPark+new Vector3(-1,.5f,-8),new Vector3(4,.3f,1.2f),gold,PrimitiveType.Cube,true);AddRealmPoint(bench,RealmUseKind.Bed,"G 坐长椅休息 / 起身");art.Piece("公园长椅靠背",PlanetPark+new Vector3(-1,1.1f,-7.5f),new Vector3(4,.9f,.2f),gold);
        PlaceSign(root,"居民家旁的游乐园 · 瞄准按钮按G游玩",PlanetPark+new Vector3(0,2.3f,-9));
    }

    public void UsePlanetPolice()
    {
        if(PlanetLostProperty<3){Notice="星球警官：请去小公园找回三件失物，带回来会有20元感谢金。已找到 "+PlanetLostProperty+" / 3。";return;}
        if(!planetPolicePaid){planetPolicePaid=true;EarnActivityCoins(20,"帮助星球警察归还失物");}Notice="失物已经归还给居民，警察谢谢你的帮忙。";
    }
    public void StartPlanetRide(int type,Transform target)
    {
        if(PlanetRideActive||Paused||(!InPlanetLife&&!InHeaven)||target==null)return;planetRideType=type;planetRidePivot=target;planetRideAge=0;planetRideRemaining=type==1?6:type==2||type==4?8:type==5?10:type==6?4:3;planetRideEntry=Player.transform.position;Player.SetFlying(false);Player.HomeRest(true);Notice="开始游玩"+(InHeaven?"天堂":"星球")+"设施，按G可提前起身。";
    }
    public void StopPlanetRide(bool completed)
    {
        if(!PlanetRideActive&&planetRidePivot==null)return;planetRideRemaining=0;planetRidePivot=null;Player.HomeRest(false);Player.Teleport(planetRideEntry);
        if(completed)EarnActivityCoins(18,"星球游乐设施");else Notice="已经安全起身。";
    }
    void TickPlanetRide()
    {
        if(!PlanetRideActive)return;if((!InPlanetLife&&!InHeaven)||planetRidePivot==null){planetRideRemaining=0;planetRidePivot=null;Player.HomeRest(false);return;}
        float dt=Time.unscaledDeltaTime;planetRideAge+=dt;planetRideRemaining=Mathf.Max(0,planetRideRemaining-dt);Vector3 seat;
        if(planetRideType==1){planetRidePivot.localRotation=Quaternion.Euler(Mathf.Sin(planetRideAge*2)*24,0,0);seat=planetRidePivot.TransformPoint(new Vector3(0,-2.2f,0));}
        else if(planetRideType==2){planetRidePivot.Rotate(0,dt*40,0);seat=planetRidePivot.TransformPoint(new Vector3(0,1.05f,2));}
        else if(planetRideType==4)seat=planetRidePivot.position+new Vector3(Mathf.Sin(planetRideAge*.7f)*4,.8f,Mathf.Cos(planetRideAge*.7f)*2);
        else if(planetRideType==5)seat=planetRidePivot.position+Vector3.up*(.7f+Mathf.Abs(Mathf.Sin(planetRideAge*2))*2.5f);
        else if(planetRideType==6)seat=Vector3.Lerp(planetRidePivot.position+new Vector3(0,3,2.5f),planetRidePivot.position+new Vector3(0,.15f,-4),Mathf.SmoothStep(0,1,Mathf.Clamp01(planetRideAge/4)));
        else{Vector3 p=PlanetPark+Vector3.right*8;seat=Vector3.Lerp(p+new Vector3(0,2.8f,2),p+new Vector3(0,.15f,-4),Mathf.SmoothStep(0,1,Mathf.Clamp01(planetRideAge/3)));}
        Player.Carry(seat-Player.transform.position);if(planetRideRemaining==0)StopPlanetRide(true);
    }
}}
