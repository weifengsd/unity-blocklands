using UnityEngine;
namespace Blocklands {
 public class StrawberryDrink:MonoBehaviour {
  Transform liquid,foam;Material glass;public float Fill {get;private set;}=.85f;
  public static StrawberryDrink Create(Transform parent,Vector3 local,float height){var root=new GameObject("透明杯草莓饮料").transform;root.SetParent(parent,false);root.localPosition=local;root.localScale=Vector3.one*height;var d=root.gameObject.AddComponent<StrawberryDrink>();d.Build();return d;}
  void Build(){var a=gameObject.AddComponent<RoyalArt>();glass=new Material(Resources.Load<Shader>("GlassPane"));glass.color=new Color(.85f,.95f,1,.15f);var shell=a.Piece("透明玻璃杯",Vector3.zero,new Vector3(.68f,.5f,.68f),Color.white,PrimitiveType.Cylinder);shell.GetComponent<Renderer>().sharedMaterial=glass;liquid=a.Piece("粉色草莓液体",new Vector3(0,-.08f,0),new Vector3(.58f,.4f,.58f),new Color(1,.39f,.62f),PrimitiveType.Cylinder);foam=a.Piece("草莓奶泡液面",new Vector3(0,.32f,0),new Vector3(.58f,.024f,.58f),new Color(1,.78f,.87f),PrimitiveType.Cylinder);a.Piece("粉白吸管",new Vector3(.12f,.46f,.02f),new Vector3(.035f,.58f,.035f),Color.white);var berry=a.Piece("杯沿草莓",new Vector3(-.24f,.43f,0),Vector3.one*.19f,new Color(.94f,.1f,.29f),PrimitiveType.Sphere);a.Piece("草莓叶",berry.localPosition+Vector3.up*.1f,new Vector3(.16f,.03f,.1f),new Color(.2f,.62f,.3f));}
  public void Sip(float progress){Fill=Mathf.Lerp(.85f,.36f,Mathf.Clamp01(progress));float h=Fill*.47f;liquid.localScale=new Vector3(.58f,h,.58f);liquid.localPosition=new Vector3(0,-.47f+h,0);foam.localPosition=new Vector3(0,-.47f+2*h,0);}
  void OnDestroy(){if(glass!=null)Destroy(glass);}
 }
 public partial class Sandbox {
  public bool RequestVisitWithJ(){if(Paused||World.Region!=6||castle==null||Vector3.Distance(Player.transform.position,new Vector3(31.5f,16,15.5f))>8)return false;bool accepted=castle.RequestVisit();if(accepted)castle.PalaceGate.Set(true);return accepted;}
 }
}
