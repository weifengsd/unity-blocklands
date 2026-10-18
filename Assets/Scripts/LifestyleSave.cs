using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands
{
    [Serializable] public class DrawingStroke {public Color color;public List<Vector2> points=new List<Vector2>();}
    [Serializable] public class OwnedHome {public int region,x,y,z;public bool basement,realm,legacy;public int secretRoomVersion,safeMoney,deepBaseVersion,deepPrizeMask,partyHallVersion,partyX,partyZ,partyW,partyD,partyDir,partyRouteVersion,transparentHouseVersion;public List<string> doorGuests=new List<string>();public int[] storedItems;public string doorSalt,doorHash,safeSalt,safeHash;}
    public partial class Sandbox
    {
        public readonly List<DrawingStroke> Drawing=new List<DrawingStroke>();public readonly List<OwnedHome> OwnedHomes=new List<OwnedHome>();
        public readonly int[] ArmorSlots=new int[4];public bool VillageGuardianEnabled;public string InvitationTitle="我的花园派对",InvitationDetails="一起吃点心、画画和玩球";public int InvitationGuests=3,InvitationsSent;
        public int ArmorScore {get {int n=0;foreach(int id in ArmorSlots)if(id>0)n+=id>=86?2:1;return n;}}
        public bool WearArmor(int id)
        {
            if(id==85||id==90){if(Inventory[id]<1)return false;SelectItem(id);Notice="已经拿起"+Blocks.Names[id];return true;}
            int slot=id>=81&&id<=84?id-81:id>=86&&id<=89?id-86:-1;if(slot<0||Inventory[id]<1)return false;ArmorSlots[slot]=ArmorSlots[slot]==id?0:id;armorAppearanceRevision++;if(CrystalArmorPieces==4)EquipSetWeapon(false);else if(AlloyArmorPieces==4)EquipSetWeapon(true);Save();Notice=ArmorSlots[slot]==0?"已经脱下装备。":"穿上了"+Blocks.Names[id]+"，C查看全身。";return true;
        }
        int armorAppearanceRevision,lastShadowArmor=-1;RoyalArt armoredShadow;bool lifestyleStarterGranted;
        void GrantLifestyleStarter(){lifestyleStarterGranted=true;Inventory[91]=Inventory[92]=0;EnsurePartyKit();}
        public void AddArmorAppearance(RoyalArt art)
        {
            foreach(Transform t in art.transform)if(t.name=="裙摆")t.gameObject.SetActive(ArmorSlots[2]==0);
            for(int i=0;i<4;i++)if(ArmorSlots[i]>0){Color c=Blocks.ColorOf((Block)ArmorSlots[i]);
                if(i==0){art.Piece("护甲帽顶",new Vector3(0,1.85f,0),new Vector3(.57f,.13f,.51f),c);foreach(float x in new[]{-.25f,.25f})art.Piece("护甲帽侧",new Vector3(x,1.67f,0),new Vector3(.08f,.34f,.48f),c);}
                if(i==1){art.Piece("水晶胸甲",new Vector3(0,1.05f,.01f),new Vector3(.54f,.5f,.36f),c);art.Piece("胸甲发光宝石",new Vector3(0,1.14f,.21f),new Vector3(.13f,.16f,.08f),Color.white,PrimitiveType.Sphere);}
                if(i==2)foreach(float x in new[]{-.14f,.14f})art.Piece("装备裤腿",new Vector3(x,.42f,0),new Vector3(.19f,.58f,.25f),c);
                if(i==3)foreach(float x in new[]{-.14f,.14f})art.Piece("装备靴子",new Vector3(x,.13f,.08f),new Vector3(.22f,.24f,.34f),c);
            }
            if(Selected==85&&!EmptyHands){art.Piece("护甲权杖柄",new Vector3(.5f,1.05f,.25f),new Vector3(.07f,1.3f,.07f),new Color(.22f,.64f,.93f));art.Piece("护甲权杖蓝晶",new Vector3(.5f,1.8f,.25f),new Vector3(.24f,.33f,.24f),new Color(.35f,.88f,1));}
        }
        void TickArmorAppearance(){if(shadowBody==null||armoredShadow==shadowBody&&lastShadowArmor==armorAppearanceRevision)return;foreach(Transform t in shadowBody.transform)if(t.name.Contains("护甲")||t.name.Contains("胸甲")||t.name.Contains("装备"))Destroy(t.gameObject);AddArmorAppearance(shadowBody);foreach(var r in shadowBody.GetComponentsInChildren<Renderer>())r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;lastShadowArmor=armorAppearanceRevision;armoredShadow=shadowBody;}
        void RestoreLifestyle(SavedWorld s)
        {
            lifestyleStarterGranted=s.lifestyleVersion>=1;
            Drawing.Clear();if(s.drawing!=null)Drawing.AddRange(s.drawing);OwnedHomes.Clear();if(s.ownedHomes!=null)OwnedHomes.AddRange(s.ownedHomes);
            Array.Clear(ArmorSlots,0,4);if(s.armorSlots!=null)for(int i=0;i<Mathf.Min(4,s.armorSlots.Length);i++)if(s.armorSlots[i]>0&&Inventory[s.armorSlots[i]]>0)ArmorSlots[i]=s.armorSlots[i];armorAppearanceRevision++;
            VillageGuardianEnabled=s.villageGuardian;InvitationTitle=string.IsNullOrEmpty(s.invitationTitle)?"我的花园派对":s.invitationTitle;InvitationDetails=s.invitationDetails??"一起吃点心、画画和玩球";InvitationGuests=Mathf.Clamp(s.invitationGuests==0?3:s.invitationGuests,1,8);InvitationsSent=s.invitationsSent;
            GrantLifestyleStarter();
            RestoreInvitationCards(s);
        }
    }
}
