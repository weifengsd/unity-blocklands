using UnityEngine;

namespace Blocklands
{
    public sealed class ExcursionWeather : MonoBehaviour
    {
        Sandbox game;Transform[] drops;float[] speeds;bool snow;System.Random random;Material weatherMaterial;
        public int VisibleFlakes=>snow&&drops!=null?drops.Length:0;
        public int VisibleRaindrops=>!snow&&drops!=null?drops.Length:0;
        public bool Raining=>!snow&&Mathf.Repeat(Time.time,85)<48;
        public void Initialize(Sandbox owner,bool snowfall)
        {
            game=owner;snow=snowfall;random=new System.Random(snow?1501:1401);
            int count=snow?180:160;drops=new Transform[count];speeds=new float[count];
            var art=gameObject.AddComponent<RoyalArt>();
            weatherMaterial=new Material(Resources.Load<Shader>("Selection"));weatherMaterial.color=snow?Color.white:new Color(.67f,.83f,1);
            for(int i=0;i<count;i++)
            {
                float x=(float)(random.NextDouble()*28-14),z=(float)(random.NextDouble()*28-14);
                float y=game.Player.transform.position.y+4+(float)random.NextDouble()*13;
                drops[i]=art.Piece(snow?"正在飘落的雪花":"正在落下的雨点",new Vector3(x,y,z),snow?Vector3.one*(.10f+(i%3)*.025f):new Vector3(.035f,.27f,.035f),snow?new Color(.98f,.99f,1):new Color(.52f,.76f,.94f),snow?PrimitiveType.Sphere:PrimitiveType.Cube);
                var renderer=drops[i].GetComponent<Renderer>();renderer.sharedMaterial=weatherMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                speeds[i]=snow?1.2f+(i%6)*.18f:5.3f+(i%7)*.35f;
            }
        }
        void Update()
        {
            if(game==null||game.Paused||drops==null)return;
            Vector3 player=game.Player.transform.position;transform.position=new Vector3(player.x,0,player.z);
            bool sheltered=false;
            if(!snow)foreach(var side in game.ExcursionSides)if(side!=null&&side.Kind==ExcursionSideKind.RainHut&&Vector3.Distance(player,side.transform.position)<2.6f){sheltered=true;break;}
            bool show=snow||Raining&&!sheltered;
            for(int i=0;i<drops.Length;i++)
            {
                if(drops[i].gameObject.activeSelf!=show)drops[i].gameObject.SetActive(show);
                if(!show)continue;
                Vector3 p=drops[i].localPosition;p.y-=speeds[i]*Time.deltaTime;
                if(snow)p.x+=Mathf.Sin(Time.time*.8f+i)*Time.deltaTime*.22f;
                if(p.y<player.y+.3f){p.y=player.y+12+(float)random.NextDouble()*6;p.x=(float)(random.NextDouble()*28-14);p.z=(float)(random.NextDouble()*28-14);}
                drops[i].localPosition=p;
            }
        }
        void OnGUI()
        {
            if(game==null||game.Paused||!snow&&!Raining)return;
            float scale=Mathf.Min(Screen.width/1000f,Screen.height/650f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
            var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("NotoSansCJKsc-Regular"),fontSize=15};style.normal.textColor=Color.white;
            Sandbox.Panel(new Rect(20,264,390,31),new Color(.11f,.21f,.3f,.78f));
            GUI.Label(new Rect(30,268,370,25),snow?"雪晶山谷 · 雪花正落下，去雪坡滑一趟吧":"萤火秘林 · 阵雨来了，树屋雨棚可以避雨",style);
        }
        void OnDestroy(){if(weatherMaterial!=null)Destroy(weatherMaterial);}
    }

    public partial class Sandbox
    {
        public ExcursionWeather ExcursionWeather {get;private set;}
        void BuildExcursionWeather()
        {
            ExcursionWeather=null;if(World.Region!=14&&World.Region!=15)return;
            var o=new GameObject(World.Region==15?"雪晶山谷真实飘雪":"萤火秘林间歇雨");o.transform.SetParent(villagePeople.transform);
            ExcursionWeather=o.AddComponent<ExcursionWeather>();ExcursionWeather.Initialize(this,World.Region==15);
        }
    }
}
