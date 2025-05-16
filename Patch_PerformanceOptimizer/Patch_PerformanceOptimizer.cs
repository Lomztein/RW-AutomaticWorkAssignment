using PerformanceOptimizer;
using RimWorld;
using System.Reflection;
using Verse;

namespace Patch_PerformanceOptimizer
{
    public class Patch_PerformanceOptimizer : Mod
    {
        public Patch_PerformanceOptimizer(ModContentPack content) : base(content)
        {
            var learnRateOptimization = PerformanceOptimizerSettings.optimizations.Find(x => x is Optimization_SkillRecord_LearnRateFactor);
            FieldInfo enabledField = typeof(Optimization_SkillRecord_LearnRateFactor).GetField("enabled", BindingFlags.NonPublic | BindingFlags.Instance);
            enabledField.SetValue(learnRateOptimization, false);
            Log.Message("[Automatic Work Assignment] Disabled Performance Optimizers SkillRecord.LearnRateFactor optimization.");
        }
    }
}