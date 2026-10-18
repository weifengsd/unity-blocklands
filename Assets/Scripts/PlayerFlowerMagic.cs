using UnityEngine;
namespace Blocklands {
 public partial class Sandbox {
  public bool FlowerWandActive {get;private set;}float nextFlower;
  public void EquipFlowerWand(){FlowerWandActive=true;Player.DisplayFlowerWand(true);}
  public void PutAwayFlowerWand(){FlowerWandActive=false;Player.DisplayFlowerWand(false);}
  public bool FireFlower(){if(!FlowerWandActive||Paused||Health<=0||RoyalTransportActive||Time.time<nextFlower)return false;nextFlower=Time.time+.4f;var o=GameObject.CreatePrimitive(PrimitiveType.Sphere);o.name="玩家花朵魔法";o.transform.position=Player.View.transform.position+Player.View.transform.forward*.55f;o.transform.localScale=Vector3.one*.18f;o.GetComponent<Collider>().enabled=false;Destroy(o.GetComponent<Collider>());o.AddComponent<FlowerBolt>().Initialize(this,Player.View.transform.forward);return true;}
 }
 public partial class FirstPerson {
  Transform flower;
  public void DisplayFlowerWand(bool active){if(flower==null&&active){flower=new GameObject("粉色小花魔法棒").transform;flower.SetParent(View.transform,false);flower.localPosition=new Vector3(.35f,-.5f,.8f);flower.localScale=Vector3.one*.55f;var a=flower.gameObject.AddComponent<RoyalArt>();a.Piece("花棒",new Vector3(0,.33f,0),new Vector3(.055f,.66f,.055f),new Color(.86f,.4f,.72f));a.Piece("花心",new Vector3(0,.78f,0),Vector3.one*.12f,new Color(1,.8f,.28f),PrimitiveType.Sphere);for(int i=0;i<6;i++){float t=i*Mathf.PI/3;a.Piece("花瓣",new Vector3(Mathf.Cos(t)*.13f,.78f+Mathf.Sin(t)*.13f,0),new Vector3(.16f,.16f,.06f),new Color(1,.48f,.73f),PrimitiveType.Sphere);}foreach(var t in flower.GetComponentsInChildren<Transform>())t.gameObject.layer=2;}if(flower!=null)flower.gameObject.SetActive(active&&!Game.InspectingHero&&!Game.HomeSeated);}
 }
 public class FlowerBolt:MonoBehaviour {
  Sandbox game;Vector3 v;float age;int region;Material m;
  public void Initialize(Sandbox g,Vector3 direction){game=g;region=g.World.Region;v=direction*16;m=new Material(Resources.Load<Shader>("Selection"));m.color=new Color(1,.48f,.8f);GetComponent<Renderer>().sharedMaterial=m;}
  void Update(){if(game==null){Destroy(gameObject);return;}if(game.Paused)return;age+=Time.deltaTime;if(region!=game.World.Region||age>4){Destroy(gameObject);return;}float distance=v.magnitude*Time.deltaTime;if(Physics.Raycast(transform.position,v.normalized,out var hit,distance+.1f,~(1<<2),QueryTriggerInteraction.Ignore)){var actor=hit.collider.GetComponentInParent<CombatActor>();if(actor!=null)actor.Hit(1);Destroy(gameObject);return;}transform.position+=v*Time.deltaTime;}
  void OnDestroy(){if(m!=null)Destroy(m);}
 }
}
