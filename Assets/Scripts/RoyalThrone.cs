using UnityEngine;
namespace Blocklands {
 public partial class Sandbox {
  public bool RoyalThroneSeated {get;private set;} Vector3 throneExit;
  public void ToggleRoyalThrone(){
   if(RoyalThroneSeated){RoyalThroneSeated=false;Player.HomeRest(false);Player.Teleport(throneExit,180,0);Notice="你从粉晶王座起身。";return;}
   throneExit=Player.transform.position;RoyalThroneSeated=true;Player.SetFlying(false);Player.Teleport(new Vector3(31.5f,CastleSystem.FloorY(9)+.18f,34.5f),180,0);Player.HomeRest(true);Notice="你坐上粉晶王座。再按 R 起身。";
  }
  public void ClearRoyalThrone(){if(!RoyalThroneSeated)return;RoyalThroneSeated=false;Player.HomeRest(false);}
 }
 public partial class CastleSystem {
  static readonly Vector3 Throne=new Vector3(31.5f,CastleSystem.FloorY(9),35);
  public bool UseThrone(){
   if(!Game.RoyalIdentity){Game.Notice="打败公主、戴上皇冠后才能坐上王座。";return false;}
   if(Game.RoyalThroneSeated){Game.ToggleRoyalThrone();return true;}
   if(Vector3.Distance(Game.Player.transform.position,Throne)>4){Game.Notice="靠近第十层的粉晶王座，再按 R。";return false;}
   Game.ToggleRoyalThrone();return true;
  }
  public void CrownedPrincess(){Jailed=false;jailUntil=0;breachAt=-1;VisitorUntil=float.PositiveInfinity;SetGate(true);}
 }
}
