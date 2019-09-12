using System;
using System.Collections.Generic;

using ProcGen;
using TUNING;

// Heavily referencing CaiLib's utils.

namespace SkyLib
{
    public static class OniUtils
    {
        public static void AddBuildingToTech(string tech, string buildingid) {
            var techlist = new List<string>(Database.Techs.TECH_GROUPING[tech]);
            techlist.Add(buildingid);
            Database.Techs.TECH_GROUPING[tech] = techlist.ToArray();
        }

        public static void AddBuildingToBuildMenu(HashedString category, string buildingid, string addAfterId = null)
        {
            var i = TUNING.BUILDINGS.PLANORDER.FindIndex(x => x.category == category);
            if (i == -1)
            {
                Logger.LogLine("SkyLib", $"Could not find building category '{category}'");
                return;
            }

            var planorderlist = TUNING.BUILDINGS.PLANORDER[i].data as IList<string>;
            if (planorderlist == null)
            {
                Logger.LogLine("SkyLib", $"Could not find planorder with the given index for '{category}'");
                return;
            }

            if (addAfterId == null)
            {
                planorderlist.Add(buildingid);
            }
            else
            {
                var neigh_i = planorderlist.IndexOf(addAfterId);
                if (neigh_i == -1)
                {
                    Logger.LogLine("SkyLib", $"Could not find the building '{addAfterId}' to add '{buildingid}' after.");
                    return;
                }
                planorderlist.Insert(neigh_i + 1, buildingid);
            }
        }

        public static void AddBuildingStrings(string id, string name, string desc, string effect)
        {
            var id_up = id.ToUpperInvariant();
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.NAME", STRINGS.UI.FormatAsLink(name, id));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.DESC", desc);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.EFFECT", effect);
        }

        public static void AddStatusItem(string status_id, string stringtype, string statusitem)
        {
            Strings.Add($"STRINGS.MISC.STATUSITEMS.{status_id.ToUpperInvariant()}.{stringtype.ToUpperInvariant()}", statusitem);
        }
        
        public static bool IsCellExposedToSpace(int cell)
        {
            // check in space biome, then check there is no drywall
            return Game.Instance.world.zoneRenderData.GetSubWorldZoneType(cell) == SubWorld.ZoneType.Space 
                && Grid.Objects[cell, (int)ObjectLayer.Backwall] == null;
        }
    }
}
