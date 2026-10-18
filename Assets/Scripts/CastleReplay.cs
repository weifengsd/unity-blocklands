using UnityEngine;

namespace Blocklands { public partial class Sandbox
{
    public bool HandleCastleReplayKey(KeyCode key)
    {
        if(key!=KeyCode.Alpha1||Paused||World.Region!=6||InVoidRealms||PhoneOpen||inspecting||RoyalMenuOpen||PalaceConsoleOpen)return false;
        return RestartCastlePrincessChallenge();
    }

    public bool RestartCastlePrincessChallenge()
    {
        if(Paused||World.Region!=6||InVoidRealms||PhoneOpen||inspecting||RoyalMenuOpen||PalaceConsoleOpen)return false;
        if(saveBlocked){Notice="请先恢复存档，暂时不能重开公主挑战；你的身份和物品没有改变。";return true;}
        if(!RoyalIdentity){Notice="你现在已经是普通访客，原公主的挑战正在开放。门口按J登记，再到十层瞄准公主按H。";return true;}
        if(PalaceLeisure!=null){PalaceLeisure.CloseConsole();PalaceLeisure.StopRide();}
        CancelFestivalForNavigation();CloseRoyalBook();ClearRoyalThrone();PutAwayFlowerWand();
        if(castle!=null)castle.gameObject.SetActive(false);
        RoyalIdentity=false;PrincessReturnVersion=Mathf.Max(PrincessReturnVersion,RequestedPrincessReturn);
        Player.Teleport(new Vector3(31.5f,16,10));Player.HomeRest(false);
        RefreshRegion();Player.Teleport(new Vector3(31.5f,16,10));SetPaused(false);Save();
        Notice="已按你的选择重开挑战：你恢复普通人身份，原公主重新出现。J申请参观；再次获胜后她会退场，直到你下次主动按1。背包、钱和房屋都保留。";
        return true;
    }
}}
