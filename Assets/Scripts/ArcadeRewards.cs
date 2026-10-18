using UnityEngine;
namespace Blocklands {public partial class Sandbox {
  public int EarnActivityCoins(int amount,string activity){
   if(amount<11||amount>100||Health<=0||saveBlocked)return 0;
   int before=Inventory[(int)Block.Coin],bankBefore=PalaceSavings;
   int toWallet=Mathf.Min(amount,9999-before);Inventory[(int)Block.Coin]+=toWallet;
   int toBank=Mathf.Min(amount-toWallet,1000000-bankBefore);PalaceSavings+=toBank;
   int awarded=toWallet+toBank;
   try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);Notice=activity+"完成，获得"+awarded+"元"+(toBank>0?"；钱包已满，其余放进金库。":"！");return awarded;}
   catch(System.Exception e){Inventory[(int)Block.Coin]=before;PalaceSavings=bankBefore;Notice="存档失败，本次奖励已撤销："+e.Message;return 0;}
  }
}}
