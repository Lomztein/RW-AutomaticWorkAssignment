using RimWorld;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class IdeoligionPawnCondition : PawnSetting, IPawnCondition
    {
        public Ideo Ideoligion;
        private string _ideoName;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return pawn.Ideo == Ideoligion;
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Ideoligion, "ideo");

            if (Scribe.mode == LoadSaveMode.Saving)
                _ideoName = Ideoligion?.name;
            
            Scribe_Values.Look(ref _ideoName, "ideoName");

            if (Scribe.mode != LoadSaveMode.Saving && !string.IsNullOrWhiteSpace(_ideoName) && Ideoligion == null)
                Ideoligion = Find.World.ideoManager.IdeosListForReading.Find(x => x.name == _ideoName);
        }
    }
}
