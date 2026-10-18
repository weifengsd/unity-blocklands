using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public class NpcNavigation:MonoBehaviour
    {
        CapsuleCollider body;readonly Queue<Vector3> route=new Queue<Vector3>();Vector3 routedGoal;float nextRoute;
        public static NpcNavigation Ensure(Transform actor)
        {
            var nav=actor.GetComponent<NpcNavigation>()??actor.gameObject.AddComponent<NpcNavigation>();
            if(actor.GetComponent<Collider>()==null){nav.body=actor.gameObject.AddComponent<CapsuleCollider>();nav.body.radius=.29f;nav.body.height=1.9f;nav.body.center=Vector3.up*.98f;}
            return nav;
        }
        void Update(){if(body!=null)body.enabled=!FloodResident.IsDown(transform)&&!(GetComponent<FloodMedic>()?.Flying??false)&&(transform.parent==null||!transform.parent.name.Contains("汽车"));}
        bool Clear(Vector3 from,Vector3 to)
        {
            Vector3 d=to-from;float distance=d.magnitude;if(distance<.005f)return true;float scale=Mathf.Clamp(transform.lossyScale.x,.55f,1.3f),radius=.27f*scale;
            foreach(var hit in Physics.CapsuleCastAll(from+Vector3.up*(radius+.09f),from+Vector3.up*(1.8f*scale-radius),radius,d/distance,distance+.035f,~(1<<2),QueryTriggerInteraction.Ignore))
            {if(hit.collider.transform==transform||hit.collider.transform.IsChildOf(transform))continue;if(Vector3.Dot(hit.normal,Vector3.up)>.65f)continue;return false;}return true;
        }
        Vector3 GroundStep(Vector3 p,float oldY){if(Physics.Raycast(p+Vector3.up*1.4f,Vector3.down,out var hit,3,~(1<<2),QueryTriggerInteraction.Ignore)&&Mathf.Abs(hit.point.y-oldY)<1.15f)p.y=hit.point.y+.03f;return p;}
        public void MoveTo(Vector3 goal,float speed,float dt)
        {
            if(dt<=0||speed<=0)return;Vector3 here=transform.position;
            if(Clear(here,goal)){route.Clear();transform.position=Vector3.MoveTowards(here,goal,speed*dt);return;}
            if((route.Count==0||Vector3.Distance(goal,routedGoal)>2)&&Time.unscaledTime>=nextRoute){FindRoute(goal);nextRoute=Time.unscaledTime+1;routedGoal=goal;}
            if(route.Count>0){Vector3 p=route.Peek();if(Vector3.Distance(here,p)<.16f){route.Dequeue();return;}Vector3 next=Vector3.MoveTowards(here,p,speed*dt);if(Clear(here,next))transform.position=next;else route.Clear();return;}
            Vector3 step=Vector3.ClampMagnitude(goal-here,speed*dt);step.y=0;var x=GroundStep(here+new Vector3(step.x,0,0),here.y);var z=GroundStep(here+new Vector3(0,0,step.z),here.y);if(Clear(here,x))transform.position=x;else if(Clear(here,z))transform.position=z;
        }
        void FindRoute(Vector3 goal)
        {
            route.Clear();Vector3 origin=transform.position;var start=Vector2Int.zero;var finish=new Vector2Int(Mathf.RoundToInt(goal.x-origin.x),Mathf.RoundToInt(goal.z-origin.z));
            var open=new List<Vector2Int>{start};var closed=new HashSet<Vector2Int>();var parent=new Dictionary<Vector2Int,Vector2Int>();var costs=new Dictionary<Vector2Int,float>{{start,0}};var heights=new Dictionary<Vector2Int,float>{{start,origin.y}};
            Vector2Int[] dirs={Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};Vector2Int best=start;float bestDistance=Vector2Int.Distance(start,finish);
            for(int budget=0;open.Count>0&&budget<160;budget++)
            {
                int chosen=0;float score=float.MaxValue;for(int i=0;i<open.Count;i++){float n=costs[open[i]]+Vector2Int.Distance(open[i],finish);if(n<score){score=n;chosen=i;}}
                var at=open[chosen];open.RemoveAt(chosen);if(!closed.Add(at))continue;float distance=Vector2Int.Distance(at,finish);if(distance<bestDistance){best=at;bestDistance=distance;}if(distance<1.5f){best=at;break;}
                Vector3 from=origin+new Vector3(at.x,heights[at]-origin.y,at.y);
                foreach(var direction in dirs){var next=at+direction;if(closed.Contains(next)||Mathf.Abs(next.x)>18||Mathf.Abs(next.y)>18)continue;Vector3 target=GroundStep(origin+new Vector3(next.x,from.y-origin.y,next.y),from.y);if(!Clear(from,target))continue;float cost=costs[at]+1;if(costs.TryGetValue(next,out float old)&&old<=cost)continue;costs[next]=cost;heights[next]=target.y;parent[next]=at;if(!open.Contains(next))open.Add(next);}
            }
            if(best==start)return;var reverse=new List<Vector3>();for(var at=best;at!=start;at=parent[at])reverse.Add(origin+new Vector3(at.x,heights[at]-origin.y,at.y));reverse.Reverse();foreach(var p in reverse)route.Enqueue(p);
        }
    }
    public partial class Sandbox
    {
        int companionWorld=-1;
        public Vector3 FindFollowerSpawn(int ordinal)
        {
            Vector3 origin=Player.transform.position;for(int attempt=0;attempt<16;attempt++){float angle=(ordinal*42+attempt*45)*Mathf.Deg2Rad;float radius=1.35f+(attempt/8)*1.2f;Vector3 point=origin+Player.transform.rotation*new Vector3(Mathf.Sin(angle)*radius,0,Mathf.Cos(angle)*radius);bool blocked=false;foreach(var c in Physics.OverlapCapsule(point+Vector3.up*.3f,point+Vector3.up*1.4f,.25f,~(1<<2),QueryTriggerInteraction.Ignore))if(c!=Player.Body){blocked=true;break;}if(!blocked)return point;}return origin+Vector3.up*2;
        }
    }
}
