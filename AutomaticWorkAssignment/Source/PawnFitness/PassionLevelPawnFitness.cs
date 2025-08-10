using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class PassionLevelPawnFitness : PawnSetting, IPawnFitness
    {
        public SkillDef SkillDef;

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            float sum = 0;
            int amount = 0;

            var defs = GetSkillDefs(specification);
            foreach (var def in defs)
            {
                SkillRecord skillRecord = pawn.skills.GetSkill(def);
                if (Utils.HasPassionIn(pawn, def))
                {
                    sum += (int)skillRecord.passion;
                    amount++;
                }
            }

            if (amount > 0)
                return sum / amount;
            else return 0;
        }

        private IEnumerable<SkillDef> GetSkillDefs(WorkSpecification workSpec)
        {
            if (SkillDef == null)
            {
                foreach (WorkTypeDef workDef in workSpec.Priorities.OrderedPriorities)
                {
                    foreach (SkillDef skillDef in workDef.relevantSkills)
                    {
                        yield return skillDef;
                    }
                }
            }
            else
            {
                yield return SkillDef;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref SkillDef, "skillDef");
        }
    }
}
