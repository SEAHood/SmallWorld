using Fusion;
using UnityEngine;

namespace Assets.Helper
{
    public static class Extensions
    {
        public static int IndexOf<T>(this NetworkArray<T> a, T o)
        {
            var i = 0;
            foreach (T t in a)
            {
                if (o.Equals(t)) return i;
                i++;
            }
            return -1;
        }

        public static string ToOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        public static void Clear(this Transform t)
        {
            foreach (Transform child in t)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

    }
}
