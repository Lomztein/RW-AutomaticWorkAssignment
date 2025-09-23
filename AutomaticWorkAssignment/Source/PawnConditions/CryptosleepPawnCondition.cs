using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class CryptosleepPawnCondition : PawnSetting, IPawnCondition
    {
        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            return pawn.InCryptosleep;
        }
    }
}
