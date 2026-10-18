using UnityEngine;

namespace Blocklands
{
    [RequireComponent(typeof(CharacterController))]
    public partial class FirstPerson : MonoBehaviour
    {
        public Sandbox Game;
        public Camera View {get;private set;}
        public CharacterController Body {get;private set;}
        public bool Swimming {get;private set;}
        public bool Flying {get;private set;}
        public bool CanUseFlightF=>Game!=null&&(Game.World.Region!=4||Game.InVoidRealms);
        public const float FlightCeiling=100;
        public float Pitch {get;private set;}
        float fallSpeed,step;
        float lastJumpTap=-10;
        Transform pickaxe;
        Transform hand;
        Light lantern;
        public void Initialize(Sandbox game)
        {
            Game=game;Body=GetComponent<CharacterController>();
            Body.height=1.8f;Body.center=new Vector3(0,.9f,0);Body.radius=.3f;
            Body.stepOffset=.5f;Body.skinWidth=.035f;Body.slopeLimit=50;
            var cameraObject=new GameObject("First-person camera");cameraObject.tag="MainCamera";
            cameraObject.transform.SetParent(transform,false);cameraObject.transform.localPosition=new Vector3(0,1.65f,0);
            View=cameraObject.AddComponent<Camera>();View.fieldOfView=75;View.nearClipPlane=.05f;View.farClipPlane=150;
            View.clearFlags=CameraClearFlags.SolidColor;View.backgroundColor=new Color(.64f,.79f,.88f);View.allowHDR=true;
            cameraObject.AddComponent<AudioListener>();
            lantern=cameraObject.AddComponent<Light>();lantern.type=LightType.Point;lantern.range=18;lantern.intensity=2;lantern.color=new Color(1,.85f,.55f);lantern.enabled=false;
            hand=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            hand.name="Block builder's hand";hand.SetParent(cameraObject.transform,false);
            hand.localPosition=new Vector3(.42f,-.38f,.65f);hand.localScale=new Vector3(.18f,.2f,.48f);
            hand.localRotation=Quaternion.Euler(-15,-15,0);hand.gameObject.layer=2;
            Destroy(hand.GetComponent<Collider>());
            var material=new Material(Resources.Load<Shader>("Selection"));material.color=new Color(.76f,.51f,.32f);
            hand.GetComponent<Renderer>().sharedMaterial=material;
            pickaxe=new GameObject("Equipped pickaxe").transform;pickaxe.SetParent(hand,false);
            var shaft=GameObject.CreatePrimitive(PrimitiveType.Cube);shaft.transform.SetParent(pickaxe,false);shaft.transform.localPosition=new Vector3(0,1.4f,0);shaft.transform.localScale=new Vector3(.32f,3,.3f);shaft.layer=2;Destroy(shaft.GetComponent<Collider>());shaft.GetComponent<Renderer>().sharedMaterial=material;
            var head=GameObject.CreatePrimitive(PrimitiveType.Cube);head.transform.SetParent(pickaxe,false);head.transform.localPosition=new Vector3(0,2.8f,0);head.transform.localScale=new Vector3(2.7f,.5f,.4f);head.layer=2;Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial=new Material(material);pickaxe.gameObject.SetActive(false);
        }
        public void Teleport(Vector3 position,float yaw=0,float pitch=0)
        {
            if(Game!=null&&Game.PalacePlayLocked)Game.PalaceLeisure.StopRide();
            if(Game!=null&&Game.FestivalLocked)Game.CancelFestivalForNavigation();
            Body.enabled=false;transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));Body.enabled=true;
            fallSpeed=0;Flying=false;lastJumpTap=-10;Pitch=pitch;View.transform.localRotation=Quaternion.Euler(Pitch,0,0);
        }
        public void ResetJumpTap()=>lastJumpTap=-10;
        public void HomeRest(bool active,bool bed=false){View.transform.localPosition=new Vector3(0,active?(bed?.65f:1):1.65f,0);hand.gameObject.SetActive(!active);if(flower!=null)flower.gameObject.SetActive(!active&&Game.FlowerWandActive);}
        public void Carry(Vector3 delta){Body.enabled=false;transform.position+=delta;Body.enabled=true;fallSpeed=0;}
        public void SetFlying(bool value)
        {
            Flying=value;fallSpeed=0;
            if(value&&Body.isGrounded)Body.Move(Vector3.up*.15f);
        }
        public bool JumpTap(float now)
        {
            if(now-lastJumpTap<=.35f){lastJumpTap=-10;SetFlying(!Flying);return true;}
            lastJumpTap=now;return false;
        }
        public void FlyStep(Vector2 input,float lift,bool fast,float dt)
        {
            if(!Flying||Game.Paused)return;
            input=Vector2.ClampMagnitude(input,1);
            Vector3 velocity=(transform.right*input.x+transform.forward*input.y)*(fast?12:7)+Vector3.up*(Mathf.Clamp(lift,-1,1)*6);
            velocity.y=Mathf.Min(velocity.y,Mathf.Max(0,FlightCeiling-transform.position.y)/Mathf.Max(.0001f,dt));
            Body.Move(velocity*dt);
        }
        void Update()
        {
            if(Game==null)return;
            if(Game.Paused){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;return;}
            if(Game.CarRiding){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;if(!Game.IsSmoke){CoasterLook(Input.GetAxisRaw("Mouse X")*2,Input.GetAxisRaw("Mouse Y")*2);Game.MatchCarView();}return;}
            if(Game.StickerOpen){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;return;}
            if(Game.SecretMonitorOpen){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;return;}
            if(Game.DeepInputCaptured){Cursor.lockState=Game.SecretChairMenuOpen||Game.DronePanelOpen||Game.DeepActivityActive?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=Cursor.lockState==CursorLockMode.None;if(!Cursor.visible&&!Game.IsSmoke)CoasterLook(Input.GetAxisRaw("Mouse X")*2,Input.GetAxisRaw("Mouse Y")*2);return;}
            if(Game.PhoneInputCaptured){Cursor.lockState=Game.PhoneOpen?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=Game.PhoneOpen;return;}
            if(Game.ExcursionActive||Game.SideRideActive)return;
            if(Game.PalaceConsoleOpen){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;return;}
            if(Game.PalacePlayLocked||Game.SecretSwingActive||Game.PartyRideActive){if(!Game.IsSmoke)CoasterLook(Input.GetAxisRaw("Mouse X")*2,Input.GetAxisRaw("Mouse Y")*2);return;}
            Cursor.lockState=Game.RoyalMenuOpen?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=Game.RoyalMenuOpen;
            if(Game.RoyalMenuOpen)return;
            if(Game.FestivalLocked){if(Game.CoasterRiding&&!Game.IsSmoke)CoasterLook(Input.GetAxisRaw("Mouse X")*2,Input.GetAxisRaw("Mouse Y")*2);return;}
            bool toggle=!Game.IsSmoke&&Input.GetKeyDown(KeyCode.Space)&&JumpTap(Time.unscaledTime);
            if(!Game.IsSmoke&&Input.GetKeyDown(KeyCode.F)&&CanUseFlightF&&Game.PortalKeyConsumedFrame!=Time.frameCount){SetFlying(!Flying);ResetJumpTap();toggle=true;}
            float dx=Game.IsSmoke?0:Input.GetAxisRaw("Mouse X")*2,dy=Game.IsSmoke?0:Input.GetAxisRaw("Mouse Y")*2;
            transform.Rotate(0,dx,0);Pitch=Mathf.Clamp(Pitch-dy,-85,85);View.transform.localRotation=Quaternion.Euler(Pitch,0,0);
            if(Game.HomeSeated||Game.HoldingBook||Game.RoyalThroneSeated||Game.RoyalMenuOpen||Game.RoyalTransportActive||Game.DepthBoatRiding||Game.VoidRideActive)return;
            Vector2 input=new Vector2((Held(KeyCode.D,KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.LeftArrow)?1:0),
                (Held(KeyCode.W,KeyCode.UpArrow)?1:0)-(Held(KeyCode.S,KeyCode.DownArrow)?1:0));
            input=Vector2.ClampMagnitude(input,1);
            Swimming=Game.World.Get(Vector3Int.FloorToInt(transform.position+Vector3.up*.7f))==Block.Water;
            float speed=Swimming?3:Input.GetKey(KeyCode.LeftShift)?8:5;
            Vector3 velocity=(transform.right*input.x+transform.forward*input.y)*speed;
            if(Flying)
            {
                float lift=Game.IsSmoke?0:(Input.GetKey(KeyCode.Space)?1:0)-(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?1:0);
                FlyStep(input,lift,Input.GetKey(KeyCode.LeftControl),Time.deltaTime);
            }
            else if(Swimming) fallSpeed=!Game.IsSmoke&&Input.GetKey(KeyCode.Space)?4:!Game.IsSmoke&&Input.GetKey(KeyCode.LeftControl)?-3:-.7f;
            else
            {
                if(Body.isGrounded && fallSpeed<0)fallSpeed=-2;
                if(!Game.IsSmoke&&Body.isGrounded && Input.GetKeyDown(KeyCode.Space)&&!toggle)fallSpeed=8;
                fallSpeed=Mathf.Max(-25,fallSpeed-22*Time.deltaTime);
            }
            velocity.y=fallSpeed;
            CollisionFlags flags=Flying?CollisionFlags.None:Body.Move(velocity*Time.deltaTime);
            if((flags&CollisionFlags.Above)!=0 && fallSpeed>0)fallSpeed=0;
            Vector3 p=transform.position;
            if(!Game.InDeepBase)Game.UpdateConnectedDepth();
            step+=Time.deltaTime*input.magnitude*speed;
            bool atControls=Game.RayFromView(out var nearby,3,QueryTriggerInteraction.Ignore)&&(nearby.collider.GetComponentInParent<SkyHouse>()!=null||nearby.collider.GetComponentInParent<RoyalLift>()!=null);
            hand.localPosition=new Vector3(.42f,(atControls?-.82f:-.38f)+Mathf.Sin(step*2.8f)*.015f,.65f);
            bool underwater=Game.World.Get(Vector3Int.FloorToInt(View.transform.position))==Block.Water;
            RenderSettings.fogColor=underwater?new Color(.12f,.4f,.5f):Game.ActiveSkyColor;
            RenderSettings.fogDensity=underwater?.1f:Game.InVoidRealms?(Game.InHell?.008f:.003f):.013f;
            View.backgroundColor=RenderSettings.fogColor;
            bool equipped=false; // HeldItems renders the exact selected object, including non-tools.
            lantern.enabled=Game.Selected==(int)Block.Torch&&Game.Inventory[Game.Selected]>0;
            pickaxe.gameObject.SetActive(equipped);
            if(equipped)
            {
                var head=pickaxe.GetChild(1);head.GetComponent<Renderer>().sharedMaterial.color=Blocks.ColorOf((Block)Game.Selected);
                var shaft=pickaxe.GetChild(0);bool hammer=Game.Selected==(int)Block.CrystalHammer;pickaxe.localScale=hammer?Vector3.one*.24f:Vector3.one*.34f;
                bool shovel=Game.Selected>=(int)Block.WoodShovel&&Game.Selected<=(int)Block.IronShovel;
                bool axe=Game.Selected>=(int)Block.WoodAxe&&Game.Selected<=(int)Block.IronAxe;
                bool sword=Game.Selected==90||Game.Selected>=(int)Block.WoodSword&&Game.Selected<=(int)Block.IronSword;
                shaft.localScale=hammer?new Vector3(.32f,2.35f,.3f):new Vector3(.32f,3,.3f);
                head.localPosition=new Vector3(0,hammer?2.25f:sword?2.3f:2.8f,0);
                head.localScale=Game.Selected==85?new Vector3(.75f,1.2f,.7f):hammer?new Vector3(2.15f,1.1f,.85f):sword?new Vector3(.6f,2.2f,.3f):shovel?new Vector3(1.3f,1.3f,.4f):axe?new Vector3(1.9f,1.3f,.4f):new Vector3(2.7f,.5f,.4f);
            }
        }
        bool Held(KeyCode a,KeyCode b)=>!Game.IsSmoke&&(Input.GetKey(a)||Input.GetKey(b));
        void OnDestroy(){if(pickaxe!=null)Destroy(pickaxe.GetChild(1).GetComponent<Renderer>().sharedMaterial);if(hand!=null)Destroy(hand.GetComponent<Renderer>().sharedMaterial);}
    }
}
