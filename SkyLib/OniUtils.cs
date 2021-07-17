using System.Collections.Generic;
using Database;
using ProcGen;
using STRINGS;
using BUILDINGS = TUNING.BUILDINGS;

// Heavily referencing CaiLib's utils.

namespace SkyLib
{
    public static class OniUtils
    {
        public static void AddBuildingToTech(string tech, string buildingid)
        {
            Db.Get().Techs.Get(tech).unlockedItemIDs.Add(buildingid);
        }

        public static void AddBuildingToBuildMenu(HashedString category, string buildingid, string addAfterId = null)
        {
            var i = BUILDINGS.PLANORDER.FindIndex(x => x.category == category);
            if (i == -1)
            {
                Logger.LogLine($"Could not find building category '{category}'");
                return;
            }

            var planorderlist = BUILDINGS.PLANORDER[i].data as IList<string>;
            if (planorderlist == null)
            {
                Logger.LogLine($"Could not find planorder with the given index for '{category}'");
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
                    Logger.LogLine($"Could not find the building '{addAfterId}' to add '{buildingid}' after.");
                    return;
                }

                planorderlist.Insert(neigh_i + 1, buildingid);
            }
        }

        public static void AddBuildingStrings(string id, string name, string desc, string effect)
        {
            var id_up = id.ToUpperInvariant();
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.NAME", UI.FormatAsLink(name, id));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.DESC", desc);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{id_up}.EFFECT", effect);
        }

        public static void AddStatusItem(string status_id, string stringtype, string statusitem, string category = "MISC")
        {
            category = category.ToUpperInvariant();
            Strings.Add($"STRINGS.{category}.STATUSITEMS.{status_id.ToUpperInvariant()}.{stringtype.ToUpperInvariant()}",
                statusitem);
        }

        public static void AddDiseaseName(string disease_id, string name)
        {
            Strings.Add($"STRINGS.DUPLICANTS.DISEASES.{disease_id.ToUpperInvariant()}.NAME", name);
        }

        public static bool IsCellExposedToSpace(int cell)
        {
            // check in space biome, then check there is no drywall
            return Game.Instance.world.zoneRenderData.GetSubWorldZoneType(cell) == SubWorld.ZoneType.Space
                   && Grid.Objects[cell, (int) ObjectLayer.Backwall] == null;
        }
    }
}
