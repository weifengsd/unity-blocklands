using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    // Only runs with an explicit CLI switch; it uses an isolated save folder.
    public partial class SmokeTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-blocklands-smoke")>=0){Application.runInBackground=true;QualitySettings.vSyncCount=0;new GameObject("Smoke test").AddComponent<SmokeTest>();}}
        IEnumerator Start()
        {
            Application.runInBackground=true;
            var args=Environment.GetCommandLineArgs();string folder=args[Array.IndexOf(args,"-blocklands-smoke")+1];Directory.CreateDirectory(folder);
            yield return null;var game=FindAnyObjectByType<Sandbox>();
            if(Array.IndexOf(args,"-loading-screen-only")>=0){yield return LoadingScreenChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            float deadline=Time.realtimeSinceStartup+180;while(!game.GameReady&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!game.GameReady){File.WriteAllText(Path.Combine(folder,"startup-failed.txt"),"Game did not finish initialization.");Application.Quit(1);yield break;}
            yield return new WaitForSecondsRealtime(2);
            if(Array.IndexOf(args,"-care-flight-glass-only")>=0){game.SetPaused(false);yield return CareFlightGlassChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-phase-combat-only")>=0){game.SetPaused(false);yield return PhaseCombatChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-gear-vehicle-only")>=0){game.SetPaused(false);yield return GearVehicleChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-drone-entry-only")>=0){game.SetPaused(false);yield return DroneEntryChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-crafting-ui-only")>=0){game.SetPaused(false);yield return CraftingUiChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-crystal-material-only")>=0){game.SetPaused(false);yield return CrystalMaterialChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-party-bag-only")>=0){game.SetPaused(false);yield return PartyBagChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-home-party-only")>=0){game.SetPaused(false);yield return HomePartyChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-deep-base-only")>=0){game.SetPaused(false);yield return DeepBaseChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-base-camera-only")>=0){game.SetPaused(false);yield return BaseCameraChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-high-tsunami-only")>=0){game.SetPaused(false);yield return HighTsunamiChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-secret-room-only")>=0){game.SetPaused(false);yield return SecretRoomChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-invitation-items-only")>=0){game.SetPaused(false);yield return InvitationItemChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-lifestyle-only")>=0){game.SetPaused(false);yield return LifestyleChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-companion-returns-only")>=0){game.SetPaused(false);yield return CompanionReturnChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-sticker-party-only")>=0){game.SetPaused(false);yield return StickerPartyChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-neighbourhood-only")>=0){game.SetPaused(false);yield return NeighbourhoodChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-castle-replay-only")>=0){game.SetPaused(false);yield return CastleReplayChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-realm-life-only")>=0){game.SetPaused(false);yield return RealmLifeChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-excursions-only")>=0){game.SetPaused(false);yield return ExcursionChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-village-pit-only")>=0){game.SetPaused(false);yield return VillagePitChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-excursion-sides-only")>=0){game.SetPaused(false);yield return ExcursionSideChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-friends-rewards-only")>=0){game.SetPaused(false);yield return FriendsAndRewardsChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-entertainment-g-only")>=0){game.SetPaused(false);yield return EntertainmentGChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-palace-life-only")>=0){game.SetPaused(false);yield return PalaceLifeChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-activity-regression-only")>=0){game.SetPaused(false);yield return ActivityRegressionChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-phone-only")>=0){game.SetPaused(false);yield return PhoneChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-encounters-only")>=0){game.SetPaused(false);yield return EncountersChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-depth-spawn-only")>=0){bool pass=true;var rows=new System.Collections.Generic.List<string>();foreach(float y in new[]{-40f,-71f,-102f,-174f}){var p=Vector3Int.FloorToInt(game.FindOpenDepth(new Vector3(31.5f,y,30.5f)));bool open=!Blocks.Solid(game.World.Get(p))&&!Blocks.Solid(game.World.Get(p+Vector3Int.up));rows.Add("depth_"+y+"_spawn_clear="+open);pass&=open;}File.WriteAllLines(Path.Combine(folder,"depth-spawn-checks.txt"),rows);Application.Quit(pass?0:1);yield break;}
            if(Array.IndexOf(args,"-coaster-smooth-only")>=0){game.SetPaused(false);yield return CoasterSmoothChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-festival-only")>=0){game.SetPaused(false);yield return FestivalChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-connected-only")>=0){game.SetPaused(false);yield return ConnectedChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-h-only")>=0){game.SetPaused(false);yield return HKeyChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-media-only")>=0){game.SetPaused(false);yield return MediaChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-sun-only")>=0){game.SetPaused(false);yield return SunVisitChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-activities-only")>=0){game.SetPaused(false);yield return ActivitiesChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-continuity-only")>=0){game.SetPaused(false);yield return ContinuityChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-princess-only")>=0){game.SetPaused(false);yield return PrincessChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-royal-only")>=0){game.SetPaused(false);yield return RoyalChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-castle-only")>=0){game.SetPaused(false);yield return CastleChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-home-alarm-only")>=0){game.SetPaused(false);yield return HomeAlarmChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-home-only")>=0){game.SetPaused(false);yield return HomeChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-lift-fifth-only")>=0){game.SetPaused(false);yield return FifthDestinationChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-lift-only")>=0){game.SetPaused(false);yield return LiftChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-market-only")>=0){game.SetPaused(false);yield return MarketChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-life-only")>=0){game.SetPaused(false);yield return VillageLifeChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            if(Array.IndexOf(args,"-village-only")>=0){game.SetPaused(false);yield return VillageRenewalChecks(game,folder);Application.Quit(adventurePass?0:1);yield break;}
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"01-welcome.png"));yield return new WaitForSecondsRealtime(1);
            game.SetPaused(false);yield return new WaitForSecondsRealtime(2);
            bool ground=game.Player.Body.isGrounded;
            Vector3 original=game.Player.transform.position;
            for(int i=0;i<60;i++){game.Player.Body.Move(Vector3.right*.025f);yield return null;}
            bool moved=game.Player.transform.position.x>original.x+1;
            var cell=new Vector3Int(31,21,32);game.World.Edit(cell,Block.Air);
            int before=game.Inventory[(int)Block.Planks];bool placed=game.TryPlace(cell,Block.Planks)&&game.Inventory[(int)Block.Planks]==before-1;
            bool overlapRejected=!game.TryPlace(Vector3Int.FloorToInt(game.Player.transform.position+Vector3.up),Block.Stone);
            int wood=game.Inventory[(int)Block.Wood];game.Crafting.SetCell(0,Block.Wood,game.Inventory);
            bool crafted=game.CollectCraft()&&game.Inventory[(int)Block.Wood]==wood-1;
            game.Save();game.World.Edit(cell,Block.Air);game.Load();bool restored=game.World.Get(cell)==Block.Planks;
            game.Player.Teleport(original,32,12);yield return new WaitForSecondsRealtime(1);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"02-world.png"));yield return new WaitForSecondsRealtime(1);
            // Lake view and smaller-window layout are separate screenshot checks.
            game.Player.Teleport(new Vector3(18,23,33),0,24);game.Player.transform.rotation=Quaternion.Euler(0,180,0);
            game.SetPaused(true);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"03-pause.png"));yield return new WaitForSecondsRealtime(1);
            game.SetPaused(false);game.Player.Teleport(new Vector3(17.5f,12,18.5f),0,0);yield return new WaitForSecondsRealtime(1);
            bool swimming=game.Player.Swimming;
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"04-water.png"));yield return new WaitForSecondsRealtime(1);
            game.Player.Teleport(new Vector3(28,35,28),35,27);
            bool single=!game.Player.JumpTap(100)&&!game.Player.Flying;
            bool doubleTap=game.Player.JumpTap(100.2f)&&game.Player.Flying;
            Vector3 startFlight=game.Player.transform.position;game.Player.FlyStep(Vector2.up,1,false,.5f);
            bool flightMove=game.Player.transform.position.y>startFlight.y+2.9f&&Vector3.Distance(game.Player.transform.position,startFlight)>4;
            Vector3 hover=game.Player.transform.position;yield return new WaitForSecondsRealtime(.5f);
            bool flightHover=Vector3.Distance(hover,game.Player.transform.position)<.01f;
            game.Player.FlyStep(Vector2.zero,-1,false,.25f);bool descent=game.Player.transform.position.y<hover.y-1.4f;
            game.SetPaused(true);Vector3 paused=game.Player.transform.position;game.Player.FlyStep(Vector2.up,1,true,1);
            bool pauseHold=game.Player.transform.position==paused;game.SetPaused(false);
            game.Save();Vector3 flyingSaved=game.Player.transform.position;game.Player.Teleport(original);game.Load();
            bool flightSave=game.Player.Flying&&Vector3.Distance(flyingSaved,game.Player.transform.position)<.01f;
            game.Player.Teleport(new Vector3(31.5f,32,31.5f));game.Player.SetFlying(true);game.World.Edit(new Vector3Int(31,35,31),Block.Stone);Physics.SyncTransforms();
            game.Player.FlyStep(Vector2.zero,1,false,1);bool roofCollision=game.Player.transform.position.y<33.25f;
            game.World.Edit(new Vector3Int(31,35,31),Block.Air);
            game.Player.Teleport(new Vector3(28,FirstPerson.FlightCeiling-1,28),35,40);game.Player.SetFlying(true);game.Player.FlyStep(Vector2.zero,1,false,1);
            bool ceiling=game.Player.transform.position.y<=FirstPerson.FlightCeiling+.01f;
            game.Player.Teleport(new Vector3(27,32,24),35,30);game.Player.SetFlying(true);
            yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"05-flight.png"));yield return new WaitForSecondsRealtime(1);
            game.OpenCrafting();game.SelectItem((int)Block.Planks);int planks=game.Inventory[(int)Block.Planks];
            foreach(int i in new[]{0,1,3,4})game.Crafting.SetCell(i,Block.Planks,game.Inventory);
            bool reserved=game.Inventory[(int)Block.Planks]==planks&&game.Crafting.Available(Block.Planks,game.Inventory)==planks-4;
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"06-crafting.png"));yield return new WaitForSecondsRealtime(1);
            bool bench=game.CollectCraft()&&game.Inventory[(int)Block.Workbench]>0;
            game.Crafting.SetCell(8,Block.Wood,game.Inventory);int pending=game.Inventory[(int)Block.Wood];game.SetPaused(false);
            bool closeSafe=game.Crafting.Grid[8]==Block.Air&&game.Inventory[(int)Block.Wood]==pending;
            game.OpenCrafting();Screen.SetResolution(960,600,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(2);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"05-small-window.png"));yield return new WaitForSecondsRealtime(1);
            // All travel/save tests stay in the explicit smoke-test save folder.
            game.ShowRecipe(Blocklands.Crafting.Recipes.Length-1);game.ShowBagPage(2);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"07-book-small.png"));yield return new WaitForSecondsRealtime(1);
            Screen.SetResolution(1440,900,FullScreenMode.Windowed);game.OpenTravel();yield return new WaitForSecondsRealtime(1);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"08-travel-map.png"));yield return new WaitForSecondsRealtime(1);
            var regionMarker=new Vector3Int(30,39,30);game.World.Edit(regionMarker,Block.WoodTile);
            int[] bagBeforeTravel=(int[])game.Inventory.Clone();bool trips=true,veins=true;
            for(int r=1;r<5;r++)
            {
                if(r==3)continue;
                trips&=game.TravelTo(r)&&game.World.Region==r;
                if(r==2)
                {
                    veins&=game.World.Get(new Vector3Int(25,19,23))==Block.Glass&&game.World.Get(new Vector3Int(35,19,23))==Block.IronOre;
                    // The same edit-and-inventory path used by mining is checked on a collectible vein.
                    var crystal=new Vector3Int(25,19,23);Block found=game.World.Get(crystal);
                    veins&=game.World.Edit(crystal,Block.Air);game.Inventory[(int)found]++;bagBeforeTravel[(int)found]++;
                }
                game.World.Edit(regionMarker,(Block)(r==1?23:r==2?29:28));
                game.Player.Teleport(r==1?new Vector3(30,26,9):r==2?new Vector3(30,26,16):new Vector3(30,28,13),0,r==1?20:16);game.Player.SetFlying(true);
                yield return new WaitForSecondsRealtime(1);
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"09-region-"+r+".png"));yield return new WaitForSecondsRealtime(1);
            }
            game.Save();game.World.RegionEdits.Clear();game.World.Generate();game.Load();
            bool regionSave=game.World.Region==4&&game.World.Get(regionMarker)==Block.DarkBrick;
            for(int r=0;r<5;r++)
            {
                if(r==3)continue;
                trips&=game.TravelTo(r);
                regionSave&=game.World.Get(regionMarker)==(r==0?Block.WoodTile:(Block)(r==1?23:r==2?29:28));
                if(r==2)regionSave&=game.World.Get(new Vector3Int(25,19,23))==Block.Air;
            }
            bool sharedBag=true;for(int i=0;i<bagBeforeTravel.Length;i++)sharedBag&=game.Inventory[i]==bagBeforeTravel[i];
            game.OpenCrafting();game.ShowRecipe(Blocklands.Crafting.Recipes.Length-1);game.ShowBagPage(2);
            yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"10-book-last-page.png"));yield return new WaitForSecondsRealtime(1);
            string result=$"grounded={ground}\nmoved={moved}\nplaced_and_decremented={placed}\nself_overlap_rejected={overlapRejected}\ncraft={crafted}\nsave_restore={restored}\nwater={swimming}\nsingle_does_not_fly={single}\ndouble_tap_flight={doubleTap}\nflight_movement={flightMove}\nflight_hover={flightHover}\nflight_descent={descent}\npaused_flight_stops={pauseHold}\nflight_save_restore={flightSave}\nflight_roof_collision={roofCollision}\nflight_ceiling={ceiling}\ngrid_reserves_without_spending={reserved}\nworkbench_crafted={bench}\nclose_grid_no_loss={closeSafe}\n";
            result+=$"all_regions_travel={trips}\nregional_crystal_veins={veins}\nregion_edits_save_restore={regionSave}\nshared_bag_preserved={sharedBag}\n";
            File.WriteAllText(Path.Combine(folder,"runtime-checks.txt"),result);
            yield return AdventureChecks(game,folder);
            yield return ExpansionChecks(game,folder);
            yield return VillageRenewalChecks(game,folder);
            yield return VillageLifeChecks(game,folder);
            yield return MarketChecks(game,folder);
            yield return LiftChecks(game,folder);
            Application.Quit(result.Contains("False")||!adventurePass?1:0);
        }
    }
}
