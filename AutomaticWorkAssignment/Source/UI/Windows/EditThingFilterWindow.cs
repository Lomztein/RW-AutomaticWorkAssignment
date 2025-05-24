using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Windows
{
    public class EditThingFilterWindow : Window
    {
        public ThingFilter ThingFilter;
        public ThingFilter ParentThingFilter;

        private ThingFilterUI.UIState _uiState;

        public override Vector2 InitialSize => new Vector2(300, 450);

        public EditThingFilterWindow(ThingFilter thingFilter, ThingFilter parentThingFilter = null)
        {
            ThingFilter = thingFilter;
            ParentThingFilter = parentThingFilter;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            _uiState = new ThingFilterUI.UIState();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Widgets.CloseButtonFor(inRect))
            {
                Close();
            }

            Rect editorRect = new Rect(inRect.x, inRect.y + 32, inRect.width, inRect.height - 32);
            ThingFilterUI.DoThingFilterConfigWindow(editorRect, _uiState, ThingFilter, ParentThingFilter);
        }
    }
}
