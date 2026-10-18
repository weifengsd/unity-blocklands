using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    Transform tsunamiRoot,tsunamiFlood;readonly System.Collections.Generic.List<Transform> tsunamiWaves=new System.Collections.Generic.List<Transform>();Material tsunamiWater;float tsunamiAge,tsunamiBase,tsunamiHitAt,tsunamiLevel;bool tsunamiRetreat;
    public const float TsunamiWaveSeconds=30,TsunamiWarningSeconds=8,TsunamiRetreatSeconds=10;
    public bool TsunamiActive=>tsunamiRoot!=null;public float TsunamiAge=>tsunamiAge;public int TsunamiFrontCount=>TsunamiActive?tsunamiWaves.Count:0;
    public float TsunamiPeakLevel=>SkyHouse.FloorHeight(SkyHouse.RoofFloor)+2.5f;
    public float TsunamiWaterLevel=>TsunamiActive?tsunamiLevel:0;
    public string TsunamiCaption=>tsunamiAge<8?"8秒准备阶段":tsunamiRetreat?"退水中，医护准备救援":"五轮大浪推进中 · 北向南 · 共30秒";
    public float TsunamiFront(int index){float t=tsunamiAge-8-index*4.5f;return t>=0&&t<=12?Mathf.Lerp(-12,108,t/12):-100;}
    public bool StartTsunami()
    {
        if(TsunamiActive){StopTsunami();Notice="海啸已停止；医护开始救助倒地村民。";return true;}
        if(InVoidRealms||World.Region==3||World.Region==4||World.Region==16||CompanionReturnBlocked){Notice="请先回到地面生活地区、结束游乐项目，再启动海啸演练。";return false;}
        tsunamiBase=World.Region==1?15:VoxelWorld.GroundHeight(48,48)+1;tsunamiLevel=tsunamiBase;tsunamiAge=0;tsunamiHitAt=0;tsunamiRetreat=false;VillageAngryUntil=0;
        foreach(var tower in FindObjectsByType<VillageTower>())tower.BreakFormation();
        tsunamiRoot=new GameObject("五轮高海啸 · 顶层高度 · 不毁房屋").transform;tsunamiRoot.SetParent(villagePeople.transform,false);var art=tsunamiRoot.gameObject.AddComponent<RoyalArt>();tsunamiWater=new Material(Resources.Load<Shader>("TsunamiWater"));tsunamiWater.color=new Color(.12f,.56f,.72f,.58f);
        for(int wave=0;wave<5;wave++){var root=new GameObject("第"+(wave+1)+"轮巨浪").transform;root.SetParent(tsunamiRoot,false);var wa=root.gameObject.AddComponent<RoyalArt>();var face=wa.Piece("高浪水体",Vector3.up*.5f,new Vector3(96,1,4),new Color(.15f,.62f,.8f));face.GetComponent<Renderer>().sharedMaterial=tsunamiWater;for(int i=0;i<24;i++)wa.Piece("卷起的浪花",new Vector3(-46+i*4,1,1),new Vector3(4.7f,1.1f,3),new Color(.87f,.97f,1),PrimitiveType.Sphere);root.gameObject.SetActive(false);tsunamiWaves.Add(root);}
        tsunamiFlood=art.Piece("逐渐淹没村庄的水面",new Vector3(48,tsunamiBase,0),new Vector3(96,.13f,1),new Color(.25f,.67f,.8f));tsunamiFlood.GetComponent<Renderer>().sharedMaterial=tsunamiWater;tsunamiFlood.gameObject.SetActive(false);
        Notice="8秒后五轮大浪来袭，水会高到村庄顶层！进密室关门，再用H看监控；`键可停止。";return true;
    }
    public void StopTsunami(){if(tsunamiRoot!=null){tsunamiRoot.gameObject.SetActive(false);Destroy(tsunamiRoot.gameObject);}if(tsunamiWater!=null)Destroy(tsunamiWater);tsunamiWater=null;tsunamiRoot=tsunamiFlood=null;tsunamiWaves.Clear();tsunamiAge=0;tsunamiLevel=0;tsunamiRetreat=false;}
    public void TickTsunami(float dt)
    {
        if(!TsunamiActive||Paused)return;tsunamiAge+=Mathf.Max(0,dt);if(tsunamiAge<8)return;float t=tsunamiAge-8;
        if(t>=TsunamiWaveSeconds+TsunamiRetreatSeconds){bool safe=SecretRoomSheltered();StopTsunami();Notice=safe?"大浪已退，密室保护了你。医护正在救助村民；重进村庄也能恢复居民。":"大浪已退，医护正在救助村民；重进村庄可恢复居民。";return;}
        tsunamiRetreat=t>=30;float rise=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/10));float depth=(TsunamiPeakLevel-tsunamiBase)*(tsunamiRetreat?Mathf.Clamp01((40-t)/10):rise);tsunamiLevel=tsunamiBase+depth;float leading=0;
        for(int i=0;i<tsunamiWaves.Count;i++){float z=TsunamiFront(i);bool active=z> -99&&!tsunamiRetreat;tsunamiWaves[i].gameObject.SetActive(active);if(!active)continue;leading=Mathf.Max(leading,z);float height=Mathf.Max(.4f,depth+1.2f+Mathf.Sin(t*2+i)*.7f);var root=tsunamiWaves[i];root.position=new Vector3(48,tsunamiBase,z);var face=root.Find("高浪水体");face.localPosition=Vector3.up*(height/2);face.localScale=new Vector3(96,height,4);int n=0;foreach(Transform part in root)if(part.name=="卷起的浪花"){part.localPosition=new Vector3(-46+n*4,height+Mathf.Sin(t*3+n*.5f)*.22f,1);n++;}}
        float covered=tsunamiRetreat||t>12?96:Mathf.Clamp(leading+3,0,96);tsunamiFlood.gameObject.SetActive(covered>0);tsunamiFlood.position=new Vector3(48,tsunamiLevel,covered/2);tsunamiFlood.localScale=new Vector3(96,.13f,Mathf.Max(.1f,covered));
        if(!tsunamiRetreat)ApplyFloodImpact(Mathf.Min(96,covered),tsunamiLevel);
        var p=Player.transform.position;bool inside=p.x>=0&&p.x<96&&p.z>=0&&p.z<covered;bool below=p.y<tsunamiLevel&&p.y>tsunamiBase-18;
        if(inside&&below&&!SecretRoomSheltered()&&tsunamiAge>=tsunamiHitAt){tsunamiHitAt=tsunamiAge+4;Damage(.5f,"海啸水冲到了你；进入密室并关门");}
    }
    void DrawSecretStatus(float w,float h)
    {
        if(SecretSwingActive)Label(new Rect(w/2-280,h-170,560,35),"密室秋千 · "+Mathf.CeilToInt(Mathf.Max(0,20-secretSwingAge))+"秒 · G下来",18,new Color(1,.88f,.7f),TextAnchor.MiddleCenter);
        if(TsunamiActive)Label(new Rect(w/2-360,155,720,55),tsunamiAge<8?"大浪倒计时 "+Mathf.CeilToInt(8-tsunamiAge)+"秒 · 顶层也会淹 · `停止":TsunamiCaption+" · 水面"+tsunamiLevel.ToString("0.0")+"米\n"+(SecretRoomSheltered()?"密室门已关，安全 · H可看监控":"请进密室关门 · 倒地村民"+FloodDownCount+"人"),18,new Color(.7f,.94f,1),TextAnchor.MiddleCenter);
        else if(FloodDownCount>0)Label(new Rect(w/2-330,155,660,32),"医护正在救助 · 等待"+FloodDownCount+"人 · 已恢复"+FloodRescuedCount+"人 · 重进村庄可全部刷新",16,new Color(.8f,.96f,.88f),TextAnchor.MiddleCenter);
        if(VillageRefunds[2]+VillageRefunds[3]>0)Label(new Rect(w-340,h-155,318,50),"回收袋：石头"+VillageRefunds[3]+" / 泥土"+VillageRefunds[2]+"\n背包满了；腾出位置后自动收回。",14,new Color(1,.9f,.7f));
    }
}}
