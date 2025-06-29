using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class SetPawnColonistBarOrderingPawnPostProcessor : NestedPawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            IPawnFitness fitness = InnerSetting as IPawnFitness;
            if (pawn != null && pawn.playerSettings != null && fitness != null)
            {
                pawn.playerSettings.displayOrder = (int)fitness.CalcFitness(pawn, workSpecification, request);
                Find.ColonistBar.MarkColonistsDirty();
            }
        }
    }
}
