using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public static class PawnConditionUIHandlers
    {
        private static readonly List<IPawnConditionUIHandler> _handlers = new List<IPawnConditionUIHandler>();

        public static void AddHandler(IPawnConditionUIHandler handler)
        {
            _handlers.Add(handler);
        }

        public static float Handle (Vector2 position, float width, IPawnCondition pawnCondition) {
            IPawnConditionUIHandler handler = _handlers.FirstOrDefault(x => x.CanHandle(pawnCondition));
            if (handler != null)
            {
                return handler.Handle(position, width, pawnCondition);
            }
            Log.Warning($"Unable to handle PawnCondition of type {pawnCondition.GetType().Name}!");
            return 0f;
        }
    }
}
