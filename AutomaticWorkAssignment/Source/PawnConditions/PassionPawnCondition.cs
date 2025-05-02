using AutomaticWorkAssignment;
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
            => pawn.skills.skills.Find(x => SkillDef?.defName == x.def.defName)?.passion != Passion.None;
    }
}
