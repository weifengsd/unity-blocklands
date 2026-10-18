using UnityEngine;
namespace Blocklands
{
    public class VillageTower:MonoBehaviour
    {
        Sandbox game;float buildAfter;public bool Active {get;private set;} public Vector3 Anchor {get;private set;}
        public Villager[] Members {get;private set;}
        public void Initialize(Sandbox g){game=g;Members=GetComponentsInChildren<Villager>();}
        public void BreakFormation(){Active=false;buildAfter=Time.time+10;}
        void Update()
        {
            if(game.Paused)return;Vector3 p=game.Player.transform.position;
            if(Active){if(Time.time>=game.VillageAngryUntil||p.y<18||Vector2.Distance(new Vector2(p.x,p.z),new Vector2(Anchor.x,Anchor.z))>4){Active=false;buildAfter=Time.time+8;}return;}
            if(Time.time<buildAfter)return;
            foreach(var v in Members)if(v.OnTower)return;
            if(Time.time>=game.VillageAngryUntil||p.y<18)return;
            var cell=Vector3Int.FloorToInt(p);cell.y=14;
            if(!Blocks.Solid(game.World.Get(cell)))return;
            for(int y=15;y<29;y++)for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)if(Blocks.Solid(game.World.Get(new Vector3Int(cell.x+x,y,cell.z+z))))return;
            Anchor=new Vector3(cell.x+.5f,15,cell.z+.5f);Active=true;game.Notice="村民正在搭人梯！六个人最多叠到约13米，快移动离开。";
        }
    }
}
