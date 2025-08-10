using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness : PawnSetting, IPawnFitness
    {
        public string sourceString;
        private string _commitedString;
        private Formula? _formula;
        internal Formula? InnerFormula => _formula;
        private string _CommitedFormula { get => _commitedString; set
            {
                if (value != _commitedString)
                {
                    Logger.Message($"[AWA:core:Formula] Loading {value}");
                    _formula = new Parser().ParseFormula(value);
                    Logger.Message($"[AWA:core:Formula] Loaded {_formula.Expression} with bindings [{string.Join(", ", _formula.BindingNames)}]");
                    var obsoleteBindings = bindingSettings.Keys.Except(_formula.BindingNames).ToArray();
                    Logger.Message($"[AWA:core:Formula] Clearing obsolete bindings [{string.Join(", ", obsoleteBindings)}]");
                    bindingSettings.RemoveRange(obsoleteBindings);
                    _commitedString = value;
                    sourceString = value;
                } else
                {
                    Logger.Message($"[AWA:core:Formula] Skip loading");
                }
            }
        }
        public Dictionary<string, IPawnFitness> bindingSettings = new();

        internal void Commit()
        {
            _CommitedFormula = sourceString;
        }

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            try
            {
                return InnerFormula.Calc(
                    pawn,
                    specification,
                    request,
                    new(bindingSettings.ToDictionary(
                        (kvp) => kvp.Key,
                        kvp => (Func<Pawn, WorkSpecification, ResolveWorkRequest, float>)kvp.Value.CalcFitness)));
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                Logger.Message($"[AWA:core:Formula] Failed to evaluate: {ex.Message}");
                return 0;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            var commited = _CommitedFormula;
            Scribe_Values.Look(ref commited, "formula");
            Scribe_Collections.Look(ref bindingSettings, "bindings");
            _CommitedFormula = commited;
        }
    }
}
