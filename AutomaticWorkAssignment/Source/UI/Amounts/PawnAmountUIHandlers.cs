using Lomzie.AutomaticWorkAssignment.Amounts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public static class PawnAmountUIHandlers
    {
        private static List<IPawnAmountUIHandler> _handlers;
        private static readonly CacheDict<IPawnAmount, IPawnAmountUIHandler> _cache = new CacheDict<IPawnAmount, IPawnAmountUIHandler>(60);

        public static void AddHandler(IPawnAmountUIHandler handler)
        {
            if (_handlers == null)
                _handlers = new List<IPawnAmountUIHandler>();
            _handlers.Add(handler);
        }

        public static IPawnAmountUIHandler? GetHandler(IPawnAmount pawnAmount)
        {
            if (_cache.TryGet(pawnAmount, out IPawnAmountUIHandler cachedHandler))
                return cachedHandler;

            IPawnAmountUIHandler? handler = _handlers.FirstOrDefault(x => x.CanHandle(pawnAmount));
            if (handler != null)
            {
                _cache.Set(pawnAmount, handler);
                return handler;
            }
            Log.Warning($"Unable to handle PawnAmount of type {pawnAmount.GetType().Name}!");
            return null;
        }
        public static void Handle(Rect inRect, IPawnAmount pawnSetting)
        {
            IPawnAmountUIHandler handler = GetHandler(pawnSetting);
            if (handler != null)
            {
                handler.Handle(inRect, pawnSetting);
            }
        }
    }
}
