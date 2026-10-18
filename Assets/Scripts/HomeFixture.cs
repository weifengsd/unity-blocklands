using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public class FixtureState {public bool Open,On;public int Channel;}
    public partial class VoxelWorld
    {
        public readonly Dictionary<Vector3Int,FixtureState> FixtureStates=new Dictionary<Vector3Int,FixtureState>();
        void RebuildFixtures(Transform chunk,int cx,int cz)
        {
            // Destroy is deferred until frame end. A second rebuild in the same frame
            // must disable every prior root, not only the first pending-destroy root.
            for(int i=chunk.childCount-1;i>=0;i--){var old=chunk.GetChild(i);if(old.name=="Home fixtures"){old.gameObject.SetActive(false);Dispose(old.gameObject);}}
            var root=new GameObject("Home fixtures").transform;root.SetParent(chunk,false);
            for(int x=cx*ChunkSize;x<(cx+1)*ChunkSize;x++)for(int z=cz*ChunkSize;z<(cz+1)*ChunkSize;z++)for(int y=MinY;y<Height;y++){
                var p=new Vector3Int(x,y,z);Block b=Get(p);if(!Blocks.Fixture(b))continue;
                if(!FixtureStates.TryGetValue(p,out var state))FixtureStates[p]=state=new FixtureState();
                var obj=new GameObject(Blocks.Names[(int)b]);obj.transform.SetParent(root,false);obj.transform.position=p;obj.AddComponent<HomeFixture>().Initialize(p,b,state);
            }
        }
    }
    public class HomeFixture:MonoBehaviour
    {
        public Vector3Int Cell;public Block Kind;[System.NonSerialized]public FixtureState State;
        Transform moving;Renderer screen;TextMesh label;Material labelMaterial;Texture2D pixels;float nextFrame,nextNpc;readonly List<Material> materials=new List<Material>();
        Material Mat(Color c,bool glass=false){var m=new Material(Resources.Load<Shader>(glass?"GlassPane":"Selection"));m.color=c;materials.Add(m);return m;}
        Transform Box(Transform root,Vector3 p,Vector3 size,Material m,bool solid=true){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.transform.SetParent(root,false);o.transform.localPosition=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=m;if(!solid){var c=o.GetComponent<Collider>();c.enabled=false;if(Application.isPlaying)Destroy(c);else DestroyImmediate(c);}return o.transform;}
        public void Initialize(Vector3Int cell,Block kind,FixtureState state)
        {
            Cell=cell;Kind=kind;State=state;var frame=Mat(new Color(.33f,.39f,.43f));
            moving=new GameObject("Movable part").transform;moving.SetParent(transform,false);
            if(kind==Block.IronDoor){moving.localPosition=new Vector3(0,0,.5f);Box(moving,new Vector3(1,1.45f,0),new Vector3(1.96f,2.9f,.12f),Mat(new Color(.65f,.73f,.78f)));for(int i=0;i<3;i++)Box(moving,new Vector3(1,.5f+i*.8f,-.075f),new Vector3(1.65f,.55f,.04f),frame);Box(moving,new Vector3(1.75f,1.3f,-.14f),new Vector3(.09f,.3f,.1f),Mat(new Color(.9f,.7f,.25f)));}
            if(kind==Block.Window){foreach(float x in new[]{.04f,.96f})Box(transform,new Vector3(x,1,.5f),new Vector3(.08f,2,.13f),frame);foreach(float y in new[]{.04f,1.96f})Box(transform,new Vector3(.5f,y,.5f),new Vector3(1,.08f,.13f),frame);moving.localPosition=new Vector3(.08f,0,.5f);Box(moving,new Vector3(.42f,1,0),new Vector3(.82f,1.82f,.045f),Mat(new Color(.5f,.85f,1,.2f),true));Box(moving,new Vector3(.42f,1,-.025f),new Vector3(.025f,1.82f,.05f),frame);}
            if(kind==Block.Curtain){Box(transform,new Vector3(.5f,2,.9f),new Vector3(1.2f,.06f,.06f),frame);moving.localPosition=new Vector3(.05f,0,.9f);var cloth=Mat(new Color(.82f,.39f,.48f));for(int i=0;i<10;i++)Box(moving,new Vector3(.045f+i*.09f,1,(i%2)*.035f),new Vector3(.095f,1.9f,.045f),cloth,false);var hit=gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(.5f,1,.9f);hit.size=new Vector3(1,2,.08f);hit.isTrigger=true;}
            if(kind==Block.Television){Box(transform,new Vector3(.5f,.55f,.5f),new Vector3(1.8f,1.15f,.18f),frame);Box(transform,new Vector3(.5f,-.08f,.5f),new Vector3(.7f,.12f,.45f),frame);var s=Box(transform,new Vector3(.5f,.56f,.602f),new Vector3(1.62f,.92f,.025f),frame);screen=s.GetComponent<Renderer>();pixels=new Texture2D(128,72,TextureFormat.RGB24,false);pixels.filterMode=FilterMode.Point;var sm=new Material(Resources.Load<Shader>("HomeScreen"));sm.mainTexture=pixels;materials.Add(sm);screen.sharedMaterial=sm;
                var text=new GameObject("Channel label");text.transform.SetParent(transform,false);text.transform.localPosition=new Vector3(.5f,1.3f,.62f);text.transform.localRotation=Quaternion.Euler(0,180,0);label=text.AddComponent<TextMesh>();label.font=Resources.Load<Font>("NotoSansCJKsc-Regular");labelMaterial=new Material(Resources.Load<Shader>("WorldText"));labelMaterial.mainTexture=label.font.material.mainTexture;materials.Add(labelMaterial);text.GetComponent<MeshRenderer>().sharedMaterial=labelMaterial;label.fontSize=36;label.characterSize=.035f;label.anchor=TextAnchor.MiddleCenter;label.color=Color.white;
            }
            Apply();
        }
        public void Toggle(){if(Kind==Block.IronDoor&&State.Open){var g=FindAnyObjectByType<Sandbox>();if(g!=null&&new Bounds(transform.position+new Vector3(1,1.45f,.5f),new Vector3(2,2.9f,.2f)).Intersects(g.Player.Body.bounds)){g.Notice="请先离开门口，再关门。";return;}}if(Kind==Block.Television)State.On=!State.On;else State.Open=!State.Open;Apply();}
        public void Channel(int delta){State.Channel=(State.Channel+delta+5)%5;Apply();}
        public int ChannelCount=>5;public string ChannelTitle=>ProgramName();public string ChannelDialogue=>Dialogue();
        string ProgramName(){if(!State.On)return "电视已关闭 · J 开启";switch(State.Channel){case 0:return "幻方世界新闻 · 现场连线";case 1:return "小熊动画 · 森林寻宝";case 2:return "自然风景 · 云谷日出";case 3:return "英语角 · Hello, friend!";default:return "故事剧场 · 窗边的约定";}}
        string Dialogue(){int beat=Mathf.FloorToInt(Time.time/3)%2;switch(State.Channel){case 0:return beat==0?"记者：村庄今天平安吗？":"村民：当然，铁门和窗户都很好！";case 1:return beat==0?"小熊：地图指向哪里？":"小兔：跟着星星走！";case 2:return beat==0?"旁白：风穿过山谷。":"旁白：水面映着第一束光。";case 3:return beat==0?"Mia: Hello, friend!":"Noah: Hello! Let's explore.";default:return beat==0?"小禾：把窗帘拉开吧。":"朋友：好呀，月光真漂亮。";}}
        void Apply(){if(Kind==Block.IronDoor||Kind==Block.Window)moving.localRotation=Quaternion.Euler(0,State.Open?95:0,0);if(Kind==Block.Curtain){moving.localScale=new Vector3(State.Open?.12f:1,1,1);var c=GetComponent<BoxCollider>();c.center=new Vector3(State.Open?.1f:.5f,1,.9f);c.size=new Vector3(State.Open?.15f:1,2,.08f);}if(label!=null)label.text=ProgramName();nextFrame=0;}
        void Update(){
            if(labelMaterial!=null){labelMaterial.mainTexture=label.font.material.mainTexture;if(label!=null&&State.On)label.text=ProgramName()+"\n"+Dialogue();}
            if(Kind==Block.IronDoor&&!State.Open&&Time.time>nextNpc){nextNpc=Time.time+1;foreach(var c in Physics.OverlapSphere(transform.position+new Vector3(1,0,.5f),2.2f))if(c.GetComponentInParent<SkyResidentInfo>()!=null||c.GetComponentInParent<Villager>()!=null){State.Open=true;Apply();break;}}
            if(pixels==null||Time.time<nextFrame)return;nextFrame=Time.time+.15f;var data=new Color[128*72];float t=Time.time;
            for(int y=0;y<72;y++)for(int x=0;x<128;x++){
                Color c=Color.black;if(State.On){c=y<22?new Color(.3f,.58f,.3f):new Color(.35f,.7f,.91f);if(State.Channel==0){c=y<18?new Color(.09f,.18f,.4f):new Color(.18f,.42f,.62f);if(x>75&&x<110&&y>28&&y<55)c=Color.yellow;if(y>4&&y<11&&((x+(int)(t*16))%50)<35)c=Color.white;}
                    if(State.Channel==1){float px=64+Mathf.Sin(t)*24,py=33+Mathf.Abs(Mathf.Sin(t*3))*8;if((x-px)*(x-px)+(y-py)*(y-py)<170)c=new Color(.67f,.38f,.18f);if((Mathf.Abs(x-px-5)<2||Mathf.Abs(x-px+5)<2)&&Mathf.Abs(y-py-2)<2)c=Color.black;}
                    if(State.Channel==2&&y<35+Mathf.Sin(x*.05f+t*.2f)*14)c=new Color(.15f,.4f,.26f);
                    if(State.Channel==3){c=y<25?new Color(.43f,.23f,.65f):new Color(.85f,.7f,.44f);float a=38+Mathf.Sin(t)*8,b=88-Mathf.Sin(t)*8;if((x-a)*(x-a)+(y-42)*(y-42)<150||(x-b)*(x-b)+(y-42)*(y-42)<150)c=new Color(.93f,.64f,.48f);}
                    if(State.Channel==4){c=y<22?new Color(.2f,.12f,.4f):new Color(.1f,.25f,.52f);float a=43+Mathf.Sin(t)*14,b=85-Mathf.Sin(t)*14;if((x-a)*(x-a)+(y-42)*(y-42)<180||(x-b)*(x-b)+(y-42)*(y-42)<180)c=Color.HSVToRGB((x+y+t*8)%128/128f,.6f,.95f);}
                }data[y*128+x]=c;
            }pixels.SetPixels(data);pixels.Apply(false);
        }
        void OnDestroy(){foreach(var m in materials)if(m!=null){if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}if(pixels!=null){if(Application.isPlaying)Destroy(pixels);else DestroyImmediate(pixels);}}
    }
}
