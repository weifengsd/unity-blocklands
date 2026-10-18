using UnityEngine;
namespace Blocklands {
 public partial class FestivalStation {
  const int CoasterSamples=2048;
  readonly System.Collections.Generic.List<Mesh> coasterMeshes=new System.Collections.Generic.List<Mesh>();
  float[] coasterLengths;float rideDistance,rideSpeed;bool coasterSeatOwned,coasterEntryFlying;
  readonly System.Collections.Generic.List<Transform> partyCars=new System.Collections.Generic.List<Transform>();
  bool coasterGripHeld;float gripReleaseTime;
  public bool CoasterGripHeld=>coasterGripHeld;
  public void SetCoasterGrip(bool held){coasterGripHeld=held;}
  public Vector3 CoasterBoardingPosition=>entry;
  public bool CoasterBoardingFlying=>coasterEntryFlying;
  public float CoasterDistance=>rideDistance;
  public float CoasterLoopLength=>coasterLengths==null?0:coasterLengths[CoasterSamples];
  public float CoasterSpeed=>rideSpeed;
  public int CoasterCompletedLaps=>CoasterLoopLength>0?Mathf.Min(2,Mathf.FloorToInt((rideDistance+.001f)/CoasterLoopLength)):0;
  public float CoasterGrade=>CoasterTangent(rideDistance).y;
  static readonly Vector3[] CoasterKnots={new Vector3(32,20,28),new Vector3(43,38,24),new Vector3(55,54,9),new Vector3(58,46,36),new Vector3(48,43,56),new Vector3(22,44,58),new Vector3(7,50,44),new Vector3(6,44,20),new Vector3(10,49,8),new Vector3(24,46,22)};
  static Vector3 CoasterCurve(float u){float f=Mathf.Repeat(u,1)*CoasterKnots.Length;int i=Mathf.FloorToInt(f),n=CoasterKnots.Length;float t=f-i;Vector3 a=CoasterKnots[(i+n-1)%n],b=CoasterKnots[i],c=CoasterKnots[(i+1)%n],d=CoasterKnots[(i+2)%n];return .5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);}
  void BuildCoaster(){track=new Vector3[CoasterSamples+1];coasterLengths=new float[CoasterSamples+1];for(int i=0;i<=CoasterSamples;i++){track[i]=CoasterCurve(i/(float)CoasterSamples);if(i>0)coasterLengths[i]=coasterLengths[i-1]+Vector3.Distance(track[i-1],track[i]);}
   // The rendered rails and the ride both use the same continuous curve.
   int rails=Mathf.CeilToInt(CoasterLoopLength/.75f);for(int i=0;i<rails;i++){float s=i*CoasterLoopLength/rails,s2=(i+1)*CoasterLoopLength/rails;Vector3 p=CoasterPoint(s),q=CoasterPoint(s2),right=Vector3.Cross(Vector3.up,CoasterTangent(s)).normalized,right2=Vector3.Cross(Vector3.up,CoasterTangent(s2)).normalized;foreach(float side in new[]{-1f,1f})Beam("连续弯曲钢轨",p+right*side*.8f,q+right2*side*.8f,.1f,new Color(.8f,.72f,.95f));if(i%2==0)Beam("枕木",p-right,p+right,.13f,new Color(.46f,.24f,.14f));if(i%16==0){var ground=FloorAt(p);if(p.y>ground.y)Beam("轨道支架",new Vector3(p.x,ground.y,p.z),p,.22f,new Color(.25f,.42f,.48f));}}
   BatchCoasterRails();cart=MakePartyCar("森林双圈家庭车",3.2f);for(int i=0;i<3;i++)partyCars.Add(MakePartyCar("伙伴车厢 "+i,2.2f));ResetCoasterCar();
  }
  Transform MakePartyCar(string name,float width){var c=new GameObject(name).transform;c.SetParent(transform);var a=c.gameObject.AddComponent<RoyalArt>();a.Piece("车厢底",Vector3.zero,new Vector3(width,.3f,3.2f),new Color(.95f,.36f,.55f));foreach(float x in new[]{-width/2,width/2})a.Piece("车厢侧壁",new Vector3(x,.35f,0),new Vector3(.1f,.6f,3.2f),new Color(.95f,.68f,.8f));a.Piece("黄色安全扶杆",new Vector3(0,1.05f,.62f),new Vector3(width,.12f,.12f),new Color(.96f,.8f,.34f));return c;}
  void PositionPartyCars(){for(int i=0;i<partyCars.Count;i++){float s=rideDistance-(i+1)*4.2f;partyCars[i].position=CoasterPoint(s);partyCars[i].rotation=Quaternion.LookRotation(CoasterTangent(s),Vector3.up);}}
  public void PartySeat(int index,out Vector3 position,out Quaternion rotation){int row=index<2?0:1+(index-2)/2;float s=rideDistance-row*4.2f;rotation=Quaternion.LookRotation(CoasterTangent(s),Vector3.up);float x=index<2?(index==0?-.75f:.75f):((index-2)%2==0?-.58f:.58f);position=CoasterPoint(s)+rotation*new Vector3(x,.4f,0);}
  public void GripTargets(Vector3 seat,Quaternion rotation,out Vector3 left,out Vector3 right){left=seat+rotation*new Vector3(-.14f,.65f,.62f);right=seat+rotation*new Vector3(.14f,.65f,.62f);}
  public Quaternion CoasterSeatRotation=>cart.rotation;
  public void HeroGripTargets(out Vector3 left,out Vector3 right){left=cart.TransformPoint(new Vector3(-.14f,1.05f,.62f));right=cart.TransformPoint(new Vector3(.14f,1.05f,.62f));}
  void BatchCoasterRails(){foreach(string name in new[]{"连续弯曲钢轨","枕木","轨道支架"}){var combine=new System.Collections.Generic.List<CombineInstance>();Material material=null;foreach(var mf in GetComponentsInChildren<MeshFilter>()){if(mf.gameObject.name!=name)continue;combine.Add(new CombineInstance{mesh=mf.sharedMesh,transform=transform.worldToLocalMatrix*mf.transform.localToWorldMatrix});if(material==null)material=mf.GetComponent<Renderer>().sharedMaterial;mf.gameObject.SetActive(false);Destroy(mf.gameObject);}var mesh=new Mesh{name="合批"+name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(combine.ToArray(),true,true);coasterMeshes.Add(mesh);var root=new GameObject("合批"+name);root.transform.SetParent(transform,false);root.AddComponent<MeshFilter>().sharedMesh=mesh;root.AddComponent<MeshRenderer>().sharedMaterial=material;}}
  void DisposeCoasterMeshes(){foreach(var mesh in coasterMeshes)if(mesh!=null)Destroy(mesh);}
  public Vector3 CoasterPoint(float distance){float s=Mathf.Repeat(distance,CoasterLoopLength);int lo=0,hi=CoasterSamples;while(hi-lo>1){int mid=(lo+hi)/2;if(coasterLengths[mid]<=s)lo=mid;else hi=mid;}float t=(s-coasterLengths[lo])/Mathf.Max(.00001f,coasterLengths[hi]-coasterLengths[lo]);return Vector3.Lerp(track[lo],track[hi],t);}
  Vector3 CoasterTangent(float s)=>(CoasterPoint(s+.22f)-CoasterPoint(s-.22f)).normalized;
  void BeginCoasterRide(){rideDistance=0;rideSpeed=3;gripReleaseTime=0;coasterGripHeld=game.IsSmoke;coasterEntryFlying=game.Player.Flying;game.Player.SetFlying(false);game.Player.BeginCoasterSeat();coasterSeatOwned=true;ResetCoasterCar();game.Player.MoveCoasterSeat(cart.position+Vector3.up*.4f);}
  void TickCoaster(float dt){if(!game.IsSmoke)coasterGripHeld=Input.GetKey(KeyCode.Space);gripReleaseTime=coasterGripHeld?0:gripReleaseTime+Mathf.Max(0,dt);if(elapsed>2&&gripReleaseTime>1.2f){Vector3 slipped=cart.position+cart.right*2+Vector3.up*.6f;Vector3 safe=entry;Cancel();game.BeginCoasterSlip(slipped,safe);return;}float left=Mathf.Max(0,dt);while(left>0&&Running){float step=Mathf.Min(left,1f/120);left-=step;float grade=CoasterTangent(rideDistance+1).y;float target=grade>.06f?Mathf.Lerp(6,3.2f,Mathf.Clamp01(grade/.65f)):grade<-.06f?Mathf.Lerp(13,24,Mathf.Clamp01(-grade/.65f)):10;float old=rideSpeed;rideSpeed=Mathf.MoveTowards(rideSpeed,target,step*(target>rideSpeed?4:6));rideDistance=Mathf.Min(CoasterLoopLength*2,rideDistance+(old+rideSpeed)*.5f*step);if(rideDistance>=CoasterLoopLength*2){Finish();return;}}
   if(!Running)return;Vector3 p=CoasterPoint(rideDistance);cart.position=p;cart.rotation=Quaternion.LookRotation(CoasterTangent(rideDistance),Vector3.up);PositionPartyCars();game.Player.MoveCoasterSeat(p+Vector3.up*.4f);
  }
  void ResetCoasterCar(){if(cart==null)return;cart.position=track[0];cart.rotation=Quaternion.LookRotation(CoasterTangent(0),Vector3.up);PositionPartyCars();}
  void EndCoasterRide(bool returnToEntry){if(coasterSeatOwned&&game!=null&&game.Player!=null){if(returnToEntry)game.Player.MoveCoasterSeat(entry);game.Player.ReleaseCoasterSeat();if(returnToEntry)game.Player.SetFlying(coasterEntryFlying);coasterSeatOwned=false;}ResetCoasterCar();}
  string CoasterStatus=>"第 "+Mathf.Min(2,CoasterCompletedLaps+1)+" / 2 圈 · "+Mathf.RoundToInt(100*rideDistance/(CoasterLoopLength*2))+"% · "+(CoasterGrade>.06f?"缓慢爬坡":CoasterGrade<-.06f?"顺坡加速":"平稳巡航")+"\n"+(coasterGripHeld?"双手正抓牢黄色扶杆":"请长按空格抓扶杆，松手太久会滑离座位")+" · J / G提前下车";
 }
 public partial class FirstPerson {
  bool coasterBodyWasEnabled,coasterPose;
  public void BeginCoasterSeat(){if(coasterPose)return;coasterPose=true;coasterBodyWasEnabled=Body.enabled;Body.enabled=false;fallSpeed=0;ResetJumpTap();}
  public void MoveCoasterSeat(Vector3 position){transform.position=position;fallSpeed=0;}
  public void ReleaseCoasterSeat(){if(!coasterPose)return;coasterPose=false;Body.enabled=coasterBodyWasEnabled;fallSpeed=0;ResetJumpTap();}
  public void CoasterLook(float horizontal,float vertical){transform.Rotate(0,horizontal,0);Pitch=Mathf.Clamp(Pitch-vertical,-85,85);View.transform.localRotation=Quaternion.Euler(Pitch,0,0);}
 }
 public partial class Sandbox {
  public FestivalStation ActiveCoaster{get{foreach(var s in FestivalStations)if(s!=null&&s.Running&&s.Kind==FestivalKind.Coaster)return s;return null;}}
  bool coasterSlipActive;Vector3 coasterSlipEntry;float coasterSlipAt;
  public bool CoasterSlipActive=>coasterSlipActive;
  public Vector3 PartyFollowOrigin=>coasterSlipActive?coasterSlipEntry:Player.transform.position;
  public void BeginCoasterSlip(Vector3 start,Vector3 safe){coasterSlipActive=true;coasterSlipEntry=safe;coasterSlipAt=Time.unscaledTime;Player.Teleport(start);Player.SetFlying(false);Damage(.5f,"松开扶杆滑出了座位；伙伴仍安全坐在车厢里");}
  void TickCoasterSlip(){if(!coasterSlipActive)return;if(Player.Body.isGrounded||Time.unscaledTime-coasterSlipAt>6){if(!Player.Body.isGrounded)Player.Teleport(coasterSlipEntry);coasterSlipActive=false;Notice="已安全落地，只有轻微摔倒。伙伴在旁边等你，没有严重伤害。";}}
  public bool LeaveCoasterWithJ(){var s=ActiveCoaster;if(s==null)return false;s.Cancel();return true;}
  public bool CoasterRiding{get{foreach(var s in FestivalStations)if(s!=null&&s.Running&&s.Kind==FestivalKind.Coaster)return true;return false;}}
  public Vector3 SafePlayerSavePosition{get{if(CarRiding)return CarSafePosition;if(PartyRideActive)return PartyRideSavePosition;if(DeepInputCaptured)return DeepSafePosition;if(SecretSwingActive)return SecretSwingEntry;if(PalacePlayLocked)return PalaceLeisure.SafeExit;foreach(var s in FestivalStations)if(s!=null&&s.Running&&s.Kind==FestivalKind.Coaster)return s.CoasterBoardingPosition;return Player.transform.position;}}
  public bool SafePlayerSaveFlying{get{if(CarRiding)return false;if(PartyRideActive)return PartyRideSaveFlying;if(DeepInputCaptured)return false;if(SecretSwingActive)return SecretSwingEntryFlying;foreach(var s in FestivalStations)if(s!=null&&s.Running&&s.Kind==FestivalKind.Coaster)return s.CoasterBoardingFlying;return Player.Flying;}}
  public void CancelFestivalForNavigation(){foreach(var s in FestivalStations)if(s!=null&&s.Running)s.Cancel();}
 }
}
