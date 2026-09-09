using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class AssignmentPawnCondition : PawnSetting, IPawnCondition
    {
        public WorkSpecification WorkSpec;
        private string _workSpecName;
        private MapWorkManager _owningManager;

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
            // Capture during LoadingVars: PostLoadInit runs after the manager's
            // ExposeData scope, and LastInitialized can belong to another map.
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                _owningManager = MapWorkManager.DeserializingManager;

            base.ExposeData();
            Scribe_References.Look(ref WorkSpec, "workSpec");

            if (Scribe.mode == LoadSaveMode.Saving)
                _workSpecName = WorkSpec?.Name;

            Scribe_Values.Look(ref _workSpecName, "workSpecName");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_owningManager == null || !_owningManager.WorkList.Contains(WorkSpec))
                {
                    // Never retain an assignment from another map or the pre-import list.
                    WorkSpec = null;
                }

                if (WorkSpec == null && _workSpecName != null)
                    WorkSpec = _owningManager?.WorkList.Find(x => x.Name == _workSpecName);
            }
        }
    }
}
