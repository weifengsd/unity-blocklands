using UnityEngine;

namespace Blocklands
{
    public partial class Sandbox
    {
        static bool Says(string text,params string[] phrases){foreach(string phrase in phrases)if(text.Contains(phrase))return true;return false;}
        string AnswerPhoneMessage(string original)
        {
            string line=original.Replace(" ","").Replace("，","").Replace("。","").Replace("？","").Replace("?","");
            bool friend=PhoneContact==0;
            bool friendNamed=Says(line,"小禾","小和","小河","小何","小猴","我的朋友");
            string speaker=friend?"小禾：":"城堡管家：";
            bool statusQuestion=Says(line,"来了吗","到了吗","到哪","还有几","还有多久","什么时候到","多久到","在哪儿","在哪里","怎么还没来","为什么不来")||(Says(line,"为什么","怎么")&&Says(line,"不来","没来"));
            if(statusQuestion&&(friend||friendNamed))return speaker+ForestFriendStatusText+"。";

            // A bodily need, joke or question is never treated as an invitation just because it contains “来”.
            if(Says(line,"上厕所","洗手间","厕所","拉屎","一坨屎","屎","大便","小便","撒尿","尿","放屁"))
                return speaker+(friend?"你先照顾好自己、记得洗手。我不会因为这句话突然跑过去；等你想玩再叫我。":"我听明白了，洗手间在城堡内；我不会因此擅自派小禾过去。");
            if(Says(line,"你来干嘛","过来干嘛","来这里干嘛","来了做什么"))
                return speaker+(friend?"如果你邀请我，我可以陪你去景点玩、聊天、看风景；不邀请我就留在小屋。":"可以和小禾一起游玩新景点，不过是否邀请由您决定。");
            if(Says(line,"不要来","不要过来","别来","别过来","不用来","不必来","不需要来","不想让你来","不想你过来","能不能不过来"))
                return speaker+(friend?"好，我不会出发。你想聊天就继续说。":"明白，我不会让小禾出发。");

            bool giftRequest=Says(line,"寄礼物","送礼物","送点东西","寄点东西","带礼物","给我礼物");
            if(giftRequest){RequestFriendGift();return speaker+(friend?"我准备两块小饼干，礼物会送到你所在地区的入口。":"已经安排礼物送到您现在所在地区的入口。看来您想收礼物，不是请小禾来玩。");}
            if(Says(line,"礼物到哪","礼物到了吗","礼物呢"))return speaker+(FriendGiftComing?"礼物还在路上，稍等几秒。":FriendGiftWaiting?"礼物已到当地入口，走近按G领取。":"目前没有正在送的礼物。");

            bool explicitInvite=Says(line,"来我这里玩","来我这儿玩","来这里玩","来这儿玩","过来玩","一起玩","来找我","过来找我","来做客","来陪我","和我一起玩","一起来玩","你能过来","能不能过来","可以过来","你可以过来","能过来","过来一下")
                ||Says(line,"过来","你过来","小禾过来","小和过来","小猴过来","小何过来","来一下")&&line.Length<=6
                ||Says(line,"邀请小禾","邀请小和","邀请小河")
                ||friendNamed&&Says(line,"请她过来","让她过来","叫她过来","请小禾来","让小禾来","让小禾过来","叫小禾过来","请小和来","让小和来","让小和过来")
                ||(friend||friendNamed)&&Says(line,"能不能来","可以来")&&Says(line,"玩","找我","这里");
            if(explicitInvite&&(friend||friendNamed))
            {
                InviteForestFriend();
                return speaker+(friend?"我听到你的邀请啦！":"我会把你的邀请转告小禾。")+ForestFriendStatusText+"。";
            }
            if(!friend&&friendNamed)return speaker+ForestFriendStatusText+"。";

            for(int i=0;i<VoxelWorld.RegionNames.Length;i++)
                if(line.Contains(VoxelWorld.RegionNames[i]))
                    return speaker+"你说的是「"+VoxelWorld.RegionNames[i]+"」吧？"+VoxelWorld.RegionDescriptions[i]+(i>=11?"按T打开旅行地图，切到新增游玩景点就能去。":"按T打开旅行地图看看路线。");
            if(Says(line,"景点","旅行","去哪玩","哪里玩","游乐场"))
                return speaker+"这次有彩虹游乐场、海风沙滩、云端风筝谷、萤火秘林和雪晶山谷。按T打开旅行地图切换到新增景点，每个地方都有不同的小游戏。";
            if(Says(line,"吃饭","饿了","面包","饼干","点心","做饭"))
                return speaker+(friend?"先带点吃的吧！森林小屋有料理台，靠近按G能做野莓面包。":"城堡餐厅有草莓点心，森林小屋也能做面包。吃饱再去玩会更有精神。");
            if(Says(line,"下雨","天气","太阳","天黑","夜晚"))
                return speaker+"我们可以先看看天空，再决定去海边吹风还是到屋里休息。";
            if(Says(line,"怪物","害怕","危险","救命"))
                return speaker+(friend?"我在电话里陪你，先找安全地方；如果想让我到你身边，请明确邀请我。":"先到安全处休息，需要我帮忙联系小禾就明确告诉我。");
            if(Says(line,"你好","在吗","早上好","晚上好"))
                return speaker+(friend?"我在呢！今天想聊哪个景点，还是想我来陪你玩？":"我在城堡值班。您想问小禾的行程，还是想听景点介绍？");
            string excerpt=original.Length>28?original.Substring(0,28)+"…":original;
            return speaker+"我听到你说「"+excerpt+"」。你想让我陪你做什么？也可以告诉我想去哪个景点。";
        }
    }
}
