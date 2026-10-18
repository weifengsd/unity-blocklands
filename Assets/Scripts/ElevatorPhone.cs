using UnityEngine;

namespace Blocklands
{
    public enum LiftPhoneApp { Home, Calls, Stars, Guide }

    public partial class Sandbox
    {
        public bool PhoneOpen { get; private set; }
        public LiftPhoneApp PhoneApp { get; private set; }
        public const int PhoneStarGoal = 8;
        public const float PhoneStarDuration = 20;
        public int PhoneStarScore { get; private set; }
        public int PhoneStarBest { get; private set; }
        public float PhoneStarRemaining { get; private set; }
        public bool PhoneStarRunning { get; private set; }
        public bool PhoneStarWon => PhoneStarScore >= PhoneStarGoal;
        public float PhoneStarSpeed => .8f + PhoneStarScore * .06f;
        public float PhoneStarPosition => Mathf.PingPong(phoneStarAge * PhoneStarSpeed, 1);
        public int PhoneContact { get; private set; } = -1;
        public int PhoneCallLine { get; private set; }
        public bool PhoneCallConnected => PhoneContact >= 0 && phoneConnectRemaining <= 0;
        public string PhoneCallText => !string.IsNullOrEmpty(PhoneSubtitle)?PhoneSubtitle:PhoneContact < 0 ? "选一位朋友，聊聊路上的风景。" : !PhoneCallConnected ? "正在接通……" : PhoneDialogue[PhoneContact][PhoneCallLine];
        public bool PhoneInputCaptured => PhoneOpen || phoneConsumedFrame == Time.frameCount || (!IsSmoke && !Paused && Input.GetKeyDown(KeyCode.A));
        public bool InPhoneLift { get { RefreshPhoneLocation(); return phoneLiftKind != 0; } }
        public string PhoneLiftName => phoneLiftKind == 1 ? "村庄观景电梯" : phoneLiftKind == 2 ? "城堡水晶电梯" : phoneLiftKind == 3 ? "博物馆观景电梯" : World == null ? "随身手机" : VoxelWorld.RegionNames[World.Region] + " · 随身手机";

        GameObject phoneVillageRoot;
        SkyHouse[] phoneVillageLifts = new SkyHouse[0];
        SkyHouse phoneVillageLift;
        int phoneLiftKind, phoneConsumedFrame = -1, phoneGuidePage,phoneHomeSelection;
        Vector2 phoneCallScroll;
        float phoneStarAge, phoneConnectRemaining;
        string phoneStarMessage = "让星星进入绿色光带，再按 A 收集。";
        static readonly string[][] PhoneDialogue = {
            new[] { "小禾：喂！我在林地小屋，正想着你呢。你在哪里玩？", "小禾：如果想让我过去，说‘来我这里玩’，我会乘空中小汽车来找你。", "小禾：我快到时会告诉你，记得到当地入口接我呀。" },
            new[] { "城堡管家：您好。我在城堡值班，随时愿意帮您联系朋友。", "城堡管家：如果想知道小禾还有多久到，直接问‘小禾来了吗’。", "城堡管家：无论您在哪个地方，都可以用随身手机联系我们。" }
        };
        static readonly string[] PhoneGuideTitles = { "村庄 · 云上小铺", "城堡 · 水晶窗外", "博物馆 · 慢慢看" };
        static readonly string[] PhoneGuideText = {
            "玻璃轿厢旁是七层小卖部与顶层平台。按轿厢里的楼层按钮，看看不同高度的村庄。\n\n门打开后再出发，给其他乘客留出下梯的空间。",
            "城堡观景电梯连接十个楼层。透过水晶玻璃，可以看到金色柱廊、粉色尖塔与远处树冠。\n\n先选好楼层，再把这段旅程留给风景。",
            "博物馆共有二十层。每一次停靠，都可以成为一段小小的探索。\n\n保持距离欣赏珍藏，不要触碰展品。到站后按 Esc 收好手机，再继续参观。"
        };
        static readonly Color PhoneInk = new Color(.09f, .16f, .22f);
        static readonly Color PhoneMuted = new Color(.37f, .46f, .51f);
        static readonly Color PhoneMint = new Color(.64f, .88f, .78f);

        void RefreshPhoneLocation()
        {
            phoneLiftKind = 0; phoneVillageLift = null;
            if (World == null || Player == null) return;
            // Region-owned references exclude the old objects awaiting end-of-frame destruction.
            if (World.Region == 1 && villagePeople != null)
            {
                if (phoneVillageRoot != villagePeople)
                {
                    phoneVillageRoot = villagePeople;
                    phoneVillageLifts = villagePeople.GetComponentsInChildren<SkyHouse>();
                }
                foreach (var lift in phoneVillageLifts)
                    if (lift != null && lift.isActiveAndEnabled && lift.PlayerInside)
                    { phoneVillageLift = lift; phoneLiftKind = 1; return; }
            }
            if (World.Region != 6 || castle == null) return;
            if (castle.Lift != null && castle.Lift.isActiveAndEnabled && castle.Lift.Inside) phoneLiftKind = 2;
            else if (castle.MuseumElevator != null && castle.MuseumElevator.isActiveAndEnabled && castle.MuseumElevator.Inside) phoneLiftKind = 3;
        }

        public bool OpenPhone()
        {
            if (PhoneOpen) return true;
            if (Paused || Health <= 0 || VictoryOpen || inspecting || FestivalLocked || RoyalMenuOpen) return false;
            RefreshPhoneLocation();
            PhoneOpen = true; PhoneApp = LiftPhoneApp.Home; phoneGuidePage = Mathf.Max(0,phoneLiftKind - 1);
            phoneConsumedFrame = Time.frameCount;
            SampleH(false, false, false, Time.unscaledTime, false);
            Player.ResetJumpTap();
            if (selection != null) selection.enabled = false;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            Notice = "随身手机已打开。选小禾或城堡管家；A操作，Esc收起。";
            return true;
        }

        public void ClosePhone()
        {
            if (!PhoneOpen) return;
            ResetPhoneVoice();
            PhoneOpen = false; PhoneStarRunning = false; PhoneContact = -1;
            phoneConsumedFrame = Time.frameCount;
            SampleH(false, false, false, Time.unscaledTime, false);
            if (Player != null) Player.ResetJumpTap();
            bool unlocked = Paused || RoyalMenuOpen;
            Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = unlocked;
        }

        public void ResetPhone()
        {
            ResetPhoneVoice();
            ClosePhone();
            phoneVillageRoot = null; phoneVillageLifts = new SkyHouse[0]; phoneVillageLift = null; phoneLiftKind = 0;
            PhoneApp = LiftPhoneApp.Home; phoneHomeSelection=0;PhoneContact = -1; PhoneCallLine = 0; phoneConnectRemaining = 0;
            PhoneStarRunning = false; PhoneStarScore = 0; PhoneStarRemaining = 0; phoneStarAge = 0;
        }

        public bool HandlePhoneKey(KeyCode key)
        {
            // Closing on focus loss must never swallow the pause screen's resume key.
            if (Paused) { ClosePhone(); return false; }
            if (key == KeyCode.A) return PhoneActionA();
            if (!PhoneOpen) return false;
            if (key == KeyCode.Escape) { ClosePhone(); return true; }
            return true;
        }

        public bool PhoneActionA()
        {
            if(Paused)return false;if(!PhoneOpen)return OpenPhone();
            if(PhoneApp==LiftPhoneApp.Home){var app=(LiftPhoneApp)(phoneHomeSelection+1);phoneHomeSelection=(phoneHomeSelection+1)%3;SelectPhoneApp(app);if(app==LiftPhoneApp.Calls)BeginPhoneCall(phoneLiftKind==2||phoneLiftKind==3?1:0);else if(app==LiftPhoneApp.Stars)StartPhoneGame();return true;}
            if(PhoneApp==LiftPhoneApp.Calls){if(PhoneContact<0){SelectPhoneApp(LiftPhoneApp.Home);return true;}if(!PhoneCallConnected)return true;bool last=PhoneCallLine+1>=PhoneDialogue[PhoneContact].Length;AdvancePhoneCall();if(last)SelectPhoneApp(LiftPhoneApp.Home);return true;}
            if(PhoneApp==LiftPhoneApp.Stars){if(PhoneStarRunning)TapPhoneStar();else if(PhoneStarWon||PhoneStarRemaining<=0)SelectPhoneApp(LiftPhoneApp.Home);else StartPhoneGame();return true;}
            if(PhoneApp==LiftPhoneApp.Guide){phoneGuidePage++;if(phoneGuidePage>=PhoneGuideTitles.Length){phoneGuidePage=0;SelectPhoneApp(LiftPhoneApp.Home);EarnActivityCoins(18,"观景故事");}return true;}return true;
        }

        public bool HandlePhoneInput()
        {
            TickPhone();
            TickPhoneVoice();
            if (Paused) return false;
            if (!IsSmoke)
            {
                if (Input.GetKeyDown(KeyCode.A)&&!PhoneTyping) HandlePhoneKey(KeyCode.A);
                else if (PhoneOpen && Input.GetKeyDown(KeyCode.Escape)) HandlePhoneKey(KeyCode.Escape);
            }
            if (!PhoneInputCaptured) return false;
            // Keep environmental health effects live without forwarding R/B/F actions.
            hitFlash = Mathf.Max(0, hitFlash - Time.deltaTime);
            if (World.Get(Vector3Int.FloorToInt(Player.transform.position - Vector3.up * .15f)) == Block.Lava) Damage(.5f, "踩到岩浆");
            if (PhoneOpen && Time.time >= nextSave) { Save(); nextSave = Time.time + 30; }
            return true;
        }

        public void TickPhone() => TickPhone(Time.deltaTime);
        public void TickPhone(float delta)
        {
            if (!PhoneOpen) return;
            if (Paused || Health <= 0 || VictoryOpen) { ClosePhone(); return; }
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            phoneConnectRemaining = Mathf.Max(0, phoneConnectRemaining - Mathf.Max(0, delta));
            if (PhoneApp == LiftPhoneApp.Stars) TickPhoneGame(delta);
        }

        public void SelectPhoneApp(LiftPhoneApp app)
        {
            if (!PhoneOpen || app < LiftPhoneApp.Home || app > LiftPhoneApp.Guide) return;
            if(app!=LiftPhoneApp.Calls){StopPhoneVoice(true);PhoneSubtitle="";}
            PhoneApp = app;
            if (app != LiftPhoneApp.Calls) PhoneContact = -1;
            if (app != LiftPhoneApp.Stars) PhoneStarRunning = false;
        }

        public bool BeginPhoneCall(int contact)
        {
            if (!PhoneOpen || Paused || contact < 0 || contact >= PhoneDialogue.Length) return false;
            ResetPhoneVoice();
            phoneCallScroll=Vector2.zero;
            SelectPhoneApp(LiftPhoneApp.Calls); PhoneContact = contact; PhoneCallLine = 0; phoneConnectRemaining = .65f; return true;
        }
        public bool AdvancePhoneCall()
        {
            if (!PhoneOpen || Paused || !PhoneCallConnected) return false;
            if (PhoneCallLine + 1 >= PhoneDialogue[PhoneContact].Length) { PhoneContact = -1; EarnActivityCoins(12,"电话聊天"); return true; }
            PhoneCallLine++; return true;
        }

        public bool StartPhoneGame()
        {
            if (!PhoneOpen || Paused) return false;
            SelectPhoneApp(LiftPhoneApp.Stars); PhoneStarScore = 0; phoneStarAge = 0; PhoneStarRemaining = PhoneStarDuration; PhoneStarRunning = true;
            phoneStarMessage = "星星进入绿色光带时，按 A 或轻点按钮。"; return true;
        }
        public void TickPhoneGame(float delta)
        {
            if (!PhoneOpen || Paused || !PhoneStarRunning || PhoneApp != LiftPhoneApp.Stars) return;
            delta = Mathf.Clamp(delta, 0, PhoneStarDuration);
            phoneStarAge += delta; PhoneStarRemaining = Mathf.Max(0, PhoneStarRemaining - delta);
            if (PhoneStarRemaining <= 0) { PhoneStarRunning = false; phoneStarMessage = "时间到！休息一下，随时再来一局。"; }
        }
        public bool TapPhoneStar()
        {
            if (!PhoneOpen || Paused || !PhoneStarRunning || PhoneApp != LiftPhoneApp.Stars) return false;
            bool hit = PhoneStarPosition >= .34f && PhoneStarPosition <= .66f;
            if (!hit) { phoneStarMessage = "差一点！等星星进入绿色光带。"; return false; }
            PhoneStarScore++; PhoneStarBest = Mathf.Max(PhoneStarBest, PhoneStarScore); phoneStarAge = 0;
            phoneStarMessage = "收集成功！再接一颗小星星。";
            if (PhoneStarWon) { PhoneStarRunning = false;int earned=EarnActivityCoins(35,"口袋摘星");phoneStarMessage = "八颗星星收集齐了，获得"+earned+"元！"; }
            return true;
        }

        string PhoneLiftStatus()
        {
            if (phoneLiftKind == 1 && phoneVillageLift != null) return SkyHouse.FloorLabel(phoneVillageLift.Floor) + " · " + (phoneVillageLift.DoorOpen ? "已开门" : "门已关闭");
            if (phoneLiftKind == 2 && castle != null) return "第 " + (castle.Lift.Floor + 1) + " 层 · " + (castle.Lift.Moving ? "行进中" : castle.Lift.DoorsOpen ? "已开门" : "正在停靠");
            if (phoneLiftKind == 3 && castle != null) return "第 " + (castle.MuseumElevator.Floor + 1) + " 层 · " + (castle.MuseumElevator.DoorsOpen ? "已开门" : "行进中");
            return ForestFriendArriving || ForestFriendWaiting ? ForestFriendStatusText : "随时可以联系小禾和管家";
        }

        public void DrawPhoneHint(float w, float h)
        {
            if (Paused || PhoneOpen || RoyalMenuOpen) return;
            Panel(new Rect(w / 2 - 160, h - 179, 320, 32), new Color(.08f, .16f, .2f, .9f));
            Label(new Rect(w / 2 - 155, h - 174, 310, 25), "A · 随时拿手机 / 聊天 / 摘星", 15, PhoneMint, TextAnchor.MiddleCenter);
        }

        static void PhoneText(Rect rect, string text, int size, Color color, TextAnchor align = TextAnchor.UpperLeft)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, wordWrap = true, alignment = align, padding = new RectOffset(0, 0, 0, 0), clipping = TextClipping.Clip };
            style.normal.textColor = color; GUI.Label(rect, text, style);
        }
        static bool PhoneButton(Rect rect, string text, Color color)
        {
            Panel(rect, color);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, wordWrap = true, padding = new RectOffset(8, 8, 3, 3) };
            style.normal.textColor = PhoneInk; return GUI.Button(rect, text, style);
        }

        public void DrawPhone(float w, float h)
        {
            if (!PhoneOpen || Paused) return;
            float x = w - 398, y = (h - 578) / 2;
            Panel(new Rect(0, 0, w, h), new Color(.025f, .08f, .12f, .23f));
            Panel(new Rect(x + 7, y + 9, 374, 578), new Color(0, 0, 0, .27f));
            Panel(new Rect(x, y, 374, 578), new Color(.07f, .12f, .18f));
            Panel(new Rect(x + 9, y + 9, 356, 560), new Color(.95f, .96f, .92f));
            Panel(new Rect(x + 141, y + 16, 92, 6), new Color(.13f, .21f, .25f));
            PhoneText(new Rect(x + 26, y + 37, 320, 24), "幻方随身机                         旅途时光", 13, PhoneMuted);
            PhoneText(new Rect(x + 26, y + 70, 320, 39), PhoneApp == LiftPhoneApp.Home ? "把路上，变成好时光。" : PhoneApp == LiftPhoneApp.Calls ? "和朋友说说话" : PhoneApp == LiftPhoneApp.Stars ? "口袋摘星" : "一页风景", 24, PhoneInk);
            Panel(new Rect(x + 25, y + 116, 324, 56), PhoneMint);
            PhoneText(new Rect(x + 39, y + 124, 296, 22), PhoneLiftName, 16, PhoneInk);
            PhoneText(new Rect(x + 39, y + 146, 296, 20), PhoneLiftStatus() + (phoneLiftKind>0?"  ·  电梯照常运行":""), 13, PhoneMuted);
            float cx = x + 25, cy = y + 187;
            if (PhoneApp == LiftPhoneApp.Home)
            {
                if (PhoneButton(new Rect(cx, cy, 324, 78), "01  拨打电话\n请小禾来玩，或问管家她几秒后到", new Color(.83f, .89f, .97f))) SelectPhoneApp(LiftPhoneApp.Calls);
                if (PhoneButton(new Rect(cx, cy + 90, 324, 78), "02  口袋摘星\n20 秒的轻松小游戏", new Color(1, .9f, .65f))) SelectPhoneApp(LiftPhoneApp.Stars);
                if (PhoneButton(new Rect(cx, cy + 180, 324, 78), "03  一页风景\n看看下一站有什么", new Color(.87f, .83f, .95f))) SelectPhoneApp(LiftPhoneApp.Guide);
                PhoneText(new Rect(cx, cy + 271, 324, 40), "随时按 A 拿手机；点选联系人\n语音键说话，或输入文字发送。", 14, PhoneMuted, TextAnchor.MiddleCenter);
            }
            else if (PhoneApp == LiftPhoneApp.Calls) DrawPhoneCalls(cx, cy);
            else if (PhoneApp == LiftPhoneApp.Stars) DrawPhoneStars(cx, cy);
            else DrawPhoneGuide(cx, cy);
            if (PhoneButton(new Rect(x + 25, y + 523, 150, 31), PhoneApp == LiftPhoneApp.Home ? "手机主页" : "‹ 返回主页", new Color(.84f, .88f, .88f))) SelectPhoneApp(LiftPhoneApp.Home);
            if (PhoneButton(new Rect(x + 188, y + 523, 161, 31), "收起 · Esc", PhoneMint)) ClosePhone();
        }

        void DrawPhoneCalls(float x, float y)
        {
            if (PhoneContact < 0)
            {
                PhoneText(new Rect(x, y, 324, 43), "选一位朋友，分享眼前的风景。", 16, PhoneMuted);
                if (PhoneButton(new Rect(x, y + 52, 324, 72), "小禾\n林地小屋的老朋友", new Color(.84f, .91f, .82f))) BeginPhoneCall(0);
                if (PhoneButton(new Rect(x, y + 138, 324, 72), "城堡管家 · 门口守卫\n可问小禾到哪里了、还有几秒", new Color(.89f, .84f, .94f))) BeginPhoneCall(1);
                PhoneText(new Rect(x, y + 236, 324, 44), "游戏角色通话 · 文字聊天\n陪你等候，也陪你看风景。", 14, PhoneMuted, TextAnchor.MiddleCenter);
                return;
            }
            PhoneText(new Rect(x, y, 215, 33), (PhoneContact == 0 ? "小禾" : "城堡管家") + (PhoneCallConnected ? "  ·  通话中" : "  ·  呼叫中"), 20, PhoneInk);
            DrawPhoneFriendPortrait(new Rect(x+236,y,72,78));
            Panel(new Rect(x, y + 45, 324, 154), Color.white);
            var talkStyle=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,alignment=TextAnchor.UpperLeft,padding=new RectOffset(0,0,0,0)};
            talkStyle.normal.textColor=PhoneInk;
            float talkHeight=Mathf.Max(150,talkStyle.CalcHeight(new GUIContent(PhoneCallText),286)+24);
            phoneCallScroll=GUI.BeginScrollView(new Rect(x,y+45,324,154),phoneCallScroll,new Rect(0,0,308,talkHeight));
            GUI.Label(new Rect(14,12,286,talkHeight-16),PhoneCallText,talkStyle);
            GUI.EndScrollView();
            if (PhoneCallConnected && PhoneButton(new Rect(x, y + 207, 155, 38), VoiceListening?"● 结束语音":"语音键 · 开始", new Color(.85f,.88f,1))) StartPhoneVoice();
            if (PhoneCallConnected && PhoneButton(new Rect(x+168, y + 207, 156, 38), PhoneCallLine == 2 ? "说声再见" : "继续聊聊", PhoneMint)) AdvancePhoneCall();
            GUI.SetNextControlName("PhoneSpeechText");PhoneDraft=GUI.TextField(new Rect(x,y+254,236,30),PhoneDraft,240);PhoneTyping=GUI.GetNameOfFocusedControl()=="PhoneSpeechText";
            if(PhoneButton(new Rect(x+244,y+254,80,30),"发送",PhoneMint)){GUI.FocusControl("");PhoneTyping=false;SubmitPhoneSpeech(PhoneDraft);}
            PhoneText(new Rect(x,y+292,324,43),VoiceState,12,PhoneMuted);
        }
        void DrawPhoneFriendPortrait(Rect r){Panel(r,new Color(.84f,.79f,.94f));Panel(new Rect(r.x+17,r.y+8,38,45),new Color(.91f,.64f,.49f));Panel(new Rect(r.x+17,r.y+5,38,11),PhoneContact==0?new Color(.3f,.18f,.17f):new Color(.2f,.23f,.5f));Panel(new Rect(r.x+23,r.y+29,5,5),PhoneInk);Panel(new Rect(r.x+43,r.y+29,5,5),PhoneInk);Panel(new Rect(r.x+32,r.y+40,9,3),new Color(.68f,.2f,.32f));PhoneText(new Rect(r.x,r.y+54,r.width,19),PhoneContact==0?"小禾":"管家",11,PhoneInk,TextAnchor.MiddleCenter);}

        void DrawPhoneStars(float x, float y)
        {
            PhoneText(new Rect(x, y, 324, 29), "收集 " + PhoneStarScore + " / " + PhoneStarGoal + "    最佳 " + PhoneStarBest + "    " + Mathf.CeilToInt(PhoneStarRemaining) + " 秒", 18, PhoneInk);
            PhoneText(new Rect(x, y + 36, 324, 44), "星星划过绿色光带时，按 A 或点按钮。收集八颗就过关！", 15, PhoneMuted);
            Panel(new Rect(x, y + 103, 324, 65), new Color(.1f, .18f, .26f));
            Panel(new Rect(x + 24 + 276 * .34f, y + 108, 276 * .32f, 55), new Color(.24f, .55f, .44f));
            PhoneText(new Rect(x + 7 + 276 * PhoneStarPosition, y + 111, 34, 46), "★", 31, new Color(1, .86f, .38f), TextAnchor.MiddleCenter);
            PhoneText(new Rect(x, y + 181, 324, 55), phoneStarMessage, 16, PhoneStarWon ? new Color(.14f, .46f, .33f) : PhoneInk, TextAnchor.MiddleCenter);
            if (PhoneStarRunning) { if (PhoneButton(new Rect(x, y + 246, 324, 46), "摘星 · A", new Color(1, .86f, .46f))) TapPhoneStar(); }
            else if (PhoneButton(new Rect(x, y + 246, 324, 46), PhoneStarRemaining == 0 && PhoneStarScore == 0 ? "开始一局" : "再玩一次", new Color(1, .86f, .46f))) StartPhoneGame();
            PhoneText(new Rect(x, y + 300, 324, 20), "轻松玩一局，分数只留作旅途纪念。", 12, PhoneMuted, TextAnchor.MiddleCenter);
        }

        void DrawPhoneGuide(float x, float y)
        {
            PhoneText(new Rect(x, y, 324, 30), PhoneGuideTitles[phoneGuidePage], 21, PhoneInk);
            Panel(new Rect(x, y + 43, 324, 205), Color.white);
            PhoneText(new Rect(x + 16, y + 59, 292, 174), PhoneGuideText[phoneGuidePage], 18, PhoneInk);
            if (PhoneButton(new Rect(x, y + 266, 324, 42), "下一页  " + (phoneGuidePage + 1) + " / 3  ›", new Color(.87f, .83f, .95f))) phoneGuidePage = (phoneGuidePage + 1) % PhoneGuideTitles.Length;
        }
    }
}
