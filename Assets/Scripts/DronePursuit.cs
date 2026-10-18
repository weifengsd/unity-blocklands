using UnityEngine;
namespace Blocklands
{
    public class DronePursuit:MonoBehaviour
    {
        public Sandbox Game;public Transform Drone;float until;
        public static bool Active(Transform t){var p=t.GetComponent<DronePursuit>();return p!=null&&p.Drone!=null&&p.Game!=null&&p.Game.DronePanelOpen;}
        public static void Begin(Transform target,Sandbox game,Transform drone){var p=target.GetComponent<DronePursuit>()??target.gameObject.AddComponent<DronePursuit>();p.Game=game;p.Drone=drone;p.until=Time.time+30;}
        public void Step(float dt){if(Game==null||Drone==null||!Game.DronePanelOpen||Time.time>until){Destroy(this);return;}if(Game.Paused)return;Vector3 goal=Drone.position;goal.y=transform.position.y;NpcNavigation.Ensure(transform).MoveTo(goal,3.8f,dt);Vector3 d=goal-transform.position;d.y=0;if(d.sqrMagnitude>.1f)transform.rotation=Quaternion.LookRotation(d);}
        void Update(){Step(Time.deltaTime);}
    }
    public partial class Sandbox
    {
        public bool OpenDroneFromMonitor(){if(!SecretMonitorOpen||monitorHome==null||monitorHome.region!=World.Region)return false;var h=monitorHome;CloseSecretMonitor();return OpenDronePanel(h,true);}
    }
}
