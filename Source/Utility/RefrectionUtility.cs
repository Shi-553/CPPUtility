using System.Reflection;

namespace CPPUtility
{
    public static class RefrectionUtility
    {
        public static BindingFlags AllBindingFlags => BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
    }
}