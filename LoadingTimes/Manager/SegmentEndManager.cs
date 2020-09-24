namespace LoadingTimes {
    using System;
    using System.Collections.Generic;
    using KianCommons;
    using LoadingTimes.Tool;
    using UnityEngine.Assertions;

    [Serializable]
    public class SegmentEndManager {
        #region LifeCycle
        public static SegmentEndManager Instance { get; private set; } = new SegmentEndManager();

        public static byte[] Serialize() => SerializationUtil.Serialize(Instance);

        public static void Deserialize(byte[] data, Version version) {
            if (data == null) {
                Instance = new SegmentEndManager();
                Log.Debug($"SegmentEndManager.Deserialize(data=null)");

            } else {
                Log.Debug($"SegmentEndManager.Deserialize(data): data.Length={data?.Length}");
                Instance = SerializationUtil.Deserialize(data, version) as SegmentEndManager;
            }
        }

        public void OnLoad() {
            UpdateAll();
        }

        #endregion LifeCycle

        public SegmentEndData[] buffer = new SegmentEndData[NetManager.MAX_SEGMENT_COUNT * 2];

        public ref SegmentEndData GetAt(ushort segmentID, ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: nodeID);
            return ref GetAt(segmentID, startNode);
        }
        public ref SegmentEndData GetAt(ushort segmentID, bool startNode) {
            if (startNode)
                return ref buffer[segmentID * 2];
            else
                return ref buffer[segmentID * 2 + 1];
        }

        public void SetAt(ushort segmentID, ushort nodeID, SegmentEndData value) {
            bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: nodeID);
            SetAt(segmentID, startNode, value);
        }

        public void SetAt(ushort segmentID, bool startNode, SegmentEndData value) {
            GetAt(segmentID, startNode) = value;
        }

        public ref SegmentEndData GetOrCreate(ushort segmentID, ushort nodeID) {
            bool startNode = NetUtil.IsStartNode(segmentId: segmentID, nodeId: nodeID);
            return ref GetOrCreate(segmentID, startNode);
        }

        public ref SegmentEndData GetOrCreate(ushort segmentID, bool startNode) {
            ref SegmentEndData data = ref GetAt(segmentID, startNode);
            if (data == null) {
                ushort nodeID = NetUtil.GetSegmentNode(segmentID, startNode);
                data = new SegmentEndData(segmentID: segmentID, nodeID: nodeID);
                SetAt(segmentID: segmentID, startNode: startNode, data);
            }
            Assertion.AssertNotNull(data);
            return ref data;
        }

        /// <summary>
        /// releases data for <paramref name="segmentID"/> <paramref name="startNode"/> if uncessary. marks segment for update.
        /// </summary>
        public void UpdateData(ushort segmentID, bool startNode) {
            SegmentEndData segEnd = GetAt(segmentID, startNode);
            if (segEnd == null) return;
            if (!NetUtil.IsSegmentValid(segmentID)) {
                ResetSegmentEndToDefault(segmentID, startNode);
                return;
            }
            ushort nodeID = segmentID.ToSegment().GetNode(startNode);
            bool selected = NodeControllerTool.Instance.SelectedNodeID == nodeID;
            if (segEnd.IsDefault() && !selected) {
                ResetSegmentEndToDefault(segmentID, startNode);
            } else {
                segEnd.Update();
            }
        }

        public void ResetSegmentEndToDefault(ushort segmentID, bool startNode) {
            SegmentEndData segEnd = GetAt(segmentID, startNode);
            if (segEnd != null)
                Log.Debug($"segment End:({segmentID},{startNode}) reset to defualt");
            else
                Log.Debug($"segment End:({segmentID},{startNode}) is already null.");
            SetAt(segmentID, startNode, null);
            NetManager.instance.UpdateSegment(segmentID);
        }

        public void UpdateAll() {
            foreach (var segmentEndData in buffer) {
                if (segmentEndData == null) continue;
                if (NetUtil.IsSegmentValid(segmentEndData.SegmentID)) {
                    segmentEndData.Update();
                } else {
                    ResetSegmentEndToDefault(segmentEndData.SegmentID, true);
                    ResetSegmentEndToDefault(segmentEndData.SegmentID, false);
                }
            }
        }

        /// <summary>
        /// Called after stock code and before postfix code.
        /// if node is invalid or otherwise unsupported, it will be set to null.
        /// </summary>
        public void OnBeforeCalculateNodePatch(ushort segmentID, bool startNode) {
            SegmentEndData segEnd = GetAt(segmentID, startNode);
            // nodeID.ToNode still has default flags.
            if (segEnd == null)
                return;
            if (!NodeData.IsSupported(segEnd.NodeID)) {
                SetAt(segmentID, startNode, null);
                return;
            }

            segEnd.Calculate();
        }

    }
}
