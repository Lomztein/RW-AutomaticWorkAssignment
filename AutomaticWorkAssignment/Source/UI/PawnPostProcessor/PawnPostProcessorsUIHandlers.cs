using Lomzie.AutomaticWorkAssignment.PawnFitness;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Lomzie.AutomaticWorkAssignment.UI.PawnPostProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public static class PawnPostProcessorUIHandlers
    {
        private static readonly List<IPawnPostProcessorUIHandler> _handlers = new List<IPawnPostProcessorUIHandler>();

        public static void AddHandler(IPawnPostProcessorUIHandler handler)
        {
            _handlers.Add(handler);
        }

        public static float Handle (Vector2 position, float width, IPawnPostProcessor pawnPostProcessor) {
            IPawnPostProcessorUIHandler handler = _handlers.FirstOrDefault(x => x.CanHandle(pawnPostProcessor));
            if (handler != null)
            {
                return handler.Handle(position, width, pawnPostProcessor);
            }
            Log.Warning($"Unable to handle PawnPostProcessor of type {pawnPostProcessor.GetType().Name}!");
            return 0f;
        }
    }
}
