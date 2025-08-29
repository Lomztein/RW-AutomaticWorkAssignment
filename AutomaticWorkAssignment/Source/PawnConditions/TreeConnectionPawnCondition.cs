using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.PawnConditions
{
    public class TreeConnectionPawnCondition : PawnSetting, IPawnCondition
    {
        public Thing Tree;

        private readonly Cache<IEnumerable<CompTreeConnection>> _mapConnections = new();

        public bool IsValid(Pawn pawn, WorkSpecification specification, ResolveWorkRequest request)
        {
            if (pawn != null)
            {
                if (Tree != null)
                {
                    CompTreeConnection connection = Tree.TryGetComp<CompTreeConnection>();
                    if (connection != null)
                    {
                        return connection.ConnectedPawn == pawn;
                    }
                }
                else
                {
                    if (!_mapConnections.TryGet(out IEnumerable<CompTreeConnection> mapConnections))
                    {
                        mapConnections = request.WorkManager.GetAllMaps().
                            SelectMany(x => x.listerThings.AllThings
                            .Select(x => x.TryGetComp<CompTreeConnection>())
                            .Where(x => x != null));

                        _mapConnections.Set(mapConnections);
                    }

                    return mapConnections.Any(x => x.ConnectedPawn == pawn);
                }
            }
            return false;
        }
    }
}
