using System;using System.Collections.Generic;using System.Text;using System.Security.Cryptography;using UnityEngine;
namespace Blocklands
{
    [Serializable] public class InvitationDesign {public int id;public string key,title,details;public int guests;public List<DrawingStroke> drawing=new List<DrawingStroke>();}
    public partial class Sandbox
    {
        public readonly List<InvitationDesign> InvitationDesigns=new List<InvitationDesign>();public readonly List<int> MadeInvitations=new List<int>();
        public readonly int[] QuickItems={1,2,3,4,5,6,7,8,9};
        [Serializable] class DesignContent {public string title,details;public int guests;public List<DrawingStroke> drawing;}
        public InvitationDesign CurrentHeldInvitation {get {if(MadeInvitations.Count==0)return null;int id=MadeInvitations[0];return InvitationDesigns.Find(d=>d.id==id);}}
        public int InvitationCount=>MadeInvitations.Count;
        public bool CreateInvitationCard()
        {
            if(MadeInvitations.Count>=9999){Notice="邀请函已经放满了，先递送一些。";return false;}
            if(string.IsNullOrWhiteSpace(InvitationTitle)){Notice="先给邀请函写一个主题。";return false;}
            var content=new DesignContent{title=InvitationTitle,details=InvitationDetails,guests=InvitationGuests,drawing=Drawing};string key;
            using(var hash=SHA256.Create())key=Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(content))));
            var design=InvitationDesigns.Find(d=>d.key==key);
            if(design==null){if(InvitationDesigns.Count>=128){Notice="不同的画稿已经很多了，先把旧邀请函送出去。";return false;}int id=1;foreach(var d in InvitationDesigns)id=Mathf.Max(id,d.id+1);design=new InvitationDesign{id=id,key=key,title=InvitationTitle,details=InvitationDetails,guests=InvitationGuests};int total=0;foreach(var stroke in Drawing)total+=stroke.points.Count;int stride=Mathf.Max(1,Mathf.CeilToInt(total/4096f));foreach(var stroke in Drawing){var copy=new DrawingStroke{color=stroke.color};for(int i=0;i<stroke.points.Count;i+=stride)copy.points.Add(stroke.points[i]);if(copy.points.Count>0)design.drawing.Add(copy);}InvitationDesigns.Add(design);}
            MadeInvitations.Add(design.id);Inventory[93]=MadeInvitations.Count;QuickItems[8]=93;Save();Notice="制作好1张邀请函！纸和铅笔一直保留。点“拿在手里”，或回到游戏按9拿起。";return true;
        }
        public bool HoldInvitation(){if(MadeInvitations.Count==0){Notice="先在画纸旁点“制作一张邀请函”。";return false;}QuickItems[8]=93;SelectItem(93);if(LifeWorkshopOpen)CloseLifeWorkshop();Notice="邀请函拿在手里了。走近并看着收信人，按Enter递给对方。";return true;}
        public Transform NearbyPerson(bool aimedOnly=false)
        {
            if(RayFromView(out var hit,4,QueryTriggerInteraction.Ignore)){var nav=hit.collider.GetComponentInParent<NpcNavigation>();if(nav!=null&&nav.GetComponent<MeadowRabbit>()==null)return nav.transform;var villager=hit.collider.GetComponentInParent<Villager>();if(villager!=null)return villager.transform;var family=hit.collider.GetComponentInParent<SkyResidentInfo>();if(family!=null)return family.transform;}
            if(aimedOnly)return null;return NearestCommunityPerson();
        }
        public bool DeliverInvitationTo(Transform person)
        {
            if(person==null||Paused||CurrentHeldInvitation==null||Vector3.Distance(person.position,Player.transform.position)>4){Notice="拿着邀请函，走近并看着要邀请的人，再按Enter。";return false;}
            var design=CurrentHeldInvitation;int guests=HomePartyGuests.Count,kit=partyKitVersion;var stock=(int[])PartyStock.Clone();if(!AcceptHomePartyInvitation(person)){Notice="宾客名单已满，邀请函还在手里。";return false;}int card=MadeInvitations[0];MadeInvitations.RemoveAt(0);Inventory[93]=MadeInvitations.Count;InvitationsSent++;
            if(!SavePartyState()){while(HomePartyGuests.Count>guests)HomePartyGuests.RemoveAt(HomePartyGuests.Count-1);Array.Copy(stock,PartyStock,PartyStock.Length);partyKitVersion=kit;MadeInvitations.Insert(0,card);Inventory[93]=MadeInvitations.Count;InvitationsSent--;return false;}
            var gift=new GameObject("亲手递出的邀请函").transform;gift.SetParent(transform,false);gift.position=Player.View.transform.position+Player.View.transform.forward*.65f;HeldItemArt.BuildCard(gift,design);gift.gameObject.AddComponent<InvitationHandoff>().Begin(this,person);Notice=person.name+"："+partyReply;return true;
        }
        public bool TryPersonEnter()
        {
            if(Paused||Health<=0||VictoryOpen||PhoneOpen||InspectingHero||CompanionReturnBlocked||RoyalMenuOpen)return false;var person=NearbyPerson();if(person==null||Vector3.Distance(person.position,Player.transform.position)>4)return false;
            if(Selected==93)return DeliverInvitationTo(person);
            var speech=person.GetComponent<NeighbourSpeech>();if(speech!=null&&((speech.Kind==NeighbourSpeechKind.CastleEnvy&&!InvitedChildStyles.Contains(person.GetComponent<CastleGirlStyle>().StyleId))||(speech.Kind==NeighbourSpeechKind.PlanetWish&&InPlanetLife&&!PlanetHeavenVisitors.Contains(person.GetComponent<PlanetCitizenAge>().CitizenId))))return OpenStickerFor(speech);
            OpenLifeWorkshop();lifeTab=3;communityPerson=person;communityReply=person.name+"：你好，我们可以聊聊派对和一起玩的事。";return true;
        }
        void RestoreInvitationCards(SavedWorld s)
        {
            InvitationDesigns.Clear();if(s.invitationDesigns!=null)InvitationDesigns.AddRange(s.invitationDesigns);MadeInvitations.Clear();if(s.madeInvitations!=null)MadeInvitations.AddRange(s.madeInvitations);
            if(s.invitationVersion<2&&Inventory[93]>0){int n=Inventory[93];CreateLegacyCardDesign();for(int i=0;i<n;i++)MadeInvitations.Add(1);}
            Inventory[91]=Inventory[92]=0;Inventory[93]=MadeInvitations.Count;for(int i=0;i<9;i++)QuickItems[i]=s.quickItems!=null&&s.quickItems.Length==9?s.quickItems[i]:i+1;if(MadeInvitations.Count>0)QuickItems[8]=93;
            if(Selected==91||Selected==92)Selected=QuickItems[0];
        }
        void CreateLegacyCardDesign(){var d=new InvitationDesign{id=1,key="legacy",title=InvitationTitle,details=InvitationDetails,guests=InvitationGuests};foreach(var stroke in Drawing)d.drawing.Add(new DrawingStroke{color=stroke.color,points=new List<Vector2>(stroke.points)});InvitationDesigns.Add(d);}
        public bool WearArmorSet(bool alloy){int start=alloy?86:81;for(int i=0;i<4;i++)if(Inventory[start+i]<1){Notice="这是材料和衣服的区别：先合成帽、衣服、裤子、鞋，再穿整套。";return false;}for(int i=0;i<4;i++)ArmorSlots[i]=start+i;armorAppearanceRevision++;EquipSetWeapon(alloy);Save();Notice="已经穿上整套"+(alloy?"下界合金装":"水晶装")+"。";return true;}
        public bool CraftArmorSet(bool alloy)
        {
            int material=alloy?80:78,amount=alloy?26:25,start=alloy?86:81;
            if(Inventory[material]<amount||Inventory[(int)Block.Stick]<1||(!alloy&&Inventory[74]<1)){Notice=alloy?"整套合金装（含剑）需要26合金锭和1木棍。":"整套水晶装（含权杖）需要25水晶块、1水晶杆、1木棍。";return false;}
            for(int i=0;i<5;i++)if(Inventory[start+i]>=9999){Notice="装备库存已满。";return false;}
            Inventory[material]-=amount;Inventory[(int)Block.Stick]--;if(!alloy)Inventory[74]--;for(int i=0;i<5;i++)Inventory[start+i]++;Save();Notice="已经做成整套装备，可以点“穿整套”。";return true;
        }
    }
    public class InvitationHandoff:MonoBehaviour
    {
        Sandbox game;Transform receiver;Vector3 from;float age;
        public void Begin(Sandbox g,Transform person){game=g;receiver=person;from=transform.position;foreach(var label in GetComponentsInChildren<TextMesh>())if(label.GetComponent<WorldTextDepth>()==null)label.gameObject.AddComponent<WorldTextDepth>();}
        void Update(){if(game==null||receiver==null){Destroy(gameObject);return;}if(game.Paused)return;age+=Time.deltaTime;Vector3 to=receiver.TransformPoint(new Vector3(.38f,1.05f,.35f));transform.position=Vector3.Lerp(from,to,Mathf.SmoothStep(0,1,Mathf.Clamp01(age/1.2f)))+Vector3.up*Mathf.Sin(Mathf.Clamp01(age/1.2f)*Mathf.PI)*.12f;transform.rotation=Quaternion.LookRotation(transform.position-game.Player.View.transform.position);if(age>6)Destroy(gameObject);}
    }
}
