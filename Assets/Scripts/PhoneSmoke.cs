using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator PhoneChecks(Sandbox g, string folder)
        {
            var checks = new List<string>();
            void Check(string name, bool value) { checks.Add(name + "=" + value); adventurePass &= value; File.WriteAllLines(Path.Combine(folder, "phone-checks.txt"), checks); }
            g.Respawn(); g.TravelTo(1); yield return null;
            SkyHouse village = null;
            foreach (var lift in FindObjectsByType<SkyHouse>()) if (Mathf.Abs(lift.Anchor.z - 13) < .1f && lift.Anchor.x < 30) village = lift;
            Check("village_lift_available", village != null);
            if (village == null) yield break;
            g.Player.Teleport(new Vector3(30, 15, 5));
            Check("phone_opens_outside_cabin", g.OpenPhone() && g.PhoneOpen && !g.InPhoneLift);
            g.ClosePhone();
            g.Player.Teleport(village.CarCenter);
            Check("a_opens_without_pause", g.HandlePhoneKey(KeyCode.A) && g.PhoneOpen && !g.Paused && Mathf.Approximately(Time.timeScale, 1));
            Check("phone_cursor_unlocked", Cursor.visible && Cursor.lockState == CursorLockMode.None);
            int[] inventory = (int[])g.Inventory.Clone();
            village.RequestFloor(6); float startY = g.Player.transform.position.y;
            Check("a_starts_context_call", g.PhoneActionA() && g.PhoneContact == 0 && !g.PhoneCallConnected);
            g.TickPhone(.7f);
            Check("call_connects_locally", g.PhoneCallConnected && g.PhoneCallText.Contains("小禾"));
            Check("a_advances_call", g.PhoneActionA() && g.PhoneCallLine == 1);
            yield return null; ScreenCapture.CaptureScreenshot(Path.Combine(folder, "phone-01-call.png"));
            g.PhoneActionA(); g.PhoneActionA();
            Check("a_ends_call_and_returns_home", g.PhoneContact == -1&&g.PhoneApp==LiftPhoneApp.Home);
            Check("a_starts_star_game",g.PhoneActionA()&&g.PhoneApp==LiftPhoneApp.Stars&&g.PhoneStarRunning);
            Check("off_target_a_does_not_score", g.PhoneActionA() && g.PhoneStarScore == 0);
            bool hits = true;
            for (int i = 0; i < Sandbox.PhoneStarGoal; i++) { g.TickPhoneGame(.5f / g.PhoneStarSpeed); int before=g.PhoneStarScore;hits &= g.PhoneActionA()&&g.PhoneStarScore==before+1; }
            Check("game_scores_and_clears", hits && g.PhoneStarWon && !g.PhoneStarRunning && g.PhoneStarScore == Sandbox.PhoneStarGoal);
            Check("finished_game_cannot_farm_score", !g.TapPhoneStar() && g.PhoneStarScore == Sandbox.PhoneStarGoal);
            yield return null; ScreenCapture.CaptureScreenshot(Path.Combine(folder, "phone-02-stars-clear.png"));
            Check("a_returns_home_after_clear",g.PhoneActionA()&&g.PhoneApp==LiftPhoneApp.Home);Check("a_opens_guide_next",g.PhoneActionA()&&g.PhoneApp==LiftPhoneApp.Guide);g.PhoneActionA();g.PhoneActionA();Check("a_finishes_guide_cycle",g.PhoneActionA()&&g.PhoneApp==LiftPhoneApp.Home);g.SelectPhoneApp(LiftPhoneApp.Stars);Check("game_retry_resets_round", g.StartPhoneGame() && g.PhoneStarScore == 0 && g.PhoneStarRunning);
            g.TickPhoneGame(Sandbox.PhoneStarDuration);
            Check("game_timeout", !g.PhoneStarRunning && g.PhoneStarRemaining == 0 && !g.PhoneStarWon);
            g.SelectPhoneApp(LiftPhoneApp.Guide);
            yield return new WaitForSecondsRealtime(2);
            Check("village_lift_carries_while_phone_open", g.PhoneOpen && village.PlayerInside && g.Player.transform.position.y > startY + 3 && !g.Paused);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "phone-03-guide-moving.png"));
            float until = Time.unscaledTime + 18;
            while (Time.unscaledTime < until && (village.Floor != 6 || !village.DoorOpen)) yield return null;
            Check("village_arrives_with_phone_open", g.PhoneOpen && village.PlayerInside && village.Floor == 6 && village.DoorOpen);
            bool unchanged = true; for (int i = 0; i < inventory.Length; i++) unchanged &= i==(int)Block.Coin ? inventory[i]+65==g.Inventory[i] : inventory[i] == g.Inventory[i];
            Check("phone_apps_earn_12_35_18_without_spending_items", unchanged);
            bool escaped = g.HandlePhoneKey(KeyCode.Escape) && !g.PhoneOpen && !g.Paused;
            yield return null; // FirstPerson applies its cursor state on the following frame.
            Check("escape_closes_without_pausing", escaped);
            if(!Application.isBatchMode)Check("escape_relocks_cursor", !Cursor.visible && Cursor.lockState == CursorLockMode.Locked);
            g.OpenPhone(); g.SetPaused(true); yield return null;
            Check("pause_releases_phone_and_resume_input", !g.PhoneOpen && !g.HandlePhoneInput() && !g.HandlePhoneKey(KeyCode.Return) && g.Paused);
            g.SetPaused(false); yield return null;
            Check("pause_can_resume", !g.Paused && Mathf.Approximately(Time.timeScale, 1) && !g.PhoneOpen);
            g.OpenPhone(); g.Player.Teleport(new Vector3(30, 15, 5)); g.TickPhone(0);
            Check("leaving_cabin_keeps_phone_open", g.PhoneOpen && !g.InPhoneLift);
            g.ClosePhone();

            g.RoyalIdentity = true; g.TravelTo(6); yield return null;
            var castle = FindAnyObjectByType<CastleSystem>();
            Check("castle_lifts_available", castle != null && castle.Lift != null && castle.MuseumElevator != null);
            if (castle == null || castle.Lift == null || castle.MuseumElevator == null) yield break;
            var royal = castle.Lift;
            g.Player.Teleport(royal.Position + Vector3.up * .2f); royal.Request(9);
            startY = g.Player.transform.position.y;
            Check("royal_phone_opens", g.OpenPhone() && g.PhoneLiftName.Contains("城堡"));
            yield return new WaitForSecondsRealtime(2);
            Check("royal_lift_moves_with_phone", g.PhoneOpen && royal.Inside && g.Player.transform.position.y > startY + 3);
            until = Time.unscaledTime + 13; while (Time.unscaledTime < until && (royal.Floor != 9 || !royal.DoorsOpen)) yield return null;
            Check("royal_arrives_with_phone_open", royal.Floor == 9 && royal.Inside && g.PhoneOpen);
            g.SelectPhoneApp(LiftPhoneApp.Home); yield return null; ScreenCapture.CaptureScreenshot(Path.Combine(folder, "phone-04-royal-home.png"));
            g.ClosePhone();
            var museum = castle.MuseumElevator;
            g.Player.Teleport(new Vector3(10, 16.2f, 29)); museum.Choice = 19; museum.Confirm();
            startY = g.Player.transform.position.y;
            Check("museum_phone_opens", g.OpenPhone() && g.PhoneLiftName.Contains("博物馆"));
            yield return new WaitForSecondsRealtime(2);
            Check("museum_lift_moves_with_phone", g.PhoneOpen && museum.Inside && g.Player.transform.position.y > startY + 3);
            until = Time.unscaledTime + 13; while (Time.unscaledTime < until && (museum.Floor != 19 || !museum.DoorsOpen)) yield return null;
            Check("museum_arrives_with_phone_open", museum.Floor == 19 && museum.Inside && g.PhoneOpen);
            Check("escape_closes_museum_phone", g.HandlePhoneKey(KeyCode.Escape) && !g.PhoneOpen && !g.Paused);
            g.OpenPhone(); g.TravelTo(0); yield return null;
            Check("travel_resets_phone", !g.PhoneOpen && !g.InPhoneLift && !g.Paused);
            g.Save();
        }
    }
}
