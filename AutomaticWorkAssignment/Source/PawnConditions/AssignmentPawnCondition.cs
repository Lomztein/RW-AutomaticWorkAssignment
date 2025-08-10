using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AssignmentPawnCondition : PawnSetting, IPawnCondition
    {
        public WorkSpecification WorkSpec;
        private string _workSpecName;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                return request.WorkManager.GetAssignmentTo(pawn, WorkSpec) != null;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref WorkSpec, "workSpec");

            if (Scribe.mode == LoadSaveMode.Saving)
                _workSpecName = WorkSpec?.Name;

            Scribe_Values.Look(ref _workSpecName, "workSpecName");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!MapWorkManager.LastInitialized.WorkList.Contains(WorkSpec))
                {
                    // We loaded the wrong maps work spec, nullify it.
                    WorkSpec = null;
                }

                if (WorkSpec == null && _workSpecName != null)
                    WorkSpec = MapWorkManager.LastInitialized.WorkList.Find(x => x.Name == _workSpecName);
            }
        }
    }
}
