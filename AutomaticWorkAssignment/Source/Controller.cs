using AutomaticWorkAssignment.Source.UI.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.UI.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.PawnFitness;
using Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class Controller : Mod
    {
        public Controller(ModContentPack content) : base(content)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Initialize fitness UI handlers.
            PawnFitnessUIHandlers.AddHandler(new PawnFitnessUIHandler<LearnRatePawnFitness>());
            PawnFitnessUIHandlers.AddHandler(new PawnFitnessUIHandler<SkillLevelPawnFitness>());

            PawnFitnessUIHandlers.AddHandler(new GenericPickerPawnFitnessUIHandler<StatPawnFitness, StatDef>(
                () => DefDatabase<StatDef>.AllDefs, x => x.label, x => x?.StatDef?.label ?? "Pick stat", (c, s) => c.StatDef = s));

            // Initialize condition UI handlers.
            PawnConditionUIHandlers.AddHandler(new SkillLevelPawnConditionUIHandler());

            PawnConditionUIHandlers.AddHandler(new GenericPickerPawnConditionUIHandler<HediffPawnCondition, HediffDef>(
                () => DefDatabase<HediffDef>.AllDefs, x => x.label, x => x?.HediffDef?.label ?? "Pick condition", (c, s) => c.HediffDef = s));
            PawnConditionUIHandlers.AddHandler(new GenericPickerPawnConditionUIHandler<PassionPawnCondition, SkillDef>(
                () => DefDatabase<SkillDef>.AllDefs, x => x.label, x => x?.SkillDef?.label ?? "Pick skill", (c, s) => c.SkillDef = s));

            PawnConditionUIHandlers.AddHandler(new NestedPawnConditionUIHandler());

            // Biotech only
            if (ModLister.BiotechInstalled)
            {
                PawnConditionUIHandlers.AddHandler(new GenericPickerPawnConditionUIHandler<LifeStagePawnCondition, LifeStageDef>(
                    () => DefDatabase<LifeStageDef>.AllDefs, x => x.label, x => x?.LifeStageDef?.label ?? "Pick stage", (c, s) => c.LifeStageDef = s));
                PawnConditionUIHandlers.AddHandler(new GenericPickerPawnConditionUIHandler<GenePawnCondition, GeneDef>(
                    () => DefDatabase<GeneDef>.AllDefs, x => x.label, x => x?.GeneDef?.label ?? "Pick gene", (c, s) => c.GeneDef = s));
            }


            // Initialize post processor UI handlers.
            PawnPostProcessorUIHandlers.AddHandler(new SetTitlePawnPostProcessorUIHandler());
        }
    }
}
