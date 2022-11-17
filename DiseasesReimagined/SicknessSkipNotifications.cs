using System.Collections.Generic;

namespace DiseasesReimagined
{
    public static class SkipNotifications
    {
        public static readonly HashSet<string> SicknessIDs = new HashSet<string>();

        public static void Skip(string sickness_id)
        {
            SicknessIDs.Add(sickness_id);
        }
    }
}
