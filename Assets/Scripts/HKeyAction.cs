using UnityEngine;
namespace Blocklands {public partial class Sandbox {
  float hStarted;bool hTracking,hLongUsed;
  public bool ShortH {get;private set;}
  public void SampleH(bool down,bool held,bool up,float now,bool allowed){
   ShortH=false;if(!allowed){hTracking=false;hLongUsed=false;return;}
   if(down){hStarted=now;hTracking=true;hLongUsed=false;}
   if(hTracking&&!hLongUsed&&(held||up)&&now-hStarted>=.65f){hLongUsed=true;if(RoyalIdentity){EquipFlowerWand();Notice="已拿出小花魔法棒，左键施放魔法。";}else Notice="成为公主后，长按H可拿出小花魔法棒。";}
   if(up&&hTracking){ShortH=!hLongUsed;hTracking=false;}
  }
  public bool ShortRoyalTV(){if(!ShortH||castle==null)return false;var tv=castle.NearbyRoyalTV();if(tv==null)return false;Vector3 aim=tv.transform.position+Vector3.up*1.8f-Player.View.transform.position;if(Vector3.Dot(Player.View.transform.forward,aim.normalized)<.5f)return false;tv.Toggle();Notice=tv.On?tv.Programme+" · 上下键换台":"皇家电视已关闭";return true;}
}}
