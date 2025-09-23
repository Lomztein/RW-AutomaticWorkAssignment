using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class WorkTagCapablePawnCondition : PawnSetting, IPawnCondition
    {
        public bool RequireAll;
        public List<WorkTags> RequiredCapabilities = new();

        public static IEnumerable<WorkTags> ValidTags
            => Enum.GetValues(typeof(WorkTags)).Cast<WorkTags>().Except(new WorkTags[] { WorkTags.None, WorkTags.AllWork });

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref RequiredCapabilities, "requiredCapabilities");
            RequiredCapabilities ??= new List<WorkTags>();
        }

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn == null) return false;
            if (RequireAll)
            {
                return !RequiredCapabilities.Any(x => pawn.WorkTagIsDisabled(x));
            }
            else
            {
                return RequiredCapabilities.Any(x => !pawn.WorkTagIsDisabled(x));
            }
            // Work tags is a byte flag, and this check could be done using bit operations for a much more performant check.
            // And I did try to do this, but honestly couldn't figure it out and gave up.
            // If you wanna take a crack at it, go right ahead.
        }
    }
}
