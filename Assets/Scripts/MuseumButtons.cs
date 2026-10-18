using UnityEngine;
namespace Blocklands {
 public class MuseumFloorButton:MonoBehaviour {public MuseumLift Lift;public int Floor;public bool Press(){if(!Lift.Inside)return false;Lift.Choice=Floor;Lift.Confirm();return true;}}
 public partial class MuseumLift {
  public int Destination=>target;public bool DoorsOpen=>open>.95f;
  public void BuildFloorButtons(){var art=car.GetComponent<RoyalArt>();for(int f=0;f<Floors;f++){Vector3 p=new Vector3((f%4-1.5f)*.48f,2.35f-f/4*.36f,1.21f);var t=art.Piece("博物馆楼层按钮 "+(f+1),p,new Vector3(.42f,.3f,.1f),new Color(.96f,.53f,.76f),PrimitiveType.Cube,true);var b=t.gameObject.AddComponent<MuseumFloorButton>();b.Lift=this;b.Floor=f;var label=new GameObject("楼层数字");label.transform.SetParent(car,false);label.transform.localPosition=p+Vector3.back*.065f;var text=label.AddComponent<TextMesh>();text.text=(f+1).ToString();text.characterSize=.055f;text.fontSize=36;text.anchor=TextAnchor.MiddleCenter;text.color=Color.black;}}
 }
 public partial class Sandbox {
  public bool TryMuseumButton(){if(Paused||RoyalMenuOpen||!RayFromView(out var hit,5))return false;var button=hit.collider.GetComponent<MuseumFloorButton>();return button!=null&&button.Press();}
 }
}
