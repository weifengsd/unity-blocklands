using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands {public partial class SmokeTest
{
    IEnumerator LoadingScreenChecks(Sandbox g,string folder)
    {
        var rows=new List<string>();void Check(string key,bool value){rows.Add(key+"="+value);adventurePass&=value;File.WriteAllLines(Path.Combine(folder,"loading-screen-checks.txt"),rows);}
        var screen=WorldLoadingScreen.Active;Check("loading_screen_exists_before_world_is_ready",screen!=null&&!g.GameReady);Check("dedicated_key_art_loaded",screen!=null&&screen.HasKeyArt);Check("gameplay_is_paused_during_startup",g.Paused&&Time.timeScale==0);
        bool existing=File.Exists(WorldSave.PathName);g.Save();Check("save_ignored_before_world_ready",File.Exists(WorldSave.PathName)==existing);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"01-actual-startup.png"));yield return null;
        float end=Time.realtimeSinceStartup+180;while(!g.GameReady&&Time.realtimeSinceStartup<end)yield return null;
        yield return null;
        Check("startup_reaches_ready",g.GameReady);if(!g.GameReady)yield break;
        Check("player_and_world_exist",g.Player!=null&&g.World!=null);Check("real_chunks_constructed",g.World.transform.childCount>=36);Check("loader_releases_start_menu",WorldLoadingScreen.Active==null&&g.Paused);
        bool monotonic=true;float previous=0;foreach(float p in screen.ProgressHistory){monotonic&=p>=previous&&p>=0&&p<=1;previous=p;}Check("progress_monotonic_and_bounded",monotonic);Check("progress_ends_at_one",Mathf.Approximately(previous,1));Check("actual_chunk_completion_is_reported",screen.PhaseHistory.Exists(s=>s.Contains("36 / 36")));Check("save_restore_phase_is_reported",screen.PhaseHistory.Exists(s=>s.Contains("恢复你的世界")));
        int inventory=g.Inventory[(int)Block.Apple];g.Inventory[(int)Block.Apple]=27;g.Save();Check("ready_world_can_be_saved",File.Exists(WorldSave.PathName));g.Inventory[(int)Block.Apple]=0;g.Load();Check("normal_save_load_still_works",g.Inventory[(int)Block.Apple]==27);g.Inventory[(int)Block.Apple]=inventory;
        var preview=WorldLoadingScreen.Create();preview.Report(.64f,"正在构筑方块地形  32 / 36"); // Only the two layout screenshots use a fixed visual fixture.
        Screen.SetResolution(1440,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.8f);rows.Add("large_layout_actual_pixels="+Screen.width+"x"+Screen.height);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"02-layout-1440x900.png"));yield return new WaitForSecondsRealtime(.2f);
        Check("normal_progress_track_fits_canvas",WorldLoadingScreen.ProgressTrack(1280,800).xMax<=1280&&WorldLoadingScreen.TitleArea(1280).xMax<=1280);
        Screen.SetResolution(960,640,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.8f);rows.Add("small_layout_actual_pixels="+Screen.width+"x"+Screen.height);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"03-layout-960x640.png"));yield return new WaitForSecondsRealtime(.2f);
        Check("small_progress_track_stays_visible",WorldLoadingScreen.ProgressTrack(1280,640/.75f).yMax<640/.75f);
        preview.Fail("测试错误提示：加载已停止。");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"04-error-state.png"));yield return new WaitForSecondsRealtime(.2f);Check("failure_state_retains_ui_without_false_completion",preview.Failed&&!preview.Ready&&Mathf.Approximately(preview.Progress,.64f));Destroy(preview.gameObject);yield return null;
        g.SetPaused(false);Check("player_can_resume_after_loading",!g.Paused&&g.Player.Body.enabled);g.SetPaused(true);
    }
}}
