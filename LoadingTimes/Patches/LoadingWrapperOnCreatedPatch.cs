using HarmonyLib;
using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using static KianCommons.Patches.TranspilerUtils;
using ICities;
using System.Reflection;
using System.Diagnostics;

namespace LoadingTimes.Patches {
    [HarmonyPatch(typeof(LoadingWrapper))]
    [HarmonyPatch("OnLoadingExtensionsCreated")]
    public static class LoadingWrapperOnCreatedPatch {
        public delegate void Handler();
        static Stopwatch sw = new Stopwatch();

        public static ILoadingExtension BeforeOnCreated(ILoadingExtension loadingExtension) {
            Log.Info($"calling {loadingExtension}.OnCreated()", copyToGameLog: true);
            sw.Reset();
            sw.Start();
            return loadingExtension;
        }
        public static void AfterOnCreated() {
            sw.Stop();
            float secs = sw.ElapsedMilliseconds * 0.001f;
            Log.Info($"OnCreated() successful. duration = {secs:f3} seconds", copyToGameLog: true);
        }

        static MethodInfo mBeforeOnCreated_ = typeof(LoadingWrapperOnCreatedPatch).GetMethod(nameof(BeforeOnCreated))
            ?? throw new Exception("mBeforeOnCreated_ is null");
        static MethodInfo mAfterOnCreated_ = typeof(LoadingWrapperOnCreatedPatch).GetMethod(nameof(AfterOnCreated))
            ?? throw new Exception("mAfterOnCreated_ is null");

        static MethodInfo mOnCreated_ = typeof(ILoadingExtension).GetMethod(nameof(ILoadingExtension.OnCreated))
            ?? throw new Exception("mAfterOnCreated_ is null");
        public static void Prefix(){ Log.Info("LoadingWrapper.OnLoadingExtensionsCreated() started", true); }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                List<CodeInstruction> codes = ToCodeList(instructions);
                var LDArg_this = new CodeInstruction(OpCodes.Ldarg_0);
                var CallVirt_OnCreated = new CodeInstruction(OpCodes.Callvirt, mOnCreated_); //callvirt instance void [ICities]ICities.ILoadingExtension::OnCreated(valuetype [ICities]ICities.LoadMode)
                var Call_BeforeOnCreated = new CodeInstruction(OpCodes.Call, mBeforeOnCreated_);
                var Call_AfterOnCreated = new CodeInstruction(OpCodes.Call, mAfterOnCreated_);

                int index = SearchInstruction(codes, CallVirt_OnCreated, 0);
                InsertInstructions(codes, new[] { Call_AfterOnCreated }, index + 1, moveLabels:false); // insert after.

                index = SearchInstruction(codes, CallVirt_OnCreated, index, dir:-1); // move before LDArg.0
                InsertInstructions(codes, new[] { Call_BeforeOnCreated }, index); // insert at (before).

                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }

        public static void Postfix() {
            Log.Info("LoadingWrapper.OnLoadingExtensionsCreated() finished", true);
        }
    }
}
