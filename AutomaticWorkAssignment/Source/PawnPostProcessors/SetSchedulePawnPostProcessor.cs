using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetSchedulePawnPostProcessor : NestedPawnSetting, IPawnPostProcessor
    {
        public List<TimeAssignmentDef> Times;
        public SetMode Mode = SetMode.Overwrite;


        public SetSchedulePawnPostProcessor()
        {
            Times = Enumerable.Range(0, GenDate.HoursPerDay).Select(x => TimeAssignmentDefOf.Anything).ToList();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Times, "times", LookMode.Def);
            Scribe_Values.Look(ref Mode, "mode", SetMode.Overwrite);
        }

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            int offset = 0;
            if (InnerSetting != null)
            {
                IPawnFitness fitness = InnerSetting as IPawnFitness;
                offset = Mathf.RoundToInt(fitness.CalcFitness(pawn, workSpecification, request));
            }

            for (int i = 0; i < Times.Count; i++)
            {
                TimeAssignmentDef def = Times[i];
                if (Mode == SetMode.Overwrite || def != TimeAssignmentDefOf.Anything)
                {
                    pawn.timetable.SetAssignment((i + offset) % Times.Count, def);
                }
            }
        }
        
        public enum SetMode
        {
            Overwrite,
            Add
        }

        public static string GetLabel(SetMode mode)
        {
            switch (mode)
            {
                case SetMode.Overwrite:
                    return "AWA.Overwrite".Translate();
                case SetMode.Add:
                    return "AWA.Add".Translate();
                default:
                    return mode.ToString();
            }
        }

        public static string GetTooltip(SetMode mode)
        {
            switch (mode)
            {
                case SetMode.Overwrite:
                    return "AWA.SetScheduleOverwriteTooltip".Translate();
                case SetMode.Add:
                    return "AWA.SetScheduleAddTooltip".Translate();
                default:
                    return string.Empty;
            }
        }
    }
}
