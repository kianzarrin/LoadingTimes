namespace LoadingTimes
{
    using CitiesHarmony.API;
    using HarmonyLib;
    using KianCommons;
    using System.Reflection;

    public static class HarmonyExtension
    {
        public static string HARMONY_ID = "CS.Kian.LoadingTimes";
        public static void InstallHarmony() => HarmonyUtil.InstallHarmony(HARMONY_ID);
        public static void UninstallHarmony() => HarmonyUtil.UninstallHarmony(HARMONY_ID);
    }
}