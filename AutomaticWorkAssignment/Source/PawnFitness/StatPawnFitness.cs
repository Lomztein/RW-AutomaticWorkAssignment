using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class StatPawnFitness : IPawnFitness
    {
        public string Label => "Stat";
        public string Description => "Uses pawn stat for fitness.";

        public StatDef StatDef;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => StatDef != null ? pawn.GetStatValue(StatDef) : 0f;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref StatDef, "statDef");
        }
    }
}
