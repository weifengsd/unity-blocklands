using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public class JoinedHandsRig:MonoBehaviour
    {
        Transform[] arms=new Transform[2],palms=new Transform[2];
        public void Initialize(int layer=0)
        {
            var art=gameObject.AddComponent<RoyalArt>();Color skin=new Color(.94f,.69f,.53f);
            for(int i=0;i<2;i++){arms[i]=art.Piece(i==0?"左前臂":"右前臂",Vector3.zero,new Vector3(.11f,.2f,.11f),skin,PrimitiveType.Capsule);palms[i]=new GameObject(i==0?"左手握姿":"右手握姿").transform;palms[i].SetParent(transform,false);var hand=palms[i].gameObject.AddComponent<RoyalArt>();hand.Piece("手掌",Vector3.zero,new Vector3(.16f,.08f,.15f),skin,PrimitiveType.Sphere);for(int finger=0;finger<4;finger++)hand.Piece("抓牢的手指",new Vector3((finger-1.5f)*.031f,-.035f,.055f),new Vector3(.025f,.045f,.08f),skin,PrimitiveType.Capsule);hand.Piece("大拇指",new Vector3(i==0?.07f:-.07f,0,0),new Vector3(.04f,.08f,.04f),skin,PrimitiveType.Capsule);}
            foreach(var t in GetComponentsInChildren<Transform>())t.gameObject.layer=layer;
        }
        public void Pose(int side,Vector3 shoulder,Vector3 hand,Quaternion rotation,bool visible,float scale=1)
        {
            arms[side].gameObject.SetActive(visible);palms[side].gameObject.SetActive(visible);if(!visible)return;
            Vector3 d=hand-shoulder;arms[side].position=(hand+shoulder)*.5f;arms[side].rotation=d.sqrMagnitude>.001f?Quaternion.FromToRotation(Vector3.up,d.normalized):Quaternion.identity;arms[side].localScale=new Vector3(.11f*scale,d.magnitude*.5f,.11f*scale);palms[side].position=hand;palms[side].rotation=rotation;palms[side].localScale=Vector3.one*scale;
        }
    }
    public partial class Sandbox
    {
        JoinedHandsRig ownPartyHands;readonly Dictionary<Transform,JoinedHandsRig> companionHands=new Dictionary<Transform,JoinedHandsRig>();readonly List<GameObject> partyPortraitObjects=new List<GameObject>();
        public bool PartyHoldingHands=>!CoasterSlipActive&&!PartyPlayingBall&&!CoasterRiding&&!InspectingHero&&(ForestFriendFollowing||InvitedChildCount>0);
        JoinedHandsRig HandsFor(Transform actor,int layer=0)
        {
            if(companionHands.TryGetValue(actor,out var rig)&&rig!=null){rig.gameObject.SetActive(true);return rig;}
            var obj=new GameObject("真正的手掌与前臂，不是牵引线");obj.transform.SetParent(transform,false);rig=obj.AddComponent<JoinedHandsRig>();rig.Initialize(layer);companionHands[actor]=rig;return rig;
        }
        void OriginalArm(Transform actor,int side,bool visible){foreach(Transform child in actor)if(child.name=="手臂"&&(side==0?child.localPosition.x<0:child.localPosition.x>0))child.gameObject.SetActive(visible);}
        Vector3 Shoulder(Transform actor,int side)=>actor.TransformPoint(new Vector3(side==0?-.31f:.31f,1.12f,.03f));
        void ActorHand(Transform actor,int side,Vector3 target,bool visible,Quaternion rotation)
        {
            if(visible&&Vector3.Distance(Shoulder(actor,side),target)>2.2f)visible=false;
            var rig=HandsFor(actor);rig.Pose(side,Shoulder(actor,side),target,rotation,visible,actor.localScale.x);OriginalArm(actor,side,!visible);
        }
        void TickPartyHands()
        {
            if(Player==null)return;var members=PartyMembers();bool grip=CoasterRiding&&ActiveCoaster.CoasterGripHeld;bool walking=PartyHoldingHands&&!Paused&&!HomeSeated&&!HoldingBook&&!VoidRideActive;
            if(ownPartyHands==null){var obj=new GameObject("主角真正伸出的双手");obj.transform.SetParent(transform,false);ownPartyHands=obj.AddComponent<JoinedHandsRig>();ownPartyHands.Initialize();}
            ownPartyHands.gameObject.SetActive(grip||walking);Player.SetPartyHandsVisible(grip||walking&&InvitedChildCount>0);
            var stale=new List<Transform>();foreach(var pair in companionHands){if(pair.Key==null){if(pair.Value!=null)Destroy(pair.Value.gameObject);stale.Add(pair.Key);continue;}if(pair.Value!=null)pair.Value.gameObject.SetActive(false);OriginalArm(pair.Key,0,true);OriginalArm(pair.Key,1,true);}foreach(var actor in stale)companionHands.Remove(actor);
            foreach(var actor in members){OriginalArm(actor,0,true);OriginalArm(actor,1,true);}
            if(grip)
            {
                Quaternion rotation=ActiveCoaster.CoasterSeatRotation;ActiveCoaster.HeroGripTargets(out var l,out var r);
                ownPartyHands.Pose(0,Player.transform.position+rotation*new Vector3(-.3f,1,.1f),l,rotation,true);ownPartyHands.Pose(1,Player.transform.position+rotation*new Vector3(.3f,1,.1f),r,rotation,true);
            }
            if(CoasterRiding)
            {
                ActiveCoaster.HeroGripTargets(out var heroLeft,out var heroRight);
                for(int i=0;i<members.Count;i++){ActiveCoaster.PartySeat(i,out var seat,out var rotation);members[i].position=seat;members[i].rotation=rotation;ActiveCoaster.GripTargets(seat,rotation,out var l,out var r);if(i==0)r=heroLeft;else if(i==1)l=heroRight;else{bool leftSeat=(i-2)%2==0;Vector3 joined=seat+rotation*new Vector3(leftSeat?.58f:-.58f,.65f,.62f);if(leftSeat)r=joined;else l=joined;}ActorHand(members[i],0,l,true,rotation);ActorHand(members[i],1,r,true,rotation);}return;
            }
            if(!walking)return;
            Quaternion facing=Player.transform.rotation;Vector3 origin=Player.transform.position;
            ownPartyHands.Pose(0,origin+facing*new Vector3(-.31f,1.12f,.03f),origin+facing*new Vector3(-.4f,1.04f,.69f),facing,ForestFriendFollowing);
            ownPartyHands.Pose(1,origin+facing*new Vector3(.31f,1.12f,.03f),origin+facing*new Vector3(.4f,.97f,.69f),facing,InvitedChildCount>0);
            if(ForestFriendFollowing)ActorHand(forestFriend,1,origin+facing*new Vector3(-.4f,1.04f,.69f),true,facing);
            int index=0;Transform previous=null;
            foreach(int id in InvitedChildStyles)if(invitedModels.TryGetValue(id,out var art)&&art!=null)
            {
                Vector3 joined=index==0?origin+facing*new Vector3(.4f,.97f,.69f):(Shoulder(previous,1)+Shoulder(art.transform,0))*.5f;
                ActorHand(art.transform,0,joined,true,facing);if(previous!=null)ActorHand(previous,1,joined,true,facing);previous=art.transform;index++;
            }
        }
        public void UpdatePartyHandPose()=>TickPartyHands();
        void BuildInvitedPortraits()
        {
            AddArmorAppearance(portrait);
            if(InvitedChildCount==0)return;portrait.transform.position=Studio;
            foreach(var t in portrait.GetComponentsInChildren<Transform>())if(t.name.StartsWith("牵手的")||t.name=="贴合的手掌")t.gameObject.SetActive(false);
            var line=new List<Transform>();if(portraitFriend!=null){portraitFriend.transform.position=Studio+Vector3.left*1.1f;line.Add(portraitFriend.transform);foreach(var t in portraitFriend.GetComponentsInChildren<Transform>())if(t.name.StartsWith("牵手的")||t.name=="贴合的手掌")t.gameObject.SetActive(false);}line.Add(portrait.transform);
            for(int i=0;i<InvitedChildStyles.Count;i++){var a=CastleGirlStyle.Make(transform,InvitedChildStyles[i]);a.transform.position=Studio+Vector3.right*(1.1f+i*.75f);partyPortraitObjects.Add(a.gameObject);foreach(var t in a.GetComponentsInChildren<Transform>())t.gameObject.layer=2;line.Add(a.transform);}
            for(int i=0;i<line.Count-1;i++){var left=line[i];var right=line[i+1];Vector3 joined=(Shoulder(left,1)+Shoulder(right,0))*.5f;foreach(var actor in new[]{left,right}){var rigObj=new GameObject("多人牵手外观的手掌");rigObj.transform.SetParent(transform,false);var rig=rigObj.AddComponent<JoinedHandsRig>();rig.Initialize(2);int side=actor==left?1:0;rig.Pose(side,Shoulder(actor,side),joined,Quaternion.identity,true,actor.localScale.x);rig.Pose(1-side,Vector3.zero,Vector3.zero,Quaternion.identity,false);partyPortraitObjects.Add(rigObj);OriginalArm(actor,side,false);}}
        }
        void DisposeInvitedPortraits(){foreach(var obj in partyPortraitObjects)if(obj!=null)Destroy(obj);partyPortraitObjects.Clear();}
        Vector3 PartyPortraitFocus=>Studio+Vector3.right*Mathf.Max(0,InvitedChildCount-1)*.35f;
        float PartyPortraitDistance=>3.2f+InvitedChildCount*.7f;
    }
    public partial class FirstPerson
    {
        public void SetPartyHandsVisible(bool hideTool){if(hand==null)return;hand.gameObject.SetActive(!hideTool&&!Game.CoasterRiding&&!Game.PalacePlayLocked&&!Game.HomeSeated&&!Game.HoldingBook&&!Game.RoyalThroneSeated&&!Game.RoyalTransportActive&&!Game.InspectingHero&&!Game.VoidRideActive&&!Game.PhoneOpen);}
    }
}
