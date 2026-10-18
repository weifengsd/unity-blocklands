using UnityEngine;
namespace Blocklands
{
    public class CavernSpirit:CombatActor
    {
        Sandbox game;bool pig;float hp=12,angryUntil;Vector3 origin;Material skin,cloth,dark,gold;CharacterController body;
        public override string Caption=>(pig?"猪灵":"羊灵（幻方世界原创）")+" "+hp+" / 12";
        public bool Friendly,Dancing;
        public void Initialize(Sandbox g,bool isPig)
        {
            game=g;pig=isPig;origin=transform.position;
            skin=MakeMaterial(pig?new Color(.76f,.44f,.39f):new Color(.92f,.87f,.75f));cloth=MakeMaterial(pig?new Color(.36f,.14f,.09f):new Color(.53f,.44f,.68f));dark=MakeMaterial(new Color(.14f,.07f,.09f));gold=MakeMaterial(new Color(1,.65f,.22f));
            Part("torso",transform,new Vector3(0,1,0),new Vector3(.8f,1,.45f),cloth);Part("head",transform,new Vector3(0,1.85f,0),new Vector3(.85f,.8f,.7f),skin);
            foreach(float x in new[]{-.24f,.24f}){
                Part("eye",transform,new Vector3(x,1.98f,.36f),new Vector3(.1f,.1f,.04f),dark);
                Part("leg",transform,new Vector3(x,.32f,0),new Vector3(.25f,.65f,.32f),dark);
                Part("arm",transform,new Vector3(x*2.3f,1,0),new Vector3(.22f,.9f,.3f),skin);
                if(pig){Part("ear",transform,new Vector3(x*2,2.12f,0),new Vector3(.35f,.25f,.22f),skin);Part("tusk",transform,new Vector3(x,1.65f,.55f),new Vector3(.12f,.35f,.15f),gold);}
                else for(int i=0;i<4;i++)Part("curled horn",transform,new Vector3(x*2.1f,2.12f+Mathf.Sin(i*.9f)*.25f,-.1f+i*.13f),new Vector3(.24f,.24f,.25f),gold);
            }
            Part("muzzle",transform,new Vector3(0,1.75f,.45f),new Vector3(.44f,.3f,.3f),skin);
            if(pig)Part("golden axe",transform,new Vector3(.65f,.8f,.45f),new Vector3(.2f,.75f,.65f),gold);
            body=gameObject.AddComponent<CharacterController>();body.height=2.3f;body.center=Vector3.up*1.15f;body.radius=.4f;body.stepOffset=.4f;
        }
        void Update()
        {
            if(game.Paused||hp<=0)return;
            if(Dancing){body.SimpleMove(Vector3.zero);return;}
            Vector3 p=game.Player.transform.position;bool chase=!Friendly&&(Time.time<angryUntil||(pig&&Vector3.Distance(transform.position,p)<5));
            Vector3 target=chase?p:origin+new Vector3(Mathf.Sin(Time.time*.3f+origin.x)*2,0,Mathf.Cos(Time.time*.3f+origin.z)*1.2f);
            Vector3 d=target-transform.position;d.y=0;Vector3 next=transform.position+d.normalized*.65f;
            Block floor=game.World.Get(Vector3Int.FloorToInt(next-Vector3.up*.2f));
            body.SimpleMove(d.magnitude>.3f&&floor!=Block.Lava&&floor!=Block.Air?d.normalized*(chase?2.3f:.8f):Vector3.zero);
            if(d.sqrMagnitude>.1f)transform.rotation=Quaternion.LookRotation(d);
            if(chase&&Vector3.Distance(transform.position,p)<1.6f&&Physics.Linecast(transform.position+Vector3.up,p+Vector3.up,out var hit)&&hit.collider==game.Player.Body)game.Damage(.5f,pig?"被猪灵追上":"羊灵反击");
        }
        public override void Hit(float damage){if(game.Paused||hp<=0)return;hp=Mathf.Max(0,hp-damage);angryUntil=Time.time+15;if(hp==0){game.Inventory[(int)Block.DarkStone]=Mathf.Min(9999,game.Inventory[(int)Block.DarkStone]+2);gameObject.SetActive(false);}}
        void OnDestroy(){Destroy(skin);Destroy(cloth);Destroy(dark);Destroy(gold);}
    }
}
