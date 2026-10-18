using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public const float CarFlightCeiling=2000;
    bool carLanding;
    public bool CarFlying=>VehicleData!=null&&VehicleData.flying;
    public bool CarLanding=>carLanding;
    public void MatchCarView(){if(!CarRiding||vehicleRoot==null||carCare!="")return;float yaw=Player.transform.eulerAngles.y;vehicleRoot.rotation=Quaternion.Euler(0,yaw,0);VehicleData.yaw=yaw;}
    void RestoreAirborneControls(){carLanding=false;if(!VehicleGround(CarPosition,out float ground)||CarPosition.y>ground+1)VehicleData.flying=true;}
    public bool ToggleCarFlight()
    {
        if(!CarRiding||carCare!="")return false;carSpeed=0;
        if(CarFlying&&!carLanding){carLanding=true;Notice="房车正在下降；再按数字0取消降落。Shift也能下降。";}
        else{carLanding=false;if(!CarFlying)vehicleRoot.position+=Vector3.up*4;VehicleData.flying=true;Notice="数字0开启飞车；空格升高，Shift下降，抬头或低头后前进可斜飞。";}
        VehicleData.position=vehicleRoot.position;SeatDriver();Save();return true;
    }
    public void LiftCar(float direction,float dt)
    {
        if(!CarRiding||!CarFlying||Paused||carCare!="")return;
        Vector3 p=CarPosition+Vector3.up*Mathf.Clamp(direction,-1,1)*12*Mathf.Clamp(dt,0,.1f);p.y=Mathf.Clamp(p.y,1,CarFlightCeiling);
        if(direction<0&&VehicleGround(CarPosition,out float ground)&&p.y<=ground+.08f){p.y=ground+.08f;if(carLanding){VehicleData.flying=false;carLanding=false;Notice="房车已降落；数字0可再次飞起。";}}
        vehicleRoot.position=p;VehicleData.position=p;SeatDriver();
    }
    public void PhaseDrive(Vector2 input,float dt)
    {
        if(!CarRiding||Paused||carCare!=""||vehicleRoot==null)return;dt=Mathf.Clamp(dt,0,.1f);carSpeed=Mathf.MoveTowards(carSpeed,Mathf.Clamp(input.y,-1,1)*9,dt*8);float turn=Mathf.Clamp(input.x,-1,1)*45*dt;vehicleRoot.Rotate(0,turn,0);Player.transform.Rotate(0,turn,0);
        Vector3 direction=CarFlying?Player.View.transform.forward:vehicleRoot.forward;
        Vector3 p=CarPosition+direction*carSpeed*dt;p.x=Mathf.Clamp(p.x,3,VoxelWorld.Width-3);p.z=Mathf.Clamp(p.z,5,VoxelWorld.Depth-5);p.y=Mathf.Clamp(p.y,1,CarFlightCeiling);
        if(VehicleGround(CarPosition,out float ground)&&p.y<ground+.08f&&CarPosition.y>=ground-.1f)p.y=ground+.08f;
        if(!CarFlying&&VehicleGround(p,out ground)&&Mathf.Abs(ground+.08f-p.y)<.7f)p.y=ground+.08f;vehicleRoot.position=p;VehicleData.position=p;VehicleData.yaw=vehicleRoot.eulerAngles.y;SeatDriver();
    }
}}
