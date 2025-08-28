using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public static class PawnSettingUIHandlers
    {
        private static List<IPawnSettingUIHandler> _handlers;
        private static readonly CacheDict<IPawnSetting, IPawnSettingUIHandler> _cache = new CacheDict<IPawnSetting, IPawnSettingUIHandler>(60);

        public static void AddHandler(IPawnSettingUIHandler handler)
        {
            if (_handlers == null)
                _handlers = new List<IPawnSettingUIHandler>();
            _handlers.Add(handler);
        }

        public static IPawnSettingUIHandler? GetHandler(IPawnSetting pawnSetting)
        {
            if (_cache.TryGet(pawnSetting, out IPawnSettingUIHandler cachedHandler))
                return cachedHandler;

            IPawnSettingUIHandler? handler = _handlers.FirstOrDefault(x => x.CanHandle(pawnSetting));
            if (handler != null)
            {
                _cache.Set(pawnSetting, handler);
                return handler;
            }
            Log.Warning($"Unable to handle PawnSetting of type {pawnSetting.GetType().Name}!");
            return null;
        }
        public static float Handle(Vector2 position, float width, IPawnSetting pawnSetting)
        {
            IPawnSettingUIHandler handler = GetHandler(pawnSetting);
            if (handler != null)
            {
                return handler.Handle(position, width, pawnSetting);
            }
            return 0f;
        }
    }
}
