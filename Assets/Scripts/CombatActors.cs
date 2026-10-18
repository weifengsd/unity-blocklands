using UnityEngine;

namespace Blocklands
{
    public abstract class CombatActor:MonoBehaviour
    {
        public abstract string Caption {get;}
        public abstract void Hit(float damage);
        protected Material MakeMaterial(Color c){var m=new Material(Resources.Load<Shader>("Selection"));m.color=c;return m;}
        protected Transform Part(string name,Transform parent,Vector3 p,Vector3 scale,Material m)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;Destroy(g.GetComponent<Collider>());return g.transform;
        }
    }
    public class FarmAnimal:CombatActor
    {
        Sandbox game;int id;float hp=3;Vector3 origin;Material coat,feet,face;
        public int Household=>id/6;public bool IsPig=>id%6<3;
        public override string Caption=>(IsPig?"猪":"羊")+" · 第"+(Household+1)+"户  "+hp+" / 3";
        public void Initialize(Sandbox g,int number)
        {
            game=g;id=number;origin=transform.position;
            coat=MakeMaterial(IsPig?new Color(.94f,.6f,.61f):new Color(.91f,.88f,.75f));feet=MakeMaterial(new Color(.22f,.17f,.15f));face=MakeMaterial(IsPig?new Color(.89f,.48f,.52f):new Color(.69f,.55f,.43f));
            Part("body",transform,new Vector3(0,1,0),new Vector3(1.3f,.9f,1.9f),coat);
            Part("head",transform,new Vector3(0,1.25f,1.1f),new Vector3(.8f,.75f,.75f),face);
            foreach(float x in new[]{-.47f,.47f})foreach(float z in new[]{-.65f,.65f})Part("leg",transform,new Vector3(x,.35f,z),new Vector3(.25f,.7f,.25f),feet);
            foreach(float x in new[]{-.27f,.27f}){Part("eye",transform,new Vector3(x,1.38f,1.485f),new Vector3(.09f,.1f,.02f),feet);Part("ear",transform,new Vector3(x*1.9f,1.52f,1.15f),new Vector3(.28f,.14f,.3f),coat);}
            Part("nose",transform,new Vector3(0,1.08f,1.52f),new Vector3(.45f,.22f,.16f),coat);
            if(IsPig)foreach(float x in new[]{-.12f,.12f})Part("nostril",transform,new Vector3(x,1.1f,1.605f),new Vector3(.08f,.09f,.025f),feet);
            var c=gameObject.AddComponent<BoxCollider>();c.center=new Vector3(0,.9f,.25f);c.size=new Vector3(1.5f,1.8f,2.6f);
        }
        void Update(){if(game.Paused||hp<=0)return;float t=Time.time*.27f+id;transform.position=origin+new Vector3(Mathf.Sin(t)*.7f,0,Mathf.Cos(t)*.7f);transform.rotation=Quaternion.Euler(0,-t*Mathf.Rad2Deg,0);}
        public override void Hit(float damage)
        {
            if(game.Paused||hp<=0||damage<=0)return;hp=Mathf.Max(0,hp-damage);
            if(hp==0){game.DeadAnimals.Add(id);int item=(int)(IsPig?Block.Pork:Block.Mutton);game.Inventory[item]=Mathf.Min(9999,game.Inventory[item]+2);game.Save();gameObject.SetActive(false);}
        }
        void OnDestroy(){Destroy(coat);Destroy(feet);Destroy(face);}
    }
    public class DreamDragon:CombatActor
    {
        Sandbox game;Transform wingL,wingR;float age,nextFire;
        public override string Caption=>"梦影龙  "+game.DragonHealth+" / 100";
        public void Initialize(Sandbox g)
        {
            game=g;var sculpt=gameObject.AddComponent<DragonSculpt>();sculpt.Build();wingL=sculpt.LeftWing;wingR=sculpt.RightWing;
            var c=gameObject.AddComponent<BoxCollider>();c.center=new Vector3(0,2,.5f);c.size=new Vector3(3.2f,6.5f,8.7f);nextFire=Time.time+7;
        }
        void Update()
        {
            if(game.Paused)return;
            if(game.DragonHealth<=0){gameObject.SetActive(false);return;}
            age+=Time.deltaTime;
            Vector3 p=game.Player.transform.position;
            Vector3 target=new Vector3(Mathf.Clamp(p.x+Mathf.Sin(age*.65f)*5,6,58),Mathf.Clamp(p.y+3,22,65),Mathf.Clamp(p.z+Mathf.Cos(age*.65f)*5,6,58));
            if(age<5)target=Vector3.Lerp(new Vector3(30,42,38),target,age/5);
            transform.position=Vector3.MoveTowards(transform.position,target,Time.deltaTime*(age<5?8:3.2f));
            Vector3 look=p+Vector3.up-transform.position;look.y=0;if(look.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look),Time.deltaTime*2.5f);
            wingL.localRotation=Quaternion.Euler(0,0,Mathf.Sin(age*4)*23);wingR.localRotation=Quaternion.Euler(0,0,-Mathf.Sin(age*4)*23);
            if(Time.time>=nextFire){nextFire=Time.time+3;var orb=GameObject.CreatePrimitive(PrimitiveType.Sphere);orb.name="Dream breath";orb.transform.SetParent(transform.parent,true);orb.transform.position=transform.position+transform.forward*5.4f+Vector3.up;orb.transform.localScale=Vector3.one*.6f;Destroy(orb.GetComponent<Collider>());orb.AddComponent<DragonBreath>().Initialize(game,(game.Player.View.transform.position-orb.transform.position).normalized);}
        }
        public override void Hit(float damage){if(game.Paused||game.DragonHealth<=0)return;game.HitDragon(damage);}
    }
    public class DragonBreath:MonoBehaviour
    {
        Sandbox game;Vector3 velocity;float age;Material material;
        public void Initialize(Sandbox g,Vector3 direction){game=g;velocity=direction*9;material=new Material(Resources.Load<Shader>("Selection"));material.color=new Color(.75f,.32f,1);GetComponent<Renderer>().sharedMaterial=material;}
        void Update()
        {
            if(game==null||game.Paused)return;age+=Time.deltaTime;
            if(game.World.Region!=3||game.DragonHealth<=0||age>10){Destroy(gameObject);return;}
            Vector3 step=velocity*Time.deltaTime;
            if(Physics.Raycast(transform.position,velocity.normalized,out var hit,step.magnitude+.18f,~(1<<2))){if(hit.collider==game.Player.Body)game.Damage(1,"梦影龙的能量");Destroy(gameObject);return;}
            transform.position+=step;
        }
        void OnDestroy(){Destroy(material);}
    }
}
