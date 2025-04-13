using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public static class PawnFitnessUIHandlers
    {
        private static readonly List<IPawnFitnessUIHandler> _handlers = new List<IPawnFitnessUIHandler>();

        public static void AddHandler(IPawnFitnessUIHandler handler)
        {
            _handlers.Add(handler);
        }

        public static float Handle (Vector2 position, float width, IPawnFitness pawnFitness) {
            IPawnFitnessUIHandler handler = _handlers.FirstOrDefault(x => x.CanHandle(pawnFitness));
            if (handler != null)
            {
                return handler.Handle(position, width, pawnFitness);
            }
            Log.Warning($"Unable to handle PawnFitness of type {pawnFitness.GetType().Name}!");
            return 0f;
        }
    }
}
