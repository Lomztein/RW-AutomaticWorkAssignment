using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using RR_PawnBadge;
using System;
using System.Reflection;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.PawnBadge
{
    [StaticConstructorOnStartup]
    public class SetBadgePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public int Position;
        public BadgeDef BadgeDef;

        private static Type _compBadgeType;
        private static FieldInfo _badgesField;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Position, "position");
            Scribe_Defs.Look(ref BadgeDef, "badgeDef");
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            MethodInfo getComp = pawn.GetType().GetMethod("GetComp").MakeGenericMethod(_compBadgeType);
            object badgeComp = getComp.Invoke(pawn, Array.Empty<object>());
            string[] current = _badgesField.GetValue(badgeComp) as string[];
            current[Position] = BadgeDef?.defName ?? string.Empty;
            _badgesField.SetValue(badgeComp, current);
        }

        static SetBadgePawnPostProcessor()
        {
            Assembly assembly = typeof(BadgeDef).Assembly;
            _compBadgeType = assembly.GetType("RR_PawnBadge.CompBadge");
            _badgesField = _compBadgeType.GetField("badges");
        }
    }
}
