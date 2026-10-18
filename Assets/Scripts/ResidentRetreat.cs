using UnityEngine;
namespace Blocklands
{
    public partial class SkyHouse
    {
        public void HitResident(int index){if(game.Paused||index<0||index>=4)return;var r=riders[index];if(r.Resting)return;game.AlertVillage();r.Info.Hits++;game.Notice=r.Info.Role;if(r.Info.Hits<10)return;r.Info.Hits=0;r.RestUntil=Time.time+10;r.Body.enabled=false;r.Boarded=false;r.Route=1;r.Root.position=home+new Vector3(4.5f,-1,-1);r.ArmL.localRotation=r.ArmR.localRotation=Quaternion.identity;phaseTime=0;handLink.enabled=aimLine.enabled=false;game.Notice=(r.Info.Child?"小孩":"老人")+"暂时退回家门口休息，没有受伤。";}
        void WakeResidents(){foreach(var r in riders)if(r.RestUntil>0&&!r.Resting){r.RestUntil=0;r.Body.enabled=true;r.Route=1;r.Boarded=false;}}
        public bool ResidentResting(int index)=>riders[index].Resting;
    }
}
