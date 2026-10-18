using UnityEngine;
namespace Blocklands {
 public class CrownChallenge:CombatActor {
  public bool StartDuel()=>engaged||BeginDuel();
  Sandbox g;Transform crown,wand;float started,nextShot,lastHit=-10;bool engaged;int hits;public int Hits=>hits;public bool Active=>engaged;public bool Ready=>engaged&&Time.time-started>=1.6f;public int Shots{get;private set;}
  public override string Caption=>engaged?"公主对决 · 命中 "+hits+"/30 · 躲避魔法":"公主 · 瞄准按H发起30击挑战";
  public void Initialize(Sandbox game){g=game;var c=gameObject.AddComponent<BoxCollider>();c.center=new Vector3(0,1.3f,0);c.size=new Vector3(1,2.5f,1);var figure=GetComponentInChildren<RoyalArt>();if(figure!=null){crown=new GameObject("皇冠变魔杖").transform;crown.SetParent(figure.transform,false);crown.localPosition=new Vector3(0,1.95f,0);var parts=figure.GetComponentsInChildren<Transform>();foreach(var part in parts)if(part.name=="皇冠"||part.name=="冠尖")part.SetParent(crown,true);wand=figure.Piece("皇冠魔法棒柄",new Vector3(.34f,.95f,.32f),new Vector3(.065f,.9f,.065f),new Color(1,.75f,.22f));wand.gameObject.SetActive(false);}}
  bool BeginDuel(){if(g.RoyalIdentity)return false;if(g.Paused||g.Health<=0||!gameObject.activeInHierarchy||Vector3.Distance(g.Player.transform.position,transform.position)>9)return false;engaged=true;hits=0;Shots=0;started=Time.time;nextShot=Time.time+2.4f;lastHit=-10;var castle=GetComponentInParent<CastleSystem>();castle.GrantChallengePermit();g.EquipFlowerWand();if(wand!=null)wand.gameObject.SetActive(true);g.Notice="公主摘下皇冠，变出魔法棒！左键攻击，命中30次获胜；W/S与方向键躲避魔法，A随时拿手机。";return true;}
  public override void Hit(float amount){if(amount<=0||g.Paused||g.Health<=0)return;if(!engaged){StartDuel();return;}if(!Ready||Time.time-lastHit<.15f)return;lastHit=Time.time;hits++;g.Notice="公主挑战："+hits+" / 30";if(hits==30){engaged=false;if(crown!=null)crown.localPosition=new Vector3(0,1.95f,0);if(wand!=null)wand.gameObject.SetActive(false);GetComponentInParent<CastleSystem>().CompleteSuccession(this);}}
  void Update(){if(g==null||g.Paused)return;if(g.Health<=0){engaged=false;return;}Vector3 d=g.Player.transform.position-transform.position;d.y=0;if(d.sqrMagnitude<160&&d.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(d),Time.deltaTime*4);if(!engaged)return;if(crown!=null)crown.localPosition=Vector3.Lerp(new Vector3(0,1.95f,0),new Vector3(.34f,1.4f,.32f),Mathf.Clamp01((Time.time-started)/1.6f));if(Time.time<nextShot)return;nextShot=Time.time+1.6f;Shots++;Vector3 p=transform.position+Vector3.up*1.35f+transform.forward*1.1f;var orb=GameObject.CreatePrimitive(PrimitiveType.Sphere);orb.name="公主的金色魔法";orb.transform.SetParent(transform.parent);orb.transform.position=p;orb.transform.localScale=Vector3.one*.28f;orb.GetComponent<Collider>().enabled=false;Destroy(orb.GetComponent<Collider>());orb.AddComponent<PrincessBolt>().Initialize(g,this,(g.Player.transform.position+Vector3.up-p).normalized);}
 }
 public class PrincessBolt:MonoBehaviour {
  Sandbox g;CrownChallenge owner;Vector3 velocity;float age;Material mat;
  public void Initialize(Sandbox game,CrownChallenge source,Vector3 direction){g=game;owner=source;velocity=direction*6;mat=new Material(Resources.Load<Shader>("Selection"));mat.color=new Color(1,.7f,.15f);GetComponent<Renderer>().sharedMaterial=mat;}
  void Update(){if(g.Paused)return;if(owner==null||!owner.Active||g.World.Region!=6||g.Health<=0||age>8){Destroy(gameObject);return;}age+=Time.deltaTime;Vector3 step=velocity*Time.deltaTime;if(Physics.Raycast(transform.position,velocity.normalized,out var hit,step.magnitude+.12f,~(1<<2),QueryTriggerInteraction.Ignore)){if(hit.collider==g.Player.Body)g.Damage(1,"被公主魔法击中");Destroy(gameObject);return;}transform.position+=step;}
  void OnDestroy(){if(mat!=null)Destroy(mat);}
 }
 public partial class CastleSystem {
  public bool DuelActive{get{var d=GetComponentInChildren<CrownChallenge>();return d!=null&&d.Active;}}
  public void GrantChallengePermit(){VisitorUntil=Time.time+180;breachAt=-1;}
  public bool TryStartDuel(){if(!Game.RayFromView(out var hit,7))return false;var d=hit.collider.GetComponentInParent<CrownChallenge>();return d!=null&&d.StartDuel();}
 }
}
