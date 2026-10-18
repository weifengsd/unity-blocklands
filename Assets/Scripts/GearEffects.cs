using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public bool EmptyHands {get;private set;}
    public void PutHeldItemAway(){EmptyHands=true;PutAwayPartyItem();PutAwayFlowerWand();Player.RefreshHeldItem();Notice="已放下手中物品；选背包物品或快捷栏可重新拿起。";}
    public bool CanMineSelected=>EmptyHands||Blocks.Buildable((Block)Selected)||Blocks.Tool((Block)Selected)||Selected==85||Selected==90||Selected==(int)Block.Bucket;
    public int CrystalArmorPieces {get{int n=0;for(int i=0;i<4;i++)if(ArmorSlots[i]==81+i)n++;return n;}}
    public int AlloyArmorPieces {get{int n=0;for(int i=0;i<4;i++)if(ArmorSlots[i]==86+i)n++;return n;}}
    float armorContactAt=-100,lastArmorContact=-100;int armorContactScore;
    public static bool IsMeleeCause(string cause)=>cause.Contains("村民")||cause.Contains("人链")||cause.Contains("卫兵")||cause.Contains("保安")||cause.Contains("傀儡")||cause.Contains("猪灵")||cause.Contains("羊灵");
    public void ResetArmorForSmoke(){if(IsSmoke){armorContactAt=lastArmorContact=-100;armorContactScore=0;}}
    public bool ArmorBlocksMelee(string cause,float distance,float now)
    {
        if(!IsMeleeCause(cause)||ArmorScore==0)return false;
        if(now-lastArmorContact>2.5f||armorContactScore!=ArmorScore)armorContactAt=now;lastArmorContact=now;armorContactScore=ArmorScore;
        float grace=CrystalArmorPieces*.75f+AlloyArmorPieces*5f;
        if(now-armorContactAt<grace){Notice="护甲挡住了连续近战，还可抵挡"+Mathf.CeilToInt(grace-(now-armorContactAt))+"秒。";return true;}
        return false;
    }
    public bool CrystalHitTarget(Collider collider)
    {
        if(collider==null||FloodResident.IsDown(collider.transform))return false;
        var golem=collider.GetComponentInParent<VillageGuardian>();if(golem!=null){golem.StunByCrystal();Notice="蓝色光波让铁傀儡停顿2秒。";return true;}
        var v=collider.GetComponentInParent<Villager>();if(v!=null&&!v.IsFriend){v.RegisterHit();v.CrystalRecoil();Notice="水晶光波击中村民，村民退让并开始警戒。";return true;}
        var f=collider.GetComponentInParent<SkyResidentInfo>();if(f!=null){f.RegisterHit();Notice="水晶光波让居民退让。";return true;}
        var rabbit=collider.GetComponentInParent<MeadowRabbit>();if(rabbit!=null){rabbit.MagicStartle();Notice="蓝色光波击中兔子，兔子跳开躲避。";return true;}
        var actor=collider.GetComponentInParent<CombatActor>();if(actor!=null){actor.Hit(6);return true;}return false;
    }
}}
