using HarmonyLib;
using PerformanceOptimizer;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.PerformanceOptimizer
{
    public class Patch_PerformanceOptimizer : Mod
    {
        public Patch_PerformanceOptimizer(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("Lomzie.AutomaticWorkAssignment.PerfOptimizer");
            var init = AccessTools.Method("PerformanceOptimizerSettings:Initialize");
            harmony.Patch(init, postfix: new Action(Settings_Initialize_PostFix));
        }

        private static void Settings_Initialize_PostFix ()
        {
            var learnRateOptimization = PerformanceOptimizerSettings.optimizations.Find(x => x is Optimization_SkillRecord_LearnRateFactor);
            FieldInfo enabledField = typeof(Optimization_SkillRecord_LearnRateFactor).GetField("enabled", BindingFlags.NonPublic | BindingFlags.Instance);
            enabledField.SetValue(learnRateOptimization, false);
            learnRateOptimization.Apply();
            Log.Message("[AWA] Disabled Performance Optimizers SkillRecord.LearnRateFactor optimization.");
        }
    }
}