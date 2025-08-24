using Lomzie.AutomaticWorkAssignment.Amounts;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class Defaults
    {
        public static IEnumerable<WorkSpecification> GenerateDefaultWorkSpecifications()
        {
            WorkSpecification spec;

            // Firefight
            spec = new WorkSpecification();
            spec.Name = "Firefight";
            spec.IsCritical = true;
            spec.TargetWorkers = new PercentagePawnAmount() { Percentage = 0.5f };
            spec.MinWorkers = new PercentagePawnAmount() { Percentage = 0.2f };
            if (ModsConfig.BiotechActive)
            {
                GenePawnCondition pyrophobiaCondition = PawnSetting.CreateFrom<GenePawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_GeneCondition"));
                pyrophobiaCondition.GeneDef = DefDatabase<GeneDef>.GetNamed("FireTerror");
            }

            spec.Priorities.AddPriority(WorkTypeDefOf.Firefighter);
            yield return spec;

            // Medic
            spec = new WorkSpecification();
            spec.Name = "Medic";
            spec.TargetWorkers = new PercentagePawnAmount() { Percentage = 1f };
            spec.MinWorkers = new IntPawnAmount() { Value = 2 };
            spec.IsCritical = true;
            SkillLevelPawnCondition skillLevelCondition = PawnSetting.CreateFrom<SkillLevelPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_SkillLevelCondition"));
            skillLevelCondition.SkillDef = SkillDefOf.Medicine;
            skillLevelCondition.MinLevel = 10;
            skillLevelCondition.MaxLevel = 100;
            spec.Conditions.Add(skillLevelCondition);

            PassionPawnCondition passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Medicine;
            spec.Conditions.Add(passionCondition);

            StatPawnFitness tendQualityFitness = PawnSetting.CreateFrom<StatPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_StatPawnFitness"));
            tendQualityFitness.StatDef = StatDefOf.MedicalTendQuality;
            spec.Fitness.Add(tendQualityFitness);

            foreach (var medicalWorkType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Medicine))
                spec.Priorities.AddPriority(medicalWorkType);

            yield return spec;

            // Basics
            spec = new WorkSpecification();
            spec.Name = "Basics";
            spec.TargetWorkers = new PercentagePawnAmount() { Percentage = 1 };
            spec.RequireFullPawnCapability = false;
            var workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(x => x.naturalPriority >= 1000)
                .Where(x => x.relevantSkills.Count == 0).Except(WorkTypeDefOf.Firefighter).ToList();
            workTypes.SortBy(x => -x.naturalPriority);
            foreach (var workType in workTypes)
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Social
            spec = new WorkSpecification();
            spec.Name = "Social";
            spec.TargetWorkers = new IntPawnAmount() { Value = 2 };
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.Commitment = 0.25f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Social;
            spec.Conditions.Add(passionCondition);

            SkillLevelPawnFitness skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var medicalWorkType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Social))
                spec.Priorities.AddPriority(medicalWorkType);

            yield return spec;

            // Research
            spec = new WorkSpecification();
            spec.Name = "Researcher";
            spec.TargetWorkers = new IntPawnAmount() { Value = 1 };
            spec.IsSpecialist = true;
            spec.Commitment = 1f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Intellectual;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var researchWorkType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Intellectual))
                spec.Priorities.AddPriority(researchWorkType);

            yield return spec;

            // Labourer
            spec = new WorkSpecification();
            spec.Name = "Labourer";
            spec.TargetWorkers = new IntPawnAmount() { Value = 1 };
            spec.RequireFullPawnCapability = false;
            spec.Commitment = 1f;

            workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(x => x.naturalPriority < 1000)
                .Where(x => x.relevantSkills.Count == 0).ToList();
            workTypes.SortBy(x => -x.naturalPriority);
            foreach (var workType in workTypes)
                spec.Priorities.AddPriority(workType);

            var inversePassionCountFitness = PawnSetting.CreateFrom<InvertPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_InvertPawnFitness"));
            inversePassionCountFitness.InnerSetting = PawnSetting.CreateFrom<PassionCountPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_PassionCountPawnFitness"));
            spec.Fitness.Add(inversePassionCountFitness);

            yield return spec;

            // Construction
            spec = new WorkSpecification();
            spec.Name = "Construction";
            spec.TargetWorkers = new PercentagePawnAmount() { Percentage = 1 };
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.Commitment = 0.25f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Construction;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Construction))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Animals
            spec = new WorkSpecification();
            spec.Name = "Handler";
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.TargetWorkers = new IntPawnAmount() { Value = 2 };
            spec.Commitment = 0.25f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Animals;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Animals))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Cooking
            spec = new WorkSpecification();
            spec.Name = "Cook";
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.TargetWorkers = new IntPawnAmount() { Value = 2 };
            spec.Commitment = 0.5f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Cooking;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Cooking))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Plants
            spec = new WorkSpecification();
            spec.Name = "Plants";
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.TargetWorkers = new IntPawnAmount() { Value = 2 };
            spec.Commitment = 0.75f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Plants;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Plants))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Mining
            spec = new WorkSpecification();
            spec.Name = "Miner";
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.TargetWorkers = new IntPawnAmount() { Value = 2 };
            spec.Commitment = 0.25f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Mining;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Mining))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Skilled Crafting
            spec = new WorkSpecification();
            spec.Name = "Crafting";
            spec.TargetWorkers = new PercentagePawnAmount() { Percentage = 0.2f };
            spec.MinWorkers = new IntPawnAmount() { Value = 1 };
            spec.Commitment = 0.75f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Crafting;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Crafting))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Art
            spec = new WorkSpecification();
            spec.Name = "Artist";
            spec.TargetWorkers = new IntPawnAmount() { Value = 1 };
            spec.Commitment = 0.5f;

            passionCondition = PawnSetting.CreateFrom<PassionPawnCondition>(DefDatabase<PawnConditionDef>.GetNamed("Lomzie_HasPassionCondition"));
            passionCondition.SkillDef = SkillDefOf.Artistic;
            spec.Conditions.Add(passionCondition);

            skillLevelFitness = PawnSetting.CreateFrom<SkillLevelPawnFitness>(DefDatabase<PawnFitnessDef>.GetNamed("Lomzie_SkillLevelFitness"));
            spec.Fitness.Add(skillLevelFitness);

            foreach (var workType in GetSortedWorkTypesRelatedToSkill(SkillDefOf.Artistic))
                spec.Priorities.AddPriority(workType);

            yield return spec;

            // Fallbacks
            spec = new WorkSpecification();
            spec.Name = "Fallbacks";
            spec.TargetWorkers = new PercentagePawnAmount() { Percentage = 1 };
            spec.RequireFullPawnCapability = false;
            spec.IncludeSpecialists = true;
            workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(x => x.naturalPriority < 1000)
                .Where(x => x.relevantSkills.Count == 0).ToList();
            workTypes.SortBy(x => -x.naturalPriority);
            foreach (var workType in workTypes)
                spec.Priorities.AddPriority(workType);

            yield return spec;
        }

        private static IEnumerable<WorkTypeDef> GetSortedWorkTypesRelatedToSkill(SkillDef skillDef)
        {
            var defs = new List<WorkTypeDef>(DefDatabase<WorkTypeDef>.AllDefsListForReading)
                .Where(x => x.relevantSkills.Contains(skillDef)).ToList();
            defs.SortBy(x => -x.naturalPriority);
            return defs;
        }
    }
}
