using UnityEngine;
namespace Blocklands
{
    public partial class Sandbox
    {
        public bool TryDirectFloorButton(){if(!RayFromView(out var hit,5))return false;var c=hit.collider.GetComponent<VillageControl>();if(c==null||c.Shop)return false;return PressFloorButton(c);}
        public bool PressFloorButton(VillageControl button){if(button==null||button.Shop)return false;if(button.Cabin&&!button.Lift.PlayerInside){Notice="先等电梯到站，走进轿厢后再按数字按钮。";return true;}button.Lift.RequestFloor(button.Floor);return true;}
    }
}
