using System;using System.Text.RegularExpressions;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public static string SecretResidentKey(Transform person){if(person==null)return null;var v=person.GetComponent<Villager>();if(v!=null&&!v.IsFriend)return "adult:"+v.ResidentId;var family=person.GetComponent<SkyResidentInfo>();if(family!=null&&family.Owner!=null)return "family:"+family.Owner.name+":"+family.Index;var community=person.GetComponent<CommunityCitizen>();return community!=null?"community:"+person.name:null;}
    public bool DiscloseSecretDoorTo(Transform person,string message)
    {
        if(person==null||string.IsNullOrEmpty(message)||!message.Contains("门")||!message.Contains("密码"))return false;string key=SecretResidentKey(person);if(key==null){Notice="这个对象不接收游戏门密码。";return true;}var match=Regex.Match(message,@"门(?:的)?\s*密码\s*(?:是|为|[:：])\s*[:：]?\s*(\d{4,12})(?!\d)");if(!match.Success||message.Contains("不告诉")||message.Contains("不要告诉")||message.Contains("不是")){Notice="没有明确提供完整的游戏门密码，居民没有获得门权限。不要分享真实账户密码。";return true;}
        OwnedHome home=null;foreach(var h in OwnedHomes)if(h.region==World.Region&&!h.realm&&h.deepBaseVersion==1&&MatchesRoomPin(h.doorSalt,h.doorHash,match.Groups[1].Value)){home=h;break;}if(home==null){Notice="这不是已设置的游戏门密码，居民没有获得进入权限。";return true;}
        if(home.doorGuests.Contains(key)){Notice="这位居民已知道这个游戏门密码。";return true;}home.doorGuests.Add(key);try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);}catch(Exception){home.doorGuests.Remove(key);Notice="权限没有保存，居民仍不能进入。";return true;}Notice="你主动告诉了这位居民游戏门密码，他现在可以进门；保险箱仍需要另一个密码。";return true;
    }
    public bool ResidentKnowsDoor(OwnedHome home,string key)=>home!=null&&home.doorGuests!=null&&home.doorGuests.Contains(key);
    public void ForgetDoorGuests(){var h=FindSecretHome();if(h==null)return;var old=new System.Collections.Generic.List<string>(h.doorGuests);h.doorGuests.Clear();try{WorldSave.Write(WorldSave.Capture(World,Player,Inventory,Selected),WorldSave.PathName);Notice="已收回所有居民的游戏门权限。";}catch(Exception){h.doorGuests=old;Notice="权限收回没有保存，已恢复。";}}
}}
