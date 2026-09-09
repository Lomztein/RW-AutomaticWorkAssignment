using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors
{
    public class PrintFormattedMessagePawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        private const string Placeholder = "{}";

        public string MessageTemplate = string.Empty;
        public List<IPawnSetting> PawnSettings = new List<IPawnSetting>();

        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            string message = MessageTemplate ?? string.Empty;
            foreach (IPawnSetting setting in PawnSettings)
            {
                int placeholderIndex = message.IndexOf(Placeholder, StringComparison.Ordinal);
                if (placeholderIndex < 0)
                    break;

                string value = GetValue(setting, pawn, workSpecification, request);
                message = message.Remove(placeholderIndex, Placeholder.Length).Insert(placeholderIndex, value);
            }

            Messages.Message($"{pawn.Name}: {message}", MessageTypeDefOf.NeutralEvent);
        }

        private string GetValue(IPawnSetting setting, Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            if (setting == null)
                return string.Empty;
            if (setting is IPawnFitness fitness)
                return fitness.CalcFitness(pawn, workSpecification, request).ToString();
            if (setting is IPawnCondition condition)
                return condition.IsValid(pawn, workSpecification, request).ToString();
            if (setting is IPawnPostProcessor)
                return "Executed";
            return setting.Label;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MessageTemplate, "messageTemplate");
            Scribe_Collections.Look(ref PawnSettings, "pawnSettings", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PawnSettings ??= new List<IPawnSetting>();
                PawnSettings = PawnSettings.Where(x => x.IsValidAfterLoad()).ToList();
            }
        }

        public override bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(MessageTemplate);
        }
    }
}
