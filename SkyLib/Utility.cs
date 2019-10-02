using System;

namespace SkyLib
{
    public static class Utility
    {
        public static T Range<T>(T min, T val, T max) where T : IComparable
        {
            if (val.CompareTo(max) > 0) return max;

            if (val.CompareTo(min) < 0) return min;

            return val;
        }
    }
}