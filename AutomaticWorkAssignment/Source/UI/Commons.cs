using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public static class Commons
    {
        public static void DoHeader(Rect inRect, string header)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawMenuSection(inRect);
            Widgets.Label(inRect, header);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
