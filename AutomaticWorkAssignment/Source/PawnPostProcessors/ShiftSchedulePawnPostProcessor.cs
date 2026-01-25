using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class ShiftSchedulePawnPostProcessor : NestedPawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (InnerSetting == null)
                return;

            IPawnFitness fitness = InnerSetting as IPawnFitness;
            float value = fitness.CalcFitness(pawn, workSpecification, request);
            TimeAssignmentDef[] prev = new TimeAssignmentDef[GenDate.HoursPerDay];
            for (int i = 0; i < GenDate.HoursPerDay; i++)
            {
                prev[i] = pawn.timetable.GetAssignment(i);
            }

            for (int i = 0; i < GenDate.HoursPerDay; i++)
            {
                int shiftedIndex = (i + (int)value) % GenDate.HoursPerDay;
                pawn.timetable.SetAssignment(shiftedIndex, prev[i]);
            }
        }
    }
}
