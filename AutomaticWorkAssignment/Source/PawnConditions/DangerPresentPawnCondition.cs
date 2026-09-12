using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class DangerPresentPawnCondition : PawnSetting, IPawnCondition
    {
        public StoryDanger MinimumDangerRating = StoryDanger.Low;

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null && pawn.Map != null)
            {
                var dangerRating = pawn.Map.dangerWatcher.DangerRating;
                if (MinimumDangerRating == StoryDanger.None)
                {
                    // If the user specifically requests "none", then only return true if none.
                    return dangerRating == StoryDanger.None;
                }
                // Otherwise , return true if the current danger rating is equal to or greater than the minimum specified.
                return dangerRating >= MinimumDangerRating;
            }
            else
            {
                return false;
            }
        }

        public static string GetDangerRatingLabel(StoryDanger danger)
        {
            return danger switch
            {
                StoryDanger.None => "AWA.None".Translate(),
                StoryDanger.Low => "AWA.Low".Translate(),
                StoryDanger.High => "AWA.High".Translate(),
                _ => "AWA.Unknown".Translate()
            };
        }
    }
}
