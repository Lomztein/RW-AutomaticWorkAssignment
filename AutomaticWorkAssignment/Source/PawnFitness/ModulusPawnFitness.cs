using AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using System.Collections.Generic;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnFitness
{
    public class ModulusPawnFitness : PawnSetting, IPawnFitness, ICompositePawnSetting
    {
        public IPawnFitness LeftHandSide;
        public IPawnFitness RightHandSide = CreateFrom<IPawnFitness>(DefDatabase<PawnSettingDef>.GetNamed("Lomzie_ConstantPawnFitness"));

        public float CalcFitness(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return LeftHandSide?.CalcFitness(pawn, specification, request) % RightHandSide?.CalcFitness(pawn, specification, request) ?? 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref LeftHandSide, "leftHandSide");
            Scribe_Deep.Look(ref RightHandSide, "rightHandSide");
        }

        public IEnumerable<IPawnSetting> GetSettings()
        {
            yield return LeftHandSide;
            yield return RightHandSide;
        }
    }
}
