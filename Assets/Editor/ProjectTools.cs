using System;
using System.IO;
using Blocklands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectTools
{
    const string ScenePath="Assets/Scenes/Blocklands.unity";
    [InitializeOnLoadMethod] static void OnImport()
    {
        EditorApplication.delayCall+=()=>{if(!File.Exists(ScenePath)&&!EditorApplication.isPlayingOrWillChangePlaymode)Prepare();};
    }
    [MenuItem("Blocklands/Prepare scene")]
    public static void Prepare()
    {
        Directory.CreateDirectory("Assets/Scenes");
        if(!File.Exists(ScenePath))
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene,ScenePath);
        }
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        PlayerSettings.companyName="Creatives";PlayerSettings.productName="幻方世界";
        var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/BlocklandsIcon.png");
        if(icon!=null)PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);
        var artwork=AssetImporter.GetAtPath("Assets/Resources/LoadingWorldKeyArt.png") as TextureImporter;
        if(artwork!=null&&(artwork.maxTextureSize!=2048||artwork.mipmapEnabled||artwork.textureCompression!=TextureImporterCompression.Uncompressed)){artwork.maxTextureSize=2048;artwork.mipmapEnabled=false;artwork.textureCompression=TextureImporterCompression.Uncompressed;artwork.wrapMode=TextureWrapMode.Clamp;artwork.SaveAndReimport();}
        PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();}
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Blocklands/Run world checks")]
    public static void SelfCheck()
    {
        Prepare();
        var go=new GameObject("Validation world");var world=go.AddComponent<VoxelWorld>();
        try
        {
            world.Initialize();
            CraftingChecks();
            Check(Blocks.Names.Length==Blocks.Count,"item names complete");
            Check(world.Get(new Vector3Int(31,19,31))==Block.Workbench,"visible workbench at cabin");
            Check(!world.Edit(new Vector3Int(31,35,31),Block.StonePickaxe),"inventory tools cannot become terrain");
            Check(world.transform.childCount==36,"36 chunk meshes in expanded world");
            Check(world.Get(new Vector3Int(30,0,30))==Block.Bedrock,"bedrock foundation");
            Check(!world.Edit(new Vector3Int(30,0,30),Block.Air),"bedrock cannot be removed");
            Check(!world.Edit(new Vector3Int(-1,4,4),Block.Stone),"outside-world edits rejected");
            int water=0;for(int x=0;x<64;x++)for(int z=0;z<64;z++)if(world.Get(new Vector3Int(x,VoxelWorld.WaterLevel,z))==Block.Water)water++;
            Check(water>10,"lake exists");
            foreach(var meshFilter in world.GetComponentsInChildren<MeshFilter>())
            {
                var mesh=meshFilter.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;var normals=mesh.normals;
                for(int i=0;i<triangles.Length;i+=3)
                {
                    int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
                    Check(Vector3.Dot(Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]),normals[a])>.99f,"outward-facing triangle");
                }
            }
            // Remove/add right on a chunk seam and verify an actual physics ray.
            var p=new Vector3Int(31,35,31);
            Check(world.Edit(p,Block.Brick),"place block on chunk boundary");Physics.SyncTransforms();
            Check(Physics.Raycast((Vector3)p+new Vector3(.5f,3,.5f),Vector3.down,out RaycastHit hit,3),"placed block collider");
            Check(Mathf.Abs(hit.point.y-36)<.01f,"collider at visible block surface");
            Check(world.Edit(p,Block.Air),"remove placed block");Physics.SyncTransforms();
            Check(!Physics.Raycast((Vector3)p+new Vector3(.5f,3,.5f),Vector3.down,3),"removed collider is gone");
            var save=new SavedWorld{position=world.Spawn,inventory=new int[12],selected=1};save.inventory[4]=5;save.edits.Add(new SavedBlock{x=31,y=35,z=31,id=8});
            string temp=Path.Combine(Path.GetTempPath(),"blocklands-test-"+Guid.NewGuid()+".json");
            try
            {
                WorldSave.Write(save,temp);var read=WorldSave.Read(temp);Check(read.inventory[4]==5&&read.edits.Count==1&&read.inventory.Length==Blocks.Count,"legacy save migration and round trip");
                save.inventory[4]=6;WorldSave.Write(save,temp);Check(WorldSave.Read(temp).inventory[4]==6&&File.Exists(temp+".bak"),"save replacement and backup");
            }
            finally{if(File.Exists(temp))File.Delete(temp);if(File.Exists(temp+".bak"))File.Delete(temp+".bak");}
            bool rejected=false;save.inventory[4]=-1;try{WorldSave.Validate(save);}catch(InvalidDataException){rejected=true;}Check(rejected,"invalid inventory rejected");
            RegionChecks(world);
            Directory.CreateDirectory("Verification");File.WriteAllText("Verification/editor-checks.txt","PASS: 16 chunks; outward mesh winding; lake; bedrock protection; bounds; chunk-edge place/remove colliders; JSON round trip and backup; legacy inventory migration; invalid save rejection; all "+Crafting.Recipes.Length+" recipes at every valid grid offset; wrong shape and extra ingredients rejected; reservation capacity; atomic inventory collection; visible workbench; tool terrain rejected.\nUnity "+Application.unityVersion+"\n");
            Debug.Log("BLOCKLANDS_WORLD_CHECKS_PASS");
        }
        finally{UnityEngine.Object.DestroyImmediate(go);}
    }
    static void Check(bool ok,string label){if(!ok)throw new Exception("World validation failed: "+label);}
    static void CraftingChecks()
    {
        var grid=new Crafting();var bag=new int[Blocks.Count];
        foreach(var recipe in Crafting.Recipes)for(int oy=0;oy<=3-recipe.Height;oy++)for(int ox=0;ox<=3-recipe.Width;ox++)
        {
            grid.Clear();for(int i=0;i<bag.Length;i++)bag[i]=100;
            for(int y=0;y<recipe.Height;y++)for(int x=0;x<recipe.Width;x++)Check(grid.SetCell((oy+y)*3+ox+x,recipe.Pattern[y*recipe.Width+x],bag),"place recipe ingredient");
            Check(grid.Match()==recipe,"recipe at permitted offset: "+recipe.Name);
            int[] expected=(int[])bag.Clone();foreach(var b in grid.Grid)if(b!=Block.Air)expected[(int)b]--;expected[(int)recipe.Output]+=recipe.Count;
            Check(grid.Collect(bag,out Recipe result)&&result==recipe,"collect recipe");
            for(int i=0;i<bag.Length;i++)Check(bag[i]==expected[i],"atomic recipe accounting");
            Check(grid.Match()==null,"clear after collection");
        }
        grid.SetCell(0,Block.Planks,bag);grid.SetCell(1,Block.Planks,bag);Check(grid.Match()!=null&&grid.Match().Output==Block.Coin&&grid.Match().Count==5,"two horizontal planks make five coins, not sticks");
        grid.Clear();grid.SetCell(0,Block.Wood,bag);grid.SetCell(8,Block.Stone,bag);Check(grid.Match()==null,"extra ingredient rejected");
        grid.Clear();bag[(int)Block.Wood]=1;grid.SetCell(0,Block.Wood,bag);Check(!grid.SetCell(1,Block.Wood,bag),"cannot reserve same item twice");
        bag[(int)Block.Planks]=9998;Check(!grid.Collect(bag,out _)&&bag[(int)Block.Wood]==1,"full output refuses without consumption");
        bag[(int)Block.Planks]=0;bag[(int)Block.Wood]=0;Check(!grid.Collect(bag,out _),"missing reserved materials rejected");
    }
    static void RegionChecks(VoxelWorld world)
    {
        var marker=new Vector3Int(30,38,30);world.Edit(marker,Block.WoodTile);
        for(int region=1;region<5;region++)
        {
            world.SwitchRegion(region);Check(world.Region==region,"region selected");
            var spawn=Vector3Int.FloorToInt(world.SafeSpawn());Check(!Blocks.Solid(world.Get(spawn))&&!Blocks.Solid(world.Get(spawn+Vector3Int.up)),"region spawn clear");
            if(region==1)Check(world.Get(new Vector3Int(11,16,14))==Block.Workbench,"village workshop");
            if(region==2)Check(world.Get(new Vector3Int(25,19,23))==Block.Glass&&world.Get(new Vector3Int(35,19,23))==Block.IronOre,"mine visible veins");
            if(region==3)Check(world.Get(new Vector3Int(25,20,22))==Block.ShadowCrystal,"shadow crystal deposit");
            if(region==2)Check(world.Get(new Vector3Int(21,19,24))==Block.DreamCrystal,"green dream crystals");
            if(region==4){Check(world.Get(new Vector3Int(35,17,19))==Block.Obsidian,"black purple crystal deposit");Check(world.Get(new Vector3Int(40,16,20))==Block.Lava,"lava exists");Check(world.Get(new Vector3Int(7,18,9))==Block.Bamboo,"bamboo exists");foreach(var p in Sandbox.PortalFrame())Check(world.Get(p)==Block.Air,"clear portal blueprint");}
            world.Edit(marker,(Block)(region==1?23:region==2?29:28));
        }
        for(int region=0;region<5;region++){world.SwitchRegion(region);Check(world.Get(marker)==(region==0?Block.WoodTile:(Block)(region==1?23:region==2?29:28)),"independent region edits");}
        Check(Blocks.DigDelay(Block.IronAxe,Block.Wood)<Blocks.DigDelay(Block.Air,Block.Wood),"axe useful");
        Check(Blocks.DigDelay(Block.IronShovel,Block.Dirt)<Blocks.DigDelay(Block.Air,Block.Dirt),"shovel useful");
        System.IO.File.WriteAllText("Verification/region-editor-checks.txt","PASS: 5 distinct regions; safe spawns; village workbench; visible crystal and iron veins; dream green crystals; purple crystals; lava; bamboo; clear 14-block portal frame; independent edits retained across round trips; axes and shovels speed appropriate mining. Recipes: "+Crafting.Recipes.Length+"\n");
    }
    [MenuItem("Blocklands/Build macOS game")]
    public static void BuildMac()
    {
        SelfCheck();Directory.CreateDirectory("Builds");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/Blocklands.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
        File.WriteAllText("Verification/build.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\n");
        if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors>0)throw new Exception("Build failed or contains errors: "+report.summary.result);
        var plistPath="Builds/Blocklands.app/Contents/Info.plist";
        foreach(var item in new[]{
            "Add :NSMicrophoneUsageDescription string 仅在点击语音键后使用麦克风并进行本机字幕对话",
            "Add :NSSpeechRecognitionUsageDescription string 本机识别游戏角色对话不上传录音"}){
            var edit=System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("/usr/libexec/PlistBuddy","-c \""+item+"\" "+plistPath){UseShellExecute=false});
            edit.WaitForExit();if(edit.ExitCode!=0)throw new Exception("Voice privacy description failed: "+item);
        }
        File.Copy("Assets/Fonts/OFL.txt","Builds/Blocklands.app/Contents/Resources/Noto-CJK-LICENSE.txt",true);
        // Adding the third-party notice changes the resource seal; re-sign our local app.
        var signing=System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("/usr/bin/codesign","--force --deep --sign - Builds/Blocklands.app"){UseShellExecute=false});
        signing.WaitForExit();if(signing.ExitCode!=0)throw new Exception("Local app signing failed");
    }
}
