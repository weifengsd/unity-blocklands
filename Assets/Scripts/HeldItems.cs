using UnityEngine;
namespace Blocklands
{
    public static class HeldItemArt
    {
        static Color Paper=new Color(1,.97f,.86f),Blue=new Color(.22f,.76f,.98f);
        static void Text(RoyalArt art,string words,Vector3 at,float size,Color color){var root=new GameObject("物品上的文字").transform;root.SetParent(art.transform,false);root.localPosition=at;var t=root.gameObject.AddComponent<TextMesh>();t.font=Resources.Load<Font>("NotoSansCJKsc-Regular");t.GetComponent<Renderer>().sharedMaterial=t.font.material;t.text=words;t.fontSize=40;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=color;}
        public static void BuildCard(Transform root,InvitationDesign design)
        {
            var art=root.gameObject.AddComponent<RoyalArt>();art.Piece("手绘邀请函纸张",Vector3.zero,new Vector3(.49f,.34f,.012f),Paper);art.Piece("邀请函粉色页边",new Vector3(0,.146f,-.011f),new Vector3(.46f,.032f,.005f),new Color(.94f,.51f,.7f));
            if(design==null){Text(art,"邀请函",new Vector3(0,.08f,-.014f),.009f,new Color(.51f,.22f,.35f));return;}
            Text(art,design.title,new Vector3(0,.142f,-.014f),.004f,new Color(.44f,.17f,.28f));Text(art,"邀请 "+design.guests+" 位朋友",new Vector3(0,-.14f,-.014f),.005f,new Color(.37f,.27f,.18f));
            int segments=0;foreach(var stroke in design.drawing)segments+=Mathf.Max(0,stroke.points.Count-1);int stride=Mathf.Max(1,Mathf.CeilToInt(segments/220f));
            foreach(var stroke in design.drawing)for(int i=1;i<stroke.points.Count;i+=stride){Vector2 p=stroke.points[i-1],q=stroke.points[Mathf.Min(stroke.points.Count-1,i+stride-1)];Vector3 a=new Vector3((p.x-.5f)*.42f,(.5f-p.y)*.22f,-.014f),b=new Vector3((q.x-.5f)*.42f,(.5f-q.y)*.22f,-.014f);Vector3 d=b-a;var line=art.Piece("自己画的彩色笔画",(a+b)/2,new Vector3(.004f,Mathf.Max(.003f,d.magnitude),.003f),stroke.color);line.localRotation=Quaternion.FromToRotation(Vector3.up,d.normalized);}
        }
        public static void Build(Transform root,Block item,InvitationDesign design=null)
        {
            if(item==Blocks.InvitationCard){BuildCard(root,design);return;}var a=root.gameObject.AddComponent<RoyalArt>();Color color=Blocks.ColorOf(item),wood=new Color(.48f,.3f,.17f),metal=new Color(.74f,.8f,.86f);
            if(item==Block.Coin){a.Piece("长纸币",Vector3.zero,new Vector3(.48f,.2f,.009f),new Color(.78f,.91f,.77f));a.Piece("纸币装饰边",new Vector3(0,0,-.009f),new Vector3(.44f,.16f,.003f),new Color(.32f,.62f,.45f));a.Piece("纸币中心纹样",new Vector3(0,0,-.013f),new Vector3(.12f,.12f,.002f),new Color(.9f,.97f,.86f));Text(a,"1 元",new Vector3(0,0,-.017f),.009f,new Color(.13f,.34f,.23f));return;}
            if(item==Blocks.SurveillanceCamera){a.Piece("手里的监控摄像头",Vector3.zero,new Vector3(.32f,.2f,.28f),new Color(.88f,.92f,.94f));a.Piece("摄像头黑色镜头",new Vector3(0,0,-.17f),new Vector3(.14f,.14f,.15f),new Color(.06f,.14f,.21f),PrimitiveType.Sphere);a.Piece("镜头蓝色玻璃",new Vector3(0,0,-.25f),new Vector3(.1f,.1f,.015f),Blue);a.Piece("摄像头底座",new Vector3(0,-.14f,.03f),new Vector3(.17f,.08f,.14f),metal);return;}
            if(item==Blocks.CrystalScepter){a.Piece("水晶魔法杖柄",new Vector3(0,-.03f,0),new Vector3(.04f,.48f,.04f),Blue);a.Piece("权杖银色托环",new Vector3(0,.18f,0),new Vector3(.15f,.03f,.15f),metal,PrimitiveType.Cylinder);var gem=a.Piece("六面水晶宝石",new Vector3(0,.27f,0),new Vector3(.14f,.19f,.14f),new Color(.3f,.91f,1));gem.localRotation=Quaternion.Euler(0,35,35);for(int i=0;i<5;i++){float t=i*Mathf.PI*2/5;a.Piece("水晶星光装饰",new Vector3(Mathf.Cos(t)*.1f,.27f+Mathf.Sin(t)*.1f,0),Vector3.one*.045f,Color.white,PrimitiveType.Sphere);}return;}
            if(item==Block.Apple){a.Piece("手里的苹果",Vector3.zero,new Vector3(.24f,.23f,.23f),new Color(.91f,.22f,.25f),PrimitiveType.Sphere);a.Piece("苹果柄",new Vector3(0,.14f,0),new Vector3(.025f,.07f,.025f),wood);a.Piece("苹果叶",new Vector3(.05f,.145f,0),new Vector3(.11f,.025f,.045f),new Color(.32f,.6f,.22f));return;}
            if(item==Block.Bread){var loaf=a.Piece("面包",Vector3.zero,new Vector3(.31f,.16f,.18f),new Color(.82f,.54f,.25f),PrimitiveType.Sphere);for(int i=-1;i<=1;i++)a.Piece("面包划纹",new Vector3(i*.075f,.075f,-.03f),new Vector3(.016f,.009f,.13f),new Color(.98f,.82f,.49f));return;}
            if(item==Block.Beef||item==Block.Mutton||item==Block.Pork){a.Piece("手里的肉",Vector3.zero,new Vector3(.29f,.2f,.12f),new Color(.77f,.32f,.32f),PrimitiveType.Sphere);a.Piece("肉的脂肪边",new Vector3(.06f,.02f,-.055f),new Vector3(.15f,.1f,.025f),new Color(.96f,.83f,.66f),PrimitiveType.Sphere);return;}
            if(item==Block.Cookie){var cookie=a.Piece("饼干",Vector3.zero,new Vector3(.23f,.025f,.23f),new Color(.77f,.52f,.29f),PrimitiveType.Cylinder);cookie.localRotation=Quaternion.Euler(90,0,0);for(int i=0;i<5;i++){float t=i*2.4f;a.Piece("饼干巧克力豆",new Vector3(Mathf.Cos(t)*.07f,Mathf.Sin(t)*.07f,-.03f),Vector3.one*.028f,wood);}return;}
            bool sword=item==Blocks.NetherSword||item>=Block.WoodSword&&item<=Block.IronSword;
            bool tool=Blocks.Tool(item)||sword;
            if(tool||item==Block.Stick||item==Blocks.CrystalRod){a.Piece("物品手柄",new Vector3(0,-.04f,0),new Vector3(.035f,.38f,.04f),item==Blocks.CrystalRod?Blue:wood);if(sword){a.Piece("剑刃",new Vector3(0,.27f,0),new Vector3(.06f,.38f,.025f),color);a.Piece("剑的护手",new Vector3(0,.065f,0),new Vector3(.17f,.035f,.04f),metal);}else if(tool){bool shovel=item>=Block.WoodShovel&&item<=Block.IronShovel;bool hammer=item==Block.CrystalHammer;a.Piece(hammer?"锤头":shovel?"铲头":"工具刃",new Vector3(0,.18f,0),shovel?new Vector3(.13f,.17f,.025f):hammer?new Vector3(.25f,.15f,.12f):new Vector3(.29f,.055f,.045f),color);}return;}
            if(item==Block.Table||item==Block.Chair||item==Block.Bed||item==Block.Sofa){a.Piece("家具坐面",Vector3.zero,new Vector3(.3f,.045f,.23f),color);foreach(float x in new[]{-.12f,.12f})foreach(float z in new[]{-.08f,.08f})a.Piece("家具支脚",new Vector3(x,-.1f,z),new Vector3(.03f,.17f,.03f),wood);if(item==Block.Chair||item==Block.Sofa)a.Piece("家具靠背",new Vector3(0,.1f,.1f),new Vector3(.3f,.22f,.035f),color);if(item==Block.Bed)a.Piece("小枕头",new Vector3(0,.055f,.065f),new Vector3(.2f,.07f,.085f),Color.white);return;}
            if(item>=Blocks.CrystalHelmet&&item<=Blocks.NetherBoots&&item!=Blocks.CrystalScepter){a.Piece("拿着的装备",Vector3.zero,new Vector3(.23f,.27f,.1f),color);a.Piece("装备上的亮晶",new Vector3(0,.04f,-.065f),Vector3.one*.07f,Color.white);return;}
            var block=a.Piece("手中"+Blocks.Names[(int)item],Vector3.zero,new Vector3(.25f,.25f,.25f),color);block.localRotation=Quaternion.Euler(-10,25,0);a.Piece("方块上沿",new Vector3(0,.13f,0),new Vector3(.25f,.015f,.25f),Color.Lerp(color,Color.white,.3f));
        }
    }
    public partial class FirstPerson
    {
        Transform heldItem;int heldId=-1,heldDesign=-1;public int VisibleHeldItemId=>heldItem!=null&&heldItem.gameObject.activeSelf?heldId:-1;
        public bool HeldItemVisible=>heldItem!=null&&heldItem.gameObject.activeSelf;
        void LateUpdate(){RefreshHeldItem();AnimateAlloySwing();}
        public void RefreshHeldItem()
        {
            if(Game==null||View==null)return;int id=Game.Selected,design=id==93?(Game.CurrentHeldInvitation?.id??-1):-1;
            bool visible=!Game.EmptyHands&&!Game.PartyRideActive&&Game.PartyHeldName==null&&Game.Inventory[id]>0&&!Game.FlowerWandActive&&!Game.InspectingHero&&!Game.HomeSeated&&!Game.HoldingBook&&!Game.CoasterRiding&&!Game.PalacePlayLocked&&!Game.PhoneOpen&&!Game.VoidRideActive;
            if(!visible){if(heldItem!=null)heldItem.gameObject.SetActive(false);if(Game.EmptyHands){hand.gameObject.SetActive(!Game.CarRiding);pickaxe.gameObject.SetActive(false);}return;}
            if(heldItem==null||heldId!=id||heldDesign!=design){if(heldItem!=null){heldItem.gameObject.SetActive(false);Destroy(heldItem.gameObject);}heldItem=new GameObject("真实手持物品："+Blocks.Names[id]).transform;heldItem.SetParent(View.transform,false);heldItem.localPosition=new Vector3(.35f,-.25f,.64f);heldItem.localRotation=Quaternion.Euler(0,0,-8);HeldItemArt.Build(heldItem,(Block)id,Game.CurrentHeldInvitation);if(id==93||id==58)foreach(var renderer in heldItem.GetComponentsInChildren<Renderer>())if(renderer.GetComponent<TextMesh>()==null)renderer.sharedMaterial.shader=Resources.Load<Shader>("Selection");var skin=heldItem.gameObject.AddComponent<RoyalArt>();skin.Piece("托住物品的手",new Vector3(0,-.17f,.1f),new Vector3(.14f,.13f,.18f),new Color(.94f,.69f,.53f),PrimitiveType.Sphere);foreach(var t in heldItem.GetComponentsInChildren<Transform>())t.gameObject.layer=2;heldId=id;heldDesign=design;}
            heldItem.gameObject.SetActive(true);hand.gameObject.SetActive(false);pickaxe.gameObject.SetActive(false);
        }
    }
    public partial class Sandbox
    {
        float nextCrystalMagic;public int CrystalMagicShots {get;private set;}
        public bool FireCrystalMagic(){if(EmptyHands||Selected!=85||Inventory[85]<1||Paused||Health<=0||Time.time<nextCrystalMagic)return false;nextCrystalMagic=Time.time+.65f;var orb=GameObject.CreatePrimitive(PrimitiveType.Sphere);orb.name="水晶权杖的蓝色光弹";orb.transform.position=Player.View.transform.position+Player.View.transform.forward*.7f;orb.transform.localScale=Vector3.one*.2f;orb.GetComponent<Collider>().enabled=false;Destroy(orb.GetComponent<Collider>());orb.AddComponent<CrystalMagicBolt>().Begin(this,Player.View.transform.forward);CrystalMagicShots++;return true;}
    }
    public class CrystalMagicBolt:MonoBehaviour
    {
        Sandbox game;Vector3 velocity;float age;int region;Material material;
        public void Begin(Sandbox g,Vector3 direction){game=g;region=g.World.Region;velocity=direction*19;material=new Material(Resources.Load<Shader>("Selection"));material.color=new Color(.21f,.83f,1);GetComponent<Renderer>().sharedMaterial=material;}
        void Update(){if(game==null||region!=game.World.Region){Destroy(gameObject);return;}if(game.Paused)return;age+=Time.deltaTime;if(age>3){Destroy(gameObject);return;}float distance=velocity.magnitude*Time.deltaTime;if(Physics.Raycast(transform.position,velocity.normalized,out var hit,distance+.12f,~(1<<2),QueryTriggerInteraction.Ignore)){var actor=hit.collider.GetComponentInParent<CombatActor>();game.CrystalHitTarget(hit.collider);Destroy(gameObject);return;}transform.position+=velocity*Time.deltaTime;}
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
