using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class HourBetweenPawnCondition : PawnSetting, IPawnCondition
    {
        public float MinHour;
        public float MaxHour;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            float currentHour = GenLocalDate.HourFloat(request.Map);
            return currentHour >= MinHour && currentHour <= MaxHour;
        }
    }
}
