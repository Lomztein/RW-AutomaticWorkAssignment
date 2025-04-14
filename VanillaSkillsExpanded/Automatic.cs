using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using RimWorld;
using Verse;

namespace Lomzie.AWAPatches.VanillaSkillsExpanded
{
    public class VanillaSkillsExpandedPassionPawnCondition : IPawnCondition
    {
        public string Label => "Passion in skill";
        public string Description => "Check if the pawn has any passion in the given skill.";

        public SkillDef SkillDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref SkillDef, "skillDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            VSE.
        }
    }
}