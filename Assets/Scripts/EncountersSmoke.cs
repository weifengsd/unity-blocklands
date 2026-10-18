using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blocklands
{
    public partial class SmokeTest
    {
        // Explicit isolated-save smoke hook: yield return EncountersChecks(game, folder).
        IEnumerator EncountersChecks(Sandbox g, string folder)
        {
            var checks = new List<string>();
            void Check(string name, bool ok) { checks.Add(name + "=" + ok); adventurePass &= ok; File.WriteAllLines(Path.Combine(folder, "encounters-checks.txt"), checks); }
            bool Press(RegionalEncounter station, int node) { g.Player.Teleport(station.Nodes[node]); return g.TryRegionalEncounter(); }
            void Picture(RegionalEncounter station, string name)
            {
                Vector3 view = station.Centre + new Vector3(4, 4, -8), delta = station.Centre + Vector3.up - view;
                g.Player.Teleport(view, Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg, -Mathf.Atan2(delta.y, new Vector2(delta.x, delta.z).magnitude) * Mathf.Rad2Deg);
                RenderSettings.fogColor=g.ActiveSkyColor;g.Player.View.backgroundColor=g.ActiveSkyColor;ScenePicture(g, Path.Combine(folder, name));
            }
            bool Clear(RegionalEncounter station)
            {
                foreach (var point in station.Nodes)
                {
                    var cell = Vector3Int.FloorToInt(point);
                    for (int h = 0; h < 2; h++)
                    {
                        Block b = g.World.Get(cell + Vector3Int.up * h);
                        if (b != Block.Air && b != Block.Water) return false;
                    }
                }
                return true;
            }

            g.Respawn(); g.TravelTo(0); g.SetPaused(false); yield return null;
            Check("encounters_integration", g.Encounters != null);
            if (g.Encounters == null) yield break;
            Check("forest_and_five_real_depth_stations", g.Encounters.Stations.Count == 6);
            g.Inventory[(int)Block.Coin] = 100;
            var s = g.Encounters.Find(RegionalEncounterKind.Photo);
            Check("forest_photo_exists_clear", s != null && Clear(s));
            if (s == null) yield break;
            g.Player.Teleport(s.Nodes[0]); g.SetPaused(true);
            Check("G_ignored_while_paused", !g.TryRegionalEncounter() && s.Values[0] == 0);
            g.SetPaused(false); Press(s, 1);
            Check("photo_requires_star_mode", !s.Complete);
            Press(s, 0); Press(s, 0); int money = g.Inventory[(int)Block.Coin]; Press(s, 1);
            Check("star_photo_completed", s.Complete && g.Inventory[(int)Block.Coin] == money + 12);
            Press(s, 1); Check("photo_no_duplicate_reward", g.Inventory[(int)Block.Coin] == money + 12);
            Picture(s, "encounter-forest-photo.png");

            g.TravelTo(1); yield return null; s = g.Encounters.Find(RegionalEncounterKind.Parcel);
            Check("village_parcel_route_clear", s != null && Clear(s)); if (s == null) yield break;
            Press(s, 2); Check("parcel_no_delivery_before_pickup", s.Progress == 0);
            money = g.Inventory[(int)Block.Coin]; Press(s, 0); Press(s, 2);
            Check("parcel_wrong_recipient_preserves_cargo", s.Progress == 1 && s.Parcel.gameObject.activeSelf);
            Press(s, 1); Press(s, 2);
            Check("two_parcels_delivered", s.Complete && s.Progress == 3 && !s.Parcel.gameObject.activeSelf && g.Inventory[(int)Block.Coin] == money + 12);
            Press(s, 0); Press(s, 1); Press(s, 2); Check("parcel_replay_earns_12", g.Inventory[(int)Block.Coin] == money + 24);

            g.TravelTo(2); yield return null; s = g.Encounters.Find(RegionalEncounterKind.Prism);
            Check("crystal_prisms_accessible", s != null && Clear(s)); if (s == null) yield break;
            Press(s, 0); Press(s, 1); Check("incorrect_prisms_not_complete", !s.Complete);
            Press(s, 3); Check("prism_reset", s.Values[0] == 0 && s.Values[1] == 0 && s.Values[2] == 0);
            money = g.Inventory[(int)Block.Coin]; Press(s, 0); Press(s, 1); Press(s, 1); Press(s, 2);
            Check("RGB_prism_solution", s.Complete && g.Inventory[(int)Block.Coin] == money + 12);
            Picture(s, "encounter-crystal-prisms.png");

            var save = WorldSave.Capture(g.World, g.Player, g.Inventory, g.Selected); save.gateUnlocked = true; WorldSave.Write(save, WorldSave.PathName); g.Load(); g.TravelTo(3, true); yield return null;
            s = g.Encounters.Find(RegionalEncounterKind.Egg);
            Check("dream_guardians_clear", s != null && Clear(s)); if (s == null) yield break;
            float dragonHp = g.DragonHealth;
            Press(s, 0); Check("wrong_ward_order_resets", s.Progress == 0 && !s.Complete);
            Press(s, 3); Check("egg_remains_locked_before_wards", !s.Complete);
            Press(s, 2); Press(s, 0); Press(s, 1); money = g.Inventory[(int)Block.Coin]; Press(s, 3);
            Check("egg_rescue_completed", s.Complete && g.Inventory[(int)Block.Coin] == money + 12);
            Check("egg_does_not_remove_boss", FindAnyObjectByType<DreamDragon>() != null && g.DragonHealth == dragonHp);
            Picture(s, "encounter-dream-egg.png");

            g.TravelTo(4); yield return null; s = g.Encounters.Find(RegionalEncounterKind.Bamboo);
            Check("bamboo_bridge_clear", s != null && Clear(s)); if (s == null) yield break;
            int edits = g.World.Edits.Count; money = g.Inventory[(int)Block.Coin]; Press(s, 0);
            Check("bridge_and_lanterns_toggle_on", s.Active && s.Moving[0].gameObject.activeSelf && s.Complete);
            Press(s, 0); Check("bridge_safe_toggle_off", !s.Active && !s.Moving[0].gameObject.activeSelf && g.World.Edits.Count == edits);
            Press(s, 0); Check("bridge_replay_earns_again", g.Inventory[(int)Block.Coin] == money + 24);

            int sameRegion = g.World.Region;
            s = g.Encounters.Find(RegionalEncounterKind.Sluice);
            Check("sluice_real_negative_height_and_clear", s != null && s.Centre.y < -30 && Clear(s)); if (s == null) yield break;
            Press(s, 0); Vector3 before = s.Moving[0].localPosition; g.Encounters.Tick(.4f);
            Check("sluice_animated_flow", s.Active && Vector3.Distance(before, s.Moving[0].localPosition) > .1f);
            before = s.Moving[0].localPosition; g.SetPaused(true); g.Encounters.Tick(1);
            Check("sluice_pauses_with_game", before == s.Moving[0].localPosition); g.SetPaused(false);
            Picture(s, "encounter-lava-sluice.png"); Press(s, 0);
            Check("sluice_shuts_no_world_mutation", !s.Active && !s.Moving[0].gameObject.activeSelf && !s.Props[1].gameObject.activeSelf && g.World.Edits.Count == edits && g.World.Region == sameRegion);

            s = g.Encounters.FindByTitle("深铁矿灯观景台");
            Check("deep_iron_photo_exists_at_correct_depth", s != null && s.Kind == RegionalEncounterKind.Photo && s.Centre.y > -91 && s.Centre.y < -61 && Clear(s)); if (s == null) yield break;
            money = g.Inventory[(int)Block.Coin]; Press(s, 0); Press(s, 0); Press(s, 1);
            Check("deep_iron_lamp_photo_completed", s.Complete && g.Inventory[(int)Block.Coin] == money + 12);
            Press(s, 1); Check("deep_iron_reward_once", g.Inventory[(int)Block.Coin] == money + 12);
            Picture(s, "encounter-deep-iron-photo.png");

            s = g.Encounters.FindByTitle("星晶光路阵");
            Check("star_crystal_prisms_exist_at_correct_depth", s != null && s.Kind == RegionalEncounterKind.Prism && s.Centre.y > -121 && s.Centre.y < -91 && Clear(s)); if (s == null) yield break;
            money = g.Inventory[(int)Block.Coin]; Press(s, 0); Press(s, 1); Press(s, 1); Press(s, 2);
            Check("star_crystal_light_path_completed", s.Complete && g.Inventory[(int)Block.Coin] == money + 12);
            Picture(s, "encounter-star-crystal-prisms.png");

            s = g.Encounters.Find(RegionalEncounterKind.Salvage);
            Check("salvage_real_negative_height_and_clear", s != null && s.Centre.y < -120 && Clear(s)); if (s == null) yield break;
            Press(s, 0); Press(s, 0); Check("salvage_miss_keeps_progress", s.Progress == 0 && !s.Active);
            money = g.Inventory[(int)Block.Coin];
            for (int i = 0; i < 3; i++) { Press(s, 0); g.Encounters.Tick(.7f); Press(s, 0); }
            Check("salvage_three_timed_catches", s.Complete && s.Progress == 3 && g.Inventory[(int)Block.Coin] == money + 12);
            Picture(s, "encounter-starsea-salvage.png"); Press(s, 0);
            Check("salvage_repeatable_reset", !s.Complete && s.Progress == 0 && s.Paid);

            s = g.Encounters.Find(RegionalEncounterKind.Garden);
            Check("garden_real_negative_height_and_clear", s != null && s.Centre.y < -160 && Clear(s)); if (s == null) yield break;
            money = g.Inventory[(int)Block.Coin];
            for (int i = 0; i < 3; i++) { Press(s, i); Press(s, i); }
            Check("garden_planted_and_watered", s.Values[0] == 2 && s.Values[1] == 2 && s.Values[2] == 2);
            g.SetPaused(true); g.Encounters.Tick(5); Check("garden_growth_pauses", s.Values[0] == 2 && s.Growth[0] == 0); g.SetPaused(false);
            g.Encounters.Tick(4.1f); Check("garden_blooms_after_wait", s.Values[0] == 3 && s.Props[0].localScale == Vector3.one);
            Picture(s, "encounter-garden-blooms.png");
            for (int i = 0; i < 3; i++) Press(s, i);
            Check("garden_three_harvests_complete", s.Complete && s.Progress == 3 && g.Inventory[(int)Block.Coin] == money + 12);
            Press(s, 3); Check("garden_free_replant", s.Values[0] == 0 && s.Values[1] == 0 && s.Values[2] == 0 && !s.Complete && s.Paid);
            Check("all_deep_encounters_preserve_continuous_world", g.World.Region == sameRegion && g.World.Edits.Count == edits);
            g.TravelTo(0); yield return null;
        }
    }
}
