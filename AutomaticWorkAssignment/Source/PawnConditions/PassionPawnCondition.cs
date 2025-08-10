using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PassionPawnCondition : PawnSetting, IPawnCondition
    {
        public SkillDef SkillDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref SkillDef, "skillDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            SkillRecord skillRecord = pawn.skills.skills.Find(x => SkillDef?.defName == x.def.defName);
            if (skillRecord != null)
            {
                return Utils.HasPassionIn(pawn, SkillDef);
            }
            return false;
        }
    }
}
