namespace LoadingTimes.LifeCycle {
    using ICities;
    using LoadingTimes.Tool;

    public class ThreadingExtension : ThreadingExtensionBase{
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            var tool = ToolsModifierControl.toolController?.CurrentTool;
            bool flag = tool == null || tool is NodeControllerTool ||
                tool.GetType() == typeof(DefaultTool) || tool is NetTool || tool is BuildingTool;
            if (flag && NodeControllerTool.ActivationShortcut.IsKeyUp()) {
                NodeControllerTool.Instance.ToggleTool();
            }
        }
    }
}
