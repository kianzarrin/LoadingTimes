namespace LoadingTimes.Patches.TMPE {
    using System.Reflection;
    using TrafficManager.Manager.Impl;
    using LoadingTimes;
    using KianCommons.Patches;
    using KianCommons;
    using HarmonyLib;

    [HarmonyPatch]
    public static class IsPedestrianCrossingAllowedConfigurable {
        public static MethodBase TargetMethod() {
            return typeof(JunctionRestrictionsManager).
                GetMethod(nameof(JunctionRestrictionsManager.IsPedestrianCrossingAllowedConfigurable));
        }

        public static bool Prefix(ushort segmentId, bool startNode, ref bool __result) {
            ushort nodeID = startNode ? segmentId.ToSegment().m_startNode : segmentId.ToSegment().m_endNode;
            var data = NodeManager.Instance.buffer[nodeID];
            return PrefixUtils.HandleTernaryBool(
                data?.IsPedestrianCrossingAllowedConfigurable(),
                ref __result);
        }
    }
}