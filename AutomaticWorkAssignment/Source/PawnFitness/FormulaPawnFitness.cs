using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public partial class FormulaPawnFitness : PawnSetting, IPawnFitness
    {
        #region UI state
        private string _sourceString;
        private Formula? _tempFormula;
        private string? _lastException;
        public string? LastException => _lastException;
        public string SourceString
        {
            get => _sourceString;
            set
            {
                if (value != _sourceString)
                {
                    _lastException = null;
                    _sourceString = value;
                    try
                    {
                        _tempFormula = new Parser().ParseFormula(value);
                    }
                    catch (ParseException ex)
                    {
                        Logger.Message($"[AWA:core:Formula] Invalid formula: {ex}");
                        _lastException = ex.Message;
                    }
                    catch (Exception)
                    {
                        _lastException = "Unexpected error. Please contact the developer with the formula you typed.";
                        throw;
                    }
                }
            }
        }
        #endregion UI state

        private string _commitedString;
        private Formula? _comittedFormula;
        internal Formula? InnerFormula => _comittedFormula;
        private string _CommitedFormula
        {
            get => _commitedString; set
            {
                if (value != _commitedString)
                {
                    Logger.Message($"[AWA:core:Formula] Loading {value}");
                    try
                    {
                        _comittedFormula = _tempFormula;
                        Logger.Message($"[AWA:core:Formula] Loaded {_comittedFormula.Expression} with bindings [{string.Join(", ", _comittedFormula.BindingNames)}]");
                        var obsoleteBindings = bindingSettings.Keys.Except(_comittedFormula.BindingNames).ToArray();
                        Logger.Message($"[AWA:core:Formula] Clearing obsolete bindings [{string.Join(", ", obsoleteBindings)}]");
                        bindingSettings.RemoveRange(obsoleteBindings);
                        _commitedString = value;
                        _sourceString = value;
                    }
                    catch (ParseException ex)
                    {
                        Logger.Message($"[AWA:core:Formula] Invalid formula: {ex}");
                        throw;
                    }
                }
                else
                {
                    Logger.Message($"[AWA:core:Formula] Skip loading");
                }
            }
        }

        public Dictionary<string, IPawnFitness> bindingSettings = new();

        internal void Commit()
        {
            _CommitedFormula = SourceString;
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
            SourceString = commited;
            _CommitedFormula = commited;
            Scribe_Collections.Look(ref bindingSettings, "bindings");
        }
    }
}
