using AutomaticWorkAssignment;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class GenderPawnCondition : PawnSetting, IPawnCondition
    {
        public Gender Gender;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Gender, "gender");
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
            => pawn.gender == Gender;
    }
}
