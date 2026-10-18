using UnityEngine;
namespace Blocklands
{
    public partial class Sandbox
    {
        public bool RayFromView(out RaycastHit hit,float distance,QueryTriggerInteraction triggers=QueryTriggerInteraction.Collide)
        {
            hit=default;bool found=false;float nearest=distance+1;
            foreach(var candidate in Physics.RaycastAll(Player.View.transform.position,Player.View.transform.forward,distance,~(1<<2),triggers)){
                if(candidate.collider==Player.Body||candidate.transform.IsChildOf(Player.transform)||candidate.distance>=nearest)continue;
                nearest=candidate.distance;hit=candidate;found=true;
            }
            return found;
        }
    }
}
