using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class ForceRelativePriorityPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public bool Before;
        public WorkSpecification Specification;
        public WorkTypeDef WorkType;

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (WorkType != null && Specification != null && !Utils.WorkTypeIsDisabled(pawn, WorkType))
            {
                WorkAssignment assignment = request.WorkManager.GetAssignmentTo(pawn, Specification);
                if (assignment != null)
                {
                    int newPriority = -1;
                    if (Before)
                    {
                        (WorkTypeDef def, int priority) = assignment.GetHighestPriority();
                        if (def != null)
                        {
                            newPriority = def.naturalPriority > WorkType.naturalPriority ? priority - 1 : priority;
                        }
                        
                    }
                    else
                    {
                        (WorkTypeDef def, int priority) = assignment.GetLowestPriority();
                        if (def != null)
                        {
                            newPriority = def.naturalPriority < WorkType.naturalPriority ? priority + 1 : priority;
                        }
                    }

                    if (newPriority != -1)
                        pawn.workSettings.SetPriority(WorkType, Math.Max(newPriority, 1));
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Before, "before", true);
            Scribe_References.Look(ref Specification, "specification");
            Scribe_Defs.Look(ref WorkType, "workType");
        }
    }
}
