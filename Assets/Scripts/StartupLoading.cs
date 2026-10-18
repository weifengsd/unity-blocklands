using System;using System.Collections;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    public bool GameReady {get;private set;}
    WorldLoadingScreen loadingScreen;
    IEnumerator Start()
    {
        var init=InitializeGame();
        while(true){bool next=false,failed=false;object current=null;try{next=init.MoveNext();if(next)current=init.Current;}catch(Exception e){failed=true;saveBlocked=true;loadingScreen.Fail("初始化遇到问题，请保留原存档后重试。");Debug.LogException(e);}if(failed||!next)yield break;yield return current;}
    }
    IEnumerator InitializeGame()
    {
        // Let the loading artwork reach the screen before building any terrain.
        yield return null;yield return null;
        Application.targetFrameRate=90;QualitySettings.vSyncCount=0;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=70;
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.38f,.43f,.46f);RenderSettings.ambientEquatorColor=new Color(.29f,.32f,.28f);RenderSettings.ambientGroundColor=new Color(.16f,.17f,.14f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.64f,.79f,.88f);RenderSettings.fogDensity=.013f;
        var sun=new GameObject("Afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.85f;sun.color=new Color(1,.95f,.86f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-34,0);
        loadingScreen.Report(.04f,"正在唤醒方块世界");yield return null;
        World=new GameObject("Voxel terrain").AddComponent<VoxelWorld>();var terrain=World.InitializeForLoading((p,label)=>loadingScreen.Report(.04f+p*.64f,label));while(terrain.MoveNext())yield return terrain.Current;
        loadingScreen.Report(.7f,"正在准备角色与探索工具");yield return null;
        Player=new GameObject("Player").AddComponent<FirstPerson>();Player.Initialize(this);Player.Teleport(World.Spawn,25);
        for(int i=1;i<=9;i++)Inventory[i]=i==7?64:32;
        var outline=new GameObject("Selected block");selection=outline.AddComponent<LineRenderer>();selection.sharedMaterial=new Material(Resources.Load<Shader>("Selection"));selection.widthMultiplier=.018f;selection.positionCount=16;selection.enabled=false;
        audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=.13f;digSound=MakeSound(130);placeSound=MakeSound(210);
        appIcon=Resources.Load<Texture2D>("BlocklandsIcon");
        loadingScreen.Report(.78f,"正在恢复你的世界、背包和伙伴");yield return null;
        if(System.IO.File.Exists(WorldSave.PathName))Load();else RefreshRegion();
        loadingScreen.Report(.93f,"正在检查生活设施与入口");yield return null;
        GrantLifestyleStarter();SetPaused(true);nextSave=30;
        loadingScreen.Complete();yield return new WaitForSecondsRealtime(.65f);loadingScreen.FadeOut();yield return new WaitForSecondsRealtime(.45f);
        Destroy(loadingScreen.gameObject);loadingScreen=null;GameReady=true;
    }
}}
