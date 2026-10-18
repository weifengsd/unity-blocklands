using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    bool VehicleSpaceBlocked(Vector3 center,Vector3 half,Quaternion rotation){foreach(var c in Physics.OverlapBox(center,half,rotation,~(1<<2),QueryTriggerInteraction.Ignore))if(vehicleRoot==null||!c.transform.IsChildOf(vehicleRoot))return true;return false;}
    bool VehicleSweepBlocked(Vector3 center,Vector3 half,Vector3 direction,Quaternion rotation,float distance){foreach(var hit in Physics.BoxCastAll(center,half,direction,rotation,distance,~(1<<2),QueryTriggerInteraction.Ignore))if(vehicleRoot==null||!hit.collider.transform.IsChildOf(vehicleRoot))return true;return false;}
    bool VehicleGround(Vector3 position,out float height){height=0;float nearest=100;bool found=false;foreach(var hit in Physics.RaycastAll(position+Vector3.up*2,Vector3.down,5,~(1<<2),QueryTriggerInteraction.Ignore)){if(vehicleRoot!=null&&hit.collider.transform.IsChildOf(vehicleRoot))continue;if(hit.normal.y<.7f||hit.distance>=nearest)continue;nearest=hit.distance;height=hit.point.y;found=true;}return found;}
    void MakeVehicleCollision(){var shell=new GameObject("穿越飞车外壳");shell.transform.SetParent(vehicleRoot,false);shell.layer=2;var c=shell.AddComponent<BoxCollider>();c.center=new Vector3(0,1.5f,0);c.size=new Vector3(3.4f,2.6f,VehicleData.camper?10.4f:7.8f);c.enabled=false;}
}}
