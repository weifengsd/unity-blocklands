using System.Collections;using System.Collections.Generic;using System.IO;using UnityEngine;
namespace Blocklands {public partial class SmokeTest {
 IEnumerator EntertainmentGChecks(Sandbox g,string folder){var rows=new List<string>();void Check(string k,bool ok){rows.Add(k+"="+ok);adventurePass&=ok;File.WriteAllLines(Path.Combine(folder,"g-checks.txt"),rows);}
  g.Respawn();g.RoyalIdentity=true;g.TravelTo(6);yield return null;g.Player.Teleport(PalaceLife.CarouselPoint);Check("g_starts_carousel",g.TryEntertainmentG()&&g.PalacePlayLocked);Check("g_exits_carousel",g.TryEntertainmentG()&&!g.PalacePlayLocked);g.Player.Teleport(PalaceLife.BankPoint);Check("g_opens_vault",g.TryEntertainmentG()&&g.PalaceConsoleOpen);Check("g_closes_vault",g.TryEntertainmentG()&&!g.PalaceConsoleOpen);
  g.TravelTo(0);yield return null;var photo=g.Encounters.Find(RegionalEncounterKind.Photo);g.Player.Teleport(photo.Nodes[0]);Check("g_starts_regional_interaction",g.TryEntertainmentG()&&photo.Values[0]==1);g.SetPaused(true);Check("g_blocked_while_paused",!g.TryEntertainmentG());g.SetPaused(false);g.TravelTo(1);yield return null;var memory=g.FestivalStations.Find(s=>s.Kind==FestivalKind.Memory);g.Player.Teleport(memory.Centre);memory.StartGame();Check("g_exits_active_activity",g.TryEntertainmentG()&&!memory.Running);memory.Cancel();
 }
}}
