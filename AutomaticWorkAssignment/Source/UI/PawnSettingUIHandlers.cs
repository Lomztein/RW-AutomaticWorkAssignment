using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public static class PawnSettingUIHandlers
    {
        private static List<IPawnSettingUIHandler> _handlers;

        public static void AddHandler(IPawnSettingUIHandler handler)
        {
            if (_handlers == null)
                _handlers = new List<IPawnSettingUIHandler>();
            _handlers.Add(handler);
        }

        public static float Handle (Vector2 position, float width, IPawnSetting pawnSetting) {
            IPawnSettingUIHandler handler = _handlers.FirstOrDefault(x => x.CanHandle(pawnSetting));
            if (handler != null)
            {
                return handler.Handle(position, width, pawnSetting);
            }
            Log.Warning($"Unable to handle PawnSetting of type {pawnSetting.GetType().Name}!");
            return 0f;
        }
    }
}
