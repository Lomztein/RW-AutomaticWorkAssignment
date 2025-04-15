using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class PassionPawnCondition : IPawnCondition
    {
        public string Label => "Passion in skill";
        public string Description => "Check if the pawn has any passion in the given skill.";

        public SkillDef SkillDef;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref SkillDef, "skillDef");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.skills.skills.Find(x => SkillDef?.defName == x.def.defName)?.passion != Passion.None;
    }
}
