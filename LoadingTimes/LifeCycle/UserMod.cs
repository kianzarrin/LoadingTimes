namespace LoadingTimes.LifeCycle
{
    using System;
    using JetBrains.Annotations;
    using ICities;
    using CitiesHarmony.API;
    using KianCommons;

    public class NodeControllerMod : IUserMod
    {
        public static Version ModVersion => typeof(NodeControllerMod).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Loading Times " + VersionString;
        public string Description => "control Road/junction transitions";
        public const string HARMONY_ID = "Kian.CS.LoadingTimes";

        [UsedImplicitly]
        public void OnEnabled()
        {
            HarmonyHelper.EnsureHarmonyInstalled();
            HarmonyHelper.DoOnHarmonyReady( () =>HarmonyUtil.InstallHarmony(HARMONY_ID) );
        }

        [UsedImplicitly]
        public void OnDisabled()
        {
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
        }

        //[UsedImplicitly]
        //public void OnSettingsUI(UIHelperBase helper) {
        //}
    }
}
