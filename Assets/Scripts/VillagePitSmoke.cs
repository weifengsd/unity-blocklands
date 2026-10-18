using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    public partial class SmokeTest
    {
        IEnumerator VillagePitChecks(Sandbox g,string folder)
        {
            var rows=new List<string>();void Check(string name,bool ok){rows.Add(name+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"village-pit-checks.txt"),rows);}
            g.Respawn();g.TravelTo(1);yield return null;
            var surface=new Vector3Int(31,14,31);var below=new Vector3Int(31,13,31);var deeper=new Vector3Int(31,12,31);var bottom=new Vector3Int(31,0,31);
            var houseFloor=new Vector3Int(12,14,11);var houseBelow=new Vector3Int(12,13,11);
            Block originalRoad=g.World.Get(surface),originalFloor=g.World.Get(houseFloor);
            Check("village_road_and_house_start_solid",Blocks.Solid(originalRoad)&&Blocks.Solid(originalFloor));
            Check("player_can_open_deep_pit",g.World.Edit(surface,Block.Air)&&g.World.Edit(below,Block.Air)&&g.World.Edit(deeper,Block.Air)&&g.World.OpenDepthHole(bottom));
            Check("house_underfloor_can_be_dug",g.World.Edit(houseFloor,Block.Air)&&g.World.Edit(houseBelow,Block.Air));
            Villager adult=null;foreach(var v in FindObjectsByType<Villager>())if(!v.IsFriend){adult=v;break;}
            adult?.RestoreUnderground(new Vector3(31,-16,31));
            SkyResidentInfo child=null,elder=null;foreach(var p in FindObjectsByType<SkyResidentInfo>())if(p.Child&&child==null)child=p;else if(!p.Child&&elder==null)elder=p;
            if(child!=null)child.transform.position=new Vector3(31,-16,32);if(elder!=null)elder.transform.position=new Vector3(31,-16,33);
            Check("adult_child_elder_can_be_below_ground",adult!=null&&adult.transform.position.y<0&&child!=null&&child.transform.position.y<0&&elder!=null&&elder.transform.position.y<0);
            g.Save();g.Load();g.SetPaused(false);yield return null;
            Check("same_visit_load_keeps_open_pit",g.World.Get(surface)==Block.Air&&g.World.Get(below)==Block.Air&&g.World.Get(bottom)==Block.Air);
            g.TravelTo(0);g.TravelTo(1);yield return null;
            Check("reentry_caps_only_road_surface",g.World.Get(surface)==originalRoad&&g.World.Get(below)==Block.Air&&g.World.Get(deeper)==Block.Air&&g.World.Get(bottom)==Block.Air);
            Check("reentry_restores_house_floor_preserves_pit",g.World.Get(houseFloor)==originalFloor&&g.World.Get(houseBelow)==Block.Air);
            int adults=0,children=0,elders=0;bool adultsAbove=true,youngOldAbove=true;
            foreach(var v in FindObjectsByType<Villager>()){if(v.IsFriend)continue;adults++;adultsAbove&=v.transform.position.y>=14;}
            foreach(var p in FindObjectsByType<SkyResidentInfo>()){if(p.Child)children++;else elders++;youngOldAbove&=p.transform.position.y>=14;}
            Check("all_six_adults_return_to_surface",adults==6&&adultsAbove&&g.FallenResidents.Count==0);
            Check("all_children_and_elders_return",children==12&&elders==12&&youngOldAbove);
            g.Save();g.Load();yield return null;
            Check("capped_surface_and_deep_pit_survive_save",g.World.Get(surface)==originalRoad&&g.World.Get(below)==Block.Air&&g.World.Get(bottom)==Block.Air);
        }
    }
}
