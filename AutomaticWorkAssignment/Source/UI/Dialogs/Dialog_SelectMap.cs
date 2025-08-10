using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_SelectMap : Window
    {
        public override Vector2 InitialSize => new Vector2(620f, 700f) / 2f;

        private List<Map> _maps;
        private Action<Map> _action;

        public Dialog_SelectMap(IEnumerable<Map> maps, Action<Map> action)
        {
            _maps = maps.ToList();
            _action = action;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Widgets.CloseButtonFor(inRect))
                Close();

            Listing_Standard listingStandard = new Listing_Standard();

            Rect listingRect = new Rect(inRect);
            listingRect.height -= 40;
            listingRect.y += 40;

            listingStandard.Begin(listingRect);

            foreach (var map in _maps)
            {
                string name = null;
                if (map?.Parent is Settlement settlement)
                    name = settlement.Name;

                if (name != null || map == null)
                {
                    string displayText = "AWA.NoParent".Translate();
                    if (name != null)
                        displayText = name;

                    if (listingStandard.ButtonText(displayText))
                        DoFileInteraction(map);
                }
            }

            listingStandard.End();
        }

        protected void DoFileInteraction(Map map)
        {
            _action(map);
            Close();
        }

        public override void PostClose()
        {
        }
    }
}
