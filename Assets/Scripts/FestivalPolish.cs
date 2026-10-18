using UnityEngine;

namespace Blocklands {
 // Feedback is deliberately separate from rewards, saves and the activity clocks.
 public partial class FestivalStation {
  static readonly string[] PolishArrows={"←","↑","→","↓"};
  static readonly string[] PolishCompass={"↑ 前方","↗ 右前方","→ 右侧","↘ 右后方","↓ 身后","↙ 左后方","← 左侧","↖ 左前方"};
  static readonly int PolishColorId=Shader.PropertyToID("_Color");
  readonly int[] polishSequence=new int[8];
  Vector3[] polishBaseScales;
  Renderer[][] polishRenderers;
  Color[][] polishBaseColors;
  MaterialPropertyBlock polishProperties;
  Transform polishBeacon;
  GUIStyle polishTextStyle,polishArrowStyle;
  string polishMemoryPreview="",polishFeedback="";
  int polishCombo,polishBestCombo,polishLastDirection=-1,polishPreviewBeat=-1;
  float polishFeedbackUntil,polishLastTime,polishBestTime;
  bool polishSequenceReady,polishFeedbackWrong;

  int PolishExpectedDirection=>polishSequenceReady?polishSequence[Mathf.Clamp(progress,0,kind==FestivalKind.Memory?4:7)]:(progress*3+1)%4;
  string PolishMemoryPreview=>polishMemoryPreview;
  bool PolishIsRhythm=>kind==FestivalKind.Dance||kind==FestivalKind.Memory;

  void BeginPolish(){
   if(kind==FestivalKind.Coaster)return;
   CachePolishTargets();
   RestorePolishTargets();
   polishCombo=0;polishLastDirection=-1;polishPreviewBeat=-1;
   polishFeedback="";polishFeedbackUntil=0;polishFeedbackWrong=false;
   if(PolishIsRhythm){
    int count=kind==FestivalKind.Memory?5:8,previous=0,next=0;
    for(int i=0;i<count;i++)previous=previous*4+polishSequence[i];
    // Adjacent arrows differ; an exact repeat of the prior round is also avoided.
    for(int i=0;i<count;i++){
     polishSequence[i]=i==0?Random.Range(0,4):(polishSequence[i-1]+Random.Range(1,4))%4;
     next=next*4+polishSequence[i];
    }
    if(polishSequenceReady&&previous==next){
     do{polishSequence[0]=(polishSequence[0]+1)%4;}while(polishSequence[0]==polishSequence[1]);
    }
    polishSequenceReady=true;
    polishMemoryPreview="";
    for(int i=0;i<5;i++)polishMemoryPreview+=(i==0?"":"  ")+PolishArrows[polishSequence[i]];
   }else if(polishBeacon==null){
    // RoyalArt owns this one-time material and disposes it with the station.
    polishBeacon=art.Piece("下一个探索目标",Vector3.zero,Vector3.one*.34f,new Color(1,.86f,.28f));
   }
   TickPolish();
  }

  void CachePolishTargets(){
   if(polishBaseScales!=null&&polishBaseScales.Length==targets.Count)return;
   polishBaseScales=new Vector3[targets.Count];
   polishRenderers=new Renderer[targets.Count][];
   polishBaseColors=new Color[targets.Count][];
   polishProperties=new MaterialPropertyBlock();
   for(int i=0;i<targets.Count;i++){
    polishBaseScales[i]=targets[i].localScale;
    polishRenderers[i]=targets[i].GetComponentsInChildren<Renderer>(true);
    polishBaseColors[i]=new Color[polishRenderers[i].Length];
    for(int j=0;j<polishRenderers[i].Length;j++){
     var material=polishRenderers[i][j].sharedMaterial;
     polishBaseColors[i][j]=material!=null&&material.HasProperty(PolishColorId)?material.GetColor(PolishColorId):Color.white;
    }
   }
  }

  void PolishCorrect(){
   if(kind==FestivalKind.Coaster)return;
   polishFeedbackWrong=false;
   polishFeedbackUntil=elapsed+1.3f;
   if(PolishIsRhythm){
    polishLastDirection=polishSequence[Mathf.Clamp(progress-1,0,polishSequence.Length-1)];
    polishCombo++;polishBestCombo=Mathf.Max(polishBestCombo,polishCombo);
    polishFeedback=polishCombo>=4?"太棒了！ "+polishCombo+" 连击":polishCombo>=2?"稳稳接住！ "+polishCombo+" 连击":"对啦！继续跟上";
   }else{
    polishFeedback=kind==FestivalKind.Dig?"遗迹发现！ "+progress+" / "+targets.Count:kind==FestivalKind.Flight?"漂亮穿环！ "+progress+" / "+targets.Count:kind==FestivalKind.Footprints?"追踪成功！ "+progress+" / "+targets.Count:"航标到达！ "+progress+" / "+targets.Count;
   }
  }

  void PolishWrong(){
   // A slip breaks the streak, not the steps the player already completed.
   polishCombo=0;polishFeedbackWrong=true;polishLastDirection=-1;
   polishFeedback="没关系，已完成的步骤保留，再试当前这一步。";
   polishFeedbackUntil=elapsed+1.7f;
   game.Notice=polishFeedback;
  }

  void CompletePolish(){
   if(kind==FestivalKind.Coaster)return;
   polishLastTime=elapsed;
   if(polishBestTime<=0||elapsed<polishBestTime)polishBestTime=elapsed;
  }

  void StopPolish(){
   if(polishBeacon!=null)polishBeacon.gameObject.SetActive(false);
   RestorePolishTargets();
  }

  void RestorePolishTargets(){
   if(polishBaseScales==null)return;
   for(int i=0;i<targets.Count&&i<polishBaseScales.Length;i++){
    if(targets[i]!=null)targets[i].localScale=polishBaseScales[i];
    foreach(var renderer in polishRenderers[i])if(renderer!=null)renderer.SetPropertyBlock(null);
   }
  }

  void TickPolish(){
   if(!Running||kind==FestivalKind.Coaster||polishBaseScales==null)return;
   int active=progress;
   if(PolishIsRhythm){
    active=kind==FestivalKind.Dance?PolishExpectedDirection:-1;
    if(kind==FestivalKind.Memory&&elapsed<5){
     int preview=Mathf.Clamp(Mathf.FloorToInt(elapsed),0,4);
     active=elapsed-preview<.8f?polishSequence[preview]:-1;
     if(preview!=polishPreviewBeat){
      polishPreviewBeat=preview;
      sound.pitch=.85f+polishSequence[preview]*.14f;
      sound.PlayOneShot(note);
     }
    }
    // Memory recall must not reveal the next answer through a glowing pad.
    if(kind==FestivalKind.Memory&&elapsed>=5&&elapsed<polishFeedbackUntil&&!polishFeedbackWrong)active=polishLastDirection;
   }
   float pulse=.5f+.5f*Mathf.Sin(elapsed*6);
   for(int i=0;i<targets.Count;i++){
    bool highlighted=i==active;
    var scale=polishBaseScales[i];
    if(PolishIsRhythm)scale.y+=highlighted?.1f+.09f*pulse:0;
    else if(i>=progress)scale*=highlighted?1.07f+.05f*pulse:1;
    else if(kind==FestivalKind.Dig)scale*=.75f;
    targets[i].localScale=scale;
    for(int j=0;j<polishRenderers[i].Length;j++){
     var renderer=polishRenderers[i][j];if(renderer==null)continue;
     Color color=polishBaseColors[i][j];
     if(highlighted)color=Color.Lerp(color,new Color(1,.94f,.55f),.38f+.2f*pulse);
     else if(PolishIsRhythm)color*=.66f;
     polishProperties.Clear();polishProperties.SetColor(PolishColorId,color);
     renderer.SetPropertyBlock(polishProperties);
    }
   }
   if(polishBeacon!=null){
    bool visible=progress<targets.Count;polishBeacon.gameObject.SetActive(visible);
    if(visible){
     polishBeacon.position=targets[progress].position+Vector3.up*(kind==FestivalKind.Flight?2.35f:1.9f)+Vector3.up*(pulse*.25f);
     polishBeacon.rotation=Quaternion.Euler(45,elapsed*80,45);
    }
   }
  }

  void DrawPolishOverlay(){
   if(kind==FestivalKind.Coaster||(!Running&&!Won))return;
   if(polishTextStyle==null){
    polishTextStyle=new GUIStyle(GUI.skin.label){font=font,fontSize=16,wordWrap=true};
    polishTextStyle.normal.textColor=Color.white;
    polishArrowStyle=new GUIStyle(polishTextStyle){fontSize=27,alignment=TextAnchor.MiddleLeft};
   }
   float width=Mathf.Min(520,Screen.width-40);
   var panel=new Rect(20,382,width,111);
   GUI.Box(panel,"");
   if(!Running){
    string record="本轮 "+polishLastTime.ToString("0.0")+" 秒  ·  本次游玩最佳 "+polishBestTime.ToString("0.0")+" 秒";
    GUI.Label(new Rect(32,389,width-24,46),"完成！"+record,polishTextStyle);
    GUI.Label(new Rect(32,435,width-24,49),PolishIsRhythm?"最高连击 "+polishBestCombo+"  ·  再玩一轮，挑战新的方向组合。":"再来一次，试试更流畅地走完这条路线。",polishTextStyle);
    return;
   }
   int total=PolishIsRhythm?(kind==FestivalKind.Memory?5:8):targets.Count;
   string detail;
   if(PolishIsRhythm){
    detail=kind==FestivalKind.Memory&&elapsed<5?"先观察灯光 · "+Mathf.CeilToInt(5-elapsed)+" 秒后开始回忆":kind==FestivalKind.Memory?"按顺序回忆第 "+(progress+1)+" 步 · 答错不会清空进度":"当前连击 "+polishCombo+"  ·  本次游玩最高 "+polishBestCombo;
    GUI.Label(new Rect(32,388,width-24,44),detail,polishTextStyle);
   }else if(progress<targets.Count){
    Vector3 delta=targets[progress].position-game.Player.transform.position;
    Vector3 forward=game.Player.View.transform.forward;forward.y=0;
    if(forward.sqrMagnitude<.001f)forward=game.Player.transform.forward;
    forward.Normalize();
    Vector3 right=Vector3.Cross(Vector3.up,forward);
    int sector=((Mathf.RoundToInt(Mathf.Atan2(Vector3.Dot(delta,right),Vector3.Dot(delta,forward))*Mathf.Rad2Deg/45)%8)+8)%8;
    GUI.Label(new Rect(32,386,165,43),PolishCompass[sector],polishArrowStyle);
    detail="第 "+(progress+1)+" / "+total+" 个目标  ·  "+delta.magnitude.ToString("0.0")+" 米";
    if(kind==FestivalKind.Flight&&Mathf.Abs(delta.y)>1.5f)detail+=delta.y>0?"  ·  向上飞":"  ·  向下飞";
    GUI.Label(new Rect(196,391,width-188,40),detail,polishTextStyle);
   }
   string feedback=elapsed<polishFeedbackUntil?polishFeedback:kind==FestivalKind.Dig?"靠近目标按 G 挖掘；回活动牌旁按 G 结束。":kind==FestivalKind.Flight?"跟随金色路标穿过光环，放稳方向再转向。":kind==FestivalKind.Footprints?"跟随金色路标，看看脚印把你带向哪里。":kind==FestivalKind.Sailing?"循着金色路标，依次驶过每一个航标。":"每按一次方向键计一步，可按自己的速度完成。";
   Color previous=GUI.color;
   GUI.color=polishFeedbackWrong&&elapsed<polishFeedbackUntil?new Color(1,.79f,.53f):new Color(.76f,1,.88f);
   GUI.Label(new Rect(32,431,width-24,44),feedback,polishTextStyle);
   GUI.color=new Color(.15f,.2f,.25f);GUI.DrawTexture(new Rect(32,481,width-24,5),Texture2D.whiteTexture);
   GUI.color=new Color(.47f,.88f,.76f);GUI.DrawTexture(new Rect(32,481,(width-24)*Mathf.Clamp01(progress/(float)Mathf.Max(1,total)),5),Texture2D.whiteTexture);
   GUI.color=previous;
  }
 }
}
