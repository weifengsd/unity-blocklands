using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator ExcursionChecks(Sandbox g,string folder)
        {
            var rows=new List<string>();
            void Check(string key,bool okay){rows.Add(key+"="+okay);adventurePass&=okay;File.WriteAllLines(Path.Combine(folder,"excursion-checks.txt"),rows);}
            Check("five_new_destinations_named",VoxelWorld.RegionNames.Length==16&&VoxelWorld.RegionNames[11]=="彩虹游乐场"&&VoxelWorld.RegionNames[15]=="雪晶山谷");
            g.Respawn();yield return null;
            g.OpenTravel();yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"new-travel-map.png"));yield return new WaitForSecondsRealtime(.15f);g.SetPaused(false);
            int oldVillagerFriends=0,princessFriends=0;foreach(var v in FindObjectsByType<Villager>())if(v.IsFriend)oldVillagerFriends++;foreach(var a in FindObjectsByType<RoyalArt>())if(a.name=="公主装扮的小禾")princessFriends++;
            Check("forest_friend_is_one_princess_model",oldVillagerFriends==0&&princessFriends==1&&g.ForestFriendPosition!=Vector3.zero);
            g.Player.Teleport(g.ForestFriendPosition+Vector3.back*4,0,0);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"forest-princess-friend.png"));yield return new WaitForSecondsRealtime(.15f);
            g.OpenPhone();g.BeginPhoneCall(0);g.TickPhone(.7f);
            Check("bathroom_sentence_does_not_invite",g.SubmitPhoneSpeech("我在这里拉了一坨屎，让小禾过来凑热闹")&&!g.ForestFriendWaiting&&!g.ForestFriendArriving&&g.PhoneCallText.Contains("洗手"));
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"topic-conversation.png"));yield return new WaitForSecondsRealtime(.15f);
            Check("question_does_not_invite",g.SubmitPhoneSpeech("为什么你不来我这里玩")&&!g.ForestFriendWaiting&&g.PhoneCallText.Contains("小禾就在"));
            Check("negative_request_does_not_invite",g.SubmitPhoneSpeech("你能不能不过来吗")&&!g.ForestFriendWaiting);
            Check("why_visit_is_answered_not_invited",g.SubmitPhoneSpeech("小禾过来干嘛")&&!g.ForestFriendWaiting&&g.PhoneCallText.Contains("陪你去景点"));
            Check("strange_sentence_is_echoed_not_invited",g.SubmitPhoneSpeech("今天我看到一颗紫色的星星")&&!g.ForestFriendWaiting&&g.PhoneCallText.Contains("紫色的星星"));
            Check("destination_topic_matches_name",g.SubmitPhoneSpeech("我想去彩虹游乐场")&&g.PhoneCallText.Contains("彩虹跳台")&&!g.ForestFriendWaiting);
            Check("natural_question_invites_at_home",g.SubmitPhoneSpeech("你能过来吗")&&g.ForestFriendWaiting&&!g.ForestFriendArriving);
            g.SendForestFriendHome();
            Check("inviting_friend_at_home_needs_no_car",g.SubmitPhoneSpeech("小禾来我这里玩")&&g.ForestFriendWaiting&&!g.ForestFriendArriving);
            g.ClosePhone();g.Player.Teleport(g.ForestFriendPosition+Vector3.right);
            Check("one_home_friend_can_hold_hands",g.TryMeetFriendOrGift()&&g.ForestFriendFollowing);
            Check("X_keeps_home_friend_at_cabin",g.SendForestFriendHome()&&!g.ForestFriendFollowing&&g.ForestFriendPosition!=Vector3.zero);

            int[] rewards={42,54,64,48,72};
            for(int n=0;n<5;n++)
            {
                int id=11+n;Check("travel_to_"+id,g.TravelTo(id)&&g.World.Region==id&&g.Excursion!=null);
                var station=g.Excursion;if(station==null)yield break;
                g.Player.Teleport(station.transform.position+Vector3.back*8,0,8);
                Check("clear_spawn_"+id,!Blocks.Solid(g.World.Get(Vector3Int.FloorToInt(g.World.SafeSpawn()))));
                yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"destination-"+id+".png"));yield return new WaitForSecondsRealtime(.15f);
                g.Player.Teleport(station.transform.position+Vector3.back*1.5f);
                int money=g.Inventory[(int)Block.Coin];
                Check("starts_"+id,g.TryExcursionG()&&station.Active);
                if(id==15){yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"snow-game-instructions.png"));yield return new WaitForSecondsRealtime(.15f);}
                CompleteRound(station);
                Check("named_game_wins_"+id,!station.Active&&station.CompletedRounds==1&&g.Inventory[(int)Block.Coin]==money+rewards[n]);
                g.Player.Teleport(station.transform.position+Vector3.forward*1.5f);
                bool replay=g.TryExcursionG();CompleteRound(station);
                Check("replay_earns_"+id,replay&&!station.Active&&station.CompletedRounds==2&&g.Inventory[(int)Block.Coin]==money+rewards[n]*2);
            }
            Vector3Int change=new Vector3Int(27,17,32);Check("new_region_edit",g.World.Edit(change,Block.CrystalLamp));
            g.TravelTo(0);g.TravelTo(15);Check("new_region_save_roundtrip",g.World.Get(change)==Block.CrystalLamp);
            g.Save();g.Load();Check("new_region_loads",g.World.Region==15&&g.Excursion!=null&&g.World.Get(change)==Block.CrystalLamp);
        }

        static void CompleteRound(ExcursionAttraction station)
        {
            if(station.Kind==ExcursionKind.Rainbow)
            {
                foreach(var key in new[]{KeyCode.LeftArrow,KeyCode.Space,KeyCode.RightArrow,KeyCode.Space,KeyCode.LeftArrow,KeyCode.RightArrow,KeyCode.Space,KeyCode.RightArrow})station.Press(key);
            }
            else if(station.Kind==ExcursionKind.Beach)
            {
                foreach(int target in new[]{2,0,1,2,1,0}){while(station.Lane<target)station.Press(KeyCode.RightArrow);while(station.Lane>target)station.Press(KeyCode.LeftArrow);station.Press(KeyCode.Space);}
            }
            else if(station.Kind==ExcursionKind.Kite)
            {
                foreach(float target in new[]{.52f,.65f,.43f,.7f})for(int step=0;step<60;step++)station.Tick(.05f,station.Altitude<target);
            }
            else if(station.Kind==ExcursionKind.Fireflies)
            {
                foreach(int color in new[]{0,2,1,3,0,1,2})station.Press((KeyCode)((int)KeyCode.Alpha1+color));
            }
            else
            {
                foreach(int hazard in new[]{1,2,0,1,2,0,1,0}){int safe=hazard==1?0:1;while(station.Lane<safe)station.Press(KeyCode.RightArrow);while(station.Lane>safe)station.Press(KeyCode.LeftArrow);station.Tick(2f,false);}
            }
        }
    }
}
