using AutomaticWorkAssignment.Source;
using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class CommitmentLimitPawnCondition : PawnSetting, IPawnCondition
    {
        public float Limit = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Limit, "Limit");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
                return WorkManager.Instance.GetPawnCommitment(pawn) < Limit;
            return false;
        }
    }
}
