using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Blocklands {public partial class Sandbox {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
  [DllImport("VoiceBridge")] static extern void BL_StartVoice();
  [DllImport("VoiceBridge")] static extern void BL_StopVoice();
  [DllImport("VoiceBridge")] static extern void BL_CancelVoice();
  [DllImport("VoiceBridge")] static extern int BL_VoiceStatus();
  [DllImport("VoiceBridge")] static extern int BL_VoiceErrorCode();
  [DllImport("VoiceBridge")] static extern IntPtr BL_VoiceText();
#endif
  public string PhoneSubtitle {get;private set;}="";
  public string PhoneDraft="";
  public string VoiceState {get;private set;}="按语音键说话，也可以输入文字。";
  public bool VoiceListening {get;private set;}
  public bool PhoneTyping {get;private set;}
  public bool PhoneVoiceNativeAvailable(){
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
   try{return BL_VoiceStatus()>=0&&BL_VoiceErrorCode()>=0;}catch(Exception){return false;}
#else
   return false;
#endif
  }
  float voiceAt,voiceStopAt;
  void ResetPhoneVoice(){StopPhoneVoice(true);PhoneSubtitle="";PhoneDraft="";PhoneTyping=false;VoiceState="按语音键说话，也可以输入文字。";}
  public bool StartPhoneVoice(){if(!PhoneOpen||PhoneApp!=LiftPhoneApp.Calls||PhoneContact<0||!PhoneCallConnected||Paused)return false;if(VoiceListening){StopPhoneVoice(false);return true;}PhoneTyping=false;voiceStopAt=0;voiceAt=Time.unscaledTime;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
   try{BL_StartVoice();VoiceListening=true;VoiceState="正在准备麦克风；首次使用请允许麦克风和语音识别权限。";return true;}
   catch(Exception){VoiceState="本机语音组件不可用，请在下面输入文字。";return false;}
#else
   VoiceState="本机语音识别不可用，请在下面输入文字。";return false;
#endif
  }
  public void StopPhoneVoice(bool cancel){if(!VoiceListening)return;VoiceListening=false;voiceStopAt=cancel?0:Time.unscaledTime;try{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    if(cancel)BL_CancelVoice();else BL_StopVoice();
#endif
   }catch(Exception){}if(cancel)VoiceState="语音已停止。";else VoiceState="正在识别刚才说的话……";}
  void TickPhoneVoice(){if(!PhoneOpen||PhoneApp!=LiftPhoneApp.Calls){if(VoiceListening)StopPhoneVoice(true);return;}if(!VoiceListening&&voiceStopAt<=0)return;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
   try{int state=BL_VoiceStatus();if(state<0){VoiceListening=false;voiceStopAt=0;int code=BL_VoiceErrorCode();VoiceState=state==-1?"麦克风或语音识别未获授权；可在系统设置中允许，或直接输入文字。":state==-2?"本机中文语音识别暂不可用；不是游戏网络差，可直接打字。":code==102?"本机语音资源未安装；可用文字输入。":code==201?"系统听写未启用；可用文字输入。":state==-4?"没有收到声音；请靠近麦克风重试，或打字。":"录音或识别出了问题；请重试，或打字。";return;}
    string words=Marshal.PtrToStringUTF8(BL_VoiceText());
    if(state==2){VoiceListening=false;voiceStopAt=0;if(!string.IsNullOrWhiteSpace(words))SubmitPhoneSpeech(words);else VoiceState="没有识别到说话内容，请重试或输入文字。";return;}
    if(state==0){VoiceState="等待系统授权，请在弹出的权限窗口点允许。";return;}
    if(VoiceListening){VoiceState=string.IsNullOrWhiteSpace(words)?"● 正在收音：说完后再点一次语音键。":"● 听到："+words+"；说完后再点一次。";}
    else if(voiceStopAt>0){VoiceState="正在把刚才的话变成字幕，请稍等……";if(Time.unscaledTime-voiceStopAt>5){voiceStopAt=0;if(!string.IsNullOrWhiteSpace(words))SubmitPhoneSpeech(words);else VoiceState="没听清，请重试或输入文字。";return;}}}
   catch(Exception){VoiceListening=false;voiceStopAt=0;VoiceState="语音组件暂时不可用，请直接输入文字。";return;}
#endif
   if(VoiceListening&&Time.unscaledTime-voiceAt>12)StopPhoneVoice(false);
  }
  public bool SubmitPhoneSpeech(string words){if(!PhoneOpen||PhoneApp!=LiftPhoneApp.Calls||PhoneContact<0||Paused||string.IsNullOrWhiteSpace(words))return false;string said=words.Trim();if(said.Length>240)said=said.Substring(0,240);PhoneSubtitle="你："+said+"\n"+AnswerPhoneMessage(said);PhoneDraft="";phoneCallScroll=Vector2.zero;VoiceState="已收到你说的话。你可以继续聊，也可以按 Esc 收起手机。";return true;}
}}
