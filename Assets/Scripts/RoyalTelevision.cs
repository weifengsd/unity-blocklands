using UnityEngine;
namespace Blocklands {
 public class RoyalTelevision:MonoBehaviour {
  public const int Channels=240;public int ChannelIndex {get;private set;}public bool On {get;private set;}
  static readonly string[] Genres={"星空旅行","海底世界","花园列车","彩虹烟花","森林小熊","赛车时刻","晴雨日记","星光舞台"};
  public string Programme=>Genres[ChannelIndex%8]+" · 第"+(ChannelIndex/8+1)+"集";
  Texture2D texture;Material screenMaterial,textMaterial;TextMesh label;float next;Color32[] pixels;Sandbox game;
  const int W=384,H=216;
  public void Initialize(Sandbox g){game=g;var art=gameObject.AddComponent<RoyalArt>();var pink=new Color(.96f,.62f,.8f);var gold=new Color(.95f,.78f,.38f);
   art.Piece("电视矮柜",new Vector3(0,.36f,0),new Vector3(4.4f,.72f,.65f),pink);foreach(float x in new[]{-1.4f,0,1.4f}){art.Piece("金色柜把手",new Vector3(x,.36f,.34f),new Vector3(.32f,.045f,.055f),gold);}
   art.Piece("圆角粉金屏框",new Vector3(0,1.93f,0),new Vector3(4.1f,2.42f,.19f),gold);art.Piece("深色内边框",new Vector3(0,1.93f,.11f),new Vector3(3.97f,2.28f,.08f),new Color(.08f,.045f,.1f));var panel=art.Piece("高清动画屏幕",new Vector3(0,1.93f,.162f),new Vector3(3.83f,2.15f,.02f),Color.white);
   texture=new Texture2D(W,H,TextureFormat.RGB24,false);texture.filterMode=FilterMode.Bilinear;pixels=new Color32[W*H];screenMaterial=new Material(Resources.Load<Shader>("HomeScreen"));screenMaterial.mainTexture=texture;panel.GetComponent<Renderer>().sharedMaterial=screenMaterial;
   var l=new GameObject("节目名称");l.transform.SetParent(transform,false);l.transform.localPosition=new Vector3(0,3.35f,.18f);l.transform.localRotation=Quaternion.Euler(0,180,0);label=l.AddComponent<TextMesh>();label.font=Resources.Load<Font>("NotoSansCJKsc-Regular");textMaterial=new Material(Resources.Load<Shader>("WorldText"));textMaterial.mainTexture=label.font.material.mainTexture;l.GetComponent<Renderer>().sharedMaterial=textMaterial;label.characterSize=.06f;label.fontSize=36;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.3f,.15f,.32f);Paint();
  }
  public void Toggle(){On=!On;Paint();}
  public void Change(int delta){ChannelIndex=(ChannelIndex+delta+Channels)%Channels;Paint();}
  void Dot(float cx,float cy,float r,Color color){int left=Mathf.Max(0,Mathf.FloorToInt(cx-r)),right=Mathf.Min(W-1,Mathf.CeilToInt(cx+r));for(int y=Mathf.Max(0,(int)(cy-r));y<=Mathf.Min(H-1,cy+r);y++)for(int x=left;x<=right;x++)if((x-cx)*(x-cx)+(y-cy)*(y-cy)<=r*r)pixels[y*W+x]=color;}
  void Rect(int x,int y,int w,int h,Color c){for(int iy=Mathf.Max(0,y);iy<Mathf.Min(H,y+h);iy++)for(int ix=Mathf.Max(0,x);ix<Mathf.Min(W,x+w);ix++)pixels[iy*W+ix]=c;}
  void Paint(){if(texture==null)return;float t=Time.time,seed=ChannelIndex/8;int genre=ChannelIndex%8;Color top=Color.HSVToRGB((.56f+seed*.019f)%1,.5f,genre==0||genre==3?.2f:.85f);for(int y=0;y<H;y++){Color c=On?Color.Lerp(new Color(.035f,.025f,.1f),top,y/(float)H):new Color(.02f,.02f,.025f);for(int x=0;x<W;x++)pixels[y*W+x]=c;}
   if(On){switch(genre){case 0:for(int i=0;i<45;i++)Dot((i*67+seed*11+t*(4+i%4))%W,(i*37+seed*9)%H,1+i%2,Color.white);Dot(240,130,38,Color.HSVToRGB(seed*.03f,.55f,.95f));Dot(236,137,27,top);break;
    case 1:for(int i=0;i<12;i++){float x=(i*51+t*(8+i%3)+seed*17)%W,y=35+i*13+Mathf.Sin(t+i)*8;Dot(x,y,9,Color.HSVToRGB(i*.09f,.7f,.95f));Dot(x+5,y+2,2,Color.black);}for(int i=0;i<15;i++)Dot((i*43)%W,(t*18+i*29)%H,2,Color.cyan);break;
    case 2:Rect(0,0,W,45,new Color(.2f,.48f,.24f));for(int i=0;i<8;i++){float x=(i*65-t*14+W*20)%W;Dot(x,80,25,new Color(.2f,.55f,.3f));}for(int i=0;i<4;i++){int x=(int)((t*26+i*65+seed*9)%(W+260))-240;Rect(x,35,60,37,Color.HSVToRGB(i*.18f,.6f,1));Rect(x+8,50,20,15,Color.white);Dot(x+10,33,7,Color.black);Dot(x+48,33,7,Color.black);}break;
    case 3:for(int i=0;i<5;i++){float age=(t*.45f+i*.21f)%1;for(int p=0;p<24;p++){float angle=p*Mathf.PI/12;Dot(45+i*72+Mathf.Cos(angle)*age*40,110+Mathf.Sin(angle)*age*40,2,Color.HSVToRGB((i*.2f+seed*.03f)%1,.7f,1-age*.5f));}}break;
    case 4:Rect(0,0,W,65,new Color(.28f,.6f,.25f));float bear=180+Mathf.Sin(t)*80;Dot(bear,85,33,new Color(.58f,.3f,.14f));Dot(bear-22,128,13,new Color(.58f,.3f,.14f));Dot(bear+22,128,13,new Color(.58f,.3f,.14f));Dot(bear,111,29,new Color(.73f,.47f,.25f));Dot(bear-9,118,3,Color.black);Dot(bear+9,118,3,Color.black);Dot(bear,101,6,new Color(.2f,.1f,.05f));break;
    case 5:Rect(70,0,244,H,new Color(.22f,.23f,.29f));for(int i=0;i<8;i++)Rect(187,(int)((i*35-t*70+H*10)%H),5,18,Color.white);for(int i=0;i<3;i++){int x=100+i*73,y=(int)((t*20+i*85+seed*7)%(H+50))-50;Rect(x,y,33,52,Color.HSVToRGB(i*.3f+seed*.01f,.8f,1));Rect(x+4,y+28,25,14,Color.cyan);}break;
    case 6:Rect(0,0,W,55,new Color(.25f,.52f,.3f));Dot(290,170,23,Color.yellow);for(int i=0;i<6;i++)Dot((i*90+t*7)%W,135+i%2*25,22,Color.white);if(ChannelIndex/8%2==1)for(int i=0;i<70;i++)Rect((i*43)%W,(int)((i*27-t*80+H*100)%H),2,6,Color.cyan);break;
    default:Rect(0,0,W,55,new Color(.55f,.2f,.45f));for(int i=0;i<5;i++){float x=50+i*70,y=85+Mathf.Abs(Mathf.Sin(t*2+i))*30;Dot(x,y+25,12,new Color(.96f,.72f,.53f));Rect((int)x-10,(int)y-15,20,32,Color.HSVToRGB(i*.2f,.6f,1));Dot(x+Mathf.Sin(t*3+i)*24,y+3,5,Color.white);}break;}}
   texture.SetPixels32(pixels);texture.Apply(false);label.text=On?"频道 "+(ChannelIndex+1)+" / 240 · "+Programme:"皇家电视 · G开启 · 240个本地动画节目";next=Time.time+.12f;
  }
  void Update(){if(game==null||game.Paused)return;if(textMaterial!=null)textMaterial.mainTexture=label.font.material.mainTexture;if(Time.time>=next&&On)Paint();}
  void OnDestroy(){if(texture!=null)Destroy(texture);if(screenMaterial!=null)Destroy(screenMaterial);if(textMaterial!=null)Destroy(textMaterial);}
 }
}
