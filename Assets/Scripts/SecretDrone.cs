using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    Transform villageDrone;Camera droneCamera;RenderTexture droneTexture;OwnedHome droneHome;bool dronePanel,droneControl;Vector3 dronePlayerEntry;Transform droneFollowing;float droneRenderAt,droneShotAt;Rect droneScreenRect;
    public bool DronePanelOpen=>dronePanel;
    public bool DroneControlActive=>dronePanel&&droneControl;
    public Vector3 DronePosition=>villageDrone==null?Vector3.zero:villageDrone.position;
    public RenderTexture DroneTexture=>droneTexture;
    public Transform MakeDroneModel(Transform parent,Vector3 position,bool world)
    {
        var r=new GameObject("可驾驶村庄无人机").transform;r.SetParent(parent,false);if(world)r.position=position;else r.localPosition=position;var a=r.gameObject.AddComponent<RoyalArt>();a.Piece("无人机机身",Vector3.zero,new Vector3(.7f,.25f,.55f),new Color(.87f,.9f,.94f));a.Piece("前方蓝色摄像头",new Vector3(0,-.03f,.32f),Vector3.one*.18f,new Color(.24f,.73f,.9f),PrimitiveType.Sphere);for(int i=0;i<4;i++){float x=i%2==0?-.48f:.48f,z=i/2==0?-.4f:.4f;a.Piece("机臂",new Vector3(x*.55f,0,z*.55f),new Vector3(.7f,.08f,.1f),new Color(.25f,.34f,.4f));a.Piece("螺旋桨",new Vector3(x,.16f,z),new Vector3(.62f,.025f,.08f),new Color(.24f,.3f,.34f));a.Piece("安全脚架",new Vector3(x,-.2f,z),new Vector3(.05f,.3f,.05f),new Color(.8f,.8f,.83f));}return r;
    }
    void EnsureVillageDrone(OwnedHome h)
    {
        if(villageDrone!=null)return;villageDrone=MakeDroneModel(secretRoomRoot,new Vector3(h.x+5,h.y+9,h.z-4),true);droneTexture=new RenderTexture(960,540,16){name="无人机实时游戏画面"};droneTexture.Create();var o=new GameObject("无人机镜头（仅游戏内）");o.transform.SetParent(villageDrone,false);o.transform.localPosition=new Vector3(0,-.05f,.42f);droneCamera=o.AddComponent<Camera>();droneCamera.enabled=false;droneCamera.targetTexture=droneTexture;droneCamera.nearClipPlane=.12f;droneCamera.farClipPlane=260;droneCamera.fieldOfView=68;droneCamera.cullingMask=~(1<<2);droneCamera.clearFlags=CameraClearFlags.SolidColor;droneCamera.backgroundColor=World.SkyColor;droneCamera.allowHDR=false;villageDrone.rotation=Quaternion.Euler(20,0,0);
    }
    public bool OpenDronePanel(OwnedHome home=null,bool fromMonitor=false)
    {
        var h=home??FindSecretHome();bool local=h!=null&&h.region==World.Region&&Player.transform.position.x>=h.x&&Player.transform.position.x<=h.x+10&&Player.transform.position.z>=h.z&&Player.transform.position.z<=h.z+10&&Player.transform.position.y<h.y;if(h==null||(!fromMonitor&&(h.deepBaseVersion!=1||CurrentDeepFloor(h)!=2))||(fromMonitor&&(!local||h.secretRoomVersion!=1))||Paused||SecretChairActive||DeepActivityActive||CompanionReturnBlocked)return false;EnsureVillageDrone(h);droneHome=h;dronePlayerEntry=Player.transform.position;dronePanel=true;droneControl=false;SetPaused(false);RenderDrone(true);Notice="无人机电视已打开，先点击“开始控制”；现在人物与无人机都不移动。";return true;
    }
    public bool BeginDroneControl(){if(!dronePanel||villageDrone==null)return false;droneControl=true;Notice="正在遥控无人机；W/↑前进，S/↓后退，A/D左右，空格升高，Shift下降，Q/E转向。4退出。";return true;}
    public void CloseDronePanel(){droneControl=dronePanel=false;droneFollowing=null;droneHome=null;}
    void DisposeDrone(){CloseDronePanel();if(droneCamera!=null)droneCamera.targetTexture=null;if(villageDrone!=null)Destroy(villageDrone.gameObject);villageDrone=null;droneCamera=null;if(droneTexture!=null){droneTexture.Release();Destroy(droneTexture);}droneTexture=null;}
    public void DroneStep(Vector2 move,float lift,float turn,float dt)
    {
        if(!DroneControlActive||Paused||villageDrone==null)return;villageDrone.Rotate(0,Mathf.Clamp(turn,-1,1)*60*dt,0,Space.World);Vector3 forward=Vector3.ProjectOnPlane(villageDrone.forward,Vector3.up).normalized,right=Vector3.Cross(Vector3.up,forward);Vector2 input=Vector2.ClampMagnitude(move,1);Vector3 delta=(forward*input.y+right*input.x+Vector3.up*Mathf.Clamp(lift,-1,1))*(6*Mathf.Min(dt,.1f));
        if(delta.sqrMagnitude>.00001f){float length=delta.magnitude;if(Physics.SphereCast(villageDrone.position,.45f,delta/length,out var hit,length+.03f,~(1<<2),QueryTriggerInteraction.Ignore))delta=delta.normalized*Mathf.Max(0,hit.distance-.04f);Vector3 p=villageDrone.position+delta;p.x=Mathf.Clamp(p.x,.6f,VoxelWorld.Width-.6f);p.z=Mathf.Clamp(p.z,.6f,VoxelWorld.Depth-.6f);p.y=Mathf.Clamp(p.y,2,125);villageDrone.position=p;}RenderDrone(true);
    }
    public bool FollowDroneResident(){if(!DroneControlActive)return false;Transform best=null;float distance=30;foreach(var v in FindObjectsByType<Villager>()){float d=Vector3.Distance(v.transform.position,villageDrone.position);if(!v.IsFriend&&d<distance){distance=d;best=v.transform;}}foreach(var v in FindObjectsByType<SkyResidentInfo>()){float d=Vector3.Distance(v.transform.position,villageDrone.position);if(d<distance){distance=d;best=v.transform;}}droneFollowing=best;Notice=best==null?"附近没找到可跟随的村民。":"无人机正在跟随附近村民；按方向键可停止跟随。";return best!=null;}
    void TickDrone(float dt)
    {
        if(!dronePanel)return;if(droneFollowing!=null&&droneControl){Vector3 target=droneFollowing.position+new Vector3(0,3,-3);Vector3 delta=target-villageDrone.position;if(delta.magnitude>.3f)DroneStep(new Vector2(Vector3.Dot(delta,villageDrone.right),Vector3.Dot(delta,Vector3.ProjectOnPlane(villageDrone.forward,Vector3.up).normalized)),Mathf.Clamp(delta.y,-1,1),0,dt);villageDrone.rotation=Quaternion.Slerp(villageDrone.rotation,Quaternion.LookRotation(droneFollowing.position+Vector3.up-villageDrone.position),dt*2);}RenderDrone();
    }
    void DroneKeys()
    {
        if(Input.GetKeyDown(KeyCode.Alpha4)||Input.GetKeyDown(KeyCode.Escape)){CloseDronePanel();return;}if(!droneControl)return;Vector2 movement=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));float lift=(Input.GetKey(KeyCode.Space)?1:0)-(Input.GetKey(KeyCode.LeftShift)?1:0),turn=(Input.GetKey(KeyCode.E)?1:0)-(Input.GetKey(KeyCode.Q)?1:0);if(movement.sqrMagnitude>0||lift!=0||turn!=0){droneFollowing=null;DroneStep(movement,lift,turn,Time.deltaTime);}
    }
    public bool DroneSignal(Vector2 viewport)
    {
        if(!DroneControlActive||Time.unscaledTime<droneShotAt||droneCamera==null)return false;droneShotAt=Time.unscaledTime+.3f;Ray ray=droneCamera.ViewportPointToRay(new Vector3(Mathf.Clamp01(viewport.x),Mathf.Clamp01(viewport.y),0));if(!Physics.Raycast(ray,out var hit,18,~(1<<2),QueryTriggerInteraction.Ignore))return false;var adult=hit.collider.GetComponentInParent<Villager>();var family=hit.collider.GetComponentInParent<SkyResidentInfo>();if(adult!=null&&!adult.IsFriend){adult.RegisterHit();DronePursuit.Begin(adult.transform,this,villageDrone);Notice="无人机光点碰到村民，他发现无人机，转身躲开。";return true;}if(family!=null){family.RegisterHit();DronePursuit.Begin(family.transform,this,villageDrone);Notice="无人机光点碰到居民，居民跑开了；不会出现血腥伤害。";return true;}Notice="光点落在前面的地面或建筑上。";return false;
    }
    void RenderDrone(bool force=false){if(droneCamera==null||!force&&Time.unscaledTime<droneRenderAt)return;droneRenderAt=Time.unscaledTime+.08f;droneCamera.Render();}
    void DrawDronePanel(float w,float h)
    {
        float x=w/2-460,y=h/2-290;Panel(new Rect(x,y,920,580),new Color(.08f,.13f,.19f,.98f));Label(new Rect(x+20,y+12,680,35),"密室 · 无人机观察电视",25,Color.white);if(GUI.Button(new Rect(x+785,y+12,115,35),"关闭 · 4/Esc")){CloseDronePanel();return;}
        droneScreenRect=new Rect(x+20,y+66,700,394);if(droneTexture!=null)GUI.DrawTexture(droneScreenRect,droneTexture,ScaleMode.ScaleToFit);Label(new Rect(x+744,y+68,150,140),"只遥控无人机\n人物留在密室\n\n高度："+DronePosition.y.ToString("0.0")+"米",18,Color.white);
        if(GUI.Button(new Rect(x+742,y+225,158,47),droneControl?"停止控制":"开始控制")){if(droneControl)droneControl=false;else BeginDroneControl();}bool old=GUI.enabled;GUI.enabled=droneControl;if(GUI.Button(new Rect(x+742,y+292,158,45),"跟随附近村民"))FollowDroneResident();if(GUI.Button(new Rect(x+742,y+354,158,45),"停止跟随"))droneFollowing=null;GUI.enabled=old;
        Label(new Rect(x+22,y+477,875,90),droneControl?"W/↑前进，S/↓后退，A/D左右；空格上升，Shift下降，Q/E转向。\n鼠标点电视画面发出光点；4关闭，恢复人物走动。":"先点击右侧“开始控制”，才接收无人机移动指令。\n只看电视时，移动键既不会移动无人机，也不会移动人物。",18,new Color(.82f,.91f,.98f));var e=Event.current;if(droneControl&&e.type==EventType.MouseDown&&e.button==0&&droneScreenRect.Contains(e.mousePosition)){Vector2 uv=new Vector2((e.mousePosition.x-droneScreenRect.x)/droneScreenRect.width,1-(e.mousePosition.y-droneScreenRect.y)/droneScreenRect.height);DroneSignal(uv);e.Use();}
    }
}}
