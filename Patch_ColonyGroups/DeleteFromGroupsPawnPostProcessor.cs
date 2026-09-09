using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using System.Linq;
using TacticalGroups;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.ColonyGroups
{
    public class DeleteFromGroupsPawnPostProcessor : PawnSetting, IPawnPostProcessor
    {
        public void PostProcess(Pawn pawn, WorkSpecification workSpecification, ResolveWorkRequest request)
        {
            var groups = TacticUtils.TacticalGroups?.pawnGroups;
            if (pawn == null || groups == null)
                return;

            foreach (var group in groups.ToArray())
            {
                if (!group.pawns.Contains(pawn) && !group.formerPawns.Contains(pawn))
                    continue;

                // Keep manually created groups available when the last member leaves.
                group.autoDisbandWithoutPawns = false;
                group.formerPawns.Remove(pawn);
                group.Disband(pawn);
                if (pawn.TryGetGroups(out var registeredGroups))
                    registeredGroups.Remove(group);
            }
        }
    }
}
