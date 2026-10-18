using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public partial class SkyHouse
    {
        readonly HashSet<int> requests=new HashSet<int>(),npcRequests=new HashSet<int>();readonly List<VillageControl> floorButtons=new List<VillageControl>();
        public readonly List<int> StopHistory=new List<int>();int direction,npcLanding=-2,npcPending=-2;float dwellUntil,npcReturnAt;
        Rider pressingRider;float pressingUntil;
        public bool Requested(int f)=>requests.Contains(f)||npcPending==f;
        // Each visit chooses its destination independently of the player's buttons.
        public bool PlanResidentVisit(int destination){if(destination<0||destination>RoofFloor)return false;shopFloor=destination;return true;}
        Vector3 VisitOrigin(int f)=>f==RoofFloor?new Vector3(column.x+side*3.5f,FloorHeight(f),column.z-1.2f):StoreOrigin(f);
        public int ResidentsVisiting(int f){int n=0;foreach(var r in riders)if(npcLanding==f&&r.OnLanding&&!r.Boarded&&!r.Resting&&Mathf.Abs(r.Root.position.y-FloorHeight(f))<.8f)n++;return n;}
        bool Aligned=>Mathf.Abs(CarY-FloorHeight(floor))<.02f;
        void ClearStops(){requests.Clear();npcRequests.Clear();direction=0;npcPending=-2;}
        void QueueStop(int f,bool npc){if(f< -1||f>RoofFloor)return;requests.Add(f);if(npc)npcRequests.Add(f);UpdateFloorLamps();}
        void UpdateFloorLamps(){foreach(var c in floorButtons)if(c!=null)c.Face.sharedMaterial.color=Requested(c.Floor)?new Color(1,.82f,.1f):new Color(.55f,.62f,.69f);}
        public bool RequestFloor(int destination){if(destination< -1||destination>RoofFloor||Time.time<game.VillageAngryUntil){game.Notice="先归还物品，和村民和好后再使用公共电梯。";return false;}QueueStop(destination,false);game.Notice="已选"+FloorLabel(destination)+"，只在有人选择的楼层停靠。";return true;}
        public bool RequestNpcFloor(int destination){Rider passenger=null;foreach(var r in riders)if(r.Boarded&&!r.Resting)passenger=r;if(passenger==null||destination< -1||destination>RoofFloor)return false;if(npcRequests.Contains(destination))return true;pressingRider=passenger;pressingUntil=Time.time+.55f;stopUntil=Mathf.Max(stopUntil,pressingUntil+.1f);if(PlayerInside||Vector3.Distance(game.Player.transform.position,CarCenter)<8)game.Notice=(passenger.Info.Child?"小孩":"老人")+"按下了"+FloorLabel(destination)+"按钮。";QueueStop(destination,true);return true;}
        void AnimateNpcSelection(){if(pressingRider!=null&&Time.time<pressingUntil&&pressingRider.Boarded&&!pressingRider.Resting){pressingRider.Root.position=car.position+new Vector3(-side*.8f,.18f,0);pressingRider.Root.rotation=Quaternion.LookRotation(new Vector3(-side,0,0));pressingRider.ArmL.localRotation=Quaternion.Euler(70,0,0);}}
        int NextStop(){int best=-99;float d=float.MaxValue;foreach(int f in requests){float delta=FloorHeight(f)-CarY;if(direction>0&&delta<-.03f||direction<0&&delta>.03f)continue;if(Mathf.Abs(delta)<d){d=Mathf.Abs(delta);best=f;}}if(best==-99){direction=-direction;foreach(int f in requests){float distance=Mathf.Abs(FloorHeight(f)-CarY);if(distance<d){d=distance;best=f;}}}return best;}
        void RecordStop(int target){floor=target;StopHistory.Add(target);stopUntil=dwellUntil=Time.time+1.6f;}
        void TravelCar(float target){bool passenger=PlayerInside;Vector3 previous=car.position;car.position=Vector3.MoveTowards(previous,new Vector3(column.x,target,column.z),Time.deltaTime*8);if(passenger)game.Player.Carry(car.position-previous);foreach(var r in riders)if(r.Boarded&&FloodResident.IsDown(r.Root))r.Root.position+=car.position-previous;PositionInCar();}
        void MoveNpcLift(int target){npcPending=target;UpdateFloorLamps();if(Time.time<stopUntil&&Aligned){SetDoors(floor!=RoofFloor);PositionInCar();return;}SetDoors(false);if(doorAmount>.03f)return;TravelCar(FloorHeight(target));if(Mathf.Abs(CarY-FloorHeight(target))<.01f){RecordStop(target);npcPending=-2;SetDoors(target!=RoofFloor);UpdateFloorLamps();}}
        void TickStops()
        {
            if(Time.time<Mathf.Max(dwellUntil,stopUntil)){SetDoors(Aligned);return;}
            if(requests.Count==0){direction=0;SetDoors(Aligned&&floor!=RoofFloor);return;}
            int next=NextStop();if(next==-99)return;
            if(Mathf.Abs(CarY-FloorHeight(next))<.01f){requests.Remove(next);RecordStop(next);SetDoors(true);
                if(npcRequests.Remove(next)){npcLanding=next;npcReturnAt=Time.time+8;stopUntil=dwellUntil=Time.time+3;foreach(var r in riders)if(r.Boarded&&!r.Resting){r.OnLanding=true;r.Boarded=false;r.Body.enabled=false;r.Route=-1;}if(next==-1){npcLanding=-2;shoppingAt=Time.time+35;shopFloor=(shopFloor+1)%(RoofFloor+1);foreach(var r in riders)if(r.OnLanding&&!r.Resting){r.OnLanding=false;r.Root.position=home+new Vector3(4.5f,-1,-1);r.Body.enabled=true;r.Route=0;} } }
                Phase="在已选的"+FloorLabel(next)+"开门上下客";UpdateFloorLamps();return;
            }
            if(direction==0)direction=FloorHeight(next)>CarY?1:-1;SetDoors(false);if(doorAmount>.03f)return;TravelCar(FloorHeight(next));Phase="驶向已选的"+FloorLabel(next)+"，未选楼层不停";
        }
        void CivilUpdate()
        {
            aimLine.enabled=handLink.enabled=false;phaseTime=0;
            if(npcLanding>=0){
                int i=0;bool boarded=true;foreach(var r in riders){if(r.Resting||!r.OnLanding){i++;continue;}if(r.Boarded){i++;continue;}boarded=false;
                    Vector3 target=VisitOrigin(npcLanding)+new Vector3((i%2-.5f)*1.1f,.2f,1.4f+i/2*.8f);
                    if(r.Route==-1){if(floor==npcLanding&&Aligned&&!DoorOpen){i++;continue;}target=new Vector3(column.x+side*3.5f,FloorHeight(npcLanding)+.2f,column.z);if(Vector3.Distance(r.Root.position,target)<.15f)r.Route=0;}
                    if(Time.time>=npcReturnAt&&!PlayerInside&&floor==npcLanding&&Aligned&&DoorOpen&&requests.Count==0){target=r.Route==0?new Vector3(column.x+side*3.5f,FloorHeight(npcLanding)+.2f,column.z):car.position+new Vector3((i%2-.5f)*1.2f,.18f,(i/2-.5f)*1.2f);stopUntil=Time.time+.8f;if(Vector3.Distance(r.Root.position,target)<.15f){if(r.Route==0)r.Route=1;else r.Boarded=true;}}
                    r.Root.position=Vector3.MoveTowards(r.Root.position,target,Time.deltaTime*3);i++;
                }
                if(Time.time>=npcReturnAt&&!PlayerInside&&(floor!=npcLanding||!DoorOpen))QueueStop(npcLanding,false);
                if(boarded){foreach(var r in riders)r.OnLanding=false;npcLanding=-2;RequestNpcFloor(-1);}
            }
            else if(requests.Count==0&&!PlayerInside&&Time.time>shoppingAt){
                if(BoardedCount<4){if(!Aligned||floor!=-1){QueueStop(-1,false);}else if(DoorOpen){foreach(var r in riders)if(!r.Boarded&&!r.Resting){Vector3 t=RiderWaypoint(r.Route),d=t-r.Root.position;d.y=0;if(d.magnitude<(r.Route==3?2f:.4f)){if(r.Route<3)r.Route++;else{r.Boarded=true;r.Body.enabled=false;r.EntryUntil=Time.time+.6f;stopUntil=Time.time+.7f;}}else r.Body.SimpleMove(d.normalized*3.4f);}PositionInCar();}}
                if(BoardedCount==4){RequestNpcFloor(shopFloor);shoppingAt=Time.time+60;}
            }
            TickStops();
            AnimateNpcSelection();
        }
    }
}
