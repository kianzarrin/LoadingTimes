using ColossalFramework;
using HarmonyLib;
using TrafficManager.Manager.Impl;

namespace LoadingTimes {
    using CSUtil.Commons;
    using KianCommons;

    [HarmonyPatch(typeof(RoadBaseAI))]
    [HarmonyPatch(nameof(RoadBaseAI.UpdateNodeFlags))]
    class UpdateNodeFlags {
        static void Postfix(ref NetNode data) {
            if (data.CountSegments() != 2)return;
            
            ushort nodeID = NetUtil.GetID(data);
            NodeData nodeData = NodeManager.Instance.buffer[nodeID];

            if (nodeData == null) return;

            if (nodeData.FirstTimeTrafficLight && TrafficLightManager.Instance.CanEnableTrafficLight(nodeID, ref data, out var res)) {
                TrafficLightManager.Instance.SetTrafficLight(nodeID, true, ref data);
                nodeData.FirstTimeTrafficLight = false;
            } else if (nodeData.CanHaveTrafficLights(out _) == TernaryBool.False) {
                data.m_flags &= ~NetNode.Flags.TrafficLights;
            }
        }
    }
}
