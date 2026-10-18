using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public partial class Sandbox
    {
        decimal preciseHealth=10m;public decimal PreciseHealth=>preciseHealth;
        public float Health {get=>(float)preciseHealth;private set=>preciseHealth=(decimal)value;}
        public float DragonHealth {get;private set;}=100;
        public bool GateUnlocked {get;private set;}
        public readonly HashSet<int> DeadAnimals=new HashSet<int>();
        public float VillageAngryUntil {get;private set;}
        public bool VictoryOpen {get;private set;}
        public int PortalKeyConsumedFrame {get;private set;}=-1;
        float hurtUntil,hitFlash,nextStrike;
        GameObject adventureActors;
        DreamDragon dragon;
        Texture2D appIcon;
        public static readonly Vector3 PortalCenter=new Vector3(30,19,25);
        public static List<Vector3Int> PortalFrame()
        {
            var result=new List<Vector3Int>();
            for(int x=28;x<=31;x++)for(int y=17;y<=21;y++)if(x==28||x==31||y==17||y==21)result.Add(new Vector3Int(x,y,25));
            return result;
        }
        public bool Damage(float amount,string cause,float attackerDistance=1)
        {
            if(Paused||Health<=0||Time.time<hurtUntil||amount<=0)return false;
            if(float.IsNaN(amount)||float.IsInfinity(amount))return false;
            if(CarRiding){Notice="穿越飞车保护了你，车外攻击无效。";return false;}
            if(ArmorBlocksMelee(cause,attackerDistance,Time.time))return false;
            if(CrystalArmorPieces==4&&IsMeleeCause(cause)){preciseHealth=System.Math.Max(0m,preciseHealth-.0000000000001m);hurtUntil=Time.time+1.1f;Notice="水晶装保护了你，近战只损失0.0000000000001颗心。";return true;}
            if(amount>.5f&&ArmorScore>0)amount=Mathf.Max(.5f,amount*(1-Mathf.Min(.65f,ArmorScore*.075f)));preciseHealth=System.Math.Max(0m,preciseHealth-(decimal)amount);hurtUntil=Time.time+1.1f;hitFlash=.2f;Notice=cause+"：失去 "+amount.ToString("0.##")+" 颗心。";
            if(Health<=0){Notice="本次冒险结束。建造与背包已保留。";SetPaused(true);Save();}
            return true;
        }
        public void Respawn()
        {
            ExitCar(false);
            Health=10;VictoryOpen=false;VillageAngryUntil=0;hurtUntil=Time.time+3;
            theft.Clear();
            World.SwitchRegion(0);Player.Teleport(World.SafeSpawn(),25);RefreshRegion();SetPaused(false);Save();
        }
        public bool EatSelected()
        {
            Block b=(Block)Selected;float heal=Blocks.FoodHeal(b);
            if(Health<=0||Health>=10||heal<=0||Inventory[Selected]<1){Notice="选择苹果、面包或肉再按 R；满血不会消耗食物。";return false;}
            Inventory[Selected]--;Health=Mathf.Min(10,Health+heal);Notice="吃掉"+Blocks.Names[Selected]+"，生命恢复到 "+Health+" / 10。";return true;
        }
        public static bool VillageOwned(Vector3Int p)
        {
            foreach(var plot in VoxelWorld.VillagePlots)if(p.x>=plot.x&&p.x<plot.x+10&&p.z>=plot.y&&p.z<plot.y+10&&p.y>=14&&p.y<=25)return true;
            return false;
        }
        public bool MineBlock(Vector3Int cell)
        {
            Block b=World.Get(cell);
            if(!CanMineSelected&&b!=Block.Lava){Notice="拿着食物、衣服或生活用品不能挖矿；换方块、工具、武器或水晶权杖。";return false;}
            if(EmptyHands&&(b==Block.Bedrock||b==Blocks.StarRock||b==Blocks.DenseRock)){Notice="空手挖不开坚硬岩层，先拿起对应工具。";return false;}
            if(cell.y<0&&b!=Block.Lava)return MineConnectedRock(cell);
            if(b==Blocks.StarRock&&Selected!=(int)Blocks.IronDrill&&Selected!=(int)Blocks.StarDrill&&Selected!=(int)Blocks.CombinedStarPick){Notice="星晶岩要先用铁钻采集；3块星晶岩加2根木棍可合成星晶镐。";return false;}
            if(cell.y==0)return MineDepth(cell,Blocks.DigDelay((Block)Selected,b));
            if(b==Block.PrisonBars)
            {
                if(Selected!=(int)Block.CrystalHammer){Notice="铁栅栏非常坚固：用城堡钥匙按H开门，或手持水晶锤左键拆除。";return false;}
                int broken=0;for(int y=Mathf.Max(1,cell.y-2);y<=Mathf.Min(VoxelWorld.Height-1,cell.y+2);y++){var p=new Vector3Int(cell.x,y,cell.z);if(World.Get(p)==Block.PrisonBars&&World.Edit(p,Block.Air,false))broken++;}
                if(broken==0)return false;World.RebuildAll();Inventory[(int)Block.PrisonBars]=Mathf.Min(9999,Inventory[(int)Block.PrisonBars]+broken);audioSource.PlayOneShot(digSound);Notice="水晶锤一次击碎了整扇铁栅栏，可以走出监狱了。";return true;
            }
            if(b==Block.Lava){
                if(Selected!=(int)Block.Bucket||Inventory[Selected]<1||Inventory[(int)Block.LavaBucket]>=9999){Notice="先合成并手持铁桶，再左键采集岩浆。";return false;}
                if(!World.Edit(cell,Block.Air))return false;Inventory[Selected]--;Inventory[(int)Block.LavaBucket]++;Notice="岩浆装进桶里了，右键可倒出。";return true;
            }
            if(!Blocks.Buildable(b))return false;
            if(Inventory[(int)b]>=9999){Notice="背包装不下了。";return false;}
            bool stolen=World.Region==1&&VillageOwned(cell)&&!World.Edits.ContainsKey(cell);
            if(!World.Edit(cell,Block.Air))return false;ForgetVillagePlacement(cell);MuseumTheft(cell);
            Inventory[(int)b]++;audioSource.PlayOneShot(digSound);CompanionMine(cell,b);
            if(stolen){theft.Add(new SavedBlock{x=cell.x,y=cell.y,z=cell.z,id=(int)b});VillageAngryUntil=Time.time+90;Notice="村民发现你拿走屋内物品！Y归还，或左右躲开追赶。";}
            else Notice="采集了 "+Blocks.Names[(int)b];
            return true;
        }
        public bool StrikeActor()
        {
            if(Paused||Health<=0||Time.time<nextStrike)return false;
            if(!RayFromView(out var hit,7))return false;
            var person=hit.collider.GetComponentInParent<Villager>();if(person!=null&&!person.IsFriend){if(!IsSmoke&&!Input.GetMouseButtonDown(0))return false;nextStrike=Time.time+.12f;person.RegisterHit();return true;}
            var family=hit.collider.GetComponentInParent<SkyResidentInfo>();if(family!=null){if(!IsSmoke&&!Input.GetMouseButtonDown(0))return false;nextStrike=Time.time+.12f;family.RegisterHit();return true;}
            var actor=hit.collider.GetComponentInParent<CombatActor>();if(actor==null)return false;
            nextStrike=Time.time+.45f;
            actor.Hit(Blocks.AttackPower(Inventory[Selected]>0?(Block)Selected:Block.Air));audioSource.PlayOneShot(digSound);return true;
        }
        public bool BuildPortal()
        {
            if(World.Region!=4||Vector3.Distance(Player.transform.position,PortalCenter)>9){Notice="到熔岩竹谷的门框蓝图旁（9米内）按 B。";return false;}
            var missing=new List<Vector3Int>();
            foreach(var p in PortalFrame())if(World.Get(p)!=Block.Obsidian)
            {
                if(World.Get(p)!=Block.Air){Notice="门框位置被别的方块挡住，请先挖走。";return false;}
                if(new Bounds((Vector3)p+Vector3.one*.5f,Vector3.one).Intersects(Player.Body.bounds)){Notice="先离开门框位置，再搭建。";return false;}
                missing.Add(p);
            }
            if(Inventory[(int)Block.Obsidian]<missing.Count){Notice="门框共14块，还缺 "+(missing.Count-Inventory[(int)Block.Obsidian])+" 个黑紫晶块。";return false;}
            foreach(var p in missing)World.Edit(p,Block.Obsidian,false);
            Inventory[(int)Block.Obsidian]-=missing.Count;World.RebuildAll();Save();Notice="门框搭好了！带6个梦影绿晶，按 F 激活进入。";return true;
        }
        public bool PortalComplete()
        {
            if(World.Region!=4)return false;
            foreach(var p in PortalFrame())if(World.Get(p)!=Block.Obsidian)return false;
            for(int x=29;x<=30;x++)for(int y=18;y<=20;y++)if(World.Get(new Vector3Int(x,y,25))!=Block.Air)return false;
            return true;
        }
        public bool EnterPortal()
        {
            if(World.Region!=4||Vector3.Distance(Player.transform.position,PortalCenter)>7){Notice="在传送门旁按 F 才能进入梦影地。";return false;}
            if(!PortalComplete()){Notice="先用14个黑紫晶块搭4×5空心门框（B可按蓝图搭建）。";return false;}
            bool first=!GateUnlocked;
            if(first&&Inventory[(int)Block.DreamCrystal]<6){Notice="激活还需要6个梦影绿晶，去水晶矿谷采集。";return false;}
            if(first){Inventory[(int)Block.DreamCrystal]-=6;GateUnlocked=true;}
            if(!TravelTo(3,true)){if(first){Inventory[(int)Block.DreamCrystal]+=6;GateUnlocked=false;}return false;}
            Notice=DragonHealth>0?"梦影龙从遗迹上空降临！左键攻击，R 吃东西回血。":"梦影龙已被击败，这里现在安全了。";return true;
        }
        public void HitDragon(float amount)
        {
            if(Paused||World.Region!=3||Health<=0||DragonHealth<=0||amount<=0)return;
            if(CompanionEnabled){amount*=2;CompanionStrike();}DragonHealth=Mathf.Max(0,DragonHealth-amount);Notice="命中梦影龙："+DragonHealth+" / 100";
            if(DragonHealth==0){Inventory[(int)Block.ShadowCrystal]=Mathf.Min(9999,Inventory[(int)Block.ShadowCrystal]+20);VictoryOpen=true;SetPaused(true);Save();}
        }
        void SurvivalUpdate()
        {
            if(!IsSmoke&&Input.GetKeyDown(KeyCode.R))EatSelected();
            if(Paused||Health<=0)return;
            hitFlash=Mathf.Max(0,hitFlash-Time.deltaTime);
            var feet=Vector3Int.FloorToInt(Player.transform.position-Vector3.up*.15f);
            if(World.Get(feet)==Block.Lava)Damage(.5f,"踩到岩浆");
            if(!IsSmoke&&Input.GetKeyDown(KeyCode.B))BuildPortal();
            if(!IsSmoke&&Input.GetKeyDown(KeyCode.F)&&World.Region==4){PortalKeyConsumedFrame=Time.frameCount;EnterPortal();}
        }
        public bool CanReachPlayer(Vector3 from,float range)
        {
            Vector3 target=Player.transform.position+Vector3.up;if(Vector3.Distance(from,target)>range)return false;
            return Player.Body.bounds.Contains(from)||(Physics.Linecast(from,target,out var hit,~0,QueryTriggerInteraction.Ignore)&&hit.collider==Player.Body);
        }
        void RestoreSurvival(SavedWorld s)
        {
            Health=s.survivalVersion==0?10:s.health;DragonHealth=s.survivalVersion==0?100:s.dragonHealth;GateUnlocked=s.gateUnlocked;
            if(s.survivalVersion==1&&!string.IsNullOrEmpty(s.preciseHealth))preciseHealth=decimal.Parse(s.preciseHealth,System.Globalization.CultureInfo.InvariantCulture);
            DeadAnimals.Clear();if(s.deadAnimals!=null)foreach(int id in s.deadAnimals)DeadAnimals.Add(id);
            VictoryOpen=false;VillageAngryUntil=0;hurtUntil=Time.time+2;
            theft.Clear();if(World.Region==1&&s.stolen!=null)theft.AddRange(s.stolen);if(theft.Count>0)VillageAngryUntil=Time.time+90;
        }
        void RefreshAdventure()
        {
            if(adventureActors!=null)Destroy(adventureActors);adventureActors=new GameObject("Adventure actors");
            dragon=null;
            if(World.Region==5)for(int i=0;i<6;i++){var p=new GameObject(i<3?"猪灵":"羊灵");p.transform.SetParent(adventureActors.transform);p.transform.position=new Vector3(i<3?17+i*2:43+(i-3)*2,11,i<3?24:43);p.AddComponent<CavernSpirit>().Initialize(this,i<3);}
            if(World.Region==5)foreach(Vector3 pos in new[]{new Vector3(18,14,24),new Vector3(45,14,43),new Vector3(30,14,33)}){var lamp=new GameObject("Lava glow");lamp.transform.SetParent(adventureActors.transform);lamp.transform.position=pos;var light=lamp.AddComponent<Light>();light.type=LightType.Point;light.range=25;light.intensity=2;light.color=new Color(1,.32f,.07f);}
            if(World.Region==1)for(int i=0;i<36;i++)if(!DeadAnimals.Contains(i))
            {
                int house=i/6;int x=house<3?9:42;int z=new[]{8,25,43}[house%3];var a=new GameObject(i%6<3?"Pig":"Sheep");a.transform.SetParent(adventureActors.transform);a.transform.position=new Vector3(x+.7f+i%3*2.8f,15,z-2.3f-(i%6/3)*2.8f);a.transform.localScale=Vector3.one*.75f;a.AddComponent<FarmAnimal>().Initialize(this,i);
            }
            if(World.Region==3&&DragonHealth>0)
            {
                var d=new GameObject("Dream dragon");d.transform.SetParent(adventureActors.transform);d.transform.position=new Vector3(30,42,38);dragon=d.AddComponent<DreamDragon>();dragon.Initialize(this);
            }
            if(World.Region==4)
            {
                var guide=new GameObject("Portal blueprint");guide.transform.SetParent(adventureActors.transform);
                var line=guide.AddComponent<LineRenderer>();line.sharedMaterial=new Material(Resources.Load<Shader>("Selection"));line.sharedMaterial.color=new Color(.5f,1,.7f);line.widthMultiplier=.065f;line.positionCount=5;
                line.SetPositions(new[]{new Vector3(28,17,25),new Vector3(32,17,25),new Vector3(32,22,25),new Vector3(28,22,25),new Vector3(28,17,25)});
                guide.AddComponent<OwnedLineMaterial>();
            }
            if(World.Region==6){var c=new GameObject("王冠城堡系统").AddComponent<CastleSystem>();c.transform.SetParent(adventureActors.transform);c.Initialize(this);AttachCastle(c);}else AttachCastle(null);
        }
        void SurvivalHUD(float w,float h)
        {
            Panel(new Rect(22,149,370,51),new Color(.08f,.13f,.13f,.88f));
            for(int i=0;i<10;i++)
            {
                Rect r=new Rect(36+i*31,157,30,32);Label(r,"♥",25,new Color(.35f,.31f,.33f));
                float portion=Mathf.Clamp01(Health-i);if(portion>0){GUI.BeginGroup(new Rect(r.x,r.y,r.width*portion,r.height));Label(new Rect(0,0,30,32),"♥",25,new Color(1,.3f,.4f));GUI.EndGroup();}
            }
            Label(new Rect(347,164,40,30),Health.ToString("0.#"),13,Color.white);
            if(RoyalIdentity)Label(new Rect(w-340,185,318,28),"皇冠公主 · C / Fn+F6 查看自己",14,new Color(1,.76f,.87f),TextAnchor.MiddleRight);
            if(Bleeding)Label(new Rect(42,198,330,28),"受伤流血 · 战胜公主后卫兵会送医",14,new Color(1,.45f,.5f));
            if(World.Region==3&&DragonHealth>0)
            {
                Panel(new Rect(w/2-105,149,210,51),new Color(.08f,.04f,.13f,.95f));Label(new Rect(w/2-93,151,190,25),"梦影龙  "+DragonHealth.ToString("0")+" / 100",15,new Color(.87f,.66f,1));Panel(new Rect(w/2-93,180,186,9),new Color(.22f,.15f,.28f));Panel(new Rect(w/2-93,180,186*DragonHealth/100,9),new Color(.66f,.3f,.91f));
            }
            if(World.Region==4)Label(new Rect(22,h-168,w-44,28),"门框：黑紫晶块14个  |  激活：梦影绿晶6个  |  靠近绿框 B搭建 / F进入",15,Color.white,TextAnchor.MiddleCenter);
            if(hitFlash>0)Panel(new Rect(0,0,w,h),new Color(1,.1f,.15f,hitFlash*.3f));
        }
        void DrawAdventureEnd(float w,float h)
        {
            Panel(new Rect(0,0,w,h),new Color(.02f,.04f,.07f,.8f));float x=w/2-280,y=h/2-145;Panel(new Rect(x,y,560,290),Paper);
            Label(new Rect(x+28,y+25,510,40),VictoryOpen?"梦影龙已被击败！":"冒险暂时结束",28,Ink);
            Label(new Rect(x+28,y+90,510,68),VictoryOpen?"你获得20个紫水晶，完成这次冒险。\n可以回家继续建造，战果会保留。":"生命耗尽了。背包和建造都已保留。\n回到林地小屋，恢复10颗心后再出发。",17,Muted);
            if(FlatButton(new Rect(x+28,y+194,504,48),"回到小屋  [Enter]",true)){if(VictoryOpen){VictoryOpen=false;TravelTo(0);}else Respawn();}
        }
    }
    public class OwnedLineMaterial:MonoBehaviour {void OnDestroy(){var l=GetComponent<LineRenderer>();if(l!=null)Destroy(l.sharedMaterial);}}
}
