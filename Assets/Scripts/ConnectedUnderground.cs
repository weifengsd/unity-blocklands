using UnityEngine;
namespace Blocklands {
 public partial class VoxelWorld {
  void GenerateConnectedUnderground(){for(int x=0;x<Width;x++)for(int z=0;z<Depth;z++)for(int y=MinY;y<0;y++){
   int d=-y;Block b=d<32?Block.Stone:d<61?Block.DarkStone:d<91?Blocks.DenseRock:d<161?Blocks.StarRock:d<186?Block.Stone:d<211?Block.Obsidian:Blocks.StarRock;
   // Irregular chambers and winding passages remain joined in one voxel array.
   float centre=30+Mathf.Sin(y*.05f)*8;bool shaft=d>5&&Mathf.Abs(x-centre)<2&&Mathf.Abs(z-(27+Mathf.Cos(y*.055f)*6))<2;
   bool tunnel=d>9&&d<120&&Mathf.Abs(y-(-18-Mathf.Sin(x*.13f+z*.06f)*7))<2.2f;
   bool lavaCave=y<-32&&y>-50&&(Mathf.Abs(z-(30+Mathf.Sin(x*.13f)*12))<9||x>27&&x<34);
   bool ironCave=y<-66&&y>-76&&Mathf.Abs(z-(26+Mathf.Sin(x*.15f)*14))<5;
   bool crystalCave=y<-96&&y>-110&&((x-32)*(x-32)+(z-30)*(z-30)<650);
   bool sea=y<=StarCeiling(x,z)&&y>=-154&&x>2&&x<61&&z>2&&z<61;
   bool garden=y<=-163&&y>=-179&&x>5&&x<58&&z>5&&z<58;
   bool glow=y<=-216&&y>=-230&&Mathf.Abs(z-32-Mathf.Sin(x*.11f)*10)<12;
   if(shaft||tunnel||lavaCave||ironCave||crystalCave||sea||garden||glow)b=Block.Air;
   if(lavaCave&&y==-49&&!(x>=28&&x<=34))b=Block.Lava;
   if(sea&&y<=-137)b=Block.Water;
   if(garden&&y==-179)b=Block.Grass;
   if(garden&&y==-178&&x%7==0&&z%9==0)b=Block.CrystalLamp;
   if(glow&&y==-229&&x%8==0)b=Blocks.PinkCrystal;
   if(b!=Block.Air&&b!=Block.Water&&b!=Block.Lava&&x%11==0&&z%7==0&&d%9==0)b=d<90?Block.IronOre:Blocks.PinkCrystal;
   Put(x,y,z,b);
  }}
  public static int StarCeiling(float x,float z)=>-124-Mathf.RoundToInt(Mathf.Sin(x*.14f)*2+Mathf.Cos(z*.12f)*2);
  public static string DepthTitle(float y)=>y>=0?"地表":y>-32?"石灰岩与天然洞道":y>-61?"熔岩洞窟":y>-91?"深铁矿层":y>-121?"星晶洞窟":y>-161?"星海与海底岩床":y>-186?"地心花园":y>-211?"黑曜岩基底":y>-236?"辉光水晶洞":y>=MinY?"原初坚岩层":y>Sandbox.RealmEntryY?"太空中的长距离坠落":y>Sandbox.HeavenFloor-45?"天空中的天堂":y>Sandbox.HellFloor+120?"巨大星球与生活大陆":"星球下方的地狱";
 }
 public partial class Sandbox {
  public StarSeaBoat SeaBoat {get;private set;}
  public bool DepthBoatRiding=>SeaBoat!=null&&SeaBoat.Riding;
  public bool MineConnectedRock(Vector3Int p){Block b=World.Get(p);if(b==Block.Air||b==Block.Water)return false;
   if((b==Blocks.DenseRock||b==Blocks.StarRock)&&p.y>-155&&Selected!=(int)Blocks.IronDrill&&Selected!=(int)Blocks.StarDrill&&Selected!=(int)Blocks.CombinedStarPick){Notice="这种坚硬岩石需要铁钻、星晶镐或结合星镐。";return false;}
   if(p.y<=-155&&Selected!=(int)Blocks.CombinedStarPick){Notice="这片深层岩床需要结合星镐。";return false;}
   if(p.y<=-155){if(Inventory[Selected]<1)return false;if(depthCell!=p){depthCell=p;depthWork=0;}depthWork+=Blocks.DigDelay((Block)Selected,b);if(depthWork<.65f){Notice="结合星镐钻穿硬岩："+Mathf.RoundToInt(depthWork/.65f*100)+"%";return false;}depthWork=0;}
   if(!Blocks.Buildable(b)||!World.Edit(p,Block.Air))return false;Inventory[(int)b]=Mathf.Min(9999,Inventory[(int)b]+1);Notice="已挖开"+Blocks.Names[(int)b]+"；上方和下方都连接在这个世界里。";return true;
  }
  public bool VisitConnectedDepth(int region){if(Health<=0)return false;if(region>=7&&region>DeepestVisit){Notice="先沿洞穴向下探索，发现后才能直接返回。";return false;}
   if(World.Region==5||World.Region>=7&&World.Region<=10){if(!TravelTo(0))return false;}
   float y=region==5?-40:region==7?-71:region==8?-102:region==9?-135.5f:-174;
   CloseRoyalBook();ResetHomePose();ClearRoyalThrone();Player.Teleport(FindOpenDepth(new Vector3(31.5f,y,30.5f)),0,0);if(region==9&&SeaBoat!=null)SeaBoat.Board();SetPaused(false);Save();Notice=VoxelWorld.DepthTitle(y)+"：与地面相连。F飞行，空格上升，Shift下降；可沿自己挖的洞原路返回。";return true;
  }
  public Vector3 FindOpenDepth(Vector3 desired){for(int radius=0;radius<=28;radius++)for(int dx=-radius;dx<=radius;dx++)for(int dz=-radius;dz<=radius;dz++){if(radius>0&&Mathf.Abs(dx)!=radius&&Mathf.Abs(dz)!=radius)continue;Vector3 p=desired+new Vector3(dx,0,dz);var cell=Vector3Int.FloorToInt(p);if(cell.x<2||cell.x>61||cell.z<2||cell.z>61||Blocks.Solid(World.Get(cell))||Blocks.Solid(World.Get(cell+Vector3Int.up)))continue;for(int depth=1;depth<=8;depth++){Block b=World.Get(cell-Vector3Int.up*depth);if(b==Block.Lava)break;if(Blocks.Solid(b))return p;}}return desired;}
  void BuildConnectedScenery(){if(SeaBoat!=null)Destroy(SeaBoat.gameObject);var root=new GameObject("连续地下星海");root.transform.SetParent(villagePeople.transform);var art=root.AddComponent<RoyalArt>();for(int i=0;i<72;i++){float x=5+(i*17)%54,z=5+(i*23)%54;var star=art.Piece("洞顶星辰",new Vector3(x,VoxelWorld.StarCeiling(x,z)+.7f,z),Vector3.one*(i%3==0?.19f:.09f),new Color(.7f,.88f,1),PrimitiveType.Sphere);star.GetComponent<Renderer>().sharedMaterial.shader=Resources.Load<Shader>("Selection");}var boat=new GameObject("星海小船");boat.transform.SetParent(root.transform);SeaBoat=boat.AddComponent<StarSeaBoat>();SeaBoat.Initialize(this);BuildVoidRealms(root.transform);}
  public void UpdateConnectedDepth(){float y=Player.transform.position.y;if(y<-34)DeepestVisit=Mathf.Max(DeepestVisit,5);if(y<-66)DeepestVisit=Mathf.Max(DeepestVisit,7);if(y<-96)DeepestVisit=Mathf.Max(DeepestVisit,8);if(y<-122)DeepestVisit=Mathf.Max(DeepestVisit,9);if(y<-163)DeepestVisit=Mathf.Max(DeepestVisit,10);if(y<VoxelWorld.MinY-8&&!voidFallAnnounced){voidFallAnnounced=true;Notice="已挖穿世界底部，还要继续坠落几百米才会见到天堂和大星球。不会自动飞行。";}if(y<HeavenFloor-50&&!voidPlanetAnnounced){voidPlanetAnnounced=true;Notice="离开天堂后继续坠落，巨大星球出现在下方；这次能落到有居民和房子的土地上。";}if(y<HellFloor+120&&!voidHellAnnounced){voidHellAnnounced=true;Notice="穿过大星球下方的长距离坠落，正在接近地狱。没有自动飞行。";}if(y>VoxelWorld.MinY){voidFallAnnounced=voidPlanetAnnounced=voidHellAnnounced=false;}}
 }
 public class StarSeaBoat:MonoBehaviour {
  Sandbox game;public bool Riding {get;private set;}public void Initialize(Sandbox g){game=g;transform.position=new Vector3(32,-135.7f,22);var a=gameObject.AddComponent<RoyalArt>();var wood=new Color(.5f,.27f,.13f);a.Piece("船底",Vector3.zero,new Vector3(2.4f,.25f,4),wood,PrimitiveType.Cube,true);foreach(float x in new[]{-1.15f,1.15f})a.Piece("船舷",new Vector3(x,.35f,0),new Vector3(.16f,.6f,4),wood);foreach(float z in new[]{-1.9f,1.9f})a.Piece("船头",new Vector3(0,.3f,z),new Vector3(2.4f,.55f,.15f),wood);a.Piece("船椅",new Vector3(0,.28f,.3f),new Vector3(2,.2f,.4f),new Color(.72f,.45f,.24f));}
  public void Board(){Riding=true;game.Player.Teleport(transform.position+Vector3.up*.4f);game.Player.HomeRest(true);game.Notice="星海小船：W/S与左右方向键划船，A拿手机；空格跳入海中，靠近船按H再上船。";}
  public bool Interact(){if(Riding)return false;if(Vector3.Distance(game.Player.transform.position,transform.position)>4)return false;Board();return true;}
  public void JumpOff(){if(!Riding)return;Riding=false;game.Player.HomeRest(false);game.Player.Teleport(transform.position+Vector3.right*2.8f+Vector3.up*.6f);game.Notice="已跳入星海，空格上浮，Ctrl下潜；海底硬岩用结合星镐。";}
  void Update(){if(game==null||game.Paused||game.PhoneOpen)return;if(Riding){if(!game.IsSmoke&&Input.GetKeyDown(KeyCode.Space)){JumpOff();return;}if(Vector3.Distance(game.Player.transform.position,transform.position)>5){Riding=false;return;}Vector3 before=transform.position;float x=game.IsSmoke?0:(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.LeftArrow)?1:0),z=game.IsSmoke?0:Input.GetAxisRaw("Vertical");transform.position=new Vector3(Mathf.Clamp(before.x+x*Time.deltaTime*4,5,59),-135.7f+Mathf.Sin(Time.time)*.035f,Mathf.Clamp(before.z+z*Time.deltaTime*4,5,59));game.Player.Carry(transform.position-before);}}
  void OnGUI(){if(game==null||game.Paused||game.Player.transform.position.y>-5)return;var style=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("NotoSansCJKsc-Regular"),fontSize=16};style.normal.textColor=Color.white;GUI.Label(new Rect(20,135,750,32),VoxelWorld.DepthTitle(game.Player.transform.position.y)+" · 深度 "+Mathf.RoundToInt(-game.Player.transform.position.y)+"米 · 同一世界，可挖通上下返回",style);if(Mathf.Abs(game.Player.transform.position.y+136)<20)GUI.Label(new Rect(20,170,750,32),Riding?"W/S与左右方向键划船 · 空格跳下船":"星海：靠近小船按H登船；Ctrl下潜，空格上浮",style);if(game.InHeaven)GUI.Label(new Rect(20,170,1100,58),"天堂在天空：找爸爸妈妈和小孩 · G玩云秋千/野餐/疗愈泉 · 走入带护栏的洞口继续向下",style);else if(game.InHell)GUI.Label(new Rect(20,170,1100,58),"地狱在星球下方：躲开熔岩、幽灵和深坑 · 坑底按G爬出 · F才会自行起飞",style);else if(game.InVoidRealms)GUI.Label(new Rect(20,170,1100,58),"星球在身旁，继续往下坠才到地狱；此处绝不自动飞行",style);}
 }
}
