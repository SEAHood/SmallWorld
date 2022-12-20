using Fusion;

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
    }
}
