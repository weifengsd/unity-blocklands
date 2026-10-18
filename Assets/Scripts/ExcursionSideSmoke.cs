using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator ExcursionSideChecks(Sandbox g,string folder)
        {
            var rows=new List<string>();void Check(string name,bool ok){rows.Add(name+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"excursion-side-checks.txt"),rows);}
            int[] counts={3,2,2,3,3};g.Respawn();yield return null;
            for(int index=0;index<5;index++)
            {
                int region=11+index;Check("travel_region_"+region,g.TravelTo(region)&&g.World.Region==region);yield return null;
                Check("multiple_activities_region_"+region,g.ExcursionSides.Count==counts[index]&&g.Excursion!=null);
                Check("landmarks_region_"+region,g.ExcursionScenery!=null&&g.ExcursionScenery.LandmarkCount>=3);
                var board=GameObject.Find("景点导览牌");var boardText=board!=null?board.GetComponentInChildren<TextMesh>():null;
                Check("entrance_guide_names_activities_"+region,boardText!=null&&boardText.text.Contains(g.ExcursionSides[0].Title)&&boardText.text.Contains(g.ExcursionSides[1].Title));
                if(region==14)Check("rainy_forest_has_weather",g.ExcursionWeather!=null&&g.ExcursionWeather.VisibleRaindrops>=80);
                if(region==15)
                {
                    Check("real_snow_ground_and_flakes",g.World.Get(new Vector3Int(30,14,30))==Blocks.Snow&&g.ExcursionWeather!=null&&g.ExcursionWeather.VisibleFlakes>=100);
                    var snowCell=new Vector3Int(8,14,8);Check("snow_can_be_dug_and_replaced",g.World.Get(snowCell)==Blocks.Snow&&g.World.Edit(snowCell,Block.Air)&&g.World.Edit(snowCell,Blocks.Snow));
                }
                bool hasShelter=false,hasFriend=false;
                foreach(var side in g.ExcursionSides)
                {
                    hasShelter|=side.transform.Find("遮雨大棚顶")!=null;
                    hasFriend|=side.Kind==ExcursionSideKind.RainbowTea||side.Kind==ExcursionSideKind.ForestStory||side.Kind==ExcursionSideKind.Snowman;
                    g.Player.Teleport(side.transform.position+Vector3.back*1.5f);
                    int before=g.Inventory[(int)Block.Coin],completed=side.Finished;
                    yield return CompleteSide(side);
                    Check("plays_"+side.Kind,side.Finished==completed+1&&g.Inventory[(int)Block.Coin]==before+side.Reward);
                    g.Player.Teleport(side.transform.position+Vector3.back*1.5f);
                    yield return CompleteSide(side);
                    Check("replay_earns_"+side.Kind,side.Finished==completed+2&&g.Inventory[(int)Block.Coin]==before+side.Reward*2);
                }
                if(region==11||region==14||region==15)Check("place_specific_social_or_shelter_"+region,hasShelter&&hasFriend);
                g.Player.Teleport(g.ExcursionSides[0].transform.position+Vector3.back*9,0,7);yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"region-"+region+"-side-activities.png"));yield return new WaitForSecondsRealtime(.15f);
            }
            var old=new SavedWorld{region=15,selected=1,inventory=new int[76],position=new Vector3(30,17,12)};
            WorldSave.Validate(old);Check("old_76_slot_save_keeps_items",old.inventory.Length==Blocks.Count);
            g.Save();g.Load();Check("snow_region_survives_save",g.World.Region==15&&g.World.Get(new Vector3Int(30,14,30))==Blocks.Snow);
        }

        static IEnumerator CompleteSide(ExcursionSideActivity side)
        {
            int before=side.Finished;if(!side.TryG())yield break;
            if(side.Running){side.Tick(25);yield return null;yield break;}
            for(int i=0;i<4&&side.Finished==before;i++){yield return new WaitForSecondsRealtime(.7f);side.TryG();}
        }
    }
}
